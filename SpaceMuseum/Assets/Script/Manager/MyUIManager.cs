using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // TextMeshPro�� ����ϱ� ���� �ݵ�� �߰�!

public class MyUIManager : MonoBehaviour
{
    // UIManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static MyUIManager Instance { get; private set; }

    [Header("GameStatus UI")]
    public TextMeshProUGUI tetherCountText; // �����Ϳ��� ������ �ؽ�Ʈ UI
    public TextMeshProUGUI byteText;

    [Header("Shop UI")]
    public GameObject shopPanel; // ���� �г� ������Ʈ

    [Header("Shop Buttons")]
    public Button buyTetherButton;

    [Header("Donates UI")]
    public Button collectionBtn;
    public GameObject collectionPanel; // ���� ��ü�� ��� �г�
    public GameObject[] stagePages; // Stage1, Stage2, Stage3 ������ UI���� ���� �迭

    public Button upgradeMoveSpeedButton;
    public TextMeshProUGUI upgradeMoveSpeedPriceText;
    public TextMeshProUGUI upgradeMoveSpeedLevelText;

    public Button upgradeInventoryButton;
    public TextMeshProUGUI upgradeInventoryPriceText;
    public TextMeshProUGUI upgradeInventoryLevelText;
    public TextMeshProUGUI InventoryUIText;
    public ScreenFader screenFader;
    public PlayerOxygen oxygenSystem;
    public PlayerController playerController;
    public Slider playerHpSlider;

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
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(false);
        }
        UpdateAllUI();
    }

    private void Update()
    {
        // 'P' Ű�� ������ ���� UI�� �Ѱ� ��
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (collectionPanel != null && collectionPanel.activeSelf)
            {
                CloseCollectionPanel();
            }
            ToggleShop();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ������ ���������� ������ �ݰ�
            if (shopPanel != null && shopPanel.activeSelf)
            {
                CloseShopPanel();
            }
            // (������ �����ְ�) ������ ���������� ������ �ݴ´�
            else if (collectionPanel != null && collectionPanel.activeSelf)
            {
                CloseCollectionPanel();
            }
        }
    }

    public void OpenCollectionPanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(true);
            // ������ ���� �⺻���� ù ��° ���������� ������
            OnStageTabClicked(0);
            UpdateCollectionDisplay();
        }
    }

    public void UpdateCollectionDisplay()
    {
        // ��� ������ ������Ʈ�� ��ȸ
        foreach (var pageObject in stagePages)
        {
            // ���� �����ִ� �������� ã�Ƽ�
            if (pageObject.activeSelf)
            {
                // �ش� �������� CollectionPage ��ũ��Ʈ���� UI ������Ʈ�� ���
                pageObject.GetComponent<CollectionPage>()?.UpdatePageDisplay();
                // �ϳ� ã������ ���� ����
                break;
            }
        }
    }

    public void OnStageTabClicked(int stageIndex)
    {
        if (stagePages == null || stagePages.Length <= stageIndex) return;
        collectionBtn.gameObject.SetActive(false);

        // ��� �������� �ϴ� �� ����
        for (int i = 0; i < stagePages.Length; i++)
        {
            stagePages[i].SetActive(false);
        }

        // ��û�� �ε����� �������� �Ҵ�
        stagePages[stageIndex].SetActive(true);

        CollectionPage collectionPage = stagePages[stageIndex].GetComponent<CollectionPage>();
        if (collectionPage != null)
        {
            // 1. �������� �ʱ�ȭ�Ͽ� ���Ե��� �����Ѵ� (���� ���� �� ���� ��쿡�� �����)
            collectionPage.InitializePage();

            // 2. ������ ���Ե��� ������ �ֽ� �����ͷ� ������Ʈ�Ѵ�
            collectionPage.UpdatePageDisplay();
        }
    }

    // ���� �ݱ� (X ��ư �Ǵ� ESCŰ�� ȣ��)
    public void CloseCollectionPanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(false);
            collectionBtn.gameObject.SetActive(true);
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

    public void CloseShopPanel()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(false);
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
            gm.moveSpeed += 2;
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
    public void UpdateBytes()
    {
        if (byteText != null)
            byteText.text = $"Byte : {InGameManager.Instance.bytes}";
    }

    public void UpdateWeightUI(int now, int max)
    {
        if (InventoryUIText != null)
            InventoryUIText.text = $"Weight : {now} / {max}";
    }


    private void UpdateAllUI()
    {
        InGameManager gm = InGameManager.Instance;
        UpdateTetherCount(gm.tetherCount);
        UpdateBytes();
        UpdateShopButtons();
        UpdateWeightUI(gm.currentWeight, gm.maxWeight);
        UpdateHealthUI();
    }
    public void UpdateTetherCount(int count)
    {
        if (tetherCountText != null)
        {
            tetherCountText.text = $"Tether : {count:D2}";
        }
    }

    public void DieUI()
    {
        StartCoroutine(DieAndRespawnRoutine());
    }

    private IEnumerator DieAndRespawnRoutine()
    {
        InGameManager igm = InGameManager.Instance;
        // ȭ�� ��Ӱ�
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut());

        // --- ��Ȱ ó�� ---
        Debug.Log("��Ȱ ó�� ����");


        PlayerController.Instance.PlayerDead();

        // 2. ü�� �� ��� ȸ��
        igm.currentHealth = igm.maxHealth;
        UpdateHealthUI();
        if (oxygenSystem != null)
        {
            oxygenSystem.currentOxygen = oxygenSystem.maxOxygen;
            oxygenSystem.UpdateOxygenGauge();
        }

        // 3. ��Ʈ�� ����
        if (playerController != null)
            playerController.SetControllable(true);

        igm.isDead = false;

        // 4. ȭ�� ���
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }
    public void UpdateHealthUI()
    {
        InGameManager igm = InGameManager.Instance;
        if (playerHpSlider == null || igm == null) return;

        // �����̴��� �ִ�/�ּҰ� ����
        playerHpSlider.minValue = 0;
        playerHpSlider.maxValue = igm.maxHealth;
        // ���� ü������ �� ����
        playerHpSlider.value = igm.currentHealth;
    }
}