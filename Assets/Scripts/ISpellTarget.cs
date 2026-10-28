using UnityEngine;

// Implemented by objects that react when a spell hits them.
public interface ISpellTarget
{
    void OnSpellHit(Vector3 point, Color spellColor);
}
