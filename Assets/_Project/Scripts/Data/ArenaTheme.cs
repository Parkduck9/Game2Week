using UnityEngine;

namespace Game2Week.Data
{
    [CreateAssetMenu(menuName="Game2Week/경기장 테마")]
    public sealed class ArenaTheme : ScriptableObject
    {
        public string id="default";
        public Color floor = new(.16f,.21f,.27f), wall = new(.35f,.48f,.56f), accent = new(.3f,.75f,.8f);
        public Material propMaterial;
        public int propCount = 8;
        public bool checker;
    }
}
