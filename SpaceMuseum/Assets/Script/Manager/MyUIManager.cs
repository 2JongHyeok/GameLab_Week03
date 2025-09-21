using UnityEngine;
using TMPro;
using UnityEngine.UI; // TextMeshPro를 사용하기 위해 반드시 추가!

public class MyUIManager : MonoBehaviour
{
    // UIManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static MyUIManager Instance { get; private set; }

    [Header("게임 상태 UI")]
    public TextMeshProUGUI tetherCountText; // 에디터에서 연결할 텍스트 UI
    public TextMeshProUGUI byteText;

    [Header("상점 UI")]
    public GameObject shopPanel; // 상점 패널 오브젝트

    [Header("상점 버튼들")]
    public Button buyTetherButton;

    public Button upgradeMoveSpeedButton;
    public TextMeshProUGUI upgradeMoveSpeedPriceText;
    public TextMeshProUGUI upgradeMoveSpeedLevelText;

    public Button upgradeInventoryButton;
    public TextMeshProUGUI upgradeInventoryPriceText;
    public TextMeshProUGUI upgradeInventoryLevelText;
    public TextMeshProUGUI InventoryUIText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // 게임 시작 시 상점은 닫혀있도록 함
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        UpdateAllUI();
    }

    private void Update()
    {
        // 'P' 키를 누르면 상점 UI를 켜고 끔
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (shopPanel == null) return;

        bool isActive = !shopPanel.activeSelf;
        shopPanel.SetActive(isActive);

        // 상점을 열 때마다 버튼 상태를 업데이트
        if (isActive)
        {
            UpdateShopButtons();
        }
    }

    public void UpdateShopButtons()
    {
        InGameManager gm = InGameManager.Instance;
        // 테더 개수가 최대치(99) 이상이면 구매 버튼을 비활성화
        if (buyTetherButton != null)
        {
            buyTetherButton.interactable = (InGameManager.Instance.tetherCount < InGameManager.maxTetherCount 
                                               || gm.bytes < gm.tetherBundlePrice);
        }
        // 이동 속도 업그레이드 버튼 업데이트
        if (upgradeMoveSpeedButton != null)
        {
            if (gm.moveSpeedLevel >= InGameManager.maxMoveSpeedLevel)
            {
                upgradeMoveSpeedButton.interactable = false;
                upgradeMoveSpeedPriceText.text = "Sold Out";
                upgradeMoveSpeedLevelText.text = "Level : Max";
            }
            else if(gm.bytes < gm.moveSpeedUpgradePrice)
            {
                upgradeMoveSpeedButton.interactable = false;
                upgradeMoveSpeedLevelText.text = $"Level.{gm.moveSpeedLevel}";
                upgradeMoveSpeedPriceText.text = $"{gm.moveSpeedUpgradePrice} Bytes";
            }
            else
            {
                upgradeMoveSpeedButton.interactable = true;
                upgradeMoveSpeedLevelText.text = $"Level.{gm.moveSpeedLevel}";
                upgradeMoveSpeedPriceText.text = $"{gm.moveSpeedUpgradePrice} Bytes";
            }
        }

        // 가방 용량 업그레이드 버튼 업데이트
        if (upgradeInventoryButton != null)
        {
            if (gm.bytes < gm.inventoryCapacityUpgradePrice)
            {
                upgradeInventoryButton.interactable = false;
                upgradeInventoryLevelText.text = $"Level.{gm.inventoryCapacityLevel}";
                upgradeInventoryPriceText.text = $"{gm.inventoryCapacityUpgradePrice} Bytes";
            }
            else
            {
                upgradeInventoryButton.interactable = true;
                upgradeInventoryLevelText.text = $"Level.{gm.inventoryCapacityLevel}";
                upgradeInventoryPriceText.text = $"{gm.inventoryCapacityUpgradePrice} Bytes";
            }
        }
    }

    public void OnClick_BuyTethers()
    {
        InGameManager gm = InGameManager.Instance;
        if (gm.bytes >= gm.tetherBundlePrice && gm.tetherCount < InGameManager.maxTetherCount)
        {
            gm.bytes -= gm.tetherBundlePrice;
            gm.tetherCount = Mathf.Min(gm.tetherCount + 10, InGameManager.maxTetherCount);
            UpdateAllUI();
        }
    }

    public void OnClick_UpgradeMoveSpeed()
    {
        InGameManager gm = InGameManager.Instance;
        if (gm.bytes >= gm.moveSpeedUpgradePrice && gm.moveSpeedLevel < InGameManager.maxMoveSpeedLevel)
        {
            gm.bytes -= gm.moveSpeedUpgradePrice;
            gm.moveSpeedLevel++;
            gm.moveSpeedUpgradePrice += 300; // 가격 300원 증가
            UpdateAllUI();
        }
    }
    public void OnClick_UpgradeInventory()
    {
        InGameManager gm = InGameManager.Instance;
        if (gm.bytes >= gm.inventoryCapacityUpgradePrice)
        {
            gm.bytes -= gm.inventoryCapacityUpgradePrice;
            gm.inventoryCapacityLevel++;
            gm.maxWeight += 10;
            gm.inventoryCapacityUpgradePrice += 500; // 가격 500원 증가
            gm.UpdateWeight();
            UpdateAllUI();
        }
    }
    public void UpdateBytes(int amount)
    {
        if (byteText != null)
            byteText.text = $"Byte : {amount}";
    }

    public void UpdateWeightUI(int now, int max)
    {
        Debug.Log(max);
        if (InventoryUIText != null)
            InventoryUIText.text = $"Weight : {now} / {max}";
    }


    private void UpdateAllUI()
    {
        InGameManager gm = InGameManager.Instance;
        UpdateTetherCount(gm.tetherCount);
        UpdateBytes(gm.bytes);
        UpdateShopButtons();
        UpdateWeightUI(gm.currentWeight, gm.maxWeight);
    }
    public void UpdateTetherCount(int count)
    {
        if (tetherCountText != null)
        {
            tetherCountText.text = $"Tether : {count:D2}";
        }
    }
}