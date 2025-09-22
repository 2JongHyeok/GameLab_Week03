using UnityEngine;

public class SellZone : MonoBehaviour
{
    InGameManager igm;
    private void Start()
    {
        igm = InGameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        // �±� Ȯ������ ���͸�
        if (!other.CompareTag("Mineral")) return;

        if (other.TryGetComponent<Mineral>(out var mineral))
        {
            int price = mineral.data.price;

            // GameManager�� ���� �߰��ش޶�� ��û�մϴ�.
            igm.AddBytes(price);

            // �ȸ� �̳׶� ������Ʈ�� �ı��մϴ�.
            Destroy(other.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}