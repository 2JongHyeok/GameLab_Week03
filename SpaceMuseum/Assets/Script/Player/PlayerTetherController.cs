using UnityEngine;

public class PlayerTetherController : MonoBehaviour
{
    [Header("Tether Settings")]
    public GameObject tetherPrefab;
    public int tetherCount = 10;

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

        if (tetherCount <= 0)
        {
            Debug.Log("테더가 부족합니다!");
            return;
        }

        tetherCount--;

        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 0f;

        var go = Instantiate(tetherPrefab, spawnPosition, Quaternion.identity);

        // ★ 핵심: 즉시 연결을 구축하고 그 다음 네트워크 갱신
        if (go.TryGetComponent<Tether>(out var tether))
        {
            tether.BuildConnections(); // Start() 기다리지 않음
        }

        OxygenNetworkManager.Instance?.UpdateOxygenNetwork();

        Debug.Log($"테더 설치 완료! 남은 개수: {tetherCount}");
    }
}
