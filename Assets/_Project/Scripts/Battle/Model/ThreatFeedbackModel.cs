using UnityEngine;

namespace Game2Week.Battle
{
    public static class ThreatFeedbackModel
    {
        public static float Volume(float distance, float range, float maximum, float settingsVolume) =>
            Mathf.Clamp01(1f - distance / Mathf.Max(.1f,range)) * Mathf.Clamp01(maximum) * Mathf.Clamp01(settingsVolume);

        public static Vector2 EdgePosition(Vector3 viewport, float margin = .08f)
        {
            var offset = new Vector2(viewport.x-.5f,viewport.y-.5f);
            if (viewport.z < 0f) offset = -offset;
            if (offset.sqrMagnitude < .0001f) offset = Vector2.down;
            float scale = (.5f-margin) / Mathf.Max(Mathf.Abs(offset.x),Mathf.Abs(offset.y));
            return Vector2.one*.5f + offset*scale;
        }
    }
}
