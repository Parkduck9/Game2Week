using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>맵 하나의 자동 플레이 측정 결과 (리포트 JSON 한 줄).</summary>
    [Serializable]
    public sealed class StageBalanceStats
    {
        public string stageId, stageName, outcome;
        public int enemyTurns, contacts, hits, dodges, jumps, parries, braces, redPasses, bluePasses, hpLeft, hpMax;
        public float averageApproachSeconds, battleSeconds;
    }

    [Serializable]
    public sealed class BalanceReport
    {
        public string createdAt;
        public string note = "자동 봇 측정 — 사람 플레이 체감을 대신하지 않음. 봇: 적에게 직진, 노랑은 가까우면 쳐내기/옆 회피, 빨강은 Ctrl 정지, 파랑은 계속 이동, 메뉴에선 공격.";
        public List<StageBalanceStats> stages = new();
    }

    /// <summary>
    /// 5단계 자동 플레이 측정 도구. 실제 키 입력으로 맵을 플레이하며 BattleFeedback·상태 변화를 센다.
    /// 전체 측정(8개 맵)은 시간이 걸려 [Explicit] — 따로 실행: -testFilter Game2Week.Tests.BalanceMeasurementTests.MeasureAllStages
    /// 결과: Logs/balance_report.json → node Tools/render_balance_report.mjs → Plans/Balance_Report.html
    /// </summary>
    public class BalanceMeasurementTests : InputTestFixture
    {
        // 사람처럼: 빨강은 몸에 닿기 직전에만 멈추고, 적 바로 앞에서는 밀고 들어간다. 노랑은 쳐내기 판정 구간에 맞춰 늦게.
        const float RedReactDistance = 1.3f;
        const float YellowReactDistance = 0.75f;
        const float PushThroughEnemyDistance = 1.5f;
        const float SpeedUp = 2f;

        Keyboard keyboard;
        Mouse mouse;
        bool holdW, holdCtrl;

        void HoldW(bool want) { if (want == holdW) return; if (want) Press(keyboard.wKey); else Release(keyboard.wKey); holdW = want; }
        void HoldCtrl(bool want) { if (want == holdCtrl) return; if (want) Press(keyboard.leftCtrlKey); else Release(keyboard.leftCtrlKey); holdCtrl = want; }

        public override void Setup()
        {
            base.Setup();
            TestSave.Begin();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            holdW = holdCtrl = false;
        }

        public override void TearDown()
        {
            Time.timeScale = 1f;
            TestSave.End();
            base.TearDown();
        }

        static string ReportPath(string file) => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Logs", file);

        [UnityTest]
        public IEnumerator Stage1_ShortRun_ProducesReport()
        {
            var report = new BalanceReport { createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            yield return Measure(0, 2, 40f, report);
            var stats = report.stages[0];
            Assert.AreEqual("stage_001", stats.stageId);
            Assert.GreaterOrEqual(stats.enemyTurns, 1);
            Assert.Greater(stats.contacts + stats.hits + stats.parries + stats.dodges, 0, "봇이 무언가를 했어야 함");
            Write(report, "balance_report_smoke.json");
        }

        /// <summary>8개 맵 전체 측정 — 수 분 걸려 명령줄에 -runBalance가 있을 때만 (Unity 배치 실행은 Explicit도 돌림)</summary>
        [UnityTest]
        public IEnumerator MeasureAllStages()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-runBalance") < 0) Assert.Ignore("전체 측정은 -runBalance로 실행");
            var report = new BalanceReport { createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            int count = new StageRepository(StageRepository.DefaultDirectory).LoadIndex().stages.Count;
            for (int i = 0; i < count; i++) yield return Measure(i, 8, 90f, report);
            Write(report, "balance_report.json");
            Assert.AreEqual(count, report.stages.Count);
        }

        static void Write(BalanceReport report, string file)
        {
            var path = ReportPath(file);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log($"[Balance] 리포트 저장: {path}");
        }

        IEnumerator Tap(ButtonControl key)
        {
            PressAndRelease(key);
            yield return null;
            yield return null;
        }

        IEnumerator Measure(int index, int maxTurns, float realtimeLimit, BalanceReport report)
        {
            Time.timeScale = 1f;
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(index, c => battle = c);
            var stats = new StageBalanceStats { stageId = battle.Context.Stage.id, stageName = battle.Context.Stage.name, outcome = "진행 중(턴 한도)" };
            var feedback = battle.World.Feedback;
            feedback.PlayerHit += _ => stats.hits++;
            feedback.PlayerDodged += _ => stats.dodges++;
            feedback.PlayerJumped += _ => stats.jumps++;
            feedback.PlayerParried += (_, _) => stats.parries++;
            feedback.PlayerBraced += _ => stats.braces++;
            feedback.ColorPassed += (c, _) => { if (c == AttackColor.Red) stats.redPasses++; else stats.bluePasses++; };
            float turnStart = 0f, approachTotal = 0f, battleStart = Time.time;
            battle.Context.Events.StateChanged += id =>
            {
                if (id == BattleStateId.EnemyTurn) { stats.enemyTurns++; turnStart = Time.time; }
                else if (id == BattleStateId.ActionMenu && turnStart > 0f) { stats.contacts++; approachTotal += Time.time - turnStart; turnStart = 0f; }
                else if (id == BattleStateId.ItemMenu) turnStart = 0f;
            };
            stats.hpMax = battle.Context.Player.MaxHp;

            Time.timeScale = SpeedUp;
            var camera = battle.CameraDirector;
            var threats = new List<ThreatPoint>();
            HoldW(false);
            HoldCtrl(false);
            float start = Time.realtimeSinceStartup, nextTrace = 0f;

            while (battle && SceneManager.GetActiveScene().name == SceneNames.Battle)
            {
                stats.hpLeft = battle.Context.Player.CurrentHp;
                if (Debug.isDebugBuild && Time.time >= nextTrace && battle.World.Player)
                {
                    nextTrace = Time.time + 1f;
                    var pp = battle.World.Player.transform.position;
                    Debug.Log($"[BalanceTrace] {stats.stageId} t={Time.time - battleStart:0.0} {battle.Context.CurrentState} hp={stats.hpLeft} 적거리={Vector3.Distance(pp, battle.Spawner.Enemy.transform.position):0.00} W={holdW} Ctrl={holdCtrl} 자세={battle.World.Player.Motor.Bracing} 속도={battle.World.Player.GroundSpeed:0.0} 록온={camera && camera.IsLockedOn} 모드={battle.Context.Input?.CurrentMode}");
                }
                if (stats.enemyTurns > maxTurns || Time.realtimeSinceStartup - start > realtimeLimit) break;
                var state = battle.Context.CurrentState;
                var ui = battle.Ui;

                if (state != BattleStateId.EnemyTurn)
                {
                    HoldW(false);
                    HoldCtrl(false);
                }
                if (ui.Dialogue.IsWaitingForInput) { yield return Tap(keyboard.zKey); continue; }

                switch (state)
                {
                    case BattleStateId.EnemyTurn:
                        if (camera && !camera.IsLockedOn) { yield return Tap(mouse.middleButton); continue; }
                        var player = battle.World.Player;
                        threats.Clear();
                        if (battle.World.Patterns.Current is IThreatSource source) source.CollectThreats(threats);
                        bool red = false, yellowNear = false;
                        bool nearEnemy = Vector3.Distance(player.transform.position, battle.Spawner.Enemy.transform.position) < PushThroughEnemyDistance;
                        foreach (var t in threats)
                        {
                            float d = Vector3.Distance(t.Position, player.transform.position);
                            // 빨강은 앞(적 쪽)에서 다가오는 것만 — 이미 몸을 지나간 탄 때문에 계속 서 있지 않게
                            bool ahead = Vector3.Dot(t.Position - player.transform.position, battle.Spawner.Enemy.transform.position - player.transform.position) > 0f;
                            if (t.Color == AttackColor.Red && d < RedReactDistance && ahead && !nearEnemy) red = true;
                            if (t.Color == AttackColor.Yellow && d < YellowReactDistance) yellowNear = true;
                        }
                        HoldCtrl(red);
                        HoldW(!red);
                        if (!red && yellowNear)
                        {
                            if (player.Motor.ParryCooldown <= 0f) { yield return Tap(mouse.rightButton); continue; }
                            if (player.Motor.DodgeCooldown <= 0f)
                            {
                                Press(keyboard.aKey); yield return null;
                                yield return Tap(keyboard.leftShiftKey);
                                Release(keyboard.aKey);
                                continue;
                            }
                        }
                        break;

                    case BattleStateId.ActionMenu when ui.IsMainMenuOpen:
                        yield return Tap(keyboard.zKey); // 공격
                        continue;

                    case BattleStateId.Fight when ui.TimingGauge.IsOpen && ui.TimingGauge.Position >= 0.47f:
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.ItemMenu when ui.IsMainMenuOpen:
                        yield return Tap(keyboard.rightArrowKey); // 넘기기
                        yield return Tap(keyboard.zKey);
                        continue;
                }
                yield return null;
            }

            HoldW(false);
            HoldCtrl(false);
            stats.battleSeconds = Time.time - battleStart;
            stats.averageApproachSeconds = stats.contacts > 0 ? approachTotal / stats.contacts : 0f;
            if (SceneManager.GetActiveScene().name != SceneNames.Battle || (battle && battle.Context.CurrentState is BattleStateId.Victory or BattleStateId.Defeat))
                stats.outcome = stats.hpLeft <= 0 ? "패배" : "처치";
            Time.timeScale = 1f;
            report.stages.Add(stats);
            Debug.Log($"[Balance] {stats.stageId}: 턴 {stats.enemyTurns}, 접근 {stats.contacts} (평균 {stats.averageApproachSeconds:0.0}초), 피격 {stats.hits}, 회피 {stats.dodges}, 쳐내기 {stats.parries}, 자세 {stats.braces}, 통과 빨강 {stats.redPasses}/파랑 {stats.bluePasses}, {stats.outcome}");
        }
    }
}
