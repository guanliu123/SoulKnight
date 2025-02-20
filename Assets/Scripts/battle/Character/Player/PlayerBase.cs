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
    protected PlayerWeaponBase nowPlayerWeapon;
    protected int nowWeaponIdx;
    private int maxWeaponCnt;

    public PlayerBase(GameObject obj) : base(obj)
    {
        playerWeapons = new List<PlayerWeaponBase>();
        maxWeaponCnt = 2;
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
        if (nowPlayerWeapon != null)
        {
            var weapon = playerWeapons[nowWeaponIdx];
            weapon.ControlWeapon(input.isAttack);
            weapon.RotateWeapon(input.WeaponAnimPos);
        }
    }
    
    protected virtual void PickUpWeapon(GameObject weaponObj)
    {
        
        var weapon = WeaponFactory.Instance.GetPlayerWeapon(weaponObj, this);
        if (playerWeapons.Count >= maxWeaponCnt)
        {
            ReplaceWeapon(weapon);
        }
        else
        {
            playerWeapons.Add(weapon);
            SwitchWeapon();
        }
    }

    protected void ReplaceWeapon(PlayerWeaponBase newWeapon)
    {
        if(nowPlayerWeapon!=null) nowPlayerWeapon.OnExit();
        playerWeapons[nowWeaponIdx] = newWeapon;
        nowPlayerWeapon = newWeapon;
        nowPlayerWeapon.OnEnter();
    }

    protected void SwitchWeapon()
    {
        if(nowPlayerWeapon!=null) nowPlayerWeapon.OnExit();
        nowWeaponIdx = (nowWeaponIdx + 1) % playerWeapons.Count;
        nowPlayerWeapon = playerWeapons[nowWeaponIdx];
        nowPlayerWeapon.OnEnter();
    }

    public void SetInput(PlayerControlInput _input)
    {
        input = _input;
    }
}
