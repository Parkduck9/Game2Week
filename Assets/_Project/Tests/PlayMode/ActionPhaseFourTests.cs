using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Data;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>4단계: 실제 키 입력으로 빨강(Ctrl 정지 자세)·파랑(실제 이동) 판정.</summary>
    public class ActionPhaseFourTests : InputTestFixture
    {
        Keyboard keyboard;

        public override void Setup()
        {
            base.Setup(); TestSave.Begin();
            keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown() { Time.timeScale = 1f; TestSave.End(); base.TearDown(); }

        IEnumerator Enter(System.Action<BattleController> ready)
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null; yield return null;
            ready(battle);
        }

        /// <summary>노랑 연습 탄을 복제해 원하는 색으로 몸을 가로지르게 쏜다 (스테이지 패턴은 멈춤).</summary>
        static Bullet TakeBullet(BattleController battle)
        {
            var pattern = (YellowTrainingPattern)battle.World.Patterns.Current;
            var clone = Object.Instantiate(pattern.Bullets[0], battle.Spawner.Arena.transform);
            battle.World.Patterns.End();
            return clone;
        }

        static bool Cross(Bullet bullet, AttackColor color, PlayerMover player, PatternContext context)
        {
            bullet.Color = color;
            var right = player.transform.right;
            bullet.Launch(player.transform.position + right * .6f + Vector3.up * .4f, -right * 4f);
            bool hit = false;
            for (int i = 0; i < 30 && bullet.Active && !hit; i++) hit = bullet.Tick(.01f, context);
            bullet.Deactivate();
            return hit;
        }

        PatternContext Context(BattleController battle) =>
            new(battle.Spawner.Arena, battle.Spawner.Enemy.transform, battle.Spawner.Enemy,
                battle.World.Player.transform, battle.World.Player.Radius, 4, null, battle.World.Player, battle.World.HitRule);

        /// <summary>회귀: Ctrl을 누른 채 메뉴로 가서 떼면, 다음 탄막 턴에 자세가 남지 않는다 (측정 봇이 찾은 버그).</summary>
        [UnityTest]
        public IEnumerator Ctrl_ReleasedDuringMenu_DoesNotStayBraced()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            var player = battle.World.Player;
            Press(keyboard.leftCtrlKey);
            yield return new WaitForSeconds(.2f);
            Assert.IsTrue(player.Motor.Bracing);

            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            Release(keyboard.leftCtrlKey);
            yield return null; yield return null;
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            Press(keyboard.wKey);
            yield return new WaitForSeconds(.2f);
            Assert.IsFalse(player.Motor.Bracing, "메뉴 중에 뗀 Ctrl은 풀려 있어야 함");
            Assert.Greater(player.GroundSpeed, 1f, "다시 움직일 수 있음");
            Release(keyboard.wKey);

            Press(keyboard.leftCtrlKey); // 메뉴를 거쳐도 실제로 누르고 있으면 다시 자세
            yield return new WaitForSeconds(.2f);
            battle.Context.ChangeState(BattleStateId.ItemMenu); yield return null;
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return new WaitForSeconds(.2f);
            Assert.IsTrue(player.Motor.Bracing, "누른 채로 다음 턴에 들어가면 자세 유지");
            Release(keyboard.leftCtrlKey);
        }

        [UnityTest]
        public IEnumerator Ctrl_Brace_PassesRed_NotYellowOrBlue()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            yield return new WaitForSeconds(.85f);
            var bullet = TakeBullet(battle);
            var player = battle.World.Player;
            var context = Context(battle);
            try
            {
                Assert.IsTrue(Cross(bullet, AttackColor.Red, player, context), "그냥 서 있으면 빨강에 맞음");

                Press(keyboard.leftCtrlKey);
                Press(keyboard.wKey); // 자세 중에는 이동 입력이 무시돼야 함
                yield return new WaitForSeconds(.2f);
                Assert.IsTrue(player.Motor.BraceReady);
                Assert.Less(player.GroundSpeed, .05f, "Ctrl 중 W를 눌러도 멈춰 있음");
                bullet.Color = AttackColor.Red; bullet.Launch(player.transform.position + Vector3.forward * 3f, Vector3.zero);
                yield return new WaitForSeconds(.1f);
                SceneCapture.Save("action_phase4_brace");

                Assert.IsFalse(Cross(bullet, AttackColor.Red, player, context), "정지 자세 → 빨강 통과");
                Assert.Greater(battle.World.HitRule.RedPasses, 0);
                Assert.IsTrue(Cross(bullet, AttackColor.Yellow, player, context), "정지 자세로 노랑은 못 막음");
                Assert.IsTrue(Cross(bullet, AttackColor.Blue, player, context), "정지 자세로 파랑은 못 막음");

                Release(keyboard.leftCtrlKey);
                yield return new WaitForSeconds(.15f);
                Assert.IsFalse(player.Motor.Bracing);
                Assert.Greater(player.GroundSpeed, 1f, "Ctrl을 놓으면 누르고 있던 W로 다시 이동");
                Assert.IsTrue(Cross(bullet, AttackColor.Red, player, context), "움직이면 빨강에 맞음");
                Release(keyboard.wKey);
            }
            finally { if (bullet) Object.Destroy(bullet.gameObject); }
        }

        [UnityTest]
        public IEnumerator Blue_PassesOnlyWhileActuallyMoving()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            yield return new WaitForSeconds(.85f);
            var bullet = TakeBullet(battle);
            var player = battle.World.Player;
            var context = Context(battle);
            var camera = Object.FindAnyObjectByType<BattleCameraDirector>();
            try
            {
                Assert.IsTrue(Cross(bullet, AttackColor.Blue, player, context), "가만히 있으면 파랑에 맞음");

                Press(keyboard.aKey);
                yield return new WaitForSeconds(.15f);
                Assert.Greater(player.GroundSpeed, player.Settings.blueMinSpeed);
                Assert.IsFalse(Cross(bullet, AttackColor.Blue, player, context), "움직이는 중 → 파랑 통과");
                Assert.Greater(battle.World.HitRule.BluePasses, 0);
                Release(keyboard.aKey);

                // 벽에 붙어 입력만 계속 → 실제로는 안 움직이므로 실패
                var into = camera.ToWorldMove(Vector2.left);
                var wall = battle.Spawner.Arena.ClampToArena(player.transform.position + new Vector3(into.x, 0f, into.y) * 50f, player.Radius);
                player.Teleport(wall);
                Press(keyboard.aKey);
                yield return new WaitForSeconds(.15f);
                Assert.Less(player.GroundSpeed, .3f, "벽에 막힘");
                Assert.IsTrue(Cross(bullet, AttackColor.Blue, player, context), "입력만 하고 못 움직이면 파랑에 맞음");
                Release(keyboard.aKey);
            }
            finally { if (bullet) Object.Destroy(bullet.gameObject); }
        }

        [UnityTest]
        public IEnumerator RedTestPattern_BraceHoldsHp_ThenHitsWithoutBrace()
        {
#if UNITY_EDITOR
            var red = UnityEditor.AssetDatabase.LoadAssetAtPath<AttackPatternData>("Assets/_Project/Data/Patterns/ColorTest/Pattern_RedTest.asset");
            Assert.IsNotNull(red, "ColorTest 빨강 패턴 에셋");
            BattleController battle = null; yield return Enter(c => battle = c);
            var player = battle.Context.Player;
            int hp = player.CurrentHp;

            battle.World.BeginPattern(red, 4);
            Press(keyboard.leftCtrlKey);
            yield return new WaitForSeconds(.9f);
            var pattern = (YellowTrainingPattern)battle.World.Patterns.Current;
            Assert.Greater(pattern.ActiveBullets, 0, "빨강 탄 발사됨");
            Assert.AreEqual(AttackColor.Red, pattern.Bullets[0].Color);
            SceneCapture.Save("action_phase4_red_pattern");
            yield return new WaitForSeconds(2.6f);
            Assert.AreEqual(hp, player.CurrentHp, "정지 자세로 빨강을 모두 통과");
            Assert.Greater(battle.World.HitRule.RedPasses, 0, "실제로 몸을 지나감");

            Release(keyboard.leftCtrlKey);
            battle.World.BeginPattern(red, 4);
            yield return new WaitForSeconds(3.5f);
            Assert.Less(player.CurrentHp, hp, "자세 없이 서 있으면 빨강에 맞음");
#else
            Assert.Ignore("에디터 전용");
            yield break;
#endif
        }
    }
}
