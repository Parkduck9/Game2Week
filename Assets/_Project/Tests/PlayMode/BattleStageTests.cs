using System.Collections;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>Battle 씬이 stage_001 데이터대로 경기장·주인공·적·보석을 만드는지, 카메라 샷별 화면 캡처.</summary>
    public class BattleStageTests
    {
        [UnityTest]
        public IEnumerator Battle_BuildsArenaFromStage()
        {
            SceneManager.LoadScene(SceneNames.Battle);
            float start = Time.realtimeSinceStartup;
            BattleStageBootstrap boot = null;
            while (boot == null || !boot.IsReady)
            {
                if (Time.realtimeSinceStartup - start > 10f) Assert.Fail("Battle 준비 시간 초과");
                boot = Object.FindAnyObjectByType<BattleStageBootstrap>();
                yield return null;
            }

            var spawner = boot.Spawner;
            var stage = new StageRepository(StageRepository.DefaultDirectory).LoadStage("stage_001").Stage;

            Assert.AreEqual(StageGeometry.ArenaSize(stage.grid), spawner.Arena.Size);
            Assert.AreEqual(stage.gems.Count, spawner.Gems.Count);
            Assert.That(Vector3.Distance(spawner.Arena.CellToWorld(stage.playerStart), spawner.Player.position), Is.LessThan(0.01f));
            Assert.IsNotNull(spawner.Enemy, "적에 EnemyView가 있어야 함");

            var clamped = spawner.Arena.ClampToArena(new Vector3(100f, 0f, -100f), 0.2f);
            Assert.That(clamped.x, Is.LessThanOrEqualTo(spawner.Arena.Size.x * 0.5f));
            Assert.That(clamped.z, Is.GreaterThanOrEqualTo(-spawner.Arena.Size.y * 0.5f));

            boot.ShowAllGemsForPreview = true;
            boot.RefreshGems();

            foreach (var shot in new[] { BattleShot.Overview, BattleShot.EnemyFocus, BattleShot.Intro })
            {
                boot.CameraDirector.Show(shot);
                yield return new WaitForSeconds(1f); // 카메라 블렌드
                SceneCapture.Save($"battle_{shot}");
            }

            boot.CameraDirector.Show(BattleShot.AttackCloseUp);
            yield return new WaitForSeconds(1f);
            SceneCapture.Save("battle_AttackCloseUp");
            spawner.Enemy.PlayHit();
            Object.FindAnyObjectByType<BattleEffects>().PlayHit(spawner.Enemy.transform.position + Vector3.up * 0.6f);
            yield return new WaitForSeconds(0.08f);
            SceneCapture.Save("battle_AttackCloseUp_Hit");
        }
    }
}
