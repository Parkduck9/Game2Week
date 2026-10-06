using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 탄의 이동 경로. 발사 순간 값(TrajectoryLaunch)과 경과 시간만으로 위치를 계산한다 (상태 없음).
    /// 게임과 패턴 에디터 미리보기가 같은 구현을 쓴다.
    /// </summary>
    public interface ITrajectory
    {
        Vector3 Evaluate(in TrajectoryLaunch launch, float elapsed);
    }

    /// <summary>발사 순간 고정되는 기준 좌표. 이후 적·주인공이 움직여도 바뀌지 않는다.</summary>
    public readonly struct TrajectoryLaunch
    {
        public TrajectoryLaunch(Vector3 origin, Vector3 flatVelocity)
        {
            Origin = origin;
            var flat = new Vector3(flatVelocity.x, 0f, flatVelocity.z);
            Speed = flat.magnitude;
            Forward = Speed > 0.0001f ? flat / Speed : Vector3.forward;
            Right = Vector3.Cross(Vector3.up, Forward);
        }

        public Vector3 Origin { get; }
        /// <summary>수평 진행 방향 (단위 벡터)</summary>
        public Vector3 Forward { get; }
        /// <summary>진행 방향 기준 수평 오른쪽 (단위 벡터)</summary>
        public Vector3 Right { get; }
        /// <summary>기준 전진 속도 (m/s)</summary>
        public float Speed { get; }
    }
}
