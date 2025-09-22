using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance { get; private set; }

    [Header("성능 설정")]
    [Tooltip("플레이어와의 이 거리 안에 있는 식물만 활성화되어 공격 로직을 수행합니다.")]
    public float activationRadius = 70f;

    [Tooltip("활성화할 식물을 찾는 주기를 설정합니다. (단위: 초)")]
    public float updateInterval = 0.5f;

    // 리스트 관리
    private readonly List<DangerousPlant> allPlants = new List<DangerousPlant>(256);
    private readonly List<DangerousPlant> activePlants = new List<DangerousPlant>(128);

    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        TryResolvePlayer();
        // timeScale의 영향을 받지 않게 하려면 아래 코루틴 방식으로 교체할 수 있음.
        InvokeRepeating(nameof(UpdateActivePlants), 0f, updateInterval);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        CancelInvoke();
    }

    private void Update()
    {
        // 활성 식물만 틱
        for (int i = 0; i < activePlants.Count; i++)
        {
            var plant = activePlants[i];
            if (plant != null)
                plant.OnUpdateTick();
        }
    }

    public void RegisterPlant(DangerousPlant plant)
    {
        if (plant == null) return;
        if (!allPlants.Contains(plant))
            allPlants.Add(plant);
        // 필요 시 즉시 갱신하고 싶다면 아래 주석 해제
        // DirtyFlag를 두었다가 다음 UpdateActivePlants에서 반영하는 식도 가능
        // UpdateActivePlants();
    }

    public void UnregisterPlant(DangerousPlant plant)
    {
        if (plant == null) return;
        allPlants.Remove(plant);
        activePlants.Remove(plant);
    }

    private void UpdateActivePlants()
    {
        // 플레이어 참조가 유실된 경우 재시도
        if (playerTransform == null && !TryResolvePlayer())
        {
            activePlants.Clear();
            return;
        }

        float sqrRadius = activationRadius * activationRadius;

        // active 갱신(단순 방식: 전체 재구성)
        activePlants.Clear();

        // foreach 대신 for(스냅샷)로 안전 순회
        int count = allPlants.Count;
        for (int i = 0; i < count; i++)
        {
            var plant = allPlants[i];
            if (plant == null)
                continue;

            float distanceSqr = (playerTransform.position - plant.transform.position).sqrMagnitude;
            if (distanceSqr <= sqrRadius)
            {
                activePlants.Add(plant);
            }
        }

        // 가끔 null 클린업(선택): 비용을 분산하려면 프레임마다 일부만 정리
        // 아래는 간단히 전수 정리 예시
        for (int i = allPlants.Count - 1; i >= 0; i--)
        {
            if (allPlants[i] == null) allPlants.RemoveAt(i);
        }
    }

    private bool TryResolvePlayer()
    {
        // 1) PlayerController 싱글톤 우선
        var pc = PlayerController.Instance;
        if (pc != null)
        {
            playerTransform = pc.transform;
            return true;
        }

        // 2) 태그 보조
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            return true;
        }

        // 찾지 못하면 다음 주기에서 재시도
        return false;
    }

    // timeScale 비의존 옵션 (필요시 사용)
    private IEnumerator UpdateLoopRealtime()
    {
        var wait = new WaitForSecondsRealtime(updateInterval);
        while (true)
        {
            UpdateActivePlants();
            yield return wait;
        }
    }
}
