using UnityEngine;

namespace TennisGame
{
    public sealed class TennisActor : MonoBehaviour
    {
        [Tooltip("Replace only the children of Visual; keep this gameplay root.")]
        public Transform visual;
        public int side;
        public float Stamina { get; private set; }
        public void ResetEnergy(float amount) => Stamina = amount;
        public void Energy(float delta, float maximum) => Stamina = Mathf.Clamp(Stamina + delta, 0, maximum);
        public void Move(Vector3 direction, float speed, TennisCourt court, float dt)
        {
            transform.position = court.ClampActor(transform.position + direction * speed * dt, side);
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = side == 0 ? Color.cyan : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 2.1f);
        }
    }
}
