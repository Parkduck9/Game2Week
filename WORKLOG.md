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
