using UnityEngine;
using UnityEngine.InputSystem;

namespace TennisGame
{
    public sealed class TennisMatch : MonoBehaviour
    {
        public TennisSettings balance;
        public TennisCourt court;
        public TennisActor player;
        public TennisActor enemy;
        public TennisBall ball;
        public Transform landingMarker;
        public Transform aimMarker;
        public enum Phase { ServeReady, Serve, Rally, PointEnd, MatchEnd }
        public Phase State { get; private set; }
        public TennisScore Score { get; private set; }
        public string Message { get; private set; }
        public float Charge => charge / Mathf.Max(.1f, balance.chargeSeconds);
        public bool Paused { get; private set; }
        public int Faults { get; private set; }
        float charge, clock, phaseAt, reactAt, swingUntil = -1, swingCharge;
        bool charging;
        Vector2 aim, swingAim;
        int shotType, swingType;

        void OnEnable()
        {
            if (!balance || !court || !player || !enemy || !ball)
            {
                Debug.LogError("TennisMatch: assign Balance, Court, Player, Enemy and Ball.", this);
                enabled = false;
                return;
            }
            ball.Bounced += OnBounce;
            ball.NetHit += OnNet;
            Restart();
        }
        void OnDisable()
        {
            if (!ball) return;
            ball.Bounced -= OnBounce;
            ball.NetHit -= OnNet;
        }
        public void Restart()
        {
            Score = new TennisScore(balance.gamesPerSet, balance.setsToWin);
            Paused = false;
            Faults = 0;
            clock = 0;
            Ready();
        }
        void Ready()
        {
            ball.Stop();
            State = Phase.ServeReady;
            phaseAt = clock;
            float sign = Score.Server == 0 ? -1 : 1;
            float x = (Score.ServeRight ? 1 : -1) * -sign * 2.1f;
            TennisActor server = Score.Server == 0 ? player : enemy;
            TennisActor receiver = Score.Server == 0 ? enemy : player;
            server.transform.position = court.World(new Vector3(x, 0, sign * (court.halfLength + .6f)));
            receiver.transform.position = court.World(new Vector3(-x * .5f, 0, -sign * (court.halfLength - 1)));
            player.ResetEnergy(balance.stamina);
            enemy.ResetEnergy(balance.stamina);
            ball.transform.position = ServePosition(server);
            charge = 0;
            charging = false;
            swingUntil = -1;
            aim = Vector2.zero;
            Message = Score.Server == 0 ? "YOUR SERVE - hold SPACE, release to serve" : "ENEMY SERVE";
            if (Faults > 0) Message = "SECOND SERVE - " + Message;
        }
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Paused = !Paused;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) { Restart(); return; }
            if (Paused) return;
            float dt = Mathf.Min(Time.deltaTime, .1f);
            clock += dt;
            if (State == Phase.PointEnd)
            {
                if (clock - phaseAt >= balance.pointDelay) Ready();
                return;
            }
            if (State == Phase.MatchEnd) return;
            InputPlayer(dt);
            if (State == Phase.ServeReady)
            {
                TennisActor server = Score.Server == 0 ? player : enemy;
                ball.transform.position = ServePosition(server);
                if (Score.Server == 1 && clock - phaseAt > 1.2f) Shoot(1, .5f, 0, Vector2.zero, true);
            }
            else
            {
                ball.Step(dt, court, balance);
                if (State == Phase.Serve || State == Phase.Rally)
                {
                    if (clock <= swingUntil && CanHit(player))
                    {
                        Shoot(0, swingCharge, swingType, swingAim, false);
                        swingUntil = -1;
                    }
                    UpdateEnemy(dt);
                }
            }
            if (landingMarker)
            {
                landingMarker.gameObject.SetActive(ball.Moving && ball.Bounces == 0);
                landingMarker.position = ball.Target + court.transform.up * .03f;
            }
            if (aimMarker)
            {
                aimMarker.gameObject.SetActive(charging);
                aimMarker.position = court.Aim(0, aim, State == Phase.ServeReady, Score.ServeRight);
            }
        }
        Vector3 ServePosition(TennisActor server) => server.transform.position + court.transform.up * 1.8f
            + court.transform.right * (server.side == 0 ? .8f : -.8f);

        void InputPlayer(float dt)
        {
            var k = Keyboard.current;
            if (k == null) return;
            Vector3 move = new Vector3((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), 0,
                (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
            // Use the raw axes so diagonal movement still aims at the full corner.
            aim = new Vector2(move.x, move.z);
            if (k.digit1Key.wasPressedThisFrame) shotType = 0;
            if (k.digit2Key.wasPressedThisFrame) shotType = 1;
            if (k.digit3Key.wasPressedThisFrame) shotType = 2;
            if (k.spaceKey.wasPressedThisFrame && (State != Phase.ServeReady || Score.Server == 0))
            {
                charging = true;
                charge = 0;
            }
            bool sprint = k.leftShiftKey.isPressed && move.sqrMagnitude > 0 && !charging && player.Stamina > 0;
            float energy = charging ? -balance.chargeDrain : sprint ? -balance.sprintDrain : balance.staminaRecovery;
            player.Energy(energy * dt, balance.stamina);
            if (charging && player.Stamina > 0) charge = Mathf.Min(charge + dt, balance.chargeSeconds);
            float rate = charging ? balance.chargeMoveMultiplier : sprint ? 1.4f : 1;
            if (player.Stamina <= 0) rate *= .75f;
            // The server remains behind the baseline until the serve is released.
            if (!(State == Phase.ServeReady && Score.Server == 0))
                player.Move(court.transform.TransformDirection(move.normalized), balance.playerSpeed * rate, court, dt);
            if (charging && k.spaceKey.wasReleasedThisFrame)
            {
                charging = false;
                float power = Charge;
                charge = 0;
                if (State == Phase.ServeReady && Score.Server == 0) Shoot(0, power, 0, aim, true);
                else
                {
                    swingUntil = clock + balance.swingBuffer;
                    swingCharge = power;
                    swingType = shotType;
                    // Keep the release direction while waiting for ball contact.
                    swingAim = aim;
                    Message = "SWING";
                }
            }
        }
        bool CanHit(TennisActor actor)
        {
            if (!ball.Moving || ball.Hitter == actor.side) return false;
            if (State == Phase.Serve && ball.Bounces == 0) return false;
            Vector3 p = court.Local(ball.transform.position);
            if ((actor.side == 0 && p.z >= 0) || (actor.side == 1 && p.z <= 0)) return false;
            if (p.y < .25f || p.y > balance.maxHitHeight) return false;
            Vector3 delta = ball.transform.position - actor.transform.position;
            delta.y = 0;
            return delta.magnitude <= balance.hitReach;
        }
        void UpdateEnemy(float dt)
        {
            bool incoming = ball.Hitter == 0 && ball.Moving;
            Vector3 target = court.World(new Vector3(0, 0, court.halfLength - 1.8f));
            if (incoming)
            {
                if (clock < reactAt) return;
                target = ball.Bounces == 0 ? ball.Target : ball.transform.position;
            }
            Vector3 delta = target - enemy.transform.position;
            delta.y = 0;
            enemy.Move(delta.normalized, Mathf.Min(balance.enemySpeed, delta.magnitude / Mathf.Max(dt, .001f)), court, dt);
            if (incoming && CanHit(enemy))
            {
                Vector3 p = court.Local(player.transform.position);
                float direction = Mathf.Abs(p.x) > 1 ? -Mathf.Sign(p.x) : (Random.value < .5f ? -1 : 1);
                Shoot(1, .4f, 0, new Vector2(direction * .72f, .5f), false);
            }
        }
        void Shoot(int side, float power, int type, Vector2 direction, bool serve)
        {
            Vector3 target = court.Aim(side, direction, serve, Score.ServeRight);
            float velocity = (serve ? balance.serveSpeed : balance.shotSpeed) * Mathf.Lerp(1, balance.chargedPower, power);
            float height = serve ? balance.shotArc * .85f : balance.shotArc;
            string quality = "NICE";
            if (!serve)
            {
                TennisActor actor = side == 0 ? player : enemy;
                Vector3 offset = ball.transform.position - actor.transform.position;
                offset.y = 0;
                bool bad = offset.magnitude > balance.hitReach * .8f;
                if (bad)
                {
                    quality = "BAD";
                    velocity *= .8f;
                    target += court.transform.right * Random.Range(-.9f, .9f);
                }
                else if (power >= .95f) { quality = "PERFECT"; velocity *= 1.08f; }
                if (type == 1) { height *= 2.3f; velocity *= .72f; }
                if (type == 2)
                {
                    target = court.Aim(side, new Vector2(direction.x, -.9f), false, Score.ServeRight);
                    velocity *= .65f;
                    height *= .7f;
                }
            }
            if (side == 1) target += court.transform.right * Random.Range(-balance.aimError, balance.aimError);
            else target = court.ClampShotTarget(target, side, serve, Score.ServeRight);
            State = serve ? Phase.Serve : Phase.Rally;
            ball.Launch(target, velocity, height, side);
            reactAt = clock + balance.reactionSeconds;
            Message = (side == 0 ? "PLAYER " : "ENEMY ") + (serve ? "SERVE" : quality);
        }
        void OnBounce(Vector3 point, int count)
        {
            if (count == 1)
            {
                bool serve = State == Phase.Serve;
                if (!court.Inside(point, 1 - ball.Hitter, serve, Score.ServeRight))
                {
                    if (serve) Fault("SERVICE OUT");
                    else EndPoint(1 - ball.Hitter, "OUT");
                }
                else State = Phase.Rally;
            }
            else EndPoint(ball.Hitter, "DOUBLE BOUNCE");
        }
        void OnNet()
        {
            if (State == Phase.Serve) Fault("NET");
            else EndPoint(1 - ball.Hitter, "NET");
        }
        void Fault(string reason)
        {
            Faults++;
            if (Faults >= 2) { EndPoint(1 - Score.Server, "DOUBLE FAULT"); return; }
            StopPoint();
            Message = reason + " - FAULT / SECOND SERVE";
        }
        void StopPoint()
        {
            ball.Stop();
            State = Phase.PointEnd;
            phaseAt = clock;
            charge = 0;
            charging = false;
            swingUntil = -1;
            if (landingMarker) landingMarker.gameObject.SetActive(false);
            if (aimMarker) aimMarker.gameObject.SetActive(false);
        }
        void EndPoint(int winner, string reason)
        {
            if (State == Phase.PointEnd || State == Phase.MatchEnd) return;
            StopPoint();
            Score.Award(winner);
            Faults = 0;
            Message = (winner == 0 ? "PLAYER POINT - " : "ENEMY POINT - ") + reason;
            if (Score.Winner >= 0)
            {
                State = Phase.MatchEnd;
                Message = Score.Winner == 0 ? "YOU WIN!  R: REMATCH" : "ENEMY WINS  R: REMATCH";
            }
        }
        void OnApplicationFocus(bool focus)
        {
            if (!focus) { Paused = true; charging = false; charge = 0; swingUntil = -1; }
        }
        void OnGUI()
        {
            if (Score == null || !balance) return;
            float scale = Mathf.Min(Screen.width / 1100f, Screen.height / 720f);
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            var title = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
            var label = new GUIStyle(GUI.skin.label) { fontSize = 17 };
            GUI.Box(new Rect(18, 18, 380, 153), "");
            GUI.Label(new Rect(32, 25, 355, 32), "TENNIS / MAIN COURT", title);
            GUI.Label(new Rect(32, 62, 355, 26), "                 POINT    GAME    SET", label);
            GUI.Label(new Rect(32, 92, 355, 26), $"{(Score.Server == 0 ? ">" : " ")} PLAYER     {Score.PointText(0),-5}      {Score.Games[0]}         {Score.Sets[0]}", label);
            GUI.Label(new Rect(32, 123, 355, 26), $"{(Score.Server == 1 ? ">" : " ")} ENEMY      {Score.PointText(1),-5}      {Score.Games[1]}         {Score.Sets[1]}", label);
            float y = Screen.height / scale - 202;
            var small = new GUIStyle(label) { fontSize = 14, wordWrap = true };
            GUI.Box(new Rect(18, y, 310, 184), "");
            GUI.Label(new Rect(30, y + 8, 286, 46), Paused ? "PAUSED - ESC to resume" : Message, small);
            GUI.Label(new Rect(30, y + 57, 286, 22), $"STAMINA {player.Stamina:0}    CHARGE {Charge:P0}", small);
            GUI.Label(new Rect(30, y + 81, 286, 22), $"SHOT: {new[] { "DRIVE", "LOB", "DROP" }[shotType]}  |  1 Drive  2 Lob  3 Drop", small);
            GUI.Label(new Rect(30, y + 105, 286, 22), "WASD Move + Aim  |  W Deep / S Short", small);
            GUI.Label(new Rect(30, y + 129, 286, 22), "Hold / release SPACE Hit  |  Shift Sprint", small);
            GUI.Label(new Rect(30, y + 153, 286, 22), "ESC Pause  |  R Restart", small);
            GUI.matrix = old;
        }
    }
}
