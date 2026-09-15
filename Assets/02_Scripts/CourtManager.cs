using UnityEngine;

public class CourtManager : MonoBehaviour
{
    [Header("Court Area")]
    public BoxCollider playerArea;
    public BoxCollider enemyArea;

    [Header("Enemy Serve Area")]
    public BoxCollider enemyServeAreaLeft;
    public BoxCollider enemyServeAreaRight;


    public bool IsInsidePlayerArea(Vector3 position)
    {
        return IsInsideArea(position, playerArea);
    }

    public bool IsInsideEnemyArea(Vector3 position)
    {
        return IsInsideArea(position, enemyArea);
    }

    public bool IsInsideEnemyServeAreaLeft(Vector3 position)
    {
        return IsInsideArea(position, enemyServeAreaLeft);
    }

    public bool IsInsideEnemyServeAreaRight(Vector3 position)
    {
        return IsInsideArea(position, enemyServeAreaRight);
    }


    private bool IsInsideArea(
        Vector3 position,
        BoxCollider area
    )
    {
        if (area == null)
            return false;

        Bounds bounds = area.bounds;

        return
            position.x >= bounds.min.x &&
            position.x <= bounds.max.x &&
            position.z >= bounds.min.z &&
            position.z <= bounds.max.z;
    }


    public Vector3 GetEnemyTargetPoint(
        float horizontal,
        float depth,
        float margin
    )
    {
        return GetTargetPoint(
            enemyArea,
            horizontal,
            depth,
            margin,
            true
        );
    }


    public Vector3 GetPlayerTargetPoint(
        float horizontal,
        float depth,
        float margin
    )
    {
        return GetTargetPoint(
            playerArea,
            horizontal,
            depth,
            margin,
            false
        );
    }


    public Vector3 GetEnemyServeTargetPoint(
        bool targetLeft,
        float horizontal,
        float depth,
        float margin
    )
    {
        BoxCollider targetArea;

        if (targetLeft)
        {
            targetArea = enemyServeAreaLeft;
        }
        else
        {
            targetArea = enemyServeAreaRight;
        }

        return GetTargetPoint(
            targetArea,
            horizontal,
            depth,
            margin,
            true
        );
    }


    private Vector3 GetTargetPoint(
        BoxCollider area,
        float horizontal,
        float depth,
        float margin,
        bool enemySide
    )
    {
        if (area == null)
        {
            Debug.LogError(
                "CourtManager: Court Area가 연결되어 있지 않습니다."
            );

            return Vector3.zero;
        }

        Bounds bounds = area.bounds;

        horizontal =
            Mathf.Clamp(horizontal, -1f, 1f);

        depth =
            Mathf.Clamp(depth, -1f, 1f);

        float minX =
            bounds.min.x + margin;

        float maxX =
            bounds.max.x - margin;

        float horizontal01 =
            (horizontal + 1f) * 0.5f;

        float targetX =
            Mathf.Lerp(
                minX,
                maxX,
                horizontal01
            );

        float nearZ;
        float farZ;

        if (enemySide)
        {
            nearZ =
                bounds.min.z + margin;

            farZ =
                bounds.max.z - margin;
        }
        else
        {
            nearZ =
                bounds.max.z - margin;

            farZ =
                bounds.min.z + margin;
        }

        float depth01 =
            (depth + 1f) * 0.5f;

        float targetZ =
            Mathf.Lerp(
                nearZ,
                farZ,
                depth01
            );

        return new Vector3(
            targetX,
            bounds.center.y,
            targetZ
        );
    }
}