using UnityEngine;

namespace Game2Week.Data
{
    [CreateAssetMenu(menuName = "Game2Week/Player Actions", fileName = "PlayerActionSettings")]
    public sealed class PlayerActionSettings : ScriptableObject
    {
        [Min(0.1f)] public float moveSpeed = 3.4f;
        [Min(0.1f)] public float dodgeDistance = 2f;
        [Min(0.05f)] public float dodgeDuration = 0.22f;
        [Min(0.1f)] public float dodgeCooldown = 1f;
        public Vector2 dodgeInvulnerability = new(0.08f, 0.18f);
        [Min(0.1f)] public float jumpHeight = 0.75f;
        [Min(0.1f)] public float jumpDuration = 0.65f;
        [Min(0.01f)] public float parryDuration = 0.4f;
        public Vector2 parryWindow = new(0.04f, 0.22f);
        [Min(0.1f)] public float parryCooldown = 0.55f;
        [Min(0f)] public float inputBuffer = 0.12f;
        [Min(0.1f)] public float bodyHeight = 0.85f;
        [Min(0.05f)] public float parryReach = 0.72f;
    }
}
