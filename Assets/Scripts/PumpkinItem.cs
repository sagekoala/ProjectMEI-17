
using System.Collections.Generic;
using UnityEngine;

// Pumpkin. Bigger = more points but heavier
public class PumpkinItem : MonoBehaviour
{
    [Tooltip("Points at normal size")]
    public int pointValue = 10;

    // Scale it spawned with.
    public Vector3 BaseScale { get; private set; }

    // 1 = original size, 2 = twice as big, 0.5 = half.
    public float SizeMultiplier =>
        BaseScale.x > 0f ? transform.localScale.x / BaseScale.x : 1f;

    // Points for delivering it now, scaled with its size.
    public int DeliveryPoints =>
        Mathf.Max(1, Mathf.RoundToInt(pointValue * SizeMultiplier));

    // list of active pumpkins (for the goblins)
    public static readonly List<PumpkinItem> All = new List<PumpkinItem>();

    void Awake()
    {
        BaseScale = transform.localScale;
    }

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }
}
