using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game2Week.Tests
{
    public class GraphicsRuntimeTests
    {
        [SetUp] public void SetUp()=>TestSave.Begin();
        [TearDown] public void TearDown(){Time.timeScale=1;TestSave.End();}
        [UnityTest]
        public IEnumerator ThreeThemes_Environment_AndFxEvents_AreConnected()
        {
            for(int i=0;i<3;i++)
            {
                BattleController battle=null;yield return SceneFlowTests.EnterStage(i,value=>battle=value);
                Assert.IsNotNull(Object.FindAnyObjectByType<VisualEnvironment>());
                Assert.IsNotNull(battle.Spawner.ActiveTheme);
                battle.Context.ChangeState(BattleStateId.EnemyTurn);yield return null;
                var accents=battle.FxRig.GetComponent<CombatAccentFxModule>();Assert.IsNotNull(accents);
                var at=battle.World.Player.transform.position;
                int before=accents.Spawned;
                battle.World.Feedback.RaisePlayerParried(at,1);battle.World.Feedback.RaisePlayerDodged(at);
                battle.World.Feedback.RaiseWarningStarted(AttackColor.Yellow,at);battle.World.Feedback.RaiseEnemyPhaseChanged(1);
                Assert.AreEqual(before+4,accents.Spawned);
                yield return null;SceneCapture.Save("phase11_theme_"+battle.Spawner.ActiveTheme.id);
                battle.Context.ChangeState(BattleStateId.ItemMenu);yield return null;
            }
        }
        [UnityTest]
        public IEnumerator MergedRendering_KeepsSkinnedGeometry_AndHiddenPartsStayHidden()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);
            yield return new WaitForSeconds(.3f);
            var sources=battle.World.Player.GetComponentsInChildren<SkinnedMeshRenderer>();
            var merged=System.Array.Find(sources,renderer=>renderer.name=="통합외형");Assert.IsNotNull(merged);
            Assert.IsTrue(merged.enabled);Assert.Less(merged.sharedMesh.subMeshCount,sources.Length);
            for(int pose=0;pose<2;pose++)
            {
                var baked=new Mesh();merged.BakeMesh(baked);var combined=baked.vertices;int offset=0;float error=0;
                foreach(var source in sources)
                {
                    if(source==merged)continue;
                    Assert.IsFalse(source.enabled);
                    var original=new Mesh();source.BakeMesh(original);
                    foreach(var vertex in original.vertices)
                    {error=Mathf.Max(error,Vector3.Distance(source.transform.TransformPoint(vertex),merged.transform.TransformPoint(combined[offset++])));}
                    Object.Destroy(original);
                }
                Assert.AreEqual(combined.Length,offset);Assert.Less(error,.005f,"통합 전후 실제 뼈 변형이 같아야 한다.");Object.Destroy(baked);
                battle.World.Player.Move(Vector2.right,.1f);yield return null;
            }
            battle.World.Player.SetVisible(false);battle.World.Player.SetVisible(true);
            foreach(var source in sources)Assert.AreEqual(source==merged,source.enabled);
        }
        [UnityTest]
        public IEnumerator BulletPool_ChangesShapeForEveryLaunch_AndPreservesTint()
        {
#if UNITY_EDITOR
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Battle/Patterns/Bullet_Yellow.prefab");
            var root=Object.Instantiate(prefab);var bullet=root.GetComponent<Bullet>();var appearance=root.GetComponent<BulletAppearance>();
            Mesh yellow=null,red=null,blue=null;
            foreach(var color in new[]{AttackColor.Yellow,AttackColor.Red,AttackColor.Blue,AttackColor.Yellow})
            {
                bullet.Color=color;bullet.Launch(Vector3.zero,Vector3.forward);
                Assert.IsNotNull(appearance.CurrentMesh);
                if(color==AttackColor.Yellow){if(yellow)Assert.AreSame(yellow,appearance.CurrentMesh);else yellow=appearance.CurrentMesh;}
                if(color==AttackColor.Red)red=appearance.CurrentMesh;if(color==AttackColor.Blue)blue=appearance.CurrentMesh;
                bullet.Deactivate();yield return null;
            }
            Assert.AreNotSame(yellow,red);Assert.AreNotSame(red,blue);Assert.AreNotSame(yellow,blue);
            Object.Destroy(root);
#else
            yield return null;
#endif
        }
    }
}
