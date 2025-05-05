using KnightServer;
using SoulKnightProtocol;

public class GameController : BaseController
{
    public GameController(ControllerManager manager) : base(manager)
    {
        requestCode = RequestCode.Game;
    }
    public MainPack StartGame(Client client, MainPack pack)
    {
        Room room = client.Room;
        if (room!= null)
        {
            pack.ReturnCode = ReturnCode.Success;
            room.Broadcast(pack);
        }
        else
        {
            pack.ReturnCode = ReturnCode.Fail;
        }
        return pack;
    }

    public MainPack EndGame(Client client, MainPack pack)
    {
        Room room = client.Room;
        if (room!= null)
        {
            pack.ReturnCode = ReturnCode.Success;
            room.Broadcast(pack);
        }
        else
        {
            pack.ReturnCode = ReturnCode.Fail;
        }
        return pack;
    }
    public MainPack UpdatePlayerState(Client client, MainPack pack)
    {
        if (null == client){
            Console.WriteLine("UpdatePlayerState client is null");
            return pack;
        }
        Room room = client.Room;
        if (room != null)
        {
            pack.CharacterPacks[0].CharacterName = client.userName;
            pack.IsBroadcastMessage = true;
            pack.ReturnCode = ReturnCode.Success;
            room.Broadcast(client, pack);
            pack.IsBroadcastMessage = false;
        }
        else
        {
            pack.ReturnCode = ReturnCode.Fail;
        }
        return pack;
    }
}
