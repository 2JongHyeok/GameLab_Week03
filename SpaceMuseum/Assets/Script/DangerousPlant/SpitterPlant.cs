using UnityEngine;

public class SpitterPlant : DangerousPlant
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;        // 발사체 프리팹
    public Transform projectileSpawnPoint;     // 발사 위치(없으면 this 사용)
    public float projectileSpeed = 15f;        // 발사 속도
    public float projectileLifeTime = 5f;      // 자동 파괴 시간

    [Header("폭발 설정")]
    public float explosionRadius = 5f;         // 폭발 반경
    public float explosionDamage = 20f;        // 폭발 데미지

    void Update()
    {
        // 수평(Y 고정) 조준만 수행
        if (player != null)
        {
            Vector3 target = player.position;
            target.y = transform.position.y;
            transform.LookAt(target);
        }
    }

    protected override void Attack()
    {
        // 기본 공격: 플레이어 향해 발사체 1발
        if (player == null || projectilePrefab == null) return;

        Transform spawn = projectileSpawnPoint != null ? projectileSpawnPoint : transform;

        // 생성
        GameObject go = Instantiate(projectilePrefab, spawn.position, spawn.rotation);

        // 속도 부여(표준 Rigidbody 사용)
        if (go.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (player.position - spawn.position).normalized;
            rb.linearVelocity = dir * projectileSpeed; // Rigidbody.linearVelocity 아님
        }

        // 스크립트 초기화(있으면)
        if (go.TryGetComponent<SpitterProjectile>(out var proj))
        {
            proj.Initialize(explosionRadius, explosionDamage, projectileLifeTime);
        }
        else
        {
            // 발사체 스크립트가 수명 정리를 안 한다면 최소한 파괴 예약
            if (projectileLifeTime > 0f) Destroy(go, projectileLifeTime);
        }
    }
}
