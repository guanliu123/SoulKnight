using System;
using System.Collections.Generic;
using System.Linq;
using OPH.Extend;
using OPH.Utilities.Pool;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace OPH.Collision.QuadTree {
    /// <summary>
    /// 创建人：一个人心
    /// 功能说明：四叉树节点
    /// </summary>
    public class QTree<T> : IRect where T : IRect
    {
        protected static ObjPool<QTree<T>> qtPool; // qt池
        protected static ObjPool<List<T>> listPool; // 列表池

        /// <summary>
        /// 配置是否初始化
        /// </summary>
        private static bool InitConfig = false;

        /// <summary>
        /// 最大深度
        /// </summary>
        public static int MAX_DEPTH { get; private set; } = 5;

        /// <summary>
        /// 节点孩子数量阈值
        /// </summary>
        private static int MAX_Threshold = 4;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        /// <summary>
        /// 节点的父亲
        /// </summary>
        public QTree<T> Parent { get; protected set; }

        /// <summary>
        /// 树深度
        /// </summary>
        public int Depth { get; protected set; }

        /// <summary>
        /// 当前区域内孩子数量
        /// </summary>
        public int ChildCount { get; protected set; }

        /// <summary>
        /// 是否是叶子节点
        /// </summary>
        public bool IsLeaf { get; protected set; }

        /// <summary>
        /// 对象引用
        /// </summary>
        public List<T> childList { get; protected set; }

        /// <summary>
        /// 子节点（4）
        /// </summary>
        public QTree<T>[] childNodes { get; private set; }

        #region Pool

        /// <summary>
        /// qt池创建
        /// </summary>
        /// <returns></returns>
        protected static QTree<T> qtCreate()
        {
            var qt = new QTree<T>();
            qt.childNodes = new QTree<T>[4];
            return qt;
        }

        /// <summary>
        /// qt池获取
        /// </summary>
        /// <param name="qt"></param>
        protected static void qtGet(QTree<T> qt)
        {
            qt.Init(); // qt初始化
        }

        /// <summary>
        /// 列表池创建
        /// </summary>
        /// <returns></returns>
        protected static List<T> listCreate()
        {
            return new List<T>(100);
        }

        #endregion

        #region 构造

        protected QTree()
        {
        }

        public static QTree<T> CreateRoot(int maxThreshold = 4, int maxDepth = 5)
        {
            if (!InitConfig)
            {
                InitConfig = true;
                MAX_DEPTH = maxDepth;
                MAX_Threshold = maxThreshold;
                int temp = (1 - (int)Math.Pow(4, maxDepth)) / (-3); // 极限QT池数
                qtPool = new ObjPool<QTree<T>>(qtCreate, qtGet, null, null, 1, temp);
                temp = (int)Math.Pow(4, maxDepth - 1) + 1; // 极限列表池数
                listPool = new ObjPool<List<T>>(listCreate, 1, temp);
            }

            return qtPool.Get().SetDepth(1).SetParent(null);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化构造
        /// </summary>
        private void Init()
        {
            ChildCount = 0; // 重置孩子数量
            IsLeaf = true; // 设置为叶子
            childList = listPool.Get(); // 设置列表
        }

        /// <summary>
        /// 初始化矩形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public QTree<T> InitRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            return this;
        }

        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected QTree<T> SetParent(QTree<T> parent)
        {
            Parent = parent;
            return this;
        }

        /// <summary>
        /// 设置深度
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        protected QTree<T> SetDepth(int depth)
        {
            Depth = depth;
            return this;
        }

        #endregion

        /// <summary>
        /// 分割空间
        /// </summary>
        private void Split()
        {
            IsLeaf = false; // 变为非子叶
            float hWidth = Width / 2;
            float hHeight = Height / 2;
            float hhWidth = hWidth / 2;
            float hhHeight = hHeight / 2;

            int newDepth = Depth + 1;
            float xMin = X - hhWidth;
            float xMax = X + hhWidth;
            float yMin = Y - hhHeight;
            float yMax = Y + hhHeight;
            childNodes[0] = qtPool.Get()
                .SetDepth(newDepth)
                .InitRect(xMin, yMax, hWidth, hHeight)
                .SetParent(this);
            childNodes[1] = qtPool.Get()
                .SetDepth(newDepth)
                .InitRect(xMax, yMax, hWidth, hHeight)
                .SetParent(this);
            childNodes[2] = qtPool.Get()
                .SetDepth(newDepth)
                .InitRect(xMin, yMin, hWidth, hHeight)
                .SetParent(this);
            childNodes[3] = qtPool.Get()
                .SetDepth(newDepth)
                .InitRect(xMax, yMin, hWidth, hHeight)
                .SetParent(this);

            // 将孩子放入子节点
            for (int i = childList.Count - 1; i >= 0; --i)
            {
                --ChildCount;
                Insert(childList[i]);
                childList.RemoveAt(i);
            }

            listPool.Release(childList); // 回收列表
            childList = null;
        }

        /// <summary>
        /// 合并空间(自身节点非子叶，孩子都是子叶)
        /// </summary>
        private void Combine()
        {
            IsLeaf = true;
            // 遍历所有区域
            for (int i = 0; i < childNodes.Length; ++i)
            {
                QTree<T> qt = childNodes[i];
                // 遍历区域中的所有孩子
                for (int j = qt.childList.Count - 1; j >= 0; --j)
                {
                    childList.Add(qt.childList[j]); // 将子孩子加到自身
                    // 在其他区域中移除该孩子
                    for (int z = i; z < childNodes.Length; ++z)
                    {
                        childNodes[z].Remove(qt.childList[j]);
                    }
                }
            }

            childNodes = null;
        }

        /// <summary>
        /// 获取目标所在的象限
        /// </summary>
        /// <param name="node"></param>
        /// <returns>在范围内>0,在范围外==0</returns>
        private int GetTargetIndex(T node)
        {
            float halfWidth = Width / 2;
            float halfHeight = Height / 2;
            float min_x = node.X - node.Width / 2;
            float min_y = node.Y - node.Height / 2;
            float max_x = node.X + node.Width / 2;
            float max_y = node.Y + node.Height / 2;

            if (min_x > X + halfWidth || max_x < X - halfWidth || min_y > Y + halfHeight ||
                max_y < Y - halfHeight) return 0;

            int idx = 0;
            bool IsLeft = min_x <= X ? true : false;
            bool IsRight = max_x >= X ? true : false;
            bool IsBottom = min_y <= Y ? true : false;
            bool IsTop = max_y >= Y ? true : false;

            if (IsLeft)
            {
                if (IsTop) idx |= AreaType.LT;
                if (IsBottom) idx |= AreaType.LB;
            }

            if (IsRight)
            {
                if (IsTop) idx |= AreaType.RT;
                if (IsBottom) idx |= AreaType.RB;
            }

            return idx;
        }

        private bool IsRectInBounds(T node)
        {
            float halfWidth = Width / 2;
            float halfHeight = Height / 2;
            return node.X >= X - halfWidth && node.X <= X + halfWidth &&
                   node.Y >= Y - halfHeight && node.Y <= Y + halfHeight;
        }

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="node"></param>
        public void Insert(T node)
        {
            /*if (!IsRectInBounds(node))
            {
                LogTool.LogWarning($"Object {node} is out of quadtree bounds");
                return;
            }*/
            if (IsLeaf)
            {
                // 大于区域上限 && 当前深度未满足上限 =》 分割+重新插入
                if (ChildCount + 1 > MAX_Threshold && Depth < MAX_DEPTH)
                {
                    Split();
                    Insert(node);
                }
                else
                {
                    childList.Add(node);
                    ++ChildCount;
                }
            }
            else
            {
                // 非叶子节点则获取所属区域并插入子区域
                int idx = GetTargetIndex(node);

                if (idx != 0) ++ChildCount;
                if ((idx & AreaType.LT) != 0) childNodes[0].Insert(node);
                if ((idx & AreaType.RT) != 0) childNodes[1].Insert(node);
                if ((idx & AreaType.LB) != 0) childNodes[2].Insert(node);
                if ((idx & AreaType.RB) != 0) childNodes[3].Insert(node);
            }
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="node">移除的对象</param>
        /// <returns>True该区域有移除</returns>
        public bool Remove(T node)
        {
            bool off = false;
            if (IsLeaf)
            {
                int idx = childList.IndexOf(node);
                if (idx >= 0)
                {
                    childList.Swap(idx, childList.Count - 1);
                    childList.RemoveAt(childList.Count - 1);
                    --ChildCount; // 区域内孩子数量-1
                    off = true;
                }
                else
                {
                    off = false;
                }
            }
            else
            {
                // 非叶子节点则获取所属区域并插入子区域
                int idx = GetTargetIndex(node);
                if ((idx & AreaType.LT) != 0)
                {
                    off |= childNodes[0].Remove(node);
                }

                if ((idx & AreaType.RT) != 0)
                {
                    off |= childNodes[1].Remove(node);
                }

                if ((idx & AreaType.LB) != 0)
                {
                    off |= childNodes[2].Remove(node);
                }

                if ((idx & AreaType.RB) != 0)
                {
                    off |= childNodes[3].Remove(node);
                }

                if (off)
                {
                    --ChildCount; // 区域中有移除则自身区域孩子-1
                }
            }

            return off;
        }

        /// <summary>
        /// 获取该节点下的所有孩子（包括自己）
        /// </summary>
        /// <param name="qtList"></param>
        public void GetAllChildNodes(ref List<QTree<T>> qtList)
        {
            qtList.Add(this);
            if (!IsLeaf)
            {
                for (int i = 0; i < childNodes.Length; ++i)
                {
                    childNodes[i].GetAllChildNodes(ref qtList);
                }
            }
        }

        /// <summary>
        /// 获取该节点下的所有孩子（包括自己）深度排序
        /// </summary>
        /// <param name="qtList"></param>
        public void GetAllChildNodesByDepth(ref List<QTree<T>> qtList)
        {
            // 比较方法
            int cmp(QTree<T> x, QTree<T> y)
            {
                if (x.Depth > y.Depth) return 1;
                else if (x.Depth == y.Depth) return 0;
                return -1;
            }

            GetAllChildNodes(ref qtList);
            qtList.Sort(cmp); // 深度排序
        }

        /// <summary>
        /// 获取目标周围对象
        /// </summary>
        /// <param name="target">目标</param>
        /// <param name="qtList">所有对象</param>
        public void GetAroundObj(T target, List<T> qtList)
        {
            if (IsLeaf)
            {
                for (int i = 0; i < childList.Count; ++i)
                {
                    if (!target.Equals(childList[i])) qtList.Add(childList[i]);
                }
            }
            else
            {
                // 非叶子节点则获取所属区域并插入子区域
                int idx = GetTargetIndex(target);
                if ((idx & AreaType.LT) != 0) childNodes[0].GetAroundObj(target, qtList);
                if ((idx & AreaType.RT) != 0) childNodes[1].GetAroundObj(target, qtList);
                if ((idx & AreaType.LB) != 0) childNodes[2].GetAroundObj(target, qtList);
                if ((idx & AreaType.RB) != 0) childNodes[3].GetAroundObj(target, qtList);
            }
        }

        /// <summary>
        /// 清除节点（顺带子节点）
        /// </summary>
        public void Clear()
        {
            // 叶子
            if (IsLeaf)
            {
                ChildCount = 0;
                childList.Clear();
                // 非根节点回收列表
                if (Parent != null)
                {
                    listPool.Release(childList);
                    childList = null;
                }
            }
            // 非叶子
            else
            {
                for (int i = 0; i < childNodes.Length; ++i)
                {
                    childNodes[i].Clear(); // 子节点清除
                    qtPool.Release(childNodes[i]); // 回收子节点
                    childNodes[i] = null;
                }

                // 根节点重置列表
                if (Parent == null)
                {
                    Init();
                }
            }
        }

        public void UpdateNode(T node)
        {
            Remove(node);
            Insert(node);
        }

        public void DrawGizmos()
        {
#if UNITY_EDITOR
            // 根据深度设置颜色（HSV色轮算法）
            float hue = (float)Depth / QTree<T>.MAX_DEPTH;
            Gizmos.color = Color.HSVToRGB(hue, 0.8f, 1f);

            // 绘制当前节点边界
            Vector3 center = new Vector3(X, Y, 0);
            Vector3 size = new Vector3(Width, Height, 0.1f);
            Gizmos.DrawWireCube(center, size);

            // 递归绘制子节点
            if (!IsLeaf)
            {
                foreach (var child in childNodes)
                {
                    child.DrawGizmos();
                }
            }
#endif
        }
    }
}
