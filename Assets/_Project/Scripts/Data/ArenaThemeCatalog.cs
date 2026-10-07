using UnityEngine;

namespace Game2Week.Data
{
    [CreateAssetMenu(menuName="Game2Week/경기장 테마 목록")]
    public sealed class ArenaThemeCatalog : ScriptableObject
    {
        public ArenaTheme[] themes = System.Array.Empty<ArenaTheme>();
        public ArenaTheme Find(string id)
        {
            foreach(var theme in themes)if(theme && theme.id==id)return theme;
            return themes.Length>0?themes[0]:null;
        }
    }
}
