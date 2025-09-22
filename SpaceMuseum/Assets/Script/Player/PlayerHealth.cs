using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("��� ���� ������")]
    public float damagePerSecond = 10f;
    private float damageTimer = 0f;

    [Header("ȸ�� ����")]
    public float healDelay = 3f;        // ������ ������ �� ȸ������ ��� �ð�
    public float healRate = 20f;        // �ʴ� ȸ����
    private float timeSinceLastDamage = 0f; // ������ ������ �� ��� �ð�

    [Header("�ʼ� ������Ʈ ����")]
    public PlayerOxygen oxygenSystem;       // ���� ��� �ý��� ��ũ��Ʈ
    public PlayerController playerController; // �÷��̾� ��Ʈ�ѷ� ��ũ��Ʈ
    private InGameManager igm;

    private bool isDead = false;

    void Start()
    {
        playerController = PlayerController.Instance;
        igm = InGameManager.Instance;
    }

    void Update()
    {
        if (isDead) return;

        // ��Ұ� ���� ��� �ʴ� ������
        if (oxygenSystem != null && oxygenSystem.currentOxygen <= 0)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= 1f)
            {
                TakeDamage(damagePerSecond);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
        timeSinceLastDamage += Time.deltaTime;

        // ȸ�� ����
        if (timeSinceLastDamage >= healDelay && igm.currentHealth > 0 && igm.currentHealth < igm.maxHealth)
        {
            float healAmount = healRate * Time.deltaTime;
            igm.currentHealth = Mathf.Min(igm.currentHealth + healAmount, igm.maxHealth);
            MyUIManager.Instance.UpdateHealthUI();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        igm.currentHealth -= damage;
        if (igm.currentHealth < 0) igm.currentHealth = 0;
        timeSinceLastDamage = 0f;

        MyUIManager.Instance.UpdateHealthUI();
        if (igm.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (igm.isDead) return;

        igm.isDead = true;

        // ���� ��Ȱ��ȭ
        if (playerController != null)
            playerController.SetControllable(false);

        MyUIManager.Instance.DieUI();
    }
}
