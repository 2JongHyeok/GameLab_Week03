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
        Mineral mineral = collision.gameObject.GetComponent<Mineral>();

        // Mineral ������Ʈ�� �ִٸ� (��, �̳׶��� ��Ҵٸ�)
        if (mineral != null)
        {
            // �̳׶� �����Ϳ��� ���� ������ �����ɴϴ�.
            int price = mineral.data.price;

            // GameManager�� ���� �߰��ش޶�� ��û�մϴ�.
            igm.AddBytes(price);

            // �ȸ� �̳׶� ������Ʈ�� �ı��մϴ�.
            Destroy(collision.gameObject);
        }
    }
}