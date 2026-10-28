using UnityEngine;

// A ghostly soul that drifts around the graveyard, bobbing up and down.
// A spell hit banishes it. Spawned and tracked by SoulHunt.
public class WanderingSoul : MonoBehaviour, ISpellTarget
{
    public SoulHunt hunt;

    public float speed = 0.8f;
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;

    private Vector3 basePosition;
    private Vector3 target;
    private float bobOffset;

    void Start()
    {
        basePosition = transform.position;
        bobOffset = Random.value * 10f;
        target = hunt.RandomPointInArea();
    }

    void Update()
    {
        basePosition = Vector3.MoveTowards(
            basePosition, target, speed * Time.deltaTime
        );

        if ((basePosition - target).sqrMagnitude < 0.01f)
            target = hunt.RandomPointInArea();

        float bob =
            Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;

        transform.position = basePosition + Vector3.up * bob;
    }

    public void OnSpellHit(Vector3 point, Color spellColor)
    {
        hunt.OnSoulBanished(this);
    }
}
