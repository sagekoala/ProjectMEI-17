using UnityEngine;
using UnityEngine.EventSystems;

// hover feedback for menu buttons
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler
{
    public WizardControlMenu menu;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menu != null)
            menu.OnButtonHovered();
    }
}
