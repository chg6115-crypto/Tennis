using UnityEngine;
using System;

public class BallSpawner : MonoBehaviour
{
    public event Action<BallController> OnBallSpawned;

    [Header("Ball Settings")]
    [SerializeField]
    private BallController ballPrefab;

    [Header("Player Serve Position")]
    [SerializeField]
    private Transform playerServeLeft;

    [SerializeField]
    private Transform playerServeRight;

    [Header("Enemy Serve Position")]
    [SerializeField]
    private Transform enemyServeLeft;

    [SerializeField]
    private Transform enemyServeRight;

    private BallController currentBall;

    public BallController CurrentBall
    {
        get { return currentBall; }
    }

    void Start()
    {
        // 첫 서브는 Player 오른쪽에서 시작
        SpawnPlayerBall(true);
    }

    public void SpawnPlayerBall(bool serveFromRight)
    {
        Transform spawnPosition =
            serveFromRight ? playerServeRight : playerServeLeft;

        SpawnBall(spawnPosition);
    }

    public void SpawnEnemyBall(bool serveFromRight)
    {
        Transform spawnPosition =
            serveFromRight ? enemyServeRight : enemyServeLeft;

        SpawnBall(spawnPosition);
    }

    private void SpawnBall(Transform spawnPosition)
    {
        if (ballPrefab == null)
        {
            Debug.LogError(
                "BallSpawner: Ball Prefab이 연결되어 있지 않습니다."
            );
            return;
        }

        if (spawnPosition == null)
        {
            Debug.LogError(
                "BallSpawner: Serve Position이 연결되어 있지 않습니다."
            );
            return;
        }

        RemoveBall();

        currentBall = Instantiate(
            ballPrefab,
            spawnPosition.position,
            Quaternion.identity
        );

        Debug.Log(
            "Ball Spawned : " + currentBall.transform.position
        );

        OnBallSpawned?.Invoke(currentBall);
    }

    public void RemoveBall()
    {
        if (currentBall == null)
            return;

        Destroy(currentBall.gameObject);
        currentBall = null;

        Debug.Log("Ball Removed");
    }
}
