using UnityEngine;

public class DonateZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        // �±� Ȯ������ ���͸�
        if (!other.CompareTag("Mineral")) return;

        if (other.TryGetComponent<Mineral>(out var mineral))
        {
            if (mineral.isDonated) return; // �ߺ� ��� ����
            mineral.isDonated = true;

            bool donated = InGameManager.Instance.DonateMineral(mineral.data);

            if (donated)
            {
                // ���������� ��ε� ��츸 ����
                Destroy(other.gameObject);
            }
        }
    }
}
