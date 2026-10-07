using System;
using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle
{
    /// <summary>
    /// 3D 월드에서 일어난 일을 연출(이펙트·소리)에 알리는 창구 (6단계 공용 연결 지점).
    /// 주인공 쪽(회피·점프·착지·쳐내기·자세·피격·색 통과)은 플레이어 코드가, 공격 쪽(발사·예고)은 패턴이 Raise한다.
    /// 이펙트(BattleFxRig)와 소리(BattleAudio)는 구독만 한다. 전투 한 번마다 BattleWorld가 하나 만든다.
    /// </summary>
    public sealed class BattleFeedback
    {
        public event Action<Vector3> PlayerDodged;
        public event Action<Vector3> PlayerJumped;
        public event Action<Vector3> PlayerLanded;
        /// <summary>(탄 위치, 탄이 빠져나가는 쪽: +1 오른쪽 어깨 / -1 왼쪽 어깨)</summary>
        public event Action<Vector3, float> PlayerParried;
        /// <summary>정지 자세 완성 (빨강 통과 가능해진 순간)</summary>
        public event Action<Vector3> PlayerBraced;
        /// <summary>실제로 피해를 받은 순간 (무적 중이면 없음)</summary>
        public event Action<Vector3> PlayerHit;
        /// <summary>색 규칙으로 탄이 몸을 통과함 (같은 색은 짧은 간격으로 한 번만)</summary>
        public event Action<AttackColor, Vector3> ColorPassed;
        /// <summary>패턴이 탄을 쏨 (발사 위치)</summary>
        public event Action<AttackColor, Vector3> BulletFired;
        /// <summary>패턴이 공격 예고를 시작함 (발사원 위치)</summary>
        public event Action<AttackColor, Vector3> WarningStarted;

        public void RaisePlayerDodged(Vector3 at) => PlayerDodged?.Invoke(at);
        public void RaisePlayerJumped(Vector3 at) => PlayerJumped?.Invoke(at);
        public void RaisePlayerLanded(Vector3 at) => PlayerLanded?.Invoke(at);
        public void RaisePlayerParried(Vector3 bullet, float side) => PlayerParried?.Invoke(bullet, side);
        public void RaisePlayerBraced(Vector3 at) => PlayerBraced?.Invoke(at);
        public void RaisePlayerHit(Vector3 at) => PlayerHit?.Invoke(at);
        public void RaiseColorPassed(AttackColor color, Vector3 at) => ColorPassed?.Invoke(color, at);
        public void RaiseBulletFired(AttackColor color, Vector3 at) => BulletFired?.Invoke(color, at);
        public void RaiseWarningStarted(AttackColor color, Vector3 at) => WarningStarted?.Invoke(color, at);
        /// <summary>적 체력 단계가 올라간 턴의 시작 (1 = 첫 단계 전환). 대화·연출이 구독 (9단계)</summary>
        public event Action<int> EnemyPhaseChanged;
        public void RaiseEnemyPhaseChanged(int phase) => EnemyPhaseChanged?.Invoke(phase);
    }
}
