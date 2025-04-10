using System;
using System.Collections.Generic;
using UnityEngine;

namespace OPH.Utilities.Pool {
    /// <summary>
    /// 创建人：一个人心
    /// 功能说明：对象池
    /// </summary>
    public class ObjPool<T> : IDisposable where T : class {
        protected readonly List<T> m_pool;

        protected Func<T> createFunc;         // 创建函数
        protected Action<T> getFunc;          // 获取时调用的函数
        protected Action<T> releaseFunc;      // 释放时调用的函数
        protected Action<T> destroyFunc;      // 删除的时候调用的函数

        /// <summary>
        /// 最大数量
        /// </summary>
        protected int MaxSize;
        /// <summary>
        /// 当前池中总数量
        /// </summary>
        public int CountAll { get; private set; }
        /// <summary>
        /// 当前激活的对象数量
        /// </summary>
        public int CountActive => CountAll - CountInactive;
        /// <summary>
        /// 当前未激活的对象数量
        /// </summary>
        public int CountInactive => m_pool.Count;
        /// <summary>
        /// 从上次重置大小后同时间激活数量最大峰值
        /// </summary>
        public int MaxActive { get; private set; } = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onCreate">创建方法</param>
        /// <param name="onGet">获取对象时方法</param>
        /// <param name="onRelease">对象释放时方法</param>
        /// <param name="onDestroy">对象销毁时方法</param>
        /// <param name="count">初始个数</param>
        /// <param name="maxSize">最大数量</param>
        /// <exception cref="Exception"></exception>
        public ObjPool(Func<T> onCreate, Action<T> onGet = null, Action<T> onRelease = null,
            Action<T> onDestroy = null, int count = 10, int maxSize = 1024) {
            if (onCreate == null) {
                throw new Exception("未设置创建函数");
            }
            if (maxSize <= 0) {
                throw new Exception("最大数量应该大于0");
            }
            m_pool = new List<T>(maxSize);
            createFunc = onCreate;
            getFunc = onGet;
            releaseFunc = onRelease;
            destroyFunc = onDestroy;
            MaxSize = maxSize;
            Instantiate(count);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onCreate">创建方法</param>
        /// <param name="count">初始个数</param>
        /// <param name="maxSize">最大数量</param>
        public ObjPool(Func<T> onCreate, int count, int maxSize = 1024):this(onCreate,null,null,
            null,count,maxSize)  { }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns></returns>
        public T Get() {
            T val;
            if (m_pool.Count == 0) {
                Instantiate(1);
            }

            val = m_pool[m_pool.Count - 1];
            m_pool.RemoveAt(m_pool.Count - 1);
            getFunc?.Invoke(val);
            MaxActive = CountActive > MaxActive ? CountActive : MaxActive;
            return val;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="val"></param>
        public void Release(T val) {
            releaseFunc?.Invoke(val);     // 调用释放
            if (CountInactive < MaxSize) {
                m_pool.Add(val);
            }
            else {
                destroyFunc?.Invoke(val);
                --CountAll;
            }
        }

        /// <summary>
        /// 删除容器中所有对象
        /// </summary>
        public void Clear() {
            if (m_pool != null) {
                for (int i = m_pool.Count -1;i >= 0;--i) {
                    destroyFunc?.Invoke(m_pool[i]);
                    m_pool.RemoveAt(i);
                }
            }
            m_pool.Clear();
            CountAll = 0;
            MaxSize = 0;
        }

        /// <summary>
        /// 设置最大数量
        /// </summary>
        /// <param name="size"></param>
        public void SetMaxSize(int size) {
            MaxSize = size;
            MaxActive = 0;
        }

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <param name="count">生成个数</param>
        protected void Instantiate(int count) {
            for (int i = 0; i < count; ++i) {
                T obj = createFunc();
                m_pool.Add(obj);
                ++CountAll;
            }
        }

        /// <summary>
        /// 适应大小
        /// </summary>
        public void AdaptSize() {
            MaxSize = MaxActive;
            MaxActive = 0;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            Clear();
        }
    }
}
