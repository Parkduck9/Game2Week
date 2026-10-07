using System;
using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Data;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game2Week.Tests
{
    /// <summary>전투 상태 흐름 — 가짜 UI/World로 플레이어 행동을 흉내 낸다.</summary>
    public class BattleFlowTests
    {
        sealed class FakeUi : IBattleUi
        {
            public string Dialogue;
            public Action CloseDialogueAction;
            public string BoxText;
            public IReadOnlyList<string> MainItems;
            public Action<int> MainSelected;
            public IReadOnlyList<string> ListItems;
            public Action<int> ListSelected;
            public Action ListCancelled;
            public string Popup;
            public bool HudVisible;
            public Action<float?> GaugeFinished;

            public void ShowTimingGauge(Action<float?> onFinished) => GaugeFinished = onFinished;

            /// <summary>게이지에서 누른 정확도 (null = 놓침)</summary>
            public void CompleteGauge(float? accuracy)
            {
                var a = GaugeFinished;
                GaugeFinished = null;
                a(accuracy);
            }

            public void ShowDialogue(string text, Action onClosed) { Dialogue = text; CloseDialogueAction = onClosed; MainItems = ListItems = null; }
            public void ShowBoxText(string text) => BoxText = text;
            public void ShowMainMenu(IReadOnlyList<string> items, Action<int> onSelected) { MainItems = items; MainSelected = onSelected; }
            public void ShowListMenu(IReadOnlyList<string> items, Action<int> onSelected, Action onCancel) { ListItems = items; ListSelected = onSelected; ListCancelled = onCancel; }
            public void ShowTurnHud(string hint) => HudVisible = true;
            public void UpdateTurnHud(float remainingRatio) { }
            public void ShowPopup(string text) => Popup = text;
            public void HideAll() { HudVisible = false; Dialogue = null; MainItems = ListItems = null; }

            public void CloseDialogue()
            {
                var a = CloseDialogueAction;
                CloseDialogueAction = null;
                a();
            }

            public void ChooseMain(int i) { var a = MainSelected; MainItems = null; a(i); }
            public void ChooseList(int i) { var a = ListSelected; ListItems = null; a(i); }
            public void CancelList() { var a = ListCancelled; ListItems = null; a(); }
        }

        sealed class FakeWorld : IBattleWorld
        {
            public bool Touching;
            public string GemUnderPlayer;
            public IReadOnlyList<string> ShownGems = Array.Empty<string>();
            public readonly List<string> Collected = new();
            public int Resets;

            public void ResetPlayer() => Resets++;
            public void MovePlayer(Vector2 input, float deltaTime) { }
            public bool IsPlayerTouchingEnemy() => Touching;
            public string TouchedGem(IReadOnlyList<string> visible) => visible.Contains(GemUnderPlayer) ? GemUnderPlayer : null;
            public void ShowGems(IReadOnlyList<string> gemIds) => ShownGems = gemIds.ToList();
            public void CollectGem(string gemId) => Collected.Add(gemId);

            public AttackPatternData Pattern;
            public int DamagePerHit;
            public int PendingDamage;
            public bool PatternRunning;

            public Battle.Patterns.EnemyPatternInfo EnemyInfo;

            public void BeginPattern(AttackPatternData pattern, int damagePerHit, Battle.Patterns.EnemyPatternInfo enemy = null)
            {
                Pattern = pattern;
                EnemyInfo = enemy;
                DamagePerHit = damagePerHit;
                PatternRunning = true;
            }

            public int ConsumePlayerDamage()
            {
                int d = PendingDamage;
                PendingDamage = 0;
                return d;
            }

            public void EndPattern() => PatternRunning = false;
        }

        readonly List<Object> created = new();
        FakeUi ui;
        FakeWorld world;
        BattleStateMachine machine;
        BattleContext context;
        BattleOutcome? finished;
        ItemData bandage;

        T Create<T>(Action<SerializedObject> setup = null) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            if (setup == null) return asset;
            var so = new SerializedObject(asset);
            setup(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        [SetUp]
        public void SetUp()
        {
            bandage = Create<ItemData>(so => so.FindProperty("healAmount").intValue = 10);
            var playerData = Create<PlayerData>(so =>
            {
                so.FindProperty("maxHp").intValue = 20;
                so.FindProperty("attack").intValue = 10;
                var items = so.FindProperty("startingItems");
                items.arraySize = 1;
                items.GetArrayElementAtIndex(0).objectReferenceValue = bandage;
            });
            var enemyData = Create<EnemyData>(so =>
            {
                so.FindProperty("maxHp").intValue = 15;
                so.FindProperty("spareThreshold").intValue = 1;
                var acts = so.FindProperty("acts");
                acts.arraySize = 1;
                acts.GetArrayElementAtIndex(0).FindPropertyRelative("spareProgress").intValue = 1;
            });

            var stage = new StageDefinition();
            stage.enemyTurn.duration = 3f;
            stage.gems.Add(new StageGem { id = "gem_01", type = GemTypes.Heal, position = new GridPoint(2, 5), spawnChance = 1f });

            ui = new FakeUi();
            world = new FakeWorld();
            machine = new BattleStateMachine();
            finished = null;
            context = new BattleContext(machine, new BattleEvents(), null, ui, world, stage,
                new PlayerCombatant(playerData), new EnemyCombatant(enemyData),
                new GemField(stage.gems, 1, new System.Random(1)), Create<GemRewardSettings>(), o => finished = o);
            BattleStates.RegisterAll(machine, context);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        void StartEnemyTurn()
        {
            machine.ChangeState(BattleStateId.Intro);
            ui.CloseDialogue();
            Assert.AreEqual(BattleStateId.EnemyTurn, machine.CurrentId);
        }

        [Test]
        public void Intro_ThenEnemyTurn_ResetsPlayerAndShowsHud()
        {
            StartEnemyTurn();
            Assert.AreEqual(1, world.Resets);
            Assert.IsTrue(ui.HudVisible);
        }

        [Test]
        public void TouchingEnemy_OpensActionMenu()
        {
            StartEnemyTurn();
            world.Touching = true;
            machine.Tick(0.1f);

            Assert.AreEqual(BattleStateId.ActionMenu, machine.CurrentId);
            CollectionAssert.AreEqual(BattleTexts.ActionMenu, ui.MainItems);
            CollectionAssert.IsEmpty(world.ShownGems, "턴이 끝나면 보석은 사라짐");
        }

        [Test]
        public void TimeOut_OpensItemMenu()
        {
            StartEnemyTurn();
            machine.Tick(1.5f);
            Assert.AreEqual(BattleStateId.EnemyTurn, machine.CurrentId);
            machine.Tick(1.6f);

            Assert.AreEqual(BattleStateId.ItemMenu, machine.CurrentId);
            CollectionAssert.AreEqual(BattleTexts.ItemMenu, ui.MainItems);
        }

        [Test]
        public void GemPickup_AppliesRewardOnce()
        {
            context.Player.TakeDamage(10);
            StartEnemyTurn();
            CollectionAssert.Contains(world.ShownGems, "gem_01");

            world.GemUnderPlayer = "gem_01";
            machine.Tick(0.1f);
            machine.Tick(0.1f);

            Assert.AreEqual(15, context.Player.CurrentHp, "회복 5 (기본값), 두 번 먹지 않음");
            CollectionAssert.AreEqual(new[] { "gem_01" }, world.Collected);
            Assert.IsNotEmpty(ui.Popup);
        }

        [Test]
        public void Fight_DamagesEnemy_AndKillsGoToVictory()
        {
            StartEnemyTurn();
            world.Touching = true;
            machine.Tick(0.1f);
            ui.ChooseMain(0);
            ui.CompleteGauge(0f); // 게이지 끝자락: 공격 10 × 0.5 = 5 → 적 HP 15 → 10

            Assert.AreEqual(10, context.Enemy.CurrentHp);
            Assert.AreEqual("-5", ui.Popup);
            ui.CloseDialogue();
            Assert.AreEqual(BattleStateId.EnemyTurn, machine.CurrentId);

            machine.Tick(0.1f); // 아직 닿아 있음 → 메뉴
            ui.ChooseMain(0);
            ui.CompleteGauge(1f); // 정중앙: 10 × 2 = 20
            ui.CloseDialogue();
            Assert.AreEqual(BattleStateId.Victory, machine.CurrentId);
            ui.CloseDialogue();
            Assert.AreEqual(BattleOutcome.EnemyDefeated, finished);
        }

        [Test]
        public void ActThenMercy_SparesEnemy()
        {
            StartEnemyTurn();
            world.Touching = true;
            machine.Tick(0.1f);

            ui.ChooseMain(2); // 자비 — 아직 안 됨
            ui.ChooseList(0);
            StringAssert.Contains("아직", ui.Dialogue);
            ui.CloseDialogue();

            machine.Tick(0.1f);
            ui.ChooseMain(1); // 행동
            Assert.AreEqual(BattleTexts.Check, ui.ListItems[0]);
            ui.ChooseList(1);
            StringAssert.Contains(BattleTexts.NowSpareable, ui.Dialogue);
            ui.CloseDialogue();

            machine.Tick(0.1f);
            ui.ChooseMain(2);
            StringAssert.Contains(BattleTexts.SpareReadyColor, ui.ListItems[0]);
            ui.ChooseList(0);
            ui.CloseDialogue();
            Assert.AreEqual(BattleOutcome.EnemySpared, finished);
        }

        [Test]
        public void CancelInActList_ReturnsToActionMenu()
        {
            StartEnemyTurn();
            world.Touching = true;
            machine.Tick(0.1f);
            ui.ChooseMain(1);
            ui.CancelList();

            Assert.AreEqual(BattleStateId.ActionMenu, machine.CurrentId);
        }

        [Test]
        public void Item_HealsAndIsConsumed_ThenEmptyMessage()
        {
            context.Player.TakeDamage(15);
            StartEnemyTurn();
            machine.Tick(5f);
            ui.ChooseMain(0); // 아이템
            ui.ChooseList(0);

            Assert.AreEqual(15, context.Player.CurrentHp);
            Assert.IsTrue(context.Player.Inventory.IsEmpty);
            ui.CloseDialogue();

            machine.Tick(5f);
            ui.ChooseMain(0);
            Assert.AreEqual(BattleTexts.NoItems, ui.Dialogue);
            ui.CloseDialogue();
            Assert.AreEqual(BattleStateId.ItemMenu, machine.CurrentId);
        }

        [Test]
        public void Skip_GoesBackToEnemyTurn()
        {
            StartEnemyTurn();
            machine.Tick(5f);
            ui.ChooseMain(1); // 넘기기
            ui.CloseDialogue();

            Assert.AreEqual(BattleStateId.EnemyTurn, machine.CurrentId);
        }

        [Test]
        public void PlayerAtZeroHp_DuringEnemyTurn_IsDefeat()
        {
            StartEnemyTurn();
            context.Player.TakeDamage(999);
            machine.Tick(0.1f);
            ui.CloseDialogue();

            Assert.AreEqual(BattleOutcome.PlayerDefeated, finished);
        }

        [Test]
        public void FightMiss_DealsNoDamage_ButUsesUpGemBoost()
        {
            StartEnemyTurn();
            context.Player.GrantAttackBoost(1.5f);
            world.Touching = true;
            machine.Tick(0.1f);
            ui.ChooseMain(0);
            ui.CompleteGauge(null);

            Assert.AreEqual(15, context.Enemy.CurrentHp);
            Assert.AreEqual(BattleTexts.Miss, ui.Popup);
            Assert.AreEqual(1f, context.Player.NextAttackMultiplier);
        }

        [Test]
        public void EnemyTurn_StartsPattern_AndBulletHitsDamagePlayer()
        {
            StartEnemyTurn();
            Assert.IsTrue(world.PatternRunning);
            Assert.AreEqual(4, world.DamagePerHit, "적 공격 4 − 주인공 방어 0");

            world.PendingDamage = 4;
            machine.Tick(0.1f);
            Assert.AreEqual(16, context.Player.CurrentHp);

            world.Touching = true;
            machine.Tick(0.1f);
            Assert.IsFalse(world.PatternRunning, "턴이 끝나면 탄막 정리");
        }

        [Test]
        public void Formulas()
        {
            Assert.AreEqual(0, BattleFormulas.FightDamage(10, null, 1f, 0), "놓치면 0");
            Assert.AreEqual(5, BattleFormulas.FightDamage(10, 0f, 1f, 0));
            Assert.AreEqual(20, BattleFormulas.FightDamage(10, 1f, 1f, 0));
            Assert.AreEqual(30, BattleFormulas.FightDamage(10, 1f, 1.5f, 0), "보석 강화 ×1.5");
            Assert.AreEqual(1, BattleFormulas.FightDamage(1, 0f, 1f, 50), "맞히면 최소 1");
            Assert.AreEqual(1, BattleFormulas.BulletDamage(2, 10));
            Assert.AreEqual(4, BattleFormulas.BulletDamage(4, 0));
        }

        [Test]
        public void TimingGauge_AccuracyAndMiss()
        {
            var gauge = new TimingGauge(1f);
            gauge.Advance(0.5f);
            gauge.Press();
            Assert.AreEqual(1f, gauge.Accuracy.Value, 0.001f, "정중앙");

            var late = new TimingGauge(1f);
            late.Advance(0.9f);
            late.Press();
            Assert.AreEqual(0.2f, late.Accuracy.Value, 0.001f);

            var missed = new TimingGauge(1f);
            missed.Advance(1.2f);
            Assert.IsTrue(missed.IsFinished);
            Assert.IsNull(missed.Accuracy);
            missed.Press();
            Assert.IsNull(missed.Accuracy, "끝난 뒤 누르면 무시");

            Assert.AreEqual(0.5f, TimingGauge.Multiplier(0f));
            Assert.AreEqual(2f, TimingGauge.Multiplier(1f));
        }

        [Test]
        public void RadialBurst_DirectionsAreEvenAndRotate()
        {
            var dirs = Game2Week.Battle.Patterns.RadialBurstPattern.Directions(4, 0f);
            Assert.AreEqual(4, dirs.Length);
            Assert.That(Vector3.Distance(dirs[0], Vector3.forward), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(dirs[1], Vector3.right), Is.LessThan(0.001f));
            var rotated = Game2Week.Battle.Patterns.RadialBurstPattern.Directions(4, 90f);
            Assert.That(Vector3.Distance(rotated[0], Vector3.right), Is.LessThan(0.001f));
        }
    }
}
