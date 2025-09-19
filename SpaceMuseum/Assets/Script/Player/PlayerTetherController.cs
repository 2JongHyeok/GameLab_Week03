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
            Debug.LogWarning("Tether Prefab�� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (tetherCount <= 0)
        {
            Debug.Log("�״��� �����մϴ�!");
            return;
        }

        tetherCount--;

        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 0f;

        var go = Instantiate(tetherPrefab, spawnPosition, Quaternion.identity);

        // �� �ٽ�: ��� ������ �����ϰ� �� ���� ��Ʈ��ũ ����
        if (go.TryGetComponent<Tether>(out var tether))
        {
            tether.BuildConnections(); // Start() ��ٸ��� ����
        }

        OxygenNetworkManager.Instance?.UpdateOxygenNetwork();

        Debug.Log($"�״� ��ġ �Ϸ�! ���� ����: {tetherCount}");
    }
}
