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
            duration = Mathf.Max(0.1f, Context.Stage.enemyTurn.duration);
            Context.World.ResetPlayer();
            Context.World.ShowGems(Context.Gems.RollForTurn());
            Context.Ui.HideAll();
            var line = Context.Enemy.NextEnemyTurnLine();
            Context.Ui.ShowTurnHud(string.IsNullOrEmpty(line) ? BattleTexts.TurnHint : $"{Context.Enemy.Data.DisplayName}: \"{line}\"   ·   {BattleTexts.TurnHint}");
            Context.World.BeginPattern(Context.NextPattern(), BattleFormulas.BulletDamage(Context.Enemy.Data.Attack, Context.Player.Data.Defense));
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
                Context.RaisePlayerHp();
            }

            int hit = Context.World.ConsumePlayerDamage();
            if (hit > 0)
            {
                Context.Events.RaisePlayerDamaged(Context.Player.TakeDamage(hit));
                Context.RaisePlayerHp();
            }

            if (Context.Player.IsDefeated) Context.ChangeState(BattleStateId.Defeat);
            else if (Context.World.IsPlayerTouchingEnemy()) Context.ChangeState(BattleStateId.ActionMenu);
            else if (elapsed >= duration) Context.ChangeState(BattleStateId.ItemMenu);
        }

        public override void Exit()
        {
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
            var flavor = Context.Enemy.NextFlavorText();
            Context.Ui.ShowBoxText(string.IsNullOrEmpty(flavor) ? BattleTexts.DefaultFlavor : flavor);
            Context.Ui.ShowMainMenu(BattleTexts.ActionMenu, index => Context.ChangeState(index switch
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
            Context.Ui.ShowBoxText(BattleTexts.MissedEnemy);
            Context.Ui.ShowMainMenu(BattleTexts.ItemMenu, index =>
            {
                if (index == 0) Context.ChangeState(BattleStateId.Item);
                else Context.Ui.ShowDialogue(BattleTexts.SkipTurn, () => Context.ChangeState(BattleStateId.EnemyTurn));
            });
        }
    }

    /// <summary>공격 — 타이밍 게이지: 누른 위치의 정확도로 데미지 (놓치면 MISS)</summary>
    public sealed class FightState : BattleStateBase
    {
        public FightState(BattleContext context) : base(context) { }

        public override void Enter()
        {
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
                () => Context.ChangeState(enemy.IsDefeated ? BattleStateId.Victory : BattleStateId.EnemyTurn));
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
                if (index == 0) text = data.CheckText;
                else
                {
                    bool wasSpareable = Context.Enemy.CanBeSpared;
                    var act = data.Acts[index - 1];
                    Context.Enemy.AddSpareProgress(act.SpareProgress);
                    text = act.ResultText;
                    if (!wasSpareable && Context.Enemy.CanBeSpared) text += "\n" + BattleTexts.NowSpareable;
                }
                Context.Ui.ShowDialogue(text, () => Context.ChangeState(BattleStateId.EnemyTurn));
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
                Context.RaisePlayerHp();
                Context.Ui.ShowDialogue(item.FormatUseText(healed), () => Context.ChangeState(BattleStateId.EnemyTurn));
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
                else Context.Ui.ShowDialogue(BattleTexts.NotReadyToSpare, () => Context.ChangeState(BattleStateId.EnemyTurn));
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
        }
    }
}
