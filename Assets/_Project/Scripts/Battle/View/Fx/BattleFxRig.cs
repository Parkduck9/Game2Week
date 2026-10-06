using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 전투 이펙트 묶음의 뿌리. BattleController가 프리팹을 하나 만들어 Bind로 알림 창구를 넘기면
    /// 아래에 붙은 모듈(IBattleFxModule — 주인공 동작 효과·적 말풍선 등)을 모두 연결한다. Battle 씬은 고치지 않아도 된다.
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
            foreach (var module in GetComponentsInChildren<IBattleFxModule>(true)) module.Bind(this);
            OnBound();
        }

        /// <summary>추가 구독이 필요한 하위 클래스용</summary>
        protected virtual void OnBound() { }
    }
}
