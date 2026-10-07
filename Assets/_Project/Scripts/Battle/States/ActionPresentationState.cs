namespace Game2Week.Battle
{
    /// <summary>공격/공동 행동/거리 확보가 끝난 뒤에만 후속 흐름을 진행한다.</summary>
    public sealed class ActionPresentationState:BattleStateBase
    {
        IActionPresentationWorld world;ActionPresentationRequest request;ActionPresentationClock clock;bool completed;
        public ActionPresentationState(BattleContext context):base(context){}
        public override void Enter()
        {
            request=Context.PendingPresentation;world=(IActionPresentationWorld)Context.World;completed=false;
            Context.Ui.HideAll();Context.Input?.EnableUI();world.BeginPresentation(request.cue);
            bool strike=request.cue==ActionCue.Strike;
            clock=new ActionPresentationClock(world.PresentationDuration(request.cue),strike?world.StrikeImpactTime:world.PresentationDuration(request.cue),strike&&request.pauseImpact?world.StrikeHoldSeconds:0);
        }
        public override void Tick(float dt)
        {
            if(completed||dt<=0)return;
            if(clock.Advance(dt))request.impact?.Invoke();
            world.UpdatePresentation(request.cue,clock.Progress,clock.Holding);
            if(!clock.Complete)return;
            completed=true;world.EndPresentation(request.cue);request.finished?.Invoke();
        }
        public override void Exit(){world?.EndPresentation(request.cue);}
    }
}
