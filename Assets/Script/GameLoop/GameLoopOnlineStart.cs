using SoulKnightProtocol;
using UnityEngine;

public class GameLoopOnlineStart : MonoBehaviour
{
    private MediatorOnlineStart m_Mediator;
    private bool needToBattleScene = false;
    private MainPack pack = null;
    
    private void Start()
    {
        Time.timeScale = 1;
        m_Mediator = new MediatorOnlineStart();
        EventCenter.Instance.RegisterObserver<MainPack>(EventType.OnNeedToBattleScene, (pack) =>
        {
            needToBattleScene = true;
            this.pack = pack;
        });
    }
    private void Update()
    {
        m_Mediator.GameUpdate();
        if (needToBattleScene)
        {
            SceneModelCommand.Instance.LoadScene(SceneName.BattleScene).completed+= (op) =>
            {
                EventCenter.Instance.NotisfyObserver(EventType.OnSwitchOnlineSceneResponse, this.pack);
            };
        }
    }
}
