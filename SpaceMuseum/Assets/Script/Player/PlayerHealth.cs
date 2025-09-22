using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("산소 부족 데미지")]
    public float damagePerSecond = 10f;
    private float damageTimer = 0f;

    [Header("회복 설정")]
    public float healDelay = 3f;        // 마지막 데미지 후 회복까지 대기 시간
    public float healRate = 20f;        // 초당 회복량
    private float timeSinceLastDamage = 0f; // 마지막 데미지 후 경과 시간

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
        timeSinceLastDamage += Time.deltaTime;

        // 회복 로직
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

        // 조작 비활성화
        if (playerController != null)
            playerController.SetControllable(false);

        MyUIManager.Instance.DieUI();
    }
}
