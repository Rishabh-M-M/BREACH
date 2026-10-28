
using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime-generated glass-like shield shards, energy sparks,
/// and a short slow-motion effect that safely restores game speed.
/// </summary>
public sealed class ShieldShatterEffect : MonoBehaviour
{
    private static ShieldShatterEffect runner;
    private static Coroutine slowMotionRoutine;
    private static float originalTimeScale = 1f;
    private static float originalFixedDeltaTime = 0.02f;

    public static void Play(
        Vector3 position,
        Vector3 shieldScale,
        Material material,
        AudioClip sound,
        int shardCount,
        float spread,
        float lifetime,
        float slowScale,
        float slowDuration)
    {
        GameObject root = new GameObject("ShieldShatterEffect_Runtime");
        root.transform.position = position;

        ShieldShatterEffect effect =
            root.AddComponent<ShieldShatterEffect>();

        effect.SpawnShards(
            position,
            shieldScale,
            material,
            Mathf.Clamp(shardCount, 8, 80),
            spread,
            lifetime
        );

        if (sound != null)
{
    GameObject audioObject = new GameObject("ShieldShatterAudio");
    audioObject.transform.position = position;

    AudioSource source = audioObject.AddComponent<AudioSource>();
    source.clip = sound;
    source.playOnAwake = false;
    source.spatialBlend = 0f; // 2D audio; no distance attenuation
    source.volume = 1f;
    source.pitch = 1f;
    source.loop = false;
    source.ignoreListenerPause = true;

    source.Play();

    Debug.Log(
        $"[ShieldShatterEffect] Playing shatter sound: {sound.name}, " +
        $"length: {sound.length:F2}s"
    );

    Destroy(audioObject, sound.length + 0.5f);
}
else
{
    Debug.LogError(
        "[ShieldShatterEffect] Shatter sound is NULL. " +
        "Assign a sound clip to the Shield component."
    );
}

        effect.StartSlowMotion(
            Mathf.Clamp(slowScale, 0.05f, 1f),
            Mathf.Max(0.1f, slowDuration)
        );

        Destroy(root, Mathf.Max(0.5f, lifetime) + 0.25f);
    }

    private void SpawnShards(
        Vector3 centre,
        Vector3 shieldScale,
        Material sourceMaterial,
        int count,
        float spread,
        float lifetime)
    {
        float sizeFactor = Mathf.Max(
            1f,
            (
                Mathf.Abs(shieldScale.x) +
                Mathf.Abs(shieldScale.y) +
                Mathf.Abs(shieldScale.z)
            ) / 3f
        );

        // Generate flying, glass-like shield fragments.
        for (int i = 0; i < count; i++)
        {
            GameObject shard =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            shard.name = "ShieldGlassShard";

            shard.transform.position =
                centre + Random.insideUnitSphere * (0.12f * sizeFactor);

            shard.transform.rotation = Random.rotation;

            shard.transform.localScale = new Vector3(
                Random.Range(0.06f, 0.18f) * sizeFactor,
                Random.Range(0.12f, 0.34f) * sizeFactor,
                Random.Range(0.015f, 0.045f) * sizeFactor
            );

            Collider shardCollider = shard.GetComponent<Collider>();

            if (shardCollider != null)
            {
                Destroy(shardCollider);
            }

            Renderer shardRenderer = shard.GetComponent<Renderer>();

            if (sourceMaterial != null)
            {
                shardRenderer.material = sourceMaterial;
            }

            Rigidbody body = shard.AddComponent<Rigidbody>();

            body.mass = Random.Range(0.05f, 0.25f);
            body.linearDamping = 0.15f;
            body.angularDamping = 0.05f;

            Vector3 direction = (
                Random.onUnitSphere +
                Vector3.up * Random.Range(0.1f, 0.8f)
            ).normalized;

            body.AddForce(
                direction * Random.Range(spread * 0.45f, spread),
                ForceMode.Impulse
            );

            body.AddTorque(
                Random.insideUnitSphere * spread,
                ForceMode.Impulse
            );

            Destroy(shard, Mathf.Max(0.5f, lifetime));
        }

        // Create a burst of bright blue energy sparks.
        GameObject sparksObject =
            new GameObject("ShieldShatterSparks");

        sparksObject.transform.position = centre;

        ParticleSystem sparks =
            sparksObject.AddComponent<ParticleSystem>();

        var main = sparks.main;
        main.loop = false;
        main.duration = 0.35f;
        main.startLifetime =
            new ParticleSystem.MinMaxCurve(0.2f, 0.65f);
        main.startSpeed =
            new ParticleSystem.MinMaxCurve(2f, spread * 1.6f);
        main.startSize =
            new ParticleSystem.MinMaxCurve(0.025f, 0.09f);
        main.startColor = new Color(0.35f, 0.8f, 1f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sparks.emission;
        emission.rateOverTime = 0;

        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(
                0f,
                (short)Mathf.Clamp(count * 4, 40, 300)
            )
        });

        var shape = sparks.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f * sizeFactor;

        // FIX: use a distinct variable name to avoid CS0136.
        var sparksRenderer =
            sparks.GetComponent<ParticleSystemRenderer>();

        sparksRenderer.renderMode =
            ParticleSystemRenderMode.Billboard;

        if (sourceMaterial != null)
        {
            sparksRenderer.material = sourceMaterial;
        }

        sparks.Play();

        Destroy(sparksObject, 1.5f);
    }

    private void StartSlowMotion(float targetScale, float duration)
    {
        if (runner == null)
        {
            GameObject runnerObject =
                new GameObject("ShieldBreakSlowMotionRunner");

            DontDestroyOnLoad(runnerObject);

            runner =
                runnerObject.AddComponent<ShieldShatterEffect>();
        }

        // Cancel an existing effect without losing the original
        // normal game speed.
        if (slowMotionRoutine != null)
        {
            runner.StopCoroutine(slowMotionRoutine);
            slowMotionRoutine = null;

            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
        else
        {
            originalTimeScale = Time.timeScale;
            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        originalTimeScale = Mathf.Max(0.01f, originalTimeScale);
        originalFixedDeltaTime =
            Mathf.Max(0.001f, originalFixedDeltaTime);

        slowMotionRoutine = runner.StartCoroutine(
            RunSlowMotion(targetScale, duration)
        );
    }

    private IEnumerator RunSlowMotion(
        float targetScale,
        float duration)
    {
        float normalScale = originalTimeScale;
        float normalFixedDelta = originalFixedDeltaTime;

        Time.timeScale = targetScale;
        Time.fixedDeltaTime = normalFixedDelta * targetScale;

        // Realtime wait continues even while the game is slowed.
        yield return new WaitForSecondsRealtime(duration);

        // Smoothly restore normal speed using unscaled time.
        float elapsed = 0f;
        const float restoreDuration = 0.22f;

        while (elapsed < restoreDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / restoreDuration);

            Time.timeScale = Mathf.Lerp(
                targetScale,
                normalScale,
                t
            );

            Time.fixedDeltaTime =
                normalFixedDelta * Time.timeScale;

            yield return null;
        }

        // Guarantee exact restoration.
        Time.timeScale = normalScale;
        Time.fixedDeltaTime = normalFixedDelta;

        slowMotionRoutine = null;
    }
}
