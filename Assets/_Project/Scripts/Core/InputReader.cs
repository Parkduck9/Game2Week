using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2Week.Core
{
    /// <summary>
    /// 게임 코드가 Input System 대신 사용하는 입력 창구.
    /// 메뉴·대사·타이밍 공격은 UI 맵, 탄막 회피 이동은 Player 맵을 사용하고 상태에 따라 전환한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Input Reader", fileName = "InputReader")]
    public sealed class InputReader : ScriptableObject
    {
        const float NavigateDeadZone = 0.5f;

        [SerializeField] InputActionAsset actions;

        /// <summary>메뉴 이동. 방향키를 누를 때마다 한 번 (상하좌우 중 하나).</summary>
        public event Action<Vector2Int> Navigate;
        public event Action Submit;
        public event Action Cancel;

        /// <summary>탄막 회피용 이동 입력 (Player 맵이 켜져 있을 때만 값이 들어온다).</summary>
        public Vector2 Move => move != null && move.enabled ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f) : Vector2.zero;

        InputActionMap uiMap;
        InputActionMap playerMap;
        InputAction navigate;
        InputAction submit;
        InputAction cancel;
        InputAction move;
        Vector2Int lastNavigate;

        void OnEnable()
        {
            if (actions == null) return;

            uiMap = actions.FindActionMap("UI", throwIfNotFound: true);
            playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            navigate = uiMap.FindAction("Navigate", throwIfNotFound: true);
            submit = uiMap.FindAction("Submit", throwIfNotFound: true);
            cancel = uiMap.FindAction("Cancel", throwIfNotFound: true);
            move = playerMap.FindAction("Move", throwIfNotFound: true);

            navigate.performed += OnNavigate;
            navigate.canceled += OnNavigate;
            submit.performed += OnSubmit;
            cancel.performed += OnCancel;
        }

        void OnDisable()
        {
            if (navigate == null) return;

            navigate.performed -= OnNavigate;
            navigate.canceled -= OnNavigate;
            submit.performed -= OnSubmit;
            cancel.performed -= OnCancel;
            DisableAll();
        }

        public void EnableUI()
        {
            playerMap?.Disable();
            uiMap?.Enable();
            lastNavigate = Vector2Int.zero;
        }

        public void EnablePlayer()
        {
            uiMap?.Disable();
            playerMap?.Enable();
        }

        public void DisableAll()
        {
            uiMap?.Disable();
            playerMap?.Disable();
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
    }
}
