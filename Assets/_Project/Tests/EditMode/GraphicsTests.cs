using System.Linq;
using Game2Week.Data;
using Game2Week.EditorTools.Build;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    public class GraphicsTests
    {
        [Test]
        public void Themes_AllEightStagesResolve_UnknownFallsBack()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ArenaThemeCatalog>(GraphicsAuthoring.ThemePath);
            Assert.AreEqual(3,catalog.themes.Length);
            Assert.AreEqual("default",catalog.Find("없는 테마").id);
            var repository=new StageRepository(StageRepository.DefaultDirectory);
            foreach(var id in repository.LoadIndex().stages)
            {var stage=repository.LoadStage(id).Stage;Assert.IsTrue(catalog.themes.Any(theme=>theme.id==stage.theme));}
        }
        [Test]
        public void ShaderGraphAndFxShaders_AreSupported_AndHaveNoErrors()
        {
            foreach(var path in new[]{"ToonCharacter.shadergraph","BulletGlow.shader","FxGlow.shader","DepthOutline.shader","GradientSky.shader"})
            {
                var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/"+path);
                Assert.IsNotNull(shader,path);Assert.IsTrue(shader.isSupported,path);
                Assert.IsFalse(ShaderUtil.ShaderHasError(shader),path);
            }
        }
    }
}
