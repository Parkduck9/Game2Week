using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle.View
{
    public sealed class BulletAppearance : MonoBehaviour
    {
        [SerializeField] MeshFilter visual;
        [SerializeField] Mesh sphere, crystal, ring;
        [SerializeField] TrailRenderer trail;
        public Mesh CurrentMesh => visual ? visual.sharedMesh : null;
        public void Show(AttackColor color)
        {
            if(visual)visual.sharedMesh=color switch { AttackColor.Red=>crystal,AttackColor.Blue=>ring,_=>sphere };
            if(trail)
            {
                trail.Clear();trail.emitting=true;
                var tint=BattleTexts.AttackColorTint(color);trail.startColor=tint;trail.endColor=new Color(tint.r,tint.g,tint.b,0);
            }
        }
        void OnDisable(){if(trail){trail.emitting=false;trail.Clear();}}
    }
}
