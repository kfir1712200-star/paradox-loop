using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Movement))]
public class PlayerAnimator : MonoBehaviour
{
    Animator animator;
    Movement movement;

    static readonly int SpeedHash       = Animator.StringToHash("Speed");
    static readonly int IsGroundedHash  = Animator.StringToHash("IsGrounded");
    static readonly int IsJumpingHash   = Animator.StringToHash("IsJumping");
    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        animator.SetFloat(SpeedHash,       movement.NormalizedSpeed);
        animator.SetBool(IsGroundedHash,   movement.IsGrounded);
        animator.SetBool(IsJumpingHash,    movement.IsJumping);
        animator.SetBool(IsCrouchingHash,  movement.IsCrouching);
    }
}
