using System.Collections.Generic;
using UnityEngine;
using Game2Week.Data.Patterns;

namespace Game2Week.Battle.Patterns.Trajectories
{
    public sealed class GraphAttackPattern : MonoBehaviour, IAttackPattern, IThreatSource
    {
        [SerializeField] GraphPatternDefinition definition;
        [SerializeField] Bullet bulletPrefab;
        [SerializeField] Material warningMaterial;
        readonly List<Bullet> bullets=new();
        readonly Dictionary<Bullet,float> expiry=new();
        readonly List<PatternShot> shots=new();
        readonly List<LineRenderer> warnings=new();
        PatternContext context;
        PatternTimeline timeline;
        GraphTrajectory trajectory;
        GraphPatternDefinition snapshot;
        public IReadOnlyList<Bullet> Bullets=>bullets;
        public PatternTimeline Timeline=>timeline;
        public void Configure(GraphPatternDefinition value,Bullet prefab,Material material)
        {definition=value;bulletPrefab=prefab;warningMaterial=material;}
        public void Begin(PatternContext value)
        {
            End();
            var errors=PatternGraphRules.Validate(definition);
            if(errors.Count>0||!bulletPrefab){Debug.LogError("패턴 시작 실패: "+string.Join(" / ",errors));return;}
            snapshot=Instantiate(definition);snapshot.hideFlags=HideFlags.HideAndDontSave;
            context=value;trajectory=GraphTrajectory.Create(snapshot);timeline=new PatternTimeline(snapshot);
            timeline.Advance(0,Local(value.Enemy.position),Local(value.Player.position),value.Arena.Size,shots);
            DrawWarnings();
        }
        Vector3 Local(Vector3 world)=>context.Arena.transform.InverseTransformPoint(world);
        Vector3 World(Vector3 local)=>context.Arena.transform.TransformPoint(local);
        public void Tick(float dt)
        {
            if(context==null||dt<=0)return;
            shots.Clear();timeline.Advance(dt,Local(context.Enemy.position),Local(context.Player.position),context.Arena.Size,shots);
            foreach(var b in bullets)if(b.Active)
            {
                if(b.Tick(dt,context)){b.Deactivate();context.ReportHit();}
                if(b.Active&&timeline.Time>=expiry[b]&&!b.Deflected)b.Deactivate();
            }
            foreach(var shot in shots)
            {
                var bullet=GetBullet();bullet.Color=snapshot.color;
                var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",AttackTint(snapshot.color));
                foreach(var renderer in bullet.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(block);
                bullet.Launch(World(shot.Origin),context.Arena.transform.TransformDirection(shot.Velocity),trajectory);
                expiry[bullet]=shot.Time+snapshot.lifetime;
                float age=timeline.Time-shot.Time;
                if(age>0&&bullet.Tick(age,context)){bullet.Deactivate();context.ReportHit();}
                if(age>=snapshot.lifetime&&!bullet.Deflected)bullet.Deactivate();
                if(context.EnemyView)context.EnemyView.PlayAttack();
            }
            DrawWarnings();
        }
        Bullet GetBullet(){foreach(var b in bullets)if(!b.Active)return b;var result=Instantiate(bulletPrefab,transform);bullets.Add(result);return result;}
        void DrawWarnings()
        {
            shots.Clear();timeline.WarningShots(shots);
            while(warnings.Count<shots.Count)
            {
                var go=new GameObject("TrajectoryWarning");go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=warningMaterial;line.positionCount=49;
                line.startWidth=line.endWidth=.035f;warnings.Add(line);
            }
            for(int i=0;i<warnings.Count;i++)
            {
                warnings[i].enabled=i<shots.Count;
                if(i>=shots.Count)continue;
                warnings[i].startColor=warnings[i].endColor=AttackTint(snapshot.color);
                var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",AttackTint(snapshot.color));warnings[i].SetPropertyBlock(block);
                var launch=new TrajectoryLaunch(World(shots[i].Origin),context.Arena.transform.TransformDirection(shots[i].Velocity));
                for(int j=0;j<49;j++)warnings[i].SetPosition(j,trajectory.Evaluate(launch,snapshot.lifetime*j/48f));
            }
        }
        static Color AttackTint(AttackColor color)=>color==AttackColor.Red?new Color(1,.15f,.15f):color==AttackColor.Blue?new Color(.15f,.45f,1):new Color(1,.8f,.1f);
        public void CollectThreats(List<ThreatPoint> output)
        {
            if(context==null)return;
            if(timeline.WarningActive)output.Add(new ThreatPoint(World(timeline.WarningOrigin),snapshot.color));
            foreach(var b in bullets)if(b.Active&&!b.Deflected)output.Add(new ThreatPoint(b.transform.position,b.Color));
        }
        public void End()
        {
            foreach(var b in bullets)if(b)b.Deactivate();
            foreach(var line in warnings)if(line)line.enabled=false;
            context=null;timeline=null;shots.Clear();
            if(snapshot)Destroy(snapshot);snapshot=null;
        }
        void OnDestroy(){if(snapshot)Destroy(snapshot);}
    }
}
