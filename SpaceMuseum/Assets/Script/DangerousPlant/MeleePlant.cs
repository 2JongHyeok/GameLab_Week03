using UnityEngine;

public class MeleePlant : DangerousPlant
{
    [Header("���� ���� ����")]
    public float attackDamage = 100f;
    public Animator animator; // ���� �ִϸ��̼�

    protected override void Attack()
    {

        // 1. ���� �ִϸ��̼� ���
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 2. �÷��̾�� ������ ����
        if (player != null)
        {
            if (pc != null)
            {
                ph.TakeDamage(attackDamage);
            }
        }
    }
}
