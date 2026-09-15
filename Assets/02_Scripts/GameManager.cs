using UnityEngine;

public class GameManager : MonoBehaviour
{
    // =========================
    // 게임 상태
    // =========================

    public enum GameState
    {
        Ready,
        ServeReady,
        Serve,
        Rally,
        PointEnd
    }

    [SerializeField]
    private GameState currentState = GameState.Ready;

    // 다른 스크립트에서 현재 상태를 확인할 때 사용
    public GameState CurrentState
    {
        get { return currentState; }
    }


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
                "GameManager: BallController를 찾을 수 없습니다."
            );

            return;
        }


        if (courtManager == null)
        {
            Debug.LogError(
                "GameManager: CourtManager를 찾을 수 없습니다."
            );

            return;
        }


        // Ball 이벤트 등록
        ball.OnBallBounce +=
            HandleBallBounce;


        // =========================
        // 게임 시작
        // =========================

        ChangeState(
            GameState.Ready
        );
    }


    void OnDestroy()
    {
        if (ball != null)
        {
            ball.OnBallBounce -=
                HandleBallBounce;
        }
    }


    // =========================
    // 상태 변경
    // =========================

    public void ChangeState(
        GameState newState
    )
    {
        currentState =
            newState;


        Debug.Log(
            "Game State → "
            + currentState
        );
    }


    // =========================
    // 서브 준비
    // =========================

    public void StartServeReady()
    {
        ChangeState(
            GameState.ServeReady
        );
    }


    // =========================
    // 서브 시작
    // =========================

    public void StartServe()
    {
        ChangeState(
            GameState.Serve
        );
    }


    // =========================
    // 랠리 시작
    // =========================

    public void StartRally()
    {
        ChangeState(
            GameState.Rally
        );
    }


    // =========================
    // 바운드 처리
    // =========================

    void HandleBallBounce(
        Vector3 bouncePosition,
        int bounceCount
    )
    {
        Debug.Log(
            bounceCount
            + "번째 바운드 위치 : "
            + bouncePosition
        );


        // 첫 번째 바운드
        if (bounceCount == 1)
        {
            CheckFirstBounce(
                bouncePosition
            );

            return;
        }


        // 두 번째 바운드
        if (bounceCount == 2)
        {
            HandleSecondBounce();
        }
    }


    // =========================
    // 첫 바운드 IN / OUT
    // =========================

    void CheckFirstBounce(
        Vector3 bouncePosition
    )
    {
        // =========================
        // Player가 친 공
        // =========================

        if (ball.lastHitter == "Player")
        {
            bool isIn =
                courtManager.IsInsideEnemyArea(
                    bouncePosition
                );


            if (isIn)
            {
                Debug.Log(
                    "Player Shot → IN"
                );

                // 정상적으로 들어온 공이면
                // 랠리 상태로 전환
                if (
                    currentState == GameState.Serve
                    || currentState == GameState.ServeReady
                )
                {
                    ChangeState(
                        GameState.Rally
                    );
                }
            }
            else
            {
                Debug.Log(
                    "Player Shot → OUT"
                );


                EndPoint(
                    "Enemy",
                    "Player가 OUT"
                );
            }


            return;
        }


        // =========================
        // Enemy가 친 공
        // =========================

        if (ball.lastHitter == "Enemy")
        {
            bool isIn =
                courtManager.IsInsidePlayerArea(
                    bouncePosition
                );


            if (isIn)
            {
                Debug.Log(
                    "Enemy Shot → IN"
                );
            }
            else
            {
                Debug.Log(
                    "Enemy Shot → OUT"
                );


                EndPoint(
                    "Player",
                    "Enemy가 OUT"
                );
            }


            return;
        }


        Debug.LogWarning(
            "GameManager: 마지막 타격자를 알 수 없습니다."
        );
    }


    // =========================
    // 두 번째 바운드
    // =========================

    void HandleSecondBounce()
    {
        // Player가 마지막으로 친 공을
        // Enemy가 받아치지 못함
        if (ball.lastHitter == "Player")
        {
            EndPoint(
                "Player",
                "Enemy가 공을 받아치지 못함"
            );
        }

        // Enemy가 마지막으로 친 공을
        // Player가 받아치지 못함
        else if (ball.lastHitter == "Enemy")
        {
            EndPoint(
                "Enemy",
                "Player가 공을 받아치지 못함"
            );
        }
    }


    // =========================
    // 포인트 종료
    // =========================

    void EndPoint(
        string winner,
        string reason
    )
    {
        // 이미 포인트가 끝났다면
        // 중복 처리 방지
        if (
            currentState
            == GameState.PointEnd
        )
        {
            return;
        }


        ChangeState(
            GameState.PointEnd
        );


        // 공 정지
        if (ball != null)
        {
            ball.StopBall();
        }


        Debug.Log(
            "===================="
        );

        Debug.Log(
            "POINT END"
        );

        Debug.Log(
            "Winner : "
            + winner
        );

        Debug.Log(
            "Reason : "
            + reason
        );

        Debug.Log(
            "===================="
        );
    }
}