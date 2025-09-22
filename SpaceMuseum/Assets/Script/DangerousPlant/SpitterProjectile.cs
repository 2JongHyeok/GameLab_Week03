using UnityEngine;

public class SpitterProjectile : MonoBehaviour
{
    private float explosionRadius;
    private float explosionDamage;
    private float lifeTime;

    public void Initialize(float radius, float damage, float life)
    {
        explosionRadius = radius;
        explosionDamage = damage;
        lifeTime = life;
        Destroy(gameObject, lifeTime); // 수명 끝나면 자동 폭발
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void OnDestroy()
    {
        // 수명 초과로 파괴될 때도 폭발
        //Explode();
    }

    private void Explode()
    {
        //Debug.Log("씨앗 폭발!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var col in colliders)
        {
            // 플레이어 데미지
            if (col.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(explosionDamage);
            }

            // 테더 파괴
            if (col.TryGetComponent<Tether>(out var tether))
            {
                tether.BreakAllConnections();          // 연결 정리 (라인 제거 포함)
                Destroy(tether.gameObject);
            }
            OxygenNetworkManager.Instance.UpdateOxygenNetwork();
        }

        Destroy(gameObject);
    }
}
