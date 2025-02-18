
#region

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

#endregion

public enum ResType
{
    PREFAB = 1,
    TEXTURE2D = 2,
    AUDIOCLIP = 3
}

public class LoadManager : SingletonBase<LoadManager>
{
    private List<string> _bundleCacheList = new();
    public string CacheingName = "ResCache";
    protected float m_unload_unused_asset_timer;
    public float m_unload_unused_asset_time = 30.0f;
    public const bool FORCE_TEST = true;
    protected Dictionary<string, bool> mObjectHasPool = new();
    protected Dictionary<string, IAsset> mAssets = new();
    private readonly List<string> _keysToDelete = new();
    private readonly Queue<InstanceRequest> mInstanceOnePerFrameQueue = new();
    private readonly Queue<InstanceRequest> mInstanceQueue = new();
    private readonly Queue<InstanceRequest> mInstance20PerFrameQueue = new();
    
    
    private void OnUpdate()
    {
        while (mInstanceQueue.Count > 0)
        {
            InstanceRequest request = mInstanceQueue.Dequeue();
            if (request.callback != null)
            {
                Instantiate(request.assetName, request.callback);
            }
        }
        
        int nCount = 0;
        while (mInstance20PerFrameQueue.Count > 0 && nCount != 20)
        {
            InstanceRequest request = mInstance20PerFrameQueue.Dequeue();
            if (request.callback != null)
            {
                Instantiate(request.assetName, request.callback);
            }
            
            nCount++;
        }
        
        if (mInstanceOnePerFrameQueue.Count > 0)
        {
            InstanceRequest request = mInstanceOnePerFrameQueue.Dequeue();
            if (request.callback != null)
            {
                Instantiate(request.assetName, request.callback);
            }
        }
        
        m_unload_unused_asset_timer += Time.deltaTime;
        if (m_unload_unused_asset_timer >= m_unload_unused_asset_time)
        {
            try
            {
                UnloadUnusedAssets();
            }
            catch (Exception ex)
            {
                LogTool.LogException(ex);
            }
            
            m_unload_unused_asset_timer = 0f;
        }
    }
    
    public string WWWProtoHead
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return "";
#else
            return "file://";
#endif
        }
    }
    
    public async void Init()
    {
        MonoManager.Instance.AddUpdateAction(OnUpdate);
        if (Application.isEditor || FORCE_TEST)
        {
            return;
        }
        
        //从StreamingAsset中获取bundle列表文件
        string bundleCacheFileURL = $"{Addressables.RuntimePath}/{CacheingName}/{CacheingName}.json";
        string url = bundleCacheFileURL;
#if UNITY_EDITOR
        //windows和mac平台上需要全路径，移动平台上是虚拟路径
        url = Path.GetFullPath(url);
#endif
        url = $"{WWWProtoHead}{url}";
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        //await request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
        await request.SendWebRequestTask(); // 使用自定义扩展方法
#else
    // 低版本Unity保持原生协程方式
    yield return request.SendWebRequest();
#endif
        if (!string.IsNullOrEmpty(request.error))
        {
            LogTool.LogError(request.error);
        }
        
        _bundleCacheList = JsonTool.DeserializeObject<List<string>>(request.downloadHandler.text);
        Addressables.InternalIdTransformFunc = Addressables_InternalIdTransformFunc;
    }
    
    public T Load<T>(string assetName, bool cache = false) where T : Object
    {
        IAsset _asset;
        if (mAssets.ContainsKey(assetName))
        {
            _asset = mAssets[assetName];
            if (_asset != null)
            {
                return _asset.asset() as T;
            }
        }
        
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>("Assets/Resources_moved/"+assetName);
        T go = handle.WaitForCompletion();
        if (handle.Status == AsyncOperationStatus.Failed)
        {
            LogTool.LogError($"同步加载资源失败，Path：{assetName}, OpException{handle.OperationException}");
            return null;
        }
        
        AAsset<T> newAsset = new(handle, assetName);
        mAssets.TryAdd(assetName, newAsset);
        _asset = newAsset;
        // res = handle.Result;
        if (cache)
        {
            newAsset.Retain();
        }
        
        return _asset.asset()==null?null:_asset.asset() as T;
    }
    
    public void LoadAsync<T>(string assetName, Action<IAsset> callback = null, bool cache = false)
        where T : Object
    {
        // MonoManager.Instance.StartCoroutine(RealLoadAsync(name, callback, cache));
        if (string.IsNullOrEmpty(assetName))
        {
            LogTool.LogErrorFormat("LoadAsync param name is null");
            return;
        }
        
        IAsset _asset;
        if (mAssets.ContainsKey(assetName))
        {
            _asset = mAssets[assetName];
            if (_asset != null)
            {
                callback?.Invoke(_asset);
                return;
            }
        }
        
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>("Assets/Resources_moved/"+assetName);
        if (handle.IsDone && handle.Result == null)
        {
            LogTool.LogError($"异步加载资源失败，Path：{assetName}, OpException{handle.OperationException}");
        }
        
        handle.Completed += (AsyncOperationHandle<T> obj) =>
        {
            AAsset<T> newAsset = new(handle, assetName);
            mAssets.TryAdd(assetName, newAsset);
            callback?.Invoke(newAsset);
            if (cache)
            {
                newAsset.Retain();
            }
        };
    }
    
    public void Instantiate(string assetName, Action<Object> completed)
    {
        // if (ownerView != null && ownerView.gameObject == null)
        // {
        //     LogTool.LogError("Instantiate ownerView.gameObject is null");
        //     return;
        // }
        
        // 已经知道是 回收对象了。直接调用回收池
        if (mObjectHasPool.ContainsKey(assetName))
        {
            ObjectPoolManager.Instance.GetPool(assetName).Spawn(assetName, completed);
            return;
        }
        
        if (assetName == null || assetName.Equals(string.Empty))
        {
            LogTool.LogError("Instantiate assetName is null");
            return;
        }
        
        LoadAsync<GameObject>(assetName, (IAsset asset) =>
        {
            Object objAsset;
            if (asset.asset() == null)
            {
                LogTool.LogError("no find " + assetName);
                // gameObject = new GameObject(assetName);
                // completed?.Invoke(gameObject);
                return;
            }
            else
            {
                objAsset = asset.asset();
            }
            
            ObjectPoolItem component = null;
            if (objAsset is GameObject)
            {
                component = (objAsset as GameObject).GetComponent<ObjectPoolItem>();
            }
            
            if (component != null)
            {
                if (component.poolName == null || component.poolName.Equals(string.Empty))
                {
                    component.poolName = assetName;
                }
                
                ObjectPoolManager.Instance.GetPool(component.poolName).Spawn(component.poolName, completed);
            }
            else
            {
                Object _object;
                // if (ownerView != null)
                // {
                //     _object = ownerView.Instantiate(objAsset);
                // }
                // else
                // {
                //     _object = Object.Instantiate(objAsset);
                // }
                _object = Object.Instantiate(objAsset);
                asset.Retain(_object);
                try
                {
                    completed?.Invoke(_object);
                }
                catch (Exception ex)
                {
                    LogTool.LogError($"{assetName} Callback Error: {ex.ToString()}");
                }
            }
        });
    }
    
    public Object Instantiate(IAsset asset, bool cache = false)
    {
        GameObject gameObject;
        if (asset.asset() == null)
        {
            LogTool.LogError("no find asset");
            // gameObject = new GameObject(assetName);
            // completed?.Invoke(gameObject);
            return null;
        }
        else
        {
            gameObject = asset.asset() as GameObject;
        }
        
        Object _object;
        // if (ownerView != null)
        // {
        //     _object = ownerView.Instantiate(gameObject);
        // }
        // else
        // {
        //     _object = Object.Instantiate(gameObject);
        // }
        _object = Object.Instantiate(gameObject);
        
        asset.Retain(_object);
        return _object;
    }
    
    public void Destroy(GameObject gameObject)
    {
        // 容错
        if (gameObject == null)
        {
            return;
        }
        
        ObjectPoolItem component = gameObject.GetComponent<ObjectPoolItem>();
        if (component != null && component.poolName != null)
        {
            ObjectPoolManager.Instance.GetPool(component.poolName).DeSpawn(gameObject, component.poolName);
            if (!mObjectHasPool.ContainsKey(component.poolName))
            {
                mObjectHasPool.Add(component.poolName, true);
            }
        }
        else
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
    
    public void UnloadUnusedAssets()
    {
        ObjectPoolManager.Instance.TryCleanPool();
        
        // for (int i = 0; i < mAssets.Count; i++)
        // {
        //     int status = mAssets[i].Process();
        //     if (status == 0)
        //     {
        //         break;
        //     }
        //     else if (mAssets[i].Process() == 2)
        //     {
        //         continue;
        //     }
        //     else if (status == 1)
        //     {
        //         mAssets.RemoveAt(i);
        //         i--;
        //     }
        // }
        if (mAssets.Count > 0)
        {
            _keysToDelete.Clear();
            foreach (KeyValuePair<string, IAsset> kvp in mAssets)
            {
                if (kvp.Value.Process() == 1)
                {
                    _keysToDelete.Add(kvp.Key);
                }
            }
            
            if (_keysToDelete.Count > 0)
            {
                foreach (string key in _keysToDelete)
                {
                    mAssets.Remove(key, out IAsset removed);
                }
            }
        }
        
        m_unload_unused_asset_timer = 0.0f;
        // GC.Collect();
        // Resources.UnloadUnusedAssets();
    }
    
    // public static Dictionary<string, IAsset> LoadList(List<LoadResItem> list,
    //     UnityAction<int, int> progressCallback = null)
    // {
    //     Dictionary<string, IAsset> assetsRet = new();
    //     int completedCount = 0;
    //     foreach (LoadResItem item in list)
    //     {
    //         IAsset asset = null;
    //         switch (item.Type)
    //         {
    //             case ResType.PREFAB:
    //                 asset = Instance.Load<Object>(item.Path, item.Cache);
    //                 break;
    //             case ResType.TEXTURE2D:
    //                 asset = Instance.Load<Texture2D>(item.Path, item.Cache);
    //                 break;
    //             case ResType.AUDIOCLIP:
    //                 asset = Instance.Load<AudioClip>(item.Path, item.Cache);
    //                 break;
    //         }
    //         
    //         if (assetsRet.ContainsKey(item.Path))
    //         {
    //             assetsRet[item.Path] = asset;
    //         }
    //         else
    //         {
    //             assetsRet.Add(item.Path, asset);
    //         }
    //         
    //         completedCount++;
    //         if (progressCallback != null)
    //         {
    //             progressCallback(completedCount, list.Count);
    //         }
    //     }
    //     
    //     return assetsRet;
    // }
    
    private string Addressables_InternalIdTransformFunc(IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            if (_bundleCacheList.Contains(location.PrimaryKey))
            {
                string fileName = Path.GetFileName(location.PrimaryKey);
                //使用LogError用来测试是否使用了StreamingAsset缓存
                return $"{Addressables.RuntimePath}/{CacheingName}/{fileName}";
            }
        }
        
        return location.InternalId;
    }
    
    //一帧仅实例化一个
    public void InstantiateOnePerFrame(string assetName, Action<Object> completed)
    {
        InstanceRequest request = new();
        request.assetName = assetName;
        request.callback = completed;
       // request.ownerView = ownerView;
        mInstanceOnePerFrameQueue.Enqueue(request);
    }
    
    //下一帧实例化
    public void InstantiateNextFrame(string assetName, Action<Object> completed)
    {
        InstanceRequest request = new();
        request.assetName = assetName;
        request.callback = completed;
        //request.ownerView = ownerView;
        mInstanceQueue.Enqueue(request);
    }
    
    //一帧最多实例化20个
    public void Instantiate20PerFrame(string assetName, Action<Object> completed)
    {
        InstanceRequest request = new();
        request.assetName = assetName;
        request.callback = completed;
        //request.ownerView = ownerView;
        mInstance20PerFrameQueue.Enqueue(request);
    }
}

public class LoadResItem
{
    public bool Cache;
    public string Path;
    
    public LoadResItem(ResType type, string path, bool cache)
    {
        Type = type;
        Path = path;
        Cache = cache;
    }
    
    public ResType Type { set; get; }
}

// 辅助类，用于保存加载操作和相应的key
public class LoadOperationContext<T>
{
    public LoadOperationContext(AsyncOperationHandle<T> handle, string key, UnityAction<T> callback)
    {
        Handle = handle;
        Key = key;
        Callback = callback;
    }
    
    public AsyncOperationHandle<T> Handle { get; private set; }
    public string Key { get; private set; }
    public UnityAction<T> Callback { get; private set; }
}

public struct InstanceRequest
{
    public Action<Object> callback;
    public string assetName;
    //public ComponentView ownerView;
}

public interface IAsset
{
    string assetName();
    Object asset();
    void Retain(Object obj);
    void Release();
    
    void Retain();
    
    // 返回 0 break;  1 Remove 2 continue
    int Process();
}

public class AAsset<T> : IAsset where T : Object
{
    private AsyncOperationHandle<T> mHanddler;
    private readonly Queue<Object> mHolderObjects = new();
    private int mRefCount = 0;
    private readonly string mAssetName;
    private readonly Fix64 fReleaseTime = (Fix64)0.0f;
    
    public AAsset(AsyncOperationHandle<T> handdler, string name)
    {
        mHanddler = handdler;
        mAssetName = name;
        fReleaseTime = Time.realtimeSinceStartup + (Fix64)30;
    }
    
    public string assetName()
    {
        return mAssetName;
    }
    
    public Object asset()
    {
        return mHanddler.Result;
    }
    
    public void Retain(Object obj)
    {
        mHolderObjects.Enqueue(obj);
        Retain();
    }
    
    public void Retain()
    {
        mRefCount++;
    }
    
    public void Release()
    {
        mRefCount--;
        if (mRefCount <= 0)
        {
            mRefCount = 0;
            Addressables.Release(mHanddler);
            mHolderObjects.Clear();
        }
    }
    
    public int Process()
    {
        if ((Fix64)Time.realtimeSinceStartup < fReleaseTime)
        {
            return 0;
        }
        
        if (mHolderObjects.Count > 0)
        {
            while (mHolderObjects.Count > 0)
            {
                if (mHolderObjects.Peek() == null)
                {
                    mHolderObjects.Dequeue();
                    Release();
                }
                else
                {
                    break;
                }
            }
            
            
            if (mRefCount == 0)
            {
                return 1;
            }
            
            return 2;
        }
        else
        {
            if (mRefCount == 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}