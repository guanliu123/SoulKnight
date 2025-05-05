using MiddleScene;
using SoulKnightProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace OnlineStartScene
{
    public class PanelOnlineStart : IPanel
    {
        private Button ButtonStart;
        private int playerNum;//当前房间玩家数量
        private MemoryModel model;
        
        public PanelOnlineStart(IPanel parent) : base(parent)
        {
            m_GameObject = UnityTool.Instance.GetGameObjectFromCanvas(GetType().Name);
            model= ModelContainer.Instance.GetModel<MemoryModel>();
            m_GameObject.gameObject.SetActive(true);
            playerNum = 0;
            Init();
        }
        protected override void OnInit()
        {
            base.OnInit();
            /*ButtonStart = UnityTool.Instance.GetComponentFromChild<Button>(m_GameObject, "ButtonStart");
            ButtonStart.gameObject.SetActive(false);
            EventCenter.Instance.RegisterObserver(EventType.OnPlayerEnterOnlineRoom, () =>
            {
                playerNum++;
                var m = ModelContainer.Instance.GetModel<MemoryModel>();
                Debug.Log("当前房间玩家数量："+playerNum+",全局玩家数量:"+m.PlayerNum);
                Debug.Log("是否是房主:"+m.isHomeOwner);
                if (playerNum == m.PlayerNum&&m.isHomeOwner)
                {
                    ButtonStart.gameObject.SetActive(true);
                }
            });
            EventCenter.Instance.RegisterObserver(EventType.PlayerExitOnline, () =>
            {
                var m = ModelContainer.Instance.GetModel<MemoryModel>();
                if (playerNum != m.PlayerNum&&m.isHomeOwner)
                {
                    ButtonStart.gameObject.SetActive(false);
                }
            });
            ButtonStart.onClick.AddListener(() =>
            {
                //todo 房主点击开始游戏
                Debug.Log(ModelContainer.Instance.GetModel<MemoryModel>().RoomName);
                //(ClientFacade.Instance.GetRequest(ActionCode.StartEnterBattle) as RequestStartEnterBattle).SendRequest(ModelContainer.Instance.GetModel<MemoryModel>().RoomName);
            });*/
            Init();
        }

        private void Init()
        {
            ButtonStart = UnityTool.Instance.GetComponentFromChild<Button>(m_GameObject, "ButtonStart");
            ButtonStart.gameObject.SetActive(model.isHomeOwner);
            EventCenter.Instance.RegisterObserver(EventType.OnPlayerEnterOnlineRoom, () =>
            {
                playerNum++;
                Debug.Log("当前房间玩家数量："+playerNum+",全局玩家数量:"+model.PlayerNum);
                Debug.Log("是否是房主:"+model.isHomeOwner);
            });
            EventCenter.Instance.RegisterObserver(EventType.PlayerExitOnline, () =>
            {
                if (playerNum != model.PlayerNum&&model.isHomeOwner)
                {
                    ButtonStart.gameObject.SetActive(false);
                }
            });
            ButtonStart.onClick.AddListener(() =>
            {
                //todo 房主点击开始游戏
                if (playerNum != model.PlayerNum || !model.isHomeOwner)
                {
                    Debug.Log("不可以开始游戏！");
                    return;
                }
                Debug.Log(model.RoomName);
                //(ClientFacade.Instance.GetRequest(ActionCode.StartEnterBattle) as RequestStartEnterBattle).SendRequest(ModelContainer.Instance.GetModel<MemoryModel>().RoomName);
            });
        }
    }
}
