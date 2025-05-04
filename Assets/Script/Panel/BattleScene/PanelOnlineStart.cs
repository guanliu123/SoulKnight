using MiddleScene;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace OnlineStartScene
{
    public class PanelOnlineStart : IPanel
    {
        private Button ButtonStart;

        private MemoryModel m_MemoryModel;
        public PanelOnlineStart(IPanel parent) : base(parent)
        {
            m_GameObject = UnityTool.Instance.GetGameObjectFromCanvas(GetType().Name);
            m_MemoryModel = ModelContainer.Instance.GetModel<MemoryModel>();
        }
        protected override void OnInit()
        {
            base.OnInit();
            ButtonStart = UnityTool.Instance.GetComponentFromChild<Button>(m_GameObject, "ButtonStart");
            EventCenter.Instance.RegisterObserver<Room>(EventType.OnPlayerEnterBossRoom, (enterRoom) =>
            {
                EventCenter.Instance.NotisfyObserver(EventType.OnPause);
                CoroutinePool.Instance.DelayInvoke(() =>
                {
                    EnterPanel(typeof(PanelBossCutScene));
                    OnResume();
                }, 0.5f);
            });
            ButtonStart.onClick.AddListener(() =>
            {
                //todo 房主点击开始游戏
            });
        }
        protected override void OnEnter()
        {
            base.OnEnter();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
