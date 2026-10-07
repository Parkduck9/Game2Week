using System;
using System.Linq;
using Game2Week.Dialogue;
using Game2Week.Stages;
using Game2Week.EditorTools.Stages;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Dialogue
{
    public sealed class DialogueEditorWindow : EditorWindow
    {
        DialogueEditorModel model;
        DialogueRunner preview;
        int selected, stageIndex;
        Vector2 scroll;
        string message = string.Empty;
        [MenuItem("Tools/Dialogue Editor")]
        public static void Open() { var window = GetWindow<DialogueEditorWindow>("대화 편집기"); window.minSize = new Vector2(960, 620); }
        void OnEnable()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Build.ReleasePreparation.FontPath);
            model = new DialogueEditorModel(new DialogueRepository(DialogueRepository.DefaultDirectory), c => font && font.HasCharacter(c));
            if (model.Files.Count > 0) model.Load(model.Files[0]);
        }
        bool LeaveCurrent()
        {
            if (!model.Dirty) return true;
            int result = EditorUtility.DisplayDialogComplex("대화 편집기", "저장하지 않은 변경이 있습니다.", "저장", "취소", "버리기");
            if (result == 1) return false;
            if (result == 0) { try { model.Save(); } catch (Exception e) { message = e.Message; return false; } }
            return true;
        }
        void OnGUI()
        {
            if (model == null) OnEnable();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("새 대화", EditorStyles.toolbarButton) && LeaveCurrent()) model.New("dlg_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                if (GUILayout.Button("저장", EditorStyles.toolbarButton)) { try { model.Save(); AssetDatabase.Refresh(); message = "검증·체크섬 저장 완료"; } catch (Exception e) { message = e.Message; } }
                using (new EditorGUI.DisabledScope(!model.CanUndo)) if (GUILayout.Button("되돌리기", EditorStyles.toolbarButton)) model.Undo();
                using (new EditorGUI.DisabledScope(!model.CanRedo)) if (GUILayout.Button("다시 하기", EditorStyles.toolbarButton)) model.Redo();
                if (GUILayout.Button("미리 보기", EditorStyles.toolbarButton)) { try { preview = new DialogueRunner(model.Current); } catch (Exception e) { message = e.Message; } }
                stageIndex = Mathf.Max(0, EditorGUILayout.IntField("스테이지 번호", stageIndex + 1, GUILayout.Width(170)) - 1);
                if (GUILayout.Button("바로 플레이", EditorStyles.toolbarButton)) Play();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(210)))
                {
                    EditorGUILayout.LabelField("대화 파일", EditorStyles.boldLabel);
                    foreach (string id in model.Files)
                        if (GUILayout.Button(id) && LeaveCurrent()) { model.Load(id); selected = 0; preview = null; }
                }
                if (model.Current != null) DrawDefinition();
            }
            if (preview != null)
            {
                EditorGUILayout.HelpBox(preview.Ended ? "대화 종료" : preview.Current.text, MessageType.Info);
                if (!preview.Ended)
                {
                    if (preview.Current.choices.Count == 0) { if (GUILayout.Button("다음 줄")) preview.Advance(); }
                    else for (int i = 0; i < preview.Current.choices.Count; i++) if (GUILayout.Button(preview.Current.choices[i].text)) { preview.Advance(i); break; }
                }
            }
            foreach (string error in model.Validate()) EditorGUILayout.HelpBox(error, MessageType.Error);
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
        }
        void DrawDefinition()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(180)))
            {
                EditorGUILayout.LabelField("노드", EditorStyles.boldLabel);
                for (int i = 0; i < model.Current.nodes.Count; i++) if (GUILayout.Toggle(selected == i, model.Current.nodes[i].id, "Button")) selected = i;
                if (GUILayout.Button("노드 추가")) { model.AddNode(); selected = model.Current.nodes.Count - 1; }
            }
            using (new EditorGUILayout.VerticalScope())
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(position.height - 245));
                var definition = model.Current;
                Text("파일 이름", definition.id, value => model.Edit(d => d.id = value));
                Text("시작 노드", definition.start, value => model.Edit(d => d.start = value));
                foreach (var entry in definition.speakers.ToArray())
                {
                    string key = entry.Key;
                    Text(key + " 표시 이름", entry.Value.name, value => model.Edit(d => d.speakers[key].name = value));
                    Text(key + " 위치", entry.Value.anchor, value => model.Edit(d => d.speakers[key].anchor = value));
                }
                if (GUILayout.Button("화자 추가")) model.Edit(d => d.speakers["speaker" + d.speakers.Count] = new DialogueSpeaker());
                if (definition.nodes.Count > 0)
                {
                    selected = Mathf.Clamp(selected, 0, definition.nodes.Count - 1); var node = definition.nodes[selected];
                    Text("노드 이름", node.id, value => EditNode(n => n.id = value));
                    Text("화자 이름", node.speaker, value => EditNode(n => n.speaker = value));
                    EditorGUILayout.LabelField("대사"); string text = EditorGUILayout.TextArea(node.text, GUILayout.MinHeight(65));
                    if (text != node.text) EditNode(n => n.text = text);
                    Text("자세", node.pose, value => EditNode(n => n.pose = value));
                    Text("카메라 컷", node.camera, value => EditNode(n => n.camera = value));
                    Text("다음 노드", node.next, value => EditNode(n => n.next = value));
                    bool end = EditorGUILayout.Toggle("대화 종료", node.end); if (end != node.end) EditNode(n => n.end = end);
                    for (int i = 0; i < node.choices.Count; i++)
                    {
                        int index = i; var choice = node.choices[i];
                        Text("선택지 " + (i + 1), choice.text, value => EditNode(n => n.choices[index].text = value));
                        Text("선택 후 노드", choice.next, value => EditNode(n => n.choices[index].next = value));
                        DrawEffects(choice.effects, change => EditNode(n => change(n.choices[index].effects)));
                        if (GUILayout.Button("선택지 삭제 " + (i + 1))) { EditNode(n => n.choices.RemoveAt(index)); break; }
                    }
                    if (GUILayout.Button("선택지 추가")) EditNode(n => n.choices.Add(new DialogueChoice { next = "end" }));
                    DrawEffects(node.effects, change => EditNode(n => change(n.effects)));
                    if (GUILayout.Button("선택 노드 삭제")) EditNodeList(d => d.nodes.RemoveAt(selected));
                }
                EditorGUILayout.EndScrollView();
            }
        }
        void DrawEffects(System.Collections.Generic.List<DialogueEffect> effects, Action<Action<System.Collections.Generic.List<DialogueEffect>>> edit)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                int index = i; var effect = effects[i];
                Text("효과 종류", effect.type, value => edit(list => list[index].type = value));
                Text("조건·플래그 이름", effect.id, value => edit(list => list[index].id = value));
                bool value = EditorGUILayout.Toggle("설정 값", effect.value); if (value != effect.value) edit(list => list[index].value = value);
                if (GUILayout.Button("효과 삭제")) { edit(list => list.RemoveAt(index)); break; }
            }
            if (GUILayout.Button("효과 추가")) edit(list => list.Add(new DialogueEffect()));
        }
        void EditNode(Action<DialogueNode> change) => model.Edit(d => change(d.nodes[selected]));
        void EditNodeList(Action<DialogueDefinition> change) => model.Edit(change);
        static void Text(string label, string current, Action<string> change)
        { string value = EditorGUILayout.TextField(label, current ?? string.Empty); if (value != current) change(value); }
        void Play()
        {
            try
            {
                model.Save();
                var catalog = AssetDatabase.LoadAssetAtPath<Game2Week.Data.ContentCatalog>("Assets/_Project/Data/ContentCatalog.asset");
                var stages = new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory), catalog); stages.Load();
                if (stageIndex >= stages.Count) throw new InvalidOperationException("없는 스테이지 번호입니다.");
                stages.SelectStage(stageIndex); stages.Edit(s => s.dialogues.intro = model.Current.id);
                StageQuickPlay.Start(stages);
            }
            catch (Exception e) { message = e.Message; }
        }
    }
}
