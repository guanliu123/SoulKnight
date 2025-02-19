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
        //键是UIInfo
        private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
        
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
        
        public void OpenPanel(BasePanel nextPanel)
        {
            if (nextPanel == null)
                return;

            //if(topPanel!=null) topPanel.OnPause();
            //加入失败说明面板已经打开了不要重复打开
            if (!panelDic.TryAdd(nextPanel.UIType.Path, nextPanel))
            {
                LogTool.Log("面板"+nextPanel.UIType.Name+"已打开，请勿重复打开！");
                return;
            }
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

        public void CloseAllPanel()
        {
            foreach (var item in panelDic)
            {
                item.Value.OnExit();
            }
            
            panelDic.Clear();
        }

        public void ClosePanel(string panelInfo)
        {
            if (panelDic.ContainsKey(panelInfo))
            {
                panelDic[panelInfo].OnExit();
                panelDic.Remove(panelInfo);
            }
        }
    }
}

