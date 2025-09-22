using TMPro;
using UnityEngine;
using UnityEngine.UI; // Image와 Text를 사용하기 위해 필요

public class CollectionSlot : MonoBehaviour
{
    [Header("UI 요소")]
    public Image mineralIcon;       // 미네랄 아이콘 이미지
    public TextMeshProUGUI progressText;       // 진행도 텍스트 (예: "20%")
    public TextMeshProUGUI nameText;       // 미네랄 이름 텍스트
    public TextMeshProUGUI sellPriceText;       // 판매 가격
    public TextMeshProUGUI droneUpgradeText;       // 드론 업그레이드
    public Sprite unknownIcon;

    private MineralData associatedMineral; // 이 슬롯이 담당하는 미네랄 데이터

    // 외부(CollectionPage)에서 이 슬롯을 초기화할 때 호출하는 함수
    public void InitializeSlot(MineralData mineralData)
    {
        associatedMineral = mineralData;
        
        // InGameManager에서 현재 진행도 가져오기
        int currentProgress = InGameManager.Instance.collectionProgress.TryGetValue(mineralData.mineralName, out int progress)
            ? progress
            : 0;
        
        UpdateSlot(currentProgress);
    }

    // 외부(CollectionPage)에서 진행도를 업데이트할 때 호출하는 함수
    public void UpdateSlot(int progress)
    {
        // 1. 텍스트 업데이트
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
            mineralIcon.color = new Color(1f, 1f, 1f, 0.8f); // Unknown은 약간 반투명하게
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

    // 이 슬롯이 어떤 미네랄을 담당하는지 이름을 반환하는 함수
    public string GetMineralName()
    {
        return associatedMineral.mineralName;
    }
}