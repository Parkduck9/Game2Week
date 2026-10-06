using UnityEngine;

namespace Game2Week.Core
{
    /// <summary>출시 이름을 모으는 설정. 회사/제품 이름은 기존 세이브 경로와 같게 유지한다.</summary>
    [CreateAssetMenu(menuName="Game2Week/Product Info")]
    public sealed class ProductInfo : ScriptableObject
    {
        public string companyName="DefaultCompany";
        public string productName="Game2Week";
        public string version="0.1.0";
        public string displayName="타이틀 (가제)";
        public string publisherDisplayName="Game2Week";
        public string packageIdentity="Game2Week.Dev";
        public string packagePublisher="CN=Game2Week Dev";
        public string description="3D 턴제 전투 게임";
        public static ProductInfo Current{get;private set;}
        public static string Title=>Current?Current.displayName:"타이틀 (가제)";
        void OnEnable()=>Current=this;
        void OnDisable(){if(Current==this)Current=null;}
        public string MsixVersion=>VersionForMsix(version);
        public static string VersionForMsix(string value)
        {
            if(!System.Version.TryParse(value,out var parsed)||parsed.Major>65535||parsed.Minor>65535||parsed.Build>65535||parsed.Revision>65535)
                throw new System.ArgumentException("버전은 0~65535의 숫자 3~4자리로 지정하세요.");
            if(parsed.Build<0)throw new System.ArgumentException("버전은 숫자 3~4자리여야 합니다.");
            return $"{parsed.Major}.{parsed.Minor}.{parsed.Build}.0";
        }
    }
}
