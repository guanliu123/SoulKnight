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
        ModelContainer.Instance.GetModel<MemoryModel>().RandomSeed = pack.BattleInitInfo.RandSeed;
        EventCenter.Instance.NotisfyObserver(EventType.OnNeedToBattleScene,pack);
    }
    public void SendRequest(string roomName,UnityAction<MainPack> receiveAction = null)
    {
        /*MainPack pack = new MainPack();
        pack.RequestCode = requestCode;
        pack.ActionCode = actionCode;
        CharacterPack characterPack = new CharacterPack();
        characterPack.PlayerType = attr.m_ShareAttr.Type.ToString();
        pack.CharacterPacks.Add(characterPack);
        base.SendRequest(pack);*/
        MainPack mainPack = new MainPack();
        mainPack.RequestCode = RequestCode.Battle;
        mainPack.ActionCode = ActionCode.StartEnterBattle;
        RoomPack roomPack = new RoomPack();
        mainPack.RoomPacks.Add(roomPack);
        roomPack.RoomName = roomName; // 房间名称
        base.SendRequest(mainPack);
    }
}