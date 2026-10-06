using System;

namespace Game2Week.Battle
{
    /// <summary>
    /// 타이밍 공격 게이지: 커서가 왼쪽(0)에서 오른쪽(1)으로 지나가고, 확인키를 누른 위치로 정확도가 정해진다.
    /// 정중앙(0.5) = 정확도 1, 양 끝 = 0. 끝까지 안 누르면 MISS.
    /// </summary>
    public sealed class TimingGauge
    {
        public const float MinMultiplier = 0.5f;
        public const float MaxMultiplier = 2.0f;

        readonly float sweepSeconds;

        public TimingGauge(float sweepSeconds = 1.3f)
        {
            if (sweepSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(sweepSeconds));
            this.sweepSeconds = sweepSeconds;
        }

        /// <summary>커서 위치 0~1</summary>
        public float Position { get; private set; }
        public bool IsFinished { get; private set; }
        /// <summary>눌렀으면 0~1, 놓쳤으면 null (끝나기 전엔 의미 없음)</summary>
        public float? Accuracy { get; private set; }

        public void Advance(float deltaTime)
        {
            if (IsFinished) return;
            Position += deltaTime / sweepSeconds;
            if (Position >= 1f)
            {
                Position = 1f;
                IsFinished = true;
                Accuracy = null; // MISS
            }
        }

        public void Press()
        {
            if (IsFinished) return;
            IsFinished = true;
            Accuracy = AccuracyAt(Position);
        }

        public static float AccuracyAt(float position) => Math.Clamp(1f - Math.Abs(position - 0.5f) * 2f, 0f, 1f);

        /// <summary>정확도 → 데미지 배율 (0 → ×0.5, 1 → ×2.0)</summary>
        public static float Multiplier(float accuracy) => MinMultiplier + (MaxMultiplier - MinMultiplier) * Math.Clamp(accuracy, 0f, 1f);
    }
}
