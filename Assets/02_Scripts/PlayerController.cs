using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // =========================
    // 이동
    // =========================

    [Header("Movement")]

    public float moveSpeed = 5f;

    public float moveLimitX = 8f;

    public float minZ = -9f;

    public float maxZ = -1f;


    // =========================
    // 타격
    // =========================

    [Header("Hit")]

    public float hitRange = 2f;

    public float hitSpeed = 10f;

    public float hitHeight = 4f;

    public float hitCooldown = 0.3f;

    private float lastHitTime =
        -999f;


    // =========================
    // 조준
    // =========================

    [Header("Aim")]

    // 코트 라인 바로 위를 노리지 않도록
    // 안쪽 여유 공간
    public float courtMargin = 1f;

    // 아무 방향도 누르지 않았을 때
    // 기본 깊이
    [Range(-1f, 1f)]
    public float defaultDepth = 0.2f;


    // =========================
    // 타격 범위 표시
    // =========================

    [Header("Hit Range Visual")]

    public LineRenderer lineRenderer;

    public int circleSegments = 50;


    // =========================
    // 다른 시스템
    // =========================

    private BallController ball;

    private CourtManager courtManager;


    // =========================
    // Unity 시작
    // =========================

    void Start()
    {
        ball =
            FindAnyObjectByType<BallController>();


        courtManager =
            FindAnyObjectByType<CourtManager>();


        if (ball == null)
        {
            Debug.LogError(
                "PlayerController: BallController를 찾을 수 없습니다."
            );
        }


        if (courtManager == null)
        {
            Debug.LogError(
                "PlayerController: CourtManager를 찾을 수 없습니다."
            );
        }


        DrawHitRange();
    }


    // =========================
    // 매 프레임
    // =========================

    void Update()
    {
        Move();

        HandleHitInput();
    }


    // =========================
    // 이동
    // =========================

    void Move()
    {
        if (Keyboard.current == null)
            return;


        float horizontal = 0f;
        float vertical = 0f;


        if (Keyboard.current.aKey.isPressed)
        {
            horizontal -= 1f;
        }


        if (Keyboard.current.dKey.isPressed)
        {
            horizontal += 1f;
        }


        if (Keyboard.current.wKey.isPressed)
        {
            vertical += 1f;
        }


        if (Keyboard.current.sKey.isPressed)
        {
            vertical -= 1f;
        }


        Vector3 direction =
            new Vector3(
                horizontal,
                0f,
                vertical
            );


        if (
            direction.sqrMagnitude
            > 1f
        )
        {
            direction.Normalize();
        }


        transform.position +=
            direction
            * moveSpeed
            * Time.deltaTime;


        Vector3 position =
            transform.position;


        position.x =
            Mathf.Clamp(
                position.x,
                -moveLimitX,
                moveLimitX
            );


        position.z =
            Mathf.Clamp(
                position.z,
                minZ,
                maxZ
            );


        transform.position =
            position;
    }


    // =========================
    // 타격 입력
    // =========================

    void HandleHitInput()
    {
        if (Keyboard.current == null)
            return;


        if (
            Keyboard.current.spaceKey
            .wasPressedThisFrame
        )
        {
            TryHit();
        }
    }


    // =========================
    // 타격
    // =========================

    void TryHit()
    {
        if (ball == null)
            return;


        if (courtManager == null)
            return;


        if (
            Time.time - lastHitTime
            < hitCooldown
        )
        {
            return;
        }


        float distance =
            Vector3.Distance(
                transform.position,
                ball.transform.position
            );


        if (distance > hitRange)
            return;


        // =========================
        // 타격 방향 입력
        // =========================

        float horizontalAim = 0f;

        float depthAim =
            defaultDepth;


        // A = 왼쪽
        if (
            Keyboard.current.aKey
            .isPressed
        )
        {
            horizontalAim = -1f;
        }


        // D = 오른쪽
        if (
            Keyboard.current.dKey
            .isPressed
        )
        {
            horizontalAim = 1f;
        }


        // W = 깊게
        if (
            Keyboard.current.wKey
            .isPressed
        )
        {
            depthAim = 1f;
        }


        // S = 짧게
        if (
            Keyboard.current.sKey
            .isPressed
        )
        {
            depthAim = -1f;
        }


        // =========================
        // Enemy 코트 안 목표점
        // =========================

        Vector3 targetPoint =
            courtManager
            .GetEnemyTargetPoint(
                horizontalAim,
                depthAim,
                courtMargin
            );


        // =========================
        // Ball 타격
        // =========================

        ball.HitToPoint(
            targetPoint,
            hitSpeed,
            hitHeight,
            "Player"
        );


        lastHitTime =
            Time.time;


        Debug.Log(
            "Player Hit! 목표 : "
            + targetPoint
        );
    }


    // =========================
    // 타격 범위 표시
    // =========================

    void DrawHitRange()
    {
        if (lineRenderer == null)
            return;


        lineRenderer.positionCount =
            circleSegments + 1;


        for (
            int i = 0;
            i <= circleSegments;
            i++
        )
        {
            float angle =
                (float)i
                / circleSegments
                * Mathf.PI
                * 2f;


            float x =
                Mathf.Cos(angle)
                * hitRange;


            float z =
                Mathf.Sin(angle)
                * hitRange;


            lineRenderer.SetPosition(
                i,
                new Vector3(
                    x,
                    0.05f,
                    z
                )
            );
        }
    }
}