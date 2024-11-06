using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    //Serialize Params

    //Cached Comps
    Animator animator;

    //State

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void RodCharge(float amount)
    {
        animator.speed = 0;
        animator.Play("rodCharge", -1, amount);
    }

    public void StartThrow()
    {
        animator.speed = 1;
        animator.SetTrigger("rodThrow");
    }

    public void SetThrowAim(float aimDirX, float charge)
    {
        animator.SetFloat("rodThrowBlendX", aimDirX);
        animator.SetFloat("rodThrowBlendY", charge);
    }

    public void SetRodMove(Vector2 rodMove)
    {
        animator.SetFloat("rodMoveX", rodMove.x);
        animator.SetFloat("rodMoveY", rodMove.y);
    }
}
