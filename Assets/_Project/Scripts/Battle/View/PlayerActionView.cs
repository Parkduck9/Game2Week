using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>리깅 없는 모델용 오른손·정지 자세 임시 표현. 완성 애니메이션으로 교체할 수 있다.</summary>
    public sealed class PlayerActionView : MonoBehaviour
    {
        static readonly Color BraceSettling = new(1f, 0.55f, 0.55f, 1f);
        static readonly Color BraceReadyColor = new(1f, 0.22f, 0.22f, 1f);
        const float BraceCrouch = 0.9f;

        PlayerMover player;
        Transform model, hand, braceDisc;
        Vector3 handRest = new(0.28f, 0.47f, 0.06f);
        Vector3 modelScale = Vector3.one;
        Material handMaterial, braceMaterial;
        float deflectionLeft;
        float outgoingSide = 1f;
        Animation.PlayerAnimationDriver animationDriver;
        public void Init(PlayerMover mover, Transform characterModel)
        {
            player = mover; model = characterModel;
            animationDriver=GetComponent<Animation.PlayerAnimationDriver>();
            if(animationDriver){animationDriver.Bind(mover);if(animationDriver.IsReady)return;}
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
            if (model) modelScale = model.localScale;

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "BraceCue";
            Destroy(disc.GetComponent<Collider>());
            braceDisc = disc.transform;
            braceDisc.SetParent(transform, false);
            braceDisc.localPosition = new Vector3(0f, 0.01f, 0f);
            braceDisc.localScale = new Vector3(0.7f, 0.005f, 0.7f);
            braceMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            disc.GetComponent<Renderer>().sharedMaterial = braceMaterial;
            disc.SetActive(false);
        }
        public void PlayDeflection(float side) { outgoingSide = side; deflectionLeft = 0.24f; if(animationDriver)animationDriver.SetParrySide(side); }
        void LateUpdate()
        {
            if (!player || player.Motor == null || Time.timeScale <= 0f) return;
            if(animationDriver&&animationDriver.IsReady)return;
            deflectionLeft = Mathf.Max(0f, deflectionLeft - Time.deltaTime);
            float progress = deflectionLeft > 0f ? 1f - deflectionLeft / 0.24f : player.Motor.ParryProgress;
            bool parry = deflectionLeft > 0f || player.Motor.Parrying;
            if (parry)
            {
                float side = deflectionLeft > 0f ? outgoingSide : 1f;
                hand.localPosition = new Vector3(Mathf.Lerp(-side * 0.34f, side * 0.38f, progress), 0.48f + Mathf.Sin(progress * Mathf.PI) * 0.12f, 0.32f);
                hand.localScale = Vector3.one * 0.16f;
            }
            else if (player.Motor.Bracing) { hand.localPosition = new Vector3(0f, 0.44f, 0.24f); hand.localScale = Vector3.one * 0.13f; }
            else { hand.localPosition = handRest; hand.localScale = Vector3.one * 0.10f; }
            if (model)
            {
                model.localRotation = Quaternion.Euler(player.Motor.Dodging ? 18f : (player.Motor.Airborne ? -10f : 0f), 0f, 0f);
                model.localScale = player.Motor.Bracing ? new Vector3(modelScale.x, modelScale.y * BraceCrouch, modelScale.z) : modelScale;
            }
            braceDisc.gameObject.SetActive(player.Motor.Bracing);
            if (player.Motor.Bracing) braceMaterial.color = player.Motor.BraceReady ? BraceReadyColor : BraceSettling;
        }
        void OnDestroy() { if (handMaterial) Destroy(handMaterial); if (braceMaterial) Destroy(braceMaterial); }
    }
}
