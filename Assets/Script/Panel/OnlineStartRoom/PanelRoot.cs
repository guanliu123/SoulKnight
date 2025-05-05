using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace OnlineStartScene
{
    public class PanelRoot : IPanel
    {
        public PanelRoot() : base(null)
        {
            m_GameObject = UnityTool.Instance.GetGameObjectFromCanvas(GetType().Name);
            children.Add(new PanelOnlineStart(this));
        }
        protected override void OnInit()
        {
            base.OnInit();
            OnResume();
            EventCenter.Instance.RegisterObserver(EventType.OnCameraArriveAtPlayer, () =>
            {
                EnterPanel(typeof(PanelOnlineStart));
                m_GameObject.SetActive(false);
            });
        }
        protected override void OnEnter()
        {
            base.OnEnter();
        }
    }
}