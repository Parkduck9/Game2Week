using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2Week.Core
{
    /// <summary>
    /// 게임 코드가 Input System 대신 사용하는 입력 창구.
    /// 메뉴·대사는 UI 맵, 탄막 턴 이동은 Player 맵을 쓰고 상태에 따라 전환한다.
    /// System 맵(일시정지)은 항상 켜져 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Input Reader", fileName = "InputReader")]
    public sealed class InputReader : ScriptableObject
    {
        public enum Mode
        {
            None,
            UI,
            Player,
        }

        const float NavigateDeadZone = 0.5f;

        [SerializeField] InputActionAsset actions;

        /// <summary>메뉴 이동. 방향키를 누를 때마다 한 번 (상하좌우 중 하나).</summary>
        public event Action<Vector2Int> Navigate;
        public event Action Submit;
        public event Action Cancel;
        /// <summary>ESC / 게임패드 Start — 어느 모드에서든</summary>
        public event Action Pause;

        /// <summary>탄막 턴 이동 입력 (Player 맵이 켜져 있을 때만 값이 들어온다).</summary>
        public Vector2 Move => move != null && move.enabled ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f) : Vector2.zero;
        public Vector2 Look => look != null && look.enabled ? look.ReadValue<Vector2>() : Vector2.zero;
        /// <summary>
        /// 정지 자세 버튼(Ctrl)을 누르고 있는지 — 빨강 공격 통과 조건.
        /// IsPressed()는 맵이 꺼진 동안(메뉴) 뗀 것을 모르고 눌린 채로 남을 수 있어, 누름/뗌을 직접 기억하고 모드가 바뀌면 지운다.
        /// 다시 켤 때 실제로 누르고 있으면 initialStateCheck로 다시 눌림이 들어온다.
        /// </summary>
        public bool BraceHeld => braceDown && CurrentMode == Mode.Player;
        public bool ConsumeDodge() => Consume(ref dodgePending);
        public bool ConsumeJump() => Consume(ref jumpPending);
        public bool ConsumeParry() => Consume(ref parryPending);
        public bool ConsumeLockOn() => Consume(ref lockPending);
        public void ClearPlayerCommands() { dodgePending = jumpPending = parryPending = lockPending = false; }
        bool Consume(ref bool pending) { bool value = CurrentMode == Mode.Player && pending; pending = false; return value; }

        public Mode CurrentMode { get; private set; }

        InputActionMap uiMap;
        InputActionMap playerMap;
        InputActionMap systemMap;
        InputAction navigate;
        InputAction submit;
        InputAction cancel;
        InputAction move;
        InputAction pause;
        InputAction look, dodge, jump, parry, lockOn, brace;
        bool dodgePending, jumpPending, parryPending, lockPending, braceDown;
        Vector2Int lastNavigate;

        void OnEnable()
        {
            if (actions == null) return;

            uiMap = actions.FindActionMap("UI", throwIfNotFound: true);
            playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            systemMap = actions.FindActionMap("System", throwIfNotFound: true);
            navigate = uiMap.FindAction("Navigate", throwIfNotFound: true);
            submit = uiMap.FindAction("Submit", throwIfNotFound: true);
            cancel = uiMap.FindAction("Cancel", throwIfNotFound: true);
            move = playerMap.FindAction("Move", throwIfNotFound: true);
            pause = systemMap.FindAction("Pause", throwIfNotFound: true);
            look = playerMap.FindAction("Look");
            dodge = playerMap.FindAction("Dodge");
            jump = playerMap.FindAction("Jump");
            parry = playerMap.FindAction("Parry");
            lockOn = playerMap.FindAction("LockOn");
            brace = playerMap.FindAction("Brace");
            if (dodge != null) dodge.performed += OnDodge;
            if (jump != null) jump.performed += OnJump;
            if (parry != null) parry.performed += OnParry;
            if (lockOn != null) lockOn.performed += OnLockOn;
            if (brace != null) { brace.performed += OnBraceDown; brace.canceled += OnBraceUp; }

            navigate.performed += OnNavigate;
            navigate.canceled += OnNavigate;
            submit.performed += OnSubmit;
            cancel.performed += OnCancel;
            pause.performed += OnPause;
        }

        void OnDisable()
        {
            if (navigate == null) return;

            navigate.performed -= OnNavigate;
            navigate.canceled -= OnNavigate;
            submit.performed -= OnSubmit;
            cancel.performed -= OnCancel;
            pause.performed -= OnPause;
            if (dodge != null) dodge.performed -= OnDodge;
            if (jump != null) jump.performed -= OnJump;
            if (parry != null) parry.performed -= OnParry;
            if (lockOn != null) lockOn.performed -= OnLockOn;
            if (brace != null) { brace.performed -= OnBraceDown; brace.canceled -= OnBraceUp; }
            DisableAll();
        }

        public void EnableUI()
        {
            ClearPlayerCommands();
            braceDown = false;
            playerMap?.Disable();
            uiMap?.Enable();
            systemMap?.Enable();
            lastNavigate = Vector2Int.zero;
            CurrentMode = Mode.UI;
        }

        public void EnablePlayer()
        {
            ClearPlayerCommands();
            braceDown = false; // 실제로 누르고 있으면 맵을 켤 때 initialStateCheck로 다시 들어온다
            uiMap?.Disable();
            playerMap?.Enable();
            systemMap?.Enable();
            CurrentMode = Mode.Player;
        }

        /// <summary>저장해 둔 모드로 되돌린다 (일시정지 해제 등).</summary>
        public void Restore(Mode mode)
        {
            if (mode == Mode.Player) EnablePlayer();
            else if (mode == Mode.UI) EnableUI();
        }

        public void DisableAll()
        {
            ClearPlayerCommands();
            braceDown = false;
            uiMap?.Disable();
            playerMap?.Disable();
            systemMap?.Disable();
            CurrentMode = Mode.None;
        }

        /// <summary>아날로그 입력을 상하좌우 한 방향으로 바꾼다. 데드존 안이면 zero.</summary>
        public static Vector2Int ToDirection(Vector2 value)
        {
            if (value.magnitude < NavigateDeadZone) return Vector2Int.zero;
            return Mathf.Abs(value.x) > Mathf.Abs(value.y)
                ? new Vector2Int(value.x > 0f ? 1 : -1, 0)
                : new Vector2Int(0, value.y > 0f ? 1 : -1);
        }

        void OnNavigate(InputAction.CallbackContext context)
        {
            var direction = ToDirection(context.ReadValue<Vector2>());
            if (direction == lastNavigate) return;

            lastNavigate = direction;
            if (direction != Vector2Int.zero) Navigate?.Invoke(direction);
        }

        void OnSubmit(InputAction.CallbackContext _) => Submit?.Invoke();

        void OnCancel(InputAction.CallbackContext _) => Cancel?.Invoke();

        void OnPause(InputAction.CallbackContext _) => Pause?.Invoke();
        void OnDodge(InputAction.CallbackContext _) { if (CurrentMode == Mode.Player) dodgePending = true; }
        void OnJump(InputAction.CallbackContext _) { if (CurrentMode == Mode.Player) jumpPending = true; }
        void OnParry(InputAction.CallbackContext _) { if (CurrentMode == Mode.Player) parryPending = true; }
        void OnLockOn(InputAction.CallbackContext _) { if (CurrentMode == Mode.Player) lockPending = true; }
        void OnBraceDown(InputAction.CallbackContext _) => braceDown = true;
        void OnBraceUp(InputAction.CallbackContext _) => braceDown = false;
    }
}
