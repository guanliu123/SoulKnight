using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
public class InitScene : SceneBase
{
    private static readonly string sceneName = SceneInfo.InitScene;

    public override void OnEnter()
    {
        if(SceneManager.GetActiveScene().name!="InitScene")
        {
            GameRoot.Instance.SwitchScene(sceneName);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        else
        {
            PanelManager.Instance.Push(new InitialPanel());
        }
    }

    public override void OnExit()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        PanelManager.Instance.Clear();
    }

    private void SceneLoaded(Scene scene,LoadSceneMode mode)
    {
        PanelManager.Instance.Push(new InitialPanel());
    }
}
