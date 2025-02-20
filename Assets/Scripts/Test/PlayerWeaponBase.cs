using cfg;
using UnityEngine;

public class PlayerWeaponBase:WeaponBase
{
    public new PlayerBase player{get=>base.character as PlayerBase;
        set => base.character = value;
    }

    protected Transform rotOrigin;

    private bool isAttackKeyDown;
    
    public PlayerWeaponBase(GameObject obj, CharacterBase character) : base(obj, character)
    {
        WeaponRoot root = gameObject.GetComponent<WeaponRoot>();
        if (!root)
        {
            LogTool.LogError("武器上未挂载WeaponRoot！");
        }

        rotOrigin = root.GetRotOrigin();
    }

    //射击按钮按下松开发射一次
    public void ControlWeapon(bool isAttack)
    {
        if (isAttackKeyDown!=isAttack)
        {
            OnFire();
            isAttackKeyDown = isAttack;
        }
    }

    public void RotateWeapon(FixVector2 dir)
    {
        if (canRotate)
        {
            float angle = 0;
            if (character.IsLeft)
            {
                angle = -Vector2.SignedAngle(Vector2.left, dir.ToVector2());

            }
            else
            {
                angle = Vector2.SignedAngle(Vector2.right, dir.ToVector2());
            }
            rotOrigin.localRotation=Quaternion.Euler(0,0,angle);
        }
    }
}