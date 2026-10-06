using System;
using System.Collections.Generic;
using System.Linq;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    public enum StageTool
    {
        Select,
        PlayerStart,
        Enemy,
        Gem,
        Erase,
        GemRegion,
        EraseRegion,
    }

    public enum StageItemKind
    {
        None,
        PlayerStart,
        Enemy,
        Gem,
    }

    public readonly struct StageItemRef
    {
        public StageItemRef(StageItemKind kind, string gemId = null)
        {
            Kind = kind;
            GemId = gemId;
        }

        public StageItemKind Kind { get; }
        public string GemId { get; }
        public static StageItemRef None => new(StageItemKind.None);
    }

    public sealed class StageSaveReport
    {
        public List<string> Saved { get; } = new();
        public List<string> Deleted { get; } = new();
        /// <summary>오류가 있어 저장하지 못한 스테이지 (id, 첫 오류)</summary>
        public List<(string id, string error)> Blocked { get; } = new();
        public bool IndexSaved { get; set; }
    }

    /// <summary>
    /// 맵툴의 편집 로직 (UI 없음). 창(StageEditorWindow)은 이 클래스만 호출한다.
    /// 되돌리기는 스테이지별 JSON 스냅샷.
    /// </summary>
    public sealed class StageEditorModel
    {
        const int MaxUndo = 100;

        sealed class Entry
        {
            public StageDefinition Stage;
            public string SavedId;
            public bool Dirty;
            public readonly Stack<string> Undo = new();
            public readonly Stack<string> Redo = new();
        }

        readonly StageRepository repository;
        readonly List<Entry> entries = new();
        readonly List<string> deletedIds = new();
        readonly List<string> loadProblems = new();
        bool indexDirty;
        bool dragSnapshotTaken;

        public StageEditorModel(StageRepository repository, IStageCatalog catalog = null)
        {
            this.repository = repository;
            Catalog = catalog;
        }

        public IStageCatalog Catalog { get; set; }
        public StageTool Tool { get; set; } = StageTool.Select;
        public int CurrentIndex { get; private set; } = -1;
        public StageItemRef Selection { get; private set; } = StageItemRef.None;

        public int Count => entries.Count;
        public StageDefinition Current => CurrentIndex >= 0 && CurrentIndex < entries.Count ? entries[CurrentIndex].Stage : null;
        public IReadOnlyList<string> LoadProblems => loadProblems;
        public bool HasUnsavedChanges => indexDirty || deletedIds.Count > 0 || entries.Any(e => e.Dirty);
        public bool CanUndo => CurrentEntry?.Undo.Count > 0;
        public bool CanRedo => CurrentEntry?.Redo.Count > 0;

        Entry CurrentEntry => CurrentIndex >= 0 && CurrentIndex < entries.Count ? entries[CurrentIndex] : null;

        public StageDefinition StageAt(int index) => entries[index].Stage;

        public bool IsDirty(int index) => entries[index].Dirty;

        // ---------- 불러오기 ----------

        public void Load()
        {
            entries.Clear();
            deletedIds.Clear();
            loadProblems.Clear();
            indexDirty = false;

            foreach (var id in repository.LoadIndex().stages)
            {
                var result = repository.LoadStage(id, Catalog);
                if (result.Stage == null)
                {
                    loadProblems.Add($"{id}: {result.Issues.FirstOrDefault().Message}");
                    continue;
                }
                if (!result.ChecksumValid) loadProblems.Add($"{id}: 맵툴 밖에서 수정됨 — 저장하면 체크섬이 새로 기록됩니다");
                entries.Add(new Entry { Stage = result.Stage, SavedId = id });
            }

            CurrentIndex = entries.Count > 0 ? 0 : -1;
            Selection = StageItemRef.None;
        }

        // ---------- 검증 ----------

        public List<StageIssue> ValidateCurrent() => Current == null ? new List<StageIssue>() : Validate(CurrentIndex);

        List<StageIssue> Validate(int index)
        {
            var stage = entries[index].Stage;
            var issues = StageValidator.Validate(stage, Catalog);
            if (entries.Where((e, i) => i != index).Any(e => e.Stage.id == stage.id))
                issues.Add(new StageIssue(IssueSeverity.Error, $"다른 스테이지와 id가 같음: {stage.id}"));
            return issues;
        }

        // ---------- 스테이지 목록 ----------

        public void SelectStage(int index)
        {
            if (index < 0 || index >= entries.Count) return;
            CurrentIndex = index;
            Selection = StageItemRef.None;
        }

        public void AddStage()
        {
            int number = NextStageNumber();
            var stage = new StageDefinition { id = $"stage_{number:000}", name = $"새 스테이지 {number}" };
            InsertEntry(new Entry { Stage = stage, Dirty = true }, entries.Count);
        }

        public void DuplicateCurrent()
        {
            if (Current == null) return;
            var copy = JsonUtility.FromJson<StageDefinition>(JsonUtility.ToJson(Current));
            int number = NextStageNumber();
            copy.id = $"stage_{number:000}";
            copy.name = Current.name + " (복사)";
            InsertEntry(new Entry { Stage = copy, Dirty = true }, CurrentIndex + 1);
        }

        public void DeleteCurrent()
        {
            var entry = CurrentEntry;
            if (entry == null) return;
            if (entry.SavedId != null) deletedIds.Add(entry.SavedId);
            entries.RemoveAt(CurrentIndex);
            indexDirty = true;
            CurrentIndex = Math.Min(CurrentIndex, entries.Count - 1);
            Selection = StageItemRef.None;
        }

        public void MoveCurrent(int delta)
        {
            int target = CurrentIndex + delta;
            if (CurrentEntry == null || target < 0 || target >= entries.Count) return;
            (entries[CurrentIndex], entries[target]) = (entries[target], entries[CurrentIndex]);
            CurrentIndex = target;
            indexDirty = true;
        }

        void InsertEntry(Entry entry, int at)
        {
            entries.Insert(at, entry);
            CurrentIndex = at;
            Selection = StageItemRef.None;
            indexDirty = true;
        }

        int NextStageNumber()
        {
            int max = 0;
            foreach (var e in entries)
                if (e.Stage.id.StartsWith("stage_") && int.TryParse(e.Stage.id.Substring(6), out int n)) max = Math.Max(max, n);
            return max + 1;
        }

        // ---------- 편집 (모든 변경은 Edit를 거친다) ----------

        public void Edit(Action<StageDefinition> change)
        {
            var entry = CurrentEntry;
            if (entry == null) return;
            PushUndo(entry);
            change(entry.Stage);
            entry.Dirty = true;
        }

        public void Undo() => Swap(e => e.Undo, e => e.Redo);

        public void Redo() => Swap(e => e.Redo, e => e.Undo);

        void Swap(Func<Entry, Stack<string>> from, Func<Entry, Stack<string>> to)
        {
            var entry = CurrentEntry;
            if (entry == null || from(entry).Count == 0) return;
            to(entry).Push(JsonUtility.ToJson(entry.Stage));
            entry.Stage = JsonUtility.FromJson<StageDefinition>(from(entry).Pop());
            entry.Dirty = true;
            if (Selection.Kind == StageItemKind.Gem && FindGem(Selection.GemId) == null) Selection = StageItemRef.None;
        }

        static void PushUndo(Entry entry)
        {
            entry.Undo.Push(JsonUtility.ToJson(entry.Stage));
            entry.Redo.Clear();
            if (entry.Undo.Count > MaxUndo)
            {
                var keep = entry.Undo.Take(MaxUndo).Reverse().ToList();
                entry.Undo.Clear();
                foreach (var s in keep) entry.Undo.Push(s);
            }
        }

        // ---------- 격자 조작 ----------

        public StageItemRef ItemAt(GridPoint cell)
        {
            var stage = Current;
            if (stage == null) return StageItemRef.None;
            if (stage.playerStart == cell) return new StageItemRef(StageItemKind.PlayerStart);
            if (stage.enemy.position == cell) return new StageItemRef(StageItemKind.Enemy);
            var gem = stage.gems.FirstOrDefault(g => g.position == cell);
            return gem != null ? new StageItemRef(StageItemKind.Gem, gem.id) : StageItemRef.None;
        }

        public StageGem FindGem(string gemId) => Current?.gems.FirstOrDefault(g => g.id == gemId);

        /// <summary>현재 도구로 칸을 클릭. 변경이 있었으면 true.</summary>
        public bool Click(GridPoint cell)
        {
            var stage = Current;
            if (stage == null || !StageGeometry.InBounds(stage.grid, cell)) return false;
            var here = ItemAt(cell);

            switch (Tool)
            {
                case StageTool.Select:
                    Selection = here;
                    return false;

                case StageTool.PlayerStart:
                    if (here.Kind != StageItemKind.None) { Selection = here; return false; }
                    Edit(s => s.playerStart = cell);
                    Selection = new StageItemRef(StageItemKind.PlayerStart);
                    return true;

                case StageTool.Enemy:
                    if (here.Kind != StageItemKind.None) { Selection = here; return false; }
                    Edit(s => s.enemy.position = cell);
                    Selection = new StageItemRef(StageItemKind.Enemy);
                    return true;

                case StageTool.Gem:
                    if (here.Kind != StageItemKind.None) { Selection = here; return false; }
                    var id = NextGemId();
                    Edit(s => s.gems.Add(new StageGem { id = id, type = GemTypes.Heal, position = cell }));
                    Selection = new StageItemRef(StageItemKind.Gem, id);
                    return true;

                case StageTool.Erase:
                    return EraseAt(cell);
            }
            return false;
        }

        /// <summary>보석만 지울 수 있다 (주인공 시작점·적은 항상 하나씩 있어야 함).</summary>
        public bool EraseAt(GridPoint cell)
        {
            var here = ItemAt(cell);
            if (here.Kind != StageItemKind.Gem) return false;
            Edit(s => s.gems.RemoveAll(g => g.id == here.GemId));
            if (Selection.GemId == here.GemId) Selection = StageItemRef.None;
            return true;
        }

        public void BeginDrag(GridPoint cell)
        {
            Selection = ItemAt(cell);
            dragSnapshotTaken = false;
        }

        /// <summary>선택된 항목을 빈 칸으로 옮긴다. 드래그 한 번 = 되돌리기 한 번.</summary>
        public bool DragTo(GridPoint cell)
        {
            var stage = Current;
            if (stage == null || Selection.Kind == StageItemKind.None) return false;
            if (!StageGeometry.InBounds(stage.grid, cell) || ItemAt(cell).Kind != StageItemKind.None) return false;

            if (!dragSnapshotTaken)
            {
                PushUndo(CurrentEntry);
                dragSnapshotTaken = true;
            }
            switch (Selection.Kind)
            {
                case StageItemKind.PlayerStart: stage.playerStart = cell; break;
                case StageItemKind.Enemy: stage.enemy.position = cell; break;
                case StageItemKind.Gem: FindGem(Selection.GemId).position = cell; break;
            }
            CurrentEntry.Dirty = true;
            return true;
        }

        public void EndDrag() => dragSnapshotTaken = false;

        /// <summary>사각 영역 보석 편집을 되돌리기 한 번으로 처리한다. 적/시작점은 보존한다.</summary>
        public int EditGemRegion(GridPoint from,GridPoint to,bool erase)
        {
            if(Current==null)return 0;
            int x0=Mathf.Max(0,Mathf.Min(from.x,to.x)),x1=Mathf.Min(Current.grid.width-1,Mathf.Max(from.x,to.x));
            int z0=Mathf.Max(0,Mathf.Min(from.z,to.z)),z1=Mathf.Min(Current.grid.depth-1,Mathf.Max(from.z,to.z));
            var cells=new List<GridPoint>();
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {var p=new GridPoint(x,z);var here=ItemAt(p);if(erase?here.Kind==StageItemKind.Gem:here.Kind==StageItemKind.None)cells.Add(p);}
            if(cells.Count==0)return 0;
            Edit(s=>{foreach(var p in cells){if(erase)s.gems.RemoveAll(g=>g.position.x==p.x&&g.position.z==p.z);else s.gems.Add(new StageGem{id=NextGemId(),position=p});}});
            Selection=StageItemRef.None;return cells.Count;
        }
        /// <summary>보석만 빈 반대편 칸으로 복사한다. 충돌·중앙 중복은 건너뛴다.</summary>
        public int MirrorGems(bool horizontal)
        {
            if(Current==null)return 0;
            var copies=new List<StageGem>();
            foreach(var g in Current.gems)
            {
                var p=horizontal?new GridPoint(Current.grid.width-1-g.position.x,g.position.z):new GridPoint(g.position.x,Current.grid.depth-1-g.position.z);
                if(ItemAt(p).Kind!=StageItemKind.None||copies.Any(c=>c.position.x==p.x&&c.position.z==p.z))continue;
                copies.Add(new StageGem{position=p,type=g.type,spawnChance=g.spawnChance});
            }
            if(copies.Count>0)Edit(s=>{foreach(var g in copies){g.id=NextGemId();s.gems.Add(g);}});
            return copies.Count;
        }

        string NextGemId()
        {
            int n = 1;
            while (Current.gems.Any(g => g.id == $"gem_{n:00}")) n++;
            return $"gem_{n:00}";
        }

        // ---------- 저장 ----------

        /// <summary>
        /// 바뀐 스테이지를 저장한다. 오류가 있는 스테이지는 저장하지 않고 보고한다
        /// (이때 목록 파일도 저장하지 않아 게임이 깨진 상태를 읽지 않게 한다).
        /// </summary>
        public StageSaveReport SaveAll()
        {
            var report = new StageSaveReport();
            for (int i = 0; i < entries.Count; i++)
            {
                if (!entries[i].Dirty && entries[i].SavedId == entries[i].Stage.id) continue;
                var firstError = Validate(i).FirstOrDefault(x => x.Severity == IssueSeverity.Error);
                if (firstError.Message != null) report.Blocked.Add((entries[i].Stage.id, firstError.Message));
            }
            if (report.Blocked.Count > 0) return report;

            foreach (var id in deletedIds)
            {
                if (entries.Any(e => e.Stage.id == id)) continue;
                repository.DeleteStage(id);
                report.Deleted.Add(id);
            }
            deletedIds.Clear();

            foreach (var entry in entries)
            {
                bool renamed = entry.SavedId != null && entry.SavedId != entry.Stage.id;
                if (!entry.Dirty && !renamed) continue;
                repository.SaveStage(entry.Stage);
                if (renamed && entries.All(e => e.Stage.id != entry.SavedId)) repository.DeleteStage(entry.SavedId);
                if (renamed || entry.SavedId == null) indexDirty = true;
                entry.SavedId = entry.Stage.id;
                entry.Dirty = false;
                report.Saved.Add(entry.Stage.id);
            }

            if (indexDirty || report.Deleted.Count > 0)
            {
                var index = new StageIndex();
                index.stages.AddRange(entries.Select(e => e.Stage.id));
                repository.SaveIndex(index);
                indexDirty = false;
                report.IndexSaved = true;
            }
            return report;
        }
    }
}
