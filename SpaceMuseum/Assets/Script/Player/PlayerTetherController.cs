using UnityEngine;

public class PlayerTetherController : MonoBehaviour
{
    [Header("Tether Settings")]
    public GameObject tetherPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlaceTether();
        }
    }

    void PlaceTether()
    {
        if (!tetherPrefab)
        {
            Debug.LogWarning("Tether Prefab이 설정되지 않았습니다!");
            return;
        }

        if (InGameManager.Instance.tetherCount > 0)
        {
            // 2. InGameManager의 테더 개수를 1 감소시킵니다.
            InGameManager.Instance.tetherCount--;

            // 3. 변경된 개수를 UIManager에 알려줍니다.
            MyUIManager.Instance.UpdateTetherCount(InGameManager.Instance.tetherCount);

            Vector3 spawnPosition = transform.position;
            spawnPosition.y = 0f;

            var go = Instantiate(tetherPrefab, spawnPosition, Quaternion.identity);
            if (go.TryGetComponent<Tether>(out var tether))
            {
                tether.BuildConnections(); // Start() 기다리지 않음
            }

            OxygenNetworkManager.Instance.UpdateOxygenNetwork();
        }
    }
}
