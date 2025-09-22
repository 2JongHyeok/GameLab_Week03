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
        Destroy(gameObject, lifeTime); // ���� ������ �ڵ� ����
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void OnDestroy()
    {
        // ���� �ʰ��� �ı��� ���� ����
        //Explode();
    }

    private void Explode()
    {
        //Debug.Log("���� ����!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var col in colliders)
        {
            // �÷��̾� ������
            if (col.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(explosionDamage);
            }

            // �״� �ı�
            if (col.TryGetComponent<Tether>(out var tether))
            {
                tether.BreakAllConnections();          // ���� ���� (���� ���� ����)
                Destroy(tether.gameObject);
            }
            OxygenNetworkManager.Instance.UpdateOxygenNetwork();
        }

        Destroy(gameObject);
    }
}
