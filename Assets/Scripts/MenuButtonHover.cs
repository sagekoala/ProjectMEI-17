using UnityEngine;
using UnityEngine.EventSystems;

// Tells the wizard menu when a pointer starts hovering one of its
// buttons, for hover feedback.
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler
{
    public WizardControlMenu menu;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menu != null)
            menu.OnButtonHovered();
    }
}
