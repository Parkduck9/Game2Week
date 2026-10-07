using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game2Week.Battle
{
    public interface ISpareCondition
    {
        float Progress(SpareTracker tracker);
        string Hint(SpareTracker tracker);
    }

    [Serializable]
    public sealed class SpareRule
    {
        public bool enabled;
        public int noFightTurns = 3;
        public bool halveOnFight;
        [SerializeReference] public List<ISpareCondition> conditions = new();
    }

    [Serializable]
    public sealed class NoFightTurns : ISpareCondition
    {
        public int required = 3;
        public float Progress(SpareTracker tracker) => Math.Min(1f, (float)tracker.NoFightTurns / Math.Max(1, required));
        public string Hint(SpareTracker tracker) => BattleTexts.NoFightHint(tracker.NoFightTurns, required);
    }

    [Serializable]
    public sealed class ActSequenceCondition : ISpareCondition
    {
        public List<string> acts = new();
        public float Progress(SpareTracker tracker) => acts.Count == 0 ? 1f : (float)tracker.SequenceProgress(acts) / acts.Count;
        public string Hint(SpareTracker tracker) => BattleTexts.ActOrderHint(string.Join(" → ", acts));
    }

    public enum SpareActionKind { Parry, RedPass, BluePass }

    [Serializable]
    public sealed class BulletActionCondition : ISpareCondition
    {
        public SpareActionKind action;
        public int required = 3;
        public float Progress(SpareTracker tracker) => Math.Min(1f, (float)tracker.ActionCount(action) / Math.Max(1, required));
        public string Hint(SpareTracker tracker) => BattleTexts.ActionCountHint(action, tracker.ActionCount(action), required);
    }

    [Serializable]
    public sealed class UnhurtTurnCondition : ISpareCondition
    {
        public int required = 1;
        public float Progress(SpareTracker tracker) => Math.Min(1f, (float)tracker.UnhurtTurns / Math.Max(1, required));
        public string Hint(SpareTracker tracker) => BattleTexts.UnhurtHint;
    }

    [Serializable]
    public sealed class DialogueFlagCondition : ISpareCondition
    {
        public string flag;
        public string hint = BattleTexts.ListenHint;
        public float Progress(SpareTracker tracker) => tracker.HasFlag(flag) ? 1f : 0f;
        public string Hint(SpareTracker tracker) => hint;
    }

    [Serializable]
    public sealed class SpareGemCondition : ISpareCondition
    {
        public int required = 2;
        public float Progress(SpareTracker tracker) => Math.Min(1f, (float)tracker.Gems / Math.Max(1, required));
        public string Hint(SpareTracker tracker) => BattleTexts.GemHint(tracker.Gems, required);
    }

    [Serializable]
    public sealed class EnemyHealthCondition : ISpareCondition
    {
        [Range(0f, 1f)] public float ratio = .5f;
        public float Progress(SpareTracker tracker) => tracker.HealthRatio <= ratio ? 1f : 0f;
        public string Hint(SpareTracker tracker) => BattleTexts.HealthHint;
    }

    /// <summary>에셋의 조건은 읽기 전용이며 모든 진행값은 전투마다 새로 생성한다.</summary>
    public sealed class SpareTracker
    {
        readonly SpareRule rule;
        readonly List<string> actHistory = new();
        readonly Dictionary<SpareActionKind, int> actions = new();
        readonly HashSet<string> flags = new();
        readonly HashSet<int> revealed = new();
        bool bulletTurn, hurt;
        public SpareTracker(SpareRule rule) { this.rule = rule ?? new SpareRule(); }
        public int NoFightTurns { get; private set; }
        public int UnhurtTurns { get; private set; }
        public int Gems { get; private set; }
        public float HealthRatio { get; set; } = 1f;
        public bool UsesRule => rule.enabled;
        IEnumerable<ISpareCondition> Conditions => new ISpareCondition[] { new NoFightTurns { required = rule.noFightTurns } }.Concat(rule.conditions.Where(c => c != null));
        public int Total => Conditions.Count();
        public int Fulfilled => Conditions.Count(c => c.Progress(this) >= 1f);
        public bool Ready => Fulfilled == Total;
        public string Hint => Conditions.FirstOrDefault(c => c.Progress(this) < 1f)?.Hint(this) ?? BattleTexts.NowSpareable;
        public void CompletePlayerTurn(bool fought = false)
        {
            if (fought) NoFightTurns = rule.halveOnFight ? NoFightTurns / 2 : 0;
            else NoFightTurns++;
        }
        public void RecordAct(string id) => actHistory.Add(id);
        public int SequenceProgress(IReadOnlyList<string> sequence)
        {
            int matched = 0;
            foreach (var id in actHistory) if (matched < sequence.Count && id == sequence[matched]) matched++;
            return matched;
        }
        public int ActionCount(SpareActionKind kind) => actions.TryGetValue(kind, out int count) ? count : 0;
        public void RecordAction(SpareActionKind kind) { if (bulletTurn) actions[kind] = ActionCount(kind) + 1; }
        public void BeginBulletTurn() { bulletTurn = true; hurt = false; }
        public void RecordHit() { if (bulletTurn) hurt = true; }
        public void EndBulletTurn() { if (bulletTurn && !hurt) UnhurtTurns++; bulletTurn = false; }
        public bool HasFlag(string id) => flags.Contains(id ?? string.Empty);
        public void SetFlag(string id, bool value = true) { if (value) flags.Add(id); else flags.Remove(id); }
        public string RevealGemHint()
        {
            Gems++;
            var all = Conditions.ToArray();
            for (int i = 0; i < all.Length; i++)
                if (all[i].Progress(this) < 1f && revealed.Add(i)) return all[i].Hint(this);
            return Hint;
        }
    }
}
