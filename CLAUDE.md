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
- 커밋 작성자는 저장소 git 설정을 따름 (Duck9). push는 하지 않음 (요청 시에만).
- `.gitignore`: Unity 생성 폴더, IDE 파일, `/Exports/`(GLB 중간본), `.claude/settings.local.json` 제외.

## Unity 배치모드 (CLI)
- 에디터: `C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe`
- **사용자가 에디터로 프로젝트를 열어 둔 상태에서는 배치모드 실행 불가** (프로젝트 잠금) → 먼저 확인.
- 패턴: `-batchmode -projectPath <경로> -executeMethod <Class.Method> -logFile <로그>` (+ 끝나면 `-quit` 또는 `EditorApplication.Exit`).
- PowerShell 5.1의 `Set-Content -Encoding utf8`은 BOM을 붙임 → Unity YAML 파일은 `[IO.File]::WriteAllText` + BOM 없는 UTF8로 쓴다.
- Unity 설정 파일을 정규식으로 고칠 땐 범위를 좁힌다 (들여쓰기만으로 매칭하면 다른 블록까지 바뀜). 원본은 템플릿 tgz에서 복구 가능.
- `AssetDatabase.ImportPackage`는 배치모드에서 비동기 → `.unitypackage`는 `-importPackage <경로> -quit`로 따로 실행.
- 임시 에디터 스크립트는 `Assets/Editor/`에 두고 실행 후 폴더째 삭제.
- 배치모드가 10분 넘게 걸릴 때가 있음 — Unity 클라우드 요청 타임아웃 대기. 오래 걸리면 `run_in_background`로 돌리고 로그 확인.
- 맵툴 등 에디터를 띄워 사용자에게 보여 줄 땐 `-batchmode` 없이 `-projectPath . -executeMethod <창 여는 메서드>`. 이후엔 프로젝트가 잠겨 배치모드 불가.

## Tasks.md와 달라진 점 (사용자 결정, 2026-10-06)
- Tasks.md 1번의 "2D" → **3D 프로젝트**. 3D는 전투 연출 + **탄막 회피도 3D 경기장에서** (2026-10-06 변경).
- **전투 박스 = 3D 경기장** (월드 공간). 대사·메뉴는 화면 UI.
- **턴 구조 (원작과 다름):** 전투 시작 → 탄막 턴 → 결과에 따라 메뉴 → 다시 탄막 턴 …
  - 탄막 턴 동안 주인공이 경기장을 뛰어다니며 피하고 **적에게 닿으면** 즉시 턴 종료 → **FIGHT / ACT / MERCY** 메뉴.
  - **닿지 못하고** 탄막 턴이 끝나면 → **ITEM / 넘기기** 메뉴.
- 탄막 턴 조작: **주인공 3D 모델**이 **바닥 평면 8방향** 이동 (점프·대시 없음), **비스듬한 고정 카메라**.
- 경기장은 **스테이지 데이터(JSON)** 로 크기·배치가 정해지고, JSON은 **맵툴로만** 편집 (계획: `Plans/MapTool_Plan.html`).
- Tasks.md 5번의 "구조만" → 테스트 패턴 1종으로 **실제 피격까지** 구현.
- Tasks.md 9번 ChatGPT 이미지는 2D 리소스(UI·텍스처·아이콘)용. 3D 모델 출처는 미정.
- **독자 IP** — 세계관/캐릭터는 미정, 정해질 때까지 임시 이름 사용.

## 확정된 결정 (사용자 승인됨)
- Unity 6 · 3D(URP) · **New Input System** · Cinemachine(카메라 연출)
- 조작: 방향키 / 확인 Z·Enter / 취소 X·Shift
- 메뉴: 적에게 닿았을 때 FIGHT · ACT · MERCY / 못 닿았을 때 ITEM · 넘기기 — **명칭은 임시**
- 테스트 적 외형: Primitive/로우폴리 도형 조합 임시 모양
- FIGHT = 움직이는 게이지 Timing Attack
- 테스트 적 1종, 그래픽은 임시 리소스 (3D는 Primitive 조합)
- Unity 버전 **6000.3.25f1** (6.3 LTS) — 프로젝트는 Claude가 CLI로 현재 폴더에 생성
- 한글 폰트 **Pretendard** (OFL) — `Art/Fonts/Pretendard-Regular SDF` (동적 아틀라스)가 TMP 기본 폰트
- 타깃 플랫폼 **PC 전용** (URP 품질 레벨 PC 하나만)
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
- 탄막 회피에 점프·대시·높이 이동을 넣지 않는다 — 바닥 평면 8방향만 (사용자 결정).
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
│  ├─ Flow/           씬별 컨트롤러 (MainMenu, Result, 임시 BattleFlowStub)
│  ├─ Battle/
│  │  ├─ States/      전투 상태 클래스
│  │  ├─ UI/          메뉴, 대사창, HP바, 타이밍 게이지
│  │  ├─ View/        EnemyView, BattleCameraDirector, 이펙트 (3D 연출)
│  │  ├─ Player/      플레이어 마커 이동·피격
│  │  └─ Patterns/    IAttackPattern, Bullet, 패턴 구현
│  ├─ Stages/         스테이지 JSON 데이터·검증·저장소 (StageDefinition, StageJson, StageValidator, StageRepository)
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
- 테스트 실행: `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode|PlayMode -testResults <xml> -logFile <log>` (`-quit` 붙이지 않음).
- 씬 사이 데이터는 `GameSession` 에셋으로 (싱글톤·static 상태 금지). 씬 이름은 `SceneNames` 상수만.
- 화면 확인은 PlayMode 테스트의 `Capture()` 이미지로 — 배치모드 첫 렌더는 색이 깨지므로 두 번 렌더.
- PowerShell 한 명령 안에 `"C:\Program Files\..."` 경로와 `Remove-Item`을 같이 쓰면 안전검사에 막힘 → `Join-Path $env:ProgramFiles` 사용, 삭제는 따로.
