using UnityEngine;

namespace TennisGame
{
    public sealed class TennisCourt : MonoBehaviour
    {
        [Header("MainScene dimensions, metres")]
        public float halfWidth = 4.115f;
        public float halfLength = 11.885f;
        public float serviceLength = 6.4f;
        public float netHeight = .914f;
        public float runOff = 3;
        public Vector3 Local(Vector3 world) => transform.InverseTransformPoint(world);
        public Vector3 World(Vector3 local) => transform.TransformPoint(local);
        public bool Inside(Vector3 world, int receiver, bool serve, bool right)
        {
            Vector3 p = Local(world);
            float z = p.z * (receiver == 1 ? 1 : -1);
            if (Mathf.Abs(p.x) > halfWidth + .06f || z < 0 || z > (serve ? serviceLength : halfLength) + .06f) return false;
            if (!serve) return true;
            float targetSign = (receiver == 1 ? -1 : 1) * (right ? 1 : -1);
            return p.x * targetSign >= -.06f;
        }
        public Vector3 Aim(int hitter, Vector2 aim, bool serve, bool right)
        {
            float sign = hitter == 0 ? 1 : -1;
            float x = aim.x * (halfWidth - .5f);
            float z = Mathf.Lerp(2.2f, halfLength - .8f, (aim.y + 1) * .5f);
            if (serve)
            {
                x = -sign * (right ? 1 : -1) * halfWidth * .5f + aim.x * .9f;
                z = serviceLength * .65f + aim.y * 1.1f;
            }
            return World(new Vector3(x, .12f, z * sign));
        }
        public Vector3 ClampShotTarget(Vector3 world, int hitter, bool serve, bool right)
        {
            Vector3 p = Local(world);
            const float margin = .5f;
            float sign = hitter == 0 ? 1 : -1;
            float minX = -halfWidth + margin;
            float maxX = halfWidth - margin;
            if (serve)
            {
                float targetSign = -sign * (right ? 1 : -1);
                if (targetSign < 0) maxX = -margin;
                else minX = margin;
            }
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.z = sign * Mathf.Clamp(p.z * sign, margin, (serve ? serviceLength : halfLength) - margin);
            p.y = .12f;
            return World(p);
        }
        public Vector3 ClampActor(Vector3 world, int side)
        {
            Vector3 p = Local(world);
            p.x = Mathf.Clamp(p.x, -halfWidth - runOff, halfWidth + runOff);
            p.z = side == 0 ? Mathf.Clamp(p.z, -halfLength - runOff, -.7f) : Mathf.Clamp(p.z, .7f, halfLength + runOff);
            p.y = 0;
            return World(p);
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, .1f, halfLength * 2));
        }
    }
}
