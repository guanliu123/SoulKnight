using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlInput
{
    public Fix64 hor;
    public Fix64 ver;
    public FixVector2 WeaponAnimPos;
    public bool isAttack;
    public bool isSwitchWeapon;
}

public class InputController : AbstractController
{
    public PlayerControlInput input { get; private set; }
    private FixVector2 dir;
    protected override void Init()
    {
        base.Init();
        input = new PlayerControlInput();
    }

    protected override void AlwaysUpdate()
    {
        base.AlwaysUpdate();
        input.hor = (Fix64)ETCInput.GetAxis("Horizontal");
        input.ver = (Fix64)ETCInput.GetAxis("Vertical");
        //todo:改成虚拟按键输入
        input.isAttack = Input.GetMouseButtonDown(0);
        input.isSwitchWeapon = Input.GetKeyDown(KeyCode.R);
        
        var moveDir = new FixVector2(input.hor, input.ver);
        moveDir.Normalize();
        dir = moveDir;

        if (FixVector2.Magnitude(dir) > 0)
        {
            input.WeaponAnimPos = dir;
        }
    }
}
