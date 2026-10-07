using System;
using Game2Week.Battle.Patterns;
using Game2Week.Data.Patterns;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public sealed class SequenceEditorPanel : IDisposable
    {
        AttackSequence selected, draft;
        SerializedObject fields;
        readonly PatternPreview preview = new();
        Vector2 scroll;
        float time;
        bool dirty;
        string notice = "";
        void Select(AttackSequence asset)
        {
            if (dirty && !EditorUtility.DisplayDialog("저장 전 변경", "변경을 버리고 다른 시퀀스를 열까요?", "버리기", "취소")) return;
            if (draft) UnityEngine.Object.DestroyImmediate(draft);
            selected = asset; draft = UnityEngine.Object.Instantiate(asset);
            draft.hideFlags = HideFlags.HideAndDontSave; fields = new SerializedObject(draft);
            dirty = false; time = 0;
        }
        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(); EditorGUILayout.BeginVertical(GUILayout.Width(205));
            if (GUILayout.Button("새 시퀀스")) Select(SequenceAssetStore.Create());
            if (GUILayout.Button("샘플 추가")) SequenceAssetStore.GenerateSample();
            foreach (var asset in SequenceAssetStore.List())
                if (GUILayout.Toggle(selected == asset, asset.name, "Button") && selected != asset) Select(asset);
            EditorGUILayout.EndVertical(); EditorGUILayout.BeginVertical();
            if (draft)
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                fields.Update(); EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(fields.FindProperty("seed"), new GUIContent("고정 시드"));
                EditorGUILayout.PropertyField(fields.FindProperty("entries"), new GUIContent("시간표: 시각·패턴·반복·횟수·간격·지속 시간"), true);
                if (EditorGUI.EndChangeCheck()) { fields.ApplyModifiedProperties(); dirty = true; }
                var errors = SequenceSchedule.Validate(draft);
                foreach (var error in errors) EditorGUILayout.HelpBox(error, MessageType.Error);
                using (new EditorGUI.DisabledScope(errors.Count > 0))
                    if (GUILayout.Button("저장 · 프리팹 · 카탈로그 등록"))
                        try { EditorUtility.CopySerialized(draft, selected); selected.hideFlags = HideFlags.None; SequenceAssetStore.Save(selected); dirty = false; notice = "저장 완료"; }
                        catch (Exception error) { notice = error.Message; }
                time = EditorGUILayout.Slider("시간 미리보기", time, 0, 30);
                var rect = GUILayoutUtility.GetRect(300, 300, GUILayout.ExpandWidth(true));
                if (errors.Count == 0)
                {
                    if (Event.current.type == EventType.Repaint) preview.DrawSequence(rect, draft, time);
                    foreach (var start in SequenceSchedule.Build(draft))
                        EditorGUILayout.LabelField($"{start.Time:0.00}초 ━ {start.Pattern.DisplayName} ━ {start.Duration:0.00}초 · 시드 {start.Seed}");
                }
                EditorGUILayout.HelpBox("시간 슬라이더로 합쳐진 탄 궤적을 확인합니다. 그래프 패턴은 실제 궤적으로 표시하며 다른 패턴은 시간표로 확인하세요. 지속 시간이 끝나면 탄도 정리됩니다.", MessageType.Info);
                if (!string.IsNullOrEmpty(notice)) EditorGUILayout.LabelField(notice);
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical(); EditorGUILayout.EndHorizontal();
        }
        public void Dispose() { preview.Dispose(); if (draft) UnityEngine.Object.DestroyImmediate(draft); }
    }
}
