using UnityEngine;

public class DonateZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // �±� Ȯ������ ���͸�
        if (!collision.gameObject.CompareTag("Mineral")) return;

        if (collision.gameObject.TryGetComponent<Mineral>(out var mineral))
        {
            if (mineral.isDonated) return; // �ߺ� ��� ����
            mineral.isDonated = true;

            bool donated = InGameManager.Instance.DonateMineral(mineral.data);

            if (donated)
            {
                // ���������� ��ε� ��츸 ����
                Destroy(collision.gameObject);
            }
        }
    }
}
