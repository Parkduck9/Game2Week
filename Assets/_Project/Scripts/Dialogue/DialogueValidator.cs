using System;
using System.Collections.Generic;
using System.Linq;

namespace Game2Week.Dialogue
{
    public static class DialogueValidator
    {
        public static List<string> Validate(DialogueDefinition definition, Func<char, bool> hasCharacter = null)
        {
            var errors = new List<string>();
            if (definition == null) { errors.Add("대화 데이터가 없습니다."); return errors; }
            if (definition.schemaVersion != 1) errors.Add("대화 형식 버전은 1이어야 합니다.");
            if (string.IsNullOrWhiteSpace(definition.id) || definition.id.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 || definition.id.Contains("..")) errors.Add("대화 파일 이름이 올바르지 않습니다.");
            var nodes = new Dictionary<string, DialogueNode>();
            foreach (var node in definition.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id) || !nodes.TryAdd(node.id, node)) { errors.Add("노드 이름이 비어 있거나 중복됩니다."); continue; }
                if (!node.end && !definition.speakers.ContainsKey(node.speaker ?? "")) errors.Add(node.id + ": 없는 화자입니다.");
                if (!node.end && node.choices.Count == 0 && string.IsNullOrEmpty(node.next)) errors.Add(node.id + ": 다음 연결이 없습니다.");
                foreach (var effect in node.effects.Concat(node.choices.SelectMany(c => c.effects)))
                    if (effect == null || (effect.type != "flag" && effect.type != "spareCondition") || string.IsNullOrWhiteSpace(effect.id)) errors.Add(node.id + ": 등록되지 않은 효과 또는 빈 조건 이름입니다.");
            }
            if (!nodes.ContainsKey(definition.start ?? "")) errors.Add("시작 노드가 없습니다.");
            foreach (var node in nodes.Values)
                foreach (var next in Connections(node)) if (!nodes.ContainsKey(next ?? "")) errors.Add(node.id + ": 끊긴 연결 " + next);
            var visited = new HashSet<string>(); var pending = new Stack<string>();
            if (nodes.ContainsKey(definition.start ?? "")) pending.Push(definition.start);
            while (pending.Count > 0)
            {
                string id = pending.Pop(); if (!visited.Add(id) || !nodes.TryGetValue(id, out var node)) continue;
                foreach (var next in Connections(node)) if (nodes.ContainsKey(next ?? "")) pending.Push(next);
            }
            foreach (string id in nodes.Keys) if (!visited.Contains(id)) errors.Add("도달할 수 없는 노드: " + id);
            if (!visited.Any(id => nodes[id].end)) errors.Add("종료 노드에 도달할 수 없습니다.");
            if (hasCharacter != null)
            {
                var text = string.Concat(definition.speakers.Values.Select(s => s.name)) + string.Concat(definition.nodes.Select(n => n.text + string.Concat(n.choices.Select(c => c.text))));
                string missing = new(text.Where(c => !char.IsWhiteSpace(c) && !hasCharacter(c)).Distinct().ToArray());
                if (missing.Length > 0) errors.Add("정적 폰트에 없는 글자: " + missing);
            }
            return errors;
        }
        static IEnumerable<string> Connections(DialogueNode node)
        {
            if (node.end) yield break;
            if (node.choices.Count > 0) { foreach (var choice in node.choices) yield return choice.next; }
            else if (!string.IsNullOrEmpty(node.next)) yield return node.next;
        }
    }
}
