using UnityEngine;

// Explosion when an enemy dies
public static class GoblinExplosion
{
    const int ChunkCount = 14;
    const float ChunkLifetime = 3f;

    static readonly Color FlashColor = new Color(0.55f, 1f, 0.2f);
    static readonly Color LightColor = new Color(1f, 0.55f, 0.15f);

    public static void Explode(Vector3 center, Material chunkMaterial)
    {
        SpawnFlash(center);
        SpawnLight(center);

        for (int i = 0; i < ChunkCount; i++)
            SpawnChunk(center, chunkMaterial);

        MagicFeedback.Play(MagicFeedback.ExplosionSound, center, 1f);
    }

    static void SpawnFlash(Vector3 center)
    {
        GameObject flash = GlowSphere.Create(
            "Explosion Flash", center, 1.2f, FlashColor
        );

        Object.Destroy(flash, 0.2f);
    }

    static void SpawnLight(Vector3 center)
    {
        GameObject lightObject = new GameObject("Explosion Light");
        lightObject.transform.position = center;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = LightColor;
        light.range = 6f;
        light.intensity = 6f;
        light.shadows = LightShadows.None;

        lightObject.AddComponent<FadingLight>().duration = 0.5f;
    }

    static void SpawnChunk(Vector3 center, Material material)
    {
        GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chunk.name = "Explosion Chunk";
        chunk.transform.position = center + Random.insideUnitSphere * 0.2f;
        chunk.transform.rotation = Random.rotation;
        chunk.transform.localScale = Vector3.one * Random.Range(0.06f, 0.14f);

        Renderer renderer = chunk.GetComponent<Renderer>();
        if (material != null)
            renderer.sharedMaterial = material;
        else
            renderer.material.color = FlashColor;

        // Fly outwards and upwards, spinning.
        Vector3 direction = Random.onUnitSphere;
        direction.y = Mathf.Abs(direction.y) + 0.3f;

        Rigidbody body = chunk.AddComponent<Rigidbody>();
        body.mass = 0.2f;
        body.linearVelocity = direction.normalized * Random.Range(3f, 6f);
        body.angularVelocity = Random.insideUnitSphere * 10f;

        Object.Destroy(chunk, ChunkLifetime + Random.value);
    }
}
