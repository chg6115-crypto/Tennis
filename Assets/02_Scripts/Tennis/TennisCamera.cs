using UnityEngine;

namespace TennisGame
{
    public sealed class TennisCamera : MonoBehaviour
    {
        public Transform player;
        public Transform court;
        public float follow = .12f;
        Vector3 origin;
        void Start() => origin = transform.position;
        void LateUpdate()
        {
            if (!player || !court) return;
            Vector3 target = origin + court.right * (court.InverseTransformPoint(player.position).x * follow);
            transform.position = Vector3.Lerp(transform.position, target, 1 - Mathf.Exp(-4 * Time.deltaTime));
        }
    }
}
