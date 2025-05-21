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

            int seed =  ModelContainer.Instance.GetModel<MemoryModel>().RandomSeed;
            RandomTool.InitEnemyRandom(seed);
            RandomTool.InitBulletRandom(seed);

            Random.InitState(seed);
            SceneModelCommand.Instance.LoadScene(SceneName.BattleScene).completed+= (op) =>
            {
            };
        }
    }
}
