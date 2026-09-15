using System;
using UnityEngine;

namespace TennisGame
{
    // Like MainScene's BallController: a target-driven arc followed by a bounce.
    // Explicit stepping makes net/landing rules independent of physics callbacks.
    public sealed class TennisBall : MonoBehaviour
    {
        public Transform visual;
        public event Action<Vector3, int> Bounced;
        public event Action NetHit;
        public int Hitter { get; private set; }
        public int Bounces { get; private set; }
        public bool Moving { get; private set; }
        public Vector3 Target { get; private set; }
        Vector3 start;
        float elapsed, duration, arc, speed;
        public void Stop() => Moving = false;
        public void Launch(Vector3 target, float velocity, float height, int hitter)
        {
            Hitter = hitter;
            Bounces = 0;
            Path(target, velocity, height);
        }
        void Path(Vector3 target, float velocity, float height)
        {
            start = transform.position;
            Target = target;
            speed = Mathf.Max(1, velocity);
            Vector3 delta = target - start;
            delta.y = 0;
            duration = Mathf.Max(.15f, delta.magnitude / speed);
            arc = height;
            elapsed = 0;
            Moving = true;
        }
        public void Step(float dt, TennisCourt court, TennisSettings settings)
        {
            // Small substeps preserve net detection even during slow frames.
            while (dt > 0 && Moving)
            {
                float step = Mathf.Min(dt, 1f / 120);
                dt -= step;
                Vector3 previous = court.Local(transform.position);
                elapsed += step;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 position = Vector3.Lerp(start, Target, t) + court.transform.up * (4 * arc * t * (1 - t));
                Vector3 next = court.Local(position);
                if (previous.z * next.z <= 0 && Mathf.Abs(previous.z - next.z) > .00001f)
                {
                    Vector3 crossing = Vector3.Lerp(previous, next, previous.z / (previous.z - next.z));
                    if (Mathf.Abs(crossing.x) <= 5.6f && crossing.y <= court.netHeight + .12f)
                    {
                        transform.position = court.World(crossing);
                        Moving = false;
                        NetHit?.Invoke();
                        return;
                    }
                }
                transform.position = position;
                if (t < 1) continue;
                Bounces++;
                Bounced?.Invoke(Target, Bounces);
                if (!Moving) return;
                if (Bounces >= 2) { Stop(); return; }
                Vector3 direction = Target - start;
                direction.y = 0;
                Path(Target + direction.normalized * settings.bounceDistance, speed * settings.bounceSpeed, settings.bounceArc);
            }
        }
    }
}
