using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("산소 부족 데미지")]
    public float damagePerSecond = 10f;
    private float damageTimer = 0f;

    [Header("필수 컴포넌트 연결")]
    public PlayerOxygen oxygenSystem;       // 기존 산소 시스템 스크립트
    public PlayerController playerController; // 플레이어 컨트롤러 스크립트
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

        // 산소가 없는 경우 초당 데미지
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
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        igm.currentHealth -= damage;
        if (igm.currentHealth < 0) igm.currentHealth = 0;
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

        // 조작 비활성화
        if (playerController != null)
            playerController.SetControllable(false);

        MyUIManager.Instance.DieUI();
    }
}
