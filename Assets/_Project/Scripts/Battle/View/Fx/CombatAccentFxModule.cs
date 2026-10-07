using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>기존 파편 효과를 보강하는 베기·실제 메시 잔상·예고·결말·단계 연출. 표현만 구독한다.</summary>
    public sealed class CombatAccentFxModule : MonoBehaviour, IBattleFxModule
    {
        sealed class Stroke { public LineRenderer line;public float age,duration;public Color tint; }
        sealed class Ghost { public MeshRenderer renderer;public MeshFilter filter;public Mesh mesh;public float age; }
        [SerializeField] Material material;
        readonly List<Stroke> strokes=new();readonly List<Ghost> ghosts=new();
        BattleFxRig rig;
        public int Spawned{get;private set;}
        public void Bind(BattleFxRig value)
        {
            Unbind();rig=value;
            value.Feedback.PlayerParried+=Parry;value.Feedback.PlayerDodged+=Dodge;
            value.Feedback.WarningStarted+=Warning;value.Feedback.EnemyPhaseChanged+=Phase;
            value.Feedback.ColorPassed+=Passed;
            value.Events.EnemyDamaged+=Strike;
            value.Events.BattleEnded+=Ended;value.Events.StateChanged+=StateChanged;
        }
        void Unbind()
        {
            if(rig==null)return;
            rig.Feedback.PlayerParried-=Parry;rig.Feedback.PlayerDodged-=Dodge;
            rig.Feedback.WarningStarted-=Warning;rig.Feedback.EnemyPhaseChanged-=Phase;
            rig.Feedback.ColorPassed-=Passed;
            rig.Events.EnemyDamaged-=Strike;
            rig.Events.BattleEnded-=Ended;rig.Events.StateChanged-=StateChanged;rig=null;
        }
        void Strike(int damage)
        {
            if(damage<=0||!rig.Spawner.Enemy)return;
            var at=rig.Spawner.Enemy.Body.position+Vector3.up*.35f;var points=new Vector3[16];
            var player=rig.Spawner.Player;
            for(int i=0;i<points.Length;i++){float angle=Mathf.Lerp(-100,100,i/(points.Length-1f))*Mathf.Deg2Rad;points[i]=at+player.right*Mathf.Sin(angle)*.55f+Vector3.up*Mathf.Cos(angle)*.45f;}
            StrokeAt(points,new Color(2,1.4f,.4f),.1f,.22f);
        }
        void Parry(Vector3 at,float side)
        {
            var player=rig.Spawner.Player;var points=new Vector3[20];
            for(int i=0;i<points.Length;i++)
            {float angle=Mathf.Lerp(-110,110,i/(points.Length-1f))*Mathf.Deg2Rad;points[i]=at+(player.right*Mathf.Sin(angle)*side+player.forward*Mathf.Cos(angle))*.8f+Vector3.up*.08f;}
            StrokeAt(points,new Color(2,1.8f,.6f),.09f,.25f);
        }
        void Warning(AttackColor color,Vector3 at)=>Ring(new Vector3(at.x,.025f,at.z),BattleTexts.AttackColorTint(color),.8f,.6f);
        void Passed(AttackColor color,Vector3 at)
        {
            if(color!=AttackColor.Blue)return;
            var player=rig.Spawner.Player;
            for(int i=0;i<3;i++)
            {
                var start=at+Vector3.up*(.2f+i*.2f)+player.right*(i-1)*.2f;
                StrokeAt(new[]{start-player.forward*.55f,start+player.forward*.4f},new Color(.4f,1.2f,2),.035f,.22f);
            }
        }
        void Phase(int phase){if(rig.Spawner.Enemy)Ring(rig.Spawner.Enemy.transform.position+Vector3.up*.03f,new Color(1.7f,.5f,1.6f),1.8f,.8f);}
        void Ended(BattleOutcome outcome)
        {
            if(!rig.Spawner.Enemy||outcome==BattleOutcome.PlayerDefeated)return;
            var at=rig.Spawner.Enemy.transform.position;
            var color=outcome==BattleOutcome.EnemySpared?new Color(.5f,2,1.4f):new Color(2,.7f,.3f);
            for(int i=0;i<3;i++)Ring(at+Vector3.up*(.1f+i*.25f),color,1+i*.3f,1f);
        }
        void Ring(Vector3 at,Color color,float radius,float duration)
        {
            var points=new Vector3[49];for(int i=0;i<points.Length;i++){float angle=i/48f*Mathf.PI*2;points[i]=at+new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*radius;}
            StrokeAt(points,color,.04f,duration);
        }
        void StrokeAt(Vector3[] points,Color color,float width,float duration)
        {
            var stroke=strokes.Find(item=>!item.line.enabled);
            if(stroke==null)
            {
                if(strokes.Count>=48)return;
                var go=new GameObject("효과 궤적");go.transform.SetParent(transform,false);
                stroke=new Stroke{line=go.AddComponent<LineRenderer>()};stroke.line.sharedMaterial=material;strokes.Add(stroke);
            }
            stroke.age=0;stroke.duration=duration;stroke.tint=color;stroke.line.enabled=true;
            stroke.line.positionCount=points.Length;stroke.line.SetPositions(points);stroke.line.startWidth=stroke.line.endWidth=width;
            stroke.line.startColor=stroke.line.endColor=color;Spawned++;
        }
        void Dodge(Vector3 at)
        {
            var ghost=ghosts.Find(item=>!item.renderer.enabled);
            if(ghost==null)
            {
                if(ghosts.Count>=6)return;
                var go=new GameObject("회피 잔상");go.transform.SetParent(transform,false);
                ghost=new Ghost{renderer=go.AddComponent<MeshRenderer>(),filter=go.AddComponent<MeshFilter>(),mesh=new Mesh()};
                ghost.renderer.sharedMaterial=material;ghost.filter.sharedMesh=ghost.mesh;ghosts.Add(ghost);
            }
            var player=rig.Spawner.Player;var combine=new List<CombineInstance>();var temporary=new List<Mesh>();
            foreach(var skin in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(!skin.enabled)continue;
                var baked=new Mesh();skin.BakeMesh(baked);temporary.Add(baked);
                combine.Add(new CombineInstance{mesh=baked,transform=player.worldToLocalMatrix*skin.transform.localToWorldMatrix});
            }
            ghost.mesh.Clear();if(combine.Count>0)ghost.mesh.CombineMeshes(combine.ToArray(),true,true);
            foreach(var mesh in temporary)Destroy(mesh);
            ghost.renderer.transform.SetPositionAndRotation(player.position,player.rotation);
            ghost.renderer.transform.localScale=player.lossyScale;ghost.renderer.enabled=true;ghost.age=0;Spawned++;
        }
        void StateChanged(BattleStateId state)
        {if(state==BattleStateId.EnemyTurn||state==BattleStateId.Victory||state==BattleStateId.Dialogue)return;foreach(var stroke in strokes)stroke.line.enabled=false;foreach(var ghost in ghosts)ghost.renderer.enabled=false;}
        void Update()
        {
            if(Time.timeScale<=0)return;
            foreach(var stroke in strokes)if(stroke.line.enabled)
            {stroke.age+=Time.deltaTime;var color=stroke.tint;color.a=Mathf.Clamp01(1-stroke.age/stroke.duration);stroke.line.startColor=stroke.line.endColor=color;if(stroke.age>=stroke.duration)stroke.line.enabled=false;}
            foreach(var ghost in ghosts)if(ghost.renderer.enabled)
            {ghost.age+=Time.deltaTime;var block=new MaterialPropertyBlock();block.SetFloat("_UseVertexColor",0);block.SetColor("_BaseColor",new Color(.4f,1.2f,1.6f,Mathf.Clamp01(1-ghost.age/.35f)*.45f));ghost.renderer.SetPropertyBlock(block);if(ghost.age>=.35f)ghost.renderer.enabled=false;}
        }
        void OnDestroy(){Unbind();foreach(var ghost in ghosts)if(ghost.mesh)Destroy(ghost.mesh);}
    }
}
