using UnityEngine;

namespace TennisGame
{
    [CreateAssetMenu(menuName = "Tennis/Match Balance")]
    public sealed class TennisSettings : ScriptableObject
    {
        [Header("Movement / charge / stamina")]
        [Min(1)] public float playerSpeed = 7.5f;
        [Min(1)] public float enemySpeed = 6.3f;
        [Range(.1f, 1)] public float chargeMoveMultiplier = .6f;
        [Min(.1f)] public float chargeSeconds = .85f;
        [Min(1)] public float stamina = 100;
        [Min(0)] public float chargeDrain = 14;
        [Min(0)] public float sprintDrain = 24;
        [Min(0)] public float staminaRecovery = 20;
        [Header("Shot: horizontal speed / arc above straight path")]
        [Min(1)] public float shotSpeed = 15;
        [Min(1)] public float serveSpeed = 18;
        [Min(.1f)] public float shotArc = 2.2f;
        [Range(1, 2)] public float chargedPower = 1.3f;
        [Min(.1f)] public float hitReach = 2.1f;
        [Min(.1f)] public float maxHitHeight = 3.2f;
        [Min(.01f)] public float swingBuffer = .16f;
        [Range(0, 1)] public float bounceSpeed = .65f;
        [Min(.1f)] public float bounceArc = 1.25f;
        [Min(.1f)] public float bounceDistance = 4.2f;
        [Header("AI")]
        [Min(0)] public float reactionSeconds = .25f;
        [Range(0, 2)] public float aimError = .45f;
        [Header("Match (new match applies these values)")]
        [Range(1, 6)] public int gamesPerSet = 3;
        [Range(1, 3)] public int setsToWin = 1;
        [Min(.2f)] public float pointDelay = 1.6f;
    }
}
