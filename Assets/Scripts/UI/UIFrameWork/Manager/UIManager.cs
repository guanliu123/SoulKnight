using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnumCenter;
using UnityEngine;
//UI管理器
namespace UIFrameWork
{
    //是一个单例，理解为UI的内存池，存储面板信息与对象的映射关系
    public class UIManager : MonoSingletonBase<UIManager>
    {
        //为了提前注册一些常用的uipanel
        private Dictionary<UIType, GameObject> dicUI = new Dictionary<UIType, GameObject>();
        

        //获取一个面板
        public GameObject GetSingleUI(UIType uIType)
        {
            if (uIType == null)
                return null;
            
            //原查找根对象，现在改成了MonoSingletonBase，UIManager挂载的对象一般就是Canvas
            // GameObject parent = GameObject.Find("Canvas");
            // if (!parent)
            // {
            //     Debug.LogError("无Canvas对象，请查询是否存在所有UI的根");
            //     return null;
            // }
            
            //如果内存池中存在该UI面板
            if (dicUI.ContainsKey(uIType))
                return dicUI[uIType];
            //如果不存在，从预设中加载
            //GameObject uiPrefab = Resources.Load<GameObject>(uIType.Path);
            GameObject uiInstance = null;
            // if (uiPrefab != null)
            // {
            //     uiInstance = GameObject.Instantiate(uiPrefab, parent.transform);
            //     uiInstance.name = uIType.Name;
            //     dicUI.Add(uIType, uiInstance);
            // }
            // else
            //     Debug.LogError($"在路径:{uIType.Path}中没有找到名为{uIType.Name}的预设，请查询");
            var asset = LoadManager.Instance.Load<GameObject>(uIType.Path+".prefab");
            if (asset == null)
            {
                LogTool.LogError($"在路径:{uIType.Path}中没有找到名为{uIType.Name}的预设，请查询");
            }
            uiInstance = GameObject.Instantiate(asset, gameObject.transform);
            uiInstance.name = uIType.Name;
            dicUI.Add(uIType, uiInstance);
            
            LogTool.Log("打开了UI"+uIType.Name);
            // if (asset != null && asset.asset() != null)
            // {
            //     GameObject uiPrefab = asset.asset() as GameObject;
            //     uiInstance = GameObject.Instantiate(uiPrefab, parent.transform);
            //     uiInstance.name = uIType.Name;
            //     dicUI.Add(uIType, uiInstance);
            // }
            // LoadManager.Instance.Instantiate(uIType.Path, (obj) =>
            // {
            //     if (!obj == null)
            //     {
            //         uiInstance = obj as GameObject;
            //         uiInstance.transform.SetParent(parent.transform);
            //         uiInstance.name = uIType.Name;
            //         dicUI.Add(uIType, uiInstance);
            //     }
            // });
            
            return uiInstance;
        }

        //销毁一个面板
        public void DestroyUI(UIType uIType)
        {
            if (uIType == null)
                return;
            if (dicUI.ContainsKey(uIType))
            {
                GameObject.Destroy(dicUI[uIType]);
                dicUI.Remove(uIType);
            }
        }
    }
}

