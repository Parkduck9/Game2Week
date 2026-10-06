using Game2Week.Core;
using TMPro;
using UnityEngine;

namespace Game2Week.Battle.View
{
    public sealed class ActionStatusView : MonoBehaviour
    {
        [SerializeField] BattleWorld world;
        [SerializeField] BattleCameraDirector cameraDirector;
        [SerializeField] InputReader input;
        [SerializeField] TMP_Text label;
        void LateUpdate()
        {
            if (!label || !world || !world.Player) return;
            bool active = input && input.CurrentMode == InputReader.Mode.Player;
            label.gameObject.SetActive(active);
            if (!active) return;
            var motor = world.Player.Motor;
            string dodge = motor.DodgeCooldown > 0f ? $"회피 {motor.DodgeCooldown:0.0}초" : "회피 준비";
            string parry = motor.ParryCooldown > 0f ? $"쳐내기 {motor.ParryCooldown:0.0}초" : "쳐내기 준비";
            label.text = $"{dodge}  ·  {parry}  ·  {(cameraDirector.IsLockedOn ? "록온" : "자유 시점")}";
        }
    }
}
