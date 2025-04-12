using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OPH.Collision.QuadTree;
using UnityEngine.Pool;

public class RectInfo
{
    public IRect rect;
    public bool isExist;
}

[ExecuteInEditMode]
public class RectCollider : MonoBehaviour, IRect
{
    #region 公共接口
    public float X
    {
        get => _worldPosition.x;
        set
        {
            Vector3 newPos = new Vector3(value, Y, transform.position.z);
            _offset = transform.InverseTransformPoint(newPos);
            //MarkDirty();
        }
    }

    public float Y
    {
        get => _worldPosition.y;
        set
        {
            Vector3 newPos = new Vector3(X, value, transform.position.z);
            _offset = transform.InverseTransformPoint(newPos);
            //MarkDirty();
        }
    }

    public float Width
    {
        get => _size.x;
        set
        {
            _size.x = Mathf.Max(0.1f, value);
            //MarkDirty();
        }
    }

    public float Height
    {
        get => _size.y;
        set
        {
            _size.y = Mathf.Max(0.1f, value);
            //MarkDirty();
        }
    }

    public bool IsObstacle;
    #endregion

    #region 序列化字段
    [SerializeField]public Vector2 _offset;
    [SerializeField] Vector2 _size = Vector2.one;
    [SerializeField] bool _drawGizmos = true;
    [SerializeField] Color _gizmoColor = Color.green;
    #endregion

    #region 私有变量
    private bool canCollide = false;
    private Vector2 _worldPosition;
    private Vector3 _lastTransformPosition;
    private Vector2 _lastSize;
    private Vector2 _lastOffset;
    private bool _isDirty;
    private bool _isInTree;
    private QuadTreeSystem _quadTreeSystem;
    
    //与其碰撞中的物品列表，每次检查时从这个列表中查找，没有的加入，OnTriggerEnter，有的保持，OnTriggerStay，有但是检查时没有去除，OnTriggerExit
    private List<RectInfo> colliders = new List<RectInfo>();
    #endregion

    #region 生命周期方法
    void Awake()
    {
        //InitializeQuadTree();
        CacheTransformState();
        colliders = new();
    }

    void Update()
    {
        UpdateWorldPosition();
        
        if (_quadTreeSystem == null||!canCollide||gameObject.tag == "Obstacles") return;

        // 使用对象池获取临时列表（避免GC分配）
        var candidates = ListPool<IRect>.Get();
        _quadTreeSystem.GetAroundObj(this, candidates);

        foreach (var other in candidates)
        {
            if ( CheckCollision(this, other))
            {
                CheckCollidType(other);
                //TriggerManager.Instance.NotisfyObserver(TriggerType.TriggerStay,gameObject,((MonoBehaviour)other).gameObject);
            }
        }

        for (int i = 0; i < colliders.Count;)
        {
            if (colliders[i].isExist)
            {
                colliders[i++].isExist = false;
                continue;
            }
            TriggerCenter.Instance.NotisfyObserver(TriggerType.OnTriggerExit,gameObject, ((MonoBehaviour)colliders[i].rect).gameObject);

            colliders.RemoveAt(i);
        }

        // 正确释放到对象池（修正语法错误）
        ListPool<IRect>.Release(candidates);
    }
    
    // 基于轴对齐矩形相交算法 ([cxyzjd.com](https://www.cxyzjd.com/article/zouxin_88/100831313))
    bool CheckCollision(IRect a, IRect b)
    {
        float dx = Mathf.Abs(a.X - b.X);
        float dy = Mathf.Abs(a.Y - b.Y);
        return dx <= (a.Width + b.Width)/2 && dy <= (a.Height + b.Height)/2;
    }

    void CheckCollidType(IRect rect)
    {
        int idx=0;
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].rect == rect)
            {
                colliders[idx].isExist = true;
                //TriggerManager.Instance.NotisfyObserver(TriggerType.TriggerStay,gameObject,((MonoBehaviour)colliders[i].rect).gameObject);
                return;
            }
        }
        
        colliders.Add(new RectInfo() { rect = rect, isExist = true });
        TriggerCenter.Instance.NotisfyObserver(TriggerType.OnTriggerEnter,gameObject,((MonoBehaviour)rect).gameObject);
    }

    void OnEnable()
    {
        _quadTreeSystem = GameMediator.Instance.GetSystem<QuadTreeSystem>();
        //障碍物自动加入四叉树管理
        if (gameObject.tag == "Obstacles")
        {
            EnableCollision();
            IsObstacle = true;
            canCollide = false;
        }
    }

    void OnDisable()
    {
        ForceRemoveFromQuadTree();
    }

    void OnDestroy()
    {
        ForceRemoveFromQuadTree();
    }
    #endregion

    #region 碰撞控制API
    /// <summary>
    /// 激活碰撞检测
    /// </summary>
    public void EnableCollision()
    {
        canCollide = true;
        if (!_isInTree)
        {
            _isInTree = true;
            _quadTreeSystem.AddToTree(this);
        }
    }

    /// <summary>
    /// 禁用碰撞检测
    /// </summary>
    public void DisableCollision()
    {
        canCollide = false;
        ForceRemoveFromQuadTree();
    }
    #endregion
    

    private void ForceRemoveFromQuadTree()
    {
        colliders.Clear();
        if(!_isInTree) return;
        _quadTreeSystem.RemoveFromTree(this);
    }

    #region 状态管理
    private void CacheTransformState()
    {
        _lastTransformPosition = transform.position;
        _lastSize = _size;
        _lastOffset = _offset;
        UpdateWorldPosition();
    }

    private void CheckTransformChanges()
    {
        bool transformChanged = transform.position != _lastTransformPosition;
        bool sizeChanged = _size != _lastSize;
        bool offsetChanged = _offset != _lastOffset;

        if (transformChanged || sizeChanged || offsetChanged)
        {
            MarkDirty();
            CacheTransformState();
        }
    }

    private void UpdateWorldPosition()
    {
        _worldPosition = transform.TransformPoint(_offset);
    }

    private void MarkDirty()
    {
        if (!_isDirty)
        {
            StartCoroutine(DelayedUpdate());
            _isDirty = true;
        }
    }

    private IEnumerator DelayedUpdate()
    {
        yield return new WaitForEndOfFrame();
        //UpdateQuadTree();
        _isDirty = false;
    }
    #endregion
    
    // 尺寸设置方法
    public void SetWorldSize(Vector2 worldSize)
    {
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;
            if (Mathf.Approximately(parentScale.x, 0) || 
                Mathf.Approximately(parentScale.y, 0))
            {
                Debug.LogError($"父级缩放值异常：{parentScale}");
                return;
            }

            // 精确计算本地缩放
            transform.localScale = new Vector3(
                worldSize.x / parentScale.x,
                worldSize.y / parentScale.y,
                1
            );
        }
        else
        {
            transform.localScale = new Vector3(worldSize.x, worldSize.y, 1);
        }

        _size = worldSize;
    }

    // 缩放补偿逻辑
    private void AdjustLocalScale()
    {
        if (transform.parent != null)
        {
            Vector3 parentLossyScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                _size.x / parentLossyScale.x,
                _size.y / parentLossyScale.y,
                1
            );
            _size = Vector2.Scale(transform.localScale, parentLossyScale);
        }
    }
    
    #region 可视化
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;

        Gizmos.color = _gizmoColor;
        DrawColliderFrame();
        DrawOffsetHandle();
    }

    private void DrawColliderFrame()
    {
        Vector3 center = new Vector3(X, Y, 0);
        Vector3 size = new Vector3(Width, Height, 0.1f);
        
        Gizmos.DrawWireCube(center, size);
        Gizmos.color *= new Color(1,1,1,0.3f);
        Gizmos.DrawCube(center, size);
    }

    private void DrawOffsetHandle()
    {
        Vector3 handlePos = transform.TransformPoint(_offset);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(handlePos, 0.1f);
    }
#endif
    #endregion
}

