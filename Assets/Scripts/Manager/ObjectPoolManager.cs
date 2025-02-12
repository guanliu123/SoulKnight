#region

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

#endregion

public class ObjectPoolItem : MonoBehaviour
{
    private List<ObjectPoolItem> m_childPoolItem;
    
    private FixVector3 m_org_scale = FixVector3.Zero;
    
    public string poolName { get; set; }
    
    private void Awake()
    {
        m_org_scale = new FixVector3(transform.lossyScale);;
    }
    
    public void Disable(float delay)
    {
        base.StartCoroutine(this._disable(delay));
    }
    
    private IEnumerator _disable(float delay)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            Disable();
        }
        catch (Exception e)
        {
            LogTool.LogException(e);
        }
    }
    
    public virtual void Reset()
    {
    }
    
    public bool IsActive()
    {
        return base.gameObject.activeInHierarchy;
    }
    
    public virtual void Disable()
    {
        ObjectPoolManager.Instance.GetPool(this.poolName).DeSpawn(base.gameObject, this.poolName);
    }
    
    public virtual void OnObjectSpawn()
    {
        SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
    }
    
    public virtual void OnObjectDespawn()
    {
        PreDespawnChild();
        //transform.localScale = this.m_org_scale;
        SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
    }
    
    public void SearchParent()
    {
        Transform parent = base.transform.parent;
        if (parent != null)
        {
            ObjectPoolItem componentInParent = parent.GetComponentInParent<ObjectPoolItem>();
            if (componentInParent != null)
            {
                componentInParent.RegisterChild(this);
            }
        }
    }
    
    public void RegisterChild(ObjectPoolItem obj)
    {
        if (this.m_childPoolItem == null)
        {
            this.m_childPoolItem = new List<ObjectPoolItem>();
        }
        
        this.m_childPoolItem.Add(obj);
    }
    
    private void PreDespawnChild()
    {
        if (this.m_childPoolItem != null)
        {
            for (int i = 0; i < this.m_childPoolItem.Count; i++)
            {
                if (this.m_childPoolItem[i] != null)
                {
                    this.m_childPoolItem[i].Disable();
                }
            }
            
            this.m_childPoolItem.Clear();
        }
    }
}

public class ObjectPool
{
    private readonly Stack<GameObject> m_pool = new();
    
    private string m_PoolObjName = string.Empty;
    
    private int m_spawn_number;
    
    public float lastDespawnTime { get; set; } = 3.40282347E+38f;
    
    public int avaliableCount
    {
        get => this.m_pool.Count;
        set { }
    }
    
    public void Spawn(string ResourceName, Action<GameObject> action)
    {
        if (this.m_PoolObjName == string.Empty)
        {
            this.m_PoolObjName = ResourceName;
        }
        
        Spawn(action);
    }
    
    private void Spawn(Action<GameObject> action)
    {
        this.lastDespawnTime = 3.40282347E+38f;
        if (this.m_pool.Count > 0)
        {
            GameObject gameObject = this.m_pool.Pop();
            gameObject.transform.SetParent(null);
            gameObject.SetActive(true);
            gameObject.GetComponent<ObjectPoolItem>().OnObjectSpawn();
            action?.Invoke(gameObject);
            return;
        }
        
        LoadManager.Instance.LoadAsync<Object>(m_PoolObjName, (IAsset asset) =>
        {
            GameObject gameObject2 = GameObject.Instantiate(asset.asset() as GameObject);
            asset.Retain(gameObject2);
            ObjectPoolItem component = gameObject2.GetComponent<ObjectPoolItem>();
            if (component != null)
            {
                component.poolName = this.m_PoolObjName;
                component.OnObjectSpawn();
                this.m_spawn_number++;
            }
            else
            {
                LogTool.LogError($"{m_PoolObjName} not ObjectPoolItem");
            }
            
            action?.Invoke(gameObject2);
        }, true);
    }
    
    public void DeSpawn(GameObject obj, string ResourceName)
    {
        lastDespawnTime = Time.realtimeSinceStartup;
        if (m_pool.Contains(obj))
        {
            //Debug.LogError("ObjectPoolManager : despawn object already in stack = " + obj.name);
            return;
        }
        
        if (this.m_PoolObjName != ResourceName)
        {
            return;
        }
        
        obj.transform.SetParent(ObjectPoolManager.Instance.m_handler.transform);
        obj.GetComponent<ObjectPoolItem>().OnObjectDespawn();
        obj.SetActive(false);
        this.m_pool.Push(obj);
    }
    
    public void Clear()
    {
        while (this.m_pool.Count != 0)
        {
            UnityEngine.Object.Destroy(this.m_pool.Pop());
        }
        
        m_pool.Clear();
        m_spawn_number = 0;
    }
    
    public void TryClean()
    {
        if (Time.realtimeSinceStartup - this.lastDespawnTime >= ObjectPoolManager.Instance.m_clean_pool_time)
        {
            this.Clear();
        }
    }
}

public class ObjectPoolManager : SingletonBase<ObjectPoolManager>
{
    private static readonly Dictionary<string, ObjectPool> m_poolList = new();
    
    public GameObject m_handler;
    
    public float m_clean_pool_time = 30.0f;
    
    
    public bool IsAnyInPool(UnityEngine.Object obj)
    {
        GameObject gameObject = (GameObject)obj;
        ObjectPoolItem component = gameObject.GetComponent<ObjectPoolItem>();
        return component && this.GetPool(component.poolName).avaliableCount != 0;
    }
    
    public ObjectPool GetPool(string name)
    {
        if (!m_poolList.ContainsKey(name))
        {
            m_poolList.Add(name, new ObjectPool());
        }
        
        return m_poolList[name];
    }
    
    public void ClearPool(string name)
    {
        if (!m_poolList.ContainsKey(name))
        {
            return;
        }
        
        m_poolList[name].Clear();
        m_poolList.Remove(name);
    }
    
    public void ClearAllPool()
    {
        foreach (ObjectPool current in m_poolList.Values)
        {
            current.Clear();
        }
        
        m_poolList.Clear();
    }
    
    public void TryCleanPool()
    {
        foreach (ObjectPool current in m_poolList.Values)
        {
            current.TryClean();
        }
    }
}