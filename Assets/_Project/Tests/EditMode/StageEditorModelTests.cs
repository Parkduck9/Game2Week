using System.IO;
using System.Linq;
using Game2Week.EditorTools.Stages;
using Game2Week.Stages;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class StageEditorModelTests
    {
        string dir;
        StageRepository repo;
        StageEditorModel model;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "Game2WeekStageEditor_" + System.Guid.NewGuid().ToString("N"));
            repo = new StageRepository(dir);
            repo.SaveStage(new StageDefinition { id = "stage_001", name = "첫째" });
            repo.SaveStage(new StageDefinition { id = "stage_002", name = "둘째" });
            repo.SaveIndex(new StageIndex { stages = { "stage_001", "stage_002" } });
            model = new StageEditorModel(repo);
            model.Load();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void Load_ReadsStagesInIndexOrder()
        {
            Assert.AreEqual(2, model.Count);
            Assert.AreEqual("첫째", model.Current.name);
            Assert.IsFalse(model.HasUnsavedChanges);
        }

        [Test]
        public void GemTool_AddsGemWithUniqueIds_OnEmptyCellsOnly()
        {
            model.Tool = StageTool.Gem;

            Assert.IsTrue(model.Click(new GridPoint(2, 5)));
            Assert.IsTrue(model.Click(new GridPoint(3, 5)));
            Assert.IsFalse(model.Click(model.Current.playerStart), "주인공 칸에는 못 놓음");
            Assert.IsFalse(model.Click(new GridPoint(99, 0)), "경기장 밖에는 못 놓음");

            CollectionAssert.AreEqual(new[] { "gem_01", "gem_02" }, model.Current.gems.Select(g => g.id));
            Assert.IsTrue(model.HasUnsavedChanges);
        }

        [Test]
        public void PlayerAndEnemyTools_MoveToEmptyCell()
        {
            model.Tool = StageTool.PlayerStart;
            model.Click(new GridPoint(0, 0));
            model.Tool = StageTool.Enemy;
            model.Click(new GridPoint(11, 13));

            Assert.AreEqual(new GridPoint(0, 0), model.Current.playerStart);
            Assert.AreEqual(new GridPoint(11, 13), model.Current.enemy.position);
        }

        [Test]
        public void Erase_RemovesGemsOnly()
        {
            model.Tool = StageTool.Gem;
            model.Click(new GridPoint(2, 5));

            Assert.IsFalse(model.EraseAt(model.Current.playerStart));
            Assert.IsTrue(model.EraseAt(new GridPoint(2, 5)));
            CollectionAssert.IsEmpty(model.Current.gems);
        }

        [Test]
        public void Drag_MovesSelectedItem_AndIsOneUndoStep()
        {
            var start = model.Current.playerStart;
            model.BeginDrag(start);
            model.DragTo(new GridPoint(start.x + 1, start.z));
            model.DragTo(new GridPoint(start.x + 2, start.z));
            Assert.IsFalse(model.DragTo(model.Current.enemy.position), "다른 항목 위로는 못 옮김");
            model.EndDrag();

            Assert.AreEqual(new GridPoint(start.x + 2, start.z), model.Current.playerStart);
            model.Undo();
            Assert.AreEqual(start, model.Current.playerStart);
        }

        [Test]
        public void UndoRedo_RestoresAndReapplies()
        {
            model.Edit(s => s.grid.width = 20);
            model.Edit(s => s.name = "바뀜");

            model.Undo();
            Assert.AreEqual("첫째", model.Current.name);
            Assert.AreEqual(20, model.Current.grid.width);
            model.Undo();
            Assert.AreEqual(12, model.Current.grid.width);
            Assert.IsFalse(model.CanUndo);

            model.Redo();
            model.Redo();
            Assert.AreEqual("바뀜", model.Current.name);
        }

        [Test]
        public void AddAndDuplicate_GetNextStageNumbers()
        {
            model.AddStage();
            Assert.AreEqual("stage_003", model.Current.id);

            model.DuplicateCurrent();
            Assert.AreEqual("stage_004", model.Current.id);
            Assert.AreEqual(3, model.CurrentIndex, "복제본은 원본 바로 뒤");
        }

        [Test]
        public void Save_WritesFilesAndIndexInOrder()
        {
            model.AddStage();
            model.SelectStage(0);
            model.MoveCurrent(1);

            var report = model.SaveAll();

            CollectionAssert.IsEmpty(report.Blocked);
            Assert.IsTrue(report.IndexSaved);
            CollectionAssert.AreEqual(new[] { "stage_002", "stage_001", "stage_003" }, repo.LoadIndex().stages);
            Assert.IsTrue(repo.LoadStage("stage_003").ChecksumValid);
            Assert.IsFalse(model.HasUnsavedChanges);
        }

        [Test]
        public void Save_BlockedWhenAnyStageHasErrors_AndNothingIsWritten()
        {
            model.Edit(s => s.grid.width = 2);
            model.SelectStage(1);
            model.DeleteCurrent();

            var report = model.SaveAll();

            Assert.AreEqual(1, report.Blocked.Count);
            Assert.IsTrue(repo.Exists("stage_002"), "막혔으면 삭제도 하지 않음");
            CollectionAssert.AreEqual(new[] { "stage_001", "stage_002" }, repo.LoadIndex().stages);
        }

        [Test]
        public void Save_DeletesRemovedStageFiles()
        {
            model.SelectStage(1);
            model.DeleteCurrent();

            var report = model.SaveAll();

            CollectionAssert.AreEqual(new[] { "stage_002" }, report.Deleted);
            Assert.IsFalse(repo.Exists("stage_002"));
            CollectionAssert.AreEqual(new[] { "stage_001" }, repo.LoadIndex().stages);
        }

        [Test]
        public void Save_RenamedStage_MovesFile()
        {
            model.Edit(s => s.id = "stage_intro");

            model.SaveAll();

            Assert.IsTrue(repo.Exists("stage_intro"));
            Assert.IsFalse(repo.Exists("stage_001"));
            CollectionAssert.AreEqual(new[] { "stage_intro", "stage_002" }, repo.LoadIndex().stages);
        }

        [Test]
        public void DuplicateStageId_IsAnError()
        {
            model.Edit(s => s.id = "stage_002");
            Assert.IsTrue(model.ValidateCurrent().Any(i => i.Severity == IssueSeverity.Error && i.Message.Contains("id가 같음")));
        }

        [Test]
        public void Load_ReportsHandEditedFile()
        {
            var path = repo.PathFor("stage_002");
            File.WriteAllText(path, File.ReadAllText(path).Replace("둘째", "손으로 고침"));

            model.Load();

            Assert.IsTrue(model.LoadProblems.Any(p => p.Contains("맵툴 밖에서 수정됨")));
        }
    }
}
