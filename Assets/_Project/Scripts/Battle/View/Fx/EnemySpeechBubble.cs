using TMPro;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 적 말풍선 (6단계): 탄막 턴 시작 때 적 대사(BattleEvents.EnemySpoke)를 적 머리 위에 띄운다.
    /// 월드 공간 글자 + 카메라를 향함. 일정 시간 뒤 또는 탄막 턴이 끝나면 숨긴다.
    /// </summary>
    public sealed class EnemySpeechBubble : MonoBehaviour, IBattleFxModule
    {
        [SerializeField, Min(0.5f)] float showSeconds = 3f;
        [SerializeField] float heightAboveEnemy = 1.45f;
        [SerializeField, Min(0.1f)] float fontSize = 3f;

        BattleFxRig rig;
        TextMeshPro text;
        float shownLeft;

        public bool IsShowing => text && text.enabled;
        public string Text => text ? text.text : null;

        public void Bind(BattleFxRig battleRig)
        {
            Unbind();
            rig = battleRig;
            if (!text)
            {
                var go = new GameObject("SpeechText");
                go.transform.SetParent(transform, false);
                text = go.AddComponent<TextMeshPro>();
                text.fontSize = fontSize;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.sizeDelta = new Vector2(4f, 1f);
                text.color = Color.white;
                text.outlineWidth = 0.25f;
                text.outlineColor = new Color32(20, 20, 30, 255);
                text.enabled = false;
            }
            if (rig.Events == null) return;
            rig.Events.EnemySpoke += Show;
            rig.Events.StateChanged += OnStateChanged;
        }

        void Unbind()
        {
            if (rig?.Events == null) { rig = null; return; }
            rig.Events.EnemySpoke -= Show;
            rig.Events.StateChanged -= OnStateChanged;
            rig = null;
        }

        void OnDestroy() => Unbind();

        void Show(string line)
        {
            if (!text) return;
            text.text = $"\"{line}\"";
            text.enabled = true;
            shownLeft = showSeconds;
            Follow();
        }

        void OnStateChanged(BattleStateId state)
        {
            if (state != BattleStateId.EnemyTurn && text) text.enabled = false;
        }

        void LateUpdate()
        {
            if (!IsShowing) return;
            if (Time.timeScale > 0f)
            {
                shownLeft -= Time.deltaTime;
                if (shownLeft <= 0f) { text.enabled = false; return; }
            }
            Follow();
        }

        void Follow()
        {
            var enemy = rig != null && rig.Spawner ? rig.Spawner.Enemy : null;
            if (enemy) text.transform.position = enemy.transform.position + Vector3.up * heightAboveEnemy;
            var cam = Camera.main;
            if (cam) text.transform.rotation = cam.transform.rotation;
        }
    }
}
