using UnityEngine;

public class SellZone : MonoBehaviour
{
    InGameManager igm;
    private void Start()
    {
        igm = InGameManager.Instance;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // �±� Ȯ������ ���͸�
        if (!collision.gameObject.CompareTag("Mineral")) return;

        if (collision.gameObject.TryGetComponent<Mineral>(out var mineral)) { 
            int price = mineral.data.price;

            // GameManager�� ���� �߰��ش޶�� ��û�մϴ�.
            igm.AddBytes(price);

            // �ȸ� �̳׶� ������Ʈ�� �ı��մϴ�.
            Destroy(collision.gameObject);
        }
    }
}