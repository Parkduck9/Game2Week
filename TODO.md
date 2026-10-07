# TODO — 3D 연출 턴제 전투 1회 완주 (독자 IP)

> 이 파일이 진행 상황의 **기준(Source of Truth)** 이다. 작업을 끝내면 `[ ]` → `[x]` 로 바꾸고 WORKLOG.md에 기록한다.
> `TodoList.html`(대시보드, http://localhost:8790/)이 이 파일을 직접 읽어 보여 준다 — 이 파일만 고치면 된다.
> 태그: `[핵심]` 확장성에 중요 · `[결정 필요]` 착수 전 사용자에게 질문

**현재 형태 (2026-10-06 변경):** 전투 박스 = **3D 경기장**(스테이지 JSON으로 크기·배치). 허리 추적 카메라·마우스 회전/록온, 카메라 기준 이동·회피·점프·노랑 쳐내기. **적에게 닿으면 FIGHT/ACT/MERCY**, **못 닿으면 ITEM/넘기기**.

## ▶ 다음 검토: 패턴 누적 · 3D 액션 회피
> 최신 요청: 맵마다 패턴 1종 추가, 발사 간격 감소, 3D 액션처럼 회피. 공통 액션과 노랑 사인파·측면은 적용했으며 8종 누적·최종 간격 패치는 후속 단계다. 기준: [MD](Plans/Action_Balance_Plan.md) · [HTML](Plans/Action_Balance_Plan.html).
- [x] 8단계 패턴 1→8종·원형 기준 간격 1.40→0.65초·동시 최대 2종 설계 초안 작성
- [x] 낮은 추적 카메라·대시·실제 점프·높이 충돌·적/보석 접촉·입력 맵 설계 초안 작성
- [x] MD 기준 HTML 생성 도구와 단계별 비교 화면, 대시보드 계획 링크 추가
- [x] 사용자 방향 반영 — 적 접촉 후 메뉴 유지, 캐릭터 뒤 허리 카메라, 노랑 쳐내기/Shift 회피, 후속 빨강 정지 자세·파랑 이동, 접근 경고음·sin 궤적
- [x] 추가 답변 반영 — 마우스 회전/록온, 우클릭 쳐내기, Ctrl 정지 자세 필수, 점프 회피, 플레이어 위치 랜덤 조준, 궤적/간격/속도 제작 툴과 총 8종
- [x] 쳐내기 방향 확정 — 오른손으로 왼쪽 탄은 오른쪽 어깨 뒤로, 오른쪽 탄은 왼쪽 어깨 뒤로 흘림. 이동은 누르고 있는 방향 유지
- [x] 1단계 공통 액션: 허리 카메라·마우스 회전/휠 클릭 록온·Shift 회피·Space 점프·우클릭 노랑 쳐내기, 1-1 직선 시험 공격. EditMode 119 / PlayMode 15 통과, 화면 확인. 손 표현은 리깅 전 임시 큐.
- [x] 2단계 랜덤 조준·사인파·측면 발사·거리 경고음·화면 밖 표시, 1-2/1-3 연결. EditMode 125 / PlayMode 16 전체 통과, 실제 화면 확인 (손/소리 임시).
- [x] 병렬 작업 계획 [MD](Plans/Parallel_Work_Plan.md) · [HTML](Plans/Parallel_Work_Plan.html) — 3단계 Codex / 4단계 Claude, 파일 담당표·worktree·합치기 규칙·Codex 지시문
- [x] 0단계 연결 지점: `ITrajectory`·`AttackColor`·`IHitRule`·`ThreatPoint`, Bullet 궤적/색/피해 규칙, 쳐내기는 노랑만. EditMode 128 / PlayMode 16 통과
- [x] worktree 두 개 생성 (`Game2Week-codex` / `Game2Week-claude`, Library 복사, 둘 다 EditMode 128 통과)
- [x] 이후 단계 5~8 분할 계획 (영역별 기본 담당 + 단계별 N-0 연결 지점) — 계획 8절
- [x] 작업량 표시 (사람 기준) — `Plans/Work_Effort.md` + 대시보드 카드
- [ ] Codex에 계획 7절 지시문 전달 (사용자)
- [x] (Codex) Unity Pattern Editor: 8종 궤적·발사 간격·속도 그래프, 동일 런타임 평가 경로와 3D 미리보기 구현/검증 — `Graph_` 프리셋 8종 카탈로그 등록 (맵 연결은 5단계)
- [ ] Pattern Editor 창 직접 조작 확인 (사용자, `Tools ▸ Pattern Editor`)
- [x] (Claude) 빨강 Ctrl 정지 자세·파랑 실제 이동 판정 구현/검증 — main 합침 `58d2e54`, EditMode 135 / PlayMode 19 (맵 연결·조합 규칙은 5단계)
- [x] 3·4단계 main에 합치기 → 통합 테스트 EditMode 150 / PlayMode 20 → 문서 갱신 (`020df2f`)
- [x] 5-0 연결 지점: `ColorCombinationRules`(빨강·파랑 겹침 금지·전환 유예), `EncounterMemory`(전투 동안 패턴 기록) — EditMode 152 / PlayMode 20
- [x] 각자 할 일 문서: Codex → [Codex_Log.md](Plans/Parallel/Codex_Log.md) · Claude → [Claude_Log.md](Plans/Parallel/Claude_Log.md) (+HTML)
- [x] 6-0 연결 지점: `BattleFeedback`(월드 사건 알림)·`BattleEvents.EnemySpoke`·`BattleFxRig` 씬 연결 — EditMode 152 / PlayMode 21
- [x] (Claude) **5·6단계**: PatternDirector·8개 맵 1→8종(간격 1.40→0.65초)·측정 봇·색 안내·Ctrl 고착 버그 수정 → 이펙트·말풍선·소리 연결 지점·리스너·씬 전환 페이드 ([Claude_Log.md](Plans/Parallel/Claude_Log.md))
- [x] (Codex) **7·8단계**: 주인공 v2·19골격·10클립·Animator·맵툴 M5 → ProductInfo·정적 한글 폰트·스토어 이미지/문구·빌드·MSIX 재검증 ([Codex_Log.md](Plans/Parallel/Codex_Log.md))
- [x] 두 브랜치 main에 합치기 (`f8a7e07`·`35b9af4`) + 통합 요청 2개 연결(피격 애니메이션·패배 쓰러짐) → 통합 EditMode 172 / PlayMode 29(+측정 전용 1)
- [ ] 적 접근 난이도 결정 — 측정 봇이 2~4초면 적에게 닿음 ([Balance_Report](Plans/Balance_Report.html)) `[결정 필요]`
- [ ] 직접 확인 (사용자): 주인공 v2 외형·동작, 맵툴 M5 바로 플레이·대칭 배치, Pattern Editor 조작, 스토어 이미지
- [ ] (선택) AudioMixer를 에디터에서 만들어 `Data/Audio/AudioRouting`에 연결 / 소리 클립 채우기 (출처 `[결정 필요]`)
> 아래 A1~A6는 v1 제작 순서 참고. 최신 제작 순서는 설계 MD의 0절을 우선하며 점프는 사용자 요청으로 포함한다.
- [x] A1 공통 카메라 기준 이동·회피 기반 구현/검증 (최신 허리 카메라 적용)
- [x] A2 실제 점프·높이/상대 이동 충돌 구현/검증 (최신 노랑 직선으로 시험, 충격파는 v1 참고안)
- [ ] A3 공통 예고와 조준·통로·충격파 패턴 구현/검증
- [ ] A4 누적 프로필·턴 간 선택 기록·패턴 일정·공유 발사 예산 구현/검증
- [ ] A5 광선·회전·낙하·교차 패턴, StageEditorModel로 8개 맵 연결/검증
- [ ] A6 실제 플레이 후 접근 시간·피격·회피·카메라 가시성으로 밸런스 확정
- [ ] 이후 작업마다 MD·관련 HTML·TODO·WORKLOG를 실제 상태에 맞춰 갱신 (HTML: `node Tools/render_action_plan.mjs`)

## ▶ 진행 중: 캐릭터 모델링 (사용자 우선순위, 01단계보다 먼저)
- [x] 캐릭터 컨셉 확정 — 주인공, 노란 양갈래, 노란 눈, 하늘색 후드티 + 청바지, 소심하지만 할 땐 하는, 2.5등신
- [x] Codex로 레퍼런스 3면도 생성 → `Reference/heroine_turnaround.png`
- [x] 로우폴리 모델 v1 (three.js 코드, 약 4.4k 삼각형, 65파츠) → `Preview/heroine_preview.html`
- [x] 미리보기 — 레퍼런스 + 회전 3D 뷰어, 정면/측면/후면/3/4, 와이어프레임
- [ ] 사용자 피드백 반영 (v2)
- [x] GLB 내보내기 → `Assets/_Project/Art/Characters/Heroine/heroine_v1.glb` (glTFast 임포트 검증: 65메시, 4410삼각형, 키 0.97)

## ▶ 진행 중: 맵툴 · 스테이지 시스템 (계획: `Plans/MapTool_Plan.html`)
- [x] 계획 작성 (요구사항 정리, JSON 스키마 제안, 툴 화면, 검증 규칙, 구현 단계 M1~M5)
- [x] 확인 필요 답변 — 보석=주우면 보상(드물게) · Unity 에디터 창 · 격자 · 1→n 순서 · 1차 범위 확정
- [x] M1 데이터 & 로더 — StageDefinition, StageJson(체크섬), StageValidator, StageGeometry, StageRepository, ContentCatalog + 테스트, 첫 스테이지 `stage_001`
- [x] M2 맵툴 1차 — `Tools ▸ Stage Editor`: 목록(추가·복제·삭제·순서), 크기, 격자 배치(시작점·적·보석), 드래그 이동, 속성, 실시간 검증(오류 칸 빨간 테두리), 저장(오류 있으면 막음), 되돌리기 + 로직 테스트
- [ ] 맵툴 사용자 확인 — 직접 써 보고 불편한 점 피드백
- [x] M3 게임 연결 — BattleArena.Build(가변 크기), StageSpawner(주인공·적·보석), GemField(턴마다 확률 등장)·GemRewards, GameSession 스테이지 번호(MainMenu→1번, 재도전→같은 스테이지)
- [x] 보석 먹기(접촉 판정)·보상 적용 — 탄막 턴에서 연결 (06과 함께)
- [ ] M4 스테이지 진행 — 결과 화면 "다음 스테이지", 마지막 스테이지 처리 (→ 서비스 흐름 S5와 합침)
- [ ] M5 맵툴 2차 — 3D 미리보기, 바로 플레이, 일괄 배치
- [x] 스테이지 3개 자동 생성 (1-3 좁은 방 · 1-4 넓은 들판 · 1-5 결정의 방) → 총 5개, 모두 검증 통과 + 전 스테이지 PlayMode 확인
- [x] 추가 맵 3개 설정 (1-6 가로지르는 빛 · 1-7 긴 접근 · 1-8 중심의 결정) → 총 8개, StageEditorModel로 생성·저장, 전체 데이터·체크섬 검증 + PlayMode 11개 통과, 새 맵 3개 화면 확인

## ▶ 다음: 서비스 흐름 · Microsoft Store 배포 (계획: `Plans/Service_Plan.html`)
> 확정: Microsoft Store(MSIX) · 최고기록 = 클리어 시간 + 결과 종류 · 설정 = 음량·화면 모드/해상도·텍스트 속도 · 앱 삭제 시 세이브도 삭제
- [x] 계획 작성 (화면 흐름, 세이브 구조, 구현 단계 S1~S8, 집에서 할 일)
- [x] S1 세이브·설정 저장소 — `Scripts/Save/`: SaveService(save.json·settings.json 분리, 임시 파일→교체 + .bak 복구), 해금 = 클리어한 다음 하나까지, 최고기록 = 더 빠를 때만(그 판의 결과 종류 함께) + 테스트
- [x] S2 메인 화면 개편 — 새로 시작(진행 있으면 확인 창, 커서는 "아니요") · 이어하기(진행 없으면 회색) · 설정 · 종료 + 설정 화면(배경음·효과음·화면 모드·해상도·텍스트 속도, 닫을 때 저장)
- [x] S3 스테이지 선택 — 해금된 것만, "최고 0:41.3 · 살려줌" / "기록 없음", X로 메인
- [x] S4 ESC 일시정지 — 계속 / 설정 / 메인으로(확인 창), 시간 정지, 전투 UI 숨김, 풀면 입력 모드 복원. 클리어 시간은 일시정지 제외
- [x] S5 결과·엔딩 — 클리어 시간·신기록, 다음 스테이지 | 엔딩으로 · 다시 도전 · 스테이지 선택, 엔딩 씬(엔딩 본 것 저장) → 메인. PlayMode 테스트로 저장 유지까지 확인
- [x] S6 출시 준비 — 빌드 스크립트(`Tools/build_windows.ps1`, 메뉴 Tools ▸ Build) · MSIX 매니페스트 템플릿 + `Tools/package_msix.ps1` · **여기서 검증 완료**: 빌드 104MB/97초, 서명 없는 MSIX 40.8MB 생성(makeappx 매니페스트 검증 통과), 빌드 실행 15초 오류 없음 · 집에서 할 일 안내서 `Plans/Store_Guide.html`
- [ ] 회사/제품 이름 확정 (세이브 경로·스토어 이름) `[결정 필요]`
- [ ] S7 (집, 사용자) 로컬 설치·삭제 확인 — 세이브가 삭제와 함께 사라지는지
- [ ] S8 (집, 사용자) 파트너 센터 제출 → 스토어 설치·삭제
- [ ] 게임 이름·아이콘 확정 (스토어 제출 전) `[결정 필요]`

## 🔧 바꿔야 할 것 (알려진 문제 · 정리 대상 · 확인 필요)
> 발견하면 여기에 추가하고, 해결하면 체크 + WORKLOG에 기록한다.
- [ ] [결정 필요] 9단계 뒤 난이도 — 측정 봇이 8개 중 6개 맵에서 패배 (피격 4~5번이면 체력 20 소진). 사람 플레이로 확인 후 피격 피해(적 공격 4)·주인공 체력(20)·겹 수 조정 여부
- [ ] 사용자 플레이테스트 (2026-10-07) 6가지 — 설계 `Plans/Playtest_Fix_Plan.md` **컨펌됨**, 8.5·9·10 완료 → 9b·11·12 진행: 전투 길이·적별 공격·자비 조건·셰이더/이펙트·애니메이션·대화 시스템/툴
- [x] 10단계 자비 조건 — 공통 3턴 + 적별 순서/행동/선택/보석/체력 조건, 공격 초기화, 자비 보석 힌트, 마음 단계 표시
- [x] 10단계 전투 대화 — JSON·체크섬·실행기·상황별 대화 상태·선택 효과, 대화 편집기·맵툴 연결·샘플 5개
- [ ] 9b단계 공격 시퀀스 탭 — 시간표·반복·시드·색 검사·Director 실행
- [ ] 11단계 그래픽 — 후처리·툰/외곽선·색별 탄 모양·행동 이펙트·테마·성능 측정
- [ ] 12단계 애니메이션 — 8방향 록온·추가 주인공 동작·적 절차 동작·대화 자세
- [x] 주인공이 T자 자세로 고정된 채 미끄러짐 (사용자 확인) — 원인: glb 안 클립의 팔다리 회전이 거의 0. `HeroineClipAuthoring`으로 10종 재생성 + `HeroinePoseRegressionTests` (8.5단계)
- [ ] InputTestFixture 키보드 이동(W·방향키)이 전체 PlayMode 실행 순서에 따라 0으로 읽힘 (단독 실행 통과, 게임은 정상) — 테스트 환경 원인 확인
- [x] 새 게임 뒤 스테이지 목록이 비어 보임 — 실제 원인: 에디터 Play 시 GameSession의 테스트용 경로 null이 빈 문자열로 복원되어 기본 맵 폴더를 읽지 못함. 경로 override 직렬화 제외 + null/빈 값/공백은 기본 폴더 사용으로 수정. 사용자 실행 중인 에디터에서 맵 8개·선택 항목 1개·전투 진입 확인. 별도로 목록 크기도 화면 비율에 맞춰 수정.
- [ ] `Tasks.md`는 여전히 "2D" — 사용자가 고칠지 결정 (Claude는 수정 금지, CLAUDE.md "달라진 점"이 우선)
- [x] git — PATH엔 없지만 GitHub Desktop 내장 git 사용 (CLAUDE.md 참고). 첫 커밋 `dce8022`
- [x] Pretendard 폰트 다운로드 (허락 받음, OFL 1.1 라이선스 파일 포함)
- [x] 템플릿 `Assets/Scenes/SampleScene` + `Settings/SampleSceneProfile` 삭제, 빌드 목록 교체
- [x] 임시 `BattleFlowStub` / `BattleStageBootstrap` 삭제 → `BattleController`로 교체, PlayMode 테스트 수정
- [x] 임시 공격 → 07 타이밍 공격으로 교체
- [x] 클로즈업에서 피격 파티클(큐브)이 화면에 비해 큼 — 6단계 크기 ×0.5
- [x] 적 대사 말풍선(월드 스페이스) — 6단계 `EnemySpeechBubble`- [ ] 테스트 실행 시 Input System 패키지 테스트가 섞임 (manifest testables) → 항상 `-assemblyNames Game2Week.Tests.*`로 실행
- [x] `Pretendard-Regular SDF.asset` 동적 폰트 git 변경 문제 — 8단계(Codex)에서 정적 아틀라스(KS X 1001 2,350자 + ASCII·기호, 게임 한글 531자 포함)로 해결. 폰트 에셋이 각 약 68MB
- [x] 음량 설정 연결 — 6단계 `AudioRouting`·`SceneBgm`·`BattleAudio`·메뉴 효과음이 설정값을 따름 (믹서는 선택)
- [x] AudioListener 없음 경고 — 6단계에서 씬마다 1개로 정리
- [ ] 회사 이름 `DefaultCompany` — 세이브 폴더 경로에 들어가므로 **출시 전에** 확정해야 함 (출시 후 바꾸면 기존 세이브를 못 찾음)
- [ ] 보석 보상 수치 임시 (`GemRewardSettings`: 회복 5, 다음 공격 ×1.5, 살려주기 +1) — 데미지 공식과 함께 확정
- [ ] 타이틀 "타이틀 (가제)" — IP 기획 때 교체
- [x] 씬 전환 연출(페이드) — 6단계 `SceneLoader` 검은 화면 0.25초 + 배경음 같이 줄이기
- [x] 템플릿 `InputSystem_Actions` → `_Project/Input/GameControls`로 교체
- [x] 타깃 플랫폼 **PC 전용** 확정 → Mobile 품질 레벨 + Mobile_RPAsset/Renderer 삭제
- [ ] 테스트 데이터 수치는 임시 (주인공 HP 20·공격 10, 테스트 적 HP 30·공격 4, 살려주기 기준 2, 붕대 10·주먹밥 15) — 데미지 공식 정할 때 같이 확정
- [ ] Claude가 기본값으로 정한 것 사용자 확인 — 후드티 연하늘색, 머리끈 파랑, 흰 운동화, 게임패드 바인딩 추가
- [ ] 모델 v1 한계 — 손가락 없음, 측면에서 눈·볼이 살짝 뜸, 머리카락 결 단순 (필요하면 v2)
- [x] 주인공 색 섞임 — 원인: 배치모드에서 카메라를 **처음 한 번** 렌더할 때만 생김 (두 번째 프레임은 정상, PlayMode 캡처로 확인). 게임 문제 아님
- [x] TextMeshPro Essentials 임포트

## 완료 기준 (Definition of Done)
- [x] 메인 → 3D 전투 → 여러 턴 → 승리(처치/살려줌) 또는 패배(HP 0) → 결과 화면 → 메인 복귀/종료 (+ 스테이지 선택·엔딩·세이브)
- [x] 키보드만으로 모든 조작 가능, 콘솔 에러 없음 (봇 테스트 + 빌드 실행 로그)
- [x] 새 적 / 새 패턴 / 새 스테이지를 코드 수정 없이 에셋 추가만으로 생성 가능 (1-2로 확인)
- [x] 전투 로직이 3D 표현에 의존하지 않음 — 상태는 IBattleUi/IBattleWorld만 알고 가짜 구현으로 테스트됨
- [x] 원작(Undertale)의 이름·캐릭터·그래픽·음악·폰트를 사용하지 않음 (독자 주인공·임시 도형·Pretendard, 사운드 없음)

---

## 01. 프로젝트 셋업
- [x] Unity **6000.3.25f1** URP 프로젝트를 CLI로 현재 폴더에 생성 (Universal 3D 템플릿) + Unity용 .gitignore
- [x] 첫 커밋 `dce8022` (.gitignore 정리 후)
- [x] 폴더 구조 생성 (`Assets/_Project/...`, CLAUDE.md 참고)
- [x] Input System 1.20 설치 (템플릿 포함, activeInputHandler = 새 Input System 전용)
- [x] Input Actions `_Project/Input/GameControls` — UI(Navigate/Submit/Cancel), Player(Move) 맵, 키보드+게임패드, 프로젝트 전역 액션으로 등록
- [x] 주인공 확인용 씬 `_Project/Scenes/Sandbox_Heroine` (카메라·조명·바닥·heroine_v1)
- [x] Cinemachine 3.1.7 설치 · glTFast 6.20.0 설치 (GLB 임포트용)
- [x] TMP Essentials + **Pretendard** Regular/Bold 동적 SDF 폰트 에셋 (TMP 기본 폰트로 지정, Bold는 굵기 700으로 연결)
- [x] **16:9** 기본 해상도 1920×1080 (Player Settings)
- [x] Canvas Scaler 기준 1920×1080 (Scale With Screen Size, match 0.5)
- [ ] Game 뷰를 16:9로 맞추기 — 에디터 UI 설정이라 사용자가 Game 탭에서 선택
- [x] URP PC 에셋 — MSAA 4x(로우폴리 외곽선), 그림자 거리 20 · 캐스케이드 2 (작은 전투 무대 기준)
- [ ] 로우폴리 공용 머티리얼/조명 프리셋 — 전투 무대(05단계) 만들 때

## 02. 아키텍처 설계
- [x] BattleStateMachine — `BattleStateId`로 전환, Enter 중 전환 요청은 큐로 순서 보장 `[핵심]`
- [x] IBattleState 인터페이스 (Enter / Tick / Exit) + `BattleStateBase`
- [x] BattleContext — Input, Events, ChangeState만 노출 (전투원·UI·박스 참조는 해당 단계에서 추가)
- [x] 이벤트 채널 `BattleEvents` — StateChanged, Player/EnemyHpChanged, Player/EnemyDamaged, BattleEnded
- [x] **로직/표현 분리** 기반 — 로직은 BattleEvents로 알리기만, View는 구독만 (실제 View는 05단계) `[핵심]`
- [x] InputReader (ScriptableObject) — UI 맵: Navigate(한 칸씩)/Submit/Cancel, Player 맵: Move, 맵 전환 API
- [x] 어셈블리 정의 `Game2Week` + EditMode 테스트 14개 통과 (상태 머신, 입력 방향)
- [x] InputReader 에셋 생성 + GameControls 연결 (`_Project/Input/InputReader.asset`)
- [x] BattleController (조립 루트: 스폰·상태 등록·UI·연출 연결·종료 처리)

## 03. 데이터 (ScriptableObject)
- [x] PlayerData — 이름, LV, MaxHP, ATK, DEF, 시작 아이템
- [x] EnemyData — 이름, View 프리팹, HP, ATK, DEF, Check/등장/턴 문구/적 대사/살려줌/처치 문구, ACT 목록, 살려주기 기준치, 패턴 목록+순서(순차/랜덤)
- [x] ActOption(살려주기 진행도) / ItemData(회복량, 사용 문구)
- [x] AttackPatternData — 패턴 프리팹, 지속 시간, 박스 크기
- [x] 런타임 모델 — PlayerCombatant, EnemyCombatant, Inventory (+ 테스트 8개, 전체 22개 통과)
- [x] 테스트 에셋 — `Data/`: Player_Heroine, Enemy_Test(ACT 2), Item_Bandage/RiceBall, Pattern_Test(프리팹 없음) + `Input/InputReader`

## 04. 씬 흐름
- [x] SceneLoader(중복 로딩 방지) + GameSession 에셋(싱글톤 대신, 현재 적·전투 결과 전달) + SceneNames, GameQuit
- [x] 공용 메뉴 부품 — MenuList(커서 계산) / MenuListView(표시) / MenuNavigator(InputReader 연결)
- [x] MainMenu 씬 — 타이틀(가제) + 시작 / 종료 + 주인공 3D 모델
- [x] Result 씬 — 승리 / 전투 종료(살려줌) / GAME OVER + 적 문구 → 다시 도전 / 메인 / 종료
- [x] 임시 Battle 씬(BattleFlowStub)으로 전체 루프 연결 — PlayMode 테스트로 메인→전투→결과→재도전→결과→메인 자동 확인
- [x] 빌드 목록: MainMenu, Battle, Result (템플릿 SampleScene 삭제)

## 05. 3D 전투 무대 & 연출
- [x] 전투 경기장 `BattleArena` — 스테이지 크기대로 바닥·벽·격자선, 조명 (Battle 씬)
- [x] EnemyView — 임시 로우폴리 적(보라색 덩어리+눈+뿔, `EnemyView_TestBlob`) + 코드 애니메이션 (대기 출렁임 / 피격 흰색 번쩍+흔들림 / 공격 점프 / 처치 축소 / 살려줌 떠오름)
- [x] 카메라 샷 4종 (Intro / Overview 탄막 턴 / EnemyFocus 메뉴 / AttackCloseUp) — Cinemachine 0.6초 블렌드, 경기장 크기에 맞춰 거리 계산
- [x] 임시 이펙트 `BattleEffects` — 피격·보석 파티클, 카메라 흔들림(진폭 0.06m)
- [x] 카메라·적 애니메이션·이펙트를 전투 이벤트에 연결 (`BattlePresentation`: 상태→샷, 피격→번쩍+파티클, 종료→처치/살려줌 연출)
- [ ] 피격 파티클 위치를 적 표면/카메라 쪽으로 (07 FIGHT 연출 때)
- [x] 전투 박스 배치 방식 → **3D 경기장** (사용자 결정 B)
- [x] 경기장은 스테이지 크기로 생성, 카메라 거리도 크기에 맞춤

## 06. 전투 UI 기본
- [x] ~~BattleBox~~ → 3D 경기장으로 대체
- [x] 대사창 `DialogueBox` — 화면 하단, 글자 단위 출력, 확인키: 전부 표시 → 닫기
- [x] 상태 줄 `StatusBar` — 이름 · LV · HP 막대 · HP 숫자 (이벤트 구독)
- [x] 메뉴 2종 — 닿음: 공격/행동/자비, 못 닿음: 아이템/넘기기 (가로 메뉴, 명칭은 `BattleTexts`에 임시)
- [x] 목록 메뉴 — 대사창 안 세로 목록, 상하 이동, 취소키 뒤로
- [x] 탄막 턴 표시 `TurnHud` (적 대사 + 안내 + 남은 시간 막대), 팝업 `PopupText` (보석 보상)
- [x] 상태 흐름 10종 등록 (`BattleStates`) + 가짜 UI/World로 흐름 테스트 11개, 실제 키보드 입력 PlayMode 테스트

## 07. FIGHT — Timing Attack
- [x] 타겟 게이지 UI `TimingGaugeView` + 이동 커서 (1.3초에 왼→오, 구간 색: 보통/좋음/완벽)
- [x] 정확도 계산 `TimingGauge` (중앙 거리 → 0~1), 끝까지 안 누르면 MISS
- [x] 데미지 공식 (사용자 결정) = 공격력 × 정확도 배율(×0.5~×2.0) × 보석 강화 − 방어력, 최소 1, MISS 0
- [x] 공격 연출 — 클로즈업 + 카메라 쪽 피격 파티클 + 적 번쩍 + 데미지 숫자 팝업 + 적 HP 막대(`EnemyHealthBar`)
- [x] 적 HP 0 → Victory (임시 공격으로 동작)

## 08. ACT / ITEM / MERCY
- [x] ACT — 살펴보기(적 Check 문구)
- [x] ACT — 적 데이터의 행동 목록, 살려주기 진행도 +N, 가능해지면 "이제 살려 줄 수 있을 것 같다"
- [x] ITEM — 인벤토리, 회복, 소모, 빈 목록 처리 (못 닿았을 때 메뉴에서)
- [x] MERCY — 살려주기 (가능하면 노란색, 아니면 "아직..." 후 탄막 턴)
- [ ] Flee(도망치기) 포함 여부 `[결정 필요]`

## 09. 적 턴 — 플레이어 마커 & 탄막
- [ ] 적 대사 말풍선 (적 모델 위 월드 스페이스 UI) → 박스 리사이즈
- [x] 주인공 3D 이동 `PlayerMover` — 바닥 8방향 2.8m/s, 경기장 경계 제한, 이동 시 통통 튐 (이후 액션 1단계에서 카메라 기준 이동 3.4m/s·회피·점프로 확장)
- [x] 적 접촉 판정 → 탄막 턴 즉시 종료 → 공격/행동/자비 메뉴
- [x] 시간 종료(못 닿음) → 아이템/넘기기 메뉴
- [x] 보석 — 턴마다 확률 등장, 닿으면 먹고 보상 팝업
- [x] 피격 — 탄 한 발 = 적 공격 − 주인공 방어(최소 1), 무적 1초 + 깜빡임, HP 0 → Defeat
- [x] `IAttackPattern` + `PatternContext` + `PatternRunner` (패턴 = 프리팹 + AttackPatternData 에셋) `[핵심]`
- [x] `Bullet` 기본 클래스 — 바닥 평면 직선 이동(하위 클래스에서 Move 교체), 거리 판정(물리 X), 경기장 밖이면 회수, 패턴별 풀링
- [x] 테스트 패턴 `RadialBurstPattern` (사용자 결정: 방사형 결정탄) — 1.1초마다 10발, 물결마다 13° 회전, 청록/보라 번갈아, 첫 발 0.7초 여유
- [x] 턴 종료 → 탄 정리 → 다음 상태 / 스테이지가 패턴을 지정하면 그 순서, 아니면 적 데이터 순서

## 10. 승리 / 패배
- [x] Victory — 처치(축소) / 살려줌(떠오름) 연출 + 적 문구 → Result
- [x] Defeat — 주인공이 파편으로 흩어지며 사라짐(임시) + 흔들림 → GAME OVER
- [x] 다시 도전 / 스테이지 선택 / 메인 / 종료 동작 확인 (PlayMode 서비스 흐름 테스트)

## 11. 마무리 & 검증
- [x] 처치 · 살려주기 · 패배 루트를 **키보드 입력만으로** 끝까지 — `BattleRouteTests` 봇 (대사 Z, ↑로 접근, 메뉴 →/↓/Z, 게이지 가운데 Z)
- [x] 두 번째 적/패턴/스테이지를 **코드 수정 없이 에셋만으로**: `Enemy_Test2`(주황, HP 45, 행동 3개, 살려주기 기준 3) + 적 프리팹 Variant, `Pattern_Test2`(빠른 결정탄, 패턴 프리팹 Variant), `stage_002`(8×9m) → 동작 확인
- [x] 사운드 연결 지점 `BattleAudio` (적 피격·빗나감·주인공 피격·승리·패배, 클립 비어 있음, 음량 = 설정)
- [x] 교체할 리소스 목록 & 규격 `Plans/Resource_List.html` — 3D 모델 출처는 `[결정 필요]`

---

## 보류: IP 기획 (세계관/캐릭터 미정)
> 사용자와 함께 정한 뒤 진행. 그 전까지는 임시 이름·임시 디자인 사용.
- [ ] 세계관 · 주인공 · 테스트 적 캐릭터 컨셉
- [ ] 시스템 명칭 독자화 (메뉴 4종, 플레이어 마커, 자비/살려주기 개념 등)
- [ ] 아트 스타일 가이드 — **로우폴리 확정**, 색 팔레트·폴리곤 기준 등 세부 가이드
