/*
 * @Author: SonG
 * @Email: songning@jinglonggame.com
 * @Description:
 * @Date: 2024.09.19 星期四 17:09:49
 */

#region

using Luban;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#endregion

public class TableManager : SingletonBase<TableManager>
{
    public override void Init()
    {
        //BetterStreamingAssets.Initialize();
        Tables = new cfg.Tables(LoadByteBuf);
    }
    
    private JSONNode LoadByteBuf(string file)
    {
        // AsyncOperationHandle<TextAsset> handle =
        //     Addressables.LoadAssetAsync<TextAsset>("Data/" + file);
        // TextAsset go = handle.WaitForCompletion();
        // if (handle.Status == AsyncOperationStatus.Succeeded)
        // {
        //     TextAsset loadedBytes = handle.Result;
        //     //Addressables.Release(handle);
        //     return new ByteBuf(loadedBytes.bytes);
        // }
        // else
        // {
        //     LogTool.LogError($"Failed to load bytes file: {file}");
        //     return null;
        // }
        AsyncOperationHandle<TextAsset> handle = 
            Addressables.LoadAssetAsync<TextAsset>("Assets/Resources_moved/Data/Luban/" + file+".json");
        TextAsset go = handle.WaitForCompletion();
    
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 重要:需要先获取文本内容后释放资源
            JSONNode json = JSON.Parse(go.text);
            Addressables.Release(handle); // 按需决定是否立即释放
            return json;
        }
        else
        {
            LogTool.LogError($"Failed to load json file: {file}");
            return null;
        }
    }
    
    //private static ByteBuf LoadByteBuf(string file)
    //{
    //	return new ByteBuf(BetterStreamingAssets.ReadAllBytes("/Data/" + file + ".bytes"));
    //}
    
    
    public cfg.Tables Tables { get; private set; }
}