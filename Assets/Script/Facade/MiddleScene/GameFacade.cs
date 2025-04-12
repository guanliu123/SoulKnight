namespace MiddleScene
{
    public class GameFacade : IGameFacade
    {
        private PlayerController m_PlayerController;
        private UIController m_UIController;
        private ItemController m_ItemController;
        private EnemyController m_EnemyController;
        private BuffController m_BuffController;
        private QuadTreeSystem m_QuadTreeSystem;
        public GameMediator m_Mediator { get; private set; }
        public GameFacade() : base()
        {

            m_PlayerController = new PlayerController();
            m_EnemyController = new EnemyController();
            m_UIController = new UIController();
            m_ItemController = new ItemController();
            m_BuffController = new BuffController();
            m_ItemController.TurnOnController();

            GameMediator.Instance.RegisterController(m_PlayerController);
            GameMediator.Instance.RegisterController(m_EnemyController);
            GameMediator.Instance.RegisterController(m_UIController);
            GameMediator.Instance.RegisterController(m_ItemController);
            GameMediator.Instance.RegisterController(m_BuffController);

            GameMediator.Instance.RegisterSystem(new AudioSystem());
            GameMediator.Instance.RegisterSystem(new TalentSystem());
            GameMediator.Instance.RegisterSystem(new BackpackSystem());
            m_QuadTreeSystem = new QuadTreeSystem();
            GameMediator.Instance.RegisterSystem(m_QuadTreeSystem);

            EventCenter.Instance.RegisterObserver(EventType.OnFinishSelectPlayer, () =>
            {
                m_BuffController.TurnOffController();
                m_PlayerController.TurnOnController();
                m_EnemyController.TurnOnController();
            });
        }
        protected override void Init()
        {
            base.Init();
        }
        public override void GameUpdate()
        {
            base.GameUpdate();

            m_BuffController.GameUpdate();
            m_UIController.GameUpdate();
            m_ItemController.GameUpdate();
            m_PlayerController.GameUpdate();
            m_EnemyController.GameUpdate();
            
            //更新四叉树
            m_QuadTreeSystem.GameUpdate();
        }
    }
}

