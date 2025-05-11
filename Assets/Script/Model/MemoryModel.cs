using SoulKnightProtocol;
using UnityEngine;

public class MemoryModel : AbstractModel
{
    public PlayerAttribute PlayerAttr;
    public PetType PetType;
    public string UserName;
    public int Stage;
    public int Money;
    public BindableProperty<bool> isOnlineMode = new BindableProperty<bool>(false);
    public string RoomName;
    public bool isHomeOwner;//是否是房主
    public int PlayerNum;//联机玩家数量
    public int RandomSeed=-1;//这局游戏的种子
    public MainPack toBattlePack = null;
    
    protected override void OnInit()
    {
        PetType = PetType.LittleCool;
        Stage = 5;
        Money = 0;
    }
}
