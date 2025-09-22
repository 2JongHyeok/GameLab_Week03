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
            Debug.LogWarning("Tether Prefab�� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (InGameManager.Instance.tetherCount > 0)
        {
            // 2. InGameManager�� �״� ������ 1 ���ҽ�ŵ�ϴ�.
            InGameManager.Instance.tetherCount--;

            // 3. ����� ������ UIManager�� �˷��ݴϴ�.
            MyUIManager.Instance.UpdateTetherCount(InGameManager.Instance.tetherCount);

            Vector3 spawnPosition = transform.position;
            spawnPosition.y = 0f;

            var go = Instantiate(tetherPrefab, spawnPosition, Quaternion.identity);
            if (go.TryGetComponent<Tether>(out var tether))
            {
                tether.BuildConnections(); // Start() ��ٸ��� ����
            }

            OxygenNetworkManager.Instance.UpdateOxygenNetwork();
        }
    }
}
