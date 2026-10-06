using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 주인공 동작·공격 알림 → 임시 이펙트 (6단계). 리소스가 생기면 이 모듈의 생성 부분만 프리팹으로 바꾼다.
    /// 회피 잔광·점프/착지 먼지·쳐내기 불꽃·정지 자세 고리·색 통과 반짝임·피격 터짐·발사 연기·예고 고리.
    /// 색만으로 구분하지 않게: 빨강은 멈춰 있는 고리/알갱이, 파랑은 빠르게 흩어지는 알갱이.
    /// 모두 재사용(풀), 일시정지(timeScale 0)에서 멈추고, 탄막 턴이 끝나면 지운다.
    /// </summary>
    public sealed class ActionFxModule : MonoBehaviour, IBattleFxModule
    {
        [SerializeField] Material particleMaterial;

        static readonly Color DodgeColor = new(0.55f, 0.85f, 1f, 0.9f);
        static readonly Color DustColor = new(0.85f, 0.82f, 0.75f, 0.8f);
        static readonly Color ParryColor = new(1f, 0.92f, 0.4f, 1f);
        static readonly Color HitColor = new(1f, 0.35f, 0.3f, 1f);
        const float RingWidth = 0.035f;
        const int RingSegments = 40;

        sealed class Ring
        {
            public LineRenderer Line;
            public float Age, Duration, From, To;
            public Color Color;
            public bool Active;
        }

        readonly Dictionary<string, FxPool<ParticleSystem>> bursts = new();
        FxPool<Ring> rings;
        BattleFxRig rig;

        /// <summary>지금까지 만든 효과 수 (테스트·디버그용)</summary>
        public int Spawned { get; private set; }
        public int ActiveRings { get { int n = 0; if (rings != null) foreach (var r in rings.Items) if (r.Active) n++; return n; } }

        public void Bind(BattleFxRig battleRig)
        {
            Unbind();
            rig = battleRig;
            rings ??= new FxPool<Ring>(CreateRing, r => !r.Active);
            var f = rig.Feedback;
            f.PlayerDodged += OnDodged;
            f.PlayerJumped += OnJumped;
            f.PlayerLanded += OnLanded;
            f.PlayerParried += OnParried;
            f.PlayerBraced += OnBraced;
            f.PlayerHit += OnHit;
            f.ColorPassed += OnColorPassed;
            f.BulletFired += OnBulletFired;
            f.WarningStarted += OnWarning;
            if (rig.Events != null) rig.Events.StateChanged += OnStateChanged;
        }

        void Unbind()
        {
            if (rig == null) return;
            var f = rig.Feedback;
            f.PlayerDodged -= OnDodged;
            f.PlayerJumped -= OnJumped;
            f.PlayerLanded -= OnLanded;
            f.PlayerParried -= OnParried;
            f.PlayerBraced -= OnBraced;
            f.PlayerHit -= OnHit;
            f.ColorPassed -= OnColorPassed;
            f.BulletFired -= OnBulletFired;
            f.WarningStarted -= OnWarning;
            if (rig.Events != null) rig.Events.StateChanged -= OnStateChanged;
            rig = null;
        }

        void OnDestroy() => Unbind();

        void OnDodged(Vector3 at) => Burst("dodge", at + Vector3.up * 0.4f, DodgeColor, 14, 0.6f, 0.08f, 0.3f, 0.25f);
        void OnJumped(Vector3 at) => Burst("dust", at + Vector3.up * 0.05f, DustColor, 10, 1.1f, 0.06f, 0.35f, 0.15f);
        void OnLanded(Vector3 at) => Burst("dust", at + Vector3.up * 0.05f, DustColor, 16, 1.4f, 0.07f, 0.4f, 0.2f);
        void OnParried(Vector3 bullet, float side) => Burst("parry", bullet, ParryColor, 18, 2.2f, 0.06f, 0.25f, 0.05f);
        void OnBraced(Vector3 at) => PlayRing(at + Vector3.up * 0.02f, BattleTexts.AttackColorTint(AttackColor.Red), 0.2f, 0.7f, 0.35f);
        void OnHit(Vector3 at) => Burst("hit", at + Vector3.up * 0.5f, HitColor, 20, 1.8f, 0.08f, 0.35f, 0.2f);

        void OnColorPassed(AttackColor color, Vector3 at)
        {
            var tint = BattleTexts.AttackColorTint(color);
            if (color == AttackColor.Red) Burst("pass_red", at + Vector3.up * 0.5f, tint, 8, 0.15f, 0.07f, 0.45f, 0.35f);
            else Burst("pass_blue", at + Vector3.up * 0.5f, tint, 10, 1.6f, 0.05f, 0.25f, 0.2f);
        }

        void OnBulletFired(AttackColor color, Vector3 at) => Burst("fire", at, BattleTexts.AttackColorTint(color), 6, 0.8f, 0.06f, 0.25f, 0.05f);

        void OnWarning(AttackColor color, Vector3 at)
        {
            var ground = at; ground.y = Mathf.Max(0.02f, at.y - 0.3f);
            PlayRing(ground, BattleTexts.AttackColorTint(color), 0.15f, 0.45f, 0.4f);
        }

        void OnStateChanged(BattleStateId state)
        {
            if (state == BattleStateId.EnemyTurn) return;
            foreach (var pool in bursts.Values)
                foreach (var ps in pool.Items) if (ps) ps.Clear(true);
            if (rings != null) foreach (var r in rings.Items) { r.Active = false; r.Line.enabled = false; }
        }

        void Burst(string kind, Vector3 at, Color color, int count, float speed, float size, float lifetime, float radius)
        {
            if (!bursts.TryGetValue(kind, out var pool))
            {
                pool = new FxPool<ParticleSystem>(() => CreateBurst(kind, count, speed, size, lifetime, radius), ps => ps && !ps.IsAlive(true));
                bursts[kind] = pool;
            }
            var system = pool.Get();
            system.transform.position = at;
            var main = system.main;
            main.startColor = color;
            system.Play(true);
            Spawned++;
        }

        ParticleSystem CreateBurst(string kind, int count, float speed, float size, float lifetime, float radius)
        {
            var go = new GameObject($"Fx_{kind}");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = lifetime;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.maxParticles = count * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(0.01f, radius);
            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (particleMaterial) renderer.sharedMaterial = particleMaterial;
            return ps;
        }

        void PlayRing(Vector3 at, Color color, float from, float to, float duration)
        {
            var ring = rings.Get();
            ring.Line.transform.position = at;
            ring.Age = 0f; ring.Duration = duration; ring.From = from; ring.To = to; ring.Color = color;
            ring.Active = true;
            ring.Line.enabled = true;
            UpdateRing(ring);
            Spawned++;
        }

        Ring CreateRing()
        {
            var go = new GameObject("Fx_Ring");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = RingSegments;
            line.startWidth = line.endWidth = RingWidth;
            if (particleMaterial) line.sharedMaterial = particleMaterial;
            line.enabled = false;
            return new Ring { Line = line };
        }

        static void UpdateRing(Ring ring)
        {
            float t = Mathf.Clamp01(ring.Age / ring.Duration);
            float radius = Mathf.Lerp(ring.From, ring.To, 1f - (1f - t) * (1f - t));
            for (int i = 0; i < RingSegments; i++)
            {
                float a = i * Mathf.PI * 2f / RingSegments;
                ring.Line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
            var c = ring.Color; c.a *= 1f - t;
            ring.Line.startColor = ring.Line.endColor = c;
        }

        void Update()
        {
            if (rings == null || Time.timeScale <= 0f) return;
            foreach (var ring in rings.Items)
            {
                if (!ring.Active) continue;
                ring.Age += Time.deltaTime;
                if (ring.Age >= ring.Duration) { ring.Active = false; ring.Line.enabled = false; continue; }
                UpdateRing(ring);
            }
        }
    }
}
