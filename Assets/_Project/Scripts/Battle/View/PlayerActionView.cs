using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>리깅 없는 모델용 오른손 임시 표현. 완성 애니메이션으로 교체할 수 있다.</summary>
    public sealed class PlayerActionView : MonoBehaviour
    {
        PlayerMover player;
        Transform model, hand;
        Vector3 handRest = new(0.28f, 0.47f, 0.06f);
        Material handMaterial;
        float deflectionLeft;
        float outgoingSide = 1f;
        public void Init(PlayerMover mover, Transform characterModel)
        {
            player = mover; model = characterModel;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "RightHand_ParryCue";
            Destroy(go.GetComponent<Collider>());
            hand = go.transform;
            hand.SetParent(transform, false);
            hand.localPosition = handRest;
            hand.localScale = Vector3.one * 0.10f;
            handMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            handMaterial.color = new Color(1f, 0.82f, 0.18f);
            go.GetComponent<Renderer>().sharedMaterial = handMaterial;
        }
        public void PlayDeflection(float side) { outgoingSide = side; deflectionLeft = 0.24f; }
        void LateUpdate()
        {
            if (!player || player.Motor == null || Time.timeScale <= 0f) return;
            deflectionLeft = Mathf.Max(0f, deflectionLeft - Time.deltaTime);
            float progress = deflectionLeft > 0f ? 1f - deflectionLeft / 0.24f : player.Motor.ParryProgress;
            bool parry = deflectionLeft > 0f || player.Motor.Parrying;
            if (parry)
            {
                float side = deflectionLeft > 0f ? outgoingSide : 1f;
                hand.localPosition = new Vector3(Mathf.Lerp(-side * 0.34f, side * 0.38f, progress), 0.48f + Mathf.Sin(progress * Mathf.PI) * 0.12f, 0.32f);
                hand.localScale = Vector3.one * 0.16f;
            }
            else { hand.localPosition = handRest; hand.localScale = Vector3.one * 0.10f; }
            if (model) model.localRotation = Quaternion.Euler(player.Motor.Dodging ? 18f : (player.Motor.Airborne ? -10f : 0f), 0f, 0f);
        }
        void OnDestroy() { if (handMaterial) Destroy(handMaterial); }
    }
}
