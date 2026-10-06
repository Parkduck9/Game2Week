using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using TMPro;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>최대 3개 가까운 위험의 거리 경고음과 화면 밖 방향 표시.</summary>
    public sealed class ThreatFeedbackView : MonoBehaviour
    {
        [SerializeField] TMP_Text[] indicators = System.Array.Empty<TMP_Text>();
        [SerializeField] AudioClip warningClip;
        [SerializeField, Min(.1f)] float hearingRange = 7f;
        [SerializeField, Range(0f,1f)] float maximumVolume = .12f;
        readonly List<Vector3> threats = new();
        readonly AudioSource[] voices = new AudioSource[3];
        PatternRunner runner;
        PlayerMover player;
        System.Func<float> volume;
        AudioClip generatedClip;
        public int ActiveVoices { get; private set; }
        public int VisibleIndicators { get; private set; }
        public void Bind(PatternRunner patterns, PlayerMover mover, System.Func<float> settingsVolume)
        {
            runner = patterns; player = mover; volume = settingsVolume;
            if (!warningClip)
            {
                const int samples = 22050;
                var wave = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = i/(float)samples;
                    float envelope = Mathf.Pow(Mathf.Sin(Mathf.PI*t),2f);
                    wave[i] = Mathf.Sin(2f*Mathf.PI*(330f*t+30f*t*t)) * .22f * envelope;
                }
                generatedClip = AudioClip.Create("Temporary_ApproachWarning",samples,1,samples,false);
                generatedClip.SetData(wave,0); warningClip = generatedClip;
            }
            for (int i = 0; i < voices.Length; i++)
            {
                var go = new GameObject($"ThreatVoice_{i}"); go.transform.SetParent(transform,false);
                var source = go.AddComponent<AudioSource>(); source.clip = warningClip;
                source.playOnAwake = false; source.loop = true; source.spatialBlend = 1f;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = .5f; source.maxDistance = hearingRange;
                voices[i] = source;
            }
            Clear();
        }
        void LateUpdate()
        {
            if (!player || !runner) return;
            if (Time.timeScale <= 0f)
            {
                foreach (var source in voices) if (source) source.Pause();
                return;
            }
            threats.Clear();
            if (runner.Current is IThreatSource sourceOfThreats) sourceOfThreats.CollectThreats(threats);
            threats.Sort((a,b)=>(a-player.transform.position).sqrMagnitude.CompareTo((b-player.transform.position).sqrMagnitude));
            ActiveVoices = VisibleIndicators = 0;
            var cam = Camera.main;
            for (int i = 0; i < voices.Length; i++)
            {
                bool active = i < threats.Count && Vector3.Distance(threats[i],player.transform.position) < hearingRange;
                var audio = voices[i];
                TMP_Text indicator = i < indicators.Length ? indicators[i] : null;
                if (!active) { audio.Stop(); if (indicator) indicator.gameObject.SetActive(false); continue; }
                var position = threats[i];
                audio.transform.position = position;
                audio.volume = ThreatFeedbackModel.Volume(Vector3.Distance(position,player.transform.position),hearingRange,maximumVolume,volume?.Invoke() ?? 1f);
                if (!audio.isPlaying) { audio.UnPause(); if (!audio.isPlaying) audio.Play(); }
                ActiveVoices++;
                if (!indicator || !cam) continue;
                var viewport = cam.WorldToViewportPoint(position);
                bool outside = viewport.z <= 0f || viewport.x < .05f || viewport.x > .95f || viewport.y < .05f || viewport.y > .95f;
                indicator.gameObject.SetActive(outside);
                if (!outside) continue;
                var edge = ThreatFeedbackModel.EdgePosition(viewport);
                indicator.rectTransform.anchorMin = indicator.rectTransform.anchorMax = edge;
                indicator.rectTransform.anchoredPosition = Vector2.zero;
                var dir = edge-Vector2.one*.5f;
                string arrow = Mathf.Abs(dir.x)>Mathf.Abs(dir.y) ? (dir.x>0f ? "▶" : "◀") : (dir.y>0f ? "▲" : "▼");
                indicator.text = arrow + " 노랑";
                VisibleIndicators++;
            }
        }
        public void Clear()
        {
            ActiveVoices = VisibleIndicators = 0;
            foreach (var source in voices) if (source) source.Stop();
            foreach (var indicator in indicators) if (indicator) indicator.gameObject.SetActive(false);
        }
        void OnDisable() => Clear();
        void OnDestroy() { if (generatedClip) Destroy(generatedClip); }
    }
}
