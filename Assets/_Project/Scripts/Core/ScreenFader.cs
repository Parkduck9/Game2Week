using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 전환 검은 화면 (6단계). SceneLoader가 처음 쓸 때 하나 만들어 씬이 바뀌어도 유지한다.
    /// 일시정지(timeScale 0) 중에도 동작하도록 실제 시간으로 움직인다.
    /// </summary>
    public sealed class ScreenFader : MonoBehaviour
    {
        const int SortingOrder = 1000;

        Image image;

        public float Alpha => image ? image.color.a : 0f;

        public static ScreenFader Create()
        {
            var go = new GameObject("ScreenFader");
            DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            var imageGo = new GameObject("Black");
            imageGo.transform.SetParent(go.transform, false);
            var rect = imageGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var fader = go.AddComponent<ScreenFader>();
            fader.image = imageGo.AddComponent<Image>();
            fader.image.color = new Color(0f, 0f, 0f, 0f);
            fader.image.raycastTarget = false;
            return fader;
        }

        public IEnumerator FadeTo(float alpha, float seconds)
        {
            float start = Alpha;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                Set(Mathf.Lerp(start, alpha, t / seconds));
                yield return null;
            }
            Set(alpha);
        }

        void Set(float alpha)
        {
            if (!image) return;
            image.color = new Color(0f, 0f, 0f, alpha);
            image.enabled = alpha > 0.001f;
        }
    }
}
