using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Dialogue
{
    public sealed class DialogueRepository
    {
        public static string DefaultDirectory => Path.Combine(Application.streamingAssetsPath, "Dialogues");
        readonly string directory;
        public DialogueRepository(string directory) { this.directory = Path.GetFullPath(directory); }
        string FilePath(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Contains("..") || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException("대화 파일 이름이 올바르지 않습니다.");
            return Path.Combine(directory, id + ".json");
        }
        public IReadOnlyList<string> List() => Directory.Exists(directory) ? Directory.GetFiles(directory, "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n).ToArray() : Array.Empty<string>();
        public DialogueDefinition Load(string id)
        {
            var definition = DialogueJson.Read(File.ReadAllText(FilePath(id)));
            if (definition.id != id || !DialogueJson.HasValidChecksum(definition)) throw new InvalidDataException("대화 파일 이름 또는 체크섬이 일치하지 않습니다: " + id);
            return definition;
        }
        public void Save(DialogueDefinition definition, Func<char, bool> hasCharacter = null)
        {
            var errors = DialogueValidator.Validate(definition, hasCharacter);
            if (errors.Count > 0) throw new InvalidDataException(string.Join("\n", errors));
            string path = FilePath(definition.id); Directory.CreateDirectory(directory);
            File.WriteAllText(path + ".tmp", DialogueJson.Write(definition), new System.Text.UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(path + ".tmp", path, null); else File.Move(path + ".tmp", path);
        }
    }
}
