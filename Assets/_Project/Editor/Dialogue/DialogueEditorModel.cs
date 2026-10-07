using System;
using System.Collections.Generic;
using System.IO;
using Game2Week.Dialogue;

namespace Game2Week.EditorTools.Dialogue
{
    /// <summary>편집 사본·되돌리기·검증·저장을 창에서 분리한다.</summary>
    public sealed class DialogueEditorModel
    {
        readonly DialogueRepository repository;
        readonly Func<char, bool> hasCharacter;
        readonly Stack<string> undo = new(), redo = new();
        string saved;
        public DialogueEditorModel(DialogueRepository repository, Func<char, bool> hasCharacter = null)
        { this.repository = repository; this.hasCharacter = hasCharacter; }
        public DialogueDefinition Current { get; private set; }
        public IReadOnlyList<string> Files => repository.List();
        public bool Dirty => Current != null && DialogueJson.Encode(Current) != saved;
        public bool CanUndo => undo.Count > 0;
        public bool CanRedo => redo.Count > 0;
        public void Load(string id)
        { Current = repository.Load(id); saved = DialogueJson.Encode(Current); undo.Clear(); redo.Clear(); }
        public void New(string id)
        {
            Current = new DialogueDefinition { id = id };
            Current.speakers.Add("heroine", new DialogueSpeaker());
            Current.nodes.Add(new DialogueNode { text = "이야기를 해 보자.", next = "end" });
            Current.nodes.Add(new DialogueNode { id = "end", end = true });
            saved = null; undo.Clear(); redo.Clear();
        }
        public void Edit(Action<DialogueDefinition> change)
        {
            if (Current == null) return;
            var before = DialogueJson.Encode(Current); var copy = DialogueJson.Clone(Current); change(copy);
            if (before == DialogueJson.Encode(copy)) return;
            undo.Push(before); redo.Clear(); Current = copy;
        }
        public void Undo() => Swap(undo, redo);
        public void Redo() => Swap(redo, undo);
        void Swap(Stack<string> from, Stack<string> to)
        { if (Current == null || from.Count == 0) return; to.Push(DialogueJson.Encode(Current)); Current = DialogueJson.Read(from.Pop()); }
        public List<string> Validate() => DialogueValidator.Validate(Current, hasCharacter);
        public void Save()
        {
            var errors = Validate(); if (errors.Count > 0) throw new InvalidDataException(string.Join("\n", errors));
            repository.Save(Current, hasCharacter); saved = DialogueJson.Encode(Current);
        }
        public void AddNode()
        {
            Edit(definition =>
            {
                int number = 1;
                while (definition.nodes.Exists(node => node.id == "n" + number)) number++;
                definition.nodes.Add(new DialogueNode { id = "n" + number, next = "end" });
            });
        }
    }
}
