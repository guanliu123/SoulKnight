using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

//面板的基类
namespace UIFrameWork
{
    public abstract class BasePanel 
    {
        public UIType UIType { get; private set; }

        protected GameObject panel;
        protected UIContainer container;
        
        public BasePanel(UIType uIType)
        {
            UIType = uIType;
        }

        private void Init()
        {
            if(panel==null) panel =UIManager.Instance.GetSingleUI(UIType);
            if(container==null) container=panel.GetComponent<UIContainer>();
        }
        //进入时
        public virtual void OnEnter()
        {
            Init();
        }
        //被其他面板覆盖时
        public virtual void OnPause()
        {
            Init();

            UITool.GetOrAddComponent<CanvasGroup>(panel).blocksRaycasts = false;
            //panel.gameObject.SetActive(false);
        }
        //恢复时
        public virtual void OnResume()
        {
            Init();

            UITool.GetOrAddComponent<CanvasGroup>(panel).blocksRaycasts = true;
            UITool.RemoveComponent<CanvasGroup>(panel);
        }
        //退出时
        public virtual void OnExit()
        {
            UIManager.Instance.DestroyUI(UIType);
        }

        protected T FindComponent<T>(string name) where T : Component
        {
            //Init();
            
            return container.GetXXX(name) as T;
        }
    }
}

