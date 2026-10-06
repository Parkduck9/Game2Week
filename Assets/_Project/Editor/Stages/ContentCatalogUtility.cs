using System.Linq;
using Game2Week.Data;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    public static class ContentCatalogUtility
    {
        public const string CatalogPath = "Assets/_Project/Data/ContentCatalog.asset";

        public static ContentCatalog LoadOrCreate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
            if (catalog != null) return catalog;

            catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            Refresh(catalog);
            return catalog;
        }

        /// <summary>프로젝트의 모든 EnemyData / AttackPatternData를 카탈로그에 채운다.</summary>
        public static void Refresh(ContentCatalog catalog)
        {
            catalog.SetContent(
                FindAll<EnemyData>().OrderBy(e => e.name),
                FindAll<AttackPatternData>().OrderBy(p => p.name));
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        static System.Collections.Generic.IEnumerable<T> FindAll<T>() where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null);
    }
}
