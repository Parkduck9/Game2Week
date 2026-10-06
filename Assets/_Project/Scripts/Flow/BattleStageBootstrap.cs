using Game2Week.Battle;
using Game2Week.Battle.View;
using Game2Week.Core;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>
    /// Battle 씬 시작 시 현재 스테이지로 경기장·주인공·적·보석을 배치하고 카메라를 맞춘다.
    /// 06단계 BattleController가 생기면 그쪽에서 이 과정을 호출하도록 옮긴다.
    /// </summary>
    public sealed class BattleStageBootstrap : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] StageSpawner spawner;
        [SerializeField] BattleCameraDirector cameraDirector;
        [Tooltip("확인용: 확률과 상관없이 모든 보석을 보여 준다")]
        [SerializeField] bool showAllGemsForPreview;

        public GemField Gems { get; private set; }
        public StageSpawner Spawner => spawner;
        public BattleCameraDirector CameraDirector => cameraDirector;
        public bool IsReady { get; private set; }

        /// <summary>테스트·확인용</summary>
        public bool ShowAllGemsForPreview
        {
            get => showAllGemsForPreview;
            set => showAllGemsForPreview = value;
        }

        void Start()
        {
            // 에디터에서 Battle 씬을 바로 재생한 경우 1번 스테이지로
            if (session.CurrentStage == null && !session.BeginStage(Mathf.Max(0, session.StageIndex)))
            {
                Debug.LogError("Battle: 불러올 스테이지가 없음");
                return;
            }

            var stage = session.CurrentStage;
            spawner.Spawn(stage, session.CurrentEnemy);
            cameraDirector.Setup(spawner.Arena.Size, spawner.Arena.transform.position, spawner.Enemy ? spawner.Enemy.transform : null);
            cameraDirector.Show(BattleShot.Overview);

            Gems = new GemField(stage.gems, stage.gemRules.maxPerTurn);
            RefreshGems();
            IsReady = true;
        }

        public void RefreshGems()
        {
            if (showAllGemsForPreview)
            {
                var all = new System.Collections.Generic.List<string>(spawner.Gems.Keys);
                spawner.ShowGems(all);
            }
            else spawner.ShowGems(Gems.RollForTurn());
        }
    }
}
