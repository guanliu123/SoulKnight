// using Game;
// using StarkSDKSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//游戏的根管理器
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        //进入时是什么场景就加载对应脚本
        if (SceneManager.GetActiveScene().name == "DemoScene")
        {
            SceneSystem.Instance.SetScene(new DemoScene());
        }
    }

    public void SwitchScene(string sceneName)
    {
        //SceneManager.LoadScene(sceneName);
        StartCoroutine(Delay(sceneName));
    }

    private IEnumerator Delay(string sceneName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        while (!ao.isDone)
        {
            yield return new WaitForSeconds(3.0f);
        }

    }
}
