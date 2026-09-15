using UnityEngine;

public class CourtManager : MonoBehaviour
{
    // =========================
    // 코트 영역
    // =========================

    public BoxCollider playerArea;
    public BoxCollider enemyArea;


    // =========================
    // IN / OUT 판정
    // =========================

    public bool IsInsidePlayerArea(
        Vector3 position
    )
    {
        return IsInsideArea(
            position,
            playerArea
        );
    }


    public bool IsInsideEnemyArea(
        Vector3 position
    )
    {
        return IsInsideArea(
            position,
            enemyArea
        );
    }


    private bool IsInsideArea(
        Vector3 position,
        BoxCollider area
    )
    {
        if (area == null)
            return false;


        Bounds bounds =
            area.bounds;


        return
            position.x >= bounds.min.x &&
            position.x <= bounds.max.x &&
            position.z >= bounds.min.z &&
            position.z <= bounds.max.z;
    }


    // =========================
    // Player가 Enemy 코트를
    // 조준할 때 사용
    // =========================

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


    // =========================
    // Enemy가 Player 코트를
    // 조준할 때 사용
    // =========================

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


    // =========================
    // 실제 목표 위치 계산
    // =========================

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


        Bounds bounds =
            area.bounds;


        // -1 ~ +1
        horizontal =
            Mathf.Clamp(
                horizontal,
                -1f,
                1f
            );


        // -1 ~ +1
        //
        // -1 = 짧게
        //  0 = 중앙
        // +1 = 깊게
        depth =
            Mathf.Clamp(
                depth,
                -1f,
                1f
            );


        // =========================
        // 좌우 위치
        // =========================

        float minX =
            bounds.min.x + margin;

        float maxX =
            bounds.max.x - margin;


        float horizontal01 =
            (horizontal + 1f)
            * 0.5f;


        float targetX =
            Mathf.Lerp(
                minX,
                maxX,
                horizontal01
            );


        // =========================
        // 깊이 위치
        // =========================

        float nearZ;
        float farZ;


        if (enemySide)
        {
            // Enemy 코트는 +Z
            //
            // Net 쪽 = 작은 Z
            // Baseline 쪽 = 큰 Z

            nearZ =
                bounds.min.z + margin;

            farZ =
                bounds.max.z - margin;
        }
        else
        {
            // Player 코트는 -Z
            //
            // Net 쪽 = 큰 Z
            // Baseline 쪽 = 작은 Z

            nearZ =
                bounds.max.z - margin;

            farZ =
                bounds.min.z + margin;
        }


        float depth01 =
            (depth + 1f)
            * 0.5f;


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