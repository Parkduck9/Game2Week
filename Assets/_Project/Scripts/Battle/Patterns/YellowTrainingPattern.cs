using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>예고 때 목표를 고정하는 노랑 공격. 에셋으로 직선/사인파·랜덤 조준·측면 교대를 설정한다.</summary>
    public sealed class YellowTrainingPattern : MonoBehaviour, IAttackPattern, IThreatSource
    {
        [SerializeField] Bullet bulletPrefab;
        [SerializeField] AttackColor attackColor = AttackColor.Yellow;
        [SerializeField] Material warningMaterial;
        [SerializeField, Min(0.1f)] float interval = 1.4f;
        [SerializeField, Min(0.1f)] float warningDuration = 0.75f;
        [SerializeField, Min(0.1f)] float speed = 2.4f;
        [SerializeField] float height = 0.32f;
        [SerializeField, Min(0f)] float aimRandomRadius;
        [SerializeField] int randomSeed = 2718;
        [SerializeField] bool alternateSideOrigins;
        [SerializeField] float warningWaveAmplitude;
        [SerializeField] float warningWaveFrequency = .65f;
        readonly List<Bullet> bullets = new();
        PatternContext context;
        LineRenderer warning;
        Vector3 origin, target;
        float timeToShot;
        bool warningActive;
        System.Random random;
        int shots;
        public Vector3 WarningOrigin => origin;
        public Vector3 WarningTarget => target;
        public bool WarningActive => warningActive;
        public int ActiveBullets { get { int count = 0; foreach (var b in bullets) if (b.Active) count++; return count; } }
        public IReadOnlyList<Bullet> Bullets => bullets;

        public void Begin(PatternContext value)
        {
            context = value;
            random = new System.Random(randomSeed); shots = 0;
            warning = gameObject.AddComponent<LineRenderer>();
            warning.sharedMaterial = warningMaterial;
            warning.positionCount = 24; warning.startWidth = warning.endWidth = 0.035f;
            timeToShot = warningDuration;
            BeginWarning();
        }
        void BeginWarning()
        {
            origin = context.Enemy.position + Vector3.up * height;
            if (alternateSideOrigins && shots % 3 != 0)
            {
                float side = shots % 3 == 1 ? -1f : 1f;
                var playerLocal = context.Arena.transform.InverseTransformPoint(context.Player.position);
                var local = new Vector3(side * (context.Arena.Size.x * .5f - .45f), height,
                    Mathf.Clamp(playerLocal.z + 1.2f,-context.Arena.Size.y*.5f+.5f,context.Arena.Size.y*.5f-.5f));
                origin = context.Arena.transform.TransformPoint(local);
            }
            var aim = context.Player.position + WaveTrajectory.AimOffset(random,aimRandomRadius);
            target = context.Arena.ClampToArena(new Vector3(aim.x, origin.y, aim.z),.35f);
            var forward = (target-origin).normalized;
            float time = Vector3.Distance(origin,target) / speed;
            for (int i = 0; i < warning.positionCount; i++)
                warning.SetPosition(i,WaveTrajectory.Evaluate(origin,forward,time*i/(warning.positionCount-1f),speed,warningWaveAmplitude,warningWaveFrequency));
            warning.enabled = true; warningActive = true;
        }
        public void Tick(float dt)
        {
            if (context == null || dt <= 0f) return;
            timeToShot -= dt;
            if (!warningActive && timeToShot <= warningDuration) BeginWarning();
            if (timeToShot <= 0f)
            {
                var dir = target - origin; dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    var bullet = GetBullet();
                    bullet.Color = attackColor;
                    bullet.Launch(origin + dir.normalized * 0.45f, dir.normalized * speed);
                    if (context.EnemyView) context.EnemyView.PlayAttack();
                }
                timeToShot = Mathf.Max(interval, warningDuration + 0.1f);
                shots++;
                warning.enabled = false; warningActive = false;
            }
            foreach (var b in bullets)
                if (b.Tick(dt, context)) { b.Deactivate(); context.ReportHit(); }
        }
        Bullet GetBullet()
        {
            foreach (var b in bullets) if (!b.Active) return b;
            var created = Instantiate(bulletPrefab, transform); bullets.Add(created); return created;
        }
        public void End()
        {
            foreach (var b in bullets) b.Deactivate();
            if (warning) warning.enabled = false;
            context = null;
        }
        public void CollectThreats(List<ThreatPoint> threats)
        {
            if (context == null) return;
            if (warningActive) threats.Add(new ThreatPoint(origin, attackColor));
            foreach (var b in bullets) if (b.Active && !b.Deflected) threats.Add(new ThreatPoint(b.transform.position, b.Color));
        }
    }
}
