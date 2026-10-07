using System;
using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle
{
    /// <summary>월드 사건을 순수 자비 모델에 전달하며 전투 종료 때 구독을 해제한다.</summary>
    public sealed class SpareFeedbackBinding : IDisposable
    {
        readonly BattleFeedback feedback;
        readonly BattleContext context;
        public SpareFeedbackBinding(BattleFeedback feedback, BattleContext context)
        {
            this.feedback = feedback; this.context = context;
            feedback.PlayerParried += Parried; feedback.ColorPassed += Passed;
            feedback.PlayerHit += Hit; feedback.EnemyPhaseChanged += Phase;
        }
        void Parried(Vector3 position, float side) { context.Enemy.Spare.RecordAction(SpareActionKind.Parry); context.RefreshSpare(); }
        void Passed(AttackColor color, Vector3 position)
        {
            if (color == AttackColor.Red) context.Enemy.Spare.RecordAction(SpareActionKind.RedPass);
            if (color == AttackColor.Blue) context.Enemy.Spare.RecordAction(SpareActionKind.BluePass);
            context.RefreshSpare();
        }
        void Hit(Vector3 position) => context.Enemy.Spare.RecordHit();
        void Phase(int phase) => context.Dialogues?.PhaseChanged(phase);
        public void Dispose()
        {
            feedback.PlayerParried -= Parried; feedback.ColorPassed -= Passed;
            feedback.PlayerHit -= Hit; feedback.EnemyPhaseChanged -= Phase;
        }
    }
}
