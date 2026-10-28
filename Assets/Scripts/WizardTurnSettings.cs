
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class WizardTurnSettings : MonoBehaviour
{
    [Header("Turning Providers")]
    public ContinuousTurnProvider continuousTurn;
    public SnapTurnProvider snapTurn;

    [Header("Input Actions")]
    public InputActionReference smoothTurnAction;
    public InputActionReference snapTurnAction;

    private bool useSmoothTurning = true;

    void Start()
    {
        ApplyTurnMode();
    }

    public void ToggleTurnMode()
    {
        useSmoothTurning = !useSmoothTurning;
        ApplyTurnMode();
    }

    private void ApplyTurnMode()
    {
        if (continuousTurn != null)
            continuousTurn.enabled = useSmoothTurning;

        if (snapTurn != null)
            snapTurn.enabled = !useSmoothTurning;

        if (smoothTurnAction != null)
        {
            if (useSmoothTurning)
                smoothTurnAction.action.Enable();
            else
                smoothTurnAction.action.Disable();
        }

        if (snapTurnAction != null)
        {
            if (useSmoothTurning)
                snapTurnAction.action.Disable();
            else
                snapTurnAction.action.Enable();
        }

        Debug.Log(useSmoothTurning
            ? "Turning mode: Smooth"
            : "Turning mode: Snap");
    }
}
