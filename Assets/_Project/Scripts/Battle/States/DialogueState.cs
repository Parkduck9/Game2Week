using System.Linq;

namespace Game2Week.Battle
{
    public sealed class DialogueState : BattleStateBase
    {
        public DialogueState(BattleContext context) : base(context) { }
        public override void Enter() { Context.Input?.EnableUI(); Context.Ui.HideAll(); ShowNode(); }
        void ShowNode()
        {
            var dialogue = Context.Dialogues;
            if (dialogue.Runner.Ended) { dialogue.Finish(); return; }
            var node = dialogue.Runner.Current;
            var speaker = dialogue.Definition.speakers[node.speaker];
            Context.Events.RaiseDialogueCue(speaker.anchor, node.pose, node.camera);
            Context.Ui.ShowDialogue(speaker.name + "\n" + node.text, () =>
            {
                if (Context.CurrentState != BattleStateId.Dialogue) return;
                if (node.choices.Count > 0)
                {
                    Context.Ui.ShowListMenu(node.choices.Select(choice => choice.text).ToArray(), index =>
                    { dialogue.Runner.Advance(index); ShowNode(); }, ShowNode);
                }
                else { dialogue.Runner.Advance(); ShowNode(); }
            });
        }
        public override void Exit() => Context.Ui.HideAll();
    }
}
