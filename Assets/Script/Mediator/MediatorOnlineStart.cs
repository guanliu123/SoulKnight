public class MediatorOnlineStart : IGameFacade
{
    private ItemController m_ItemController;
    private PlayerController m_PlayerSystem;
    private TalentSystem m_TalentSystem;
    private UIController m_UIController;
    public PlayerController PlayerSystem => m_PlayerSystem;
    public MediatorOnlineStart()
    {
        m_ItemController = new ItemController();
        //m_PlayerSystem = GameMediator.Instance.GetController<PlayerController>();
        m_PlayerSystem=new PlayerController();
        m_UIController=new UIController();
        m_TalentSystem = new TalentSystem();
        /*if (m_PlayerSystem == null)
        {
            m_PlayerSystem=new PlayerController();
        }*/
        GameMediator.Instance.RegisterController(m_PlayerSystem);

        GameMediator.Instance.RegisterController(m_ItemController);
        
        GameMediator.Instance.RegisterSystem(m_TalentSystem);
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

