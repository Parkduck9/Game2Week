# Codex 작업 문서 — 할 일 · 한 일

최종 갱신: 2026-10-06 (Claude가 5단계 지시 작성) · 지금 할 일: **5단계 — 패턴 누적·발사 간격·예산, 8개 맵 연결**

> Codex는 이 문서의 "지금 할 일"부터 바로 시작한다. 끝나면 "한 일"과 "작업량"을 이 문서에 직접 추가한다.
> 짝 문서: Claude 쪽은 `Plans/Parallel/Claude_Log.md`. 전체 규칙은 `CLAUDE.md`와 `Plans/Parallel_Work_Plan.md`.

## 0. 작성 규칙 (꼭 지킬 것)

- **모든 기록·주석·커밋 메시지·답변은 한국어로 쓴다.** 영어로 바꾸지 않는다. 코드 식별자(클래스·변수 이름)만 영어.
- 작업 폴더는 `C:\Unity\Game2Week\Game2Week-codex` 하나. Unity 배치모드·테스트도 `-projectPath`를 이 폴더로만 준다.
  - 에디터: `C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe`
  - 테스트: `-runTests -testPlatform EditMode|PlayMode -assemblyNames Game2Week.Tests.EditMode|Game2Week.Tests.PlayMode` (`-quit` 없음)
- **담당 파일만 고친다** (아래 각 단계 표). 공용 파일이나 Claude 담당 파일을 고쳐야 하면 고치지 말고 이 문서의 "요청" 칸에 적는다.
- `TODO.md`·`WORKLOG.md`·`CLAUDE.md`·`Plans/Action_Balance_Plan.md`·`Plans/Work_Effort.md`는 고치지 않는다 (Claude가 main에 합칠 때 이 문서를 보고 반영).
- git commit·push·merge는 하지 않는다. 사용자가 요청하면 Claude가 한다.
- 끝내기 전에 `Assets/_Project/Art/Fonts/Pretendard-Regular SDF.asset`과 `ProjectSettings/` 자동 변경을 `git checkout`으로 되돌린다.
- `node`는 PATH에 없다: `C:\Program Files\Adobe\Adobe Creative Cloud Experience\libs\node.exe`. 이 문서의 HTML은 `node Tools/render_plan.mjs Plans/Parallel/Codex_Log.md`로 다시 만든다.
- 끝낼 때: EditMode·PlayMode 전체 통과 (결과 xml 경로 기록) → "한 일"에 한 일·결정·검증·요청·다음에 할 일 → "작업량"에 줄 추가 → HTML 재생성.

## 1. 지금 할 일 — 5단계 (브랜치 `phase5-director`)

### 시작 전 상태
- main에 3단계(Pattern Editor)·4단계(빨강·파랑 판정)·**5-0 연결 지점**이 합쳐져 있다. Codex 폴더는 이미 `phase5-director` 브랜치로 맞춰 둔다 (Claude가 준비).
- 5-0 연결 지점 (공용 — 사용만, 수정 금지):
  - `ColorCombinationRules` — `CanOverlap(a, b)`, `IsAllowed(colors)`, `SwitchGraceSeconds` (0.6초). 빨강·파랑은 동시에 위험하면 안 된다.
  - `EncounterMemory` + `PatternContext.Memory` — 전투 한 번 동안 유지되는 기록. `context.Memory.GetOrCreate<T>("키")`로 소개 여부·셔플 백을 턴 사이에 유지. BattleWorld가 전투마다 하나 만들어 넣는다.

### 만들 것
1. **DifficultyProfile** (ScriptableObject): 단계별 기준 발사 간격(1.40 → 0.65초), 탄속 배율, 동시 위험 상한(1~4단계 1, 5단계부터 2), 공유 예산(초당 발사 수·살아 있는 탄 수).
2. **PatternEncounterData** (ScriptableObject): 이 맵에서 새로 배우는 패턴 1개, 이미 배운 패턴 목록, 허용 조합 목록, 사용할 DifficultyProfile.
3. **PatternDirector** (IAttackPattern을 구현한 루트 프리팹 — 하위 패턴들을 관리):
   - 새 패턴은 그 맵 첫 턴에 단독으로 먼저 보여 준다. 소개 전에 턴이 끝나면 다음 턴에 다시 먼저 (`context.Memory`에 기록).
   - 이후 셔플 백으로 배운 패턴을 순환 (같은 패턴 연속 금지, 시드 재현).
   - 반복 간격 = `max(최소 간격, 기본 간격 × 단계 기준 간격 / 1.40)` — 배율은 한 번만 적용.
   - 동시 실행 상한·공유 예산 지키기. 예산이 모자라면 묶음 전체를 미룬다 (한 프레임에 몰아 쏘지 않음).
   - **색 조합**: `ColorCombinationRules`로 빨강·파랑이 동시에 살아 있지 않게, 바뀔 때 `SwitchGraceSeconds` 비우기.
   - `End()`에서 하위 패턴·예고·예약·탄 모두 정리.
4. **8개 맵 연결** (맵툴 `StageEditorModel`로만 저장 — JSON 직접 수정 금지). 맵마다 새 패턴 1종, 누적 1 → 8종. **기본안** (바꾸면 이유를 기록):

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

   - 새 색과 새 궤적을 같은 맵에서 동시에 소개하지 않는다. 나선·베지어는 툴 프리셋으로 남겨도 된다.
   - 맵 크기·배치·보석·턴 시간은 유지. 1-1은 단일 패턴 그대로 (Director 없이도 됨).
5. **테스트**: Director (새 패턴 우선 소개·턴 넘어 유지·셔플 백 연속 금지·간격 단조 감소·예산·색 겹침 금지·End 정리), 8개 맵 데이터·패턴 연결 검증, 전투 회귀.

### 5단계 담당 파일
| 구분 | 경로 |
|---|---|
| 새로 만듦 | `Scripts/Battle/Patterns/Director/`, `Scripts/Data/Patterns/` (DifficultyProfile·PatternEncounterData), `Data/Patterns/Director/`, `Prefabs/Battle/Patterns/Director/` |
| 기존 | 패턴 쪽 전부 (`Scripts/Battle/Patterns/` 중 Bullet·공용 연결 지점 제외), `Editor/Patterns/`, `Editor/Stages/`, `Data/ContentCatalog.asset`, `StreamingAssets/Stages/` (맵툴로만) |
| 테스트 | 새 `Tests/EditMode/PatternDirector*`, `Tests/PlayMode/PatternDirector*`, 기존 `WavePatternTests`·`TrajectoryGraphTests`·`PatternEditor*`·`ActionPhaseTwoTests`, **이번 단계에 한해 `BattleStageTests`** (맵 패턴이 바뀌면 단언 수정 허용) |
| 공용 — 수정 금지 | `ColorCombinationRules`, `EncounterMemory`, `IAttackPattern`(PatternContext), `ITrajectory`, `AttackColor`, `IHitRule`, `IThreatSource`, `Bullet` |
| Claude 담당 — 수정 금지 | 플레이어·입력·카메라·`BattleWorld`·`BattleController`·전투 UI·씬·`BattleTexts`·`Tests/PlayMode/Balance*`·`ActionPhaseFourTests` 등 (`Parallel_Work_Plan.md` 8절) |

### 같은 시간 Claude가 하는 일 (참고)
- 자동 플레이 측정 도구 (맵별 피격·접근 시간·회피/쳐내기/자세 사용 리포트), 색 처음 등장 안내 문구.
- 합친 뒤 측정 리포트를 보고 수치를 조정한다 — 데이터 에셋은 Codex 담당이라 그때 Codex에 요청이 간다.

### 요청 (Codex → Claude/사용자)
- (없음 — 생기면 여기에 적는다)

## 2. 이후 할 일 (각 단계는 Claude가 main에 N-0 연결 지점을 만든 뒤 이 문서에 자세히 적어 준다)

| 단계 | Codex 할 일 |
|---|---|
| 6 연출·사운드 | 피격 파티클 크기·위치, 적 말풍선(월드 UI), 회피 잔상·착지 먼지·쳐내기 효과 |
| 7 캐릭터·툴 | 맵툴 M5: 3D 미리보기·바로 플레이·일괄 배치 |
| 8 출시 준비 | 스토어 이미지·아이콘 세트, 스토어 설명 문구 초안 (회사·게임 이름은 사용자 결정 후) |

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

중급 Unity 개발자 1명이 같은 결과를 만드는 추정 시간 (AI 실행 시간 아님, 8시간 = 1일). main의 `Plans/Work_Effort.md`에 옮겨진 줄은 ✓.

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Codex | 패턴 에디터 | 8종 궤적·그래프 적분·일정·런타임·경고 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 편집 창·3D 미리보기·에셋 CRUD·카탈로그 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 수식/일정/에셋 테스트·전투 회귀·기록 ✓ | 8 |
