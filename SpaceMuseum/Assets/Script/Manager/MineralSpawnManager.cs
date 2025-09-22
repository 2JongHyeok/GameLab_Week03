using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralSpawnManager : MonoBehaviour
{
    public static MineralSpawnManager Instance;

    [Header("스폰 설정")]
    public GameObject[] stageBoxPrefabs; // Stage별 Box 프리팹 (Inspector에서 1,2,3 넣어주기)
    private int minSpawnPerTile = 35;
    private int maxSpawnPerTile = 40;
    public LayerMask obstacleLayer;
    public LayerMask groundLayer;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private float boxRadius = 2f;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (stageBoxPrefabs != null && stageBoxPrefabs.Length > 0)
        {
            Collider col = stageBoxPrefabs[0].GetComponent<Collider>();
            if (col != null)
                boxRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
        }

        SpawnByTiles();
    }

    void SpawnByTiles()
    {
        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in tiles)
        {
            int level = tile.level;
            if (level < 1 || level > stageBoxPrefabs.Length) continue;

            int spawnCount = Random.Range(minSpawnPerTile, maxSpawnPerTile + 1);

            Collider tileCol = tile.GetComponent<Collider>();
            if (tileCol == null) continue;

            Vector3 tileCenter = tileCol.bounds.center;
            Vector3 tileSize = tileCol.bounds.size;
            Debug.Log(spawnCount);

            for (int i = 0; i < spawnCount; i++)
            {
                int maxAttempts = 30;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    // 타일 범위 안 랜덤 좌표
                    float randX = Random.Range(-tileSize.x / 2f, tileSize.x / 2f);
                    float randZ = Random.Range(-tileSize.z / 2f, tileSize.z / 2f);
                    Vector3 potentialPosition = new Vector3(
                        tileCenter.x + randX,
                        tileCenter.y + 20f,
                        tileCenter.z + randZ
                    );

                    if (!Physics.Raycast(potentialPosition, Vector3.down, out RaycastHit hit, 100f, groundLayer))
                        continue;

                    potentialPosition = hit.point + Vector3.up * 1f;

                    if (IsPositionValid(potentialPosition))
                    {
                        SpawnBox(potentialPosition, level - 1, tile);
                        spawnedPositions.Add(potentialPosition);
                        break;
                    }
                }
            }
        }

        Debug.Log($"총 {spawnedPositions.Count}개의 광물 상자 스폰 완료!");
    }

    bool IsPositionValid(Vector3 position)
    {
        if (Physics.CheckSphere(position, boxRadius, obstacleLayer))
            return false;

        foreach (var spawnedPos in spawnedPositions)
        {
            if (Vector3.SqrMagnitude(position - spawnedPos) < 15f * 15f)
                return false;
        }
        return true;
    }


    public void RespawnBox(int stageIndex, Tile tile)
    {
        StartCoroutine(RespawnCoroutine(stageIndex, tile));
    }

    private IEnumerator RespawnCoroutine(int stageIndex, Tile tile)
    {
        yield return new WaitForSeconds(600f); // 10분

        if (tile == null) yield break;
        Collider tileCol = tile.GetComponent<Collider>();
        if (tileCol == null) yield break;

        Vector3 tileCenter = tileCol.bounds.center;
        Vector3 tileSize = tileCol.bounds.size;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            float randX = Random.Range(-tileSize.x / 2f, tileSize.x / 2f);
            float randZ = Random.Range(-tileSize.z / 2f, tileSize.z / 2f);
            Vector3 potentialPosition = new Vector3(
                tileCenter.x + randX,
                tileCenter.y + 20f,
                tileCenter.z + randZ
            );

            if (Physics.Raycast(potentialPosition, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            {
                potentialPosition = hit.point + Vector3.up * 1f;

                if (IsPositionValid(potentialPosition))
                {
                    SpawnBox(potentialPosition, stageIndex, tile);
                    break;
                }
            }
        }
    }

    private void SpawnBox(Vector3 position, int stageIndex, Tile parentTile)
    {
        GameObject boxGO = Instantiate(stageBoxPrefabs[stageIndex], position, Quaternion.identity);

        if (boxGO.TryGetComponent<MineralBox>(out var box))
        {
            box.stageIndex = stageIndex;
            box.parentTile = parentTile;
        }
    }
}
