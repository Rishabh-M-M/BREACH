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

        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private MaterialPropertyBlock propertyBlock;
        private Vector3 bossFullScale;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            if (bossVisualRoot != null) bossFullScale = bossVisualRoot.localScale;
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

                // Vortex opens then closes regardless of direction.
                SetVortexProgress(Mathf.Sin(t * Mathf.PI) * maxVortexProgress);

                yield return null;
            }

            if (bossVisualRoot != null)
                bossVisualRoot.localScale = growing ? bossFullScale : Vector3.zero;
            SetVortexProgress(0f);

            onComplete?.Invoke();
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