using Game2Week.Core;
using Game2Week.UI;
using TMPro;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>엔딩: 문구가 천천히 올라오고, 잠시 뒤 확인키로 메인. 들어오는 순간 "엔딩 봄"을 저장.</summary>
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] InputReader input;
        [SerializeField] TMP_Text body;
        [SerializeField] TMP_Text hint;
        [SerializeField] float scrollDuration = 4f;
        [SerializeField] float scrollDistance = 160f;
        [SerializeField] float inputDelay = 1.5f;

        RectTransform bodyRect;
        Vector2 bodyTarget;
        float time;
        bool leaving;

        void Start()
        {
            session.Save.MarkEndingSeen();
            body.text = string.Join("\n", UiTexts.EndingLines);
            hint.text = UiTexts.EndingHint;
            hint.gameObject.SetActive(false);
            bodyRect = body.rectTransform;
            bodyTarget = bodyRect.anchoredPosition;
            bodyRect.anchoredPosition = bodyTarget + Vector2.down * scrollDistance;
            input.EnableUI();
            input.Submit += OnSubmit;
        }

        void OnDestroy()
        {
            if (input) input.Submit -= OnSubmit;
        }

        void Update()
        {
            time += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / scrollDuration));
            bodyRect.anchoredPosition = bodyTarget + Vector2.down * (scrollDistance * (1f - k));
            var c = body.color;
            c.a = k;
            body.color = c;
            if (time >= inputDelay) hint.gameObject.SetActive(true);
        }

        void OnSubmit()
        {
            if (time >= inputDelay) Finish();
        }

        public void Finish()
        {
            if (leaving) return;
            leaving = true;
            SceneLoader.Load(SceneNames.MainMenu);
        }
    }
}
