using UnityEngine;

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RectCollider))]
public class RectColliderEditor : UnityEditor.Editor
{
    private const float HANDLE_SIZE = 0.2f;

    void OnSceneGUI()
    {
        RectCollider collider = target as RectCollider;
        if (collider == null) return;

        DrawOffsetHandle(collider);
        DrawSizeHandles(collider);
    }

    private void DrawOffsetHandle(RectCollider collider)
    {
        Vector3 worldCenter = collider.transform.TransformPoint(collider._offset);
        
        UnityEditor.Handles.color = Color.cyan;
        var fmh_25_13_638777335961735517 = Quaternion.identity; Vector3 newPos = UnityEditor.Handles.FreeMoveHandle(
            worldCenter,
            HANDLE_SIZE,
            Vector3.zero,
            UnityEditor.Handles.SphereHandleCap
        );

        if (newPos != worldCenter)
        {
            UnityEditor.Undo.RecordObject(collider, "Move Collider Center");
            collider._offset = collider.transform.InverseTransformPoint(newPos);
        }
    }

    private void DrawSizeHandles(RectCollider collider)
    {
        Vector3[] corners = GetWorldCorners(collider);
        
        UnityEditor.Handles.color = Color.green;
        for (int i = 0; i < 4; i++)
        {
            var fmh_47_17_638777335961758368 = Quaternion.identity; Vector3 newPos = UnityEditor.Handles.FreeMoveHandle(
                corners[i],
                HANDLE_SIZE,
                Vector3.zero,
                UnityEditor.Handles.RectangleHandleCap
            );

            if (newPos != corners[i])
            {
                UpdateSize(collider, corners, i, newPos);
                break;
            }
        }
    }

    private Vector3[] GetWorldCorners(RectCollider collider)
    {
        Vector2 center = new Vector2(collider.X, collider.Y);
        Vector2 halfSize = new Vector2(collider.Width/2, collider.Height/2);
        
        return new Vector3[]
        {
            new Vector3(center.x - halfSize.x, center.y + halfSize.y, 0), // 左上
            new Vector3(center.x + halfSize.x, center.y + halfSize.y, 0), // 右上
            new Vector3(center.x + halfSize.x, center.y - halfSize.y, 0), // 右下
            new Vector3(center.x - halfSize.x, center.y - halfSize.y, 0)  // 左下
        };
    }

    private void UpdateSize(RectCollider collider, Vector3[] originalCorners, int movedIndex, Vector3 newPos)
    {
        Vector2 localNewPos = collider.transform.InverseTransformPoint(newPos);
        Vector2 delta = localNewPos - (Vector2)collider.transform.InverseTransformPoint(originalCorners[movedIndex]);

        switch (movedIndex)
        {
            case 0: // 左上
                collider.Width += delta.x;
                collider.Height -= delta.y;
                collider._offset += new Vector2(-delta.x/2, delta.y/2);
                break;
            case 1: // 右上
                collider.Width -= delta.x;
                collider.Height -= delta.y;
                collider._offset += new Vector2(delta.x/2, delta.y/2);
                break;
            case 2: // 右下
                collider.Width -= delta.x;
                collider.Height += delta.y;
                collider._offset += new Vector2(delta.x/2, -delta.y/2);
                break;
            case 3: // 左下
                collider.Width += delta.x;
                collider.Height += delta.y;
                collider._offset += new Vector2(-delta.x/2, -delta.y/2);
                break;
        }
    }
}
#endif
