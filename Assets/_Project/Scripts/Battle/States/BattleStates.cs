using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game2Week.Battle
{
    // 전투 상태들. 각 상태는 Context만 쓰고, 다음 상태는 BattleStateId로만 요청한다.

    /// <summary>등장 문구 → 탄막 턴</summary>
    public sealed class IntroState : BattleStateBase
    {
        public IntroState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            Context.Ui.HideAll();
            if (Context.Dialogues?.TryStart(DialogueTrigger.Intro, () => Context.ChangeState(BattleStateId.EnemyTurn)) == true) return;
            Context.Ui.ShowDialogue(Context.Enemy.Data.EncounterText, () => Context.ChangeState(BattleStateId.EnemyTurn));
        }
    }

    /// <summary>
    /// 탄막 턴: 주인공이 경기장을 움직인다. 적에게 닿으면 행동 메뉴, 시간이 다 되면 아이템 메뉴.
    /// 보석을 주우면 보상. (탄막 패턴은 09단계에서 이 상태에 붙인다)
    /// </summary>
    public sealed class EnemyTurnState : BattleStateBase
    {
        float elapsed;
        float duration;

        public EnemyTurnState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            elapsed = 0f;
            Context.Enemy.Spare.BeginBulletTurn();
            duration = Mathf.Max(0.1f, Context.Stage.enemyTurn.duration);
            Context.World.ResetPlayer();
            Context.World.ShowGems(Context.Gems.RollForTurn());
            Context.Ui.HideAll();
            var line = Context.Enemy.NextEnemyTurnLine();
            if (Context.Enemy.Spare.UsesRule) line = BattleTexts.SpareHearts(Context.Enemy.Spare) + " " + line;
            Context.Ui.ShowTurnHud(string.IsNullOrEmpty(line) ? BattleTexts.TurnHint : $"{Context.Enemy.Data.DisplayName}: \"{line}\"   ·   {BattleTexts.TurnHint}");
            if (!string.IsNullOrEmpty(line)) Context.Events.RaiseEnemySpoke(line);
            var enemy = Context.Enemy;
            var enemyInfo = new Patterns.EnemyPatternInfo(enemy.Data.SignatureMoves, enemy.Data.PhaseMoves, () => enemy.MaxHp > 0 ? (float)enemy.CurrentHp / enemy.MaxHp : 1f);
            Context.World.BeginPattern(Context.NextPattern(), BattleFormulas.BulletDamage(enemy.Data.Attack, Context.Player.Data.Defense), enemyInfo);
            Context.Input?.EnablePlayer();
        }

        public override void Tick(float deltaTime)
        {
            elapsed += deltaTime;
            Context.Ui.UpdateTurnHud(1f - Mathf.Clamp01(elapsed / duration));
            Context.World.MovePlayer(Context.Input ? Context.Input.Move : Vector2.zero, deltaTime);

            var gemId = Context.World.TouchedGem(Context.Gems.Active);
            if (gemId != null && Context.Gems.Collect(gemId) is { } gem)
            {
                Context.World.CollectGem(gemId);
                Context.Ui.ShowPopup(GemRewards.Apply(gem.type, Context.RewardSettings, Context.Player, Context.Enemy));
                Context.RefreshSpare();
                Context.RaisePlayerHp();
            }

            int hit = Context.World.ConsumePlayerDamage();
            if (hit > 0)
            {
                Context.Enemy.Spare.RecordHit();
                Context.Events.RaisePlayerDamaged(Context.Player.TakeDamage(hit));
                Context.RaisePlayerHp();
            }

            if (Context.Player.IsDefeated) Context.ChangeState(BattleStateId.Defeat);
            else if (Context.World.IsPlayerTouchingEnemy()) Context.ChangeState(BattleStateId.ActionMenu);
            else if (elapsed >= duration) Context.ChangeState(BattleStateId.ItemMenu);
        }

        public override void Exit()
        {
            Context.Enemy.Spare.EndBulletTurn();
            Context.RefreshSpare();
            Context.World.EndPattern();
            Context.Gems.EndTurn();
            Context.World.ShowGems(System.Array.Empty<string>());
            Context.Ui.HideAll(); // 탄막 턴 표시(남은 시간 막대) 정리
            Context.Input?.EnableUI();
        }
    }

    /// <summary>적에게 닿았을 때: 공격 / 행동 / 자비</summary>
    public sealed class ActionMenuState : BattleStateBase
    {
        public ActionMenuState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            if (Context.Dialogues?.BeforePlayerMenu(BattleStateId.ActionMenu) == true) return;
            var flavor = Context.Enemy.NextFlavorText();
            Context.Ui.ShowBoxText(string.IsNullOrEmpty(flavor) ? BattleTexts.DefaultFlavor : flavor);
            var labels = (string[])BattleTexts.ActionMenu.Clone();
            if (Context.Enemy.CanBeSpared) labels[2] = $"<color={BattleTexts.SpareReadyColor}>{labels[2]}</color>";
            Context.Ui.ShowMainMenu(labels, index => Context.ChangeState(index switch
            {
                0 => BattleStateId.Fight,
                1 => BattleStateId.Act,
                _ => BattleStateId.Mercy,
            }));
        }
    }

    /// <summary>못 닿았을 때: 아이템 / 넘기기</summary>
    public sealed class ItemMenuState : BattleStateBase
    {
        public ItemMenuState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            if (Context.Dialogues?.BeforePlayerMenu(BattleStateId.ItemMenu) == true) return;
            Context.Ui.ShowBoxText(BattleTexts.MissedEnemy);
            Context.Ui.ShowMainMenu(BattleTexts.ItemMenu, index =>
            {
                if (index == 0) Context.ChangeState(BattleStateId.Item);
                else { Context.CompletePlayerTurn(); Context.Ui.ShowDialogue(BattleTexts.SkipTurn, Context.NextTurn); }
            });
        }
    }

    /// <summary>공격 — 타이밍 게이지: 누른 위치의 정확도로 데미지 (놓치면 MISS)</summary>
    public sealed class FightState : BattleStateBase
    {
        public FightState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            Context.CompletePlayerTurn(true);
            Context.Ui.ShowTimingGauge(OnGaugeFinished);
        }

        void OnGaugeFinished(float? accuracy)
        {
            var enemy = Context.Enemy;
            var player = Context.Player;
            // 보석 공격 강화는 놓쳐도 소모된다 (한 번의 공격 기회에 쓰인 것)
            float boost = player.ConsumeAttackMultiplier();
            int damage = BattleFormulas.FightDamage(player.Data.Attack, accuracy, boost, enemy.Data.Defense);
            int applied = enemy.TakeDamage(damage);
            Context.Events.RaiseEnemyDamaged(applied);
            Context.RaiseEnemyHp();
            Context.Ui.ShowPopup(applied > 0 ? $"-{applied}" : BattleTexts.Miss);
            Context.Ui.ShowDialogue(BattleTexts.FightResult(enemy.Data.DisplayName, applied),
                () => { if (enemy.IsDefeated) Context.ChangeState(BattleStateId.Victory); else Context.NextTurn(); });
        }
    }

    /// <summary>행동 — 살펴보기 + 적 데이터의 ACT 목록</summary>
    public sealed class ActState : BattleStateBase
    {
        public ActState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            var data = Context.Enemy.Data;
            var items = new List<string> { BattleTexts.Check };
            items.AddRange(data.Acts.Select(a => a.DisplayName));

            Context.Ui.ShowListMenu(items, index =>
            {
                string text;
                if (index == 0) text = data.CheckText + (Context.Enemy.Spare.UsesRule ? "\n" + Context.Enemy.Spare.Hint : string.Empty);
                else
                {
                    bool wasSpareable = Context.Enemy.CanBeSpared;
                    var act = data.Acts[index - 1];
                    Context.Enemy.Spare.RecordAct(act.DisplayName);
                    foreach (string flag in act.SpareFlags) Context.Enemy.Spare.SetFlag(flag);
                    Context.Enemy.AddSpareProgress(act.SpareProgress);
                    text = act.ResultText;
                    if (!wasSpareable && Context.Enemy.CanBeSpared) text += "\n" + BattleTexts.NowSpareable;
                }
                Context.CompletePlayerTurn();
                if (Context.Enemy.Spare.UsesRule) text += "\n" + Context.Enemy.Spare.Hint;
                Context.Ui.ShowDialogue(text, Context.NextTurn);
            }, () => Context.ChangeState(BattleStateId.ActionMenu));
        }
    }

    /// <summary>아이템 — 인벤토리에서 골라 사용 (소모)</summary>
    public sealed class ItemState : BattleStateBase
    {
        public ItemState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            var inventory = Context.Player.Inventory;
            if (inventory.IsEmpty)
            {
                Context.Ui.ShowDialogue(BattleTexts.NoItems, () => Context.ChangeState(BattleStateId.ItemMenu));
                return;
            }

            Context.Ui.ShowListMenu(inventory.Items.Select(i => i.DisplayName).ToList(), index =>
            {
                var item = inventory.Take(index);
                int healed = Context.Player.Heal(item.HealAmount);
                Context.CompletePlayerTurn();
                Context.RaisePlayerHp();
                Context.Ui.ShowDialogue(item.FormatUseText(healed), Context.NextTurn);
            }, () => Context.ChangeState(BattleStateId.ItemMenu));
        }
    }

    /// <summary>자비 — 살려줄 수 있으면 노란색, 고르면 승리(살려줌). (Flee는 결정 대기)</summary>
    public sealed class MercyState : BattleStateBase
    {
        public MercyState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            bool ready = Context.Enemy.CanBeSpared;
            var label = ready ? $"<color={BattleTexts.SpareReadyColor}>{BattleTexts.Spare}</color>" : BattleTexts.Spare;
            Context.Ui.ShowListMenu(new[] { label }, _ =>
            {
                if (Context.Enemy.CanBeSpared) Context.ChangeState(BattleStateId.Victory);
                else { Context.CompletePlayerTurn(); Context.Ui.ShowDialogue(BattleTexts.NotReadyToSpare + "\n" + Context.Enemy.Spare.Hint, Context.NextTurn); }
            }, () => Context.ChangeState(BattleStateId.ActionMenu));
        }
    }

    /// <summary>승리 — 적이 쓰러졌으면 처치, 아니면 살려줌</summary>
    public sealed class VictoryState : BattleStateBase
    {
        public VictoryState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            var outcome = Context.Enemy.IsDefeated ? BattleOutcome.EnemyDefeated : BattleOutcome.EnemySpared;
            Context.Events.RaiseBattleEnded(outcome);
            Context.Ui.HideAll();
            var data = Context.Enemy.Data;
            if (Context.Dialogues?.TryStart(DialogueTrigger.Victory, () => Context.FinishBattle(outcome)) == true) return;
            Context.Ui.ShowDialogue(outcome == BattleOutcome.EnemyDefeated ? data.DefeatText : data.SpareText, () => Context.FinishBattle(outcome));
        }
    }

    public sealed class DefeatState : BattleStateBase
    {
        public DefeatState(BattleContext context) : base(context) { }

        public override void Enter()
        {
            Context.Events.RaiseBattleEnded(BattleOutcome.PlayerDefeated);
            Context.Ui.HideAll();
            Context.Ui.ShowDialogue(BattleTexts.PlayerDefeated, () => Context.FinishBattle(BattleOutcome.PlayerDefeated));
        }
    }

    public static class BattleStates
    {
        /// <summary>모든 전투 상태를 등록한다 (BattleController와 테스트가 같이 사용).</summary>
        public static void RegisterAll(BattleStateMachine machine, BattleContext context)
        {
            machine.Register(BattleStateId.Intro, new IntroState(context));
            machine.Register(BattleStateId.EnemyTurn, new EnemyTurnState(context));
            machine.Register(BattleStateId.ActionMenu, new ActionMenuState(context));
            machine.Register(BattleStateId.ItemMenu, new ItemMenuState(context));
            machine.Register(BattleStateId.Fight, new FightState(context));
            machine.Register(BattleStateId.Act, new ActState(context));
            machine.Register(BattleStateId.Item, new ItemState(context));
            machine.Register(BattleStateId.Mercy, new MercyState(context));
            machine.Register(BattleStateId.Victory, new VictoryState(context));
            machine.Register(BattleStateId.Defeat, new DefeatState(context));
            machine.Register(BattleStateId.Dialogue, new DialogueState(context));
        }
    }
}
