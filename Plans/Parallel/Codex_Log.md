# Codex 작업 문서 — 할 일 · 한 일

최종 갱신: 2026-10-06 (Claude 작성) · 지금 할 일: **7단계 → 8단계를 순서대로 한 번에** (같은 시간에 Claude는 5·6단계)

> 사용자 결정 (2026-10-06): **5·6단계 = Claude, 7·8단계 = Codex.** 이 문서의 이전 판에 있던 5·6단계 지시는 취소됐다 — 하지 않는다.
> Codex는 "1. 할 일"을 7단계부터 순서대로 끝까지 진행한다. 단계 하나가 끝날 때마다 "3. 한 일"과 "작업량"을 추가하고 다음 단계로 넘어간다.
> 짝 문서: `Plans/Parallel/Claude_Log.md`. 전체 규칙은 `CLAUDE.md`와 `Plans/Parallel_Work_Plan.md`.

## 0. 작성·작업 규칙 (꼭 지킬 것)

- **모든 기록·주석·커밋 메시지·답변은 한국어로 쓴다.** 영어로 바꾸지 않는다. 코드 식별자(클래스·변수 이름)만 영어.
- 작업 폴더 `C:\Unity\Game2Week\Game2Week-codex`, 브랜치 **`phase7-codex`** 하나로 7·8단계를 진행한다.
  - Unity 배치모드·테스트는 `-projectPath`를 이 폴더로만. 에디터: `C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe`
  - 테스트: `-runTests -testPlatform EditMode|PlayMode -assemblyNames Game2Week.Tests.EditMode|Game2Week.Tests.PlayMode` (`-quit` 없음)
- **아래 각 단계의 "담당 파일"만 고친다. 그 밖의 파일은 전부 수정 금지** (Claude가 5·6단계에서 전투·패턴·맵·UI·소리·씬을 고치는 중). 꼭 필요하면 고치지 말고 "2. 요청"에 적고 우회한다.
- 특히 **씬 파일(`Scenes/*.unity`)은 고치지 않는다.** 스테이지 JSON(`StreamingAssets/Stages/`)도 고치지 않는다 (5단계 맵 연결 중).
- `TODO.md`·`WORKLOG.md`·`CLAUDE.md`·`Plans/Action_Balance_Plan.md`·`Plans/Work_Effort.md`·`Plans/Parallel_Work_Plan.md`·`Plans/Parallel/Claude_Log.md`는 고치지 않는다 (Claude가 합칠 때 이 문서를 보고 반영).
- **커밋**: 사용자 "한 번에 작업" 요청 → 단계 하나가 끝나고 전체 테스트가 통과하면 이 브랜치에 커밋 (예: `7단계 (Codex): ...`). push·main 합치기는 하지 않는다.
- 커밋 전: `Assets/_Project/Art/Fonts/Pretendard-Regular SDF.asset`(8단계에서 정적 아틀라스로 바꾸기 전까지)과 `ProjectSettings/` 자동 변경을 되돌리고, `git diff --cached --name-only`로 담당 밖 파일이 없는지 확인.
- 임시 에디터 스크립트는 `Assets/Editor/`에 두고 실행 후 폴더째 삭제.
- `node`는 PATH에 없다: `C:\Program Files\Adobe\Adobe Creative Cloud Experience\libs\node.exe`. 이 문서의 HTML: `node Tools/render_plan.mjs Plans/Parallel/Codex_Log.md`.
- 단계마다: 전체 테스트 통과 (결과 xml 경로 기록) → "3. 한 일" → "작업량" → HTML 재생성 → 커밋 → 다음 단계.
- 막히면 (사용자 결정 필요·담당 밖 파일 필요) 그 항목만 "2. 요청"에 적고 나머지를 계속 진행한다. 질문하려고 멈추지 않는다.

## 1. 할 일 (순서대로)

### 7단계 — 주인공 v2 · 리깅 · 애니메이션 + 맵툴 2차 (M5)

**A. 주인공 v2·리깅·애니메이션**
1. 모델 v2 (`Preview/heroine_preview.html`의 three.js 코드 모델을 고친다, v1은 남겨 둠): 손 추가 (쳐내기용 오른손이 보이게), 측면에서 눈·볼이 뜨는 문제, 머리카락 결 보강. 설정: 노란 양갈래·노란 눈·연하늘 후드티·청바지·2.5등신·로우폴리 단색면. 레퍼런스 `Reference/heroine_turnaround.png` (덮어쓰지 말고 새 파일은 `_v2`).
2. 리깅: three.js `SkinnedMesh` + 뼈 (골반·척추·머리·양팔(위팔·아래팔·손)·양다리·양갈래 2마디) → GLB 내보내기 (`Exports/` → `Assets/_Project/Art/Characters/Heroine/heroine_v2.glb`).
3. 애니메이션 클립 (GLB에 포함): 대기·달리기·회피·점프·착지·쳐내기 2종(오른손 왼→오 / 오→왼, 반대쪽 어깨 뒤로 흘리는 동작)·정지 자세(빨강 대응, 몸을 낮추고 손을 가슴 앞)·피격·쓰러짐.
4. Unity 연결: Animator Controller (`Art/Characters/Heroine/`), `Prefabs/Player_Heroine` 외형을 v2로 교체, 새 `PlayerAnimationDriver`(`Scripts/Battle/View/Animation/`)가 `PlayerMover.Motor` 상태(이동 속도·Dodging·Airborne·Parrying·Bracing·BraceReady)와 `BattleFeedback`(PlayerParried 쪽 방향·PlayerHit)을 읽어 Animator를 구동.
   - `PlayerActionView`의 임시 손 큐·원판·웅크림은 Animator가 있으면 끄고 없으면 그대로 (둘 다 동작해야 함). 쳐낸 탄의 곡선 경로(Bullet)는 그대로.
   - 판정 높이(`PlayerActionSettings.bodyHeight`)는 바꾸지 않는다.
5. 테스트: 모터 상태 → Animator 상태 대응 (EditMode는 순수 매핑 함수, PlayMode는 실제 전투에서 회피·점프·자세 때 상태 전환), 기존 쳐내기·자세·입력 테스트 회귀, 화면 캡처 (`SceneCapture.Save("heroine_v2_*")`).

**B. 맵툴 2차 (M5)** — 기존 공개 API만 사용 (`GameSession.BeginStage(index)`, `SceneNames`, `StageRepository`, `StageEditorModel`)
1. Stage Editor 3D 미리보기: Pattern Editor와 같은 `PreviewRenderUtility` 방식으로 경기장·시작점·적·보석 배치를 3D로.
2. "바로 플레이": 편집 중인 스테이지 저장 → `GameSession`을 그 스테이지로 → Battle 씬 Play. 열어 둔 씬은 저장 여부를 물은 뒤 Play가 끝나면 원래 씬으로 돌아온다 (`EditorSceneManager`, `EditorApplication.playModeStateChanged`).
3. 일괄 배치: 영역 드래그로 보석 여러 개 배치·삭제, 좌우/상하 대칭 복사.
4. 기존 검증·되돌리기·체크섬 유지. 테스트: StageEditorModel 일괄 배치·대칭 로직 EditMode.

**7단계 담당 파일**: `Preview/`, `Reference/`(새 파일만), `Exports/`, `Art/Characters/`, `Prefabs/Player_Heroine*`, `Scripts/Battle/View/PlayerActionView.cs`, 새 `Scripts/Battle/View/Animation/`, `Editor/Stages/`, 테스트 새 `Tests/*/PlayerAnimation*`·`StageEditorBatch*`, 기존 `StageEditorModelTests`.

### 8단계 — 출시 준비

**전제**: 회사·게임 이름은 사용자 결정 전 → **한 곳(`ProductInfo`)에 자리표시자로 모으고**, 이미지에는 제목 글자를 넣지 않는다 (로고 자리만 비움).

1. `ProductInfo` (ScriptableObject `Data/ProductInfo.asset` + `Scripts/Core/ProductInfo.cs`): 회사 이름·게임 이름·버전·스토어 표시 이름. 메인 화면 타이틀 문구(`UiTexts`)·빌드 스크립트(PlayerSettings 회사/제품 이름)·MSIX 매니페스트가 여기서 읽도록. 지금 값은 기존과 같게 (회사 `DefaultCompany`는 세이브 경로라 **바꾸지 말고** "이름 확정 시 바꿀 곳" 안내만).
2. 정적 폰트 아틀라스: Pretendard로 KS X 1001 한글 2,350자 + 영문·숫자·기호. 게임 문구가 모두 들어가는지 검사(코드·데이터·스테이지 JSON의 한글을 모아 아틀라스에 없는 글자 목록 출력) → 동적 폰트가 매번 git 변경으로 잡히는 문제 해결. 기존 폰트 에셋 GUID를 유지하거나 TMP 기본 폰트 설정만 바꿔 씬 수정 없이 적용.
3. 스토어 이미지 세트 (이미지 생성 사용, `Reference/title_screen.png`·`store_screenshot.png`·주인공 3면도 참고): 정사각 로고 300×300·150×150·71×71·44×44, 와이드 타일 310×150, 대표 이미지 1920×1080, 게임 화면 스크린샷 3~5장(실제 PlayMode 캡처 기반). 저장 `Reference/store/`. MSIX 로고(`Tools/msix/`)에 연결.
4. 스토어 설명 문구 초안 (한국어): 짧은 소개·긴 설명·주요 특징 5개·검색어, 이름은 `{게임 이름}` 자리표시자 → `Plans/Store_Listing.md` (+HTML). Undertale 고유 요소(이름·캐릭터·빨간 하트 등)를 쓰지 않았는지 점검해 기록.
5. 빌드·MSIX 재검증: `Tools/build_windows.ps1` → `Tools/package_msix.ps1`, 빌드 실행 15초 오류 없음. `Plans/Store_Guide.html`에 "이름 확정 시 바꿀 곳" 추가.

**8단계 담당 파일**: 새 `Scripts/Core/ProductInfo.cs`·`Data/ProductInfo.asset`, `Scripts/UI/UiTexts.cs`(타이틀 문구만), `Art/Fonts/`, TMP 설정(`TextMesh Pro/Resources/TMP Settings.asset`), `ProjectSettings/ProjectSettings.asset`, `Editor/Build/`, `Tools/build_windows.ps1`·`package_msix.ps1`·`msix/`, `Reference/store/`, `Plans/Store_Listing.md/.html`, `Plans/Store_Guide.html`, 테스트 새 `Tests/EditMode/ProductInfo*`·`FontCoverage*`.

## 2. 요청 (Codex → Claude/사용자)

- 7단계 피격 사건 연결: `BattleWorld.Init`에서 `player.Init(...)` 다음에 `player.GetComponent<PlayerAnimationDriver>()?.Bind(player, feedback)` 연결이 필요하다. BattleWorld는 Claude 담당이므로 수정하지 않았다. 모터 상태와 쳐내기 좌우는 PlayerActionView로 연결되어 실제 게임에서 동작하며, 피격은 공개 Bind 주입 후 동작하는 것을 테스트한다. 통합 시 이 연결을 추가한다.
- 쓰러짐 클립은 `PlayFall()` 진입 API를 제공한다. 전투 패배 표현의 호출 지점은 담당 밖 BattlePresentation이므로 통합 시 연결이 필요하다. 기존 패배 흐름은 유지한다.

## 3. 한 일

### 2026-10-06 — 7단계 주인공 v2·맵툴 M5

**한 일**
- v1 보존, 공통 코드에서 v2 제작: 얼굴 면에 눈·볼 밀착, 손 크기/두께와 머리카락 결 보강. 노란 양갈래·노란 눈·하늘 후드티·청바지 스타일 유지.
- three.js SkinnedMesh·골격 19개(골반/척추/머리·팔/손·다리/발·양갈래), 대기/달리기/회피/점프/착지/좌우 쳐내기/정지 자세/피격/쓰러짐 10클립 포함 GLB. Exports와 Art/Characters에 저장, v2 전용 HTML 미리보기와 재현용 제작/내보내기 스크립트.
- Unity Animator Controller·Player_Heroine 외형 교체, PlayerAnimationMap/Driver. 모터 실제 이동·회피·점프·쳐내기·정지 자세로 전환. Animator 사용 시 임시 손 큐/원판/모델 강제 변형 비활성, 기존 모델은 기존 표현 유지. 판정 높이·Bullet·씬 미변경.
- Stage Editor: 3D 배치 미리보기/시점 회전, 보석 영역 드래그 배치·삭제, 좌우/상하 대칭 복사, 한 번 되돌리기·중복/적/시작점 보호. 바로 플레이는 검증·저장 및 씬 저장 확인 후 선택 맵을 재생하고 종료 시 원래 씬 구성을 복원.
- 기본안: 리깅은 현재 로우폴리 파츠를 스킨 메시로 바꾸는 방식. 새 모델링 도구나 에셋 구매 없이 원본 코드를 유지한다. 미리보기 의존성 three.js 0.170.0과 MIT 라이선스를 Preview/vendor에 고정 저장.

**검증/남은 연결**
- 전체 EditMode 156/156: Logs/phase7_editmode.xml. 전체 PlayMode 22/22: Logs/phase7_playmode.xml. 실제 스킨·회피·점프·자세/피격 매핑, 기존 입력/쳐내기/색 통과/8개 맵/결말 회귀.
- 최종 편집기 컴파일·v2 정면/측면/후면·맵 3D 렌더 정상 종료: Logs/phase7_preview.log. 실제 전투 캡처 Logs/scene_heroine_v2_*.png와 모델 정면 확인.
- 피격 Bind와 쓰러짐 호출은 담당 밖 조립 코드가 필요해 2절에 남겼다. 피격 테스트는 공개 Bind로 실제 BattleFeedback을 주입하며, 쓰러짐은 공개 API를 호출해 검사한다. 이 두 항목의 자동 연결 완료로 기록하지 않는다.
- 맵툴 바로 플레이의 저장 대화상자·원래 씬 복귀 직접 조작은 사용자 확인 대상. 스테이지 JSON/씬을 작업 과정에서 수정하지 않았다.
- 최종 외형 보정: 얼굴 장식 삼각형을 세분화해 면 겹침을 없앴고 정지 자세의 손을 가슴 앞으로 모았다. 실제 팔 골격 회전을 추가 검사한 PlayMode 22/22 통과 (Logs/phase8_playmode.xml), 정면/측면/후면과 맵 3D 확인 이미지를 Reference/*_v2.png 및 stage_editor_m5.png에 보관.

### 다음에 할 일
- 8단계 ProductInfo·정적 폰트·스토어 자원/문구·빌드 검증 진행. 통합 시 2절의 두 애니메이션 연결을 적용한다.

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

중급 Unity 개발자 1명이 같은 결과를 만드는 추정 시간 (AI 실행 시간 아님, 8시간 = 1일). main의 `Plans/Work_Effort.md`에 옮겨진 줄은 ✓. 단계 열 이름: `캐릭터`, `맵툴`, `출시`.

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Codex | 패턴 에디터 | 8종 궤적·그래프 적분·일정·런타임·경고 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 편집 창·3D 미리보기·에셋 CRUD·카탈로그 ✓ | 16 |
| 2026-10-06 | Codex | 패턴 에디터 | 수식/일정/에셋 테스트·전투 회귀·기록 ✓ | 8 |
| 2026-10-06 | Codex | 캐릭터 | v2 면/손/머리 보강·19골격·10클립·GLB·Animator·회귀 검증 | 24 |
| 2026-10-06 | Codex | 맵툴 | M5 3D 미리보기·바로 플레이·영역/대칭·검증 | 12 |
