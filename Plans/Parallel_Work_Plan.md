# Claude · Codex 병렬 작업 계획 (3·4단계)

작성일: 2026-10-06 · 상태: 3·4단계 완료·main 합침, 5-0 연결 지점 완료, 5단계 진행 · 5~8단계 분할 계획 포함

> **지금 각자 할 일은 따로 정리한 문서를 본다**: Codex → [Codex_Log.md](Parallel/Codex_Log.md) · Claude → [Claude_Log.md](Parallel/Claude_Log.md). 두 문서 모두 "할 일 → 한 일 → 작업량" 순서이고, 기록은 **한국어로만** 쓴다. (`claude.md`는 Windows에서 `CLAUDE.md`와 같은 파일이 되므로 쓰지 않는다.)

HTML 보기: [Parallel_Work_Plan.html](Parallel_Work_Plan.html) · 액션 설계: [Action_Balance_Plan.md](Action_Balance_Plan.md)

## 1. 한눈에 보기

| 누가 | 단계 | 브랜치 | 작업 폴더 |
|---|---|---|---|
| **Codex** | 3단계 Unity Pattern Editor · 궤적 8종 · 발사 간격/속도 그래프 · 3D 미리보기 | `phase3-pattern-editor` | `C:\Unity\Game2Week\Game2Week-codex` |
| **Claude** | 4단계 Ctrl 빨강 정지 자세 · 파랑 실제 이동 판정 · 색 규칙 | `phase4-color-rules` | `C:\Unity\Game2Week\Game2Week-claude` |
| 사용자 | 플레이 확인 · 합치기 요청 · 커밋/푸시 요청 | `main` | `C:\Unity\Game2Week\Game2Week` (원래 폴더) |

- 5단계(누적 패턴·간격·예산, 8개 맵 연결, 밸런스)는 3·4단계 결과가 둘 다 필요하므로 **둘 다 main에 합친 뒤** 한쪽이 맡는다.
- 담당은 사용자 결정(2026-10-06): 처음 제안(Claude 3단계 / Codex 4단계)에서 **서로 바꿈**.

## 2. 왜 0단계가 먼저인가

3단계(탄의 **움직임**)와 4단계(탄이 몸에 닿았을 때의 **판정**)는 둘 다 `Bullet.cs`·`PatternContext`·패턴 코드를 고쳐야 했다. 그래서 둘이 쓸 연결 지점만 main에 먼저 만들었다. 이제 각자 **새 파일 위주**로 작업하고 공용 파일은 건드리지 않는다.

| 연결 지점 (main에 있음) | 파일 | 누가 쓰나 |
|---|---|---|
| `ITrajectory` + `TrajectoryLaunch` — 발사 기준값과 경과 시간으로 위치 계산 (상태 없음) | `Scripts/Battle/Patterns/ITrajectory.cs` | Codex가 구현 (궤적 8종) |
| `Bullet.Launch(position, velocity, trajectory)` — 궤적을 넘기면 그 경로로 이동 | `Scripts/Battle/Patterns/Bullet.cs` | Codex가 호출 |
| `AttackColor` (Yellow/Red/Blue), `Bullet.Color` — 발사 전에 패턴이 지정 | `Scripts/Battle/Patterns/AttackColor.cs` | 둘 다 사용 |
| `IHitRule.ShouldHit(color)` — 몸과 겹친 탄이 실제로 피해를 주는지 | `Scripts/Battle/Patterns/IHitRule.cs` | Claude가 구현 |
| `PatternContext.ShouldHit(color)` — 규칙이 없으면 항상 피해 | `Scripts/Battle/Patterns/IAttackPattern.cs` | Bullet이 호출 |
| `IThreatSource.CollectThreats(List<ThreatPoint>)` — 위치 + 색 | `Scripts/Battle/Patterns/IThreatSource.cs` | Codex 패턴이 제공, Claude 경고 표시가 사용 |
| 쳐내기는 노랑만 (`Bullet.Parryable`) | `Bullet.cs` | 공통 규칙 |

검증: `BulletContractTests` 3개 추가 (궤적 교체, 색별 피해 규칙, 노랑만 쳐내기). 기존 동작은 바뀌지 않음.

## 3. 파일 담당표

**자기 담당이 아닌 파일은 고치지 않는다.** 꼭 필요하면 작업을 멈추고 사용자에게 알린다 (사용자가 상대에게 전달하거나 main에서 작게 고친 뒤 양쪽이 받는다).

### Codex — 3단계

| 구분 | 경로 |
|---|---|
| 새로 만듦 | `Assets/_Project/Editor/Patterns/` (Pattern Editor 창·미리보기·검증) |
| 새로 만듦 | `Assets/_Project/Scripts/Battle/Patterns/Trajectories/` (궤적 8종, 그래프 평가) |
| 새로 만듦 | `Assets/_Project/Scripts/Data/Patterns/` (패턴 정의 ScriptableObject) |
| 기존 파일 | `Scripts/Battle/Patterns/` 중 `YellowTrainingPattern.cs`, `SineBullet.cs`, `WaveTrajectory.cs`, `RadialBurstPattern.cs`, `PatternRunner.cs` |
| 기존 파일 | `Scripts/Data/AttackPatternData.cs`, `Data/Patterns/`, `Prefabs/Battle/Patterns/` (단, `ColorTest/` 하위 폴더 제외) |
| 맵·카탈로그 | `Data/ContentCatalog.asset`, `Editor/Stages/`, `Assets/StreamingAssets/Stages/` (맵툴로만) |
| 일회성 스크립트 | `Editor/ActionPhaseOneSetup.cs`, `Editor/ActionPhaseTwoSetup.cs` — **다시 실행 금지** (Battle 씬 등 Claude 담당 파일을 고침). 정리 여부는 Codex가 결정 |
| 테스트 | `Tests/EditMode/WavePatternTests.cs`, 새 `Tests/EditMode/Trajectory*`·`PatternEditor*`, `Tests/PlayMode/ActionPhaseTwoTests.cs`, 새 `Tests/PlayMode/PatternEditor*` |
| 기록 | `Plans/Parallel/Codex_Log.md` |

### Claude — 4단계

| 구분 | 경로 |
|---|---|
| 입력 | `Input/GameControls.inputactions`, `Input/InputReader.asset`, `Scripts/Core/InputReader.cs` (Ctrl 정지 자세) |
| 플레이어 | `Scripts/Battle/Model/PlayerMotorModel.cs`, `Scripts/Data/PlayerActionSettings.cs`, `Data/PlayerActionSettings.asset` |
| 화면 | `Scripts/Battle/View/PlayerMover.cs`, `PlayerActionView.cs`, `ActionStatusView.cs`, `ThreatFeedbackView.cs`, `BattleWorld.cs` |
| 판정 | `Scripts/Battle/Patterns/Bullet.cs`, `AttackGeometry.cs`, 새 색 규칙 파일 (`Scripts/Battle/Model/`) |
| 조립 | `Scripts/Flow/BattleController.cs`, `Scenes/Battle.unity`, `Scripts/Battle/BattleTexts.cs` |
| 시험 자원 | `Prefabs/Battle/Patterns/ColorTest/`, `Data/Patterns/ColorTest/` (빨강·파랑 시험 패턴 — 맵에는 5단계에 연결) |
| 테스트 | `Tests/EditMode/PlayerActionTests.cs`, `BulletContractTests.cs`, 새 `Tests/EditMode/ColorRule*`, `Tests/PlayMode/ActionPhaseOneTests.cs`, `BattleInputTests.cs`, 새 `Tests/PlayMode/ActionPhaseFour*` |
| 기록 | `Plans/Parallel/Claude_Log.md` |

### 공용 — 병렬 기간에는 고치지 않음

| 파일 | 규칙 |
|---|---|
| `ITrajectory.cs`, `AttackColor.cs`, `IHitRule.cs`, `IThreatSource.cs`, `IAttackPattern.cs` | 연결 지점. 바꿔야 하면 사용자에게 먼저 알림 |
| `CLAUDE.md`, `TODO.md`, `WORKLOG.md`, `TodoList.html`, `Plans/Action_Balance_Plan.md/.html` | **합칠 때 main에서만** 갱신. 작업 중 기록은 각자 로그 파일에 |
| 그 밖의 씬 (`MainMenu`, `StageSelect`, `Result`, `Ending`), `Scripts/Flow/` 나머지, `Scripts/Save/`, `Scripts/UI/` | 이번 범위 밖 — 둘 다 고치지 않음 |
| `ProjectSettings/`, `Packages/manifest.json` | 고치지 않음 (필요하면 사용자에게 알림) |
| `Art/Fonts/Pretendard-Regular SDF.asset` | 플레이·테스트 때마다 자동 변경 → **커밋 전 항상 되돌림** |
| 위 표에 없는 기존 테스트 (`SceneFlowTests`, `BattleStageTests`, `BattleRouteTests` 등) | 고치지 않음. 깨지면 원인을 로그에 적고 사용자에게 알림 |

## 4. 작업 폴더 (git worktree)

같은 프로젝트 폴더에서는 Unity를 두 개 띄울 수 없다(프로젝트 잠금). 그래서 **각자 다른 폴더**에서 작업한다. 원래 폴더는 사용자가 Unity로 열어 확인하는 곳.

```text
C:\Unity\Game2Week\Game2Week          main   — 사용자 (플레이 확인, 합치기)
C:\Unity\Game2Week\Game2Week-codex    phase3-pattern-editor — Codex
C:\Unity\Game2Week\Game2Week-claude   phase4-color-rules    — Claude
```

- 만들기: 원래 폴더에서 `git worktree add ..\Game2Week-codex -b phase3-pattern-editor` (Claude 쪽도 같은 방식).
- 처음 Unity로 열면 `Library/`를 새로 만드느라 오래 걸린다. 원래 폴더의 `Library/`를 복사해 두면 빨라진다 (Unity를 모두 닫은 상태에서).
- 배치모드·테스트는 **자기 폴더에서만** 실행한다 (`-projectPath`를 자기 폴더로).

## 5. 커밋과 합치기

1. 각자 자기 브랜치에 커밋한다. 커밋·푸시는 기존 규칙대로 **사용자가 요청할 때만**.
2. 커밋 전: 컴파일 + EditMode/PlayMode 전체 통과, `git diff --cached --name-only`로 담당 밖 파일이 없는지 확인, 폰트 에셋 되돌리기.
3. 먼저 끝난 쪽부터 사용자가 요청하면 main에 합친다 → main에서 전체 테스트.
4. 나중 쪽은 main을 자기 브랜치로 받아(merge) 전체 테스트 후 합친다. 충돌이 나면 담당표를 기준으로 해결하고, 담당표 위반이면 사용자에게 알린다.
5. 합칠 때 main에서 `TODO.md`·`WORKLOG.md`·`Action_Balance_Plan.md`(+HTML)·`CLAUDE.md`를 로그 내용대로 갱신한다.
6. 둘 다 합쳐지면 worktree를 정리한다 (`git worktree remove`).

기준 테스트 수 (0단계 후 main): EditMode 128 · PlayMode 16.

## 6. 단계별 완료 기준

### Codex — 3단계 Pattern Editor
- `Tools ▸ Pattern Editor` 창: 패턴 목록(생성·복제·삭제), 속성(색·조준 오차·발사 수/배열·예고·수명), 저장/검증 오류 표시.
- 궤적 8종 (직선·사인파·지그재그·포물선·원호·나선·8자·자유 곡선) = `ITrajectory` 구현. 게임과 미리보기가 **같은 구현**을 사용.
- 발사 간격 그래프(패턴 경과 시간 → 초, 발사 예약 순간 한 번 평가, 최소 간격 제한), 탄 속도 배율 그래프(탄 수명 비율 → 배율).
- 3D 미리보기: 재생·정지·한 프레임·시간 스크럽·반복·시드 고정.
- 패턴이 탄의 색을 `Bullet.Color`로, 경고 위치를 `ThreatPoint`로 알린다.
- 맵 연결은 기존처럼 AttackPatternData + ContentCatalog + 맵툴. 기존 1-1~1-3 동작 유지.

### Claude — 4단계 빨강·파랑
- Ctrl 정지 자세: 누르면 이동 명령 억제, 대시/공중 중에는 착지·종료 후 자세로 (판정 건너뛰기 없음). 빨강 = 자세 + 실제 정지일 때만 통과.
- 파랑 = 실제 이동 속도가 기준 이상일 때만 통과 (벽에 막혀 입력만 유지하면 실패).
- `IHitRule` 구현을 `BattleWorld`에서 `PatternContext`에 주입. 쳐내기는 노랑만 유지, 빨강 자세가 만능 방어가 되지 않음.
- 색별 표시(모양·아이콘·문구), 경고 표시에 색 이름, 상태 HUD에 정지 자세 표시.
- 빨강·파랑 시험 패턴은 `ColorTest/`에 두고 테스트로만 검증. 맵 연결과 조합 규칙(모순 조합 금지)은 5단계.

## 7. Codex에게 줄 지시문

아래를 그대로 Codex에 붙여 넣는다.

```text
C:\Unity\Game2Week\Game2Week-codex 폴더(브랜치 phase3-pattern-editor)에서 작업해.
먼저 CLAUDE.md, Plans/Parallel_Work_Plan.md, Plans/Action_Balance_Plan.md(0절·1절·"제작 툴" 절)를 읽어.

할 일: 3단계 Unity Pattern Editor — 궤적 8종(ITrajectory 구현), 발사 간격/속도 그래프,
3D 미리보기(재생·스크럽·시드), 에셋 저장·검증, 맵툴 연결. 게임과 미리보기는 같은 평가 코드를 쓴다.

규칙:
- Parallel_Work_Plan.md 3절의 "Codex" 담당 파일만 고친다. 공용·Claude 담당 파일이 필요하면 멈추고 나에게 알려.
- 연결 지점(ITrajectory, TrajectoryLaunch, Bullet.Launch의 trajectory 인자, Bullet.Color, ThreatPoint)을 사용한다.
- Unity 배치모드·테스트는 -projectPath C:\Unity\Game2Week\Game2Week-codex 로만 실행.
- TODO.md·WORKLOG.md·CLAUDE.md·Action_Balance_Plan·Work_Effort.md는 고치지 말고, 진행 기록은 Plans/Parallel/Codex_Log.md에 쓴다
  (각 기록 끝에 "### 다음에 할 일", 파일 맨 아래 "## 작업량" 표에 끝낸 작업마다
  | 날짜 | Codex | 단계 | 작업 | 사람 기준 시간 | 한 줄 — 중급 Unity 개발자 1명 기준 추정, 8시간=1일).
- 커밋·푸시는 내가 요청할 때만. 커밋 전 EditMode/PlayMode 전체 통과, 폰트 에셋(Pretendard-Regular SDF.asset) 되돌리기.
- ActionPhaseOneSetup/ActionPhaseTwoSetup은 다시 실행하지 마 (Claude 담당 Battle 씬을 고침).
```

## 8. 이후 단계도 같은 방식으로

### 기본 담당 영역

단계마다 새로 정하지 않도록 **영역 단위로 주인을 정해 둔다.** 새 작업은 그 영역 주인이 맡고, 두 영역에 걸치면 아래 "N-0 연결 지점"을 먼저 만든다.

| 영역 | 기본 담당 | 대표 경로 |
|---|---|---|
| 패턴·탄막·궤적·Pattern Editor | Codex | `Scripts/Battle/Patterns/`(Bullet 제외), `Editor/Patterns/`, `Data/Patterns/` |
| 맵·맵툴·ContentCatalog | Codex | `Editor/Stages/`, `StreamingAssets/Stages/`, `Data/ContentCatalog.asset` |
| 전투 이펙트·적 연출 (파티클·말풍선·잔상) | Codex | `View/BattleEffects.cs`, `View/EnemyView.cs`, `Prefabs/FX/`, 새 이펙트 파일 |
| 이미지 생성 (스토어·아이콘·컨셉) | Codex | `Reference/` (새 파일 `_v2` 등) |
| 플레이어·입력·카메라·피격 판정 | Claude | `PlayerMotorModel`, `PlayerMover`, `PlayerActionView`, `InputReader`, `BattleCameraDirector`, `Bullet`, `BattleWorld` |
| 전투 흐름·상태·전투 UI | Claude | `Battle/States/`, `Battle/UI/`, `BattleController`, `Scenes/Battle.unity` |
| 씬 흐름·세이브·설정·공용 UI·오디오 | Claude | `Scripts/Flow/`, `Scripts/Save/`, `Scripts/UI/`, `Scripts/Core/`, `BattleAudio`, 나머지 씬 |
| 캐릭터 모델·리깅 | Claude | `Preview/`, `Art/Characters/` |
| 빌드·출시 | Claude | `Tools/build_windows.ps1`, `Tools/package_msix.ps1`, `Tools/msix/`, `ProjectSettings/` |

### 단계별 분할 계획

| 단계 | 먼저 (main, N-0 연결 지점) | Codex | Claude | 합친 뒤 |
|---|---|---|---|---|
| **5 누적·밸런스** | 회피·쳐내기·색 통과 이벤트를 `BattleEvents`에 추가, 색 조합 검사 순수 함수(`ColorCombinationRules`) | PatternDirector·DifficultyProfile·EncounterData (셔플 백·간격·공유 예산), 8개 맵에 1→8종 연결, 조합 허용 목록 데이터 | 자동 플레이 측정 도구 (봇이 맵별 피격·접근 시간·회피/쳐내기 사용률 기록 → 리포트 HTML), 색 첫 등장 안내 문구·HUD | 측정 리포트로 수치 조정 (데이터는 Codex 담당) → 사용자 플레이 확인 |
| **6 연출·사운드** | `BattleAudio`/이펙트 훅 이벤트 목록 확정 | 피격 파티클 크기·위치, 적 말풍선(월드 UI), 회피 잔상·착지 먼지·쳐내기 효과 | AudioMixer + 음량 설정 연결, BGM·메뉴/전투 효과음 훅, AudioListener 정리, 씬 전환 페이드 | 실제 소리 자원 연결 (출처는 `[결정 필요]`) |
| **7 캐릭터·툴** | 애니메이션 상태 이름 목록 (`PlayerActionView` ↔ Animator) | 맵툴 M5: 3D 미리보기·바로 플레이·일괄 배치 | 주인공 v2 + 리깅 + 애니메이션 (쳐내기 오른손 두 방향·회피·점프·정지 자세) | 맵툴에서 바로 플레이로 새 모델 확인 |
| **8 출시 준비** | 사용자 결정: 회사·게임 이름, 아이콘 방향 | 스토어 이미지·아이콘 세트, 스토어 설명 문구 초안 (`Plans/Store_*`) | 이름 반영 (PlayerSettings·MSIX 매니페스트·UI 문구), 정적 폰트 아틀라스, 빌드·MSIX 재검증 | 사용자: 집에서 설치·삭제·제출 (`Store_Guide.html`) |

- IP 기획(세계관·캐릭터·시스템 명칭)은 사용자와 정하는 일이라 병렬 대상이 아니다. 정해지면 문구는 `BattleTexts`·`UiTexts`(Claude), 적·패턴 이름은 데이터(Codex)로 나눠 반영한다.
- 단계마다 이 계획의 3절 담당표를 그 단계 기준으로 다시 써서 main에 커밋한 뒤 브랜치를 나눈다. 브랜치 이름은 `phaseN-<주제>`.
- 한쪽이 먼저 끝나면 다음 단계의 자기 몫을 시작해도 된다. 단, 다음 단계의 N-0 연결 지점이 main에 없으면 먼저 사용자에게 알린다.

## 9. 작업량 기록 (사람 기준)

- 기준 파일: [Work_Effort.md](Work_Effort.md). 대시보드(`TodoList.html`)의 "작업량 — 사람 기준" 카드가 이 표를 읽어 Claude·Codex 막대, 영역별 표, 최근 작업을 보여 준다.
- **사람 기준 시간** = 중급 Unity 개발자 1명이 같은 결과(코드 + 테스트 + 문서)를 직접 만들 때의 추정 시간, 8시간 = 1일. AI가 실제로 쓴 시간이 아니다.
- 작업을 끝낼 때마다 그 작업을 한 쪽이 한 줄 추가한다: `| 날짜 | 누가 | 단계 | 작업 | 시간 |`. 단계 열은 같은 이름을 재사용한다 (영역별 합계가 묶이도록).
- 병렬 기간에는 각자 로그 파일 맨 아래 "작업량" 표에 같은 형식으로 적고, main에 합칠 때 `Work_Effort.md`로 옮긴다 (공용 파일이라 병렬 중 직접 고치지 않음).

## 다음에 할 일

- 사용자: 계획 7절 지시문을 Codex에 전달 (worktree 두 개는 만들어 둠).
- Claude: `Game2Week-claude`(브랜치 `phase4-color-rules`)에서 4단계 시작, 기록은 `Plans/Parallel/Claude_Log.md`.
