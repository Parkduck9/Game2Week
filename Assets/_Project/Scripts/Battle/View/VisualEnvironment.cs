using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game2Week.Battle.View
{
    /// <summary>씬마다 같은 조명·하늘·안개와 후처리를 사용하고 이전 전역 값을 복원한다.</summary>
    public sealed class VisualEnvironment : MonoBehaviour
    {
        [SerializeField] Material sky;
        Material previousSky;
        bool previousFog;
        Color previousFogColor, previousAmbient;
        float previousStart, previousEnd;
        FogMode previousMode;
        void OnEnable()
        {
            previousSky=RenderSettings.skybox; previousFog=RenderSettings.fog; previousFogColor=RenderSettings.fogColor;
            previousStart=RenderSettings.fogStartDistance; previousEnd=RenderSettings.fogEndDistance;
            previousAmbient=RenderSettings.ambientLight; previousMode=RenderSettings.fogMode;
            RenderSettings.skybox=sky;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=28;RenderSettings.fogEndDistance=75;
            RenderSettings.fogColor=new Color(.27f,.36f,.45f);RenderSettings.ambientLight=new Color(.48f,.52f,.6f);
        }
        void Start()
        {
            foreach(var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if(camera.TryGetComponent<UniversalAdditionalCameraData>(out var data)){data.renderPostProcessing=true;camera.clearFlags=CameraClearFlags.Skybox;}
        }
        void OnDisable()
        {
            RenderSettings.skybox=previousSky;RenderSettings.fog=previousFog;RenderSettings.fogColor=previousFogColor;
            RenderSettings.fogStartDistance=previousStart;RenderSettings.fogEndDistance=previousEnd;
            RenderSettings.fogMode=previousMode;RenderSettings.ambientLight=previousAmbient;
        }
    }
}
