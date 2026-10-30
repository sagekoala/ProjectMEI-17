
using UnityEngine;

// Unlit glowing spheres (spells, flashes). Material is in Resources
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
