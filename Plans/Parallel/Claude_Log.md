# Claude 작업 문서 — 할 일 · 한 일

최종 갱신: 2026-10-06 · 지금 할 일: **5 → 6 → 7 → 8단계를 순서대로 한 번에** (Codex도 같은 시간에 자기 몫 진행)

> Claude는 "1. 할 일"을 위에서부터 진행하고, 단계마다 "3. 한 일"과 "작업량"을 추가한다.
> 짝 문서: Codex 쪽은 `Plans/Parallel/Codex_Log.md`. 전체 규칙은 `CLAUDE.md`와 `Plans/Parallel_Work_Plan.md`.

## 0. 작성·작업 규칙

- 기록·주석·커밋 메시지·사용자 답변은 **한국어**로 쓴다. 코드 식별자만 영어.
- 작업 폴더 `C:\Unity\Game2Week\Game2Week-claude`, 브랜치 `phase5-measure` 하나로 5~8단계 진행. Unity 배치모드·테스트는 이 폴더로만.
- 담당 파일만 고친다 (Codex 문서 0절의 "Claude 담당" 목록이 기준). 공용 연결 지점을 바꿔야 하면 사용자에게 먼저 알린다.
- 병렬 기간에는 TODO·WORKLOG·설계 MD·Work_Effort를 고치지 않고 이 문서에 기록 → main에 합칠 때 반영 (Codex 문서도 함께).
- 커밋: 단계가 끝나고 전체 테스트가 통과하면 자기 브랜치에 커밋 (사용자 "한 번에 작업" 요청). push·main 합치기는 사용자 요청 시. 커밋 전 폰트 에셋·ProjectSettings 자동 변경 되돌리기.
- HTML: `node Tools/render_plan.mjs Plans/Parallel/Claude_Log.md`.

## 1. 할 일 (순서대로)

### 5단계 — 자동 플레이 측정 도구 · 색 처음 등장 안내
1. **자동 플레이 측정 도구** (PlayMode 테스트 형태의 봇):
   - 8개 맵을 정해진 턴 수만큼 자동으로 플레이한다.
   - 봇은 적에게 접근하면서 대응한다: 노랑은 가까우면 쳐내기·회피, 빨강은 Ctrl 정지 자세, 파랑은 계속 이동.
   - 기록할 것: 맵별 피격 수, 적 접근 성공률·소요 시간, 회피·쳐내기·자세 사용, 색 통과 수, 턴 수. `BattleFeedback` 구독으로 센다.
   - 결과는 `Logs/balance_report.json`에 쓴다. `Tools/render_balance_report.mjs`가 그걸 읽어 `Plans/Balance_Report.html`(맵별 표·막대)을 만든다.
   - 자동 측정만으로 재미나 밸런스를 확정하지 않는다. 사용자 플레이 확인의 근거 자료로 쓴다.
2. **색 처음 등장 안내**:
   - 한 전투에서 빨강·파랑이 처음 나오는 순간 상단 안내를 잠깐 바꾼다 ("빨강 = Ctrl로 멈추기" / "파랑 = 계속 움직이기").
   - 색은 `ThreatPoint`에서 읽고, 문구는 `BattleTexts`에 둔다.
3. 테스트: 안내는 처음 한 번만 나오는지, 측정 도구가 8개 맵 리포트를 만드는지.
4. Codex 5단계(맵 1→8종 연결)가 합쳐진 뒤 측정을 다시 돌린다. 조정할 수치는 Codex 문서 "요청"에 적는다 (데이터는 Codex 담당).

### 6단계 — 소리 · 씬 전환
1. **AudioMixer** (`Art/Audio/GameMixer`): BGM·SFX 그룹. 설정 음량(배경음/효과음)을 dB로 바꿔 연결 (`SettingsModel`·`SettingsPanel`·`BattleAudio`).
2. **BattleAudio 확장**: `BattleFeedback`(회피·점프·착지·쳐내기·자세·피격·색 통과·발사·예고)과 `BattleEvents` 구독, 클립 비어 있으면 무음. `ThreatFeedbackView` 경고음도 믹서 SFX 그룹으로.
3. **BGM 재생기**: 씬별 BGM 슬롯 (메인·스테이지 선택·전투·결과·엔딩), 씬 전환 때 교차 페이드. 클립은 비워 두고 연결 지점만 (소리 출처는 사용자 결정).
4. **메뉴 효과음** 연결 지점: `MenuNavigator`(이동·확인·취소), `ConfirmPopup`.
5. **AudioListener 정리**: 씬마다 하나 (PlayMode 경고 제거).
6. **씬 전환 페이드**: `SceneLoader`에 검은 화면 페이드 (일시정지·timeScale 0에서도 동작, 테스트에서 끌 수 있게).
7. 테스트: 음량 → 믹서 dB 변환, 리스너 1개, 페이드 후 씬 전환 완료, 기존 흐름 회귀.

### 7단계 — 주인공 v2 · 리깅 · 애니메이션
1. `Preview/heroine_preview.html` 모델 v2: 손(쳐내기용 오른손), 측면 눈·볼 뜸 수정, 머리 결 보강. 레퍼런스는 덮어쓰지 않고 `_v2`.
2. 리깅 (three.js SkinnedMesh → GLB): 몸통·머리·양팔·양다리·양갈래 뼈.
3. 애니메이션 클립: 대기·달리기·회피·점프/착지·쳐내기(오른손 왼→오, 오→왼)·정지 자세·피격·쓰러짐.
4. Unity: `Art/Characters/Heroine/heroine_v2.glb` 임포트 → Animator Controller → `PlayerActionView`가 모터 상태로 구동 (임시 손 큐·원판은 대체 또는 보조로 남김).
5. 테스트: 상태별 Animator 상태 전환, 기존 쳐내기·자세 테스트 회귀, 화면 캡처.

### 8단계 — 출시 준비 (이름은 사용자 결정 전 → 한 곳에 모아 두기)
1. `ProductInfo` (회사·게임 이름·버전) 한 곳에서 PlayerSettings·MSIX 매니페스트·타이틀 문구가 읽도록. 지금은 자리표시자.
2. 정적 폰트 아틀라스 (KS X 1001 한글 + 영문·숫자·기호) — 동적 폰트가 매번 git 변경으로 잡히는 문제 해결.
3. Codex 스토어 이미지(`Reference/store/`)를 MSIX 로고·타일에 연결.
4. 빌드·MSIX 재검증 (`Tools/build_windows.ps1`, `Tools/package_msix.ps1`), 빌드 실행 오류 없음.
5. 이름이 정해지면 `ProductInfo`만 바꾸면 되도록 안내를 `Plans/Store_Guide.html`에 추가.

### 합칠 때 (사용자 요청 시)
- 두 브랜치를 main에 합친다 (먼저 끝난 쪽부터, 나중 쪽은 main을 받아 전체 테스트) → 두 문서 내용을 TODO·WORKLOG·설계 MD·Work_Effort에 반영.

## 2. 요청 (Claude → Codex/사용자)
- 사용자 결정 필요: 회사·게임 이름 (8단계), 소리 출처 (6단계 클립), 3D 모델 출처 (7단계는 지금처럼 three.js 코드 모델로 진행).

## 3. 한 일

### 2026-10-06 — 6-0 연결 지점 (main)
- `BattleFeedback`: 월드 사건 알림 창구 — 회피·점프·착지·쳐내기(쪽 방향)·정지 자세 완성·피격·색 통과(같은 색 0.25초에 한 번)·발사·예고. `BattleWorld.Feedback`, `PatternContext.Feedback`.
  - 주인공 쪽 Raise 연결 완료: `PlayerMover`(회피·점프·착지·자세·쳐내기), `BattleWorld`(실제 피격), `PlayerHitRule`(색 통과). 발사·예고 Raise는 Codex 6단계.
- `BattleEvents.EnemySpoke`: 탄막 턴 시작 때 적 대사 (말풍선용).
- `BattleFxRig` + 프리팹 `Prefabs/Battle/FX/BattleFxRig.prefab`: Battle 씬의 `BattleController.fxRigPrefab`에 연결 → 만들어서 `Bind(events, feedback, spawner)`. Codex가 이 프리팹 아래에 이펙트를 붙이면 씬을 안 고쳐도 됨.
- `FeedbackContractTests` (PlayMode): FxRig 연결, 적 대사·회피·점프·착지 알림.
- 7·8단계는 서로 겹치는 파일이 없어 연결 지점이 필요 없음 (Codex 7단계는 기존 `GameSession.BeginStage` 등 공개 API만 사용).

### 2026-10-06 — 5-0 연결 지점 (main)
- `ColorCombinationRules`: 빨강·파랑 동시 위험 금지 (`CanOverlap`, `IsAllowed`), 전환 유예 `SwitchGraceSeconds` 0.6초.
- `EncounterMemory` + `PatternContext.Memory`: 전투 동안 유지되는 패턴 기록. `BattleWorld`가 전투마다 하나 만들어 주입. 재도전하면 새로 시작.
- `EncounterContractTests` 2개.
- 3단계(Codex) 합침: Codex 브랜치에 main(4단계)을 받아 통합 테스트 EditMode 150/150, PlayMode 20/20 → main `020df2f`.

### 2026-10-06 — 4단계 빨강·파랑 판정 (브랜치 `phase4-color-rules` → main `58d2e54`)

**한 일**
- 입력: Player 맵에 `Brace` (왼/오른 Ctrl, 게임패드 LB), `InputReader.BraceHeld`.
- `PlayerMotorModel` 정지 자세 (`SetBrace`·`Bracing`·`BraceReady`):
  - 지상이고 회피 중이 아닐 때만 시작한다. 회피·점프 중에 누르면 끝난 뒤에 자세로 들어간다.
  - 자세 중에는 이동 입력을 무시하고, 새 회피·점프·쳐내기를 막는다.
  - 자세 전환 시간 0.08초가 지나야 `BraceReady`가 된다.
- `PlayerMover.GroundSpeed`: 경계로 제한한 뒤 실제로 움직인 수평 속도 (벽에 막히면 0에 가깝다).
- `ColorRules`(순수 로직) + `PlayerHitRule`(IHitRule) → `BattleWorld`가 `PatternContext`에 주입.
  - 빨강 = 자세 + 실제 정지 (≤ 0.05m/s)일 때 통과.
  - 파랑 = 실제 이동 ≥ 1.2m/s일 때 통과.
  - 노랑은 색 규칙으로 통과하지 않는다.
- 표시 (색만으로 구분하지 않음):
  - 탄 색 입히기 + 회전 차이 (빨강은 멈춤, 파랑은 2배 빠르게).
  - 정지 자세: 몸을 낮추고 손을 가슴 앞에, 발밑 원판이 연빨강 → 빨강.
  - HUD에 정지 자세 표시, 화면 밖 경고를 색별 글자색으로.
- 시험 패턴 `ColorTest/Pattern_RedTest`·`Pattern_BlueTest`.

**검증**
- EditMode 135/135: `ColorRuleTests` 7개 추가.
- PlayMode 19/19: `ActionPhaseFourTests` 3개 추가, 모두 실제 키 입력으로 확인.
  - Ctrl+W를 누르면 멈춰 있고 빨강만 통과한다.
  - 이동 중이면 파랑이 통과하고, 벽에 막혀 입력만 하면 맞는다.
  - 빨강 패턴을 돌렸을 때 자세면 HP가 그대로이고, 자세가 없으면 HP가 줄어든다.

**남은 것**
- 기존 `YellowTrainingPattern` 예고선은 노랑 고정이다 (새 Graph 패턴은 색별 예고선).
- 손·자세 표현은 리깅 전 임시 큐 (7단계에서 대체).

### 2026-10-06 — 병렬 준비
- 병렬 작업 계획 (`Plans/Parallel_Work_Plan.md`): 파일 담당표·worktree·합치기 규칙·5~8단계 분할.
- 0단계 연결 지점: `ITrajectory`·`AttackColor`·`IHitRule`·`ThreatPoint`.
- 작업 폴더 두 개, 사람 기준 작업량 표시 (`Plans/Work_Effort.md` + 대시보드 카드).

### 2026-10-06 — 이전 작업 (병렬 이전, WORKLOG에 상세)
- 기획·문서, 주인공 모델 v1, Unity 프로젝트 셋업, 전투 상태 머신·데이터·UI, 맵툴 M1~M3, 서비스 흐름 S1~S6(세이브·설정·스테이지 선택·일시정지·결과/엔딩·빌드/MSIX), 타이밍 공격·방사형 탄막, 세 결말 검증, 키보드 포커스 버그, 맵 1-3~1-5.

## 작업량

중급 Unity 개발자 1명 기준 추정 시간 (8시간 = 1일). main의 `Plans/Work_Effort.md`에 옮겨진 줄은 ✓.

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Claude | 액션 4단계 | Ctrl 정지 자세 입력·이동 모델, 실제 속도, 빨강/파랑 색 규칙 + 주입, 색 표시·HUD, 시험 패턴, EditMode 7·PlayMode 3 ✓ | 16 |
| 2026-10-06 | Claude | 병렬 준비 | 3단계 통합 테스트·합치기, 5-0 연결 지점(색 조합 규칙·전투 기록) + 테스트, 할 일 문서 정리 ✓ | 4 |
| 2026-10-06 | Claude | 병렬 준비 | 6-0 연결 지점(BattleFeedback·EnemySpoke·BattleFxRig 씬 연결) + 테스트, 5~8단계 지시 작성 ✓ | 5 |
