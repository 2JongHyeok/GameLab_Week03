using UnityEngine;
using UnityEngine.UI; // Slider�� ����ϱ� ���� �߰�
using System.Collections.Generic;

public class MineralBox : MonoBehaviour, IInteractable
{
    [Header("����")]
    public GameObject mineralPrefab;
    public List<MineralData> mineralDropTable;
    public GameObject interactionPromptPrefab; // ���� �����̽� UI ������

    private GameObject promptInstance;
    private Slider interactionSlider; // ���� UI�� �ִ� �����̴�
    private bool isOpened = false;    // �ڽ��� �ߺ� ������ �ʵ��� ����

    // OnInteract�� ���� �÷��̾�κ��� ������� �޾� �����̴��� ������Ʈ�մϴ�.
    public void OnInteract(float progress)
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

    public void Open()
    {
        if (isOpened) return; // �ߺ� ����
        isOpened = true;

        SpawnRandomMineral();

        // UI ����
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }

        // TreasureBox �ı�
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
            // ��� ��ġ�� ��¦ ���� ���
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject mineralObject = Instantiate(mineralPrefab, spawnPos, Quaternion.identity);

            // ������ ������Ʈ ����
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
