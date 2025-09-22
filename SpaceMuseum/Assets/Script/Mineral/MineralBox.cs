using UnityEngine;
using UnityEngine.UI; // Slider를 사용하기 위해 추가
using System.Collections.Generic;

public class MineralBox : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public GameObject mineralPrefab;
    public List<MineralData> mineralDropTable;
    public GameObject interactionPromptPrefab; // 월드 스페이스 UI 프리팹
    private PlayerInteraction currentPlayerInteraction; // 플레이어 상호작용 스크립트 저장

    private GameObject promptInstance;
    private Slider interactionSlider; // 월드 UI에 있는 슬라이더
    private bool isOpened = false;    // 박스가 중복 열리지 않도록 방지

    [HideInInspector] public int stageIndex;
    [HideInInspector] public Tile parentTile;

    // OnInteract는 이제 플레이어로부터 진행률을 받아 슬라이더를 업데이트합니다.
    public void OnHoldInteract(float progress)
    {
        if (interactionSlider != null)
        {
            interactionSlider.value = progress;
        }

        if (progress >= 1.0f)
        {
            Open();
        }
    }
    public void OnInstantInteract() { }

    public void Open()
    {
        if (isOpened) return; // 중복 방지
        isOpened = true;

        SpawnRandomMineral();

        currentPlayerInteraction?.ClearInteractable(this);

        // UI 정리
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }

        if (parentTile != null)
        {
            MineralSpawnManager.Instance.RespawnBox(stageIndex, parentTile);
        }

        // 자신은 제거
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayerInteraction = other.GetComponent<PlayerInteraction>();
            other.GetComponent<PlayerInteraction>()?.SetInteractable(this);

            if (interactionPromptPrefab != null && promptInstance == null)
            {
                promptInstance = Instantiate(interactionPromptPrefab, transform);
                interactionSlider = promptInstance.GetComponentInChildren<Slider>();
                if (interactionSlider != null)
                {
                    interactionSlider.value = 0;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerInteraction>()?.ClearInteractable(this);

            if (promptInstance != null)
            {
                Destroy(promptInstance);
                interactionSlider = null;
            }
        }
    }

    private void SpawnRandomMineral()
    {
        if (mineralPrefab == null || mineralDropTable == null || mineralDropTable.Count == 0) return;

        float totalProbability = 0f;
        foreach (var data in mineralDropTable) totalProbability += data.spawnProbability;
        if (totalProbability <= 0f) return;

        float randomValue = Random.Range(0, totalProbability);
        float cumulative = 0f;
        MineralData selectedMineral = null;

        foreach (var data in mineralDropTable)
        {
            cumulative += data.spawnProbability;
            if (randomValue <= cumulative)
            {
                selectedMineral = data;
                break;
            }
        }

        if (selectedMineral != null)
        {
            // 드롭 위치를 살짝 위로 띄움
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject mineralObject = Instantiate(mineralPrefab, spawnPos, Quaternion.identity);

            // 안전한 컴포넌트 접근
            if (mineralObject.TryGetComponent<Renderer>(out var rend))
            {
                rend.material = selectedMineral.mineralMaterial;
            }

            if (mineralObject.TryGetComponent<Mineral>(out var mineralComp))
            {
                mineralComp.data = selectedMineral;
            }

            mineralObject.name = selectedMineral.mineralName;
        }
    }
}
