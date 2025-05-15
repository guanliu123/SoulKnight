using System.Collections.Generic;
using SoulKnightProtocol;
using UnityEngine;
using UnityEngine.Events;

public class RequestStartEnterBattle : BaseRequest
{
    public RequestStartEnterBattle(RequestManager manager) : base(manager)
    {
        requestCode = RequestCode.Room;
        actionCode = ActionCode.StartEnterBattle;
    }
    public override void OnResponse(MainPack pack)
    {
        if (pack.ReturnCode == ReturnCode.Success)
        {
            Debug.Log("房主成功发送进入战斗");
        }
        if (pack.ReturnCode == ReturnCode.Fail)
        {
            Debug.Log("房主发送进入战斗失败");
        }
        //EventCenter.Instance.NotisfyObserver(EventType.OnStartEnterBattleResponse, pack);
        var seed=pack.BattleInitInfo.RandSeed;
        ModelContainer.Instance.GetModel<MemoryModel>().RandomSeed = seed;
        // Random.InitState(seed);
        ModelContainer.Instance.GetModel<MemoryModel>().toBattlePack=pack;
        EventCenter.Instance.NotisfyObserver(EventType.OnNeedToBattleScene);
    }
    public void SendRequest(string roomName,UnityAction<MainPack> receiveAction = null)
    {
        MainPack mainPack = new MainPack();
        mainPack.RequestCode = RequestCode.Battle;
        mainPack.ActionCode = ActionCode.StartEnterBattle;
        RoomPack roomPack = new RoomPack();
        mainPack.RoomPacks.Add(roomPack);
        roomPack.RoomName = roomName; // 房间名称
        base.SendRequest(mainPack);
    }
}