using System;
using System.Collections.Generic;
using System.IO;
using Game2Week.Dialogue;
using Game2Week.EditorTools.Dialogue;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class DialogueTests
    {
        static DialogueDefinition Sample()
        {
            var definition = new DialogueDefinition(); definition.speakers["heroine"] = new DialogueSpeaker();
            definition.nodes.Add(new DialogueNode { choices = new List<DialogueChoice> { new() { text = "듣는다", next = "end", effects = new List<DialogueEffect> { new() { type = "spareCondition", id = "heard" } } } } });
            definition.nodes.Add(new DialogueNode { id = "end", end = true }); return definition;
        }
        [Test] public void 사전형화자와선택효과가한글JSON으로왕복하고변조를검출()
        {
            var definition = Sample(); var json = DialogueJson.Write(definition); StringAssert.Contains("주인공", json);
            var loaded = DialogueJson.Read(json); Assert.IsTrue(DialogueJson.HasValidChecksum(loaded));
            Assert.AreEqual("player", loaded.speakers["heroine"].anchor); loaded.nodes[0].text = "변경";
            Assert.IsFalse(DialogueJson.HasValidChecksum(loaded));
        }
        [Test] public void 끊긴연결없는화자미지원효과를거부()
        {
            var definition = Sample(); definition.nodes[0].speaker = "absent"; definition.nodes[0].choices[0].next = "absent";
            definition.nodes[0].choices[0].effects[0].type = "damage";
            Assert.GreaterOrEqual(DialogueValidator.Validate(definition).Count, 3);
        }
        [Test] public void 도달못하는노드와없는폰트글자를검사()
        {
            var definition = Sample(); definition.nodes.Add(new DialogueNode { id = "orphan", end = true });
            Assert.IsTrue(DialogueValidator.Validate(definition, c => c != '듣').Exists(e => e.Contains("정적 폰트")));
            Assert.IsTrue(DialogueValidator.Validate(definition).Exists(e => e.Contains("도달할 수 없는")));
        }
        [Test] public void 선택효과는등록한자비처리기에한번만전달()
        {
            int count = 0; var runner = new DialogueRunner(Sample(), new Dictionary<string, Action<DialogueEffect>> { ["spareCondition"] = effect => { Assert.AreEqual("heard", effect.id); count++; } });
            Assert.Throws<ArgumentOutOfRangeException>(() => runner.Advance()); runner.Advance(0); runner.Advance();
            Assert.IsTrue(runner.Ended); Assert.AreEqual(1, count);
        }
        [Test] public void 편집되돌리기와실패저장이기존파일을보호()
        {
            string directory = Path.Combine(Path.GetTempPath(), "대화검사_" + Guid.NewGuid().ToString("N"));
            try
            {
                var repository = new DialogueRepository(directory); var model = new DialogueEditorModel(repository); model.New("dlg_test"); model.Save();
                model.Edit(d => d.nodes[0].text = "고친 대사"); model.Undo(); Assert.IsFalse(model.Dirty); model.Redo(); Assert.IsTrue(model.Dirty);
                model.Edit(d => d.nodes[0].next = "missing"); Assert.Throws<InvalidDataException>(() => model.Save());
                Assert.AreEqual("이야기를 해 보자.", repository.Load("dlg_test").nodes[0].text);
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
        [Test] public void 저장소는경로탈출을거부() => Assert.Throws<ArgumentException>(() => new DialogueRepository(Path.GetTempPath()).Load("../outside"));
    }
}
