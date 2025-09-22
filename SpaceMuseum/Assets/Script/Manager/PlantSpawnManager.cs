using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelPrefabSet
{
    [Tooltip("�� ��Ʈ�� ����� Ÿ�� ����")]
    public int level = 1;
    [Tooltip("�� �������� ����� �ĺ� �����յ� (�տ������� level�� ���)")]
    public List<GameObject> prefabs = new List<GameObject>();
}

public class PlantSpawnManager : MonoBehaviour
{
    public static PlantSpawnManager Instance;

    [Header("������ ������ ��Ʈ")]
    [Tooltip("�� Ÿ�� level ���� �����ϴ� ������ ���. ���� level�� ���� ���� ù ��° ��Ʈ�� ���")]
    public List<LevelPrefabSet> levelSets = new List<LevelPrefabSet>();

    [Header("Ÿ�ϴ� ���� ����")]
    public int minSpawnPerTile = 12;
    public int maxSpawnPerTile = 16;

    [Header("��ħ/�浹 �˻�")]
    [Tooltip("�Ĺ�/�̳׶�/���� �� ��ġ�� �ȵǴ� ��� ���̾� ����")]
    public LayerMask obstacleLayer;
    [Tooltip("���� ���̾�(����Ʒ� Raycast��)")]
    public LayerMask groundLayer;

    [Tooltip("��ü �� �ּ� ����(��ħ ����)")]
    public float minimumSpawnDistance = 6f;

    [Tooltip("���� ��ġ Ž�� �ִ� �õ�(��ü��)")]
    public int maxAttemptsPerObject = 30;

    [Tooltip("NonAlloc ���� ũ��(��ġ�� �ĺ� �� ����ġ)")]
    public int nonAllocBufferSize = 32;

    [Header("���� �ɼ�")]
    public bool randomYRotation = true;
    public bool alignToHitNormal = false; // ���鿡 ���� �Ĺ� ����̱�
    public float groundRayUp = 30f;       // Ÿ�� ���� ���� ������
    public float groundRayDown = 80f;     // �Ʒ��� ��Ÿ�

    private Collider[] overlapBuffer;
    private readonly List<Vector3> spawnedPositions = new List<Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        overlapBuffer = new Collider[Mathf.Max(8, nonAllocBufferSize)];
        SpawnByTiles();
    }

    public void SpawnByTiles()
    {
        var tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (var tile in tiles)
        {
            if (tile == null) continue;

            int level = tile.level;
            var prefabsForLevel = GetPrefabsForLevel(level);
            if (prefabsForLevel == null || prefabsForLevel.Count == 0) continue;

            // �� Ÿ�Ͽ��� ���� ����� ���� �ٸ� ������ N�� (N = level, �� �ĺ��� ������ �ĺ� ��)
            int typeCount = Mathf.Min(level, prefabsForLevel.Count);
            var types = prefabsForLevel.GetRange(0, typeCount);

            // Ÿ�� AABB
            var tileCol = tile.GetComponent<Collider>();
            if (tileCol == null) continue;

            Vector3 center = tileCol.bounds.center;
            Vector3 size = tileCol.bounds.size;

            int spawnCount = Random.Range(minSpawnPerTile, maxSpawnPerTile + 1);
            // ������ �յ� �й�(�ܿ��� �տ������� +1)
            var quotas = SplitQuota(spawnCount, typeCount);

            for (int t = 0; t < typeCount; t++)
            {
                var prefab = types[t];
                int quota = quotas[t];

                for (int i = 0; i < quota; i++)
                {
                    bool placed = false;

                    for (int attempt = 0; attempt < maxAttemptsPerObject; attempt++)
                    {
                        // 1) Ÿ�� AABB ���� ������(XZ)
                        float rx = Random.Range(-size.x * 0.5f, size.x * 0.5f);
                        float rz = Random.Range(-size.z * 0.5f, size.z * 0.5f);
                        Vector3 probeTop = new Vector3(center.x + rx, center.y + groundRayUp, center.z + rz);

                        // 2) ����Ʒ� ���� ����ĳ��Ʈ
                        if (!Physics.Raycast(probeTop, Vector3.down, out RaycastHit hit, groundRayUp + groundRayDown, groundLayer, QueryTriggerInteraction.Ignore))
                            continue;

                        Vector3 spawnPos = hit.point;
                        Quaternion rot = Quaternion.identity;

                        // 3) ���� �ɼ�
                        if (alignToHitNormal)
                            rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        if (randomYRotation)
                            rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * rot;

                        // 4) ���̾� ��� ��ħ �˻�
                        float r = minimumSpawnDistance;
                        int hits = Physics.OverlapSphereNonAlloc(spawnPos, r, overlapBuffer, obstacleLayer, QueryTriggerInteraction.Ignore);
                        if (hits > 0) continue;

                        // 5) ���� �ּ� ����(�̹� ������ ��ġ��� �Ÿ� üũ)
                        if (!IsFarEnoughFromSpawned(spawnPos, minimumSpawnDistance))
                            continue;

                        // 6) ����
                        Instantiate(prefab, spawnPos, rot);
                        spawnedPositions.Add(spawnPos);

                        placed = true;
                        break;
                    }

                    if (!placed)
                    {
                        // �ʿ� �� �α�
                        // Debug.LogWarning($"[Tile:{tile.name}] {prefab.name} ���� ����(ȥ��/����)");
                    }
                }
            }
        }
    }

    // level�� �´� ������ ����Ʈ ã��
    private List<GameObject> GetPrefabsForLevel(int level)
    {
        for (int i = 0; i < levelSets.Count; i++)
        {
            if (levelSets[i].level == level)
                return levelSets[i].prefabs;
        }
        return null;
    }

    // �������� N������ �յ� �й�(�ܿ��� �տ������� +1)
    private List<int> SplitQuota(int total, int kinds)
    {
        var result = new List<int>(kinds);
        int baseQ = total / kinds;
        int rem = total % kinds;

        for (int i = 0; i < kinds; i++)
            result.Add(baseQ + (i < rem ? 1 : 0));

        return result;
    }

    private bool IsFarEnoughFromSpawned(Vector3 pos, float minDist)
    {
        float minSqr = minDist * minDist;
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if ((spawnedPositions[i] - pos).sqrMagnitude < minSqr)
                return false;
        }
        return true;
    }
}
