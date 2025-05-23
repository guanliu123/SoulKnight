﻿using Google.Protobuf;
using SoulKnightProtocol;

public class Message
{
    private byte[] buffer = new byte[1024];
    public byte[] Buffer => buffer;
    private int startIndex;
    public int StartIndex => startIndex;
    public int RemainSize => buffer.Length - startIndex;
    public void ReadBuffer(int len, Action<MainPack> Callback)
    {
        startIndex += len;
        if (startIndex <= 4)
        {
            return;
        }
        int Count = BitConverter.ToInt32(buffer, 0);
        while (true)
        {
            if (startIndex >= Count + 4)
            {
                MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, Count);
                Callback(pack);
                Array.Copy(buffer, Count + 4, buffer, 0, startIndex - Count - 4);
                startIndex -= Count + 4;
            }
            else
            {
                break;
            }
        }
    }
    public static byte[] ConvertToByteArray(MainPack pack)
    {
        byte[] data = pack.ToByteArray();
        byte[] head = BitConverter.GetBytes(data.Length);
        if (data.Length > 1024){
            Console.WriteLine($"data too long {pack}");
        }
        return head.Concat(data).ToArray();
    }
    public static byte[] PackDataUDP(MainPack pack)
    {
        return pack.ToByteArray();
    }
}
