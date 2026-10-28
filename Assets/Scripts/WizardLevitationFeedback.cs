
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WizardLevitationFeedback : MonoBehaviour
{
    [Header("Levitation")]
    public WizardLevitation levitation;

    [Header("Ray Settings")]
    public float maxDistance = 4f;
    public float rayWidth = 0.012f;

    [Header("Colors")]
    public Color targetColor = Color.cyan;
    public Color holdingColor = new Color(1f, 0.75f, 0.1f);

    private LineRenderer ray;

    void Awake()
    {
        ray = GetComponent<LineRenderer>();
        ray.positionCount = 2;
        ray.startWidth = rayWidth;
        ray.endWidth = rayWidth;
        ray.useWorldSpace = true;
        ray.enabled = false;
    }

    void LateUpdate()
    {
        if (levitation == null)
        {
            ray.enabled = false;
            return;
        }

        Rigidbody held = levitation.HeldObject;

        if (held != null)
        {
            DrawRay(held.worldCenterOfMass, holdingColor);
            return;
        }

        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hit,
            maxDistance))
        {
            Rigidbody target = hit.rigidbody;

            if (target != null &&
                !target.isKinematic &&
                target.CompareTag("Levitatable"))
            {
                DrawRay(hit.point, targetColor);
                return;
            }
        }

        ray.enabled = false;
    }


    void DrawRay(Vector3 endPoint, Color color)
    {
        ray.enabled = true;
        ray.SetPosition(0, transform.position);
        ray.SetPosition(1, endPoint);

        ray.startColor = color;
        ray.endColor = color;

        // Set the URP material color so it matches the targeting state.
        ray.material.SetColor("_BaseColor", color);
    }

}
