using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Battle
{
    /// <summary>
    /// 상태가 화면 UI에 요청하는 창구 (대사·메뉴처럼 플레이어 응답이 필요한 것).
    /// HP 표시처럼 보여 주기만 하는 것은 <see cref="BattleEvents"/> 구독으로 처리한다.
    /// </summary>
    public interface IBattleUi
    {
        /// <summary>대사를 한 글자씩 보여 주고, 확인키로 닫히면 onClosed.</summary>
        void ShowDialogue(string text, Action onClosed);

        /// <summary>대사창에 글만 띄운다 (기다리지 않음) — 메뉴 고를 때의 상황 문구.</summary>
        void ShowBoxText(string text);

        /// <summary>가로 버튼 메뉴 (공격/행동/자비, 아이템/넘기기). 고르면 메뉴는 닫힌다.</summary>
        void ShowMainMenu(IReadOnlyList<string> items, Action<int> onSelected);

        /// <summary>대사창 안 세로 목록 (행동·아이템 목록). 취소키면 onCancel.</summary>
        void ShowListMenu(IReadOnlyList<string> items, Action<int> onSelected, Action onCancel);

        /// <summary>타이밍 게이지를 띄우고, 끝나면 정확도(0~1, 놓치면 null)를 돌려준다.</summary>
        void ShowTimingGauge(Action<float?> onFinished);

        void ShowTurnHud(string hint);
        void UpdateTurnHud(float remainingRatio);

        /// <summary>잠깐 떴다 사라지는 문구 (보석 보상 등)</summary>
        void ShowPopup(string text);

        /// <summary>대사창·메뉴·턴 표시를 모두 숨긴다 (상태 줄은 남김).</summary>
        void HideAll();
    }

    /// <summary>상태가 3D 경기장에 요청하는 창구 (이동·접촉 판정·보석 표시).</summary>
    public interface IBattleWorld
    {
        /// <summary>탄막 턴 시작 — 주인공을 시작 칸으로.</summary>
        void ResetPlayer();

        void MovePlayer(Vector2 input, float deltaTime);

        bool IsPlayerTouchingEnemy();

        /// <summary>주인공이 닿은 보석 id (보이는 것 중), 없으면 null.</summary>
        string TouchedGem(IReadOnlyList<string> visibleGemIds);

        void ShowGems(IReadOnlyList<string> gemIds);

        /// <summary>보석을 먹는 연출 + 숨기기.</summary>
        void CollectGem(string gemId);

        /// <summary>탄막 패턴 시작. 탄 한 발 데미지와 적 정보(고유 기술·체력 비율, 9단계)를 함께 준다. pattern이 null이면 탄막 없음.</summary>
        void BeginPattern(Data.AttackPatternData pattern, int damagePerHit, Patterns.EnemyPatternInfo enemy = null);

        /// <summary>지난 호출 이후 주인공이 맞은 데미지 합 (무적 시간 중엔 안 맞음).</summary>
        int ConsumePlayerDamage();

        /// <summary>탄막 정리 (남은 탄 제거).</summary>
        void EndPattern();
    }
}
