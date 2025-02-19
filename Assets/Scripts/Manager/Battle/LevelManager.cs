using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    //第几大关
    public int NowLevelNumber { get; private set; }
    //关卡ID，方便读取房间
    public int NowLevelID { get; private set; }
    //第几小关
    public int NowStage { get; private set; }

    public override void Init()
    {
        base.Init();
        NowLevelNumber = 1;
        
        PanelManager.Instance.OpenPanel(new LoadingPanel());
        RandomLevel();
        MapManager.Instance.Init();
    }
    
    private void RandomLevel()
    {
        var levelList = TableManager.Instance.Tables.TBLevel.DataList;
        List<int> randomIdList = new List<int>();
        foreach (var item in levelList)
        {
            if (item.Levelnumber == NowLevelNumber)
            {
                randomIdList.Add(item.Id);
            }
        }

        int index = Random.Range(0, randomIdList.Count);

        NowLevelID = randomIdList[index];
        NowStage++;
    }
    protected override void RegisterEvent()
    {
        EventManager.Instance.On(EventId.MAPMANAGER_CONFIG_UPDATE_COMPLETED,OnMapManagerInitCompleted);
        EventManager.Instance.On(EventId.MAP_GENERATION_COMPLETED,OnMapGenerateCompleted);
    }

    protected override void UnregisterEvent()
    {
        EventManager.Instance.Off(EventId.MAPMANAGER_CONFIG_UPDATE_COMPLETED,OnMapManagerInitCompleted);
        EventManager.Instance.Off(EventId.MAP_GENERATION_COMPLETED,OnMapGenerateCompleted);
    }

    #region 事件函数
    
    private void OnMapManagerInitCompleted()
    {
        LogTool.Log("开始生成地图");
        MapManager.Instance.GenerateMap();
    }
    private void OnMapGenerateCompleted()
    {
        //todo:生成第一波敌人

        //经过2s再关闭加载界面
        TimerManager.Register(2f, () =>
        {
            PanelManager.Instance.ClosePanel(UIInfo.LoadingPanel);
        });
    }

    #endregion
    
}
