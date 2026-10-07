using Game2Week.Battle;
using Game2Week.Battle.UI;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>
    /// Battle 씬의 조립 루트: 스테이지로 경기장을 만들고, 전투 모델·상태 머신·UI·연출을 연결해 전투를 시작한다.
    /// 전투가 끝나면 결과를 GameSession에 넘기고 Result 씬으로.
    /// </summary>
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] InputReader input;
        [SerializeField] GemRewardSettings gemRewards;
        [SerializeField] StageSpawner spawner;
        [SerializeField] BattleCameraDirector cameraDirector;
        [SerializeField] BattleWorld world;
        [SerializeField] BattlePresentation presentation;
        [SerializeField] BattleUi ui;
        [SerializeField] StatusBar statusBar;
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] EnemyHealthBar enemyHealthBar;
        [Tooltip("효과음 연결 지점 (선택)")]
        [SerializeField] BattleAudio audioHooks;
        [Tooltip("전투 이펙트 묶음 프리팹 (선택) — BattleFeedback·BattleEvents를 구독")]
        [SerializeField] BattleFxRig fxRigPrefab;

        BattleStateMachine machine;
        bool finished;
        SpareFeedbackBinding spareFeedback;

        /// <summary>전투 시간 (일시정지 제외) — 클리어 기록용</summary>
        public float ElapsedSeconds { get; private set; }
        public PauseMenu Pause => pauseMenu;

        public BattleContext Context { get; private set; }
        public BattleUi Ui => ui;
        public BattleWorld World => world;
        public StageSpawner Spawner => spawner;
        public BattleCameraDirector CameraDirector => cameraDirector;
        public BattleFxRig FxRig { get; private set; }
        public BattleAudio Audio => audioHooks;
        public bool IsReady { get; private set; }

        void Start()
        {
            // 에디터에서 Battle 씬을 바로 재생한 경우 1번 스테이지로
            if (session.CurrentStage == null && !session.BeginStage(Mathf.Max(0, session.StageIndex)))
            {
                Debug.LogError("Battle: 불러올 스테이지가 없음");
                return;
            }

            var stage = session.CurrentStage;
            var enemyData = session.CurrentEnemy;
            spawner.Spawn(stage, enemyData);
            cameraDirector.Setup(spawner.Arena.Size, spawner.Arena.transform.position, spawner.Enemy ? spawner.Enemy.transform : null);
            world.Init(spawner, stage);
            world.ConfigureActions(input, cameraDirector);
            world.BindThreatVolume(() => session.Save.Settings.sfxVolume);

            var events = new BattleEvents();
            var player = new PlayerCombatant(session.Player);
            var enemy = new EnemyCombatant(enemyData);
            var gems = new GemField(stage.gems, stage.gemRules.maxPerTurn);
            machine = new BattleStateMachine();
            Context = new BattleContext(machine, events, input, ui, world, stage, player, enemy, gems, gemRewards, Finish, StagePatternSource(stage));
            Context.ConfigureDialogues(new Game2Week.Dialogue.DialogueRepository(Game2Week.Dialogue.DialogueRepository.DefaultDirectory));
            spareFeedback = new SpareFeedbackBinding(world.Feedback, Context);
            BattleStates.RegisterAll(machine, Context);

            presentation.Bind(events, spawner.Enemy, world.Player,player.MaxHp);
            if (audioHooks)
            {
                audioHooks.Bind(events, () => session.Save.Settings.sfxVolume);
                audioHooks.BindFeedback(world.Feedback);
            }
            if (fxRigPrefab)
            {
                FxRig = Instantiate(fxRigPrefab, transform);
                FxRig.Bind(events, world.Feedback, spawner);
            }
            statusBar.Bind(events, player.Data.DisplayName, player.Data.Level, player.CurrentHp, player.MaxHp);
            if (enemyHealthBar) enemyHealthBar.Bind(events, enemyData.DisplayName, enemy.CurrentHp, enemy.MaxHp);
            Context.RefreshSpare();
            machine.StateChanged += (_, id) => events.RaiseStateChanged(id);
            ui.Dialogue.SpeedMultiplier = () => Save.TextSpeeds.Multiplier(session.Save.Settings.textSpeed);

            input.EnableUI();
            IsReady = true;
            machine.ChangeState(BattleStateId.Intro);
        }

        void Update()
        {
            if (!IsReady || finished || (pauseMenu && pauseMenu.IsPaused)) return;
            ElapsedSeconds += Time.deltaTime;
            machine.Tick(Time.deltaTime);
        }

        void OnDestroy()
        {
            spareFeedback?.Dispose();
            if (input) input.EnableUI();
        }

        /// <summary>스테이지가 패턴을 지정했으면 그 목록을 순서대로, 아니면 null (적 데이터 순서 사용)</summary>
        System.Func<AttackPatternData> StagePatternSource(Stages.StageDefinition stage)
        {
            var list = new System.Collections.Generic.List<AttackPatternData>();
            if (session.Catalog)
                foreach (var name in stage.enemyTurn.patterns)
                    if (session.Catalog.FindPattern(name) is { } p) list.Add(p);
            if (list.Count == 0) return null;
            int next = 0;
            return () => list[next++ % list.Count];
        }

        void Finish(BattleOutcome outcome)
        {
            if (finished) return;
            finished = true;
            if (pauseMenu) pauseMenu.CanPause = false;
            session.EndBattle(outcome, ElapsedSeconds);
            SceneLoader.Load(SceneNames.Result);
        }
    }
}
