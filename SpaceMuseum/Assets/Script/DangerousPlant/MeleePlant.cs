using UnityEngine;

public class MeleePlant : DangerousPlant
{
    [Header("근접 공격 설정")]
    public float attackDamage = 100f;
    public Animator animator; // 공격 애니메이션

    protected override void Attack()
    {

        // 1. 공격 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 2. 플레이어에게 데미지 적용
        if (player != null)
        {
            if (pc != null)
            {
                ph.TakeDamage(attackDamage);
            }
        }
    }
}
