using Google.Protobuf;
using SoulKnightProtocol;
using System;
using System.Linq;
using System.Text; // 添加 System.Text 命名空间
using UnityEngine; // 添加 UnityEngine 命名空间

public class Message
{
    private byte[] buffer = new byte[2048];
    public byte[] Buffer => buffer;
    private int startIndex;
    public int StartIndex => startIndex;
    public int RemainSize => buffer.Length - startIndex;
    // --- 新增辅助方法 ---
    /// <summary>
    /// 将字节数组的一部分转换为十六进制字符串，便于调试查看
    /// </summary>
    private static string BytesToHexString(byte[] bytes, int offset, int count)
    {
        if (bytes == null) return "null";
        count = Math.Min(count, bytes.Length - offset);
        if (count <= 0 || offset >= bytes.Length) return "[]";
        StringBuilder hex = new StringBuilder(count * 3);
        for (int i = 0; i < count; i++)
        {
            hex.AppendFormat("{0:X2} ", bytes[offset + i]);
        }
        return hex.ToString().Trim();
    }
    // --- 辅助方法结束 ---

    public void ReadBuffer(int len, Action<MainPack> Callback)
    {
        startIndex += len;
        // 保留接收日志，了解数据流入情况

        while (true)
        {
            if (startIndex <= 4)
            {
                // 数据不够读长度头，这是正常情况，不需要频繁打印日志
                return;
            }

            int Count = BitConverter.ToInt32(buffer, 0);
            // 保留读取到的长度日志，这是关键信息

            // 健壮性检查日志只在出错时打印
            if (Count <= 0 || Count > buffer.Length - 4)
            {
                // 使用 LogError 突出显示无效长度错误
                startIndex = 0;
                return;
            }

            if (startIndex >= Count + 4)
            {
                // 准备解析前打印日志
                try
                {
                    // 解析前可以打印少量数据预览，帮助判断
                    int previewLength = Math.Min(Count, 32); // 减少预览长度

                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, Count);
                    // 解析成功后打印简短确认信息
                    Callback(pack);

                    int bytesProcessed = Count + 4;
                    int remainingBytes = startIndex - bytesProcessed;
                    Array.Copy(buffer, bytesProcessed, buffer, 0, remainingBytes);
                    startIndex = remainingBytes;
                    // 处理完一个消息后，打印缓冲区状态变化
                    // 继续循环处理下一个消息
                }
                // --- 错误日志重点区域 ---
                catch (Google.Protobuf.InvalidProtocolBufferException protoEx)
                {
                    // 使用 LogError 清晰标记 Protobuf 解析错误
                    Debug.LogError($"--- PROTOBUF PARSE ERROR ---");
                    Debug.LogError($"[ReadBuffer ERROR] InvalidProtocolBufferException while parsing message (Reported Length={Count}): {protoEx.Message}");
                    // 打印错误发生时的缓冲区内容，这是定位问题的关键
                    int errorPreviewLength = Math.Min(startIndex, 128);
                    Debug.LogError($"[ReadBuffer ERROR] Buffer content at time of error (first {errorPreviewLength} bytes): [{BytesToHexString(buffer, 0, errorPreviewLength)}]");
                    Debug.LogError($"--- END PROTOBUF PARSE ERROR ---");


                    // 错误处理日志使用 LogWarning
                    Debug.LogWarning($"[ReadBuffer ERROR Handling] Discarding problematic message data ({Count + 4} bytes).");
                    int bytesToDiscard = Count + 4;
                    int remainingBytes = startIndex - bytesToDiscard;
                    if (remainingBytes < 0) remainingBytes = 0;
                    Array.Copy(buffer, bytesToDiscard, buffer, 0, remainingBytes);
                    startIndex = remainingBytes;
                    Debug.LogWarning($"[ReadBuffer ERROR Handling] Buffer state after discard: startIndex = {startIndex}.");
                }
                catch (Exception ex)
                {
                     // 其他异常也用 LogError
                    Debug.LogError($"--- UNEXPECTED ERROR ---");
                    Debug.LogError($"[ReadBuffer ERROR] Unexpected exception: {ex}");
                    int errorPreviewLength = Math.Min(startIndex, 128);
                    Debug.LogError($"[ReadBuffer ERROR] Buffer content at time of error (first {errorPreviewLength} bytes): [{BytesToHexString(buffer, 0, errorPreviewLength)}]");
                    Debug.LogError($"--- END UNEXPECTED ERROR ---");
                    startIndex = 0;
                    break;
                }
            }
            else
            {
                // 数据不够一个完整消息体，这是正常等待，减少日志打印
                // Debug.Log($"[ReadBuffer Loop] Insufficient data for message body (Need {Count + 4}, Have {startIndex}). Waiting...");
                break;
            }
        }
    }
    public static byte[] ConvertToByteArray(MainPack pack)
    {
        byte[] data = pack.ToByteArray();
        byte[] head = BitConverter.GetBytes(data.Length);
        return head.Concat(data).ToArray();
    }
    public static PlayerControlInput ConvertToPlayerInput(InputPack pack)
    {
        PlayerControlInput input = new PlayerControlInput();
        input.Vertical = pack.Vertical;
        input.Horizontal = pack.Horizontal;
        input.isAttackKeyDown = pack.IsAttackKeyDown;
        input.MouseWorldPos.Set(pack.MousePosX, pack.MousePosY);
        input.CharacterPos.Set(pack.CharacterPosX, pack.CharacterPosY);
        return input;
    }
    public static byte[] PackDataUDP(MainPack pack)
    {
        return pack.ToByteArray();
    }
}
