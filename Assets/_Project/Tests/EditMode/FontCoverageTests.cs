using System.IO;
using System.Linq;
using Game2Week.EditorTools.Build;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class FontCoverageTests
    {
        [Test] public void 정적폰트에표준한글과전체문구가있다()
        {
            var required=File.ReadAllText("Assets/_Project/Art/Fonts/korean_static_characters.txt").Where(c=>c>='가'&&c<='힣').Distinct().ToArray();Assert.AreEqual(2350,required.Length);
            string characters=new string(required)+new string(ReleasePreparation.KoreanInContent().ToArray());
            Assert.IsTrue(ReleasePreparation.IsStaticFontCovered(ReleasePreparation.FontPath,characters));
            Assert.IsTrue(ReleasePreparation.IsStaticFontCovered(ReleasePreparation.FontPath.Replace("Regular","Bold"),characters));
        }
    }
}
