using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UIFrameWork
{
    //管理所有同时出现的面板对象，采用堆栈方式
    public class PanelManager : SingletonBase<PanelManager>
    {
        //private Stack<BasePanel> stackPanel = new Stack<BasePanel>();
        //private BasePanel topPanel;
        //private BasePanel lastPanel;
        private Dictionary<BasePanel, UIType> panelDic = new Dictionary<BasePanel, UIType>();
        
        //添加一个面板到顶部
        // public void Push(BasePanel nextPanel)
        // {
        //     if (nextPanel == null)
        //         return;
        //     if (stackPanel.Count > 0)
        //     {
        //         //添加新面板时原顶部面板要停止
        //         BasePanel topPanel = stackPanel.Peek();
        //         topPanel.OnPause();
        //     }
        //     stackPanel.Push(nextPanel);
        //     panels.TryAdd(nextPanel, 1);
        //     GameObject panel = UIManager.Instance.GetSingleUI(nextPanel.UIType);
        //     nextPanel.OnEnter();//新面板要调用进入方法
        // }
        
        public void OpenUI(BasePanel nextPanel)
        {
            if (nextPanel == null)
                return;

            //if(topPanel!=null) topPanel.OnPause();
            panelDic.TryAdd(nextPanel,nextPanel.UIType);
            //GameObject panel = UIManager.Instance.GetSingleUI(nextPanel.UIType);
            nextPanel.OnEnter();//新面板要调用进入方法
        }

        // public void Pop()//弹出顶部面板
        // {
        //     if (stackPanel.Count > 0)
        //         stackPanel.Pop().OnExit();//弹出的面板要销毁
        //     if (stackPanel.Count > 0)
        //         stackPanel.Peek().OnResume();//新的顶部面板要恢复
        // }

        // public void Clear()//清空所有面板
        // {
        //     while (stackPanel.Count > 0)//每一个面板都要销毁
        //         stackPanel.Pop().OnExit();
        // }

        public void CloseAllUI()
        {
            foreach (var item in panelDic)
            {
                item.Key.OnExit();
            }
            
            panelDic.Clear();
        }

        public void Close(BasePanel panel)
        {
            if (panelDic.ContainsKey(panel))
            {
                panel.OnExit();
                panelDic.Remove(panel);
            }
        }
    }
}

