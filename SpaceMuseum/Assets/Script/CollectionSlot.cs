using TMPro;
using UnityEngine;
using UnityEngine.UI; // Image�� Text�� ����ϱ� ���� �ʿ�

public class CollectionSlot : MonoBehaviour
{
    [Header("UI ���")]
    public Image mineralIcon;       // �̳׶� ������ �̹���
    public TextMeshProUGUI progressText;       // ���൵ �ؽ�Ʈ (��: "20%")
    public TextMeshProUGUI nameText;       // �̳׶� �̸� �ؽ�Ʈ
    public TextMeshProUGUI sellPriceText;       // �Ǹ� ����
    public TextMeshProUGUI droneUpgradeText;       // ��� ���׷��̵�
    public Sprite unknownIcon;

    private MineralData associatedMineral; // �� ������ ����ϴ� �̳׶� ������

    // �ܺ�(CollectionPage)���� �� ������ �ʱ�ȭ�� �� ȣ���ϴ� �Լ�
    public void InitializeSlot(MineralData mineralData)
    {
        associatedMineral = mineralData;
        
        // InGameManager���� ���� ���൵ ��������
        int currentProgress = InGameManager.Instance.collectionProgress.TryGetValue(mineralData.mineralName, out int progress)
            ? progress
            : 0;
        
        UpdateSlot(currentProgress);
    }

    // �ܺ�(CollectionPage)���� ���൵�� ������Ʈ�� �� ȣ���ϴ� �Լ�
    public void UpdateSlot(int progress)
    {
        // 1. �ؽ�Ʈ ������Ʈ
        if (progressText != null)
        {
            progressText.text = progress + "%";
            sellPriceText.text = associatedMineral.price+" Byte";
        }
        if (progress == 0)
        {
            nameText.text = "???";
        }
        else
        {
            nameText.text = associatedMineral.name;
        }

        if (progress <= 0)
        {
            mineralIcon.sprite = unknownIcon;
            mineralIcon.color = new Color(1f, 1f, 1f, 0.8f); // Unknown�� �ణ �������ϰ�
        }
        else
        {
            mineralIcon.sprite = associatedMineral.mineralIcon;
            float alpha = Mathf.Lerp(0.2f, 1.0f, Mathf.Clamp01(progress / 100f));
            mineralIcon.color = new Color(mineralIcon.color.r, mineralIcon.color.g, mineralIcon.color.b, alpha);
        }
        if (progress >= 100)
        {
            droneUpgradeText.text = "Drone Damage +5";
        }
    }

    // �� ������ � �̳׶��� ����ϴ��� �̸��� ��ȯ�ϴ� �Լ�
    public string GetMineralName()
    {
        return associatedMineral.mineralName;
    }
}