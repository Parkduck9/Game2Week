using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace Game2Week.Dialogue
{
    public static class DialogueJson
    {
        static DataContractJsonSerializer Serializer() => new(typeof(DialogueDefinition), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
        public static string Encode(DialogueDefinition definition)
        {
            using var stream = new MemoryStream();
            Serializer().WriteObject(stream, definition);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        public static DialogueDefinition Read(string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var definition = (DialogueDefinition)Serializer().ReadObject(stream);
            definition.speakers ??= new(); definition.nodes ??= new(); definition._tool ??= new();
            foreach (var node in definition.nodes)
            {
                node.choices ??= new(); node.effects ??= new();
                foreach (var choice in node.choices) choice.effects ??= new();
            }
            return definition;
        }
        public static DialogueDefinition Clone(DialogueDefinition definition) => Read(Encode(definition));
        public static string Write(DialogueDefinition definition)
        {
            definition._tool ??= new();
            definition._tool.generatedBy = "DialogueEditor"; definition._tool.toolVersion = 1;
            definition._tool.checksum = string.Empty;
            definition._tool.checksum = Hash(Encode(definition));
            return Encode(definition);
        }
        public static bool HasValidChecksum(DialogueDefinition definition)
        {
            if (string.IsNullOrEmpty(definition?._tool?.checksum)) return false;
            var copy = Clone(definition); var stored = copy._tool.checksum; copy._tool.checksum = string.Empty;
            return stored == Hash(Encode(copy));
        }
        static string Hash(string value)
        {
            using var hash = SHA256.Create();
            return "sha256:" + BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
