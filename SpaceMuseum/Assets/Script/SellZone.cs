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
        // 태그 확인으로 필터링
        if (!collision.gameObject.CompareTag("Mineral")) return;

        if (collision.gameObject.TryGetComponent<Mineral>(out var mineral)) { 
            int price = mineral.data.price;

            // GameManager에 돈을 추가해달라고 요청합니다.
            igm.AddBytes(price);

            // 팔린 미네랄 오브젝트는 파괴합니다.
            Destroy(collision.gameObject);
        }
    }
}