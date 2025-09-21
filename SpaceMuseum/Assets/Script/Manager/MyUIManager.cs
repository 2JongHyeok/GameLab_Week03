using UnityEngine;
using TMPro;
using UnityEngine.UI; // TextMeshPro�� ����ϱ� ���� �ݵ�� �߰�!

public class MyUIManager : MonoBehaviour
{
    // UIManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static MyUIManager Instance { get; private set; }

    [Header("���� ���� UI")]
    public TextMeshProUGUI tetherCountText; // �����Ϳ��� ������ �ؽ�Ʈ UI
    public TextMeshProUGUI byteText;

    [Header("���� UI")]
    public GameObject shopPanel; // ���� �г� ������Ʈ

    [Header("���� ��ư��")]
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
        // ���� ���� �� ������ �����ֵ��� ��
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        UpdateAllUI();
    }

    private void Update()
    {
        // 'P' Ű�� ������ ���� UI�� �Ѱ� ��
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

        // ������ �� ������ ��ư ���¸� ������Ʈ
        if (isActive)
        {
            UpdateShopButtons();
        }
    }

    public void UpdateShopButtons()
    {
        InGameManager gm = InGameManager.Instance;
        // �״� ������ �ִ�ġ(99) �̻��̸� ���� ��ư�� ��Ȱ��ȭ
        if (buyTetherButton != null)
        {
            buyTetherButton.interactable = (InGameManager.Instance.tetherCount < InGameManager.maxTetherCount 
                                               || gm.bytes < gm.tetherBundlePrice);
        }
        // �̵� �ӵ� ���׷��̵� ��ư ������Ʈ
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

        // ���� �뷮 ���׷��̵� ��ư ������Ʈ
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
            gm.moveSpeedUpgradePrice += 300; // ���� 300�� ����
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
            gm.inventoryCapacityUpgradePrice += 500; // ���� 500�� ����
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