using System.IO;
using System.Linq;
using Game2Week.EditorTools.Stages;
using Game2Week.Stages;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class StageEditorBatchTests
    {
        string dir;StageEditorModel model;StageRepository repo;
        [SetUp] public void 준비()
        {
            dir=Path.Combine(Path.GetTempPath(),"StageBatch_"+System.Guid.NewGuid().ToString("N"));repo=new StageRepository(dir);
            repo.SaveStage(new StageDefinition());repo.SaveIndex(new StageIndex{stages={"stage_001"}});model=new StageEditorModel(repo);model.Load();
        }
        [TearDown] public void 정리(){Directory.Delete(dir,true);}
        [Test] public void 영역배치중복방지되돌리기와체크섬()
        {
            var start=model.Current.playerStart;
            Assert.AreEqual(8,model.EditGemRegion(new GridPoint(5,0),new GridPoint(7,2),false));
            Assert.AreEqual(0,model.EditGemRegion(new GridPoint(5,0),new GridPoint(7,2),false));
            Assert.AreEqual(8,model.Current.gems.Select(g=>g.id).Distinct().Count());Assert.AreEqual(start,model.Current.playerStart);
            model.Undo();Assert.IsEmpty(model.Current.gems);model.Redo();Assert.AreEqual(8,model.Current.gems.Count);
            model.SaveAll();Assert.IsTrue(repo.LoadStage("stage_001").ChecksumValid);
        }
        [Test] public void 영역삭제는시작점적을보존하며대칭은빈칸만복사()
        {
            model.Tool=StageTool.Gem;model.Click(new GridPoint(1,2));model.Current.gems[0].type=GemTypes.Attack;
            Assert.AreEqual(1,model.MirrorGems(true));Assert.AreEqual(GemTypes.Attack,model.Current.gems[1].type);
            Assert.AreEqual(0,model.MirrorGems(true));model.Undo();Assert.AreEqual(1,model.Current.gems.Count);
            Assert.AreEqual(1,model.EditGemRegion(new GridPoint(-10,-10),new GridPoint(100,100),true));
            Assert.AreEqual(new GridPoint(6,1),model.Current.playerStart);Assert.AreEqual(new GridPoint(6,12),model.Current.enemy.position);
            model.Undo();Assert.AreEqual(1,model.Current.gems.Count);
        }
    }
}
