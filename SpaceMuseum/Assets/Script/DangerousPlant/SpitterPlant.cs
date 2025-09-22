using UnityEngine;

public class SpitterPlant : DangerousPlant
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;        // 발사체 프리팹
    public Transform projectileSpawnPoint;     // 발사 위치 (머리 부분)
    public float projectileSpeed = 15f;        // 발사 속도
    public float projectileLifeTime = 5f;      // 자동 파괴 시간

    [Header("폭발 설정")]
    public float explosionRadius = 5f;         // 폭발 반경
    public float explosionDamage = 20f;        // 폭발 데미지

    protected void Update()
    {
        if (player != null)
        {
            Vector3 targetPos = player.position;

            // y축 고정 회전 (머리가 땅속에 파묻히지 않게)
            targetPos.y = transform.position.y;

            transform.LookAt(targetPos);
        }
    }

    protected override void Attack()
    {
        Debug.Log($"[SpitterPlant.Attack] 발사! 시간: {Time.time}");
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        Debug.Log("원거리 식물이 폭발탄을 발사합니다!");

        // 발사체 생성
        GameObject projectileGO = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // 속도 적용
        if (projectileGO.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;
            rb.linearVelocity = direction * projectileSpeed;
        }

        // Projectile 설정
        if (projectileGO.TryGetComponent<SpitterProjectile>(out var proj))
        {
            proj.Initialize(explosionRadius, explosionDamage, projectileLifeTime);
        }
    }
}
