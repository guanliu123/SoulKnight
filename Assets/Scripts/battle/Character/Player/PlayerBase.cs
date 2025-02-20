using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBase : CharacterBase
{    
    private FixVector2 moveDir;
    public Animator animator { get; protected set; }
    
    public Rigidbody2D rigidBody{ get; protected set; }
    public PlayerControlInput input { get; protected set; }
    
    protected PlayerStateMachine stateMachine;
    
    protected List<PlayerWeaponBase> playerWeapons;
    protected int nowWeaponIdx;

    public PlayerBase(GameObject obj) : base(obj)
    {
        playerWeapons = new List<PlayerWeaponBase>();
        
    }

    protected override void OnInit()
    {
        base.OnInit();
        nowWeaponIdx = 0;
        stateMachine = new PlayerStateMachine(this);
        animator = characterRoot.GetAnimator();
        rigidBody = characterRoot.GetRigidBody();
    }

    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        stateMachine.OnUpdate();
        if (nowWeaponIdx<playerWeapons.Count&& playerWeapons[nowWeaponIdx] != null)
        {
            var weapon = playerWeapons[nowWeaponIdx];
            weapon.ControlWeapon(input.isAttack);
            UpdateDir();
            weapon.RotateWeapon(input.WeaponAnimPos);
        }
    }

    protected void UpdateDir()
    {
        Fix64 hor =(Fix64)ETCInput.GetAxis("Horizontal");
        Fix64 ver =(Fix64)ETCInput.GetAxis("Vertical");
        moveDir = new FixVector2(hor, ver);
    }

    public void SetInput(PlayerControlInput _input)
    {
        input = _input;
    }
}
