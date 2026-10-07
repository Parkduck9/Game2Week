using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Data.Patterns
{
    [Serializable]
    public sealed class SequenceEntry
    {
        public float time;
        public AttackPatternData pattern;
        public bool repeat;
        public int count = 3;
        public float interval = 1.2f;
        [Tooltip("이 시간이 지나면 자식 패턴의 발사를 종료합니다.")]
        public float duration = 1f;
    }

    [CreateAssetMenu(menuName = "Game2Week/공격 시퀀스")]
    public sealed class AttackSequence : ScriptableObject
    {
        public int seed = 1234;
        public List<SequenceEntry> entries = new();
    }
}
