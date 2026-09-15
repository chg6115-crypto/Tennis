using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float moveLimitX = 8f;
    public float minZ = -9f;
    public float maxZ = -1f;

    [Header("Hit")]
    public float hitRange = 2f;
    public float hitSpeed = 10f;
    public float hitHeight = 4f;
    public float hitCooldown = 0.3f;
    private float lastHitTime = -999f;

    [Header("Aim")]
    public float courtMargin = 1f;
    [Range(-1f, 1f)] public float defaultDepth = 0.2f;

    [Header("Serve")]
    public float serveSpeed = 12f;
    public float serveHeight = 5f;

    // 서비스 박스 중앙을 기준으로 실제 착지점을 얼마나 움직일지 결정
    public float serveAimOffsetX = 0f;
    public float serveAimOffsetZ = 0f;

    [Header("Hit Range Visual")]
    public LineRenderer lineRenderer;
    public int circleSegments = 50;

    private BallController ball;
    private BallSpawner ballSpawner;
    private CourtManager courtManager;
    private GameManager gameManager;

    void Start()
    {
        courtManager = FindAnyObjectByType<CourtManager>();
        ballSpawner = FindAnyObjectByType<BallSpawner>();
        gameManager = FindAnyObjectByType<GameManager>();

        if (courtManager == null)
            Debug.LogError("PlayerController: CourtManager를 찾을 수 없습니다.");

        if (gameManager == null)
            Debug.LogError("PlayerController: GameManager를 찾을 수 없습니다.");

        if (ballSpawner == null)
        {
            Debug.LogError("PlayerController: BallSpawner를 찾을 수 없습니다.");
        }
        else
        {
            ballSpawner.OnBallSpawned += HandleBallSpawned;

            if (ballSpawner.CurrentBall != null)
                HandleBallSpawned(ballSpawner.CurrentBall);
        }

        DrawHitRange();
    }

    void OnDestroy()
    {
        if (ballSpawner != null)
            ballSpawner.OnBallSpawned -= HandleBallSpawned;
    }

    void HandleBallSpawned(BallController newBall)
    {
        ball = newBall;
        Debug.Log("PlayerController: 새 Ball 연결 완료");
    }

    void Update()
    {
        Move();
        HandleHitInput();
    }

    void Move()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -moveLimitX, moveLimitX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        transform.position = position;
    }

    void HandleHitInput()
    {
        if (Keyboard.current == null || gameManager == null)
            return;

        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        if (gameManager.CurrentState == GameManager.GameState.ServeReady)
        {
            TryServe();
            return;
        }

        if (gameManager.CurrentState == GameManager.GameState.Serve ||
            gameManager.CurrentState == GameManager.GameState.Rally)
        {
            TryHit();
        }
    }

    void TryServe()
    {
        if (ball == null || courtManager == null || gameManager == null)
            return;

        bool targetLeft = gameManager.ServeFromRight;

        // 먼저 정상적인 서비스 박스 중앙을 얻는다.
        Vector3 targetPoint =
            courtManager.GetEnemyServeTargetPoint(
                targetLeft,
                0f,
                0f,
                0f
            );

        // 그 후 실제 공의 착지점에 별도의 오차를 더한다.
        // 따라서 ServiceArea를 움직이지 않고도 Fault 테스트가 가능하다.
        targetPoint.x += serveAimOffsetX;
        targetPoint.z += serveAimOffsetZ;

        gameManager.StartServe();

        ball.HitToPoint(
            targetPoint,
            serveSpeed,
            serveHeight,
            "Player"
        );

        lastHitTime = Time.time;

        Debug.Log(
            "Player Serve 목표 : " + targetPoint
            + " / Offset X : " + serveAimOffsetX
            + " / Offset Z : " + serveAimOffsetZ
        );
    }

    void TryHit()
    {
        if (ball == null || courtManager == null)
            return;

        if (Time.time - lastHitTime < hitCooldown)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                ball.transform.position
            );

        if (distance > hitRange)
            return;

        float horizontalAim = 0f;
        float depthAim = defaultDepth;

        if (Keyboard.current.aKey.isPressed) horizontalAim = -1f;
        if (Keyboard.current.dKey.isPressed) horizontalAim = 1f;
        if (Keyboard.current.wKey.isPressed) depthAim = 1f;
        if (Keyboard.current.sKey.isPressed) depthAim = -1f;

        Vector3 targetPoint =
            courtManager.GetEnemyTargetPoint(
                horizontalAim,
                depthAim,
                courtMargin
            );

        ball.HitToPoint(
            targetPoint,
            hitSpeed,
            hitHeight,
            "Player"
        );

        lastHitTime = Time.time;

        Debug.Log(
            "Player Hit! 목표 : " + targetPoint
        );
    }

    void DrawHitRange()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = circleSegments + 1;

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle =
                (float)i / circleSegments
                * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * hitRange;
            float z = Mathf.Sin(angle) * hitRange;

            lineRenderer.SetPosition(
                i,
                new Vector3(x, 0.05f, z)
            );
        }
    }
}
