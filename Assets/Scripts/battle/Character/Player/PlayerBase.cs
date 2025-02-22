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
    public PlayerWeaponBase NowPlayerWeapon { get; protected set; }
    
    protected PlayerStateMachine stateMachine;
    protected List<PlayerWeaponBase> playerWeapons;
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
        if (NowPlayerWeapon != null)
        {
            var weapon = playerWeapons[nowWeaponIdx];
            weapon.ControlWeapon(input.isAttack);
            weapon.RotateWeapon(input.WeaponAnimPos);
            if (input.isSwitchWeapon)
            {
                SwitchWeapon();
            }
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
        if(NowPlayerWeapon!=null) NowPlayerWeapon.OnReplace();
        playerWeapons[nowWeaponIdx] = newWeapon;
        NowPlayerWeapon = newWeapon;
        NowPlayerWeapon.OnEnter();
    }

    protected void SwitchWeapon()
    {
        if(NowPlayerWeapon!=null) NowPlayerWeapon.OnExit();
        nowWeaponIdx = (nowWeaponIdx + 1) % playerWeapons.Count;
        NowPlayerWeapon = playerWeapons[nowWeaponIdx];
        NowPlayerWeapon.OnEnter();

        input.isSwitchWeapon = false;
    }

    public void SetInput(PlayerControlInput _input)
    {
        input = _input;
    }
}
