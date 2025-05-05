using SoulKnightProtocol;
using UnityEngine;

public class ToBattleRoom : MonoBehaviour
{
    private MemoryModel m_MemoryModel;
    private void Start()
    {
        m_MemoryModel = ModelContainer.Instance.GetModel<MemoryModel>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (m_MemoryModel.isOnlineMode)
            {
                //m_MemoryModel.PlayerAttr = collision.transform.parent.GetComponent<Symbol>().GetCharacter().m_Attr as PlayerAttribute;
                m_MemoryModel.PlayerAttr = collision.GetComponent<Symbol>().GetCharacter().m_Attr as PlayerAttribute;

                SceneModelCommand.Instance.LoadScene(SceneName.OnlineStartScene).completed += (op) =>
                {
                    (ClientFacade.Instance.GetRequest(ActionCode.EnterOnlineStartRoom) as RequestEnterOnlineStartRoom).SendRequest(m_MemoryModel.PlayerAttr);
                };
            }
            else
            {
                m_MemoryModel.PlayerAttr = collision.GetComponent<Symbol>().GetCharacter().m_Attr as PlayerAttribute;
                SceneModelCommand.Instance.LoadScene(SceneName.BattleScene);
            }
        }
    }
}
