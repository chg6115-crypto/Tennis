using UnityEngine;
using System;

public class BallController : MonoBehaviour
{
    // =========================
    // 이벤트
    // =========================

    public event Action<Vector3> OnBallHit;

    public event Action<Vector3, int> OnBallBounce;


    // =========================
    // 이동 상태
    // =========================

    private bool isMoving = false;

    private Vector3 startPoint;
    private Vector3 controlPoint;
    private Vector3 targetPoint;

    private float moveTime = 0f;
    private float travelTime = 1f;


    // =========================
    // 타구 설정
    // =========================

    [Header("Shot")]

    [Range(0.2f, 0.8f)]
    public float apexPosition = 0.4f;

    [Range(0.1f, 2f)]
    public float trajectoryHeightMultiplier = 0.6f;


    // =========================
    // 바운드 설정
    // =========================

    [Header("Bounce")]

    public int bounceCount = 0;

    [Range(0f, 1f)]
    public float bounceDistanceRate = 0.45f;

    [Range(0f, 1f)]
    public float bounceHeightRate = 0.35f;

    [Range(0f, 1f)]
    public float bounceSpeedRate = 0.7f;


    private bool isBouncePath = false;


    // =========================
    // 마지막 타격자
    // =========================

    public string lastHitter = "";


    // =========================
    // 외부 접근
    // =========================

    public Vector3 TargetPoint
    {
        get
        {
            return targetPoint;
        }
    }


    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
    }


    // =========================
    // Unity 시작
    // =========================

    void Start()
    {
        Rigidbody rb =
            GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }


    void Update()
    {
        if (!isMoving)
            return;


        MoveBall();
    }


    // =========================
    // 목표 지점 타격
    // =========================

    public void HitToPoint(
        Vector3 landingPoint,
        float hitSpeed,
        float trajectoryHeight,
        string hitter
    )
    {
        Vector3 start =
            transform.position;


        // 코트 표면
        landingPoint.y = 0f;


        Vector3 direction =
            landingPoint
            - start;


        direction.y = 0f;


        if (
            direction.sqrMagnitude
            <= 0.001f
        )
        {
            return;
        }


        direction.Normalize();


        lastHitter =
            hitter;


        bounceCount = 0;

        isBouncePath = false;


        StartShot(
            start,
            landingPoint,
            trajectoryHeight,
            hitSpeed,
            false
        );


        // Enemy AI가
        // Player 타구를 인식하는 이벤트
        OnBallHit?.Invoke(
            direction
        );
    }


    // =========================
    // 실제 궤적 시작
    // =========================

    void StartShot(
        Vector3 start,
        Vector3 target,
        float height,
        float speed,
        bool bounceShot
    )
    {
        startPoint =
            start;


        targetPoint =
            target;


        float controlPosition;


        if (!bounceShot)
        {
            controlPosition =
                apexPosition;
        }
        else
        {
            controlPosition =
                0.5f;
        }


        Vector3 middle =
            Vector3.Lerp(
                startPoint,
                targetPoint,
                controlPosition
            );


        float finalHeight;


        if (!bounceShot)
        {
            finalHeight =
                height
                * trajectoryHeightMultiplier;
        }
        else
        {
            finalHeight =
                height;
        }


        controlPoint =
            middle
            + Vector3.up
            * finalHeight;


        // =========================
        // 이동 시간
        // =========================

        Vector3 flatStart =
            new Vector3(
                startPoint.x,
                0f,
                startPoint.z
            );


        Vector3 flatTarget =
            new Vector3(
                targetPoint.x,
                0f,
                targetPoint.z
            );


        float distance =
            Vector3.Distance(
                flatStart,
                flatTarget
            );


        speed =
            Mathf.Max(
                speed,
                0.01f
            );


        travelTime =
            distance / speed;


        travelTime =
            Mathf.Max(
                travelTime,
                0.1f
            );


        moveTime = 0f;

        isMoving = true;
    }


    // =========================
    // 공 이동
    // =========================

    void MoveBall()
    {
        moveTime +=
            Time.deltaTime;


        float t =
            moveTime / travelTime;


        if (t < 1f)
        {
            transform.position =
                GetBezierPoint(
                    startPoint,
                    controlPoint,
                    targetPoint,
                    t
                );


            return;
        }


        transform.position =
            targetPoint;


        if (!isBouncePath)
        {
            FirstBounce();
        }
        else
        {
            FinishBounce();
        }
    }


    // =========================
    // 첫 바운드
    // =========================

    void FirstBounce()
    {
        bounceCount = 1;


        Debug.Log(
            "Ball Bounce : "
            + bounceCount
        );


        OnBallBounce?.Invoke(
            targetPoint,
            bounceCount
        );


        // OUT 등으로 GameManager가
        // 공을 정지시킨 경우
        if (!isMoving)
            return;


        Vector3 direction =
            targetPoint
            - startPoint;


        direction.y = 0f;


        if (
            direction.sqrMagnitude
            <= 0.001f
        )
        {
            isMoving = false;

            return;
        }


        direction.Normalize();


        Vector3 flatStart =
            new Vector3(
                startPoint.x,
                0f,
                startPoint.z
            );


        Vector3 flatTarget =
            new Vector3(
                targetPoint.x,
                0f,
                targetPoint.z
            );


        float originalDistance =
            Vector3.Distance(
                flatStart,
                flatTarget
            );


        float bounceDistance =
            originalDistance
            * bounceDistanceRate;


        Vector3 bounceTarget =
            targetPoint
            + direction
            * bounceDistance;


        bounceTarget.y = 0f;


        float originalHeight =
            Mathf.Max(
                controlPoint.y
                - targetPoint.y,
                0.1f
            );


        float newHeight =
            originalHeight
            * bounceHeightRate;


        float originalSpeed =
            originalDistance
            / travelTime;


        float bounceSpeed =
            originalSpeed
            * bounceSpeedRate;


        isBouncePath = true;


        StartShot(
            targetPoint,
            bounceTarget,
            newHeight,
            bounceSpeed,
            true
        );
    }


    // =========================
    // 두 번째 바운드
    // =========================

    void FinishBounce()
    {
        bounceCount = 2;


        Debug.Log(
            "Ball Bounce : "
            + bounceCount
        );


        OnBallBounce?.Invoke(
            targetPoint,
            bounceCount
        );


        isMoving = false;
    }


    // =========================
    // 공 정지
    // =========================

    public void StopBall()
    {
        isMoving = false;
    }


    // =========================
    // 2차 베지어 곡선
    // =========================

    Vector3 GetBezierPoint(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        float t
    )
    {
        Vector3 a =
            Vector3.Lerp(
                p0,
                p1,
                t
            );


        Vector3 b =
            Vector3.Lerp(
                p1,
                p2,
                t
            );


        return Vector3.Lerp(
            a,
            b,
            t
        );
    }
}