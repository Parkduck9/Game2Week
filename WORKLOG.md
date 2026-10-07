# WORKLOG — 작업 기록

새 기록은 아래에 추가한다. 각 기록: **한 일 / 결정 / 주의 / 다음에 할 일**.

---

## 2026-10-06 — 기획 & 문서 세팅

### 한 일
- `Tasks.md` 분석. 폴더에 Unity 프로젝트가 아직 없음 확인 (`.gitattributes`, `Tasks.md`만 존재).
- 사용자에게 범위 질문 4개 → 답변 받음 (아래 결정).
- `TodoList.html` 작성 — 10단계 47개 항목, 체크 상태는 브라우저 localStorage.
- `TODO.md` 작성 — HTML과 같은 항목, Claude가 체크하는 기준 문서.
- `CLAUDE.md` 작성 — 규칙(DO/DON'T), 확정/미확정 결정.
- `WORKLOG.md` (이 파일) 작성.

### 결정 (사용자 승인)
- 메뉴: FIGHT + ACT + ITEM + MERCY (최소 기능)
- Unity 6 + New Input System
- 적 턴: 하트 이동 + 간단한 패턴 1개 (Tasks.md의 "구조만"보다 한 단계 더 — 실제 피격까지)
- 할 일 목록 형태: 로컬 HTML 파일

### 주의
- Unity 프로젝트 생성은 Unity Hub/에디터에서 해야 할 수 있음 → 방법은 사용자와 확인.
- 해상도, 한글 폰트, 데미지 수치, 적/패턴 컨셉, Flee 여부는 미정 → 해당 단계 전에 질문.

---

## 2026-10-06 — 방향 전환: 3D 연출 + 독자 IP

### 한 일
- 사용자 요청 "언더테일을 3D로, 우리만의 IP로" → 질문 3개 → 답변 받음.
- `TODO.md`, `CLAUDE.md`, `TodoList.html` 을 3D 기준으로 갱신.
  - 05단계 "3D 전투 무대 & 연출" 추가 (아레나, EnemyView, Cinemachine 카메라, 이펙트) → 총 11단계.
  - "보류: IP 기획" 섹션 추가.
  - CLAUDE.md에 "Tasks.md와 달라진 점", 원작 요소 사용 금지 규칙 추가.

### 결정 (사용자 승인)
- **지금 바로 3D로 전환** (2D 먼저 완성하지 않음).
- 3D 범위는 **전투 연출만** — 메뉴·탄막 회피는 평면(2.5D) 유지.
- IP 세계관/캐릭터는 **아직 미정** → 임시 이름으로 진행.

### Claude가 기본값으로 정한 것 (사용자가 바꿀 수 있음)
- Cinemachine으로 카메라 연출.
- 코드에서 원작 용어 대신 중립 이름 사용 (`PlayerMarker`, `SpareAction` 등).
- 3D 임시 리소스는 Unity Primitive 조합.

### 주의
- Tasks.md는 여전히 "2D"라고 적혀 있음 → CLAUDE.md의 "Tasks.md와 달라진 점"이 우선.
- ChatGPT 이미지 생성은 3D 모델을 만들 수 없음 → 3D 모델 출처 결정 필요.

---

## 2026-10-06 — 셋업 관련 결정

### 결정 (사용자 승인)
- 화면 비율 **16:9**
- **한글 사용**
- 아트 스타일 **로우폴리**

### Claude가 기본값으로 정한 것
- 기준 해상도 1920×1080 (Canvas Scaler reference).

---

## 2026-10-06 — 셋업 결정 + 우선순위 변경: 캐릭터 모델링

### 결정 (사용자 승인)
- Unity **6000.3.25f1**, Claude가 CLI로 생성, 폰트 **Pretendard**.
- **우선 귀여운 여자 캐릭터 모델링부터** — Codex(OAuth)로 이미지 생성 → 그걸 보고 3D 모델링 → 미리보기.
  Unity 프로젝트 생성은 그 뒤로 미룸.

### 환경 확인
- Codex 데스크톱 앱 설치 + 로그인됨 (`~/.codex/auth.json`), CLI `codex.exe` 앱 내부에 있음 (PATH 아님).
- Blender, Node, Python(실제), git(PATH) 없음.

### 사용자에게 보낸 질문 (답변 대기)
- 캐릭터 역할/외형/비율/색, 레퍼런스 이미지 형태, 모델링 방식(three.js 코드 / Blender 설치 / 이미지→3D 서비스), 리깅 필요 여부.

---

## 2026-10-06 — 주인공 캐릭터 레퍼런스 + 로우폴리 모델 v1

### 결정 (사용자 승인)
- 주인공: 노란 양갈래, 노란 눈, 평범한 후드티 + 청바지, "소심해 보이지만 할 땐 하는" 성격, 2.5등신.
- 레퍼런스 3면도, 모델링 방식 A(three.js 코드), 단색 면 위주, 선 자세만 (리깅은 나중).

### Claude가 기본값으로 정한 것
- 후드티 색 연하늘(#a3c4ee), 머리끈 파랑, 흰 운동화 — 사용자가 색을 정하지 않아서.
- 키 ≈ 1.0 유닛, +Z 정면, 원점이 발바닥 (Unity 임포트 기준).

### 한 일
- Codex CLI(`codex exec`, 내장 image_gen)로 3면도 생성 → `Reference/heroine_turnaround.png`.
- `Preview/heroine_preview.html` — three.js(CDN)로 모델을 코드로 생성, 레퍼런스와 나란히 비교, GLB 내보내기 버튼.
  - 파츠 이름이 붙어 있음 (Face, Hair_Tail_L, Sleeve_R …) → 나중에 리깅/교체할 때 사용.
- `Tools/serve.ps1` + `.claude/launch.json` — 미리보기용 로컬 서버 (http://localhost:8765/).
  브라우저 패널은 파일을 data: URL로 열어서 상대 경로 이미지가 깨지므로 서버가 필요. 사용자는 HTML 더블클릭으로도 열 수 있음.
- 1차 렌더 후 수정: 피부/눈 색 밝게, 앞머리를 넓적한 덩어리로, 양갈래 위치를 뒤로, 목 노출 줄임.

### 주의
- tube()의 Frenet 프레임은 방향이 제멋대로라 납작한 단면은 `side` 옵션으로 축을 지정해야 함.
  `side`가 곡선 접선과 평행하면 NaN → 양갈래는 side=Z, 앞머리/옆머리는 side=X.
- 얼굴 파츠(눈·입·볼)는 `faceZ()`로 얼굴 곡면에 붙임 — 얼굴 형태(FACE, jaw)를 바꾸면 자동으로 따라감.
- glTF에는 flat shading 플래그가 없어서 지오메트리를 non-indexed + 면 노멀로 만들어 둠 → Unity에서도 각진 느낌 유지.

---

## 2026-10-06 — GLB 내보내기 + Unity 프로젝트 생성

### 결정 (사용자 승인)
- 모델 v1 확정 → GLB 내보내기, Unity 프로젝트 생성.

### 한 일
- `Tools/serve.ps1`에 `POST /save?name=*.glb` 추가 → `Exports/heroine_v1.glb` (438KB). 포트 8765가 OS 예약돼서 **8790**으로 변경.
- Unity CLI: `-createProject . -cloneFromTemplate com.unity.template.3d-cross-platform-17.0.14.tgz` (= Universal 3D).
  - 포함: URP 17.3.0, Input System 1.20.0 (activeInputHandler=1, 새 시스템 전용).
  - 추가: Cinemachine 3.1.7, glTFast 6.20.0 (임시 에디터 스크립트 + `Client.AddAndRemove`, 실행 후 삭제).
- 템플릿 TutorialInfo/Readme 삭제, 기본 해상도 1920×1080.
- `Assets/_Project/` 폴더 구조 생성, GLB → `Art/Characters/Heroine/`, 레퍼런스 → `Art/Reference/`.
- 배치모드로 GLB 임포트 검증: 65메시, 4410삼각형, 크기 (0.71, 0.97, 0.40), 발바닥 y=0.
- Unity용 `.gitignore` 추가.

### 주의
- git이 PATH에 없어 커밋 못 함.
- 템플릿 기본 `Assets/Scenes/SampleScene`, `Assets/InputSystem_Actions.inputactions`는 남겨 둠 → 01/02단계에서 정리.
- 미리보기 모델을 수정하면: 미리보기 → GLB 내보내기 → `Assets/_Project/Art/Characters/Heroine/`로 복사 (새 버전은 `_v2`).

---

## 2026-10-06 — 01단계 나머지 + 대시보드

### 사용자 요청
- 계속 진행 + "한 작업과 바꿔야 할 작업을 md와 html로 적고 있는지" 확인.

### 한 일
- **대시보드**: `TodoList.html`을 TODO.md / WORKLOG.md / CLAUDE.md를 직접 읽어 보여 주는 페이지로 교체.
  - 전체·단계별 진행률, "다음에 할 일"(WORKLOG 마지막 블록), "바꿔야 할 것", "결정 필요" 카드, 문서 탭. 5초마다 자동 갱신.
  - 이제 HTML은 손으로 동기화할 필요 없음 → MD만 고치면 됨. 서버 루트(`/`)가 대시보드.
- TODO.md에 **"🔧 바꿔야 할 것"** 섹션 추가.
- `_Project/Input/GameControls.inputactions` 작성 (UI: Navigate/Submit/Cancel, Player: Move; 키보드 방향키·Z/Enter·X/Shift + 게임패드),
  프로젝트 전역 액션으로 등록, 템플릿 `InputSystem_Actions` 삭제.
- `_Project/Scenes/Sandbox_Heroine.unity` 생성 (확인용, 빌드 목록엔 안 넣음).
- URP `PC_RPAsset`: MSAA 4x, 그림자 거리 20, 캐스케이드 2.

### Claude가 기본값으로 정한 것
- 게임패드 바인딩도 추가 (키보드가 기본, 확장 대비).
- 메뉴·대사·타이밍 공격은 UI 맵(Submit), 탄막 회피 이동은 Player 맵(Move) — 상태에 따라 InputReader가 맵을 전환.

### 주의
- 배치모드에서 `Camera.Render()`로 한 번 찍으면 SRP Batcher 때문에 머티리얼 색이 섞여 나옴 (끄면 정상). 에디터 확인 필요 → "바꿔야 할 것"에 등록.
- 임시 에디터 스크립트(`Assets/Editor/ProjectSetup01.cs`)는 실행 후 삭제함.

---

## 2026-10-06 — 02 아키텍처 기반 코드

### 한 일
- `Assets/_Project/Scripts/Game2Week.asmdef` (Input System 참조).
- `Core/InputReader.cs` — ScriptableObject. UI 맵(Navigate 한 칸씩 이벤트, Submit, Cancel), Player 맵(Move 값), `EnableUI / EnablePlayer / DisableAll`.
- `Battle/BattleStateId.cs` (상태 ID + BattleOutcome), `States/IBattleState.cs` (+ BattleStateBase), `States/BattleStateMachine.cs`,
  `BattleEvents.cs`, `BattleContext.cs`.
- `Tests/EditMode/` — 상태 머신 6개 + 입력 방향 8케이스 = 14개, 배치모드 실행 전부 통과, 컴파일 경고 0.

### 설계 메모
- 상태 머신: Enter 안에서 다시 ChangeState를 불러도 큐에 넣고 현재 전환이 끝난 뒤 처리 → Exit/Enter 순서가 꼬이지 않음.
- StateChanged는 Exit 후, 새 상태 Enter 전에 발생 (UI가 새 상태 기준으로 먼저 정리할 수 있게).
- BattleContext는 상태 머신 자체를 노출하지 않고 ChangeState만 노출.
- BattleController(조립 루트)는 등록할 상태가 생기는 04~06단계에서 작성.

---

## 2026-10-06 — 폰트 + PC 전용 + 03 데이터

### 결정 (사용자 승인)
- Pretendard 폰트 다운로드 허락. **PC 전용**.

### 한 일
- Pretendard Regular/Bold OTF(각 1.5MB, npm 패키지 경로) + OFL 라이선스 → `Art/Fonts/`.
  (gh 경로 `dist/public/static`은 404 — npm `pretendard@1.3.9` 경로가 맞음)
- TMP Essentials 임포트, 동적 SDF 폰트 에셋 2개 생성, Regular를 TMP 기본 폰트로, Bold를 굵기 700으로 연결.
- QualitySettings에서 Mobile 레벨 제거, Mobile URP 에셋 삭제.
  - 실수: 처음 정규식이 PC 항목의 `: 1` 값까지 0으로 바꿈 → 템플릿 tgz의 원본으로 복구 후 다시 적용, 차이가 플랫폼 기본값뿐인 것 확인.
- 03 데이터: `Scripts/Data/` PlayerData, EnemyData(+ActOption, PatternOrder), ItemData, AttackPatternData.
- 런타임 모델: `Scripts/Battle/Model/` PlayerCombatant, EnemyCombatant(살려주기 진행도, 패턴/문구 순환), Inventory.
- 테스트 에셋 + InputReader 에셋 생성. EditMode 테스트 22개 전부 통과.

### Claude가 기본값으로 정한 것 (임시)
- 살려주기: ACT마다 진행도 +N, 적의 기준치 이상이면 살려주기 가능 (Check는 모든 적 공통이라 데이터에 넣지 않음).
- 테스트 수치·문구는 전부 임시 — "바꿔야 할 것"에 등록.

---

## 2026-10-06 — 04 씬 흐름

### 한 일
- Core: `SceneNames`, `SceneLoader`(로딩 중 중복 요청 무시), `GameQuit`(에디터면 플레이 중지), `GameSession` 에셋(현재 적·마지막 결과).
  - TODO의 "GameManager"는 싱글톤 대신 `GameSession` ScriptableObject로 구현.
- UI: `MenuList`(순수 로직, 끝에서 반대편으로 순환), `MenuListView`(템플릿 텍스트 복제, 선택 항목 노랑 + "> "), `MenuNavigator`(InputReader → 커서/확인/취소).
- Flow: `MainMenuController`, `ResultController`, 임시 `BattleFlowStub`.
- 씬 3개를 에디터 스크립트로 생성 (Canvas Scaler 1920×1080), 빌드 목록 교체, 템플릿 SampleScene/Profile 삭제.
  - MainMenu에 주인공 3D 모델 배치. 처음엔 화면 왼쪽(제목과 겹침)에 놓임 — 카메라가 -Z를 보면 +X가 화면 왼쪽 → x=-0.75로 수정.
- 테스트: EditMode 25개(MenuList 3개 추가) + PlayMode 1개(전체 루프 + 화면 캡처) 통과.
- 색 섞임 원인 확정: 배치모드 첫 렌더에만 발생, 두 번째 프레임은 정상.

### Claude가 기본값으로 정한 것
- 메뉴는 끝에서 반대편으로 순환, 선택 표시는 노란색 + "> " (원작 하트 커서 대신).
- 결과 화면 문구: 처치 "승리", 살려줌 "전투 종료", 패배 "GAME OVER" + "* 포기하지 마..."; 메뉴 "다시 도전 / 메인 화면으로 / 게임 종료".
- 메인 화면 타이틀 "타이틀 (가제)".

---

## 2026-10-06 — 전투 방식 변경 + 맵툴 계획

### 결정 (사용자 승인)
- 전투 박스 = **3D 경기장 (B)**, 테스트 적은 임시 도형.
- **새 턴 구조:** 탄막 턴 중 적에게 닿으면 FIGHT/ACT/MERCY, 못 닿고 끝나면 ITEM/넘기기.
- 주인공 3D 모델이 바닥 8방향 이동, 비스듬한 고정 카메라, 점프·대시 없음.
- **맵 크기 가변, 보석 개수 가변, 스테이지 1~n, JSON은 맵툴로만 편집** → 맵툴 제작.

### 한 일
- CLAUDE.md: "Tasks.md와 달라진 점"·DON'T 갱신 ("탄막 회피는 평면" 규칙 → "바닥 8방향, 점프·대시 없음", "스테이지 JSON 손으로 수정 금지").
- `Plans/MapTool_Plan.html` 작성 — 요구사항, 확인 필요 Q1~Q5, JSON 스키마(스테이지당 파일 + stages.json, 체크섬으로 직접 수정 감지),
  툴 와이어프레임, 기능(1차/2차), 검증 규칙, 게임 연결 흐름, 구현 단계 M1~M5, 기존 TODO 영향. 대시보드 상단에 링크.
- TODO.md: "맵툴 · 스테이지" 섹션 추가, 05/06/09 항목을 새 전투 방식에 맞게 수정.
- 05 구현은 보류 — 경기장 크기·배치가 스테이지 데이터에서 오므로 맵툴 데이터 구조를 먼저 확정하는 게 맞음.

---

## 2026-10-06 — 맵툴 결정 + M1 시작 + 첫 커밋

### 결정 (사용자 승인)
- 보석 = 주우면 보상, **자주 나오지 않게**. 맵툴 = Unity 에디터 창. 격자 배치. 스테이지 1→n 순서. 1차 범위 확정.

### Claude가 기본값으로 정한 것
- "자주 나오지 않게" = 보석 위치마다 등장 확률(기본 25%) + 한 턴 최대 1개(`gemRules.maxPerTurn`) + 먹으면 그 전투에서 다시 안 나옴.
- 보석 종류: heal / attack / spare (수치는 데미지 공식과 함께 확정).

### 한 일
- 계획 HTML을 확정본으로 갱신.
- M1 코드 (테스트는 아직): `Scripts/Stages/` StageDefinition(+GridPoint, GemTypes), StageIndex, StageJson(SHA-256 체크섬), StageGeometry, StageValidator, StageRepository; `Data/ContentCatalog`.
- 실수: `init` 접근자 사용 → Unity 컴파일 에러 → `set`으로 수정, CLAUDE.md에 규칙 추가.
- `.gitignore` 정리 (Exports/, .claude/settings.local.json, OS 임시 파일 등), GitHub Desktop 내장 git으로 **첫 커밋 `dce8022`** (274파일, EditMode 25개 통과 상태).

---

## 2026-10-06 — M1 완료 + M2 맵툴 1차

### 한 일
- 문서 커밋 `aaf6a65`.
- M1: `StageTests` (JSON 왕복, 체크섬 변조 감지, 검증 규칙별, 좌표 변환, 저장소). `ContentCatalog` 에셋 생성.
  `stage_001`("1-1 첫 만남", 12×14칸, 보석 3개: 회복·공격·살림, Pattern_Test)과 `stages.json`을 StageRepository 경로로 생성.
- M2: `Assets/_Project/Editor/` (asmdef `Game2Week.Editor`)
  - `StageEditorModel` — 편집 로직 전부 (도구별 클릭, 드래그 = 되돌리기 1번, 스냅샷 되돌리기 최대 100, 스테이지 추가/복제/삭제/순서,
    저장: 오류 있는 스테이지가 하나라도 있으면 아무것도 안 씀, id 바꾸면 파일 이름도 바뀜, 지운 스테이지 파일 삭제).
  - `StageEditorWindow` — IMGUI 창: 툴바(도구·되돌리기·카탈로그 새로고침·다시 불러오기·저장), 왼쪽 목록, 가운데 격자(위=적 쪽),
    오른쪽 속성(id·이름·크기·셀 크기·적 종류·턴 시간·패턴·보석 규칙·선택한 보석의 종류/확률), 아래 검증 결과. Ctrl+Z/Y/S.
  - `ContentCatalogUtility` — 카탈로그 생성/새로고침.
- 테스트: EditMode 70개 전부 통과.

### Claude가 기본값으로 정한 것
- 주인공 시작점·적은 지울 수 없음 (항상 1개씩), 지우개는 보석만. 이미 뭔가 있는 칸에 배치하면 그 항목을 선택.
- 새 보석 기본값: 회복, 등장 확률 25%. 확률은 5% 단위, 턴 시간은 0.5초 단위로 반올림.

### 주의
- 배치모드 실행이 10분 넘게 걸리는 경우 있음 (Unity 클라우드 요청 타임아웃).
- 에디터를 띄워 두면 배치모드(테스트·셋업 스크립트) 불가 → 사용자가 Unity를 닫아야 다음 자동 작업 가능.

---

## 2026-10-06 — 커밋 + M3 게임 연결 + 05 3D 무대

### 사용자
- 맵툴 확인 완료("잘 되네") → 커밋 `34dc202` (첫 시도는 `-m` 여러 줄이 조용히 실패 → 메시지 파일 `-F`로 성공).

### 한 일
- 로직: `GemField`(턴마다 위치별 확률, 한 턴 최대 N개, 먹은 건 다시 안 나옴), `GemRewards` + `GemRewardSettings`(임시 수치),
  `PlayerCombatant` 다음 공격 배율(중첩 X, 한 번 쓰면 초기화).
- `GameSession`: 스테이지 번호·현재 스테이지·카탈로그로 적 찾기. MainMenu 시작 → 1번 스테이지, 재도전 → 같은 스테이지.
- View: `LowPolyMesh`, `BattleArena`(크기 가변 바닥·벽·격자선, 경계 제한), `StageSpawner`, `GemView`(돌기·출렁임·팝),
  `EnemyView`(코드 애니메이션 + 흰색 번쩍), `BattleCameraDirector`(샷 4종, 경기장 크기 맞춤), `BattleEffects`.
- 에셋: 머티리얼 11개, 로우폴리 메시 5개, 프리팹(EnemyView_TestBlob, Gem, Player_Heroine, FX 2개), Enemy_Test.viewPrefab 연결.
- Battle 씬: 경기장·스포너·Cinemachine 카메라·이펙트·`BattleStageBootstrap`, 임시 결과 메뉴는 왼쪽 아래로.
- 테스트: EditMode 78, PlayMode 2(씬 흐름 + 스테이지 빌드·카메라 샷 캡처) 통과. 캡처 공용화 `SceneCapture`.

### 문제 → 해결
- Gem 프리팹 참조가 씬에 비어 있었음 — 프리팹 저장 후 다른 에셋 저장으로 재임포트되며 참조가 끊김 → 경로로 다시 읽어 연결.
- 공격 클로즈업 피격 화면이 바닥 밑으로 — Impulse 기본 세기(~1m) → 진폭 0.06m로 조절.

### Claude가 기본값으로 정한 것
- 임시 적 디자인(보라 로우폴리 덩어리 + 큰 눈 + 뿔), 보석 색(회복 초록 / 공격 주황 / 살림 분홍), 경기장 색(어두운 남색 바닥 + 밝은 벽).
- 카메라: 탄막 턴 55° 내려다보기·FOV 40, 블렌드 0.6초.

### 추가 (같은 날)
- 커밋 `93c2e98` (M3 + 05).
- GitHub 연결: 사용자가 만든 빈 저장소 https://github.com/Parkduck9/Game2Week 를 `origin`으로 추가하고 `main` push (커밋 5개). 이후 `git push`만 하면 됨.

---

## 2026-10-06 — 06 전투 흐름·UI (+ 08, 09 일부) + 서비스 흐름 계획

### 한 일
- 상태 10종 (`BattleStates.cs`): Intro → EnemyTurn → ActionMenu(공격/행동/자비) | ItemMenu(아이템/넘기기) → Fight/Act/Item/Mercy → Victory/Defeat.
  - EnemyTurn: 주인공 리셋·보석 굴리기·턴 표시, 이동, 보석 먹기→보상 팝업, 적 접촉→ActionMenu, 시간 끝→ItemMenu, HP 0→Defeat.
  - Fight는 **임시**로 바로 데미지 (07에서 타이밍 공격으로 교체).
- `IBattleUi`/`IBattleWorld` 인터페이스 + `BattleContext` 확장(전투원·스테이지·보석·보상 설정·종료 콜백), `BattleTexts`(공통 문구).
- View: `PlayerMover`(8방향 2.8m/s, 경계 제한, 통통), `BattleWorld`, `BattlePresentation`(상태→카메라 샷, 피격·종료 연출).
- UI: `BattleUi`, `DialogueBox`(타자 효과), `StatusBar`, `TurnHud`, `PopupText`, `BarView` — Battle 씬에 에디터 스크립트로 배치.
- `BattleController`가 조립 루트, 임시 `BattleFlowStub`·`BattleStageBootstrap` 삭제.
- 테스트: EditMode 89 (흐름 테스트 11개 추가), PlayMode 3 — **실제 키보드 입력 테스트** 포함(↑로 적에게 닿기 → →,Z로 행동 → X로 취소).
- 캡처 확인 후 수정: 메뉴에 탄막 턴 표시가 남음(EnemyTurn.Exit에서 HideAll), 하단 메뉴가 HP 줄과 겹침(메뉴 오른쪽으로).

### 실수 → 규칙
- MonoBehaviour 4개를 한 파일에 둠 → Unity는 파일 이름 = 클래스 이름이어야 씬에 붙음 → 파일 분리, CLAUDE.md 규칙 추가.
- manifest testables 때문에 Input System 패키지 테스트가 같이 돎 → `-assemblyNames`로 우리 테스트만.

### 사용자 요청 (서비스 수준) → 결정
- 메인(새로 시작·이어하기·설정·종료) → 스테이지 선택(해금·최고기록) → ESC 일시정지 → 엔딩 → 메인, 진행·설정 저장, 스토어 설치·삭제(집에서).
- Microsoft Store(MSIX), 최고기록 = 클리어 시간 + 결과 종류, 설정 = 음량·화면 모드/해상도·텍스트 속도, 삭제 시 세이브도 삭제.
- `Plans/Service_Plan.html` 작성 (흐름, 세이브 구조, S1~S8, 집에서 할 일), 대시보드 링크, TODO 섹션.

### Claude가 기본값으로 정한 것
- 메뉴 이름 임시: 공격/행동/자비, 아이템/넘기기, 살펴보기, 살려주기. 하단 대사창 + 왼쪽 아래 HP 줄 + 위쪽 턴 표시 배치.
- 탄막 턴마다 주인공은 시작 칸으로 돌아감.
- 서비스 계획: 새로 시작은 진행만 초기화(설정 유지), 해금은 "클리어한 다음 하나까지"로 계산, 일시정지 메뉴에 설정 추가.

### 다음에 할 일
- 사용자에게 순서 확인: 서비스 흐름 S1~S5 먼저 vs 07(타이밍 공격)·09(탄막) 먼저.
- 이번 06 작업 커밋 여부 확인.


---

## 2026-10-06 — 타이틀 화면 · 스토어 스크린샷 컨셉 이미지

### 한 일
- Codex CLI(`codex exec -i`)에 주인공 3면도 + 전투 캡처(`Logs/scene_battle_2_EnemyTurn.png`)를 참고 이미지로 넣어 생성 (둘 다 1920×1080, 글자·UI 없음).
  - `Reference/title_screen.png` — 별하늘 경기장, 왼쪽 주인공(파이팅 자세) vs 오른쪽 보라 뿔 블롭, 보석 파티클. 위쪽 가운데는 로고 자리로 비움.
  - `Reference/store_screenshot.png` — 비스듬한 위쪽 시점, 적이 쏘는 청록/보라 결정탄을 피해 달리는 주인공 (Microsoft Store 홍보용).

### 결정 (사용자 선택)
- 이미지에 제목 글자는 넣지 않음 → 제목은 IP 확정 후 Unity TMP로 얹음.
- "앱 화면 사진" = 스토어 스크린샷/대표 이미지.

### 다음에 할 일
- (이어서) 사용자에게 순서 확인: 서비스 흐름 S1~S5 먼저 vs 07(타이밍 공격)·09(탄막) 먼저.
- 타이틀 이미지를 메인 메뉴 배경으로 쓸지 확인 (쓰면 `Art/`로 복사 + 로고·메뉴 배치).
---

## 2026-10-06 — 서비스 흐름 S1~S5 + S6 준비

### 사용자
- "서비스흐름부터" → 06 커밋·푸시 `d531f31` 후 진행. "다 되면 커밋하고 다음꺼 실행".

### 한 일
- S1 `Scripts/Save/`: SaveService(진행·설정 분리, 원자적 쓰기 + .bak 복구, 해금 계산, 최고기록), SettingsModel(줄별 값 변경·표시), DisplaySettings(16:9 해상도 목록, 화면 적용 — 에디터에선 안 바꿈).
- S2 메인: 새로 시작(확인 창, 기본 "아니요") · 이어하기(진행 없으면 회색, MenuList 비활성 항목 지원) · 설정 · 종료. `ConfirmPopup`·`SettingsPanel` 프리팹(`Prefabs/UI/`).
- S3 `StageSelect` 씬: 해금된 스테이지 + 최고기록/결과.
- S4 ESC 일시정지 `PauseMenu`: Input에 System 맵(ESC/Start) 추가, timeScale 0, 전투 UI 끄기, 풀면 입력 모드 복원. DialogueBox는 다시 켜질 때 입력 재구독. 텍스트 속도 설정 → 대사 속도.
- S5 결과: 클리어 시간·신기록, 메뉴 동적(다음 스테이지 | 엔딩으로 · 다시 도전 · 스테이지 선택). `Ending` 씬. 빌드 목록 5개.
- S6 준비: `BuildScript`(Tools ▸ Build, 배치모드), `Tools/build_windows.ps1`, `Tools/msix/AppxManifest.template.xml`, `Tools/package_msix.ps1`.
- 테스트: EditMode 102, PlayMode 5 (서비스 흐름 2개, ESC 일시정지 실제 키 입력 포함) — 임시 세이브 폴더 사용.
- 캡처 확인 후 수정: 세로 메뉴 줄바꿈(LayoutGroup 너비 채우기), 0초 클리어 기록(최소 0.1초).

### Claude가 기본값으로 정한 것
- 해상도 목록은 모니터가 지원하는 16:9만, 음량 10% 단위, 텍스트 속도 느림 0.6× / 보통 / 빠름 1.8×.
- 엔딩 문구 임시("소심해 보여도, 할 땐 하는 아이였다."), 스테이지 선택 커서는 가장 최근에 열린 스테이지.
- MSIX 기본 Identity `Game2Week.Dev` / `CN=Game2Week Dev`(테스트용), 임시 로고는 스크립트가 생성(남색 바탕 노란 마름모).

### 주의
- 회사 이름(DefaultCompany)이 세이브 경로에 들어감 → 출시 전 확정.
- 동적 폰트 에셋이 플레이할 때마다 바뀌어 git 변경으로 잡힘.

### 다음에 할 일
- S6: 여기서 빌드 + MSIX 포장(서명 없이) 검증, 집에서 할 일 안내서(`Plans/Store_Guide.html`).
- 그다음 07 타이밍 공격 / 09 탄막 패턴.
---

## 2026-10-06 — S6 빌드·MSIX 포장 검증

### 한 일
- `Tools/build_windows.ps1 -Version 0.1.0` → Builds/Windows (104MB, 97초, Mono x64).
- `Tools/package_msix.ps1 -Version 0.1.0.0 -NoSign` → `Builds/Msix/Game2Week.Dev_0.1.0.0_x64.msix` (40.8MB). makeappx가 매니페스트 검증 통과, `_DoNotShip` 폴더 제외 확인.
- 빌드된 exe를 배치모드로 15초 실행 → 로그에 오류 없음.
- 집에서 할 일 안내서 `Plans/Store_Guide.html` (준비 → 빌드·포장 → 인증서 신뢰·설치 → 체크리스트 → 삭제 후 세이브 삭제 확인 → 스토어 제출 → 문제 해결), 대시보드 링크.

### 문제 → 해결
- `.ps1`을 BOM 없는 UTF-8로 저장 → PowerShell 5.1이 한글을 깨뜨림 (출력·MSIX 표시 이름) → BOM 붙여 다시 저장, 규칙 추가.

### 여기서 안 한 것 (보안 설정이라 사용자가 집에서)
- 테스트 인증서 생성·신뢰(LocalMachine\TrustedPeople), 실제 설치/삭제, 파트너 센터 제출.

### 다음에 할 일
- 07 타이밍 공격 / 09 탄막 패턴 — 착수 전 결정: 데미지 공식·수치, 탄막 패턴 모양.
---

## 2026-10-06 — 07 타이밍 공격 + 09 탄막 패턴

### 결정 (사용자)
- 데미지 = 공격력 × 정확도 배율(가장자리 ×0.5 ~ 정중앙 ×2.0) − 방어 (수치는 임시값 유지).
- 첫 탄막 패턴 = 방사형 결정탄.

### 한 일
- 07: `TimingGauge`(순수 로직), `TimingGaugeView`(커서·구간·결과 표시, 일시정지 대응), `BattleFormulas.FightDamage`, FightState를 게이지 결과로, 데미지 팝업·`EnemyHealthBar`. 피격 파티클을 카메라 쪽으로.
  - 보석 공격 강화는 놓쳐도 소모 (한 번의 공격 기회에 쓰인 것).
- 09: `IAttackPattern`/`PatternContext`/`PatternRunner`, `Bullet`, `RadialBurstPattern`, 결정탄 프리팹 2종(청록·보라 발광 팔면체), `Pattern_RadialBurst` 프리팹 → Pattern_Test 연결.
  - BattleWorld: 패턴 시작/정리, 맞은 데미지 모으기, 무적 1초 + 깜빡임. EnemyTurnState가 데미지를 HP에 반영.
  - `AttackPatternData.boxSize` 제거 (턴 길이·경기장 크기는 스테이지가 정함).
- 테스트: EditMode 106(게이지·공식·탄막 흐름·방향 계산 추가), PlayMode 5(탄 발사·가만히 있으면 맞음·게이지·공격 확인).

### Claude가 기본값으로 정한 것
- 게이지 1.3초, 판정 구간 색(완벽 ±3% 노랑 / 좋음 ±10% / 보통 ±25%), 결과 문구(완벽!/좋아!/아슬아슬/MISS).
- 방사형: 1.1초 간격 10발, 13° 회전, 속도 2.4m/s, 첫 발 0.7초 뒤. 탄 반지름 0.14m, 무적 1초.

### 다음에 할 일
- 10·11 검증: 처치·살려주기·패배 루트 실제 플레이, 두 번째 적/패턴을 에셋 복제만으로 추가해 보기.
- 사용자 결정 대기: 게임 이름·회사 이름, Flee 여부, 3D 모델 출처, 동적 폰트 처리.
---

## 2026-10-06 — 10·11 검증 완료

### 한 일
- 10: 패배 연출(주인공 파편 + 사라짐 + 흔들림, 카메라는 전체 샷), `BattlePresentation`에 주인공 연결.
- 11-1 `BattleRouteTests`: InputTestFixture 키보드 봇이 1-1을 **처치(6.1초) · 살려주기(5.7초) · 패배(6.8초, timeScale 3)** 세 결말까지 실제 입력으로 플레이 → 결과 화면·세이브 기록 확인.
- 11-2 확장성: 에디터 API만으로(코드 수정 0) `Enemy_Test2` + `EnemyView_TestBlob_Orange`(프리팹 Variant), `Pattern_Test2` + `Pattern_RadialBurst_Fast`(Variant: 6발·0.75초·3.3m/s·25°), `stage_002`(16×18칸, 보석 2, StageRepository 저장) → `Stage2_FromAssetsOnly_Works` 통과. 스테이지가 2개라 서비스 흐름 테스트도 "다음 스테이지 → 1-2 → 엔딩"으로 갱신.
- 11-3 `BattleAudio`(효과음 연결 지점), 11-4 `Plans/Resource_List.html`(교체 리소스·규격·위치·출처), 대시보드 링크.
- 테스트: EditMode 106, PlayMode 10 전부 통과. **완료 기준(DoD) 5개 모두 체크.**

### 다음에 할 일
- 사용자 결정 대기: 게임·회사 이름(세이브 경로!), Flee, 3D 모델 출처, 동적 폰트, 사운드 출처.
- 집에서: `Plans/Store_Guide.html` 순서대로 설치·삭제 확인 → 스토어 제출.
- 남은 다듬기: 클로즈업 피격 파티클 크기, 적 말풍선, 메뉴 효과음·BGM 연결 지점, 맵툴 2차(M5), 주인공 모델 v2·리깅.
---

## 2026-10-06 — 버그: 에디터에서 처음 Play 누르면 키보드가 안 먹음

### 원인
- Input System 기본 설정 `PointersAndKeyboardsRespectGameViewFocus` — Game 창에 포커스가 없으면(Play 직후 Scene/Hierarchy에 포커스가 남은 경우) 키보드 입력을 무시. 로그에 오류 없음.
- 자동 테스트(InputTestFixture)는 포커스와 무관해서 못 잡았음.

### 수정
- `Assets/_Project/Input/InputSystemSettings.asset` 생성, `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`, 프로젝트 Input System 설정으로 등록 (빌드 게임엔 영향 없음).
- `Editor/InputSettingsSetup.cs`([InitializeOnLoad]) — 에디터를 켤 때 설정이 빠져 있으면 다시 맞춤. 배치모드용 `ApplyAndExit`.
- Editor asmdef에 Unity.InputSystem 참조 추가.
- 첫 적용 시도는 사용자가 Unity를 여는 중이라 프로젝트 잠금으로 실패 → 사용자가 끈 뒤 배치모드로 적용.
- 테스트: EditMode 106, PlayMode 10 통과.

### 다음에 할 일
- 사용자 확인: Unity 켜고 MainMenu에서 Play → Game 창 클릭 없이 방향키·Z 동작하는지.
---

## 2026-10-06 — 스테이지 3개 자동 생성

### 사용자 요청
- "스테이지도 자동으로 3개 정도 생성".

### 한 일
- 맵툴과 같은 경로(StageRepository + StageValidator)로 생성 — JSON 직접 작성 아님, 체크섬 포함:
  - `stage_003` 1-3 좁은 방: 10×12칸(5×6m), 테스트 적, 방사형, 턴 7초, 살림 보석 1.
  - `stage_004` 1-4 넓은 들판: 24×20칸(12×10m), 주황 적, 빠른 결정탄, 턴 10초, 보석 3(회복·공격·살림).
  - `stage_005` 1-5 결정의 방: 18×18칸(9×9m), 주황 적이 왼쪽 위 구석·주인공 오른쪽 아래(대각선 접근), 두 패턴 번갈아, 한 턴 보석 최대 2.
- 테스트: `AllStages_BuildFromData`(전 스테이지 진입 → 경기장 크기·보석 수·적 위치·탄막 실행 확인 + 캡처 `Logs/scene_stage_*`).
- 테스트 정리: Battle 씬을 바로 열던 테스트가 이전 테스트의 스테이지를 이어받아 실패 → `SceneFlowTests.EnterStage(index)`로 항상 스테이지 선택을 거쳐 들어가게 통일. "마지막 스테이지 → 엔딩" 테스트도 스테이지 수와 무관하게.
- EditMode 106, PlayMode 11 통과.

### Claude가 기본값으로 정한 것
- 스테이지 이름·크기·배치·보석·턴 시간 (난이도: 좁음 → 넓고 빠름 → 대각선 + 패턴 섞기). 맵툴에서 자유롭게 고치면 됨.

### 다음에 할 일
- 사용자: 키보드 수정 확인(Game 창 클릭 없이), 새 스테이지 플레이해 보기 → 커밋 요청 시 커밋·푸시 (키보드 수정 + 스테이지 3개 미커밋 상태).

---

## 2026-10-06 — Codex: 기존 5개 뒤에 맵 3개 추가

### 한 일
- 프로젝트 MD 4개(CLAUDE.md, Tasks.md, TODO.md, WORKLOG.md)를 모두 읽고 기존 스테이지 5개와 맵툴 구조 확인.
- 임시 Unity 에디터 스크립트에서 StageEditorModel의 Load → AddStage → Edit → ValidateCurrent → SaveAll 경로로 생성. JSON 직접 편집 없이 기존 1~5번을 유지하고 stages.json 뒤에 6~8번 추가. 생성 스크립트와 meta는 실행 후 제거.
- `stage_006` 1-6 가로지르는 빛: 26×12칸(13×6m), 왼쪽 아래 시작 → 오른쪽 위 보라 적, 기본 방사형, 턴 9초, 회복·공격 보석 2곳.
- `stage_007` 1-7 긴 접근: 12×28칸(6×14m), 아래 시작 → 위쪽 주황 적, 빠른 방사형, 턴 10초, 회복·살림 보석 2곳.
- `stage_008` 1-8 중심의 결정: 22×22칸(11×11m), 오른쪽 아래 시작 → 중앙 주황 적, 기본/빠른 방사형 순차 교대, 턴 8초, 공격·회복·살림 보석 3곳.
- 전체 8개를 실제 ContentCatalog로 검증: 데이터 오류·경고 없음, 스테이지 및 목록 체크섬 정상. PlayMode 11개 전부 통과(Logs/maps_playmode.xml), AllStages_BuildFromData가 8개 모두 진입·경기장·적·보석·패턴을 확인.
- Logs/scene_stage_6_stage_006.png, scene_stage_7_stage_007.png, scene_stage_8_stage_008.png를 열어 경기장·플레이어·적·보석·탄막 및 카메라 화면 확인.

### 결정
- 이번 요청은 기존 맵에 3개를 더 추가하는 것으로 적용, 총 8개. 기존 적/패턴만 재사용하고 크기·접근 방향·보석 배치로 변화.
- 새 맵 보석 등장 확률은 각 25%, 한 턴 최대 1개. 기존 순차 해금·다음 스테이지·마지막 스테이지 엔딩 흐름 사용.
- 커밋·푸시는 실행하지 않음.

### 주의
- 제한된 실행 환경에서는 Unity 라이선스 IPC 연결이 거부되어 첫 실행이 진행되지 않음. 해당 실행을 종료하고 승인된 일반 실행 환경에서 생성·PlayMode 검증 완료.
- 기존 씬의 AudioListener 없음 경고를 TODO에 기록(사운드 연결 시 확인).

### 다음에 할 일
- Unity Tools ▸ Stage Editor에서 1-6~1-8 확인·수정, 게임에서는 1-5 클리어 후 순서대로 해금하여 플레이.
- 실제 플레이 느낌에 따라 맵 크기·턴 시간·보석 배치 조정.

---

## 2026-10-06 — Codex: 스테이지 선택 목록 화면 배치 수정

### 한 일
- 사용자 증상: Unity Play → 메인 → 새 게임 → 선택 창에 항목이 안 보임.
- 실제 Editor 로그 확인: StageSelect 씬 전환은 실행되고 스크립트 예외 없음. 새 게임 재현 테스트에서도 스테이지 1개 및 TMP 글자 메시가 정상 생성됨.
- StageList RectTransform이 가운데 47% 기준 고정 1400×620이라 Game 창이 가로로 넓고 세로로 작으면 첫 항목이 화면 위로 나갈 수 있음. 기존 일반 Play 캡처에서도 항목이 제목에 붙어 올라가는 것을 확인.
- StageSelect.unity의 StageList만 수정: 화면 가로 12~88%, 세로 18~72%에 맞춰 늘어나는 앵커, 고정 크기 제거. 제목 아래부터 목록이 배치되어 작은 Game 창에서도 첫 항목이 표시됨.
- StageSelectVisibilityTests 추가: 실제 새 게임 → 선택 씬 흐름, 첫 항목 크기·글자 메시·cull·Canvas 내부 좌표 검증. 일반 Unity 렌더링에서 1개 통과(Logs/stage_visibility_fixed.xml), 화면 확인(Logs/stage_select_overlay.png).

### 주의
- 사용자 화면에서 항목이 완전히 사라지는 정확한 Game 창 크기는 직접 확인하지 못함. 정상 데이터 로드와 발견한 화면 배치 문제를 구분해 확인함.
- 기본 순차 해금 유지: 새 게임은 1-1부터 표시되고 클리어하면 다음 맵이 열림.
- 기존 SceneCapture는 카메라 Canvas 전환과 ForceUpdateCanvases를 사용하므로, 이번 테스트는 일반 Overlay 화면을 따로 캡처함. 배치모드에서는 WaitForEndOfFrame/화면 캡처를 생략.

### 다음에 할 일
- 사용자가 Unity에서 Play 중지 후 다시 새 게임으로 진입하여 목록 표시 확인. 1-1을 선택해 전투 진입.
- 계속 비어 있으면 Game 창 크기와 Console 오류 및 화면 확인으로 추가 원인 조사.

---

## 2026-10-06 — Codex: 실제 에디터 Play에서 빈 맵 목록 원인 수정

### 한 일
- 앞선 화면 배치 수정만으로 해결됐다는 판단을 정정. 사용 중인 Unity 창에서 같은 증상을 직접 확인하고 임시 에디터 진단으로 실제 런타임 값 기록.
- 수정 전 Logs/stage_select_live.txt: 기본 폴더 목록은 8개, GameSession.StageCount는 0, StageDirectoryOverride는 빈 문자열, 선택 항목 0개. 게임은 빈 경로를 유효한 override로 받아 프로젝트 루트의 없는 stages.json을 읽었음.
- GameSession.StageDirectoryOverride에 `[field: NonSerialized]`를 추가해 에디터 도메인 리로드의 자동 프로퍼티 복원에서 제외. Repository는 string.IsNullOrWhiteSpace이면 StageRepository.DefaultDirectory를 사용하도록 수정.
- 수정 후 동일 사용자 에디터에서 Play 재실행: override=null, StageCount=8, 선택 항목 1개, Item_0 메시 및 위치 정상. 사용자 입력에 따른 Battle 진입과 경기장·주인공·적·탄막 화면도 직접 확인.
- GameSessionStagePathTests 추가: null/빈 문자열/공백 override로 프로젝트 맵 로드·1번 진입, 명시적 다른 폴더 override 유지(4케이스). 이번에는 사용자가 에디터에서 플레이 중이므로 배치 테스트 실행은 하지 않고 실제 에디터의 재현 전후로 검증.
- computer-use 스킬로 실제 Unity 창에서 재현·확인. 임시 진단 스크립트 및 meta 정리.

### 원인 및 테스트 차이
- 실제 에디터에서는 편집 모드에 미리 로드된 ScriptableObject가 Play 진입 시 다시 복원되어 테스트용 문자열 프로퍼티의 null이 빈 값으로 바뀔 수 있었음. 기존 테스트는 Play 중 씬을 로드해 이 초기 상태 차이를 놓침.
- 화면 비율 수정은 별도 배치 개선이며, 이번 빈 목록의 근본 원인은 맵 경로 선택 로직이었음.

### 다음에 할 일
- 현재 게임에서 정상 플레이 진행. 에디터를 닫은 뒤 테스트 실행 시 GameSessionStagePathTests 4케이스와 기존 씬 흐름 검증 포함.

---

## 2026-10-06 — Codex: 패턴 누적 · 발사 간격 · 3D 액션 설계

### 사용자 요청
- 난이도가 올라가면 패턴을 하나씩 늘리고 쏘는 시간을 줄이는 밸런스를 MD·HTML로 먼저 설계한 뒤 구현 방법 검토.
- 탑다운 느낌에서 3D 액션처럼 피하는 전투로 전환. 이후 작업 내용도 MD·HTML에 반드시 반영.

### 한 일
- 현재 PlayerMover(바닥 이동), BattleCameraDirector(55도 전체 샷), InputReader, Bullet(XZ 충돌), BattleWorld(접촉/무적), PatternRunner/AttackPatternData 구조를 확인.
- `Plans/Action_Balance_Plan.md`: 8단계 누적 1→8종, 원형 기준 발사 간격 1.40→0.65초, 동시 위험 최대 2종, 패턴별 최소 간격·예고·발사량 예산·안전 통로 설계.
- 낮은 추적 카메라, 카메라 기준 이동, Shift 대시/Space 점프, 실제 높이·연속 충돌, 적 접촉 조건, 공중 보석 획득 제한, 일시정지/종료 정리 제안.
- A1~A6 제작 순서와 데이터 구조·검증 항목 작성. 첫 구현은 한 맵의 카메라/대시/점프 감각 확인을 제안.
- `Tools/render_action_plan.mjs`: MD의 본문/표와 단계 데이터를 읽어 오프라인 HTML 생성. `Plans/Action_Balance_Plan.html`에 단계 선택·누적 목록·발사 간격 비교·3D 회피 개념도 제공.
- TODO에 설계 완료와 미구현 작업을 구분, TodoList.html에 계획 링크 추가. CLAUDE.md에 최신 액션 전환 요청과 문서 동기화 규칙 기록.

### 문서 검증
- HTML 생성 성공: 8단계 표·11개 본문 섹션. MD 본문과 수치를 생성 입력으로 사용해 중복 편집을 피함.
- 실제 브라우저에서 첫 단계 표시, 1-4 선택(4종·1.00초·동시 1종), 1-8 선택(8종·0.65초·동시 2종), 누적 목록과 개념도 렌더링 확인. 브라우저 오류 로그 없음.
- 기존 PowerShell HttpListener 서버는 현재 실행 환경에서 지원되지 않아 미리보기는 Node의 127.0.0.1:8790 임시 정적 서버로 확인. HTML 자체는 서버/외부 라이브러리 없이 열 수 있음.
- 이번 변경은 문서/HTML 생성 도구만으로 Unity 테스트는 실행하지 않음. 수치의 게임 밸런스 검증은 아직 진행하지 않음.

### 해석과 범위
- "쏘는 시간이 줄어듦"은 발사 간격 감소로 해석했다. 턴 제한 시간/예고 시간을 일괄 단축하지 않는다.
- 수치·조작·8종 패턴은 검증 전 제안. 게임 코드·씬·스테이지 JSON·세이브는 이번 작업에서 변경하지 않는다. 커밋·푸시 없음.
- 기존의 점프/대시 금지 방향은 최신 액션 전환 요청에 대한 설계 제한으로 사용하지 않으며, 현행 구현과 앞으로의 제안을 구분했다.

### 다음에 할 일
- 사용자와 MD/HTML의 조작·카메라·패턴 누적·발사 간격 검토. 후속 구현 첫 범위로 A1~A2(한 맵의 3D 회피) 제안.
- 구현을 진행할 때 실제 적용/검증된 내용으로 MD와 HTML 재생성, TODO·WORKLOG를 함께 갱신.
- 기존 GameSessionStagePathTests 4케이스는 에디터를 닫은 뒤 후속 테스트에서 실행.

---

## 2026-10-06 — Codex: 허리 카메라 · 색 대응 · 그래프 패턴 요구 반영

### 사용자 지정
- 적에게 다가가 접촉하면 공격/행동 메뉴를 여는 기존 전투 유지.
- 다크소울·세키로·진삼국무쌍 같은 캐릭터 뒤 허리 카메라.
- 노랑은 쳐내거나 Shift 회피. 이후 빨강은 정지 모션 버튼과 움직이지 않기, 파랑은 움직이기.
- 난이도에 따라 공격 궤적/측면 방향 다양화, 가까워질수록 커지는 경고 소리, sin 같은 그래프 패턴.

### 한 일
- Action_Balance_Plan.md를 v2로 갱신하고 최신 요구를 0절에 정리. v1의 점프 중심 조작/제작 순서보다 최신 내용이 우선함을 명시.
- 색 대응·곡선 궤적·발사 방향을 분리, 빨강/파랑 모순 조합 금지, 실제 이동 판정, 사인 궤적 식과 속도/곡률·연속 충돌 고려, 3D 거리 경고음 상한 제안.
- 카메라 회전/록온, 쳐내기·정지 버튼, 빨강 정지 기준, 쳐내기 효과, 점프와 첫 제작 범위, 그래프 편집 범위 질문 제시.
- TODO·CLAUDE의 최신 방향과 미확정 항목 갱신. HTML도 MD 기준으로 재생성. 게임 코드/씬/스테이지/세이브는 변경하지 않음.

### 다음에 할 일
- 질문 답변을 반영해 조작·판정·첫 제작 범위를 확정하고 MD/HTML 갱신.
- 우선 한 맵에서 허리 카메라·노랑 쳐내기·Shift 회피·사인 궤적·경고음을 검증하는 순서 제안. 점프는 답변 전 필수 기능으로 간주하지 않음.

---

## 2026-10-06 — Codex: 회피/쳐내기/점프와 패턴 제작 툴 답변 확정

### 사용자 답변
- 허리 카메라: 마우스 회전 + 적 록온. 우클릭 쳐내기, Shift 회피, Ctrl 정지 자세가 빨강 통과의 필수 조건.
- 노랑은 플레이어 위치에 랜덤을 섞어 조준하고 점프로도 회피. 쳐내는 동안에도 입력한 방향으로 이동.
- 탄 궤적뿐 아니라 발사 간격과 속도 변화를 직접 제작할 툴, 총 8개 패턴.
- 쳐내기는 오른손으로 왼쪽 탄을 오른쪽 어깨 뒤로, 오른쪽 탄을 왼쪽 어깨 뒤로 자연스럽게 흘려보내기.

### 한 일
- 설계 v3에 확정 조작/판정과 오른손 쳐내기 동작, 캐릭터 기준 좌우, 성공 탄의 재피격 방지와 짧은 곡선 경로 기록.
- Unity Pattern Editor 제안: 8종(직선·사인파·지그재그·포물선·원호·나선·8자·자유 곡선), 3D 미리보기·속도/간격 그래프·시간 스크럽·시드·에셋 저장·맵툴 연결.
- 최신 8단계 표는 궤적 누적 제안으로 갱신, v1 공격 형태 예시와 구분. 마우스 가운데 록온/Space 점프는 기본안, 세부 수치는 검증 전 제안.
- TODO·CLAUDE 최신 내용 반영, HTML 생성 도구의 비교 화면도 최신 표현으로 갱신. 게임 구현/씬/스테이지 변경은 아직 없음.

### 다음에 할 일
- 필수 방향 질문은 해결. 첫 구현은 한 맵의 허리 카메라·마우스 회전/록온·우클릭 쳐내기·Shift 회피·점프부터 진행하는 안.
- 이후 노랑 랜덤 조준/사인파/측면/경고음, 패턴 제작 툴과 8종, 빨강/파랑 확장, 맵별 밸런스를 순서대로 구현. 작업마다 MD·HTML·TODO·WORKLOG를 갱신.

---

## 2026-10-06 — Codex: 1단계 액션 기반 완료, 2단계 이어서 착수

### 사용자 요청과 계획
- MD/HTML에 반영 후 제작 계획을 작성하고 1단계까지 구현. 구현/검증 도중 사용자가 "다음으로 지나가자" 요청하여 1단계 검증 후 2단계도 이어서 진행.
- 설계 MD 1절에 실제 제작 계획 1~5단계와 완료 기준, 2단계 구체 범위 기록하고 HTML 재생성.

### 1단계 한 일
- PlayerActionSettings SO·PlayerMotorModel·PlayerMover/PlayerActionView: 이동·회피·점프·타이밍 쳐내기, 오른손 임시 큐, 이동 중 쳐내기, 콜다운/무적/입력 버퍼.
- InputReader/Input Actions: WASD·방향키·Shift·Space·우클릭·마우스 delta·휠 클릭 록온, 메뉴/일시정지 입력 분리.
- BattleCameraDirector: 허리 추적·마우스 회전·록온·카메라 기준 이동. 종료/메뉴 커서 복원, 가까운 장식 경계벽 가림 개선과 어깨 오프셋 조정.
- Bullet/AttackGeometry: 높이/상대 이동 충돌, 성공 탄은 반대 어깨를 거쳐 후방 곡선으로 흘리며 자해 제외. 적 접촉은 지상·대시 종료 상태, 점프 중 바닥 보석 획득 방지.
- YellowTrainingPattern과 노랑 탄·재질·설정 에셋, 1-1 직선 조작 연습 연결. StageEditorModel 저장 경로, 기존 8개 맵 배치/턴 시간 유지.
- ActionPhaseOneSetup을 통해 실제 에셋/씬 연결. 전투 씬 AudioListener 추가.

### 검증
- Logs/action_phase1_editmode.xml: EditMode 119개 통과(기존 + 경로 회귀 + 액션/충돌).
- Logs/action_phase1_playmode.xml: PlayMode 15개 통과(실제 입력/카메라/점프/양쪽 쳐내기/메뉴·일시정지 + 기존 8개 맵/결말).
- 실제 렌더 이미지 확인: 완전 전환 후 허리 카메라 확인. 초반 블렌드 캡처는 아직 등장 카메라 구도가 섞이므로 구분. 캐릭터 가림/경계벽 문제를 후속 조정하고 2단계 회귀에서 재확인 예정.
- 첫 제한 실행의 라이선스 IPC 문제는 본인이 띄운 배치만 종료하고 일반 실행 환경으로 진행. PlayMode 첫 시도는 사용자 에디터가 열려 프로젝트 잠금으로 종료되었고, 사용자 에디터 종료 후 재시도해 통과. 사용자 에디터를 강제로 종료하지 않음.

### 2단계 진행 중
- 랜덤 목표 고정·사인파 탄·측면 교대, 최대 3개 거리 경고음·화면 밖 표시, 회피/쳐내기 준비 상태 HUD 구현.
- ActionPhaseTwoSetup으로 1-2 사인파·1-3 측면 연결. 커브/속도/간격 에디터, 나머지 6종, 빨강/파랑은 다음 단계.
- EditMode 및 PlayMode 회귀 검증과 최종 화면 확인을 이어서 수행.

### 다음에 할 일
- 2단계 검증을 마무리하고 적용 수치/제한을 MD·HTML·TODO·WORKLOG에 기록한 뒤 사용자에게 결과 전달.
- 다음 3단계는 Unity Pattern Editor와 총 8종 궤적·속도/발사 간격 그래프 제작.

---

## 2026-10-06 — Codex: 2단계 노랑 사인파·측면·경고 완료

### 한 일
- WaveTrajectory/SineBullet: 사인파 이동과 고정 시드 랜덤 조준, 공통 높이 충돌/쳐내기 재사용. 연속 이동은 최대 0.015초 구간으로 나눠 탄과 플레이어 상대 위치를 평가.
- YellowTrainingPattern: 예고 시 목표 고정, 적→왼쪽→오른쪽 발사 교대, 궤적 예고선. 에셋으로 직선/사인파와 측면/오차 설정.
- ThreatFeedbackModel/View: 가까운 위험 최대 3개, 거리 경고음과 화면 밖 노랑 방향 표시, 효과음 설정/일시정지/턴 종료 정리. 임시 합성 소리는 교체 가능.
- ActionStatusView: 회피/쳐내기 준비·쿨다운과 록온 상태 표시. 어깨 오프셋과 장식 경계벽 숨김으로 적 가시성 개선.
- ActionPhaseTwoSetup/StageEditorModel: 1-1 직선 유지, 1-2 사인파/랜덤, 1-3 측면 교대 연결. 맵 배치·보석·턴 시간 유지, 사용자 세이브 미변경.
- 실제 적용 수치/임시 리소스/남은 범위를 설계 MD에 기록하고 HTML 재생성. TODO·CLAUDE 갱신.

### 검증과 제한
- EditMode 125/125 통과: Logs/action_phase2_editmode.xml.
- PlayMode 16/16 통과: Logs/action_phase2_playmode_retry.xml. 새 패턴/경고/일시정지/종료, 실제 입력/카메라/쳐내기, 8개 맵과 처치/살려주기/패배 및 서비스 흐름.
- 첫 전체 실행의 기존 입력 테스트 5개 실패는 새 씬 테스트와 입력 fixture의 환경 수명 혼합 때문. ActionPhaseTwoTests를 같은 InputTestFixture로 격리 후 전체 재실행 통과.
- 실제 렌더에서 허리 카메라·적 가시성·측면 예고/방향 표시 확인. 화면 위 경고 캡처는 투영 검사 시험 탄을 포함하므로 일반 공격과 구분.
- 손은 리깅 전 임시 큐, 소리는 임시 합성 자원. 발사 간격 1.40/1.50/1.65초는 학습용이며 단계별 간격 감소·8종 누적은 5단계 작업.

### 다음에 할 일
- 3단계 Unity Pattern Editor: 총 8종 궤적과 속도/발사 간격 그래프, 런타임과 같은 평가 경로, 3D 미리보기·시드/시간 스크럽·에셋 저장/맵툴 연결 구현 및 검증.
- 이후 빨강 Ctrl 정지 자세·파랑 실제 이동 판정, 8개 맵 누적/간격 밸런스. 단계마다 MD·HTML·TODO·WORKLOG 갱신.

---

## 2026-10-06 — Claude: 병렬 작업 계획 + 0단계 연결 지점

### 사용자 요청
- 3단계부터 Claude와 Codex가 나눠서 작업. 겹치는 파일이 없도록 계획을 다시 세움. 담당은 처음 제안에서 바꿔 **Codex = 3단계 Pattern Editor, Claude = 4단계 빨강·파랑**.

### 한 일
- 직전에 쌓인 작업 커밋·푸시: `660e80a` (1·2단계 액션, 맵 8개, 키보드 포커스, 빈 목록 수정). 커밋 전 EditMode 125 재확인, 동적 폰트 에셋은 되돌림.
- 0단계 연결 지점 (3·4단계가 같은 파일을 고치지 않도록):
  - `ITrajectory` + `TrajectoryLaunch`: 발사 기준값 + 경과 시간 → 위치 (상태 없음, 게임·미리보기 공용).
  - `Bullet.Launch(position, velocity, trajectory)`, `Bullet.Color`(AttackColor), `Bullet.Parryable` = 노랑만.
  - `IHitRule` + `PatternContext.ShouldHit(color)`: 규칙이 없으면 항상 피해 (현재 동작 유지).
  - `IThreatSource.CollectThreats(List<ThreatPoint>)`: 위치 + 색. 화면 밖 표시 문구는 `BattleTexts.AttackColorName`.
  - `YellowTrainingPattern`에 `attackColor` 필드 (기본 노랑).
- `BulletContractTests` 3개 추가. EditMode 128/128, PlayMode 16/16 (`Logs/phase0_*.xml`).
- `Plans/Parallel_Work_Plan.md`(+HTML): 파일 담당표, worktree 폴더, 커밋·합치기 규칙, 단계별 완료 기준, Codex 지시문.
- `Tools/render_plan.mjs`: 범용 계획 MD → HTML. TodoList.html에 링크, CLAUDE.md에 병렬 규칙, TODO 갱신.

### 결정
- 병렬 기간에는 TODO·WORKLOG·CLAUDE·설계 MD를 main에서 합칠 때만 갱신, 각자 `Plans/Parallel/<이름>_Log.md`에 기록.
- 빨강·파랑 맵 연결과 색 조합 규칙은 5단계 (두 단계 합친 뒤).

### 다음에 할 일
- 사용자: 계획 확인 → 0단계 커밋 요청 → worktree 두 개 생성(Claude가 해도 됨) → Codex에 계획 7절 지시문 전달.
- Claude: `Game2Week-claude`(브랜치 `phase4-color-rules`)에서 4단계 시작.
---

## 2026-10-06 — Claude: worktree 생성 + 이후 단계 분할 계획 + 작업량 표시

### 한 일
- 0단계 커밋·푸시 `0d3a146`.
- worktree 두 개: `..\Game2Week-codex` (`phase3-pattern-editor`), `..\Game2Week-claude` (`phase4-color-rules`). 원래 폴더의 Library(2.1GB)를 복사해 첫 실행 약 90초. 두 폴더 모두 EditMode 128/128.
- 계획 8절: 영역별 기본 담당 (Codex = 패턴·맵·전투 이펙트·이미지, Claude = 플레이어·입력·카메라·판정·흐름·UI·세이브·오디오·캐릭터·출시)과 5~8단계 분할표 (단계마다 N-0 연결 지점을 main에 먼저).
- 계획 9절 + `Plans/Work_Effort.md`: 사람 기준 작업량 표 (중급 Unity 개발자 1명 추정, 8시간=1일). WORKLOG 기록마다 시간을 추정해 채움 — 현재 Claude 205시간(25.6일), Codex 87시간(10.9일).
- `TodoList.html`에 "작업량 — 사람 기준" 카드: 비율 막대, 사람별 막대, 영역별 표, 최근 작업. 표가 없어도 대시보드는 동작.

### 주의
- worktree에서 Unity를 돌리면 `ProjectSettings/EditorBuildSettings.asset`·`ShaderGraphSettings.asset`이 줄바꿈만 바뀌어 M으로 보임 (내용 diff 없음) — 커밋에서 빼거나 무시.
- 이번 문서 변경(8·9절, Work_Effort, 대시보드)은 아직 커밋 전이라 두 worktree 브랜치에는 없음. Codex 지시문(7절)에는 작업량 기록 규칙을 직접 넣어 둠.

### 다음에 할 일
- 사용자: 이번 문서 변경 커밋 여부 결정 (커밋하면 두 브랜치도 main으로 맞춤) → Codex에 7절 지시문 전달.
- Claude: `Game2Week-claude`에서 4단계 시작.
---

## 2026-10-06 — Claude: 4단계 빨강·파랑 판정 (병렬) → main 합침

### 한 일
- `Game2Week-claude`(브랜치 `phase4-color-rules`)에서 구현·검증 후 커밋 `05f836f`, 사용자 요청으로 main에 합침 `58d2e54` (no-ff). Codex 3단계는 `Game2Week-codex`에서 진행 중.
- Ctrl 정지 자세(`Brace` 입력, PlayerMotorModel `Bracing`/`BraceReady`), 실제 수평 속도 `PlayerMover.GroundSpeed`, `ColorRules` + `PlayerHitRule` 주입. 빨강 = 자세 + 정지, 파랑 = 실제 이동 ≥ 1.2m/s, 노랑은 색 규칙 통과 없음.
- 색 표시(탄 색·회전 차이, 자세 원판·웅크림, HUD, 화면 밖 표시 색), 시험 패턴 `ColorTest/Pattern_RedTest·BlueTest` (맵 미연결).
- 상세: `Plans/Parallel/Claude_Log.md`, 설계 MD 1절 "실제 적용된 4단계". 작업량 16시간 → `Work_Effort.md`.

### 검증
- 브랜치: EditMode 135/135, PlayMode 19/19. 합친 main에서 전체 재실행 (`Logs/merge4_*.xml`).

### 다음에 할 일
- Codex 3단계 완료 → Codex 브랜치에 main 받기(merge) → 전체 테스트 → main 합침 → Codex_Log 내용을 TODO·WORKLOG·설계 MD·Work_Effort에 반영.
- 그다음 5단계 N-0 연결 지점(회피·쳐내기·색 통과 이벤트, 색 조합 검사 함수)을 main에 만들고 분할 진행.
---

## 2026-10-06 — Claude: 3단계 합침 + 5-0 연결 지점 + 각자 할 일 문서

### 한 일
- Codex 3단계(Pattern Editor) 커밋 `ed6c404` → Codex 브랜치에 main(4단계) 받기 → 통합 테스트 EditMode 150/150, PlayMode 20/20 → main 합침 `020df2f`. Codex 작업은 모두 담당 경로 안, 요청 사항 없음.
- 5-0 연결 지점: `ColorCombinationRules`(빨강·파랑 동시 위험 금지, 전환 유예 0.6초), `EncounterMemory` + `PatternContext.Memory`(전투 동안 유지되는 패턴 기록, BattleWorld가 주입). `EncounterContractTests` 2개. main EditMode 152/152, PlayMode 20/20.
- 각자 할 일 문서 정리 (사용자 요청): `Plans/Parallel/Codex_Log.md` — Codex가 바로 시작할 5단계 지시(만들 것·8개 맵 기본안·담당 파일·작성 규칙), `Plans/Parallel/Claude_Log.md` — Claude 5단계 할 일. 둘 다 "할 일 → 한 일 → 작업량", HTML 생성.
- 설계 MD에 "실제 적용된 3단계", TODO·작업량·CLAUDE.md 갱신.

### 결정
- `claude.md`는 만들지 않음 — Windows에서 `CLAUDE.md`(규칙 파일)와 같은 파일. 기존 `Claude_Log.md`·`Codex_Log.md`에 씀.
- 모든 기록은 한국어로만 (사용자 요청) — 두 문서와 CLAUDE.md에 규칙으로 기록.
- 5단계 브랜치: Codex `phase5-director`, Claude `phase5-measure`.

### 다음에 할 일
- 사용자: Codex에게 "`Plans/Parallel/Codex_Log.md`의 지금 할 일부터 진행" 전달.
- Claude: `Game2Week-claude`(`phase5-measure`)에서 측정 도구·색 안내.
---

## 2026-10-06 — Claude: 6-0 연결 지점 + 5~8단계 한 번에 진행 지시

### 사용자 요청
- 남은 단계를 Codex 절반·Claude 절반으로 한 번에 작업하고 나중에 합치기. 각자 할 일 문서에 Codex가 바로 작업할 수 있게 작성.

### 한 일
- 6-0 연결 지점 (main): `BattleFeedback`(회피·점프·착지·쳐내기·자세·피격·색 통과·발사·예고 알림, `BattleWorld.Feedback`·`PatternContext.Feedback`), `BattleEvents.EnemySpoke`, `BattleFxRig` + 프리팹을 Battle 씬 `BattleController.fxRigPrefab`에 연결. 주인공 쪽 Raise 연결 완료. `FeedbackContractTests`. EditMode 152/152, PlayMode 21/21.
- 7·8단계는 두 쪽 파일이 겹치지 않아 연결 지점 불필요 (Codex 7단계는 `GameSession.BeginStage` 등 공개 API만 사용).
- `Codex_Log.md`: 5(패턴 누적·8개 맵) → 6(전투 이펙트·말풍선) → 7(맵툴 M5) → 8(스토어 이미지·문구) 지시, 단계별 담당 파일, 공용·Claude 담당 금지 목록, 막히면 "요청"에 적고 계속.
- `Claude_Log.md`: 5(측정 도구·색 안내) → 6(믹서·소리·BGM·페이드) → 7(주인공 v2·리깅·애니메이션) → 8(ProductInfo·정적 폰트·빌드 재검증).

### 결정
- 사용자 "한 번에 작업" 요청 → 각자 단계가 끝나고 전체 테스트가 통과하면 **자기 브랜치에 커밋** (push·main 합치기는 사용자 요청 시).
- 브랜치는 그대로: Codex `phase5-director`, Claude `phase5-measure` (5~8단계 계속).

### 다음에 할 일
- 사용자: Codex에게 "`Plans/Parallel/Codex_Log.md`의 할 일을 5단계부터 8단계까지 순서대로" 전달.
- Claude: `Game2Week-claude`에서 5단계부터.
- 둘 다 끝나면 main에 합치기 → 전체 테스트 → 문서 반영.
---

## 2026-10-06 — Claude: 5~8단계 배분 변경 (단계 단위)

### 사용자 결정
- 단계 하나를 둘이 반씩 나누지 않고 **단계 단위로 나눔: Claude = 5·6단계, Codex = 7·8단계.** 동시 진행 5↔7 → 6↔8, 끝나면 합침.

### 한 일
- `Codex_Log.md` 다시 작성: 7단계(주인공 v2·리깅·애니메이션 + 맵툴 M5) → 8단계(ProductInfo·정적 폰트 아틀라스·스토어 이미지·설명 문구·빌드/MSIX 재검증). 이전 판의 5·6단계 지시는 취소로 명시. 씬·스테이지 JSON 수정 금지, 담당 파일 외 전부 금지.
- `Claude_Log.md` 다시 작성: 5단계(색 안내·DifficultyProfile·PatternEncounterData·PatternDirector·8개 맵·측정 봇) → 6단계(발사·예고 알림·말풍선·이펙트·믹서·BGM·메뉴 효과음·리스너·페이드).
- 브랜치 이름 변경: Codex `phase7-codex`, Claude `phase5-claude`.

### 다음에 할 일
- 사용자: Codex에 바뀐 지시 전달 (이미 5단계를 시작했다면 멈추고 7단계부터).
- Claude: `Game2Week-claude`에서 5단계 계속 (색 안내 진행 중).
---

## 2026-10-06 — Claude: 5~8단계 합침 (Claude 5·6 / Codex 7·8) + 통합 연결

### 한 일
- Claude 5·6단계 (`phase5-claude`) → main `f8a7e07`. Codex 7·8단계 (`phase7-codex`)에 main을 받아 통합 → main `35b9af4`. 두 브랜치가 겹친 파일 0개.
- Codex 통합 요청 2개 연결: `BattleWorld`가 `PlayerAnimationDriver`에 `BattleFeedback` 연결(피격 동작), `BattlePresentation` 패배 때 `PlayFall()` → 0.45초 뒤 파편·사라짐. 피격 파티클 카메라 쪽 이중 당김 제거. `IntegrationTests`.
- 통합 테스트: EditMode 172/172, PlayMode 29/29 (+측정 전용 1 건너뜀).
- 문서: TODO·작업량(Claude 5·6 44시간, Codex 7·8 56시간, 통합 3시간)·설계 MD("실제 적용된 5단계")·CLAUDE.md(새 규칙·에디터 함정 3)·병렬 계획 상태 갱신.

### 5~8단계 요약 (상세는 두 로그)
- 5 (Claude): PatternDirector·난이도 프로필, 8개 맵 1→8종(간격 1.40→0.65초), 색 처음 등장 안내, 자동 플레이 측정 봇 + `Plans/Balance_Report.html`, Ctrl 고착 버그 수정.
- 6 (Claude): 발사·예고 알림, 이펙트 모듈 8종, 적 말풍선, 소리 연결 지점(믹서 선택), 리스너 정리, 씬 전환 페이드.
- 7 (Codex): 주인공 v2·19골격·10클립·Animator, 맵툴 M5(3D 미리보기·바로 플레이·영역/대칭 배치).
- 8 (Codex): ProductInfo, 정적 한글 폰트(각 약 68MB), 스토어 이미지·문구, Windows 빌드 167MB·MSIX 55MB 재검증.

### 결정 필요 / 사용자 확인
- 적 접근 난이도: 직진하면 2~4초면 적에게 닿아 패턴을 거의 못 봄 (측정 리포트).
- 소리 출처, 회사·게임 이름 (ProductInfo 한 곳만 바꾸면 됨).
- 직접 확인: 주인공 v2 외형·동작, 맵툴 바로 플레이·대칭 배치, Pattern Editor, 스토어 이미지.

### 다음에 할 일
- 사용자 플레이 확인 → 접근 난이도·간격 조정 결정.
- 작업 폴더(worktree) 두 개는 남겨 둠 — 다음 병렬 작업 때 main으로 맞춰 재사용하거나 `git worktree remove`로 정리.
---

## 2026-10-06 — Claude: 정적 폰트를 Git LFS로 (기록까지 정리)

### 한 일
- 사용자 선택: 기록까지 정리. `git lfs migrate import`로 폰트가 바뀌기 시작한 `18c23f2` 이후 커밋 10개만 다시 씀 (그 전 기록은 그대로). `.gitattributes`에 `Pretendard-*SDF.asset` LFS 규칙.
- 다시 쓴 뒤 작업 폴더의 폰트가 133바이트 포인터로 남아 `git lfs checkout`으로 실제 내용 복원. 원래 커밋 `1644593`과 비교해 폰트 2개 SHA256 동일, 나머지 파일 차이 0.
- `git push --force-with-lease=main:1644593` → LFS 객체 4개(140MB) 업로드, main `6ddb5b0`. GitHub 큰 파일 경고 사라짐.
- 작업 폴더 두 개를 새 main으로 맞춤 (두 브랜치 내용은 이미 main에 포함). 로컬 백업 브랜치 `backup/before-lfs`(= 원래 `1644593`) 남김 — 확인 후 지워도 됨.

### 주의
- `git lfs migrate`는 지정하지 않은 브랜치라도 같은 커밋을 가리키면 같이 옮긴다 → 백업은 원래 커밋 해시로 다시 만들었다.
- 다른 컴퓨터에 받아 둔 사본이 있으면 새로 받아야 함 (최근 커밋 해시가 바뀜).

### 다음에 할 일
- 사용자 플레이 확인 → 접근 난이도·간격 결정 (이전 기록 참고).
---

## 2026-10-07 — Claude: 플레이테스트 개선 설계 + 8.5단계 T자 자세 버그 수정

### 사용자 요청
- 직접 플레이 결과 6가지 (전투 20초 안에 끝남, 적마다 공격 하나, 자비 너무 빠름, 셰이더·이펙트 없음, 애니메이션 없음, 대화 시스템·툴 필요) → 설계 MD·HTML 후 컨펌.
- 답: 맵 확대만 · 자비 = 공통 3턴 + 적별 조건 · 로우폴리 유지 + 셰이더 먼저 · 주인공이 T자 자세로 고정된 채 움직임. 나머지 질문은 "제안대로" → 8.5단계부터 시작.

### 한 일
- `Plans/Playtest_Fix_Plan.md`(+HTML, 대시보드 링크): 원인 분석과 8.5~12단계 설계, 질문 답 반영. 남은 질문(Q2b·Q5~Q8)은 제안 기본값으로 확정.
- 8.5 원인: 뼈 연결은 정상(클립 경로 76개 모두 연결). **glb 안 클립 자체가 팔다리 회전이 거의 0** — 모델 기본 자세가 팔을 54° 벌린 T자에 가까운 자세라 그대로 보였고, Run도 다리가 움직이지 않아 미끄러져 보였다.
- `Editor/Animation/HeroineClipAuthoring.cs` (`Tools ▸ Heroine ▸ 동작 클립 다시 만들기`): 동작 10종을 캐릭터 공간 각도(팔 앞·옆·팔꿈치, 다리 앞·무릎, 몸 기울기·비틀기·머리·머리꼬리)로 정의 → 실제 뼈 회전으로 구워 `Art/Characters/Heroine/Clips/Heroine_*.anim` 저장, 컨트롤러 상태에 연결. 앞·오른쪽 방향은 눈 메시·팔 위치로 자동 판별(이름 좌우 반전 대비), 다리 굽힘만큼 엉덩이를 내려 발이 바닥에 붙게. Run 주기 0.42초(3.4m/s에 맞춤).
- `HeroineV2Authoring.Apply`가 끝에 위 도구를 호출 → 다시 실행해도 T자 클립으로 돌아가지 않음.
- 회귀 테스트 `HeroinePoseRegressionTests`: 실제 전투에서 서 있을 때 두 팔이 아래로 30° 이내, 이동 중 Run 동작 + 허벅지 앞뒤 범위 > 0.6. 캡처 `Logs/scene_heroine_pose_idle/run.png`.

### 검증
- EditMode 172/172, PlayMode 30/30 (+측정 전용 1 건너뜀) (`Logs/all_*.xml`).

### 주의
- 키보드 이동(W·방향키)을 쓰는 InputTestFixture 테스트는 전체 실행 순서에 따라 이동 값이 0으로 읽힘 (단독 실행은 통과, 실제 게임은 정상). 그래서 이 회귀 테스트는 이동을 `PlayerMover.Move`로 직접 준다. → TODO 🔧에 확인 항목.

### 다음에 할 일
- 사용자: 에디터에서 주인공 동작 확인 (서기·달리기·회피·점프·쳐내기·정지 자세·피격·쓰러짐).
- Claude: 9-0 연결 지점 (스테이지 JSON v2 `dialogues`·`theme`, `EnemyMoveSet`, `ISpareCondition`, `DialogueDefinition`, `BattleStateId.Dialogue`) → 9단계 (맵 확대·적별 기술·겹·페이즈).
---

## 2026-10-07 — Claude: 9단계 맵 확대 · 겹 공격 · 적별 기술 · 체력 단계

### 한 일
- 8.5단계 커밋 `20cc19e`.
- 9-0: 스테이지 JSON v2 (`theme`, `dialogues` intro/phase2/spareReady/victory). v1도 읽고 저장하면 v2. 맵툴 경고 "시작점-적 15m 미만". `PatternContext.EnemyInfo`(적 고유 기술·단계 기술·체력 비율), `IBattleWorld.BeginPattern(…, EnemyPatternInfo)`, `BattleFeedback.EnemyPhaseChanged`.
- 맵 확대 (맵툴 코드 `StageEditorModel`로 저장): 칸 1m, 시작점-적 18~28.6m, 탄막 턴 14~19초.
- `PatternDirector`/`DirectorPlanner`: 최대 3겹, 소개 턴은 새 패턴 2초 뒤 다른 겹 합류, 후보에 적 기술 추가, `DifficultyProfile.phases`로 체력 단계(겹·간격·탄속).
- 적 고유 기술 4종(Graph 에셋, Pattern Editor 저장 경로 사용): 테스트 적 고리·양옆 지그재그, 주황 테스트 적 부채꼴 저격·나선 고리. 적 체력 70/90. 1-1도 Director(`Attack_Stage_1-1`).
- 그래프 탄 수명을 넓은 맵에 맞게 (최대 10초, 진행률로 모양이 정해지는 궤적은 유지). 적 말풍선이 멀리서도 읽히게 거리만큼 크기 조절.
- 측정 봇: 시작 거리·시간 초과·최대 겹·체력 단계 기록, 맵당 12턴·240초. `Plans/Balance_Report.html` 다시 생성.
- 테스트: `Stage9Tests` 4개, Director 계획·단계·후보 3개, 실제 전투 적 기술·단계 1개. 좁은 맵·단일 패턴 전제 테스트 10개 갱신 (`BattleTestUtil`). 처치·살려주기 흐름 봇은 피하지 않으므로 체력 유지(밸런스는 측정 봇 담당).

### 검증
- EditMode 179/179, PlayMode 31/31 (+측정 전용 1). 측정 봇 전체 실행 통과.
- 측정: 적 접근 평균 5.0~8.1초 (이전 1.5~3.8초), 시간 초과 0, 최대 2~3겹, 단계 전환 확인. **단순 봇은 6/8 맵 패배** (피격 4~5번 = 체력 20).

### 결정 필요
- 난이도: 사람 플레이로 확인 → 피격 피해(적 공격 4)·주인공 체력(20)·겹 수 조정 여부 (수치는 미결정 항목이라 임의로 바꾸지 않음).

### 다음에 할 일
- 사용자: 1-1·1-4·1-8 직접 플레이 → 길이·난이도 체감 알려 주기.
- Claude: 10단계 — 자비 조건(공통 3턴 + 적별 조건, FIGHT 시 0) + 대화 JSON·실행기·Dialogue Editor.
---

## 2026-10-07 — Claude: 사용자 플레이 피드백 수정 (메뉴 WASD · 탄속 · 예측 조준)

### 사용자 요청
- 행동 메뉴에서 WASD가 안 되고 방향키만 됨 → A·D로 좌우 이동. 총알이 너무 느림. 옆에서 오는 공격이 원래 위치에 쏴서 아예 안 맞음 → 약간 예측해서 쏘기. "조금 어렵게".

### 한 일
- 입력: UI 맵 `Navigate`에 WASD 묶음 추가 (W·S 목록 위아래, A·D 좌우). 회귀 테스트 `BattleInputTests.Menu_WASD_MovesLikeArrows`.
- 탄속: 8개 맵 `DifficultyProfile.speedScale` ×1.5 (1-1 1.5 … 1-8 2.0). 노랑 기본 2.4m/s → 1-1 3.6, 1-8 약 4.8m/s (주인공 이동 3.4).
- 예측 조준 `AimLead.Predict` (순수 함수): 조준이 고정되는 예고 시간 + 비행 시간 뒤 주인공 위치를 `leadFactor`만큼 반영 (기본 0.7, 최대 2초 앞). `PlayerMover.GroundVelocity`(부드럽게, 이동 속도 이하 — 회피 순간 속도로 튀지 않음) → `PatternContext.PlayerVelocity`. 노랑 패턴(적·측면 모두)과 그래프 패턴(`GraphPatternDefinition.leadFactor`, `PatternTimeline.Advance` 속도 인자 — Pattern Editor 미리보기는 속도 0이라 그대로) 적용.
- 테스트: `AimLeadTests` 4개.

### 검증
- EditMode 183/183, PlayMode 33/33 (+측정 전용 1).

### 다음에 할 일
- 사용자: 다시 플레이 → 탄속·예측 정도 체감 (예측이 너무 정확하면 `leadFactor`를 0.5로, 느리면 speedScale을 더).
---

## 2026-10-07 — Claude: 9단계 커밋 + Codex 인계

### 한 일
- 9단계 + 피드백 수정 커밋 `1993860` (push 안 함).
- 사용자 결정: 이후 작업은 Codex 혼자. `Plans/Parallel/Codex_Log.md`(+HTML) 다시 작성 — 0절 규칙(원래 폴더·main, 수치 결정 금지, 문서 갱신 방법, 커밋 규칙), 1절 할 일 10단계(자비 조건·대화 JSON·실행기·Dialogue Editor·샘플 대화) → 9b(시퀀스 탭) → 11(셰이더·이펙트·테마) → 12(애니메이션). `Plans/Playtest_Fix_Plan.md` 7절에 인계 표시.

### 다음에 할 일
- Codex: `Plans/Parallel/Codex_Log.md`의 "1. 할 일"을 10단계부터 순서대로.
- 사용자: 난이도 체감(피격 피해·주인공 체력·겹 수), 소리 출처, 회사·게임 이름 결정.
## 2026-10-07 — Codex: 10단계 자비 조건·전투 대화와 편집기

### 한 일
- 원래 폴더 main에서 단독 진행. 시작 시 Unity 실행을 알렸고 사용자 종료 응답 후 프로젝트 잠금 해제 확인.
- 전투마다 자비 추적기를 생성: 공통 3턴 + 조건 6종, FIGHT 선택 시 초기화/절반, 보석 힌트·마음 단계·메뉴 강조·살펴보기 힌트. 기존 규칙 없는 적 데이터 호환.
- 첫 적은 응원하기→말 걸기와 대화 선택 플래그, 둘째 적은 같이 뛰기와 쳐내기 3회. 도입에서 이야기를 미뤄도 말 걸기로 회복. 데미지·체력 수치는 수정하지 않음.
- 대화 JSON·체크섬·검증·저장소·등록식 효과 실행기, Dialogue 상태, 도입/체력 단계/자비 가능/승리 연결과 카메라 컷. 선택 효과는 자비에만 적용.
- 대화 편집기 3열·되돌리기·미리 보기·바로 플레이·실패 저장 방지, 적 조건 인스펙터, 맵툴 대화 선택. 샘플 5개와 맵 연결은 편집기 모델로 저장.
- 정적 폰트에 새 문구와 ●○ 기호 포함. TODO·설계 MD/HTML·작업량·Codex 기록 갱신.

### 검증
- 전체 EditMode 196/196: Logs/phase10_editmode.xml. 전체 PlayMode 게임 검사 34/34, -runBalance 전용 1개 제외: Logs/phase10_playmode.xml.
- 도입 선택·자비 플래그·3턴 전 자비 금지·조건 충족 대화 복귀, 기존 세 결말·색 대응·애니메이션 회귀.
- 화면: Logs/scene_phase10_dialogue_intro.png, Logs/scene_phase10_spare_ready.png. 편집기 모델의 되돌리기·실패 저장·파일 보호 검사. 툴 창 마우스 직접 조작은 별도 사용자 확인 대상.

### 결정 필요
- 기존 난이도/피격 피해·주인공 체력·적 체력, 소리 출처, 이름, 모델 교체 결정은 그대로 대기. 임의 변경 없음.

### 다음에 할 일
- Codex: 9b 공격 시퀀스 탭 → 11 그래픽·이펙트 → 12 애니메이션. 각 단계 전체 테스트 후 문서 갱신과 main 커밋, push 없음.

## 2026-10-07 — Codex: 9b단계 공격 시퀀스

### 한 일
- 시간표·반복·지속 시간·독립 시드·난이도 전달과 경계별 실행을 만들고 Director의 위험 수집·턴 종료 정리에 연결했다. 빨강+파랑·잘못된 시간·중첩은 저장을 막는다.
- Pattern Editor 시퀀스 탭의 목록·임시 사본·시간표·시간 슬라이더·그래프 합성 3D 미리보기·프리팹/카탈로그 저장을 추가했다. 다른 패턴은 시간표로 확인한다.
- 주황 적의 단계 기술에 고리 12발 뒤 부채꼴 3발씩 세 번 샘플을 도구로 등록했다. 피해·체력·데미지 공식은 그대로다.

### 검증
- 전체 EditMode 199/199 (`Logs/phase9b_editmode.xml`), 전체 PlayMode 35개 통과·실패 0·별도 성능 측정 1개 제외 (`Logs/phase9b_playmode.xml`).
- 시드·시간표·배율·색/중첩 거부·적 데이터 연결과 실제 Director 실행(총 21발)·위험 수집·종료 정리를 검증했다. 캡처 `Logs/scene_sequence_runtime.png`.

### 결정 필요
- 추가 결정 없음. 기존 수치·소리 출처·이름·모델 교체 결정 대기는 유지한다.

### 다음에 할 일
- 11단계 그래픽 → 12단계 애니메이션을 진행한다.

## 2026-10-07 — Codex: 11단계 그래픽·테마·이펙트와 렌더 최적화

### 한 일
- 공용 3단 툰·림·깊이 외곽선, 블룸·색 보정·약한 SSAO·안개·하늘을 연결했다. 로우폴리 모델은 유지했다.
- 노랑 구·빨강 결정·파랑 고리와 트레일, 베기·실제 스킨 회피 잔상·통과 속도선·예고·자비/처치·단계 이펙트를 추가했다.
- 3종 경기장 테마를 맵툴로 8개 맵에 배정했다. 주인공 71개 스킨 파츠는 같은 골격·정점을 유지한 17개 재질 묶음으로 합쳤다.
- 피해·체력·데미지 공식은 변경하지 않았다. 재현 도구와 설계 MD/HTML·측정 리포트를 갱신했다.

### 검증
- 전체 EditMode 201/201 (`Logs/phase11_editmode.xml`), 전체 PlayMode 39/39 (`Logs/phase11_playmode.xml`, 성능 측정 포함) 통과.
- 1080p 편집기 측정 8개 맵 평균 78.4~106.0fps. 적용 전후 값은 설계 4.3절에 기록했다. 배경 작업·편집기 실행 조건을 포함한 평균이며 빌드 최저 프레임 보장은 아니다.
- 통합 스킨과 원래 스킨의 대기/이동 정점 차이 0.005m 이내, 기존 T자 자세·숨김·피격·세 결말 회귀 확인.
- 화면 `Logs/scene_phase11_before_stage_001.png`부터 008, `Logs/scene_phase11_after_stage_001.png`부터 008. 측정 `Logs/phase11_before_report.json`, `Logs/phase11_after_report.json`.

### 결정 필요
- 추가 결정 없음. 기존 수치·소리 출처·이름·모델 교체 결정 대기는 유지한다.

### 다음에 할 일
- 12단계 록온 8방향·추가 주인공 동작·적 절차 동작·대화 자세를 구현하고 전체 회귀 검사 후 main에 커밋한다.
