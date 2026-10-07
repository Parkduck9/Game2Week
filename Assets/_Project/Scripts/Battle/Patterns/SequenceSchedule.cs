using System;
using System.Collections.Generic;
using Game2Week.Data;
using Game2Week.Data.Patterns;

namespace Game2Week.Battle.Patterns
{
    public readonly struct SequenceStart
    {
        public SequenceStart(float time, float duration, AttackPatternData pattern, int seed)
        { Time = time; Duration = duration; Pattern = pattern; Seed = seed; }
        public float Time { get; }
        public float Duration { get; }
        public AttackPatternData Pattern { get; }
        public int Seed { get; }
    }

    /// <summary>프레임 분할과 무관한 시간표와 각 발사의 독립 시드.</summary>
    public static class SequenceSchedule
    {
        public static List<string> Validate(AttackSequence sequence)
        {
            var errors = new List<string>();
            if (!sequence) { errors.Add("시퀀스가 없습니다."); return errors; }
            if (sequence.entries == null || sequence.entries.Count == 0)
            { errors.Add("공격을 하나 이상 추가하세요."); return errors; }
            bool red = false, blue = false;
            foreach (var entry in sequence.entries)
            {
                if (entry == null) { errors.Add("빈 시간표 항목입니다."); continue; }
                if (!Finite(entry.time) || entry.time < 0 || entry.time > 60 ||
                    !Finite(entry.duration) || entry.duration <= 0 || entry.duration > 60 ||
                    (entry.repeat && (!Finite(entry.interval) || entry.interval < .05f || entry.count < 1 || entry.count > 64)))
                    errors.Add("시각·지속 시간·반복 간격과 횟수를 확인하세요.");
                var prefab = entry.pattern ? entry.pattern.PatternPrefab : null;
                if (!prefab || !prefab.TryGetComponent<IAttackPattern>(out _))
                { errors.Add("실행 가능한 공격 패턴을 선택하세요."); continue; }
                if (prefab.TryGetComponent<SequencePattern>(out _))
                { errors.Add("순환 참조를 막기 위해 시퀀스 안에는 일반 패턴을 넣으세요."); continue; }
                if (prefab.TryGetComponent<IDirectablePattern>(out var directed))
                { red |= directed.PatternColor == AttackColor.Red; blue |= directed.PatternColor == AttackColor.Blue; }
            }
            if (red && blue) errors.Add("빨강과 파랑은 같은 시퀀스에 넣을 수 없습니다.");
            return errors;
        }

        public static AttackColor Color(AttackSequence sequence)
        {
            var color = AttackColor.Yellow;
            if (!sequence || sequence.entries == null) return color;
            foreach (var entry in sequence.entries)
                if (entry?.pattern && entry.pattern.PatternPrefab &&
                    entry.pattern.PatternPrefab.TryGetComponent<IDirectablePattern>(out var directed) &&
                    directed.PatternColor != AttackColor.Yellow) color = directed.PatternColor;
            return color;
        }

        public static List<SequenceStart> Build(AttackSequence sequence, float scale = 1f)
        {
            var errors = Validate(sequence);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(" / ", errors));
            if (!Finite(scale) || scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
            var result = new List<SequenceStart>();
            var random = new Random(sequence.seed);
            foreach (var entry in sequence.entries)
                for (int i = 0; i < (entry.repeat ? entry.count : 1); i++)
                    result.Add(new SequenceStart((entry.time + i * entry.interval) * scale,
                        entry.duration * scale, entry.pattern, random.Next()));
            // 같은 시각이면 입력 순서대로 유지한다.
            for (int i = 1; i < result.Count; i++)
            {
                var item = result[i]; int j = i - 1;
                while (j >= 0 && result[j].Time > item.Time) { result[j + 1] = result[j]; j--; }
                result[j + 1] = item;
            }
            return result;
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
    public interface ISeededPattern { void ApplySeed(int seed); }
}
