public class MediatorOnlineStart : IGameFacade
{
    private ItemController m_ItemController;
    private PlayerController m_PlayerSystem;
    public PlayerController PlayerSystem => m_PlayerSystem;
    public MediatorOnlineStart()
    {
        m_ItemController = new ItemController();
        m_PlayerSystem = GameMediator.Instance.GetController<PlayerController>();
        if (m_PlayerSystem == null)
        {
            m_PlayerSystem=new PlayerController();
            GameMediator.Instance.RegisterController(m_PlayerSystem);
        }

        GameMediator.Instance.RegisterController(m_ItemController);
        
        m_ItemController.TurnOnController();
    }
    protected override void Init()
    {
        base.Init();
        m_ItemController.TurnOnController();
        m_PlayerSystem.TurnOnController();
    }
    public override void GameUpdate()
    {
        base.GameUpdate();
        m_ItemController.GameUpdate();
        m_PlayerSystem.GameUpdate();
    }
}

