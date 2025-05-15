using SoulKnightProtocol;
using UnityEngine;

public class GameLoopOnlineStart : MonoBehaviour
{
    private MediatorOnlineStart m_Mediator;
    private bool needToBattleScene = false;
    
    private void Start()
    {
        Time.timeScale = 1;
        m_Mediator = new MediatorOnlineStart();
        EventCenter.Instance.RegisterObserver(EventType.OnNeedToBattleScene,  ()=>
        {
            needToBattleScene = true;
        });
    }
    private void Update()
    {
        m_Mediator.GameUpdate();
        if (needToBattleScene)
        {
            needToBattleScene=false;
            SceneModelCommand.Instance.LoadScene(SceneName.BattleScene).completed+= (op) =>
            {
                int seed =  ModelContainer.Instance.GetModel<MemoryModel>().RandomSeed;
                Random.InitState(seed);
            };
        }
    }
}
