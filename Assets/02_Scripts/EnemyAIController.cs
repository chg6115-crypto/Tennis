using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private enum AIState
    {
        Returning,
        Tracking
    }

    private AIState currentState = AIState.Returning;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float moveLimitX = 8f;
    public Vector3 centerPosition =
        new Vector3(0f, 1f, 8f);

    [Header("Hit")]
    public float hitRange = 2f;
    public float hitSpeed = 10f;
    public float hitHeight = 4f;
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    [Header("Aim")]
    public float courtMargin = 1f;
    public float playerCenterThreshold = 1f;

    [Range(-1f, 1f)]
    public float defaultDepth = 0.4f;

    [Range(0f, 0.5f)]
    public float horizontalRandomness = 0.15f;

    [Range(0f, 0.5f)]
    public float depthRandomness = 0.15f;

    private BallController ball;
    private BallSpawner ballSpawner;
    private CourtManager courtManager;
    private Transform player;

    private Vector3 predictedPosition;

    void Start()
    {
        courtManager =
            FindAnyObjectByType<CourtManager>();

        ballSpawner =
            FindAnyObjectByType<BallSpawner>();

        PlayerController playerController =
            FindAnyObjectByType<PlayerController>();

        if (playerController != null)
        {
            player = playerController.transform;
        }
        else
        {
            Debug.LogError(
                "EnemyAIController: PlayerController를 찾을 수 없습니다."
            );
        }

        if (courtManager == null)
        {
            Debug.LogError(
                "EnemyAIController: CourtManager를 찾을 수 없습니다."
            );
        }

        if (ballSpawner == null)
        {
            Debug.LogError(
                "EnemyAIController: BallSpawner를 찾을 수 없습니다."
            );
        }
        else
        {
            // 앞으로 새 Ball이 생성될 때 알림 받기
            ballSpawner.OnBallSpawned += HandleBallSpawned;

            // 이미 생성되어 있다면 바로 연결
            if (ballSpawner.CurrentBall != null)
            {
                HandleBallSpawned(
                    ballSpawner.CurrentBall
                );
            }
        }
    }

    void OnDestroy()
    {
        if (ballSpawner != null)
        {
            ballSpawner.OnBallSpawned -=
                HandleBallSpawned;
        }

        if (ball != null)
        {
            ball.OnBallHit -= HandleBallHit;
        }
    }

    void HandleBallSpawned(
        BallController newBall
    )
    {
        // 기존 Ball 이벤트 해제
        if (ball != null)
        {
            ball.OnBallHit -= HandleBallHit;
        }

        // 새 Ball 저장
        ball = newBall;

        // 새 Ball 이벤트 연결
        if (ball != null)
        {
            ball.OnBallHit += HandleBallHit;
        }

        currentState = AIState.Returning;

        Debug.Log(
            "EnemyAIController: 새 Ball 연결 완료"
        );
    }

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

    void HandleBallHit(Vector3 direction)
    {
        if (
            direction.z > 0f &&
            ball.lastHitter == "Player"
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

    void MoveToCenter()
    {
        float newX =
            Mathf.MoveTowards(
                transform.position.x,
                centerPosition.x,
                moveSpeed * Time.deltaTime
            );

        transform.position =
            new Vector3(
                newX,
                transform.position.y,
                centerPosition.z
            );
    }

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
                moveSpeed * Time.deltaTime
            );

        transform.position =
            new Vector3(
                newX,
                transform.position.y,
                centerPosition.z
            );
    }

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

        Vector3 targetPoint =
            CalculateShotTarget();

        ball.HitToPoint(
            targetPoint,
            hitSpeed,
            hitHeight,
            "Enemy"
        );

        lastHitTime = Time.time;

        currentState =
            AIState.Returning;

        Debug.Log(
            "Enemy Hit! 목표 : "
            + targetPoint
        );
    }

    Vector3 CalculateShotTarget()
    {
        float horizontalAim;

        if (player == null)
        {
            horizontalAim = 0f;
        }
        else if (
            player.position.x
            < -playerCenterThreshold
        )
        {
            horizontalAim = 0.8f;
        }
        else if (
            player.position.x
            > playerCenterThreshold
        )
        {
            horizontalAim = -0.8f;
        }
        else
        {
            horizontalAim =
                Random.value < 0.5f
                ? -0.6f
                : 0.6f;
        }

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
            courtManager.GetPlayerTargetPoint(
                horizontalAim,
                depthAim,
                courtMargin
            );
    }
}