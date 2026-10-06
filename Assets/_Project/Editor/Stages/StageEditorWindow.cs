using System.Collections.Generic;
using System.Linq;
using Game2Week.Data;
using Game2Week.Stages;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    /// <summary>
    /// 스테이지 맵툴. 편집 로직은 <see cref="StageEditorModel"/>에 있고, 이 창은 그리기와 입력 전달만 한다.
    /// 스테이지 JSON은 이 창으로만 고친다.
    /// </summary>
    public sealed class StageEditorWindow : EditorWindow
    {
        const float ListWidth = 190f;
        const float InspectorWidth = 270f;
        const float IssuesHeight = 96f;

        static readonly Color FloorColor = new(0.22f, 0.23f, 0.27f);
        static readonly Color FloorAltColor = new(0.25f, 0.26f, 0.30f);
        static readonly Color PlayerColor = new(0.91f, 0.70f, 0.14f);
        static readonly Color EnemyColor = new(0.54f, 0.42f, 0.82f);
        static readonly Color ErrorColor = new(0.86f, 0.25f, 0.2f);
        static readonly Dictionary<string, Color> GemColors = new()
        {
            [GemTypes.Heal] = new Color(0.25f, 0.75f, 0.45f),
            [GemTypes.Attack] = new Color(0.95f, 0.45f, 0.2f),
            [GemTypes.Spare] = new Color(0.95f, 0.5f, 0.75f),
        };
        static readonly Dictionary<string, string> GemLabels = new()
        {
            [GemTypes.Heal] = "회복", [GemTypes.Attack] = "공격", [GemTypes.Spare] = "살림",
        };
        static readonly string[] ToolLabels = { "선택/이동", "시작점", "적", "보석", "지우개", "영역 보석", "영역 삭제" };

        StageEditorModel model;
        ContentCatalog catalog;
        Vector2 listScroll;
        Vector2 inspectorScroll;
        Vector2 issuesScroll;
        string statusMessage = string.Empty;
        bool dragging;
        bool preview3D,regionDragging;
        GridPoint regionFrom,regionTo;
        StagePreview3D stagePreview;
        float previewYaw=20,previewPitch=45;

        [MenuItem("Tools/Stage Editor")]
        public static void Open()
        {
            var window = GetWindow<StageEditorWindow>("Stage Editor");
            window.minSize = new Vector2(900, 520);
            window.Show();
        }

        void OnEnable()
        {
            catalog = ContentCatalogUtility.LoadOrCreate();
            model = new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory), catalog);
            model.Load();
            stagePreview=new StagePreview3D();
            statusMessage = $"스테이지 {model.Count}개 불러옴";
        }

        void OnDestroy()
        {
            stagePreview?.Dispose();
            if (model == null || !model.HasUnsavedChanges) return;
            if (EditorUtility.DisplayDialog("Stage Editor", "저장하지 않은 변경이 있어요. 저장할까요?", "저장", "버리기"))
                Save();
        }

        void OnGUI()
        {
            HandleShortcuts();
            DrawToolbar();
            DrawBatchToolbar();

            var body = new Rect(0, EditorGUIUtility.singleLineHeight * 2 + 12, position.width, position.height - EditorGUIUtility.singleLineHeight * 2 - 12 - IssuesHeight);
            DrawStageList(new Rect(body.x, body.y, ListWidth, body.height));
            var canvas=new Rect(body.x + ListWidth, body.y, body.width - ListWidth - InspectorWidth, body.height);
            if(preview3D&&model.Current!=null)
            {
                if(Event.current.type==EventType.Repaint)GUI.DrawTexture(canvas,stagePreview.Render(canvas,model.Current,previewYaw,previewPitch));
                if(Event.current.type==EventType.MouseDrag&&canvas.Contains(Event.current.mousePosition)){previewYaw+=Event.current.delta.x;previewPitch=Mathf.Clamp(previewPitch-Event.current.delta.y,10,80);Event.current.Use();Repaint();}
            }
            else DrawCanvas(canvas);
            DrawInspector(new Rect(body.xMax - InspectorWidth, body.y, InspectorWidth, body.height));
            DrawIssues(new Rect(0, body.yMax, position.width, IssuesHeight));
        }

        // ---------- 툴바 ----------

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                model.Tool = (StageTool)GUILayout.Toolbar((int)model.Tool, ToolLabels, EditorStyles.toolbarButton, GUILayout.Width(420));
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(!model.CanUndo))
                    if (GUILayout.Button("되돌리기", EditorStyles.toolbarButton)) model.Undo();
                using (new EditorGUI.DisabledScope(!model.CanRedo))
                    if (GUILayout.Button("다시 실행", EditorStyles.toolbarButton)) model.Redo();

                if (GUILayout.Button("카탈로그 새로고침", EditorStyles.toolbarButton))
                {
                    ContentCatalogUtility.Refresh(catalog);
                    statusMessage = $"카탈로그: 적 {catalog.Enemies.Count}, 패턴 {catalog.Patterns.Count}";
                }
                if (GUILayout.Button("다시 불러오기", EditorStyles.toolbarButton) &&
                    (!model.HasUnsavedChanges || EditorUtility.DisplayDialog("Stage Editor", "저장하지 않은 변경을 버리고 다시 불러올까요?", "다시 불러오기", "취소")))
                {
                    model.Load();
                    statusMessage = "다시 불러옴";
                }

                var saveLabel = model.HasUnsavedChanges ? "저장 *" : "저장";
                if (GUILayout.Button(saveLabel, EditorStyles.toolbarButton, GUILayout.Width(60))) Save();
            }
        }

        void DrawBatchToolbar()
        {
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                preview3D=GUILayout.Toggle(preview3D,"3D 미리보기",EditorStyles.toolbarButton,GUILayout.Width(110));
                if(GUILayout.Button("바로 플레이",EditorStyles.toolbarButton,GUILayout.Width(100)))statusMessage=StageQuickPlay.Start(model)?"선택 스테이지 플레이":"검증 오류·씬 저장 취소로 실행하지 않았습니다.";
                if(GUILayout.Button("보석 좌우 복사",EditorStyles.toolbarButton,GUILayout.Width(110)))statusMessage=$"보석 {model.MirrorGems(true)}개 복사";
                if(GUILayout.Button("보석 상하 복사",EditorStyles.toolbarButton,GUILayout.Width(110)))statusMessage=$"보석 {model.MirrorGems(false)}개 복사";
                GUILayout.Label(preview3D?"드래그: 시점 회전":"영역 도구: 드래그 후 놓으면 일괄 편집",EditorStyles.miniLabel);
            }
        }

        void HandleShortcuts()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown || !(e.control || e.command)) return;
            switch (e.keyCode)
            {
                case KeyCode.Z: model.Undo(); e.Use(); break;
                case KeyCode.Y: model.Redo(); e.Use(); break;
                case KeyCode.S: Save(); e.Use(); break;
            }
        }

        void Save()
        {
            var report = model.SaveAll();
            if (report.Blocked.Count > 0)
            {
                statusMessage = "저장 안 됨 — 오류: " + string.Join(", ", report.Blocked.Select(b => $"{b.id} ({b.error})"));
                EditorUtility.DisplayDialog("Stage Editor", "오류가 있는 스테이지가 있어 저장하지 않았어요.\n\n" +
                    string.Join("\n", report.Blocked.Select(b => $"• {b.id}: {b.error}")), "확인");
                return;
            }
            AssetDatabase.Refresh();
            statusMessage = report.Saved.Count + report.Deleted.Count == 0 && !report.IndexSaved
                ? "바뀐 내용 없음"
                : $"저장: {string.Join(", ", report.Saved)}" + (report.Deleted.Count > 0 ? $" · 삭제: {string.Join(", ", report.Deleted)}" : string.Empty);
        }

        // ---------- 스테이지 목록 ----------

        void DrawStageList(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            EditorGUILayout.LabelField("스테이지 (진행 순서)", EditorStyles.boldLabel);
            listScroll = EditorGUILayout.BeginScrollView(listScroll);
            for (int i = 0; i < model.Count; i++)
            {
                var stage = model.StageAt(i);
                var label = $"{i + 1}. {stage.name}{(model.IsDirty(i) ? " *" : string.Empty)}";
                var style = i == model.CurrentIndex ? EditorStyles.toolbarButton : EditorStyles.label;
                if (GUILayout.Toggle(i == model.CurrentIndex, label, style) && i != model.CurrentIndex) model.SelectStage(i);
            }
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("추가")) model.AddStage();
                using (new EditorGUI.DisabledScope(model.Current == null))
                {
                    if (GUILayout.Button("복제")) model.DuplicateCurrent();
                    if (GUILayout.Button("삭제") && EditorUtility.DisplayDialog("Stage Editor", $"{model.Current.name} 스테이지를 삭제할까요?\n(저장해야 파일이 지워집니다)", "삭제", "취소"))
                        model.DeleteCurrent();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledScope(model.Current == null))
            {
                if (GUILayout.Button("▲ 위로")) model.MoveCurrent(-1);
                if (GUILayout.Button("▼ 아래로")) model.MoveCurrent(1);
            }
            GUILayout.EndArea();
        }

        // ---------- 격자 캔버스 ----------

        void DrawCanvas(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.17f));
            var stage = model.Current;
            if (stage == null)
            {
                GUI.Label(rect, "스테이지가 없어요. 왼쪽에서 \"추가\"를 누르세요.", CenteredStyle(Color.gray));
                return;
            }

            var grid = stage.grid;
            int w = Mathf.Max(1, grid.width), d = Mathf.Max(1, grid.depth);
            float px = Mathf.Floor(Mathf.Clamp(Mathf.Min((rect.width - 40) / w, (rect.height - 40) / d), 8f, 48f));
            var origin = new Vector2(rect.x + (rect.width - px * w) * 0.5f, rect.y + (rect.height - px * d) * 0.5f);
            Rect CellRect(GridPoint c) => new(origin.x + c.x * px, origin.y + (d - 1 - c.z) * px, px - 1, px - 1);

            // 바닥 (z가 큰 쪽 = 적 쪽 = 화면 위)
            for (int x = 0; x < w; x++)
            for (int z = 0; z < d; z++)
                EditorGUI.DrawRect(CellRect(new GridPoint(x, z)), (x + z) % 2 == 0 ? FloorColor : FloorAltColor);

            GUI.Label(new Rect(origin.x, origin.y - 18, px * w, 16), "▲ 적 쪽", CenteredStyle(Color.gray));
            GUI.Label(new Rect(origin.x, origin.y + px * d + 2, px * w, 16), $"▼ 카메라 쪽 · {w} × {d}칸 = {w * grid.cellSize:0.##} × {d * grid.cellSize:0.##}m", CenteredStyle(Color.gray));

            // 배치물
            DrawItem(CellRect(stage.playerStart), PlayerColor, "주", model.Selection.Kind == StageItemKind.PlayerStart);
            DrawItem(CellRect(stage.enemy.position), EnemyColor, "적", model.Selection.Kind == StageItemKind.Enemy);
            foreach (var gem in stage.gems)
            {
                var color = GemColors.TryGetValue(gem.type, out var c) ? c : Color.gray;
                var label = GemLabels.TryGetValue(gem.type, out var l) ? l : "?";
                DrawItem(Shrink(CellRect(gem.position), px * 0.15f), color, px >= 28 ? label : label.Substring(0, 1),
                    model.Selection.Kind == StageItemKind.Gem && model.Selection.GemId == gem.id);
            }

            // 오류 칸 강조
            foreach (var issue in model.ValidateCurrent())
                if (issue.Severity == IssueSeverity.Error && issue.Cell is { } cell && StageGeometry.InBounds(grid, cell))
                    DrawOutline(CellRect(cell), ErrorColor, 2f);

            HandleCanvasInput(rect, origin, px, w, d);
            if(regionDragging)
            {
                var a=CellRect(regionFrom);var b=CellRect(regionTo);
                DrawOutline(Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.xMax,b.xMax),Mathf.Max(a.yMax,b.yMax)),Color.cyan,2);
            }
        }

        void HandleCanvasInput(Rect rect, Vector2 origin, float px, int w, int d)
        {
            var e = Event.current;
            if (!rect.Contains(e.mousePosition) && e.type != EventType.MouseUp) return;

            var local = e.mousePosition - origin;
            var cell = new GridPoint(Mathf.FloorToInt(local.x / px), d - 1 - Mathf.FloorToInt(local.y / px));
            bool inside = local.x >= 0 && local.y >= 0 && cell.x < w && cell.z >= 0;
            if(model.Tool==StageTool.GemRegion||model.Tool==StageTool.EraseRegion)
            {
                if(e.type==EventType.MouseDown&&inside&&e.button==0){regionFrom=regionTo=cell;regionDragging=true;e.Use();}
                else if(e.type==EventType.MouseDrag&&regionDragging){regionTo=new GridPoint(Mathf.Clamp(cell.x,0,w-1),Mathf.Clamp(cell.z,0,d-1));e.Use();Repaint();}
                else if(e.type==EventType.MouseUp&&regionDragging){statusMessage=$"보석 {model.EditGemRegion(regionFrom,regionTo,model.Tool==StageTool.EraseRegion)}개 변경";regionDragging=false;e.Use();Repaint();}
                return;
            }

            switch (e.type)
            {
                case EventType.MouseDown when inside && e.button == 0:
                    if (model.Tool == StageTool.Select)
                    {
                        model.BeginDrag(cell);
                        dragging = model.Selection.Kind != StageItemKind.None;
                    }
                    else model.Click(cell);
                    e.Use();
                    Repaint();
                    break;
                case EventType.MouseDown when inside && e.button == 1:
                    model.EraseAt(cell);
                    e.Use();
                    Repaint();
                    break;
                case EventType.MouseDrag when dragging && inside:
                    if (model.DragTo(cell)) Repaint();
                    e.Use();
                    break;
                case EventType.MouseUp when dragging:
                    model.EndDrag();
                    dragging = false;
                    e.Use();
                    break;
            }
        }

        static void DrawItem(Rect rect, Color color, string label, bool selected)
        {
            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, label, CenteredStyle(Color.black, bold: true));
            if (selected) DrawOutline(rect, Color.white, 2f);
        }

        static void DrawOutline(Rect r, Color color, float t)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), color);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), color);
            EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), color);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), color);
        }

        static Rect Shrink(Rect r, float by) => new(r.x + by, r.y + by, r.width - by * 2, r.height - by * 2);

        static GUIStyle CenteredStyle(Color color, bool bold = false) => new(bold ? EditorStyles.boldLabel : EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            clipping = TextClipping.Clip,
        };

        // ---------- 속성 패널 ----------

        void DrawInspector(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
            var stage = model.Current;
            if (stage == null)
            {
                EditorGUILayout.LabelField("선택된 스테이지 없음");
            }
            else
            {
                EditorGUIUtility.labelWidth = 92;
                EditorGUILayout.LabelField("스테이지", EditorStyles.boldLabel);
                Field(EditorGUILayout.DelayedTextField("id", stage.id), stage.id, v => model.Edit(s => s.id = v));
                Field(EditorGUILayout.DelayedTextField("이름", stage.name), stage.name, v => model.Edit(s => s.name = v));

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("맵 크기", EditorStyles.boldLabel);
                Field(EditorGUILayout.IntSlider("가로 (칸)", stage.grid.width, StageLimits.MinCells, StageLimits.MaxCells), stage.grid.width, v => model.Edit(s => s.grid.width = v));
                Field(EditorGUILayout.IntSlider("세로 (칸)", stage.grid.depth, StageLimits.MinCells, StageLimits.MaxCells), stage.grid.depth, v => model.Edit(s => s.grid.depth = v));
                Field(EditorGUILayout.Slider("셀 크기 (m)", stage.grid.cellSize, StageLimits.MinCellSize, StageLimits.MaxCellSize), stage.grid.cellSize, v => model.Edit(s => s.grid.cellSize = Mathf.Round(v * 20f) / 20f));

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("적", EditorStyles.boldLabel);
                var enemyNames = catalog.Enemies.Where(x => x).Select(x => x.name).ToList();
                if (!enemyNames.Contains(stage.enemy.enemyId)) enemyNames.Insert(0, stage.enemy.enemyId);
                int enemyIndex = EditorGUILayout.Popup("종류", enemyNames.IndexOf(stage.enemy.enemyId), enemyNames.ToArray());
                if (enemyIndex >= 0) Field(enemyNames[enemyIndex], stage.enemy.enemyId, v => model.Edit(s => s.enemy.enemyId = v));
                EditorGUILayout.LabelField("위치", stage.enemy.position.ToString());

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("탄막 턴", EditorStyles.boldLabel);
                Field(EditorGUILayout.Slider("시간 (초)", stage.enemyTurn.duration, StageLimits.MinTurnDuration, StageLimits.MaxTurnDuration), stage.enemyTurn.duration,
                    v => model.Edit(s => s.enemyTurn.duration = Mathf.Round(v * 2f) / 2f));
                EditorGUILayout.LabelField("사용할 패턴 (없으면 적 기본 패턴)", EditorStyles.miniLabel);
                foreach (var pattern in catalog.Patterns.Where(x => x))
                {
                    bool on = stage.enemyTurn.patterns.Contains(pattern.name);
                    bool next = EditorGUILayout.ToggleLeft(pattern.name, on);
                    if (next != on) model.Edit(s => { if (next) s.enemyTurn.patterns.Add(pattern.name); else s.enemyTurn.patterns.Remove(pattern.name); });
                }

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("보석", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("배치된 위치", $"{stage.gems.Count}개");
                Field(EditorGUILayout.IntSlider("한 턴 최대", stage.gemRules.maxPerTurn, 0, 5), stage.gemRules.maxPerTurn, v => model.Edit(s => s.gemRules.maxPerTurn = v));

                if (model.Selection.Kind == StageItemKind.Gem && model.FindGem(model.Selection.GemId) is { } gem)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField($"선택: {gem.id}  {gem.position}", EditorStyles.miniBoldLabel);
                    var types = GemTypes.All.ToArray();
                    int typeIndex = EditorGUILayout.Popup("종류", System.Array.IndexOf(types, gem.type), types.Select(t => $"{t} ({GemLabels[t]})").ToArray());
                    if (typeIndex >= 0) Field(types[typeIndex], gem.type, v => model.Edit(s => s.gems.First(g => g.id == gem.id).type = v));
                    Field(EditorGUILayout.Slider("등장 확률", gem.spawnChance, 0f, 1f), gem.spawnChance,
                        v => model.Edit(s => s.gems.First(g => g.id == gem.id).spawnChance = Mathf.Round(v * 20f) / 20f));
                }
                else if (model.Selection.Kind is StageItemKind.PlayerStart or StageItemKind.Enemy)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(model.Selection.Kind == StageItemKind.PlayerStart ? $"선택: 주인공 시작점 {stage.playerStart}" : $"선택: 적 {stage.enemy.position}", EditorStyles.miniBoldLabel);
                }

                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("왼쪽 클릭: 현재 도구로 배치 · 선택 도구로 끌어서 이동\n오른쪽 클릭: 보석 지우기\nCtrl+Z / Ctrl+Y / Ctrl+S", MessageType.None);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>값이 실제로 바뀌었을 때만 편집(+되돌리기 기록).</summary>
        static void Field<T>(T value, T current, System.Action<T> apply)
        {
            if (!EqualityComparer<T>.Default.Equals(value, current)) apply(value);
        }

        // ---------- 검증 결과 ----------

        void DrawIssues(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            issuesScroll = EditorGUILayout.BeginScrollView(issuesScroll);
            foreach (var problem in model.LoadProblems) EditorGUILayout.LabelField("⚠ " + problem, EditorStyles.miniLabel);

            var issues = model.ValidateCurrent();
            if (model.Current != null && issues.Count == 0) EditorGUILayout.LabelField("✔ 검증 통과", EditorStyles.miniBoldLabel);
            foreach (var issue in issues)
                EditorGUILayout.LabelField((issue.Severity == IssueSeverity.Error ? "✖ " : "⚠ ") + issue.Message + (issue.Cell is { } c ? $"  {c}" : string.Empty), EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField(statusMessage, EditorStyles.miniLabel);
            GUILayout.EndArea();
        }
    }
}
