using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>병렬 작업 연결 지점: 궤적 교체(ITrajectory), 색(AttackColor), 피해 규칙(IHitRule).</summary>
    public class BulletContractTests
    {
        readonly List<Object> created = new();

        sealed class SideStep : ITrajectory
        {
            public Vector3 Evaluate(in TrajectoryLaunch launch, float elapsed) =>
                launch.Origin + launch.Forward * launch.Speed * elapsed + launch.Right * elapsed;
        }

        sealed class FixedRule : IHitRule
        {
            public AttackColor Passes;
            public bool ShouldHit(AttackColor color) => color != Passes;
        }

        T Make<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            created.Add(go);
            return go.TryGetComponent<T>(out var existing) ? existing : go.AddComponent<T>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) if (o) Object.DestroyImmediate(o);
            created.Clear();
        }

        PatternContext Context(Transform player, IHitRule rule)
        {
            var arena = Make<BattleArena>("Arena");
            // Build가 프리미티브 콜라이더를 Destroy로 지움 → 에디트 모드 경고는 테스트와 무관
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            arena.Build(new StageGrid { width = 20, depth = 20, cellSize = 0.5f });
            return new PatternContext(arena, arena.transform, null, player, 0.22f, 1, null, null, rule);
        }

        [Test]
        public void TrajectoryLaunch_FreezesFlatAxes()
        {
            var launch = new TrajectoryLaunch(new Vector3(1f, 0.3f, 2f), new Vector3(0f, 5f, 3f));
            Assert.AreEqual(3f, launch.Speed, 1e-4f);
            Assert.AreEqual(Vector3.forward, launch.Forward);
            Assert.AreEqual(Vector3.right, launch.Right);
        }

        [Test]
        public void Bullet_FollowsTrajectory_FromFrozenOrigin()
        {
            var player = Make<Transform>("Player");
            player.position = new Vector3(0f, 0f, -4f);
            var context = Context(player, null);
            var bullet = Make<Bullet>("Bullet");
            bullet.Launch(new Vector3(0f, 0.3f, 2f), Vector3.forward * 2f, new SideStep());
            bullet.Tick(0.5f, context);
            Assert.That(Vector3.Distance(new Vector3(0.5f, 0.3f, 3f), bullet.transform.position), Is.LessThan(1e-3f));
        }

        [Test]
        public void HitRule_DecidesByColor_AndOnlyYellowIsParryable()
        {
            var player = Make<Transform>("Player");
            var rule = new FixedRule { Passes = AttackColor.Red };
            var context = Context(player, rule);
            var bullet = Make<Bullet>("Bullet");

            bullet.Color = AttackColor.Red;
            bullet.Launch(new Vector3(0f, 0.3f, 0.5f), Vector3.back * 2f);
            Assert.IsFalse(bullet.Tick(0.5f, context), "규칙이 통과시킨 색은 겹쳐도 피해 없음");
            Assert.IsFalse(bullet.Parryable);

            bullet.Color = AttackColor.Blue;
            bullet.Launch(new Vector3(0f, 0.3f, 0.5f), Vector3.back * 2f);
            Assert.IsTrue(bullet.Tick(0.5f, context), "다른 색은 겹치면 피해");

            Assert.IsTrue(new PatternContext(null, null, null, null, 0f, 1, null).ShouldHit(AttackColor.Red), "규칙이 없으면 항상 피해");
        }
    }
}
