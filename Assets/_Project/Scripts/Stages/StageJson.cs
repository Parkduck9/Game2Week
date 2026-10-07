using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Game2Week.Stages
{
    /// <summary>
    /// 스테이지 JSON 직렬화 + 체크섬. 체크섬은 checksum 칸을 비운 상태의 JSON으로 계산하므로,
    /// 맵툴 밖에서 값을 고치면 <see cref="HasValidChecksum"/>가 false가 된다.
    /// </summary>
    public static class StageJson
    {
        public const string ToolName = "StageEditor";
        public const int ToolVersion = 1;

        public static string Write(StageDefinition stage)
        {
            stage.schemaVersion = StageDefinition.CurrentSchemaVersion; // 저장하면 최신 형식으로
            stage.theme = string.IsNullOrWhiteSpace(stage.theme) ? StageThemes.Default : stage.theme;
            stage.dialogues ??= new StageDialogues();
            Stamp(stage._tool ??= new StageToolInfo(), () => JsonUtility.ToJson(stage));
            return JsonUtility.ToJson(stage, prettyPrint: true);
        }

        public static string Write(StageIndex index)
        {
            Stamp(index._tool ??= new StageToolInfo(), () => JsonUtility.ToJson(index));
            return JsonUtility.ToJson(index, prettyPrint: true);
        }

        public static StageDefinition ReadStage(string json) => Read<StageDefinition>(json);

        public static StageIndex ReadIndex(string json) => Read<StageIndex>(json);

        public static bool HasValidChecksum(StageDefinition stage) =>
            stage?._tool != null && Matches(stage._tool, () => JsonUtility.ToJson(stage));

        public static bool HasValidChecksum(StageIndex index) =>
            index?._tool != null && Matches(index._tool, () => JsonUtility.ToJson(index));

        static T Read<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) throw new FormatException("JSON이 비어 있음");
            try
            {
                return JsonUtility.FromJson<T>(json) ?? throw new FormatException("JSON을 읽을 수 없음");
            }
            catch (ArgumentException e)
            {
                throw new FormatException("JSON 형식 오류: " + e.Message, e);
            }
        }

        static void Stamp(StageToolInfo tool, Func<string> compactJson)
        {
            tool.generatedBy = ToolName;
            tool.toolVersion = ToolVersion;
            tool.checksum = string.Empty;
            tool.checksum = Hash(compactJson());
        }

        static bool Matches(StageToolInfo tool, Func<string> compactJson)
        {
            var stored = tool.checksum;
            if (string.IsNullOrEmpty(stored)) return false;

            tool.checksum = string.Empty;
            try
            {
                return stored == Hash(compactJson());
            }
            finally
            {
                tool.checksum = stored;
            }
        }

        static string Hash(string text)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            var sb = new StringBuilder("sha256:", 7 + bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
