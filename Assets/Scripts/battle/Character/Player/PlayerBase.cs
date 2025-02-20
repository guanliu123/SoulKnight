using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : CharacterBase
{    
    private FixVector2 moveDir;
    public Animator animator { get; protected set; }
    
    public Rigidbody2D rigidBody{ get; protected set; }
    protected PlayerStateMachine stateMachine;
    
    protected List<PlayerWeaponBase> playerWeapons;

    public PlayerBase(GameObject obj) : base(obj)
    {
        playerWeapons = new List<PlayerWeaponBase>();
    }

    protected override void OnInit()
    {
        base.OnInit();
        stateMachine = new PlayerStateMachine(this);
        animator = characterRoot.GetAnimator();
        rigidBody = characterRoot.GetRigidBody();
    }

    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        stateMachine.OnUpdate();
    }
}
