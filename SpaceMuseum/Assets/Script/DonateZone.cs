using UnityEngine;

public class DonateZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        // 태그 확인으로 필터링
        if (!other.CompareTag("Mineral")) return;

        if (other.TryGetComponent<Mineral>(out var mineral))
        {
            if (mineral.isDonated) return; // 중복 기부 방지
            mineral.isDonated = true;

            bool donated = InGameManager.Instance.DonateMineral(mineral.data);

            if (donated)
            {
                // 성공적으로 기부된 경우만 삭제
                Destroy(other.gameObject);
            }
        }
    }
}
