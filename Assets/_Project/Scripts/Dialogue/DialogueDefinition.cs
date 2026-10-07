using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game2Week.Dialogue
{
    [DataContract]
    public sealed class DialogueDefinition
    {
        [DataMember] public int schemaVersion = 1;
        [DataMember] public string id = "dlg_new";
        [DataMember] public Dictionary<string, DialogueSpeaker> speakers = new();
        [DataMember] public string start = "n1";
        [DataMember] public List<DialogueNode> nodes = new();
        [DataMember] public DialogueToolInfo _tool = new();
    }
    [DataContract] public sealed class DialogueSpeaker
    {
        [DataMember] public string name = "주인공";
        [DataMember] public string anchor = "player";
    }
    [DataContract] public sealed class DialogueNode
    {
        [DataMember] public string id = "n1";
        [DataMember] public string speaker = "heroine";
        [DataMember] public string text = string.Empty;
        [DataMember] public string pose = string.Empty;
        [DataMember] public string camera = string.Empty;
        [DataMember] public string next = string.Empty;
        [DataMember] public List<DialogueChoice> choices = new();
        [DataMember] public List<DialogueEffect> effects = new();
        [DataMember] public bool end;
    }
    [DataContract] public sealed class DialogueChoice
    {
        [DataMember] public string text = "선택";
        [DataMember] public string next = string.Empty;
        [DataMember] public List<DialogueEffect> effects = new();
    }
    [DataContract] public sealed class DialogueEffect
    {
        [DataMember] public string type = "flag";
        [DataMember] public string id = string.Empty;
        [DataMember] public bool value = true;
    }
    [DataContract] public sealed class DialogueToolInfo
    {
        [DataMember] public string generatedBy = "DialogueEditor";
        [DataMember] public int toolVersion = 1;
        [DataMember] public string checksum = string.Empty;
    }
}
