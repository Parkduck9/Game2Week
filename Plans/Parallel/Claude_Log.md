# Claude 작업 문서 — 할 일 · 한 일

최종 갱신: 2026-10-06 · 상태: **5·6단계 완료** (브랜치 `phase5-claude` 커밋) → Codex 7·8단계 완료 후 합치기 대기

> 사용자 결정 (2026-10-06): **5·6단계 = Claude, 7·8단계 = Codex.** 동시 진행은 5 ↔ 7, 그다음 6 ↔ 8.
> Claude는 "1. 할 일"을 위에서부터 진행하고, 단계마다 "3. 한 일"과 "작업량"을 추가한다.
> 짝 문서: `Plans/Parallel/Codex_Log.md`. 전체 규칙은 `CLAUDE.md`와 `Plans/Parallel_Work_Plan.md`.

## 0. 작성·작업 규칙

- 기록·주석·커밋 메시지·사용자 답변은 **한국어**로 쓴다. 코드 식별자만 영어.
- 작업 폴더 `C:\Unity\Game2Week\Game2Week-claude`, 브랜치 **`phase5-claude`** 하나로 5·6단계 진행. Unity 배치모드·테스트는 이 폴더로만.
- **Codex 7·8단계 담당 파일은 고치지 않는다**: `Preview/`, `Reference/`, `Exports/`, `Art/Characters/`, `Prefabs/Player_Heroine*`, `PlayerActionView.cs`, `Scripts/Battle/View/Animation/`, `Editor/Stages/`(코드), `ProductInfo`, `Scripts/UI/UiTexts.cs`, `Art/Fonts/`, TMP 설정, `ProjectSettings/`, `Editor/Build/`, `Tools/build_windows.ps1`·`package_msix.ps1`·`msix/`, `Plans/Store_*`.
  - 맵 연결은 `StageEditorModel`을 **사용만** 해서 `StreamingAssets/Stages/`에 저장한다 (맵툴 코드는 Codex가 M5로 고치는 중).
- 병렬 기간에는 TODO·WORKLOG·설계 MD·Work_Effort를 고치지 않고 이 문서에 기록 → main에 합칠 때 반영 (Codex 문서도 함께).
- 커밋: 단계가 끝나고 전체 테스트가 통과하면 자기 브랜치에 커밋 (사용자 "한 번에 작업" 요청). push·main 합치기는 사용자 요청 시. 커밋 전 폰트 에셋·ProjectSettings 자동 변경 되돌리기.
- HTML: `node Tools/render_plan.mjs Plans/Parallel/Claude_Log.md`.

## 1. 할 일 (순서대로)

### 5단계 — 패턴 누적·발사 간격·예산, 8개 맵 연결, 측정·색 안내

**연결 지점 (main에 있음)**: `ColorCombinationRules`(빨강·파랑 동시 위험 금지, 전환 유예 0.6초), `EncounterMemory` + `PatternContext.Memory`(전투 동안 유지되는 패턴 기록).

1. **색 처음 등장 안내** — 완료: `ColorGuideModel`(순수 로직, 색당 한 번) + `ColorGuideView`(Battle 씬 상단 문구, 3초), `BattleTexts.RedGuide/BlueGuide`.
2. **DifficultyProfile** (ScriptableObject): 단계별 기준 발사 간격(1.40 → 0.65초), 탄속 배율, 동시 위험 상한(1~4단계 1, 5단계부터 2), 공유 예산(초당 발사 수·살아 있는 탄 수).
3. **PatternEncounterData** (ScriptableObject): 이 맵에서 새로 배우는 패턴 1개, 이미 배운 패턴 목록, 허용 조합, 사용할 DifficultyProfile.
4. **PatternDirector** (IAttackPattern + IThreatSource 루트 프리팹):
   - 새 패턴은 그 맵 첫 턴에 단독으로 먼저. 소개 전에 턴이 끝나면 다음 턴에 다시 먼저 (`context.Memory`).
   - 셔플 백으로 배운 패턴 순환 (연속 금지, 시드 재현).
   - 반복 간격 = `max(최소 간격, 기본 간격 × 기준 간격 / 1.40)` — 배율은 한 번만.
   - 동시 상한·공유 예산 (모자라면 묶음 전체를 미룸). `ColorCombinationRules`로 빨강·파랑 겹침 금지 + 전환 유예.
   - 하위 패턴들의 위험 위치를 모아 `CollectThreats`, `End()`에서 모두 정리.
5. **8개 맵 연결** (기본안):

   | 맵 | 새로 배우는 패턴 | 누적 | 기준 간격 | 동시 상한 |
   |---|---|---:|---:|---:|
   | 1-1 | 노랑 직선 (기존 유지) | 1 | 1.40 | 1 |
   | 1-2 | 노랑 사인파 | 2 | 1.25 | 1 |
   | 1-3 | 노랑 측면 교대 / 지그재그 | 3 | 1.10 | 1 |
   | 1-4 | **빨강** 직선 (새 색은 익힌 궤적으로만) | 4 | 1.00 | 1 |
   | 1-5 | 노랑 포물선 (점프로 넘기) | 5 | 0.90 | 2 |
   | 1-6 | **파랑** 사인파 | 6 | 0.80 | 2 |
   | 1-7 | 원호 (빨강·파랑 교대 — 겹침 금지) | 7 | 0.70 | 2 |
   | 1-8 | 8자 | 8 | 0.65 | 2 |

6. **자동 플레이 측정 도구** (PlayMode 봇): 8개 맵을 자동 플레이 — 노랑은 쳐내기·회피, 빨강은 Ctrl 정지, 파랑은 계속 이동. 맵별 피격·접근 성공률·소요 시간·회피/쳐내기/자세 사용·색 통과·턴 수를 `BattleFeedback`으로 세어 `Logs/balance_report.json` → `Tools/render_balance_report.mjs` → `Plans/Balance_Report.html`. 전체 측정은 따로 실행 (`[Explicit]`), 일반 테스트에는 1개 맵 짧은 확인만.
7. 측정 결과로 간격·예산 1차 조정. 최종 밸런스는 사용자 플레이 확인 후.
8. 테스트: 색 안내(한 번만), Director(우선 소개·턴 간 유지·연속 금지·간격 단조 감소·예산·색 겹침 금지·End 정리), 8개 맵 데이터, 리포트 생성, 전투 회귀.

### 6단계 — 전투 연출 이펙트 · 소리 · 씬 전환

**연결 지점 (main에 있음)**: `BattleFeedback`(회피·점프·착지·쳐내기·자세·피격·색 통과 Raise 연결됨, 발사·예고는 패턴에서 Raise 필요), `BattleEvents.EnemySpoke`, `BattleFxRig`(Battle 씬 연결됨 — 이펙트는 이 프리팹 아래에).

1. 패턴이 발사·예고 때 `context.Feedback.RaiseBulletFired / RaiseWarningStarted` (Graph·YellowTraining·Director).
2. **적 말풍선** (월드 공간 UI): `EnemySpoke` → 적 머리 위, 탄막 턴이 끝나면 숨김.
3. 피격 파티클 크기·위치 정리 (클로즈업에서 너무 큰 큐브), 주인공 동작 효과 (회피 잔상·점프/착지 먼지·쳐내기 섬광·자세 완성 고리·색 통과 반짝임·피격 번쩍임), 발사·예고 효과 (색별 + 모양 차이). 풀링, 일시정지 정지, 종료 정리.
4. **AudioMixer** (`Art/Audio/GameMixer`): BGM·SFX 그룹, 설정 음량(배경음/효과음) → dB 연결.
5. **BattleAudio 확장**: `BattleFeedback`·`BattleEvents` 구독 (클립 비면 무음), 경고음도 SFX 그룹. **BGM 재생기** (씬별 슬롯, 교차 페이드), **메뉴 효과음** (`MenuNavigator`·`ConfirmPopup`). 클립은 비워 두고 연결 지점만 (소리 출처는 사용자 결정).
6. **AudioListener** 씬마다 하나, **씬 전환 페이드** (`SceneLoader`, 테스트에서 끌 수 있게).
7. 테스트: FxRig 구독·이펙트 생성·풀 재사용·정리, 말풍선, 음량 → dB, 리스너 1개, 페이드 후 전환 완료, 흐름 회귀.

### 합칠 때 (사용자 요청 시)
- 두 브랜치를 main에 합친다 (먼저 끝난 쪽부터, 나중 쪽은 main을 받아 전체 테스트) → 두 문서 내용을 TODO·WORKLOG·설계 MD·Work_Effort에 반영.

## 2. 요청 (Claude → Codex/사용자)
- 사용자 결정 필요: 회사·게임 이름 (8단계, Codex는 자리표시자로 진행), 소리 출처 (6단계 클립 — 연결 지점은 완료).
- **사용자 확인 필요 (5단계 측정)**: 직진하면 2~4초 안에 적에게 닿아 패턴을 거의 못 보고 턴이 끝난다 — 접근 난이도를 올릴지 (`Plans/Balance_Report.html`).
- 선택: AudioMixer를 에디터에서 만들어 `Data/Audio/AudioRouting`에 연결 (6단계 한 일 참고).

## 3. 한 일

### 2026-10-06 — 6단계 완료 (브랜치 `phase5-claude`)

**한 일**
- **발사·예고 알림**: YellowTraining·Graph·Radial 패턴이 `BulletFired`·`WarningStarted`를 Raise. YellowTraining 예고선도 공격 색으로 (4단계에서 남긴 항목 해결).
- **이펙트 모듈** (`Scripts/Battle/View/Fx/`, `BattleFxRig` 프리팹 아래 — Battle 씬 수정 없음): `IBattleFxModule`, `FxPool`(재사용).
  - `ActionFxModule`: 회피 잔광, 점프·착지 먼지, 쳐내기 불꽃, 정지 자세 완성 빨강 고리, 색 통과(빨강은 멈춘 알갱이 / 파랑은 빠르게 흩어짐), 피격 터짐, 발사 연기, 예고 고리.
  - 일시정지에서 멈추고, 탄막 턴이 끝나면 지운다.
- **적 말풍선** `EnemySpeechBubble`: `EnemySpoke` → 적 머리 위 월드 글자(카메라를 향함), 3초 또는 턴 종료 시 숨김.
- **피격 파티클 정리**: 크기 ×0.5, 적 중심에서 카메라 쪽으로 0.35m (클로즈업에서 화면을 덮던 문제).
- **소리 연결 지점** (클립은 비어 있음, 소리 출처는 사용자 결정):
  - `AudioRouting`: 믹서가 있으면 BGM·SFX 그룹 + 음량을 dB로, 없으면 소스 음량 = 설정값.
  - `BattleAudio`: 기존 5종 + 회피·점프·착지·쳐내기·자세·빨강/파랑 통과·발사·예고, `BattleFeedback` 구독.
  - `UiSoundSet` + `MenuNavigator`: 이동·확인·거부·취소. 씬 4곳·ConfirmPopup의 메뉴 전부 연결.
  - `SceneBgm`: 씬마다, 시작할 때 서서히 커지고 전환할 때 줄어듦, 설정 음량을 계속 따름.
- **AudioListener** 씬마다 1개 (PlayMode 경고 해결).
- **씬 전환 페이드**: `SceneLoader`가 검은 화면 0.25초 → 로드 → 0.25초. 실제 시간 기준이라 일시정지 중에도 동작하고, `Leaving` 알림에 맞춰 배경음도 같이 줄어든다.

**결정·제한**
- Unity는 스크립트로 AudioMixer 파일을 만들 수 없다. 그래서 연결 지점(`AudioRouting`)만 만들었다.
  - 사용자 할 일 (선택): Create ▸ Audio Mixer → BGM·SFX 그룹 → 그룹 Volume을 "BgmVolume"·"SfxVolume"으로 노출 → `Data/Audio/AudioRouting`에 연결. 연결하지 않아도 음량 설정은 동작한다.
- 이펙트는 임시 사각 알갱이·선 고리다. 리소스가 생기면 `ActionFxModule` 생성부만 프리팹으로 바꾼다.
- 작업 중 함정: 연결 스크립트에서 씬을 연 뒤 앞에서 잡아 둔 에셋 참조가 끊겨 값이 조용히 비었다. 씬마다 에셋을 다시 읽어 해결했고, EditMode 테스트가 씬 5개의 리스너·배경음·메뉴 효과음 연결을 검사한다.

**검증**
- 작업 폴더에서 EditMode 161/161, PlayMode 27 통과 + 1 건너뜀(`MeasureAllStages`는 `-runBalance` 전용) (`Logs/phase6_*.xml`).
- 추가된 테스트:
  - `Stage6Tests`: dB 변환, 믹서 없을 때 음량, `FxPool` 재사용, 씬 5개 연결.
  - `Stage6RuntimeTests`: 말풍선 표시·숨김, 예고·발사·회피 이펙트와 효과음 알림, 턴 종료 정리, 메인 ↔ 선택 페이드.
- 캡처 `Logs/scene_stage6_battle_fx.png`: 말풍선 "간다!", 회피 잔광.

### 2026-10-06 — 5단계 완료 (브랜치 `phase5-claude`)

**한 일**
- **PatternDirector** (`Scripts/Battle/Patterns/Director/`): 순수 로직 `DirectorPlanner` + 실행 컴포넌트.
  - 턴마다 패턴 하나를 고른다. 새 패턴(0번)은 2초 이상 보여 주기 전까지 매 턴 단독으로 먼저 나온다.
  - 그 뒤로는 셔플 백(연속 금지, 시드 재현)으로 순환한다.
  - 동시 상한 2인 단계에서는 색이 겹쳐도 되는 두 번째 패턴을 1.6초 뒤 겹친다. 빨강+파랑은 금지하고, 살아 있는 위험이 상한 이상이면 미룬다.
  - 하위 패턴의 위험 위치를 모아 경고음·화면 밖 표시·측정 봇에 넘기고, 턴이 끝나면 하위 패턴을 모두 정리한다.
- **DifficultyProfile·PatternEncounterData** (`Scripts/Data/Patterns/`) + **IDirectablePattern**.
  - YellowTraining·Graph·Radial 패턴이 난이도 배율(간격 × 기준/1.40, 탄속 ×)을 Begin 전에 한 번만 받는다.
  - Graph 패턴은 정의 사본에만 적용한다.
- **8개 맵 연결** (`StageEditorModel`로 저장, 검증 오류 0): 1-1은 `Pattern_YellowTraining` 단독, 1-2~1-8은 `Attack_Stage_1-n`(Director). 새 패턴 순서:
  - 노랑 사인파 → 측면 교대 → **빨강** 직선 → 노랑 포물선 → **파랑** 사인파(새로 만듦) → 원호 → 8자 (1 → 8종 누적).
  - 기준 간격 1.40 → 0.65초, 탄속 ×1.00 → ×1.33, 동시 상한 1-5부터 2, 위험 상한 6 → 16.
- **색 처음 등장 안내**: `ColorGuideModel` + `ColorGuideView`(Battle 씬 상단, 3초).
  - "빨강 공격! Ctrl을 누르고 멈추면 통과한다" / "파랑 공격! 계속 움직이면 통과한다", 전투마다 색당 한 번.
- **자동 플레이 측정 도구** `BalanceMeasurementTests`:
  - 실제 키 입력 봇이 적에게 직진하면서 대응한다. 앞에서 오는 빨강은 Ctrl, 가까운 노랑은 쳐내기·옆 회피, 메뉴에서는 공격.
  - `BattleFeedback`으로 사건을 세서 `Logs/balance_report.json`에 쓴다 → `Tools/render_balance_report.mjs` → `Plans/Balance_Report.html`.
  - 전체 측정은 `-runBalance` 명령줄일 때만 돈다. 일반 실행은 1-1 짧은 확인만.
- **버그 수정 (측정 봇이 찾음)**: Ctrl을 누른 채 메뉴로 가서 떼면 정지 자세가 안 풀려 다음 턴·다음 맵에서 움직일 수 없었다.
  - 원인: 입력 맵이 꺼진 동안 뗀 것을 `IsPressed()`가 모름.
  - `InputReader`가 누름·뗌 이벤트로 직접 기억하고, 입력 모드가 바뀔 때 지우게 했다 (실제로 누르고 있으면 다시 들어옴). 회귀 테스트 추가.
- 기존 테스트 조정: `BattleStageTests`·`ActionPhaseTwoTests`에서 1-2·1-3 패턴을 Director 아래 하위 패턴으로 찾도록.

**측정 결과** (`Plans/Balance_Report.html`): 봇이 8개 맵을 모두 2~3턴 만에 처치했다. 맵마다 피격 0~3, 적 접근 평균 1.5~3.8초.
- **사용자 확인 필요**: 직진하면 2~4초 안에 적에게 닿아서, 패턴을 몇 발 보지 못하고 턴이 끝난다. 접근을 더 어렵게 할지 결정이 필요하다 (예: 적 주변 접근 시간·첫 발사 시점·맵 크기·턴 구조). 봇은 단순 직진형이라 사람 체감과 다를 수 있다.

**검증**
- 작업 폴더에서 EditMode 157/157, PlayMode 26/26 (`Logs/phase5_*.xml`, 8개 맵 측정 포함).
- 추가된 테스트:
  - `PatternDirectorTests` 5개: 새 패턴 우선, 셔플 백, 색 겹침 금지, 8개 맵 누적·간격 감소, 색 안내.
  - `PatternDirectorRuntimeTests` 2개: 1-5 단독 소개 → 기록 유지 → 두 패턴 겹침 → 정리, 1-4 빨강 안내 한 번.
  - `BalanceMeasurementTests`, Ctrl 고착 회귀 테스트.
- 캡처: `Logs/scene_director_stage5_overlap.png`, `scene_director_red_guide.png`.

### 2026-10-06 — 6-0 연결 지점 (main)
- `BattleFeedback`: 월드 사건 알림 창구 — 회피·점프·착지·쳐내기(쪽 방향)·정지 자세 완성·피격·색 통과(같은 색 0.25초에 한 번)·발사·예고. `BattleWorld.Feedback`, `PatternContext.Feedback`.
  - 주인공 쪽 Raise 연결 완료: `PlayerMover`(회피·점프·착지·자세·쳐내기), `BattleWorld`(실제 피격), `PlayerHitRule`(색 통과). 발사·예고 Raise는 6단계.
- `BattleEvents.EnemySpoke`: 탄막 턴 시작 때 적 대사 (말풍선용).
- `BattleFxRig` + 프리팹 `Prefabs/Battle/FX/BattleFxRig.prefab`: Battle 씬의 `BattleController.fxRigPrefab`에 연결 → 만들어서 `Bind(events, feedback, spawner)`. 이펙트를 이 프리팹 아래에 붙이면 씬을 안 고쳐도 됨.
- `FeedbackContractTests` (PlayMode): FxRig 연결, 적 대사·회피·점프·착지 알림.
- 7·8단계(Codex)와 5·6단계(Claude)는 위 담당 구분으로 파일이 겹치지 않음.

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
- 손·자세 표현은 리깅 전 임시 큐 (7단계 Codex가 Animator로 대체).

### 2026-10-06 — 병렬 준비
- 병렬 작업 계획 (`Plans/Parallel_Work_Plan.md`): 파일 담당표·worktree·합치기 규칙·5~8단계 분할.
- 0단계 연결 지점: `ITrajectory`·`AttackColor`·`IHitRule`·`ThreatPoint`.
- 작업 폴더 두 개, 사람 기준 작업량 표시 (`Plans/Work_Effort.md` + 대시보드 카드).

### 2026-10-06 — 이전 작업 (병렬 이전, WORKLOG에 상세)
- 기획·문서, 주인공 모델 v1, Unity 프로젝트 셋업, 전투 상태 머신·데이터·UI, 맵툴 M1~M3, 서비스 흐름 S1~S6(세이브·설정·스테이지 선택·일시정지·결과/엔딩·빌드/MSIX), 타이밍 공격·방사형 탄막, 세 결말 검증, 키보드 포커스 버그, 맵 1-3~1-5.

## 작업량

중급 Unity 개발자 1명 기준 추정 시간 (8시간 = 1일). main의 `Plans/Work_Effort.md`에 옮겨진 줄은 ✓. 단계 열 이름: `패턴 누적`, `전투 연출`.

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Claude | 액션 4단계 | Ctrl 정지 자세 입력·이동 모델, 실제 속도, 빨강/파랑 색 규칙 + 주입, 색 표시·HUD, 시험 패턴, EditMode 7·PlayMode 3 ✓ | 16 |
| 2026-10-06 | Claude | 병렬 준비 | 3단계 통합 테스트·합치기, 5-0 연결 지점(색 조합 규칙·전투 기록) + 테스트, 할 일 문서 정리 ✓ | 4 |
| 2026-10-06 | Claude | 병렬 준비 | 6-0 연결 지점(BattleFeedback·EnemySpoke·BattleFxRig 씬 연결) + 테스트, 5~8단계 지시 작성 ✓ | 5 |
| 2026-10-06 | Claude | 패턴 누적 | PatternDirector(선택·겹침·색 규칙·예산)·난이도 프로필·만남 데이터, 패턴 3종 난이도 배율, 8개 맵 1→8종 연결, 파랑 사인파 | 14 |
| 2026-10-06 | Claude | 패턴 누적 | 색 처음 등장 안내, 자동 플레이 측정 봇 + 리포트 HTML, Ctrl 고착 버그 수정, 테스트 9개 | 10 |
| 2026-10-06 | Claude | 전투 연출 | 발사·예고 알림, 이펙트 모듈 8종 + 풀, 적 말풍선, 피격 파티클 정리 | 10 |
| 2026-10-06 | Claude | 전투 연출 | 소리 연결 지점(AudioRouting·BattleAudio 13종·메뉴 효과음·씬 배경음), 리스너 정리, 씬 전환 페이드, 테스트 6개 | 10 |
