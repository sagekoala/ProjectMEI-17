
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WizardHandIK : MonoBehaviour
{
    [Header("Quest 3 Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Hand Rotation Offsets")]
    public Vector3 leftHandRotationOffset = Vector3.zero;
    public Vector3 rightHandRotationOffset = Vector3.zero;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        // LEFT HAND
        if (leftController != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftHand, 1f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand, 1f
            );

            animator.SetIKPosition(
                AvatarIKGoal.LeftHand,
                leftController.position
            );

            animator.SetIKRotation(
                AvatarIKGoal.LeftHand,
                leftController.rotation *
                Quaternion.Euler(leftHandRotationOffset)
            );
        }

        // RIGHT HAND
        if (rightController != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightHand, 1f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand, 1f
            );

            animator.SetIKPosition(
                AvatarIKGoal.RightHand,
                rightController.position
            );

            animator.SetIKRotation(
                AvatarIKGoal.RightHand,
                rightController.rotation *
                Quaternion.Euler(rightHandRotationOffset)
            );
        }
    }
}
