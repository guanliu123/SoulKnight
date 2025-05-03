
using Internal;
using SoulKnightProtocol;
namespace KnightServer
{
    public class RoomController : BaseController
    {
        public RoomController(ControllerManager manager) : base(manager)
        {
            requestCode = RequestCode.Room;
        }
        public MainPack CreateRoom(Client client, MainPack pack)
        {
            try
            {
                // 检查RoomPacks是否为空
                if (pack.RoomPacks == null || pack.RoomPacks.Count == 0)
                {
                    // 如果为空，创建一个默认的房间
                    RoomPack roomPack = new RoomPack();
                    roomPack.RoomName = $"{client.userName}的房间";
                    roomPack.MaxNum = 4; // 默认最大人数
                    pack.RoomPacks.Add(roomPack);
                }

                // 检查数据库中是否有相同名字的房间
                if (RoomManager.Instance.RoomExists(pack.RoomPacks[0].RoomName)){
                    pack.ReturnCode = ReturnCode.Fail;
                    return pack;
                }
                
                // 先检查是否在房间，不在就创建，否则返回失败
                if (client.Room != null)
                {
                    pack.ReturnCode = ReturnCode.Fail;
                    return pack;
                }
                
                Room room = RoomManager.Instance.CreateRoom(pack.RoomPacks[0].RoomName, pack.RoomPacks[0].MaxNum);
                room.AddPlayer(client);
                Console.WriteLine("创建房间成功" + pack.RoomPacks[0].RoomName);
                pack.ReturnCode = ReturnCode.Success;
                pack.ActionCode = ActionCode.CreateRoom;
            }
            catch (Exception e)
            {
                Console.WriteLine("创建房间失败" + e.Message);
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
        public MainPack FindRoom(Client client, MainPack pack)
        {
            pack.ActionCode = ActionCode.FindRoom;
            try
            {
                if (RoomManager.Instance.GetRoomCount() == 0)
                {
                    pack.ReturnCode = ReturnCode.NoRoom;
                }
                else
                {
                    foreach (Room room in RoomManager.Instance.GetAllRooms())
                    {
                         pack.RoomPacks.Add(room.GetRoomPack());
                    }
                    Console.WriteLine("查询房间成功" + pack.RoomPacks.Count);
                    pack.ReturnCode = ReturnCode.Success;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("查询房间失败" + e.Message);
                Console.WriteLine(e.StackTrace);
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
        public MainPack JoinRoom(Client client, MainPack pack)
        {
            bool isFind = false;
            Room findRoom = null;
            Console.WriteLine("加入房间" + pack.RoomPacks[0].RoomName);
            foreach (Room room in RoomManager.Instance.GetAllRooms())
            {
                if (room.m_RoomName == pack.RoomPacks[0].RoomName)
                {
                    isFind = true;
                    findRoom = room;
                    break;
                }
            }
            if (isFind)
            {
                pack.ReturnCode = findRoom.AddPlayer(client);
                if (pack.ReturnCode == ReturnCode.Success)
                {
                    pack.RoomPacks.Clear();
                    pack.RoomPacks.Add(findRoom.GetRoomPack());
                    pack.ActionCode = ActionCode.FindPlayer;
                    findRoom.Broadcast(pack);
                    pack.ActionCode = ActionCode.JoinRoom;
                }else{
                    Console.WriteLine("加入房间失败" + pack.ReturnCode);
                }
            }
            else
            {
                Console.WriteLine("房间不存在");
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
        public MainPack ExitRoom(Client client, MainPack pack)
        {
            Room room = client.Room;
            if (client.Room != null)
            {
                pack.ReturnCode = client.Room.RemovePlayer(client);
                if (pack.ReturnCode == ReturnCode.Success)
                {
                    pack.RoomPacks.Clear();
                    pack.RoomPacks.Add(room.GetRoomPack());
                    pack.ActionCode = ActionCode.FindPlayer;
                    room.Broadcast(pack);
                    pack.ActionCode = ActionCode.ExitRoom;
                }
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
        public MainPack EnterOnlineStartRoom(Client client, MainPack pack)
        {
            client.PlayerType = pack.CharacterPacks[0].PlayerType;
            Room room = client.Room;
            if (room != null)
            {
                if (client.userName == room.Clients[0].userName)
                {
                    room.hostCode = HostCode.WaitForStartGame;
                }
                pack.ReturnCode = ReturnCode.Success;
                pack.IsBroadcastMessage = true;
                pack.CharacterPacks[0].CharacterName = client.userName;
                room.Broadcast(client, pack);
                pack.IsBroadcastMessage = false;
                pack.CharacterPacks.Clear();
                foreach (Client c in room.Clients)
                {
                    if (c.PlayerType != null && c.PlayerType.Length != 0)
                    {
                        CharacterPack p = new CharacterPack();
                        p.CharacterName = c.userName;
                        p.PlayerType = c.PlayerType;
                        pack.CharacterPacks.Add(p);
                    }
                }
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
    }
}

