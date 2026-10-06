# Codex 작업 문서 — 할 일 · 한 일

최종 갱신: 2026-10-06 (Claude가 5~8단계 지시 작성) · 지금 할 일: **5 → 6 → 7 → 8단계를 순서대로 한 번에**

> Codex는 이 문서의 "1. 할 일"을 위에서부터 순서대로 끝까지 진행한다. 단계 하나가 끝날 때마다 "3. 한 일"과 "작업량"을 이 문서에 추가하고 다음 단계로 넘어간다.
> 짝 문서: Claude 쪽은 `Plans/Parallel/Claude_Log.md` (같은 시간에 Claude도 5~8단계 자기 몫을 진행). 전체 규칙은 `CLAUDE.md`와 `Plans/Parallel_Work_Plan.md`.

## 0. 작성·작업 규칙 (꼭 지킬 것)

- **모든 기록·주석·커밋 메시지·답변은 한국어로 쓴다.** 영어로 바꾸지 않는다. 코드 식별자(클래스·변수 이름)만 영어.
- 작업 폴더는 `C:\Unity\Game2Week\Game2Week-codex` 하나, 브랜치는 `phase5-director` 하나로 5~8단계를 계속 진행한다.
  - Unity 배치모드·테스트는 `-projectPath`를 이 폴더로만 준다. 에디터: `C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe`
  - 테스트: `-runTests -testPlatform EditMode|PlayMode -assemblyNames Game2Week.Tests.EditMode|Game2Week.Tests.PlayMode` (`-quit` 없음)
- **담당 파일만 고친다** (각 단계 표 + 아래 "공용·Claude 담당"). 꼭 필요하면 고치지 말고 "2. 요청"에 적고 그 부분은 우회한다.
- `TODO.md`·`WORKLOG.md`·`CLAUDE.md`·`Plans/Action_Balance_Plan.md`·`Plans/Work_Effort.md`·`Plans/Parallel_Work_Plan.md`·`Plans/Parallel/Claude_Log.md`는 고치지 않는다 (Claude가 합칠 때 이 문서를 보고 반영).
- **커밋**: 사용자가 "한 번에 작업"을 요청했으므로 **단계 하나가 끝나고 전체 테스트가 통과하면 이 브랜치에 커밋한다** (메시지 예: `5단계 (Codex): ...`). push·main 합치기는 하지 않는다 — 사용자가 요청하면 Claude가 한다.
- 커밋 전: `Assets/_Project/Art/Fonts/Pretendard-Regular SDF.asset`과 `ProjectSettings/` 자동 변경을 `git checkout`으로 되돌리고, `git diff --cached --name-only`로 담당 밖 파일이 없는지 확인.
- 임시 에디터 스크립트는 `Assets/Editor/`에 두고 실행 후 폴더째 삭제. 스테이지 JSON은 맵툴 코드(`StageEditorModel`)로만 저장.
- `node`는 PATH에 없다: `C:\Program Files\Adobe\Adobe Creative Cloud Experience\libs\node.exe`. 이 문서의 HTML은 `node Tools/render_plan.mjs Plans/Parallel/Codex_Log.md`.
- 단계마다: EditMode·PlayMode 전체 통과 (결과 xml 경로 기록) → "3. 한 일"에 한 일·결정·검증·요청 → "작업량"에 줄 추가 → HTML 재생성 → 커밋 → 다음 단계.
- 막히면 (사용자 결정 필요·담당 밖 파일 필요) 그 항목만 "2. 요청"에 적고 나머지를 계속 진행한다. 질문하려고 멈추지 않는다.

### 공용 연결 지점 — 사용만, 수정 금지
`ITrajectory`·`TrajectoryLaunch`, `AttackColor`, `IHitRule`, `IThreatSource`·`ThreatPoint`, `IAttackPattern`(`PatternContext` — `Memory`·`Feedback` 포함), `Bullet`, `ColorCombinationRules`, `EncounterMemory`, `BattleFeedback`, `BattleEvents`

### Claude 담당 — 수정 금지
플레이어·입력·카메라 (`PlayerMotorModel`, `PlayerMover`, `PlayerActionView`, `PlayerHitRule`, `InputReader`, `GameControls`, `BattleCameraDirector`), `BattleWorld`, `BattleController`, `Battle/States/`, `Battle/UI/`, `BattleTexts`, `BattleAudio`, `ThreatFeedbackView`, `ActionStatusView`, 모든 씬(`Scenes/`), `Scripts/Flow`·`Save`·`UI`·`Core`, `Preview/`, `Art/Characters/`, `Tools/build_windows.ps1`·`package_msix.ps1`·`msix/`, `ProjectSettings/`, `Tests/PlayMode/Balance*`·`ActionPhaseFourTests`·`FeedbackContractTests`·`BattleInputTests`·`SceneFlowTests`·`BattleRouteTests`

## 1. 할 일 (순서대로)

### 5단계 — 패턴 누적·발사 간격·예산, 8개 맵 연결

**연결 지점 (main에 있음)**: `ColorCombinationRules` (`CanOverlap`, `IsAllowed`, `SwitchGraceSeconds` 0.6초 — 빨강·파랑 동시 위험 금지), `EncounterMemory` + `PatternContext.Memory` (`GetOrCreate<T>("키")` — 전투 동안 유지, 재도전하면 새로).

**만들 것**
1. **DifficultyProfile** (ScriptableObject): 단계별 기준 발사 간격(1.40 → 0.65초), 탄속 배율, 동시 위험 상한(1~4단계 1, 5단계부터 2), 공유 예산(초당 발사 수·살아 있는 탄 수).
2. **PatternEncounterData** (ScriptableObject): 이 맵에서 새로 배우는 패턴 1개, 이미 배운 패턴 목록, 허용 조합, 사용할 DifficultyProfile.
3. **PatternDirector** (IAttackPattern 루트 프리팹, 하위 패턴 관리):
   - 새 패턴은 그 맵 첫 턴에 단독으로 먼저. 소개 전에 턴이 끝나면 다음 턴에 다시 먼저 (`context.Memory`).
   - 이후 셔플 백으로 배운 패턴 순환 (같은 패턴 연속 금지, 시드 재현).
   - 반복 간격 = `max(최소 간격, 기본 간격 × 단계 기준 간격 / 1.40)` — 배율은 한 번만.
   - 동시 상한·공유 예산. 예산이 모자라면 묶음 전체를 미룬다 (한 프레임에 몰아 쏘지 않음).
   - `ColorCombinationRules`로 빨강·파랑이 동시에 살아 있지 않게, 바뀔 때 `SwitchGraceSeconds` 비우기.
   - `End()`에서 하위 패턴·예고·예약·탄 모두 정리.
4. **8개 맵 연결** (기본안 — 바꾸면 이유 기록):

   | 맵 | 새로 배우는 패턴 | 누적 | 기준 간격 | 동시 상한 |
   |---|---|---:|---:|---:|
   | 1-1 | 노랑 직선 (기존 `Pattern_YellowTraining` 유지) | 1 | 1.40 | 1 |
   | 1-2 | 노랑 사인파 | 2 | 1.25 | 1 |
   | 1-3 | 노랑 측면 교대 / 지그재그 | 3 | 1.10 | 1 |
   | 1-4 | **빨강** 직선 (새 색은 익힌 궤적으로만 소개) | 4 | 1.00 | 1 |
   | 1-5 | 노랑 포물선 (점프로 넘기) | 5 | 0.90 | 2 |
   | 1-6 | **파랑** 사인파 (새 색은 익힌 궤적으로) | 6 | 0.80 | 2 |
   | 1-7 | 원호 (빨강·파랑 교대 — 겹침 금지) | 7 | 0.70 | 2 |
   | 1-8 | 8자 | 8 | 0.65 | 2 |

   - 새 색과 새 궤적을 같은 맵에서 동시에 소개하지 않는다. 맵 크기·배치·보석·턴 시간은 유지. 1-1은 단일 패턴 그대로.
   - 시험용 `ColorTest/Pattern_RedTest`·`Pattern_BlueTest`가 카탈로그에 있다 — 써도 되고 Graph 패턴으로 대체해도 된다.
5. **테스트**: Director (새 패턴 우선·턴 넘어 유지·셔플 백 연속 금지·간격 단조 감소·예산·색 겹침 금지·End 정리), 8개 맵 데이터·패턴 연결, 전투 회귀.

**담당 파일**: 새 `Scripts/Battle/Patterns/Director/`, `Scripts/Data/Patterns/`, `Data/Patterns/Director/`, `Prefabs/Battle/Patterns/Director/` · 기존 패턴 쪽 전부(Bullet·공용 제외), `Editor/Patterns/`, `Editor/Stages/`, `Data/ContentCatalog.asset`, `StreamingAssets/Stages/`(맵툴로만) · 테스트 새 `PatternDirector*`, 기존 `WavePatternTests`·`TrajectoryGraphTests`·`PatternEditor*`·`ActionPhaseTwoTests`, **`BattleStageTests`**(맵 패턴이 바뀌면 단언 수정 허용).

### 6단계 — 전투 연출 이펙트

**연결 지점 (main에 있음)**
- `BattleFeedback` (`BattleWorld.Feedback`, `PatternContext.Feedback`): `PlayerDodged`·`PlayerJumped`·`PlayerLanded`·`PlayerParried(탄 위치, 빠져나가는 쪽 ±1)`·`PlayerBraced`·`PlayerHit`·`ColorPassed(색, 위치)` — 주인공 쪽은 Claude 코드가 이미 Raise한다. `BulletFired(색, 위치)`·`WarningStarted(색, 위치)`는 **패턴이 Raise해야 한다 (Codex)**.
- `BattleEvents.EnemySpoke(대사)` — 탄막 턴 시작 때 Raise됨. 그 밖에 `StateChanged`·`EnemyDamaged`·`PlayerDamaged`·`BattleEnded`.
- `BattleFxRig` (`Scripts/Battle/View/Fx/BattleFxRig.cs`) + 프리팹 `Prefabs/Battle/Fx/BattleFxRig.prefab` — Battle 씬에 이미 연결돼 있고, BattleController가 만들어 `Bind(events, feedback, spawner)` 한다. **새 이펙트는 이 프리팹 아래에 붙이고 `OnBound()`에서 구독** → 씬을 고칠 필요 없음. `BattleFxRig.cs`와 프리팹은 이 단계부터 Codex 담당 (Bind 시그니처만 유지).

**만들 것**
1. 패턴(`GraphAttackPattern`·`YellowTrainingPattern`·`PatternDirector` 하위)이 발사·예고 때 `context.Feedback.RaiseBulletFired / RaiseWarningStarted`.
2. 적 말풍선 (월드 공간 UI): `EnemySpoke`를 받아 적 머리 위에 표시, 탄막 턴이 끝나면 숨김 (`StateChanged`). 글꼴은 TMP 기본(Pretendard).
3. 피격 파티클 정리: 클로즈업에서 너무 큰 큐브 → 크기·위치(적 표면/카메라 쪽) 조정 (`BattleEffects`, `FX_HitBurst`).
4. 주인공 동작 효과: 회피 잔상, 점프/착지 먼지, 쳐내기 섬광(빠져나가는 쪽 방향), 정지 자세 완성 고리, 색 통과 반짝임(빨강·파랑), 피격 번쩍임.
5. 발사·예고 효과 (색별, 색만으로 구분하지 않게 모양 차이도).
6. 풀링 (매 발사 Instantiate 금지), 일시정지(Time.timeScale 0)에서 멈춤, 전투 종료 시 정리.
7. 테스트: FxRig 구독·이펙트 생성·풀 재사용·종료 정리, 말풍선 표시/숨김. 화면 캡처는 `SceneCapture.Save`.

**담당 파일**: `Scripts/Battle/View/Fx/` (BattleFxRig 포함), 새 `Prefabs/Battle/Fx/` 하위, 기존 `BattleEffects.cs`·`BattlePresentation.cs`·`EnemyView.cs`·`Prefabs/Battle/FX_*`·`Prefabs/Enemies/`, `Art/Placeholder/Materials/FX_*` · 테스트 새 `Tests/*/BattleFx*`. **주의**: `BattleWorld`가 `BattleEffects.PlayGemPickup`, `BattleController`가 `BattlePresentation.Bind(events, enemy, player)`를 부르므로 이 둘의 공개 메서드는 유지. 카메라 흔들림은 기존처럼 `BattleCameraDirector.Shake`만 사용 (진폭 조절).

### 7단계 — 맵툴 2차 (M5)

**연결 지점**: 필요 없음 (기존 공개 API만 사용 — `GameSession.BeginStage(index)`, `SceneNames`, `StageRepository`, `StageEditorModel`).

**만들 것**
1. Stage Editor에 3D 미리보기: Pattern Editor와 같은 `PreviewRenderUtility` 방식으로 경기장·시작점·적·보석 배치를 3D로.
2. "바로 플레이": 지금 편집 중인 스테이지를 저장 → `GameSession`을 그 스테이지로 → Battle 씬으로 Play 진입. 사용자가 열어 둔 씬은 저장 여부를 물은 뒤 Play가 끝나면 원래 씬으로 돌아온다 (`EditorSceneManager`, `EditorApplication.playModeStateChanged`).
3. 일괄 배치: 영역 드래그로 보석 여러 개 배치·삭제, 좌우/상하 대칭 복사.
4. 기존 검증(StageValidator)·되돌리기·체크섬 유지. 테스트: StageEditorModel 일괄 배치·대칭 로직 EditMode.

**담당 파일**: `Editor/Stages/`, 새 `Tests/EditMode/StageEditorBatch*`, 기존 `StageEditorModelTests`.

### 8단계 — 출시 자료 (스토어)

**전제**: 회사·게임 이름은 아직 사용자 결정 전 → **이미지에 제목 글자를 넣지 않는다** (로고 자리만 비움). 이름이 들어가는 문구는 `{게임 이름}` 자리표시자로.

**만들 것**
1. 스토어 이미지 세트 (이미지 생성 사용, 기존 `Reference/title_screen.png`·`store_screenshot.png`·주인공 3면도를 참고): 정사각 로고 300×300·150×150·71×71·44×44, 와이드 타일 310×150, 대표 이미지 1920×1080, 게임 화면 스크린샷 3~5장(실제 게임 캡처 기반 — PlayMode 캡처를 써도 됨). 저장: `Reference/store/` (기존 파일은 덮어쓰지 말고 새 이름).
2. 스토어 설명 문구 초안 (한국어): 짧은 소개·긴 설명·주요 특징 5개·검색어 → `Plans/Store_Listing.md` (+ `render_plan.mjs`로 HTML).
3. Undertale 고유 요소(이름·캐릭터·빨간 하트 등)를 쓰지 않았는지 스스로 점검해 기록.

**담당 파일**: `Reference/store/`, `Plans/Store_Listing.md/.html`. MSIX 매니페스트·빌드 스크립트·ProjectSettings는 Claude 담당 — 이미지 파일 경로만 "2. 요청"에 적으면 Claude가 연결한다.

## 2. 요청 (Codex → Claude/사용자)

- (없음 — 생기면 여기에 적는다)

## 3. 한 일

### 2026-10-06 — 3단계 Pattern Editor (브랜치 `phase3-pattern-editor` → main `020df2f`)

**한 일**
- Tools → Pattern Editor: 목록·새 패턴·복제·삭제, 임시 편집 사본, 검증 오류 표시, 저장 후 카탈로그 등록. 맵에서 사용하는 에셋은 삭제를 막는다.
- GraphPatternDefinition: 색·발사원·단발/부채꼴/원형 배열·발사 수·조준 오차·시드·예고·수명·높이·기준 속도·궤적 속성·두 그래프를 ScriptableObject로 저장.
- ITrajectory 구현 8종: 직선·사인파·지그재그·높이 포물선·원호·확장 나선·8자·3D 베지어. 발사 기준 좌표와 경과 시간으로 평가하며 실제 게임과 미리보기가 같은 구현을 사용한다.
- 속도 배율은 수명 비율 0~1을 입력으로 받으며 256개 고정 구간의 적분값을 보간한다. 프레임 분할 수와 독립적인 절대 시간 평가이다.
- PatternTimeline: 예고 때 목표 고정, 시드 재현, 적/좌/우/교대 발사원, 다음 발사 예약 때 간격 그래프를 한 번 평가. 최소 간격과 예고 시간보다 짧게 예약하지 않는다.
- GraphAttackPattern: 기존 Bullet.Launch(trajectory)/Color를 사용, 탄 풀·수명·기존 충돌/쳐내기·예고선·색 표시·ThreatPoint 경고·종료 정리.
- PatternAssetStore: 정의/프리팹/AttackPatternData 묶음 저장, ContentCatalog 등록. 8종 Graph_ 프리셋을 추가했고 Stage Editor에서 Attack_Graph_이름으로 선택 가능하다. 기존 맵 연결은 변경하지 않았다.
- 3D 미리보기: 재생/일시정지/정지·1/60초 진행·시간 스크럽·반복·고정 시드·카메라 각도·허리 시점·경기장/적/목표 좌표, 경계 밖 궤적 경고.

**선택한 기본안과 제한**
- 편집기는 새 Graph 패턴용이다. 기존 YellowTraining/Radial 프리팹은 자동 변환하지 않는다.
- 주파수는 진행 거리 m당 주기이다. 모든 곡선의 호 길이를 일정하게 만드는 시스템은 아니다. 베지어는 수명 끝에 끝점에 도달한다.
- 나선과 8자는 발사 지점 주변을 도는 형태다 (가독성 위해 진폭 2m·주파수 0.2주기/m).
- 경기장/타깃 미리보기는 정적 위치이다. 접근 안전로의 자동 판정은 아직 없다.
- 그래프 입력 범위: 간격 X=0~30초/Y=초, 속도 X=0~1/Y=0~5. 검사는 키 값·유한값과 257개 샘플.

**검증**
- 브랜치: EditMode 143/143, PlayMode 17/17. 4단계와 통합 후 (Claude): EditMode 150/150, PlayMode 20/20 (`Logs/merge34_*.xml`).
- 미리보기 캡처 `Logs/preview_Graph_*.png`, 전투 캡처 `Logs/scene_pattern_editor_sine_runtime.png`.
- 편집 창 마우스 조작은 사용자 확인 대상.

### 2026-10-06 — 이전 작업 (병렬 이전, WORKLOG에 상세)
- 맵 1-6~1-8 추가, 스테이지 선택 목록 배치 수정, 에디터 Play 빈 맵 목록 원인 수정.
- 3D 액션 설계 v1~v3와 MD→HTML 생성기, 액션 1단계(허리 카메라·회피·점프·쳐내기), 2단계(랜덤 조준·사인파·측면·경고음).

## 작업량

중급 Unity 개발자 1명이 같은 결과를 만드는 추정 시간 (AI 실행 시간 아님, 8시간 = 1일). main의 `Plans/Work_Effort.md`에 옮겨진 줄은 ✓. 단계 열 이름: `패턴 누적`, `전투 연출`, `맵툴`, `출시`.

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Codex | 패턴 에디터 | 8종 궤적·그래프 적분·일정·런타임·경고 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 편집 창·3D 미리보기·에셋 CRUD·카탈로그 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 수식/일정/에셋 테스트·전투 회귀·기록 ✓ | 8 |
