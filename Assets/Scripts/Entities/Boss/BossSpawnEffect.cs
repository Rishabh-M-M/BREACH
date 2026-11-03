using System;
using System.Collections;
using UnityEngine;

namespace ProjectBreach.Boss
{
    public class BossSpawnEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform bossVisualRoot;
        [SerializeField] private Renderer vortexRenderer;

        [Header("Timing")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private AnimationCurve bossScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float maxVortexProgress = 1f;
        [Tooltip("How much faster the vortex opens/closes relative to the overall effect duration. " +
                 "1 = finishes exactly when the boss finishes scaling; higher values close the vortex " +
                 "earlier so it doesn't linger wide open once the boss has already shrunk away.")]
        [SerializeField] private float vortexSpeedMultiplier = 1.8f;

        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private MaterialPropertyBlock propertyBlock;
        private Vector3 bossFullScale;
        private Renderer[] bossRenderers;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            if (bossVisualRoot != null)
            {
                bossFullScale = bossVisualRoot.localScale;
                bossRenderers = bossVisualRoot.GetComponentsInChildren<Renderer>();
            }
        }

        public void PlaySpawnEffect()
        {
            StopAllCoroutines();
            StartCoroutine(PlayEffect(growing: true, onComplete: null));
        }

        public void PlayDespawnEffect(Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(PlayEffect(growing: false, onComplete: onComplete));
        }

        private IEnumerator PlayEffect(bool growing, Action onComplete)
        {
            if (bossVisualRoot != null)
                bossVisualRoot.localScale = growing ? Vector3.zero : bossFullScale;

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                float bossT = growing ? t : 1f - t;
                float curveValue = bossScaleCurve.Evaluate(bossT);
                if (bossVisualRoot != null) bossVisualRoot.localScale = bossFullScale * curveValue;

                // Consumed = inverse of how "there" the boss currently is, so
                // it matches the scale curve for both growing and shrinking.
                SetBossConsumeProgress(1f - curveValue);

                // Vortex opens then closes regardless of direction - sped up relative to
                // the boss's own timeline so it finishes well before/by the time the boss
                // has fully shrunk away, instead of lingering open.
                float vortexT = Mathf.Clamp01(t * vortexSpeedMultiplier);
                SetVortexProgress(Mathf.Sin(vortexT * Mathf.PI) * maxVortexProgress);

                yield return null;
            }

            if (bossVisualRoot != null)
                bossVisualRoot.localScale = growing ? bossFullScale : Vector3.zero;
            SetBossConsumeProgress(growing ? 0f : 1f);
            SetVortexProgress(0f);

            onComplete?.Invoke();
        }

        private void SetBossConsumeProgress(float value)
        {
            if (bossRenderers == null) return;
            foreach (Renderer r in bossRenderers)
            {
                r.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(ProgressID, value);
                r.SetPropertyBlock(propertyBlock);
            }
        }

        private void SetVortexProgress(float value)
        {
            if (vortexRenderer == null) return;
            vortexRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(ProgressID, value);
            vortexRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}