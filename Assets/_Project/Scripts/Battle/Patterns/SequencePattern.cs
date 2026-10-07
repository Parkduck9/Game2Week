using System.Collections.Generic;
using Game2Week.Data.Patterns;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    public sealed class SequencePattern : MonoBehaviour, IAttackPattern, IDirectablePattern, IThreatSource
    {
        sealed class Child
        {
            public GameObject root;
            public IAttackPattern pattern;
            public float end;
        }
        [SerializeField] AttackSequence sequence;
        readonly List<Child> children = new();
        List<SequenceStart> schedule;
        PatternContext context;
        float time, intervalScale = 1, speedScale = 1;
        int next;
        public AttackColor PatternColor => SequenceSchedule.Color(sequence);
        public int ActiveChildren => children.Count;
        public void Configure(AttackSequence value) => sequence = value;
        public void ApplyDifficulty(float interval, float speed) { intervalScale = interval; speedScale = speed; }
        public void Begin(PatternContext value)
        {
            End(); schedule = SequenceSchedule.Build(sequence, intervalScale);
            context = value; time = 0; next = 0; LaunchDue();
        }
        void LaunchDue()
        {
            while (next < schedule.Count && schedule[next].Time <= time + .000001f)
            {
                var entry = schedule[next++];
                var root = Instantiate(entry.Pattern.PatternPrefab, transform);
                root.TryGetComponent<IAttackPattern>(out var pattern);
                if (root.TryGetComponent<IDirectablePattern>(out var directed)) directed.ApplyDifficulty(intervalScale, speedScale);
                if (root.TryGetComponent<ISeededPattern>(out var seeded)) seeded.ApplySeed(entry.Seed);
                pattern.Begin(context);
                children.Add(new Child { root = root, pattern = pattern, end = entry.Time + entry.Duration });
            }
        }
        public void Tick(float delta)
        {
            if (context == null || delta <= 0 || float.IsNaN(delta) || float.IsInfinity(delta)) return;
            float end = time + delta;
            while (time < end)
            {
                float boundary = end;
                if (next < schedule.Count) boundary = Mathf.Min(boundary, schedule[next].Time);
                foreach (var child in children) boundary = Mathf.Min(boundary, child.end);
                float step = Mathf.Max(0, boundary - time);
                foreach (var child in children) child.pattern.Tick(step);
                time = boundary;
                for (int i = children.Count - 1; i >= 0; i--)
                    if (children[i].end <= time + .000001f) Remove(i);
                LaunchDue();
            }
        }
        void Remove(int index)
        { var child = children[index]; child.pattern.End(); Destroy(child.root); children.RemoveAt(index); }
        public void CollectThreats(List<ThreatPoint> output)
        { foreach (var child in children) if (child.pattern is IThreatSource source) source.CollectThreats(output); }
        public void End()
        { for (int i = children.Count - 1; i >= 0; i--) Remove(i); context = null; schedule = null; time = 0; next = 0; }
        void OnDestroy() => End();
    }
}
