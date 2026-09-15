using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
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

    [Header("Point")]
    [SerializeField]
    private float nextPointDelay = 1.5f;

    [Header("Serve")]
    [SerializeField]
    private bool serveFromRight = true;

    public GameState CurrentState
    {
        get { return currentState; }
    }

    public bool ServeFromRight
    {
        get { return serveFromRight; }
    }

    private BallController ball;
    private BallSpawner ballSpawner;
    private CourtManager courtManager;

    void Start()
    {
        courtManager = FindAnyObjectByType<CourtManager>();
        ballSpawner = FindAnyObjectByType<BallSpawner>();

        if (courtManager == null)
        {
            Debug.LogError(
                "GameManager: CourtManager를 찾을 수 없습니다."
            );
        }

        if (ballSpawner == null)
        {
            Debug.LogError(
                "GameManager: BallSpawner를 찾을 수 없습니다."
            );
            return;
        }

        ballSpawner.OnBallSpawned += HandleBallSpawned;

        if (ballSpawner.CurrentBall != null)
        {
            HandleBallSpawned(ballSpawner.CurrentBall);
        }

        // 첫 포인트는 무조건 Player 오른쪽에서 서브
        serveFromRight = true;

        ChangeState(GameState.Ready);
        StartCoroutine(StartFirstPoint());
    }

    void OnDestroy()
    {
        if (ballSpawner != null)
        {
            ballSpawner.OnBallSpawned -= HandleBallSpawned;
        }

        if (ball != null)
        {
            ball.OnBallBounce -= HandleBallBounce;
        }
    }

    IEnumerator StartFirstPoint()
    {
        yield return null;

        // BallSpawner의 Start에서 첫 공을 오른쪽에 생성한다.
        ChangeState(GameState.ServeReady);

        DebugServeSide();
    }

    void HandleBallSpawned(BallController newBall)
    {
        if (ball != null)
        {
            ball.OnBallBounce -= HandleBallBounce;
        }

        ball = newBall;

        if (ball != null)
        {
            ball.OnBallBounce += HandleBallBounce;
        }

        Debug.Log("GameManager: 새 Ball 연결 완료");
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Debug.Log(
            "Game State → " + currentState
        );
    }

    public void StartServe()
    {
        if (currentState != GameState.ServeReady)
            return;

        ChangeState(GameState.Serve);
    }

    public void StartRally()
    {
        ChangeState(GameState.Rally);
    }

    void HandleBallBounce(
        Vector3 bouncePosition,
        int bounceCount
    )
    {
        if (currentState == GameState.PointEnd)
            return;

        Debug.Log(
            bounceCount
            + "번째 바운드 위치 : "
            + bouncePosition
        );

        if (bounceCount == 1)
        {
            CheckFirstBounce(bouncePosition);
            return;
        }

        if (bounceCount == 2)
        {
            HandleSecondBounce();
        }
    }

    void CheckFirstBounce(Vector3 bouncePosition)
    {
        if (ball == null || courtManager == null)
            return;

        if (ball.lastHitter == "Player")
        {
            // 서브 중에는 일반 EnemyArea가 아니라
            // 대각선 반대편 ServiceArea를 검사한다.
            if (currentState == GameState.Serve)
            {
                bool serveIn;

                if (serveFromRight)
                {
                    serveIn =
                        courtManager.IsInsideEnemyServeAreaLeft(
                            bouncePosition
                        );
                }
                else
                {
                    serveIn =
                        courtManager.IsInsideEnemyServeAreaRight(
                            bouncePosition
                        );
                }

                if (serveIn)
                {
                    Debug.Log("Player Serve → IN");
                    ChangeState(GameState.Rally);
                }
                else
                {
                    Debug.Log("Player Serve → OUT");

                    EndPoint(
                        "Enemy",
                        "Player 서브가 서비스 박스를 벗어남"
                    );
                }

                return;
            }

            bool isIn =
                courtManager.IsInsideEnemyArea(
                    bouncePosition
                );

            if (isIn)
            {
                Debug.Log("Player Shot → IN");
            }
            else
            {
                Debug.Log("Player Shot → OUT");

                EndPoint(
                    "Enemy",
                    "Player가 OUT"
                );
            }

            return;
        }

        if (ball.lastHitter == "Enemy")
        {
            bool isIn =
                courtManager.IsInsidePlayerArea(
                    bouncePosition
                );

            if (isIn)
            {
                Debug.Log("Enemy Shot → IN");
            }
            else
            {
                Debug.Log("Enemy Shot → OUT");

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

    void HandleSecondBounce()
    {
        if (ball == null)
            return;

        if (ball.lastHitter == "Player")
        {
            EndPoint(
                "Player",
                "Enemy가 공을 받아치지 못함"
            );
        }
        else if (ball.lastHitter == "Enemy")
        {
            EndPoint(
                "Enemy",
                "Player가 공을 받아치지 못함"
            );
        }
    }

    void EndPoint(string winner, string reason)
    {
        if (currentState == GameState.PointEnd)
            return;

        ChangeState(GameState.PointEnd);

        if (ball != null)
        {
            ball.StopBall();
        }

        Debug.Log("====================");
        Debug.Log("POINT END");
        Debug.Log("Winner : " + winner);
        Debug.Log("Reason : " + reason);
        Debug.Log("====================");

        StartCoroutine(PrepareNextPoint());
    }

    IEnumerator PrepareNextPoint()
    {
        yield return new WaitForSeconds(
            nextPointDelay
        );

        if (ballSpawner == null)
            yield break;

        // 포인트가 끝날 때마다 서브 위치를 반대로 바꾼다.
        serveFromRight = !serveFromRight;

        // 현재는 Player가 계속 서브한다고 가정한다.
        ballSpawner.SpawnPlayerBall(
            serveFromRight
        );

        yield return null;

        ChangeState(GameState.ServeReady);

        DebugServeSide();
    }

    void DebugServeSide()
    {
        if (serveFromRight)
        {
            Debug.Log(
                "다음 서브 : Player Right → Enemy Left"
            );
        }
        else
        {
            Debug.Log(
                "다음 서브 : Player Left → Enemy Right"
            );
        }
    }
}
