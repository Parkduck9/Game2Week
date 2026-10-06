using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game2Week.Save
{
    /// <summary>
    /// 깨지지 않는 JSON 파일 저장: 임시 파일에 다 쓴 뒤 바꿔치기하고, 직전 파일은 .bak으로 남긴다.
    /// 읽기는 본 파일 → .bak 순서로 시도.
    /// </summary>
    public static class JsonFileStore
    {
        static readonly UTF8Encoding Utf8NoBom = new(false);

        public static void Write<T>(string path, T data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp = path + ".tmp";
            var backup = path + ".bak";
            File.WriteAllText(temp, JsonUtility.ToJson(data, prettyPrint: true), Utf8NoBom);

            if (File.Exists(path)) File.Replace(temp, path, backup);
            else File.Move(temp, path);
        }

        /// <summary>읽을 수 있는 파일이 없으면 null. isValid로 내용 검사 (예: 버전).</summary>
        public static T Read<T>(string path, Func<T, bool> isValid, Action<string> warn = null) where T : class
        {
            foreach (var candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    var data = JsonUtility.FromJson<T>(File.ReadAllText(candidate, Utf8NoBom));
                    if (data != null && isValid(data))
                    {
                        if (candidate != path) warn?.Invoke($"{Path.GetFileName(path)}이(가) 깨져서 백업에서 불러옴");
                        return data;
                    }
                    warn?.Invoke($"{Path.GetFileName(candidate)}: 내용이 올바르지 않음");
                }
                catch (Exception e) when (e is ArgumentException or IOException)
                {
                    warn?.Invoke($"{Path.GetFileName(candidate)}: 읽기 실패 — {e.Message}");
                }
            }
            return null;
        }
    }
}
