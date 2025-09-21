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

        // Mineral 컴포넌트가 있다면 (즉, 미네랄이 닿았다면)
        if (mineral != null)
        {
            // 미네랄 데이터에서 가격 정보를 가져옵니다.
            int price = mineral.data.price;

            // GameManager에 돈을 추가해달라고 요청합니다.
            igm.AddBytes(price);

            // 팔린 미네랄 오브젝트는 파괴합니다.
            Destroy(collision.gameObject);
        }
    }
}