using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Game2Week.Core;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Build
{
    public static class ReleasePreparation
    {
        public const string FontPath="Assets/_Project/Art/Fonts/Pretendard-Regular SDF.asset";
        public static void Prepare()
        {
            const string path="Assets/_Project/Data/ProductInfo.asset";
            var info=AssetDatabase.LoadAssetAtPath<ProductInfo>(path);
            if(!info){info=ScriptableObject.CreateInstance<ProductInfo>();AssetDatabase.CreateAsset(info,path);}
            var preloaded=PlayerSettings.GetPreloadedAssets().Where(a=>a).ToList();if(!preloaded.Contains(info)){preloaded.Add(info);PlayerSettings.SetPreloadedAssets(preloaded.ToArray());}
            PrepareFont(FontPath);PrepareFont(FontPath.Replace("Regular","Bold"));AssetDatabase.SaveAssets();
            Debug.Log("제품 설정과 정적 폰트 준비 완료");
        }
        public static HashSet<char> KoreanInContent()
        {
            var chars=new HashSet<char>();
            foreach(string root in new[]{"Assets/_Project/Scripts","Assets/_Project/Data","Assets/StreamingAssets"})
                foreach(var path in Directory.GetFiles(root,"*",SearchOption.AllDirectories).Where(p=>p.EndsWith(".cs")||p.EndsWith(".asset")||p.EndsWith(".json")))
                    foreach(char c in File.ReadAllText(path))if(c>='가'&&c<='힣')chars.Add(c);
            return chars;
        }
        public static bool IsStaticFontCovered(string path,string characters)
        {
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            return font&&font.atlasPopulationMode==AtlasPopulationMode.Static&&characters.All(c=>font.HasCharacter(c));
        }
        static void PrepareFont(string path)
        {
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(!font)throw new InvalidOperationException("기존 폰트 에셋이 없습니다.");
            var chars=new HashSet<char>(File.ReadAllText("Assets/_Project/Art/Fonts/korean_static_characters.txt"));
            chars.UnionWith(KoreanInContent());for(int i=32;i<127;i++)chars.Add((char)i);
            chars.UnionWith("·→←↑↓▲▼◀▶×★☆✖⚠…●○");
            font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            var serialized=new SerializedObject(font);serialized.FindProperty("m_AtlasWidth").intValue=4096;serialized.FindProperty("m_AtlasHeight").intValue=4096;serialized.ApplyModifiedPropertiesWithoutUndo();
            font.ClearFontAssetData();
            font.TryAddCharacters(new string(chars.OrderBy(c=>c).ToArray()),out string missing);
            font.atlasPopulationMode=AtlasPopulationMode.Static;
            var missingKorean=missing.Where(c=>c>='가'&&c<='힣').ToArray();
            if(missingKorean.Length>0)throw new InvalidOperationException("한글 누락: "+new string(missingKorean));
            foreach(var texture in font.atlasTextures)if(texture){EditorUtility.SetDirty(texture);if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))AssetDatabase.AddObjectToAsset(texture,font);}
            EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/font_coverage_"+Path.GetFileNameWithoutExtension(path)+".txt",$"한글 콘텐츠 {KoreanInContent().Count}자 / 전체 {font.characterTable.Count}자 / 페이지 {font.atlasTextures.Length}\n지원하지 않는 기타 기호: {missing}");
        }
    }
}
