using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Battle
{
    /// <summary>이동 상태와 시간. Transform과 입력 장치에 의존하지 않는다.</summary>
    public sealed class PlayerMotorModel
    {
        readonly PlayerActionSettings settings;
        float dodgeElapsed, dodgeCooldown, jumpElapsed, parryElapsed, parryCooldown;
        float dodgeBuffer, jumpBuffer, parryBuffer;
        Vector2 lastDirection = Vector2.up;
        Vector2 dodgeDirection;

        public PlayerMotorModel(PlayerActionSettings settings) { this.settings = settings; }
        public bool Dodging { get; private set; }
        public bool Airborne { get; private set; }
        public bool Parrying { get; private set; }
        public bool IsGrounded => !Airborne;
        public bool DodgeInvulnerable => Dodging && dodgeElapsed >= settings.dodgeInvulnerability.x && dodgeElapsed <= settings.dodgeInvulnerability.y;
        public bool CanParry => Parrying && parryElapsed >= settings.parryWindow.x && parryElapsed <= settings.parryWindow.y;
        public float DodgeCooldown => dodgeCooldown;
        public float ParryCooldown => parryCooldown;
        public float ParryProgress => Parrying ? Mathf.Clamp01(parryElapsed / settings.parryDuration) : 0f;
        public float Height => Airborne ? 4f * settings.jumpHeight * (jumpElapsed / settings.jumpDuration) * (1f - jumpElapsed / settings.jumpDuration) : 0f;
        public Vector2 Displacement { get; private set; }

        public void RequestDodge() => dodgeBuffer = settings.inputBuffer;
        public void RequestJump() => jumpBuffer = settings.inputBuffer;
        public void RequestParry() => parryBuffer = settings.inputBuffer;
        public void ClearBuffer() { dodgeBuffer = jumpBuffer = parryBuffer = 0f; }
        public void Reset()
        {
            Dodging = Airborne = Parrying = false;
            dodgeElapsed = dodgeCooldown = jumpElapsed = parryElapsed = parryCooldown = 0f;
            Displacement = Vector2.zero;
            lastDirection = Vector2.up;
            ClearBuffer();
        }

        public void Tick(Vector2 move, float dt)
        {
            Displacement = Vector2.zero;
            if (dt <= 0f) return;
            move = Vector2.ClampMagnitude(move, 1f);
            if (move.sqrMagnitude > 0.001f) lastDirection = move.normalized;
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);
            parryCooldown = Mathf.Max(0f, parryCooldown - dt);
            if (dodgeBuffer > 0f && !Dodging && !Airborne && dodgeCooldown <= 0f)
            {
                Dodging = true; dodgeElapsed = 0f; dodgeCooldown = settings.dodgeCooldown;
                dodgeDirection = lastDirection; dodgeBuffer = 0f;
                Parrying = false; parryBuffer = 0f;
            }
            else if (jumpBuffer > 0f && !Dodging && !Airborne)
            { Airborne = true; jumpElapsed = 0f; jumpBuffer = 0f; }
            if (parryBuffer > 0f && !Dodging && parryCooldown <= 0f)
            { Parrying = true; parryElapsed = 0f; parryCooldown = settings.parryCooldown; parryBuffer = 0f; }

            if (Dodging)
            {
                float timeMoved = Mathf.Min(dt, settings.dodgeDuration - dodgeElapsed);
                Displacement = dodgeDirection * (settings.dodgeDistance / settings.dodgeDuration * timeMoved);
                dodgeElapsed += dt;
                if (dodgeElapsed >= settings.dodgeDuration) Dodging = false;
            }
            else Displacement = move * (settings.moveSpeed * dt);
            if (Airborne)
            {
                jumpElapsed += dt;
                if (jumpElapsed >= settings.jumpDuration) { Airborne = false; jumpElapsed = 0f; }
            }
            if (Parrying)
            {
                parryElapsed += dt;
                if (parryElapsed >= settings.parryDuration) Parrying = false;
            }
            dodgeBuffer = Mathf.Max(0f, dodgeBuffer - dt);
            jumpBuffer = Mathf.Max(0f, jumpBuffer - dt);
            parryBuffer = Mathf.Max(0f, parryBuffer - dt);
        }
    }
}
