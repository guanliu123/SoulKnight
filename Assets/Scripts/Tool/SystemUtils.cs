
#region

using System;

#endregion

public class SystemUtils
{
    /// <summary>
    ///     将传入的二进制字符串资料以GZip算法解压缩
    /// </summary>
    /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
    /// <returns>原始未压缩字符串</returns>
    public static string DecompressGzipBase64(string zippedString)
    {
        if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
        {
            return "";
        }
        else
        {
            byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
            return (string)System.Text.Encoding.UTF8.GetString(Decompress(zippedData));
        }
    }
    
    /// <summary>
    ///     ZIP解压
    /// </summary>
    /// <param name="zippedData"></param>
    /// <returns></returns>
    public static byte[] Decompress(byte[] zippedData)
    {
        System.IO.MemoryStream ms = new(zippedData);
        System.IO.Compression.GZipStream
            compressedzipStream = new(ms, System.IO.Compression.CompressionMode.Decompress);
        System.IO.MemoryStream outBuffer = new();
        byte[] block = new byte[1024];
        while (true)
        {
            int bytesRead = compressedzipStream.Read(block, 0, block.Length);
            if (bytesRead <= 0)
            {
                break;
            }
            else
            {
                outBuffer.Write(block, 0, bytesRead);
            }
        }
        
        compressedzipStream.Close();
        return outBuffer.ToArray();
    }
}