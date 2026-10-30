
using UnityEngine;

// Glowing spheres for magic effects (spells, flashes, the ready orb).
// They use an unlit material, so they look bright even in the dark
// graveyard; a lit sphere with a light at its center looks almost black
// from outside. The material lives in Assets/Resources so it is always
// included in the build. Colors go through a property block, so no
// material copies are created.
public static class GlowSphere
{
    const string MaterialName = "GlowUnlit";

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    static Material material;
    static MaterialPropertyBlock propertyBlock;

    public static Material Material
    {
        get
        {
            if (material == null)
            {
                material = Resources.Load<Material>(MaterialName);

                if (material == null)
                    Debug.LogWarning($"GlowSphere: Resources/{MaterialName} not found.");
            }

            return material;
        }
    }

    public static GameObject Create(
        string name,
        Vector3 position,
        float size,
        Color color,
        bool keepCollider = false)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * size;

        if (!keepCollider)
            Object.Destroy(sphere.GetComponent<Collider>());

        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        if (Material != null)
            renderer.sharedMaterial = Material;

        SetColor(renderer, color);
        return sphere;
    }

    public static void SetColor(Renderer renderer, Color color)
    {
        if (renderer == null)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, color);
        propertyBlock.SetColor(ColorId, color);
        renderer.SetPropertyBlock(propertyBlock);
    }
}
