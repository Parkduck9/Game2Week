using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 전투 이펙트 묶음의 뿌리 (6단계 Codex 담당). BattleController가 프리팹을 하나 만들어 Bind로 알림 창구를 넘긴다.
    /// 새 이펙트(피격 파티클·적 말풍선·회피 잔상·착지 먼지·쳐내기 효과)는 이 프리팹 아래 컴포넌트로 추가하고
    /// 여기서 구독을 연결한다 — Battle 씬은 고치지 않아도 된다.
    /// </summary>
    public class BattleFxRig : MonoBehaviour
    {
        public BattleEvents Events { get; private set; }
        public BattleFeedback Feedback { get; private set; }
        public StageSpawner Spawner { get; private set; }
        public bool IsBound => Feedback != null;

        public void Bind(BattleEvents events, BattleFeedback feedback, StageSpawner spawner)
        {
            Events = events;
            Feedback = feedback;
            Spawner = spawner;
            OnBound();
        }

        /// <summary>하위 이펙트 구독 연결 지점 (6단계에서 채움)</summary>
        protected virtual void OnBound() { }
    }
}
