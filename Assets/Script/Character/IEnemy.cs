using UnityEngine;

public class IEnemy : ICharacter
{
    public IEnemyWeapon m_Weapon { get; protected set; }
    public Room m_Room;
    protected GameObject DecHpPoint;
    protected GameObject Exclamation;
    private GameObject FootCircle;
    protected MaterialPropertyBlock block;
    private Vector2 weaponDir;
    public FixVector2 Velocity;
    protected IPlayer targetPlayer;
    public RectCollider rectCollider;
    private float changeTargetTimer;
    private float changeTargetCD;

    public IEnemy(GameObject obj) : base(obj)
    {
        block = new MaterialPropertyBlock();
        m_Animator = gameObject?.GetComponent<Animator>();
        m_rb = gameObject.GetComponent<Rigidbody2D>();
        DecHpPoint = gameObject.transform.Find("DecHpPoint")?.gameObject;
        Exclamation = gameObject.transform.Find("Exclamation")?.gameObject;
        FootCircle = gameObject.transform.Find("FootCircle")?.gameObject;
        rectCollider = gameObject.GetComponent<RectCollider>();
        EventCenter.Instance.RegisterObserver<Room>(EventType.OnPlayerEnterBattleRoom, (room) =>
        {
            if (m_Room == room)
            {
                m_Attr.isRun = true;
            }
        });
        
    }
    protected override void OnCharacterStart()
    {
        base.OnCharacterStart();
        m_Attr.CurrentHp = m_Attr.m_ShareAttr.MaxHp;
    }
    protected override void AlwaysUpdate()
    {
        base.AlwaysUpdate();
    }
    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        var pc = GameMediator.Instance.GetController<PlayerController>();
        if (!ModelContainer.Instance.GetModel<MemoryModel>().isOnlineMode)
        {
            if(targetPlayer==null) targetPlayer =pc.Player;
        }
        else
        {
            if (targetPlayer == null) targetPlayer = pc.allPlayers[0];
            changeTargetTimer += Time.deltaTime;
            if (changeTargetTimer > changeTargetCD)
            {
                changeTargetTimer = 0;
                changeTargetCD = Random.Range(2, 5);
                var targetIdx = Random.Range(0, pc.allPlayers.Count);
                targetPlayer = pc.allPlayers[targetIdx];
            }
        }
        if (m_Weapon != null)//������ʱ�������
        {
            weaponDir = (Vector2)targetPlayer.gameObject.transform.position - m_Weapon.GetRotOriginPos();
            if (weaponDir.normalized.x > 0.02f)
            {
                isLeft = false;
            }
            else if (weaponDir.normalized.x < -0.02f)
            {
                isLeft = true;
            }
            m_Weapon.RotateWeapon(weaponDir);
            m_Weapon.OnUpdate();
        }
        
        if (targetPlayer != null)
        {
            Velocity=new FixVector2(targetPlayer.transform.position - this.transform.position).GetNormalized();
        }
        else
        {
            Velocity=FixVector2.Zero;
        }
    }
    protected override void OnCharacterDieStart()
    {
        base.OnCharacterDieStart();
        if (m_Room != null)
        {
            m_Room.CurrentEnemyNum -= 1;
        }
        Exclamation?.SetActive(false);
        m_Weapon?.OnExit();
        Object.Destroy(m_Weapon?.gameObject);
        m_Animator.SetBool("isDie", true);
        gameObject.tag = "Untagged";

        if (m_rb != null)
        {
            m_rb.velocity = Vector2.zero;
        }
        Remove();
    }
    public override void UnderAttack(int damage)
    {
        if (m_Attr.isRun)
        {
            base.UnderAttack(damage);
        }
        DecHp effect = ItemPool.Instance.GetItem(EffectType.DecHp, DecHpPoint.transform.position) as DecHp;
        effect.SetTextValue(damage);
        effect.AddToController();
    }
    public void AddWeapon(IEnemyWeapon weapon)
    {
        m_Weapon = weapon;
    }
    public void SetFootCircleActive(bool isopen)
    {
        FootCircle?.SetActive(isopen);
    }

}
