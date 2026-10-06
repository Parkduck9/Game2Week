using Game2Week.Core;
using NUnit.Framework;
using UnityEditor;

namespace Game2Week.Tests
{
    public class ProductInfoTests
    {
        [TestCase("0.1.0","0.1.0.0")][TestCase("1.2.3.4","1.2.3.0")]
        public void 버전변환(string value,string expected)=>Assert.AreEqual(expected,ProductInfo.VersionForMsix(value));
        [TestCase("1.2")][TestCase("1.2.70000")][TestCase("abc")]
        public void 잘못된버전거부(string value)=>Assert.Throws<System.ArgumentException>(()=>ProductInfo.VersionForMsix(value));
        [Test] public void 기존세이브경로와사전로드유지()
        {var info=AssetDatabase.LoadAssetAtPath<ProductInfo>("Assets/_Project/Data/ProductInfo.asset");Assert.AreEqual("DefaultCompany",info.companyName);Assert.AreEqual("Game2Week",info.productName);Assert.Contains(info,PlayerSettings.GetPreloadedAssets());}
    }
}
