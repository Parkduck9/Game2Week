using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class StageTests
    {
        sealed class FakeCatalog : IStageCatalog
        {
            public HashSet<string> Enemies { get; } = new() { "Enemy_Test" };
            public HashSet<string> Patterns { get; } = new() { "Pattern_Test" };
            public bool HasEnemy(string enemyId) => Enemies.Contains(enemyId);
            public bool HasPattern(string patternId) => Patterns.Contains(patternId);
        }

        static StageDefinition ValidStage()
        {
            var stage = new StageDefinition { id = "stage_001", name = "테스트" };
            stage.grid.cellSize = 1.5f; // 시작점-적 11칸 = 16.5m (9단계 권장 거리 경고 없음)
            stage.gems.Add(new StageGem { id = "gem_01", type = GemTypes.Heal, position = new GridPoint(3, 6) });
            stage.gems.Add(new StageGem { id = "gem_02", type = GemTypes.Spare, position = new GridPoint(9, 8) });
            stage.enemyTurn.patterns.Add("Pattern_Test");
            return stage;
        }

        static List<StageIssue> Errors(StageDefinition stage, IStageCatalog catalog = null) =>
            StageValidator.Validate(stage, catalog).Where(i => i.Severity == IssueSeverity.Error).ToList();

        static void AssertSingleError(StageDefinition stage, string expectedFragment, IStageCatalog catalog = null)
        {
            var errors = Errors(stage, catalog);
            Assert.AreEqual(1, errors.Count, string.Join("\n", errors));
            StringAssert.Contains(expectedFragment, errors[0].Message);
        }

        // ---------- JSON / 체크섬 ----------

        [Test]
        public void Json_RoundTrip_KeepsAllFields()
        {
            var original = ValidStage();
            original.grid.width = 20;
            original.gemRules.maxPerTurn = 2;
            original.enemyTurn.duration = 12.5f;

            var copy = StageJson.ReadStage(StageJson.Write(original));

            Assert.AreEqual(20, copy.grid.width);
            Assert.AreEqual("테스트", copy.name);
            Assert.AreEqual(new GridPoint(9, 8), copy.gems[1].position);
            Assert.AreEqual(GemTypes.Spare, copy.gems[1].type);
            Assert.AreEqual(2, copy.gemRules.maxPerTurn);
            Assert.AreEqual(12.5f, copy.enemyTurn.duration);
            CollectionAssert.AreEqual(new[] { "Pattern_Test" }, copy.enemyTurn.patterns);
            Assert.AreEqual(StageJson.ToolName, copy._tool.generatedBy);
        }

        [Test]
        public void Checksum_IsValidRightAfterWrite()
        {
            var copy = StageJson.ReadStage(StageJson.Write(ValidStage()));
            Assert.IsTrue(StageJson.HasValidChecksum(copy));
        }

        [Test]
        public void Checksum_DetectsHandEditedValue()
        {
            var json = StageJson.Write(ValidStage());
            var edited = json.Replace("\"width\": 12", "\"width\": 30");
            Assert.AreNotEqual(json, edited, "테스트 전제: JSON에 width 12가 있어야 함");

            Assert.IsFalse(StageJson.HasValidChecksum(StageJson.ReadStage(edited)));
        }

        [Test]
        public void Checksum_MissingIsInvalid()
        {
            Assert.IsFalse(StageJson.HasValidChecksum(ValidStage()));
        }

        [Test]
        public void Index_RoundTripWithChecksum()
        {
            var index = new StageIndex();
            index.stages.AddRange(new[] { "stage_001", "stage_002" });

            var copy = StageJson.ReadIndex(StageJson.Write(index));

            CollectionAssert.AreEqual(new[] { "stage_001", "stage_002" }, copy.stages);
            Assert.IsTrue(StageJson.HasValidChecksum(copy));
        }

        [Test]
        public void Read_InvalidJson_ThrowsFormatException()
        {
            Assert.Throws<System.FormatException>(() => StageJson.ReadStage("{ not json"));
            Assert.Throws<System.FormatException>(() => StageJson.ReadStage("  "));
        }

        // ---------- 검증 ----------

        [Test]
        public void Validate_ValidStage_HasNoIssues()
        {
            CollectionAssert.IsEmpty(StageValidator.Validate(ValidStage(), new FakeCatalog()));
        }

        [Test]
        public void Validate_DefaultNewStage_HasNoErrors()
        {
            CollectionAssert.IsEmpty(Errors(new StageDefinition()));
        }

        [Test]
        public void Validate_NoPatterns_IsOnlyWarning()
        {
            var stage = ValidStage();
            stage.enemyTurn.patterns.Clear();

            var issues = StageValidator.Validate(stage);

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(IssueSeverity.Warning, issues[0].Severity);
        }

        [TestCase(3, 14)]
        [TestCase(12, 41)]
        public void Validate_GridSizeOutOfRange(int width, int depth)
        {
            var stage = ValidStage();
            stage.grid.width = width;
            stage.grid.depth = depth;
            stage.playerStart = new GridPoint(1, 1);
            stage.enemy.position = new GridPoint(1, 9);
            stage.gems.Clear();
            AssertSingleError(stage, "맵 크기");
        }

        [Test]
        public void Validate_CellSizeOutOfRange()
        {
            var stage = ValidStage();
            stage.grid.cellSize = 5f;
            AssertSingleError(stage, "셀 크기");
        }

        [Test]
        public void Validate_GemOutsideArena()
        {
            var stage = ValidStage();
            stage.gems[0].position = new GridPoint(12, 3);
            AssertSingleError(stage, "경기장 밖");
        }

        [Test]
        public void Validate_GemOnPlayerStart()
        {
            var stage = ValidStage();
            stage.gems[0].position = stage.playerStart;
            AssertSingleError(stage, "같은 칸");
        }

        [Test]
        public void Validate_EnemyTooCloseToPlayer()
        {
            var stage = ValidStage();
            stage.enemy.position = new GridPoint(6, 3);
            AssertSingleError(stage, "떨어져야");
        }

        [Test]
        public void Validate_DuplicateGemId()
        {
            var stage = ValidStage();
            stage.gems[1].id = "gem_01";
            AssertSingleError(stage, "중복");
        }

        [Test]
        public void Validate_UnknownGemType()
        {
            var stage = ValidStage();
            stage.gems[0].type = "gold";
            AssertSingleError(stage, "알 수 없는 종류");
        }

        [TestCase(-0.1f)]
        [TestCase(1.5f)]
        public void Validate_SpawnChanceOutOfRange(float chance)
        {
            var stage = ValidStage();
            stage.gems[0].spawnChance = chance;
            AssertSingleError(stage, "확률");
        }

        [TestCase("Stage 1")]
        [TestCase("")]
        [TestCase("스테이지")]
        public void Validate_BadStageId(string id)
        {
            var stage = ValidStage();
            stage.id = id;
            AssertSingleError(stage, "id");
        }

        [Test]
        public void Validate_TurnDurationOutOfRange()
        {
            var stage = ValidStage();
            stage.enemyTurn.duration = 1f;
            AssertSingleError(stage, "탄막 턴 시간");
        }

        [Test]
        public void Validate_UnknownEnemyAndPatternInCatalog()
        {
            var stage = ValidStage();
            stage.enemy.enemyId = "Enemy_Nope";
            stage.enemyTurn.patterns.Add("Pattern_Nope");

            var errors = Errors(stage, new FakeCatalog());

            Assert.AreEqual(2, errors.Count, string.Join("\n", errors));
        }

        [Test]
        public void Validate_ErrorCarriesCellForHighlighting()
        {
            var stage = ValidStage();
            stage.gems[0].position = new GridPoint(50, 50);
            Assert.AreEqual(new GridPoint(50, 50), Errors(stage)[0].Cell);
        }

        // ---------- 좌표 ----------

        [Test]
        public void Geometry_CellCentersAreSymmetricAroundOrigin()
        {
            var grid = new StageGrid { width = 4, depth = 2, cellSize = 0.5f };

            Assert.AreEqual(new Vector2(2f, 1f), StageGeometry.ArenaSize(grid));
            Assert.AreEqual(new Vector3(-0.75f, 0f, -0.25f), StageGeometry.CellToLocal(grid, new GridPoint(0, 0)));
            Assert.AreEqual(new Vector3(0.75f, 0f, 0.25f), StageGeometry.CellToLocal(grid, new GridPoint(3, 1)));
        }

        [Test]
        public void Geometry_LocalToCell_InvertsCellToLocal()
        {
            var grid = new StageGrid { width = 12, depth = 14, cellSize = 0.5f };
            for (int x = 0; x < grid.width; x++)
            for (int z = 0; z < grid.depth; z++)
            {
                var cell = new GridPoint(x, z);
                Assert.AreEqual(cell, StageGeometry.LocalToCell(grid, StageGeometry.CellToLocal(grid, cell)));
            }
        }

        // ---------- 저장소 ----------

        string tempDir;

        [SetUp]
        public void SetUp() => tempDir = Path.Combine(Path.GetTempPath(), "Game2WeekStageTests_" + System.Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }

        [Test]
        public void Repository_SaveThenLoad()
        {
            var repo = new StageRepository(tempDir);
            repo.SaveStage(ValidStage());
            repo.SaveIndex(new StageIndex { stages = { "stage_001" } });

            var result = repo.LoadStage("stage_001", new FakeCatalog());

            Assert.IsTrue(result.IsUsable, string.Join("\n", result.Issues));
            Assert.IsTrue(result.ChecksumValid);
            CollectionAssert.AreEqual(new[] { "stage_001" }, repo.LoadIndex().stages);
        }

        [Test]
        public void Repository_HandEditedFile_LoadsWithWarning()
        {
            var repo = new StageRepository(tempDir);
            repo.SaveStage(ValidStage());
            var path = repo.PathFor("stage_001");
            File.WriteAllText(path, File.ReadAllText(path).Replace("\"depth\": 14", "\"depth\": 16"));

            var result = repo.LoadStage("stage_001");

            Assert.IsFalse(result.ChecksumValid);
            Assert.IsTrue(result.IsUsable, "체크섬 불일치는 경고일 뿐 로드는 된다");
            StringAssert.Contains("맵툴 밖에서 수정됨", result.Issues[0].Message);
        }

        [Test]
        public void Repository_MissingOrBrokenFile_IsNotUsable()
        {
            var repo = new StageRepository(tempDir);
            Assert.IsFalse(repo.LoadStage("stage_404").IsUsable);

            Directory.CreateDirectory(tempDir);
            File.WriteAllText(repo.PathFor("stage_bad"), "{ broken");
            Assert.IsFalse(repo.LoadStage("stage_bad").IsUsable);
        }

        [Test]
        public void Repository_FileNameMustMatchId()
        {
            var repo = new StageRepository(tempDir);
            repo.SaveStage(ValidStage());
            File.Move(repo.PathFor("stage_001"), repo.PathFor("stage_002"));

            Assert.IsFalse(repo.LoadStage("stage_002").IsUsable);
        }

        [Test]
        public void Repository_MissingIndex_IsEmpty()
        {
            CollectionAssert.IsEmpty(new StageRepository(tempDir).LoadIndex().stages);
        }
    }
}
