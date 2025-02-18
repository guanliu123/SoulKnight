using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBase
{
    protected string sceneName;
    protected string scenePath;
    protected BasePanel basePanel;
    //初始化必要数据
    public virtual void Init()
    {
        
    }
    
    //场景进入时
    public virtual void OnEnter()
    {
        if(SceneManager.GetActiveScene().name!=sceneName)
        {
            GameRoot.Instance.ChangeScene(scenePath);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        else
        {
            if(basePanel!=null) PanelManager.Instance.OpenPanel(basePanel);
        }
    }

    //场景离开时
    public virtual void OnExit()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        PanelManager.Instance.CloseAllPanel();
    }

    protected virtual void SceneLoaded(Scene scene,LoadSceneMode mode)
    {
        if(basePanel!=null) PanelManager.Instance.OpenPanel(basePanel);
    }
}
