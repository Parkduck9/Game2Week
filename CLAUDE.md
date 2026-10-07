# CLAUDE.md — 작업 규칙 & 판단 기준

Undertale의 **전투 시스템에서 영감을 받은** 턴제 전투 게임을 Unity로 만든다. IP(세계관·캐릭터)는 독자적으로 만든다.
원본 요구사항은 `Tasks.md` (단, 아래 "Tasks.md와 달라진 점"이 우선).
**현재 목표:** 메인 → 3D 연출 전투 1회 → 승리/패배 → 종료를 끝까지 플레이할 수 있는 상태.

## 세션 시작 시 할 일
1. `WORKLOG.md` 맨 아래 "다음에 할 일"을 읽는다.
2. `TODO.md`에서 체크 안 된 첫 항목부터 이어서 진행한다.
3. 작업을 끝내면 `TODO.md` 체크 + `WORKLOG.md`에 기록 (무엇을 했고, 무엇을 결정했고, 다음은 무엇인지).
4. 문제·정리 대상·확인할 것을 발견하면 `TODO.md`의 "🔧 바꿔야 할 것"에 추가. 결정이 필요한 항목엔 `[결정 필요]` 태그.
5. WORKLOG 각 기록의 마지막은 항상 `### 다음에 할 일` (대시보드가 마지막 블록을 읽음).
6. **사용자 요청: 작업한 내용은 MD와 관련 HTML에도 반영한다.** TODO·WORKLOG의 상태를 갱신하고 관련 설계 문서를 실제 구현에 맞춘다. `TodoList.html`은 MD를 읽으므로 체크리스트를 중복 작성하지 않되 새 계획 링크는 추가한다. 액션 설계는 `Plans/Action_Balance_Plan.md`를 수정한 뒤 `node Tools/render_action_plan.mjs`로 HTML을 재생성한다. 제안·미구현·검증 완료를 구분한다.

## 문서 역할
| 파일 | 용도 | 누가 갱신 |
|---|---|---|
| `Tasks.md` | 사용자의 원본 요구사항 | 사용자만 (수정 금지) |
| `TODO.md` | 진행 체크리스트 (기준 문서) | Claude |
| `WORKLOG.md` | 작업 기록, 결정 사항, 다음 할 일 | Claude |
| `TodoList.html` | 대시보드 — 위 MD 3개를 직접 읽어 렌더 (http://localhost:8790/). **따로 동기화 불필요** | 구조 바꿀 때만 |
| `Reference/` | Codex로 만든 레퍼런스 이미지 | Claude (덮어쓰지 말고 `_v2` 등으로 새로 저장) |
| `Preview/` | three.js 모델 + 미리보기 (모델 원본 코드) | Claude |
| `Tools/serve.ps1` | 미리보기 로컬 서버 (`.claude/launch.json`의 `preview`, 포트 8790). `POST /save?name=x.glb` → `Exports/` 저장 | Claude |
| `Exports/` | 미리보기에서 내보낸 GLB 원본 → `Assets/_Project/Art/Characters/...`로 복사해서 사용 | Claude |

## Git
- PATH에 없음 → `$env:LOCALAPPDATA\GitHubDesktop\app-3.6.6\resources\app\git\cmd\git.exe` 사용 (GitHub Desktop 업데이트 시 `app-*` 버전 폴더 확인).
- 커밋은 사용자가 요청할 때만. 커밋 전 컴파일·테스트 통과 확인, `git diff --cached --name-only`로 Library/Exports/Logs/csproj가 없는지 확인.
- 커밋 작성자는 저장소 git 설정을 따름 (Duck9).
- 원격: `origin` = https://github.com/Parkduck9/Game2Week (main 추적). push는 사용자가 요청할 때만. 인증은 GitHub Desktop의 credential manager.
- **Git LFS** (2026-10-06): 정적 폰트 `Assets/_Project/Art/Fonts/Pretendard-*SDF.asset`(각 약 65MB)은 LFS (`.gitattributes`). 처음 받은 폴더·새 worktree에서 폰트가 133바이트 포인터로 보이면 `git lfs checkout` (Unity에서 폰트가 깨짐). 50MB 넘는 새 바이너리도 LFS에 추가.
- 여러 줄 커밋 메시지는 `-m` 대신 파일로 `-F` (PowerShell에서 `-m` 여러 줄이 조용히 실패한 적 있음).
- `.gitignore`: Unity 생성 폴더, IDE 파일, `/Exports/`(GLB 중간본), `.claude/settings.local.json` 제외.

## Unity 배치모드 (CLI)
- 에디터: `C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe`
- **사용자가 에디터로 프로젝트를 열어 둔 상태에서는 배치모드 실행 불가** (프로젝트 잠금) → 먼저 확인.
- 패턴: `-batchmode -projectPath <경로> -executeMethod <Class.Method> -logFile <로그>` (+ 끝나면 `-quit` 또는 `EditorApplication.Exit`).
- PowerShell 5.1의 `Set-Content -Encoding utf8`은 BOM을 붙임 → Unity YAML 파일은 `[IO.File]::WriteAllText` + BOM 없는 UTF8로 쓴다.
- 반대로 **한글이 든 `.ps1` 스크립트는 UTF-8 BOM으로 저장** (PowerShell 5.1은 BOM 없으면 ANSI로 읽어 한글이 깨짐 — MSIX 표시 이름까지 깨질 수 있음).
- Unity 설정 파일을 정규식으로 고칠 땐 범위를 좁힌다 (들여쓰기만으로 매칭하면 다른 블록까지 바뀜). 원본은 템플릿 tgz에서 복구 가능.
- `AssetDatabase.ImportPackage`는 배치모드에서 비동기 → `.unitypackage`는 `-importPackage <경로> -quit`로 따로 실행.
- 임시 에디터 스크립트는 `Assets/Editor/`에 두고 실행 후 폴더째 삭제.
- 에디터 스크립트 함정: (1) 프리팹 저장 직후 받은 참조는 이후 다른 에셋 저장으로 재임포트되면 끊길 수 있음 → 연결 직전에 `LoadAssetAtPath`로 다시 읽기.
  (2) `GetComponent<T>() ?? Add…` 금지 (에디터의 가짜 null) → `TryGetComponent`.
- Cinemachine Impulse 기본 세기는 약 1m — 반드시 `BattleCameraDirector.Shake`(진폭 조절)로만 흔든다.
- 배치모드가 10분 넘게 걸릴 때가 있음 — Unity 클라우드 요청 타임아웃 대기. 오래 걸리면 `run_in_background`로 돌리고 로그 확인.
- 맵툴 등 에디터를 띄워 사용자에게 보여 줄 땐 `-batchmode` 없이 `-projectPath . -executeMethod <창 여는 메서드>`. 이후엔 프로젝트가 잠겨 배치모드 불가.

## Tasks.md와 달라진 점 (사용자 결정, 2026-10-06)
- Tasks.md 1번의 "2D" → **3D 프로젝트**. 3D는 전투 연출 + **탄막 회피도 3D 경기장에서** (2026-10-06 변경).
- **전투 박스 = 3D 경기장** (월드 공간). 대사·메뉴는 화면 UI.
- **턴 구조 (원작과 다름):** 전투 시작 → 탄막 턴 → 결과에 따라 메뉴 → 다시 탄막 턴 …
  - 탄막 턴 동안 주인공이 경기장을 뛰어다니며 피하고 **적에게 닿으면** 즉시 턴 종료 → **FIGHT / ACT / MERCY** 메뉴.
  - **닿지 못하고** 탄막 턴이 끝나면 → **ITEM / 넘기기** 메뉴.
- 현재 구현 조작: 주인공이 **카메라 기준 이동·Shift 회피·Space 점프·우클릭 쳐내기·Ctrl 정지 자세(빨강 통과, 파랑은 실제 이동으로 통과)**, 캐릭터 뒤 허리 추적 카메라와 마우스 회전/휠 클릭 록온. 1·2단계 구현과 기존 흐름 검증 완료: 노랑 직선/사인파/측면 교대, 거리 경고음·화면 밖 표시. 최신 상세는 액션 설계 MD 1절의 실제 적용 상태를 따른다.
- 경기장은 **스테이지 데이터(JSON)** 로 크기·배치가 정해지고, JSON은 **맵툴로만** 편집 (계획: `Plans/MapTool_Plan.html`).
- 새 탄막 패턴은 **`Tools ▸ Pattern Editor`** 로 만든다 (`GraphPatternDefinition` + 궤적 8종, 저장 시 ContentCatalog 등록 → 맵툴에서 `Attack_Graph_이름` 선택).
- Tasks.md 5번의 "구조만" → 테스트 패턴 1종으로 **실제 피격까지** 구현.
- Tasks.md 9번 ChatGPT 이미지는 2D 리소스(UI·텍스처·아이콘)용. 3D 모델 출처는 미정.
- **독자 IP** — 세계관/캐릭터는 미정, 정해질 때까지 임시 이름 사용.

### 최신 사용자 요청: 패턴 누적 · 3D 액션 회피 (2026-10-06)
- 사용자는 단계마다 패턴을 하나씩 늘리고 발사 간격을 줄이며, 탑다운 느낌에서 3D 액션처럼 피하는 전투로 바꾸길 요청했다.
- 먼저 MD·HTML로 설계한 뒤 구현 방법을 함께 검토한다. 설계: `Plans/Action_Balance_Plan.md` / `.html`.
- 허리 추적 카메라·회피·점프·노랑 쳐내기는 구현했다. 이전 "점프·대시 없음" 설명은 최신 구현을 제한하지 않는다. 실제 적용 수치와 완료 여부는 액션 설계 MD 1절을 따른다.
- 후속 사용자 지정: 적 접촉 후 메뉴 유지, 캐릭터 뒤 허리 카메라+마우스 회전/록온, 노랑 우클릭 쳐내기/Shift 회피/점프, 이후 Ctrl 빨강 정지 자세 필수·파랑 이동, 접근할수록 커지는 경고음, 플레이어 위치 랜덤 조준, 8종 그래프 궤적+발사 간격/속도 제작 툴. 최신 기준은 설계 MD 0절.
- 쳐내기는 오른손으로 왼쪽에서 오는 탄을 오른쪽 어깨 뒤로, 오른쪽 탄을 왼쪽 어깨 뒤로 흘린다. 입력 방향으로 이동을 계속한다. 적에게 단순 반사하는 방식이나 강제 뒤 밀림이 아니다. 점프는 후속 답변으로 포함이 확정됐고, 록온 가운데 버튼/점프 Space는 기본안이다.

### 5~8단계 결과로 생긴 규칙 (2026-10-06 합침)
- 맵 패턴은 `PatternDirector` + `PatternEncounterData`/`DifficultyProfile`(`Data/Patterns/Director/`)로 누적 — 새 패턴은 `IDirectablePattern` 구현(색·난이도 배율). 9단계부터 1-1 포함 8개 맵 모두 Director, 한 턴 최대 3겹, 적 고유 기술은 `EnemyData.signatureMoves`/`phaseMoves`, 체력 단계는 `DifficultyProfile.phases`.
- 맵은 칸 1m·시작점-적 15m 이상 (맵툴 경고). 스테이지 JSON v2 (`theme`·`dialogues`). 테스트에서 하위 패턴은 `BattleTestUtil.FindPattern`, 탄을 맞히려면 `MoveNearEnemy`.
- 월드 사건은 `BattleFeedback`, 이펙트는 `BattleFxRig` 프리팹 아래 `IBattleFxModule`(씬 수정 없이), 소리는 `BattleAudio`·`UiSoundSet`·`SceneBgm` + `AudioRouting`(믹서 선택). 씬 전환은 `SceneLoader`(페이드 0.25초, `FadeSeconds`).
- 이름·버전은 `Data/ProductInfo.asset` 한 곳 (회사 `DefaultCompany`는 세이브 경로라 확정 전까지 유지). 폰트는 정적 아틀라스 — 새 한글 문구는 `FontCoverageTests`가 검사.
- 주인공 동작 클립은 glb 안 클립이 아니라 `Tools ▸ Heroine ▸ 동작 클립 다시 만들기`(`HeroineClipAuthoring`)가 만든 `Art/Characters/Heroine/Clips/Heroine_*.anim` — 동작을 고칠 땐 이 도구의 자세 함수를 고치고 다시 실행 (2026-10-07, glb 클립은 T자 자세 버그).
- 개선 설계(9~12단계)는 `Plans/Playtest_Fix_Plan.md` (사용자 컨펌 2026-10-07): 맵 확대, 적별 기술·겹·페이즈, 자비 = 공통 3턴 + 적별 조건, 대화 JSON + Dialogue Editor(전투 안에서만), 로우폴리 유지 + 셰이더·이펙트.
- 밸런스 측정: `-runBalance`로 `BalanceMeasurementTests.MeasureAllStages` → `node Tools/render_balance_report.mjs`.
- 에디터 스크립트 함정 추가: (3) `EditorSceneManager.OpenScene` 뒤에는 앞에서 잡은 에셋 참조가 끊길 수 있음 → 씬마다 다시 `LoadAssetAtPath`.

### 병렬 작업 (2026-10-06, 사용자 결정)
- 3단계(Pattern Editor) = **Codex**, 4단계(빨강·파랑) = **Claude**. 기준: `Plans/Parallel_Work_Plan.md` (파일 담당표·worktree·합치기 규칙).
- 각자 worktree 폴더(`..\Game2Week-codex`, `..\Game2Week-claude`)에서 작업하고 **담당 밖 파일은 고치지 않는다.** 배치모드도 자기 폴더에서만.
- 병렬 기간에는 TODO·WORKLOG·CLAUDE·설계 MD를 고치지 않고 `Plans/Parallel/<이름>_Log.md`에 기록 → 합칠 때 main에서 반영.
- 각자 할 일·한 일 문서: Codex → `Plans/Parallel/Codex_Log.md`, Claude → `Plans/Parallel/Claude_Log.md` (+HTML). 다음 단계 지시는 Claude가 여기에 쓴다. **모든 기록은 한국어로만** (사용자 요청).
- 연결 지점(`ITrajectory`, `AttackColor`, `IHitRule`, `ThreatPoint`, `PatternContext`)은 공용 — 바꿔야 하면 사용자에게 먼저 알린다.
- 이후 단계(5~8)도 같은 방식: 영역별 기본 담당(계획 8절) + 단계마다 N-0 연결 지점을 main에 먼저.
- **작업량(사람 기준)**: 작업을 끝낼 때마다 `Plans/Work_Effort.md`에 한 줄 (중급 Unity 개발자 1명 추정 시간, 8시간=1일). 대시보드가 Claude·Codex 막대로 표시. 병렬 중에는 로그 파일의 "작업량" 표에 적고 합칠 때 옮긴다.
- 계획 MD → HTML: `Tools/render_plan.mjs <md>`. `node`는 PATH에 없음 → `C:\Program Files\Adobe\Adobe Creative Cloud Experience\libs\node.exe` (v20) 사용.

## 확정된 결정 (사용자 승인됨)
- Unity 6 · 3D(URP) · **New Input System** · Cinemachine(카메라 연출)
- 조작: 방향키 / 확인 Z·Enter / 취소 X·Shift (메뉴도 WASD 가능, 2026-10-07 사용자 요청)
- 메뉴: 적에게 닿았을 때 FIGHT · ACT · MERCY / 못 닿았을 때 ITEM · 넘기기 — **명칭은 임시**
- 테스트 적 외형: Primitive/로우폴리 도형 조합 임시 모양
- FIGHT = 움직이는 게이지 Timing Attack
- 테스트 적 1종, 그래픽은 임시 리소스 (3D는 Primitive 조합)
- Unity 버전 **6000.3.25f1** (6.3 LTS) — 프로젝트는 Claude가 CLI로 현재 폴더에 생성
- 한글 폰트 **Pretendard** (OFL) — `Art/Fonts/Pretendard-Regular SDF` (동적 아틀라스)가 TMP 기본 폰트
- 타깃 플랫폼 **PC 전용** (URP 품질 레벨 PC 하나만)
- **서비스 흐름 (2026-10-06):** 메인(새로 시작·이어하기·설정·종료) → 스테이지 선택(해금된 것만·최고기록) → 전투(ESC 일시정지: 계속·메인으로) → 결과 → 엔딩 → 메인. 진행·설정 저장.
  - 배포: **Microsoft Store (MSIX)** — 회사에선 빌드·패키징 준비, 설치·삭제·제출은 사용자가 집에서
  - 최고기록 = **클리어 시간 + 결과 종류**(처치/살려줌), 설정 = **음량(배경음/효과음)·화면 모드/해상도·텍스트 속도**
  - **앱을 삭제하면 세이브도 삭제** (persistentDataPath, MSIX 기본 동작) — 계획: `Plans/Service_Plan.html`
- 이미지 생성은 **Codex CLI** (`codex.exe`, ChatGPT OAuth 로그인됨) 사용 — 경로: `C:\Program Files\WindowsApps\OpenAI.Codex_*\app\resources\codex.exe`
- 화면 비율 **16:9** — 기준 해상도 1920×1080 (Canvas Scaler 기준값)
- **한글 사용** — 대사·UI 텍스트 한글, TMP 한글 폰트 에셋 필요 (원작 폰트 금지, 상업 이용 가능한 무료 폰트)
- 아트 스타일 **로우폴리** — 플랫/단색 머티리얼 위주, 임시 모델도 이 방향에 맞춤

## 아직 결정 안 됨 — 착수 전 반드시 질문
- 맵툴·스테이지 세부 (`Plans/MapTool_Plan.html`의 "확인 필요" — 보석의 역할, 툴 형태 등)
- 3D 모델 리소스 출처 (에셋 스토어 / Blender / AI 3D 생성 등)
- 데미지 공식과 수치 (플레이어·적 스탯)
- 탄막 패턴 모양
- MERCY에 Flee 포함 여부
- IP: 세계관, 캐릭터, 시스템 명칭

## 해야 할 것 (DO)
- **상태 머신으로 전투 흐름 관리.** 상태는 `IBattleState`(Enter/Tick/Exit) 클래스로 분리.
- **로직과 표현 분리.** 전투 로직은 Transform·Animator·카메라를 모른다. `EnemyView`, `BattleCameraDirector` 등 View가 이벤트를 구독해 연출한다.
- **데이터는 ScriptableObject.** 적·아이템·ACT·패턴은 에셋 추가만으로 늘릴 수 있어야 한다. 적 외형은 `EnemyData`의 View 프리팹으로 교체.
- **UI는 이벤트를 구독해서 갱신.** 로직이 UI를 직접 조작하지 않는다.
  - 예외: 플레이어 응답이 필요한 대사·메뉴는 상태가 `IBattleUi` 인터페이스로 요청 (구현 `BattleUi`), 3D 이동·접촉은 `IBattleWorld` (구현 `BattleWorld`). 테스트는 가짜 구현으로.
- 화면에 나오는 공통 문구·메뉴 이름은 `BattleTexts` 한 곳에 (적별 문구는 EnemyData).
- MonoBehaviour는 **파일 하나에 하나, 파일 이름 = 클래스 이름** (아니면 씬에서 Missing Script).
- **입력은 `InputReader` 래퍼를 통해서만.**
- **탄막 패턴은 `IAttackPattern` + 프리팹** 으로 교체/추가 가능하게.
- **리소스는 SerializeField / 데이터 에셋으로 참조** → 모델·스프라이트만 바꿔 끼우면 교체 완료.
- 사용자에게 보이는 이름·텍스트는 데이터/상수로 모아 IP 확정 시 한 번에 바꿀 수 있게.
- 프로토타입이 아니라 확장 가능한 품질: 명확한 이름, 단일 책임, 매직 넘버는 데이터나 상수로.
- 의미 있는 단위마다 커밋.

## 하면 안 되는 것 (DON'T)
- **중요한 설계 선택을 임의로 결정하지 않는다** (Tasks.md 10번). 위 "아직 결정 안 됨" 항목은 질문 먼저.
- **Undertale의 고유 요소 사용 금지** — 캐릭터, 이름(지명·인물), 대사, 스프라이트, 음악·효과음, 폰트(Determination 등), 빨간 하트 SOUL 디자인을 그대로 쓰지 않는다. 장르적 메커니즘(턴제 + 탄막 회피 + 타이밍 공격 + 비전투 해결)만 참고.
- 코드 클래스명에 원작 고유 용어를 박지 않는다 (예: `Soul` 대신 `PlayerMarker`, `Mercy` 대신 `SpareAction`처럼 중립적으로).
- 오버월드·필드 탐험은 만들지 않는다 — 스테이지 = 전투 경기장.
- 최신 액션 구현과 후속 작업은 `Plans/Action_Balance_Plan.md`의 실제 적용 상태를 따른다. 빨강/파랑·8종 제작 툴·맵별 누적은 해당 단계 검증 전 완료로 기록하지 않는다.
- 스테이지 JSON을 손으로 고치지 않는다 — 맵툴로만 (Claude도 마찬가지, 툴/코드로 생성).
- `Tasks.md`를 수정하지 않는다.
- 싱글톤 남발, `FindObjectOfType`/`GameObject.Find` 로 참조 잡기 금지 → BattleContext/인스펙터 주입 사용.
- 상태 클래스끼리 직접 참조 금지 (상태 전환은 상태 머신을 통해서만).
- 적/패턴별 `switch`·`if` 분기를 전투 코드에 하드코딩하지 않는다.
- 구 Input Manager(`Input.GetKey`) 사용 금지.
- 범위 밖 기능(세이브, 다중 적, 레벨업, 오버월드 등)을 미리 만들지 않는다 — 필요해 보이면 제안만.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/` 커밋 금지.

## 권장 폴더 구조
```
Assets/_Project/
├─ Scenes/            MainMenu, Battle, Result
├─ Scripts/
│  ├─ Core/           GameSession(씬 간 데이터), SceneLoader, SceneNames, GameQuit, InputReader
│  ├─ UI/             공용 UI 부품 (MenuList, MenuListView, MenuNavigator)
│  ├─ Flow/           씬별 컨트롤러 (MainMenu, StageSelect, Battle, Result, Ending, PauseMenu)
│  ├─ Save/           SaveService, SaveData/SettingsData, JsonFileStore, SettingsModel, DisplaySettings
│  ├─ Battle/
│  │  ├─ States/      전투 상태 클래스
│  │  ├─ UI/          BattleUi(IBattleUi), DialogueBox, StatusBar, TurnHud, PopupText, BarView
│  │  ├─ Model/       PlayerCombatant, EnemyCombatant, Inventory, GemField, GemRewards (순수 로직)
│  │  ├─ View/        BattleArena, StageSpawner, EnemyView, GemView, BattleCameraDirector, BattleEffects (3D 연출)
│  │  ├─ Player/      플레이어 마커 이동·피격
│  │  └─ Patterns/    IAttackPattern, Bullet, 패턴 구현
│  ├─ Stages/         스테이지 JSON 데이터·검증·저장소 (StageDefinition, StageJson, StageValidator, StageRepository)
│  ├─ Rendering/      LowPolyMesh (임시 리소스용 각진 메시)
│  └─ Data/           ScriptableObject 정의 (+ ContentCatalog: 스테이지의 이름 → 에셋)
├─ Editor/            에디터 전용 (asmdef: Game2Week.Editor) — 맵툴 Tools ▸ Stage Editor
├─ Data/              Enemy, Item, Pattern 에셋
├─ Art/Placeholder/   임시 모델·머티리얼·스프라이트
├─ Prefabs/
├─ Input/             GameControls.inputactions (프로젝트 전역 액션)
├─ Tests/EditMode/    NUnit 테스트 (asmdef: Game2Week.Tests.EditMode, Editor 어셈블리도 참조)
└─ Tests/PlayMode/    씬 흐름 테스트 + 화면 캡처 → Logs/scene_*.png
Assets/StreamingAssets/Stages/   stages.json(순서) + stage_XXX.json — 맵툴로만 편집
```

## 코드 규칙
- 네임스페이스 `Game2Week.<영역>` (Core, Battle …), 어셈블리 `Game2Week` (Scripts/), Input System 참조.
- 상태 전환은 `BattleContext.ChangeState(BattleStateId)`만 사용.
- 순수 로직은 MonoBehaviour 밖 C# 클래스로 → EditMode 테스트 작성.
- Unity C# 제약: `init` 접근자·`record` 사용 금지 (IsExternalInit 없음). 커밋 전에는 컴파일·테스트 통과 확인.
- 테스트 실행: `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode|PlayMode -assemblyNames Game2Week.Tests.EditMode|Game2Week.Tests.PlayMode -testResults <xml> -logFile <log>` (`-quit` 붙이지 않음, `-assemblyNames` 없으면 Input System 패키지 테스트가 섞임).
- 키보드 입력 테스트는 `InputTestFixture` (manifest `testables`에 inputsystem) — `BattleInputTests` 참고.
  - 주의: InputTestFixture는 Game 창 포커스를 무시하므로 "에디터에서 키가 안 먹는" 문제는 못 잡는다.
- Input System 설정 에셋 `_Project/Input/InputSystemSettings.asset`: 에디터 플레이 중 키보드가 항상 Game 창으로 (`InputSettingsSetup`이 에디터 켤 때 확인). 지우지 말 것.
- PlayMode 테스트는 반드시 `TestSave.Begin()/End()` — 실제 세이브(persistentDataPath) 대신 임시 폴더 (`GameSession.SaveDirectoryOverrideForTests`).
- 세이브: `GameSession.Save`(SaveService) 하나로만 읽고 쓴다. 진행(save.json)·설정(settings.json) 분리, 쓰기는 `JsonFileStore`(임시 파일→교체 + .bak).
- 입력 맵: UI(메뉴) / Player(이동) / System(ESC, 항상 켜짐). 메뉴·창이 겹칠 땐 여는 쪽이 자기 메뉴 GameObject를 끈다 (켜진 MenuNavigator만 입력을 받음).
- 빌드: `Tools/build_windows.ps1` → Builds/Windows, 포장: `Tools/package_msix.ps1` → Builds/Msix (Builds/는 git 제외).
- 씬 사이 데이터는 `GameSession` 에셋으로 (싱글톤·static 상태 금지). 씬 이름은 `SceneNames` 상수만.
- 화면 확인은 PlayMode 테스트의 `Capture()` 이미지로 — 배치모드 첫 렌더는 색이 깨지므로 두 번 렌더.
- PowerShell 한 명령 안에 `"C:\Program Files\..."` 경로와 `Remove-Item`을 같이 쓰면 안전검사에 막힘 → `Join-Path $env:ProgramFiles` 사용, 삭제는 따로.
