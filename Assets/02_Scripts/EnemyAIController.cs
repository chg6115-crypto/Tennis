using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    // =========================
    // AI 상태
    // =========================

    private enum AIState
    {
        Returning,
        Tracking
    }


    private AIState currentState =
        AIState.Returning;


    // =========================
    // 이동
    // =========================

    [Header("Movement")]

    public float moveSpeed = 4f;

    public float moveLimitX = 8f;

    public Vector3 centerPosition =
        new Vector3(
            0f,
            1f,
            8f
        );


    // =========================
    // 타격
    // =========================

    [Header("Hit")]

    public float hitRange = 2f;

    public float hitSpeed = 10f;

    public float hitHeight = 4f;

    public float hitCooldown = 0.5f;

    private float lastHitTime =
        -999f;


    // =========================
    // AI 조준
    // =========================

    [Header("Aim")]

    // 코트 라인에서 안쪽으로
    // 얼마나 여유를 둘지
    public float courtMargin = 1f;


    // Player가 중앙이라고
    // 판단하는 범위
    public float playerCenterThreshold = 1f;


    // 기본적으로 얼마나 깊게 칠지
    [Range(-1f, 1f)]
    public float defaultDepth = 0.4f;


    // 타구 좌우 오차
    [Range(0f, 0.5f)]
    public float horizontalRandomness = 0.15f;


    // 타구 깊이 오차
    [Range(0f, 0.5f)]
    public float depthRandomness = 0.15f;


    // =========================
    // 다른 시스템
    // =========================

    private BallController ball;

    private CourtManager courtManager;

    private Transform player;


    // Player가 친 공의
    // 예상 착지점
    private Vector3 predictedPosition;


    // =========================
    // Unity 시작
    // =========================

    void Start()
    {
        ball =
            FindAnyObjectByType<BallController>();


        courtManager =
            FindAnyObjectByType<CourtManager>();


        PlayerController playerController =
            FindAnyObjectByType<PlayerController>();


        if (playerController != null)
        {
            player =
                playerController.transform;
        }
        else
        {
            Debug.LogError(
                "EnemyAIController: PlayerController를 찾을 수 없습니다."
            );
        }


        if (ball != null)
        {
            ball.OnBallHit +=
                HandleBallHit;
        }
        else
        {
            Debug.LogError(
                "EnemyAIController: BallController를 찾을 수 없습니다."
            );
        }


        if (courtManager == null)
        {
            Debug.LogError(
                "EnemyAIController: CourtManager를 찾을 수 없습니다."
            );
        }
    }


    void OnDestroy()
    {
        if (ball != null)
        {
            ball.OnBallHit -=
                HandleBallHit;
        }
    }


    // =========================
    // 매 프레임 AI
    // =========================

    void Update()
    {
        if (ball == null)
            return;


        switch (currentState)
        {
            case AIState.Returning:

                MoveToCenter();

                break;


            case AIState.Tracking:

                TrackLandingPoint();

                TryHit();

                break;
        }
    }


    // =========================
    // Player 타격 감지
    // =========================

    void HandleBallHit(
        Vector3 direction
    )
    {
        // Player → Enemy 방향
        if (
            direction.z > 0f
            && ball.lastHitter == "Player"
        )
        {
            predictedPosition =
                ball.TargetPoint;


            currentState =
                AIState.Tracking;


            Debug.Log(
                "Enemy 예상 착지점 : "
                + predictedPosition
            );
        }
    }


    // =========================
    // 중앙 복귀
    // =========================

    void MoveToCenter()
    {
        // Enemy는 베이스라인에서
        // 좌우로만 이동

        float newX =
            Mathf.MoveTowards(
                transform.position.x,
                centerPosition.x,
                moveSpeed
                * Time.deltaTime
            );


        transform.position =
            new Vector3(
                newX,
                transform.position.y,
                centerPosition.z
            );
    }


    // =========================
    // 예상 착지점 추적
    // =========================

    void TrackLandingPoint()
    {
        float targetX =
            Mathf.Clamp(
                predictedPosition.x,
                -moveLimitX,
                moveLimitX
            );


        float newX =
            Mathf.MoveTowards(
                transform.position.x,
                targetX,
                moveSpeed
                * Time.deltaTime
            );


        // Z는 항상 베이스라인 유지
        transform.position =
            new Vector3(
                newX,
                transform.position.y,
                centerPosition.z
            );
    }


    // =========================
    // 타격
    // =========================

    void TryHit()
    {
        if (courtManager == null)
            return;


        if (!ball.IsMoving)
        {
            currentState =
                AIState.Returning;

            return;
        }


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
        // Player 코트 목표점
        // =========================

        Vector3 targetPoint =
            CalculateShotTarget();


        // =========================
        // 타격
        // =========================

        ball.HitToPoint(
            targetPoint,
            hitSpeed,
            hitHeight,
            "Enemy"
        );


        lastHitTime =
            Time.time;


        currentState =
            AIState.Returning;


        Debug.Log(
            "Enemy Hit! 목표 : "
            + targetPoint
        );
    }


    // =========================
    // AI 목표점 계산
    // =========================

    Vector3 CalculateShotTarget()
    {
        float horizontalAim;


        // Player 정보를 못 찾은 경우
        if (player == null)
        {
            horizontalAim = 0f;
        }

        // Player가 왼쪽에 있음
        else if (
            player.position.x
            < -playerCenterThreshold
        )
        {
            // 반대쪽 오른쪽
            horizontalAim = 0.8f;
        }

        // Player가 오른쪽에 있음
        else if (
            player.position.x
            > playerCenterThreshold
        )
        {
            // 반대쪽 왼쪽
            horizontalAim = -0.8f;
        }

        // Player가 중앙
        else
        {
            // 좌 / 우 랜덤
            if (Random.value < 0.5f)
            {
                horizontalAim = -0.6f;
            }
            else
            {
                horizontalAim = 0.6f;
            }
        }


        // =========================
        // 작은 랜덤 오차
        // =========================

        horizontalAim +=
            Random.Range(
                -horizontalRandomness,
                horizontalRandomness
            );


        float depthAim =
            defaultDepth
            + Random.Range(
                -depthRandomness,
                depthRandomness
            );


        horizontalAim =
            Mathf.Clamp(
                horizontalAim,
                -1f,
                1f
            );


        depthAim =
            Mathf.Clamp(
                depthAim,
                -1f,
                1f
            );


        return
            courtManager
            .GetPlayerTargetPoint(
                horizontalAim,
                depthAim,
                courtMargin
            );
    }
}