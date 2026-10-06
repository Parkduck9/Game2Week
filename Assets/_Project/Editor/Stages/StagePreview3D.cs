using Game2Week.Stages;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    public sealed class StagePreview3D : System.IDisposable
    {
        readonly PreviewRenderUtility preview=new();
        readonly Mesh cube,sphere;
        readonly Material material;
        public StagePreview3D()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=go.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(go);
            go=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere=go.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(go);
            material=new Material(Shader.Find("Universal Render Pipeline/Lit")){hideFlags=HideFlags.HideAndDontSave};
            preview.camera.nearClipPlane=.1f;preview.camera.farClipPlane=100;
            preview.lights[0].intensity=1.5f;preview.lights[0].transform.rotation=Quaternion.Euler(45,20,0);preview.ambientColor=Color.gray;
        }
        public Texture Render(Rect rect,StageDefinition stage,float yaw,float pitch)
        {
            preview.BeginPreview(rect,GUIStyle.none);
            var size=StageGeometry.ArenaSize(stage.grid);float distance=Mathf.Max(size.x,size.y)*1.5f;
            preview.camera.transform.position=Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);preview.camera.transform.LookAt(Vector3.zero);
            Draw(cube,new Vector3(0,-.07f,0),new Vector3(size.x,.1f,size.y),new Color(.15f,.19f,.25f));
            Draw(cube,StageGeometry.CellToLocal(stage.grid,stage.playerStart)+Vector3.up*.45f,new Vector3(.35f,.9f,.35f),Color.yellow);
            Draw(sphere,StageGeometry.CellToLocal(stage.grid,stage.enemy.position)+Vector3.up*.5f,Vector3.one*.7f,new Color(.6f,.35f,.8f));
            foreach(var gem in stage.gems)Draw(sphere,StageGeometry.CellToLocal(stage.grid,gem.position)+Vector3.up*.25f,Vector3.one*.25f,gem.type==GemTypes.Heal?Color.green:gem.type==GemTypes.Attack?new Color(1,.5f,0):Color.magenta);
            preview.Render(true);return preview.EndPreview();
        }
        void Draw(Mesh mesh,Vector3 pos,Vector3 scale,Color color){var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);preview.DrawMesh(mesh,Matrix4x4.TRS(pos,Quaternion.identity,scale),material,0,block);}
        public void Dispose(){preview.Cleanup();Object.DestroyImmediate(material);}
    }
}
