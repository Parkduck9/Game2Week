using System.Linq;
using Game2Week.Battle.View.Animation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game2Week.Tests
{
    public class AnimationPhaseTests
    {
        [Test] public void 표현우선순위와강약피격은판정수치를바꾸지않는다()
        {
            Assert.IsTrue(PlayerAnimationMap.StrongHit(4,20));Assert.IsFalse(PlayerAnimationMap.StrongHit(1,20));
            Assert.AreEqual(PlayerMotion.HitStrong,PlayerAnimationMap.Present(PlayerMotion.Hit,true,true,true,PlayerMotion.Talk,PlayerMotion.Victory));
            Assert.AreEqual(PlayerMotion.Fall,PlayerAnimationMap.Present(PlayerMotion.Fall,true,true,true,PlayerMotion.Talk,PlayerMotion.Victory));
            Assert.AreEqual(PlayerMotion.Strafe,PlayerAnimationMap.Present(PlayerMotion.Run,true,false,false,null,null));
            Assert.AreEqual(PlayerMotion.Stop,PlayerAnimationMap.Present(PlayerMotion.Idle,false,true,false,null,null));
            Assert.AreEqual(PlayerMotion.Nod,PlayerAnimationMap.Present(PlayerMotion.Idle,false,false,false,PlayerMotion.Nod,PlayerMotion.Spare));
        }
        [Test] public void 여덟방향은캐릭터회전에맞춰변환된다()
        {
            var rotation=Quaternion.Euler(0,90,0);
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;var local=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
                Assert.Less(Vector2.Distance(new Vector2(local.x,local.z),PlayerAnimationMap.LocalDirection(rotation*local,rotation)),.001f);
            }
        }
        [Test] public void 등록된대화자세와적우선순위만사용한다()
        {
            foreach(var id in new[]{"idle","talk","nod","surprise","victory","spare"})
            {Assert.IsTrue(PlayerAnimationMap.TryPose(id,out _));Assert.IsTrue(EnemyMotionMap.TryPose(id,out _));}
            Assert.IsFalse(PlayerAnimationMap.TryPose("없는자세",out _));Assert.IsFalse(EnemyMotionMap.TryPose("없는자세",out _));
            Assert.AreEqual(EnemyMotion.Defeated,EnemyMotionMap.Resolve(true,true,true,true,true,true,true,EnemyMotion.Nod));
            Assert.AreEqual(EnemyMotion.Phase,EnemyMotionMap.Resolve(false,false,false,true,true,true,true,null));
            Assert.AreEqual(EnemyMotion.Relaxed,EnemyMotionMap.Resolve(false,false,false,false,false,false,true,null));
        }
        [Test] public void 주인공툰재질은원본의색을보존한다()
        {
            var originals=AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Characters/Heroine/heroine_v2.glb").OfType<Material>().ToArray();
            Assert.Greater(originals.Length,0);
            foreach(var source in originals)
            {
                var target=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Presentation/툰_"+source.name+".mat");
                Assert.IsNotNull(target);Assert.Less(Vector4.Distance(Game2Week.EditorTools.Build.GraphicsAuthoring.OriginalColor(source),target.GetColor("_BaseColor")),.00001f);
            }
        }
        [Test] public void 블렌드트리는여덟방향과중앙대기클립을포함한다()
        {
            const string path="Assets/_Project/Art/Characters/Heroine/HeroineV2.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            var state=controller.layers[0].stateMachine.states.First(item=>item.state.name=="Strafe").state;
            var tree=state.motion as BlendTree;Assert.IsNotNull(tree);Assert.AreEqual(9,tree.children.Length);
            Assert.AreEqual(BlendTreeType.FreeformDirectional2D,tree.blendType);
            foreach(var child in tree.children){Assert.IsNotNull(child.motion);Assert.Greater(AnimationUtility.GetCurveBindings((AnimationClip)child.motion).Length,0);}
            foreach(var name in new[]{"Stop","HitStrong","Victory","Spare","Talk","Nod","Surprise"})
                Assert.IsNotNull(controller.layers[0].stateMachine.states.First(item=>item.state.name==name).state.motion);
        }
    }
}
