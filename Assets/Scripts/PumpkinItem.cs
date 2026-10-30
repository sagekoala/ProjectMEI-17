
using System.Collections.Generic;
using UnityEngine;

// A magic pumpkin. Its size matters: bigger pumpkins are worth more when
// delivered and take goblins longer to eat, but they are heavier to carry
// with telekinesis. Smaller ones are the opposite.
public class PumpkinItem : MonoBehaviour
{
    [Tooltip("Points for delivering it at its original size.")]
    public int pointValue = 10;

    // Scale it spawned with.
    public Vector3 BaseScale { get; private set; }

    // 1 = original size, 2 = twice as big, 0.5 = half.
    public float SizeMultiplier =>
        BaseScale.x > 0f ? transform.localScale.x / BaseScale.x : 1f;

    // Points for delivering it now, scaled with its size.
    public int DeliveryPoints =>
        Mathf.Max(1, Mathf.RoundToInt(pointValue * SizeMultiplier));

    // Every active pumpkin, so goblins can find them without searching
    // the whole scene each frame.
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
