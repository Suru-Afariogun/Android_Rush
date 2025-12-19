using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator anim;

    public void SetMoving(bool value)
    {
        if (anim == null) return;
        anim.SetBool("IsMoving", value);
        // Only trigger StartMoving when starting to move (not when stopping)
        if (value) 
        {
            anim.SetTrigger("StartMoving");
        }
        // When stopping, make sure IsMoving is false (bool handles transition back to idle)
    }

    public void SetGrounded(bool value)
    {
        if (anim == null) return;
        anim.SetBool("IsGrounded",value);
    }

    public void TriggerGroundAttackA() => anim?.SetTrigger("AttackGroundA");
    public void TriggerGroundAttackB() => anim?.SetTrigger("AttackGroundB");
    public void TriggerAirAttackA()    => anim?.SetTrigger("AttackAirA");
    public void TriggerAirAttackB()    => anim?.SetTrigger("AttackAirB");
    public void TriggerRecover()       => anim?.SetTrigger("Recover");
}