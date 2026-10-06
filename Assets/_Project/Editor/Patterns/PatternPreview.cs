using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Data.Patterns;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public sealed class PatternPreview : System.IDisposable
    {
        readonly PreviewRenderUtility render=new PreviewRenderUtility();
        readonly Mesh sphere,cube;
        readonly Material material;
        readonly List<PatternShot> shots=new();
        readonly List<PatternShot> warning=new();
        public int ActiveCount{get;private set;}
        public bool HasBoundaryCrossing{get;private set;}
        public PatternPreview()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere=go.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(go);
            go=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=go.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(go);
            material=new Material(Shader.Find("Universal Render Pipeline/Lit")){hideFlags=HideFlags.HideAndDontSave};
            render.camera.nearClipPlane=.05f;render.camera.farClipPlane=100;
            render.camera.clearFlags=CameraClearFlags.SolidColor;render.camera.backgroundColor=new Color(.04f,.05f,.08f);
            render.lights[0].intensity=1.5f;render.lights[0].transform.rotation=Quaternion.Euler(45,35,0);render.ambientColor=Color.gray;
        }
        public void Draw(Rect rect,GraphPatternDefinition d,float time,float yaw,float pitch,bool waist,Vector2 arena,Vector3 enemy,Vector3 target)
        {
            if(rect.width<1||rect.height<1)return;
            GUI.DrawTexture(rect,RenderFrame(rect,d,time,yaw,pitch,waist,arena,enemy,target),ScaleMode.StretchToFill,false);
        }
        public Texture RenderFrame(Rect rect,GraphPatternDefinition d,float time,float yaw,float pitch,bool waist,Vector2 arena,Vector3 enemy,Vector3 target)
        {
            shots.Clear();warning.Clear();ActiveCount=0;HasBoundaryCrossing=false;
            var timeline=new PatternTimeline(d);
            timeline.Advance(time,enemy,target,arena,shots);timeline.WarningShots(warning);
            var trajectory=GraphTrajectory.Create(d);
            render.BeginPreview(rect,GUIStyle.none);
            var focus=waist?target+Vector3.up*.6f:Vector3.zero;
            render.camera.transform.position=focus+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,waist?-2.8f:-12f);
            render.camera.transform.LookAt(waist?target+Vector3.forward*4+Vector3.up*.6f:Vector3.zero);
            render.camera.fieldOfView=60;
            DrawMesh(cube,new Vector3(0,-.08f,0),new Vector3(arena.x,.1f,arena.y),new Color(.12f,.16f,.22f));
            DrawMesh(cube,target+Vector3.up*.45f,new Vector3(.4f,.9f,.4f),Color.cyan);
            DrawMesh(sphere,enemy+Vector3.up*.65f,Vector3.one*.7f,new Color(.7f,.3f,.8f));
            var color=ColorFor(d.color);
            foreach(var shot in shots)
            {
                float age=time-shot.Time;
                if(age<0||age>=d.lifetime||CrossedBoundary(trajectory,shot,age,arena))continue;
                ActiveCount++;
                var launch=new TrajectoryLaunch(shot.Origin,shot.Velocity);
                DrawMesh(sphere,trajectory.Evaluate(launch,age),Vector3.one*.28f,color);
            }
            foreach(var shot in warning)
            {
                var launch=new TrajectoryLaunch(shot.Origin,shot.Velocity);
                for(int i=0;i<=64;i++)
                {
                    var p=trajectory.Evaluate(launch,d.lifetime*i/64f);
                    if(Outside(p,arena))HasBoundaryCrossing=true;
                    DrawMesh(sphere,p,Vector3.one*.045f,color);
                }
            }
            render.Render(true);return render.EndPreview();
        }
        bool CrossedBoundary(GraphTrajectory trajectory,PatternShot shot,float age,Vector2 arena)
        {
            var launch=new TrajectoryLaunch(shot.Origin,shot.Velocity);
            for(float t=0;t<age;t+=.015f)if(Outside(trajectory.Evaluate(launch,t),arena))return true;
            return Outside(trajectory.Evaluate(launch,age),arena);
        }
        static bool Outside(Vector3 p,Vector2 arena)=>Mathf.Abs(p.x)>arena.x*.5f+.5f||Mathf.Abs(p.z)>arena.y*.5f+.5f;
        void DrawMesh(Mesh mesh,Vector3 position,Vector3 scale,Color color)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);
            render.DrawMesh(mesh,Matrix4x4.TRS(position,Quaternion.identity,scale),material,0,block);
        }
        public static Color ColorFor(AttackColor color)=>color==AttackColor.Red?new Color(1,.15f,.15f):color==AttackColor.Blue?new Color(.15f,.45f,1):new Color(1,.8f,.1f);
        public void Dispose(){render.Cleanup();Object.DestroyImmediate(material);}
    }
}
