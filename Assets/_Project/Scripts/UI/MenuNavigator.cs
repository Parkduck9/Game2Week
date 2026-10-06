using System;
using System.Collections.Generic;
using Game2Week.Core;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>InputReader 입력으로 메뉴 커서를 움직이고, 확인/취소를 알린다. 켜져 있을 때만 입력을 받는다.</summary>
    public sealed class MenuNavigator : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] MenuListView view;
        [Tooltip("켜면 좌우, 끄면 상하로 이동")]
        [SerializeField] bool horizontal;
        [Tooltip("메뉴 효과음 (선택)")]
        [SerializeField] UiSoundSet sounds;

        readonly MenuList list = new();
        AudioSource soundSource;

        void PlaySound(AudioClip clip)
        {
            if (!sounds || !clip) return;
            if (!soundSource && !TryGetComponent(out soundSource))
            {
                soundSource = gameObject.AddComponent<AudioSource>();
                soundSource.playOnAwake = false;
                soundSource.ignoreListenerPause = true; // 일시정지 메뉴에서도 들리게
            }
            sounds.Play(soundSource, clip);
        }

        public event Action<int> Selected;
        public event Action Cancelled;

        public int Index => list.Index;
        public bool HasSounds => sounds;
        public IReadOnlyList<string> Labels => view.Labels;

        public void SetItems(IReadOnlyList<string> items, IEnumerable<int> disabledIndices = null)
        {
            var disabled = disabledIndices != null ? new List<int>(disabledIndices) : null;
            list.SetCount(items.Count, disabled);
            list.Reset();
            view.SetItems(items, disabled);
            view.SetSelected(list.Index);
        }

        public bool IsEnabled(int index) => list.IsEnabled(index);

        public void Select(int index)
        {
            list.Select(index);
            view.SetSelected(list.Index);
        }

        /// <summary>테스트·코드용: 확인키 없이 항목 고르기 (비활성 항목은 무시)</summary>
        public void Choose(int index)
        {
            if (list.IsEnabled(index)) Selected?.Invoke(index);
        }

        public void CancelMenu() => Cancelled?.Invoke();

        void OnEnable()
        {
            input.Navigate += OnNavigate;
            input.Submit += OnSubmit;
            input.Cancel += OnCancel;
            input.EnableUI();
        }

        void OnDisable()
        {
            input.Navigate -= OnNavigate;
            input.Submit -= OnSubmit;
            input.Cancel -= OnCancel;
        }

        void OnNavigate(Vector2Int direction)
        {
            // 위쪽 화살표 = 이전 항목
            int delta = horizontal ? direction.x : -direction.y;
            if (!list.Move(delta)) return;
            view.SetSelected(list.Index);
            if (sounds) PlaySound(sounds.move);
        }

        void OnSubmit()
        {
            if (!list.IsEnabled(list.Index)) { if (sounds) PlaySound(sounds.denied); return; }
            if (sounds) PlaySound(sounds.submit);
            Selected?.Invoke(list.Index);
        }

        void OnCancel()
        {
            if (sounds) PlaySound(sounds.cancel);
            Cancelled?.Invoke();
        }
    }
}
