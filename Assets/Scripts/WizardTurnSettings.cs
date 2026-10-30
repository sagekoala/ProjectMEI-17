
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class WizardTurnSettings : MonoBehaviour
{
    [Header("Turning Providers")]
    public ContinuousTurnProvider continuousTurn;
    public SnapTurnProvider snapTurn;

    [Header("Controller Input Managers")]
    [Tooltip("Turn mode is set on the XRI input managers")]
    public ControllerInputActionManager[] inputManagers;

    [Header("Interface")]
    [Tooltip("Optional label that shows the current turn mode.")]
    public TMP_Text modeLabel;

    private bool useSmoothTurning = true;

    void Start()
    {
        // Start from the mode the rig is configured with.
        if (continuousTurn != null)
            useSmoothTurning = continuousTurn.enabled;

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

        // The managers enable the matching turn action (Turn or Snap Turn).
        foreach (ControllerInputActionManager manager in inputManagers)
        {
            if (manager != null)
                manager.smoothTurnEnabled = useSmoothTurning;
        }

        if (modeLabel != null)
        {
            modeLabel.text = useSmoothTurning
                ? "TURN: SMOOTH"
                : "TURN: SNAP";
        }

        Debug.Log(useSmoothTurning
            ? "Turning mode: Smooth"
            : "Turning mode: Snap");
    }
}
