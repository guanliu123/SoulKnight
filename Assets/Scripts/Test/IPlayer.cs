using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPlayer : ICharacter
{    
    //private Fix64 hor, ver;
    private FixVector2 moveDir; 
    public Animator animator { get; protected set; }
    
    public Rigidbody2D rigidBody{ get; protected set; }
    protected PlayerStateMachine stateMachine;

    public IPlayer(GameObject obj) : base(obj) 
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        stateMachine = new PlayerStateMachine(this);
        animator = transform.Find("Sprite").GetComponent<Animator>();
        rigidBody = transform.GetComponent<Rigidbody2D>();
    }

    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        stateMachine.OnUpdate();
    }
}
