using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Battle.View
{
    public static class ArenaDecoration
    {
        public static Transform Build(BattleArena arena,ArenaTheme theme)
        {
            if(!theme)return null;
            var root=new GameObject("경기장 테마_"+theme.id).transform;root.SetParent(arena.transform,false);
            foreach(var renderer in arena.GetComponentsInChildren<MeshRenderer>())
            {
                Color color=renderer.name.StartsWith("Wall_")?theme.wall:renderer.name=="Floor"?theme.floor:theme.accent*.6f;
                var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);renderer.SetPropertyBlock(block);
            }
            var size=arena.Size;
            int props=Mathf.Clamp(theme.propCount,0,24);
            for(int i=0;i<props;i++)
            {
                float side=i%2==0?-1:1;
                float z=Mathf.Lerp(-size.y*.5f,size.y*.5f,(i/2f+1)/(props/2f+1));
                Box(root,new Vector3(side*(size.x*.5f+1.2f),.9f,z),new Vector3(.65f,1.8f,.65f),theme.propMaterial,theme.wall);
                Box(root,new Vector3(side*(size.x*.5f+1.2f),1.85f,z),new Vector3(.85f,.12f,.85f),theme.propMaterial,theme.accent);
            }
            for(int i=0;i<10;i++)
            {
                float z=Mathf.Lerp(-size.y*.5f+1,size.y*.5f-1,i/9f);
                Box(root,new Vector3(0,.006f,z),new Vector3(size.x*.88f,.008f,theme.checker?.22f:.04f),theme.propMaterial,theme.accent*.55f);
            }
            return root;
        }
        static void Box(Transform root,Vector3 at,Vector3 scale,Material material,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(root,false);go.transform.localPosition=at;go.transform.localScale=scale;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);renderer.SetPropertyBlock(block);
        }
    }
}
