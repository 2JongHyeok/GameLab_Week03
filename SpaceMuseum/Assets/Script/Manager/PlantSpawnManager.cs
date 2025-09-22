using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelPrefabSet
{
    [Tooltip("이 세트가 적용될 타일 레벨")]
    public int level = 1;
    [Tooltip("이 레벨에서 사용할 후보 프리팹들 (앞에서부터 level개 사용)")]
    public List<GameObject> prefabs = new List<GameObject>();
}

public class PlantSpawnManager : MonoBehaviour
{
    public static PlantSpawnManager Instance;

    [Header("레벨별 프리팹 세트")]
    [Tooltip("각 타일 level 값에 대응하는 프리팹 목록. 동일 level이 여러 개면 첫 번째 세트만 사용")]
    public List<LevelPrefabSet> levelSets = new List<LevelPrefabSet>();

    [Header("타일당 스폰 개수")]
    public int minSpawnPerTile = 12;
    public int maxSpawnPerTile = 16;

    [Header("겹침/충돌 검사")]
    [Tooltip("식물/미네랄/바위 등 겹치면 안되는 모든 레이어 포함")]
    public LayerMask obstacleLayer;
    [Tooltip("지면 레이어(위→아래 Raycast용)")]
    public LayerMask groundLayer;

    [Tooltip("개체 간 최소 간격(겹침 방지)")]
    public float minimumSpawnDistance = 6f;

    [Tooltip("스폰 위치 탐색 최대 시도(개체당)")]
    public int maxAttemptsPerObject = 30;

    [Tooltip("NonAlloc 버퍼 크기(겹치는 후보 수 추정치)")]
    public int nonAllocBufferSize = 32;

    [Header("정렬 옵션")]
    public bool randomYRotation = true;
    public bool alignToHitNormal = false; // 경사면에 따라 식물 기울이기
    public float groundRayUp = 30f;       // 타일 중점 위쪽 오프셋
    public float groundRayDown = 80f;     // 아래로 사거리

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

            // 이 타일에서 실제 사용할 서로 다른 프리팹 N개 (N = level, 단 후보가 적으면 후보 수)
            int typeCount = Mathf.Min(level, prefabsForLevel.Count);
            var types = prefabsForLevel.GetRange(0, typeCount);

            // 타일 AABB
            var tileCol = tile.GetComponent<Collider>();
            if (tileCol == null) continue;

            Vector3 center = tileCol.bounds.center;
            Vector3 size = tileCol.bounds.size;

            int spawnCount = Random.Range(minSpawnPerTile, maxSpawnPerTile + 1);
            // 종류별 균등 분배(잔여는 앞에서부터 +1)
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
                        // 1) 타일 AABB 내부 무작위(XZ)
                        float rx = Random.Range(-size.x * 0.5f, size.x * 0.5f);
                        float rz = Random.Range(-size.z * 0.5f, size.z * 0.5f);
                        Vector3 probeTop = new Vector3(center.x + rx, center.y + groundRayUp, center.z + rz);

                        // 2) 위→아래 지면 레이캐스트
                        if (!Physics.Raycast(probeTop, Vector3.down, out RaycastHit hit, groundRayUp + groundRayDown, groundLayer, QueryTriggerInteraction.Ignore))
                            continue;

                        Vector3 spawnPos = hit.point;
                        Quaternion rot = Quaternion.identity;

                        // 3) 정렬 옵션
                        if (alignToHitNormal)
                            rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        if (randomYRotation)
                            rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * rot;

                        // 4) 레이어 기반 겹침 검사
                        float r = minimumSpawnDistance;
                        int hits = Physics.OverlapSphereNonAlloc(spawnPos, r, overlapBuffer, obstacleLayer, QueryTriggerInteraction.Ignore);
                        if (hits > 0) continue;

                        // 5) 전역 최소 간격(이미 스폰된 위치들과 거리 체크)
                        if (!IsFarEnoughFromSpawned(spawnPos, minimumSpawnDistance))
                            continue;

                        // 6) 스폰
                        Instantiate(prefab, spawnPos, rot);
                        spawnedPositions.Add(spawnPos);

                        placed = true;
                        break;
                    }

                    if (!placed)
                    {
                        // 필요 시 로그
                        // Debug.LogWarning($"[Tile:{tile.name}] {prefab.name} 스폰 실패(혼잡/지형)");
                    }
                }
            }
        }
    }

    // level에 맞는 프리팹 리스트 찾기
    private List<GameObject> GetPrefabsForLevel(int level)
    {
        for (int i = 0; i < levelSets.Count; i++)
        {
            if (levelSets[i].level == level)
                return levelSets[i].prefabs;
        }
        return null;
    }

    // 스폰수를 N종류에 균등 분배(잔여는 앞에서부터 +1)
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
