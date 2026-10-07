using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>전투 프레임의 실제 소요 시간을 수집한다. 로딩과 일시정지는 제외한다.</summary>
    public sealed class FrameMeasurement : MonoBehaviour
    {
        public System.Func<bool> IsMeasuring;
        public Camera RenderCamera;
        public int Frames { get; private set; }
        public double Seconds { get; private set; }
        public float AverageMilliseconds => Frames > 0 ? (float)(Seconds * 1000 / Frames) : 0;
        public float AverageFps => Seconds > 0 ? (float)(Frames / Seconds) : 0;
        void Update()
        {
            if (Time.timeScale <= 0 || IsMeasuring == null || !IsMeasuring()) return;
            // 배치 에디터에는 게임 뷰 렌더가 없으므로 지정한 1080p 대상에 실제로 렌더한다.
            if(RenderCamera)RenderCamera.Render();
            Frames++; Seconds += Time.unscaledDeltaTime;
        }
    }
}
