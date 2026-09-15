using UnityEngine;

public class CameraController : MonoBehaviour
{
    // =========================
    // 추적 대상
    // =========================

    public Transform player;


    // =========================
    // 화면 Y 제한
    // =========================

    // Player가 화면 아래 5%보다 내려가지 않도록 함
    [Range(0f, 1f)]
    public float minViewportY = 0.05f;

    // Player가 화면 아래 20%보다 올라가지 않도록 함
    [Range(0f, 1f)]
    public float maxViewportY = 0.20f;


    // =========================
    // 앞뒤 카메라 추적
    // =========================

    // 5~20% 사이에서
    // Player를 얼마나 부드럽게 따라갈지
    public float zFollowAmount = 0.3f;

    // Z축 부드러운 이동 속도
    public float zFollowSpeed = 5f;


    // =========================
    // 좌우 카메라 변화
    // =========================

    // PlayerController의 moveLimitX와 동일하게
    public float playerMoveLimitX = 8f;

    // 좌우 끝으로 이동했을 때
    // Camera가 얼마나 좌우로 이동할지
    public float sideMoveAmount = 2f;

    // 좌우 위치에 따른 최대 Camera 회전 각도
    public float maxYawAngle = 8f;


    // =========================
    // 카메라 부드러움
    // =========================

    // 좌우 이동 속도
    public float sideFollowSpeed = 5f;

    // 회전 속도
    public float rotateSpeed = 5f;


    // =========================
    // 내부 변수
    // =========================

    private Camera cam;

    // 게임 시작 시 Camera 위치
    private Vector3 startPosition;

    // 게임 시작 시 Camera 회전
    private Quaternion startRotation;

    // Camera의 현재 목표 Z 위치
    private float targetCameraZ;

    // 지난 프레임 Player Z
    private float previousPlayerZ;


    void Start()
    {
        cam = GetComponent<Camera>();

        // 시작 Camera 위치 저장
        startPosition = transform.position;

        // 시작 Camera 각도 저장
        startRotation = transform.rotation;

        // 시작 Camera Z 저장
        targetCameraZ = transform.position.z;

        // 시작 Player Z 저장
        previousPlayerZ = player.position.z;
    }


    void LateUpdate()
    {
        if (player == null)
            return;


        // =========================
        // 1. Player 좌우 위치 비율
        // =========================

        // 왼쪽 끝  = -1
        // 중앙      =  0
        // 오른쪽 끝 = +1

        float horizontalRatio =
            Mathf.Clamp(
                player.position.x / playerMoveLimitX,
                -1f,
                1f
            );


        // =========================
        // 2. Player 화면 위치 확인
        // =========================

        Vector3 viewportPosition =
            cam.WorldToViewportPoint(player.position);


        // =========================
        // 3. Player Z 이동량 계산
        // =========================

        float playerZMovement =
            player.position.z - previousPlayerZ;


        // =========================
        // 4. 앞뒤 Camera 추적
        // =========================


        // -------------------------
        // 5% ~ 20% 사이
        // -------------------------

        if (viewportPosition.y >= minViewportY &&
            viewportPosition.y <= maxViewportY)
        {
            // Player 이동량의 일부만 따라감
            //
            // 예:
            // Player가 1만큼 이동
            // zFollowAmount = 0.3
            //
            // Camera는 0.3만큼 이동
            //
            // 따라서 Player도 화면에서 움직이고
            // Camera도 천천히 따라옴

            targetCameraZ +=
                playerZMovement * zFollowAmount;
        }


        // -------------------------
        // 20% 위로 넘어감
        // -------------------------

        else if (viewportPosition.y > maxViewportY)
        {
            // Player가 움직인 만큼
            // Camera가 즉시 따라감

            targetCameraZ += playerZMovement;
        }


        // -------------------------
        // 5% 아래로 내려감
        // -------------------------

        else if (viewportPosition.y < minViewportY)
        {
            // Player가 움직인 만큼
            // Camera가 즉시 따라감

            targetCameraZ += playerZMovement;
        }


        // =========================
        // 5. Camera 목표 위치
        // =========================

        Vector3 targetPosition =
            transform.position;


        // -------------------------
        // X축
        // -------------------------

        targetPosition.x =
            startPosition.x
            + horizontalRatio * sideMoveAmount;


        // -------------------------
        // Y축
        // -------------------------

        // 높이는 처음 값 유지
        targetPosition.y =
            startPosition.y;


        // -------------------------
        // Z축
        // -------------------------

        targetPosition.z =
            targetCameraZ;


        // =========================
        // 6. Camera 위치 적용
        // =========================

        Vector3 currentPosition =
            transform.position;


        // -------------------------
        // X축은 부드럽게
        // -------------------------

        currentPosition.x =
            Mathf.Lerp(
                currentPosition.x,
                targetPosition.x,
                sideFollowSpeed * Time.deltaTime
            );


        // -------------------------
        // Y축 고정
        // -------------------------

        currentPosition.y =
            targetPosition.y;


        // -------------------------
        // Z축
        // -------------------------

        // 5~20% 안에 있을 때는
        // 부드럽게 따라감

        if (viewportPosition.y >= minViewportY &&
            viewportPosition.y <= maxViewportY)
        {
            currentPosition.z =
                Mathf.Lerp(
                    currentPosition.z,
                    targetPosition.z,
                    zFollowSpeed * Time.deltaTime
                );
        }

        // 영역 밖이면 즉시 따라감
        else
        {
            currentPosition.z =
                targetPosition.z;
        }


        transform.position =
            currentPosition;


        // =========================
        // 7. 좌우 Camera 회전
        // =========================

        float yawAngle =
            horizontalRatio * maxYawAngle;


        // 시작 Pitch는 유지하고
        // Yaw만 변화시킴
        Quaternion targetRotation =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y + yawAngle,
                startRotation.eulerAngles.z
            );


        // =========================
        // 8. 부드럽게 회전
        // =========================

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );


        // =========================
        // 9. 현재 Player Z 저장
        // =========================

        previousPlayerZ =
            player.position.z;
    }
}