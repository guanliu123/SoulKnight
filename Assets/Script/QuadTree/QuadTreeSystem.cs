using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OPH.Collision.QuadTree;
using UnityEngine;

public class QuadTreeSystem : AbstractSystem
{
    private List<IRect> rects;
    public QTree<IRect> QuadTree { get;private set; }

    protected override void OnInit()
    {
        base.OnInit();
        rects = new List<IRect>();

        InitializeQuadTree();
        
    }

    private void InitializeQuadTree()
    {
        if (QuadTree == null)
        {
            //var sceneBounds = CalculateSceneBounds();
            QuadTree = QTree<IRect>.CreateRoot(4, 6).InitRect(0, 0, 150,
                150);
        }
    }

    public void GameUpdate()
    {
        //base.GameUpdate();
        TreeUpdate();
    }

    //每帧重构四叉树（效率比每帧更新每个物体高）
    private void TreeUpdate()
    {
        QuadTree.Clear();

        //float t1 = Time.realtimeSinceStartup;
        for (int i = 0; i < rects.Count; ++i)
        {
            {
                QuadTree.Insert(rects[i]);
            }
        }

        //float t2 = Time.realtimeSinceStartup;
        //LogTool.Log("重构四叉树耗时:"+(t2 - t1));
    }

    public void RemoveFromTree(IRect rect)
    {
        rects.Remove(rect);
    }

    public void AddToTree(IRect rect)
    {
        rects.Add(rect);
    }

    public void GetAroundObj(IRect target,List<IRect> list)
    {
        QuadTree.GetAroundObj(target, list);
    }
    
    //=============================ORCA相关
    public List<IRect> QueryCircle(Vector2 center, float radius)
    {
        List<IRect> results = new List<IRect>();
        QueryCircleRecursive(QuadTree, center, radius, results);
        return results;
    }

    private void QueryCircleRecursive(QTree<IRect> node, Vector2 center, float radius, List<IRect> results)
    {
        // 创建临时矩形用于快速碰撞检测
        Rect nodeRect = new Rect(
            node.X - node.Width/2, 
            node.Y - node.Height/2,
            node.Width, 
            node.Height
        );

        if (!CircleRectOverlap(center, radius, nodeRect)) return;

        if (!node.IsLeaf)
        {
            foreach (var child in node.childNodes)
            {
                QueryCircleRecursive(child, center, radius, results);
            }
        }
        else
        {
            foreach (var item in node.childList)
            {
                Rect itemRect = new Rect(
                    item.X - item.Width/2,
                    item.Y - item.Height/2,
                    item.Width,
                    item.Height
                );
        
                if (CircleRectOverlap(center, radius, itemRect))
                {
                    results.Add(item);
                }
            }
        }
    }

    private bool CircleRectOverlap(Vector2 circleCenter, float radius, Rect rect)
    {
        float dx = Mathf.Max(rect.xMin - circleCenter.x, 0, circleCenter.x - rect.xMax);
        float dy = Mathf.Max(rect.yMin - circleCenter.y, 0, circleCenter.y - rect.yMax);
        return (dx*dx + dy*dy) < (radius*radius);
    }
}