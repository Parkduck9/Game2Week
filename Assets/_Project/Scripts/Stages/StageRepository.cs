using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game2Week.Stages
{
    public sealed class StageLoadResult
    {
        // Unity C#은 init 접근자를 지원하지 않음
        public StageDefinition Stage { get; set; }
        public bool ChecksumValid { get; set; }
        public List<StageIssue> Issues { get; set; } = new();
        public bool IsUsable => Stage != null && !StageValidator.HasErrors(Issues);
    }

    /// <summary>
    /// StreamingAssets/Stages 의 스테이지 파일 읽기·쓰기. 쓰기는 맵툴(에디터)만 사용한다.
    /// </summary>
    public sealed class StageRepository
    {
        public const string IndexFileName = "stages.json";
        static readonly UTF8Encoding Utf8NoBom = new(false);

        public StageRepository(string directory)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public static string DefaultDirectory => Path.Combine(Application.streamingAssetsPath, "Stages");

        public string Directory { get; }

        public string PathFor(string stageId) => Path.Combine(Directory, stageId + ".json");

        public bool Exists(string stageId) => File.Exists(PathFor(stageId));

        /// <summary>목록 파일이 없으면 빈 목록.</summary>
        public StageIndex LoadIndex()
        {
            var path = Path.Combine(Directory, IndexFileName);
            return File.Exists(path) ? StageJson.ReadIndex(File.ReadAllText(path, Utf8NoBom)) : new StageIndex();
        }

        public void SaveIndex(StageIndex index)
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(Path.Combine(Directory, IndexFileName), StageJson.Write(index), Utf8NoBom);
        }

        public StageLoadResult LoadStage(string stageId, IStageCatalog catalog = null)
        {
            var path = PathFor(stageId);
            if (!File.Exists(path))
                return new StageLoadResult { Issues = { new StageIssue(IssueSeverity.Error, $"스테이지 파일 없음: {path}") } };

            StageDefinition stage;
            try
            {
                stage = StageJson.ReadStage(File.ReadAllText(path, Utf8NoBom));
            }
            catch (FormatException e)
            {
                return new StageLoadResult { Issues = { new StageIssue(IssueSeverity.Error, $"{stageId}: {e.Message}") } };
            }

            var issues = StageValidator.Validate(stage, catalog);
            bool checksumValid = StageJson.HasValidChecksum(stage);
            if (!checksumValid) issues.Insert(0, new StageIssue(IssueSeverity.Warning, $"{stageId}: 맵툴 밖에서 수정됨 (체크섬 불일치)"));
            if (stage.id != stageId) issues.Add(new StageIssue(IssueSeverity.Error, $"파일 이름({stageId})과 id({stage.id})가 다름"));

            return new StageLoadResult { Stage = stage, ChecksumValid = checksumValid, Issues = issues };
        }

        /// <summary>맵툴 전용. 체크섬을 새로 찍어 저장한다.</summary>
        public void SaveStage(StageDefinition stage)
        {
            if (!StageValidator.IsValidId(stage.id)) throw new ArgumentException($"잘못된 스테이지 id: {stage.id}");
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(PathFor(stage.id), StageJson.Write(stage), Utf8NoBom);
        }

        public void DeleteStage(string stageId)
        {
            var path = PathFor(stageId);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
