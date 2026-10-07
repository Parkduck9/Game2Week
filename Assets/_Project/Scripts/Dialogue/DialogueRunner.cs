using System;
using System.Collections.Generic;
using System.Linq;

namespace Game2Week.Dialogue
{
    /// <summary>분기와 효과만 실행한다. 카메라·입력·적 데이터에 의존하지 않는다.</summary>
    public sealed class DialogueRunner
    {
        readonly Dictionary<string, DialogueNode> nodes;
        readonly Dictionary<string, Action<DialogueEffect>> handlers;
        public DialogueRunner(DialogueDefinition definition, Dictionary<string, Action<DialogueEffect>> handlers = null)
        {
            var errors = DialogueValidator.Validate(definition);
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors));
            this.handlers = handlers ?? new(); nodes = definition.nodes.ToDictionary(n => n.id);
            Enter(definition.start);
        }
        public DialogueNode Current { get; private set; }
        public bool Ended => Current.end;
        public void Advance(int choice = -1)
        {
            if (Ended) return;
            string next = Current.next;
            if (Current.choices.Count > 0)
            {
                if (choice < 0 || choice >= Current.choices.Count) throw new ArgumentOutOfRangeException(nameof(choice));
                var selected = Current.choices[choice]; Apply(selected.effects); next = selected.next;
            }
            Enter(next);
        }
        void Enter(string id) { Current = nodes[id]; Apply(Current.effects); }
        void Apply(IEnumerable<DialogueEffect> effects)
        {
            foreach (var effect in effects)
                if (handlers.TryGetValue(effect.type, out var handler)) handler(effect);
                else if (handlers.Count > 0) throw new InvalidOperationException("등록되지 않은 대화 효과: " + effect.type);
        }
    }
}
