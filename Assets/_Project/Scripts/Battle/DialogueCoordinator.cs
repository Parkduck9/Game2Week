using System;
using System.Collections.Generic;
using Game2Week.Dialogue;

namespace Game2Week.Battle
{
    public enum DialogueTrigger { Intro, Phase2, SpareReady, Victory }

    /// <summary>상황별 파일 선택과 대화 후 복귀를 맡는다. 선택 효과는 자비 추적기에만 적용한다.</summary>
    public sealed class DialogueCoordinator
    {
        readonly BattleContext context;
        readonly DialogueRepository repository;
        bool pendingPhase, readyShown;
        Action continuation;
        public DialogueDefinition Definition { get; private set; }
        public DialogueRunner Runner { get; private set; }
        public DialogueCoordinator(BattleContext context, DialogueRepository repository)
        { this.context = context; this.repository = repository; }
        public void PhaseChanged(int phase) { if (phase >= 1) pendingPhase = true; }
        public bool BeforePlayerMenu(BattleStateId resume)
        {
            if (pendingPhase)
            {
                pendingPhase = false;
                if (TryStart(DialogueTrigger.Phase2, () => context.ChangeState(resume))) return true;
            }
            if (!readyShown && context.Enemy.CanBeSpared)
            {
                readyShown = true;
                return TryStart(DialogueTrigger.SpareReady, () => context.ChangeState(resume));
            }
            return false;
        }
        public void NextTurn()
        {
            if (!readyShown && context.Enemy.CanBeSpared)
            {
                readyShown = true;
                if (TryStart(DialogueTrigger.SpareReady, () => context.ChangeState(BattleStateId.EnemyTurn))) return;
            }
            context.ChangeState(BattleStateId.EnemyTurn);
        }
        public bool TryStart(DialogueTrigger trigger, Action onFinished)
        {
            string id = Value(context.Stage.dialogues, trigger);
            if (string.IsNullOrEmpty(id)) id = Value(context.Enemy.Data.Dialogues, trigger);
            if (string.IsNullOrEmpty(id)) return false;
            try
            {
                Definition = repository.Load(id);
                var tracker = context.Enemy.Spare;
                Runner = new DialogueRunner(Definition, new Dictionary<string, Action<DialogueEffect>>
                {
                    ["flag"] = effect => { tracker.SetFlag(effect.id, effect.value); context.RefreshSpare(); },
                    ["spareCondition"] = effect => { tracker.SetFlag(effect.id, effect.value); context.RefreshSpare(); },
                });
            }
            catch (Exception error) { UnityEngine.Debug.LogWarning("대화를 불러오지 못해 기존 흐름으로 진행합니다: " + error.Message); return false; }
            continuation = onFinished;
            context.ChangeState(BattleStateId.Dialogue);
            return true;
        }
        public void Finish()
        {
            var callback = continuation; continuation = null; Runner = null; Definition = null;
            callback?.Invoke();
        }
        static string Value(Stages.StageDialogues dialogues, DialogueTrigger trigger)
        {
            if (dialogues == null) return string.Empty;
            return trigger switch { DialogueTrigger.Intro => dialogues.intro, DialogueTrigger.Phase2 => dialogues.phase2, DialogueTrigger.SpareReady => dialogues.spareReady, _ => dialogues.victory };
        }
    }
}
