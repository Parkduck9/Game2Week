# Claude 작업 로그 — 4단계 빨강·파랑 (브랜치 `phase4-color-rules`)

> 병렬 기간 기록. main에 합칠 때 TODO·WORKLOG·Action_Balance_Plan·Work_Effort로 옮긴다.

## 2026-10-06 — 4단계 구현 완료

### 한 일
- **입력**: Player 맵에 `Brace` 동작 (왼/오른 Ctrl, 게임패드 LB). `InputReader.BraceHeld` (누르고 있는지).
- **이동 모델** `PlayerMotorModel`: `SetBrace`, `Bracing`, `BraceReady`.
  - 지상이고 회피 중이 아닐 때만 자세로 들어간다. 회피·점프 중에 누르면 끝난 뒤에 자세로 들어가 판정을 건너뛰지 않는다.
  - 자세 중에는 이동 입력을 무시하고, 새 회피·점프·쳐내기를 막는다. 진행 중이던 쳐내기는 취소된다.
  - 자세 전환 시간(`braceSettleTime` 0.08초)이 지나야 `BraceReady`가 된다.
- **실제 속도** `PlayerMover.GroundSpeed`: 경기장 경계로 제한한 뒤 실제로 움직인 수평 속도. 벽에 막히면 입력이 있어도 0에 가깝다. 자세 중에는 몸을 돌리지 않는다.
- **색 규칙** `ColorRules`(순수 로직) + `PlayerHitRule`(IHitRule 구현) → `BattleWorld`가 `PatternContext`에 주입한다.
  - 빨강: 정지 자세 완성 + 실제 속도 ≤ 0.05m/s일 때만 통과.
  - 파랑: 실제 속도 ≥ 1.2m/s일 때만 통과 (이동 속도 3.4의 약 35%).
  - 노랑: 색 규칙으로는 통과 없음 (쳐내기·회피·점프로 대응). 정지 자세가 노랑·파랑을 막지 못한다.
- **표시**: 색만으로 구분하지 않도록 했다.
  - `Bullet`이 발사할 때 빨강·파랑 색을 입힌다. 노랑은 프리팹 재질 그대로.
  - 회전으로도 구분한다: 빨강 = 멈춤, 파랑 = 2배 빠르게.
  - 정지 자세: 몸을 10% 낮추고 손을 가슴 앞에 두며, 발밑 원판이 연빨강 → 빨강(준비 완료)으로 바뀐다.
  - HUD에 `정지 자세` / `자세 잡는 중` / `Ctrl 정지 자세`를 표시한다.
  - 화면 밖 경고 표시를 색별 글자색으로 바꿨다.
  - 조작 안내에 `Ctrl 정지 자세`를 추가했다.
- **시험 자원** `ColorTest/Pattern_RedTest`·`Pattern_BlueTest`: `Pattern_YellowTraining` 프리팹 Variant(attackColor만 바꿈)와 AttackPatternData. 맵·ContentCatalog에는 연결하지 않았다 (5단계).
- 수치는 `PlayerActionSettings`에 있다: `braceSettleTime 0.08`, `stillSpeedMax 0.05`, `blueMinSpeed 1.2`.

### 검증
- **EditMode** 135/135 (`Logs/phase4_EditMode.xml`). `ColorRuleTests` 7개 추가:
  - 빨강·파랑·노랑 판정
  - 자세가 이동을 막는지, 전환 시간
  - 회피 중·공중에 누른 자세는 끝난 뒤 시작되는지
  - 자세 중 새 동작이 막히는지
- **PlayMode** 19/19 (`Logs/phase4_PlayMode.xml`). `ActionPhaseFourTests` 3개 추가, 모두 실제 키 입력으로 확인:
  - Ctrl+W를 누르면 멈춰 있고 빨강만 통과한다. 노랑·파랑은 맞는다. Ctrl을 놓으면 다시 이동하고 빨강에 맞는다.
  - A로 이동 중이면 파랑이 통과하고, 가만히 있거나 벽에 막힌 채 입력만 하면 맞는다.
  - `Pattern_RedTest`를 실제로 돌렸을 때 정지 자세로는 HP가 그대로이고, 자세 없이 서 있으면 HP가 줄어든다.
- 화면 확인: `Logs/scene_action_phase4_brace.png`, `scene_action_phase4_red_pattern.png`.
  - 빨강 원판, HUD의 "정지 자세", 빨강 탄을 확인했다.

### 남은 것 / Codex 쪽에 알릴 것
- `YellowTrainingPattern`의 예고선 재질은 노랑으로 고정이다. 빨강·파랑 패턴에서도 노랑 선이 나오므로 색별 예고선이 필요하다. Codex 담당 파일이라 고치지 않았다.
  - 3단계 새 패턴에서 `AttackColor`에 맞춰 칠하면 해결된다.
- 색 첫 등장 안내, 색 조합 금지 규칙, 맵 연결은 5단계에서 한다.
- 손 동작과 자세 표현은 리깅 전 임시 큐다 (7단계).

### 다음에 할 일
- 사용자 요청 시 커밋한다. 커밋 전에 폰트 에셋을 되돌리고, ProjectSettings 줄바꿈 변경은 커밋에서 뺀다.
- Codex 3단계가 끝나면 합치는 순서를 정한다 (계획 5절).

## 작업량

| 날짜 | 누가 | 단계 | 작업 | 사람 기준(시간) |
|---|---|---|---|---:|
| 2026-10-06 | Claude | 액션 4단계 | Ctrl 정지 자세 입력·이동 모델, 실제 속도, 빨강/파랑 색 규칙 + 주입, 색 표시·HUD, 시험 패턴, EditMode 7·PlayMode 3 | 16 |
