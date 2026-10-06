using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 방사형 결정탄: 적에게서 일정 간격으로 여러 방향 탄을 한꺼번에 쏜다.
    /// 물결마다 각도를 조금씩 돌려 같은 자리에 안 맞게 하고, 탄 종류(색)를 번갈아 쓴다.
    /// </summary>
    public sealed class RadialBurstPattern : MonoBehaviour, IAttackPattern, IDirectablePattern, IThreatSource
    {
        [SerializeField] Bullet[] bulletPrefabs;
        [SerializeField, Min(3)] int bulletsPerBurst = 10;
        [SerializeField, Min(0.2f)] float burstInterval = 1.1f;
        [Tooltip("첫 발사까지 여유 (탄막 턴 시작하자마자 맞지 않게)")]
        [SerializeField, Min(0f)] float firstDelay = 0.7f;
        [SerializeField, Min(0.1f)] float bulletSpeed = 2.4f;
        [SerializeField] float rotatePerBurst = 13f;
        [SerializeField] float spawnHeight = 0.35f;
        [SerializeField] float spawnOffset = 0.6f;

        readonly List<Bullet> pool = new();
        PatternContext context;
        float timer;
        int burstCount;

        public int ActiveBullets
        {
            get
            {
                int n = 0;
                foreach (var b in pool) if (b.Active) n++;
                return n;
            }
        }

        public AttackColor PatternColor => AttackColor.Yellow;

        public void ApplyDifficulty(float intervalScale, float speedScale)
        {
            burstInterval = Mathf.Max(0.2f, burstInterval * intervalScale);
            bulletSpeed *= speedScale;
        }

        public void CollectThreats(List<ThreatPoint> threats)
        {
            if (context == null) return;
            foreach (var b in pool) if (b.Active && !b.Deflected) threats.Add(new ThreatPoint(b.transform.position, b.Color));
        }

        /// <summary>한 물결의 방향 (바닥 평면, 단위 벡터)</summary>
        public static Vector3[] Directions(int count, float offsetDegrees)
        {
            var dirs = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float a = (offsetDegrees + 360f * i / count) * Mathf.Deg2Rad;
                dirs[i] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
            }
            return dirs;
        }

        public void Begin(PatternContext patternContext)
        {
            context = patternContext;
            timer = burstInterval - firstDelay;
            burstCount = 0;
        }

        public void Tick(float deltaTime)
        {
            if (context == null) return;
            timer += deltaTime;
            if (timer >= burstInterval)
            {
                timer -= burstInterval;
                Fire();
            }

            foreach (var bullet in pool)
            {
                if (!bullet.Tick(deltaTime, context)) continue;
                bullet.Deactivate();
                context.ReportHit();
            }
        }

        public void End()
        {
            foreach (var bullet in pool) bullet.Deactivate();
            context = null;
        }

        void Fire()
        {
            if (context.EnemyView) context.EnemyView.PlayAttack();
            var origin = context.Enemy.position + Vector3.up * spawnHeight;
            var prefab = bulletPrefabs[burstCount % bulletPrefabs.Length];
            foreach (var dir in Directions(bulletsPerBurst, burstCount * rotatePerBurst))
                Get(prefab).Launch(origin + dir * spawnOffset, dir * bulletSpeed);
            burstCount++;
        }

        Bullet Get(Bullet prefab)
        {
            foreach (var b in pool)
                if (!b.Active && b.name == prefab.name) return b;
            var created = Instantiate(prefab, transform);
            created.name = prefab.name;
            pool.Add(created);
            return created;
        }
    }
}
