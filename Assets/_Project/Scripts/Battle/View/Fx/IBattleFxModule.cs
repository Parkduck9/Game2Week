namespace Game2Week.Battle.View
{
    /// <summary>BattleFxRig 프리팹 아래에 붙는 이펙트 모듈. Rig가 Bind될 때 같이 연결된다.</summary>
    public interface IBattleFxModule
    {
        void Bind(BattleFxRig rig);
    }
}
