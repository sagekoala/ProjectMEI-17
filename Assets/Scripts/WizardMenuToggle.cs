
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardMenuToggle : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuPanel;

    [Header("Quest 3 Input")]
    public InputActionReference toggleAction;

    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (menuPanel == null || toggleAction == null ||
            toggleAction.action == null)
        {
            return;
        }

        if (toggleAction.action.WasPressedThisFrame())
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }
}
