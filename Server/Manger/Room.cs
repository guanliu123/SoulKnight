﻿using System;
using Google.Protobuf.Collections;
using KnightServer;
using SoulKnightProtocol;

public class Room
{
    private string RoomName;
    public string m_RoomName => RoomName;
    private int MaxNum;
    private int CurrentNum;
    private List<Client> clients = new List<Client>();
    public List<Client> Clients => clients;
    public HostCode hostCode = HostCode.SelectCharacter;
    public Room(string RoomName, int  maxNum)
    {
        this.RoomName = RoomName;
        this.MaxNum = maxNum;
        this.CurrentNum = 0;
    }
    public RoomPack GetRoomPack()
    {
        RoomPack roomPack = new RoomPack();
        roomPack.RoomName = RoomName;
        roomPack.MaxNum = MaxNum;
        roomPack.CurrentNum = clients.Count;
        foreach (PlayerPack playerPack in GetPlayerPacks())
        {
            roomPack.PlayerPacks.Add(playerPack);
        }
        return roomPack;
    }
    public RepeatedField<PlayerPack> GetPlayerPacks()
    {
        RepeatedField<PlayerPack> playerPacks = new RepeatedField<PlayerPack>();
        foreach (Client client in clients)
        {
            PlayerPack playerPack = new PlayerPack();
            if (client.userName is null)
            client.userName = "unknow";
            else
            playerPack.PlayerName = client.userName;
            playerPacks.Add(playerPack);
        }
        return playerPacks;
    }
    public ReturnCode AddPlayer(Client client)
    {
        if (this.hostCode == HostCode.GameStart){
            return ReturnCode.Fail;
        }
        if (clients.Count < MaxNum)
        {
            client.SetRoom(this);
            clients.Add(client);
            CurrentNum += 1;
            return ReturnCode.Success;
        }
        else
        {
            return ReturnCode.Fail;
        }
    }
    public ReturnCode RemovePlayer(Client client)
    {
        if (client != null)
        {
            client.SetRoom(null!);
            clients.Remove(client);
            CurrentNum -= 1;
            return ReturnCode.Success;
        }
        return ReturnCode.Fail;
    }
    public void Broadcast(Client except, MainPack pack)
    {
        foreach (Client client in clients)
        {
            if (client != except)
            {
                client.Send(pack);
            }
        }
    }
    public void Broadcast(MainPack pack)
    {
        foreach (Client client in clients)
        {
            client.Send(pack);
        }
    }
    public void EnterGameStart()
    {
        this.hostCode = HostCode.GameStart;
    }

    public void BroadcastTo(Client except, MainPack pack)
    {
        foreach (Client client in clients)
        {
            if (client != except)
            {
                client.SendTo(pack);
            }
        }
    }

}
