using System.Collections;
using Game2Week.Battle.View.Animation;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>
    /// 8.5단계 회귀 — 주인공이 T자 자세로 고정된 채 미끄러지던 버그.
    /// 실제 전투 씬에서 서 있을 때 팔이 내려와 있고, 달릴 때 다리가 앞뒤로 움직여야 한다.
    /// 입력이 아니라 동작을 검사하므로 이동은 PlayerMover에 직접 준다 (키 입력 테스트는 ActionPhase 테스트들이 담당).
    /// </summary>
    public class HeroinePoseRegressionTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() => TestSave.End();

        static Transform Bone(Component root, string name) => System.Array.Find(root.GetComponentsInChildren<Transform>(), t => t.name == name);
        static float DownAngle(Transform from, Transform to) => Vector3.Angle(to.position - from.position, Vector3.down);

        [UnityTest] public IEnumerator 서있을때팔이내려오고_달릴때다리가움직인다()
        {
            BattleController battle = null; yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var player = battle.World.Player;
            var driver = player.GetComponent<PlayerAnimationDriver>();
            Assert.IsTrue(driver.IsReady);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(PlayerMotion.Idle, driver.Current);
            foreach (var side in new[] { "Right", "Left" })
            {
                float angle = DownAngle(Bone(driver, side + "UpperArm"), Bone(driver, side + "ForeArm"));
                Assert.Less(angle, 30f, $"{side} 팔이 아래로 내려와야 한다 (T자 자세 아님). 지금 {angle:F0}°");
            }
            SceneCapture.Save("heroine_pose_idle");

            var hip = Bone(driver, "RightUpperLeg"); var knee = Bone(driver, "RightLowerLeg");
            float min = float.MaxValue, max = float.MinValue, runSeconds = 0f;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                player.Move(Vector2.right, Time.deltaTime, null);
                yield return null;
                if (driver.Current != PlayerMotion.Run) continue;
                runSeconds += Time.deltaTime;
                var swing = driver.transform.InverseTransformDirection(knee.position - hip.position).normalized.z;
                min = Mathf.Min(min, swing); max = Mathf.Max(max, swing);
            }
            Assert.Greater(runSeconds, 0.4f, $"이동 중에는 달리기 동작이어야 한다. 마지막 동작 {driver.Current}");
            SceneCapture.Save("heroine_pose_run");
            Assert.Greater(max - min, 0.6f, $"달리기 중 허벅지가 앞뒤로 움직여야 한다. 범위 {min:F2}~{max:F2}");
        }
    }
}
