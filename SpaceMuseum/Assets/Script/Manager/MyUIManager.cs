using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // TextMeshPro를 사용하기 위해 반드시 추가!

public class MyUIManager : MonoBehaviour
{
    // UIManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static MyUIManager Instance { get; private set; }

    [Header("GameStatus UI")]
    public TextMeshProUGUI tetherCountText; // 에디터에서 연결할 텍스트 UI
    public TextMeshProUGUI byteText;

    [Header("Shop UI")]
    public GameObject shopPanel; // 상점 패널 오브젝트

    [Header("Shop Buttons")]
    public Button buyTetherButton;

    [Header("Donates UI")]
    public Button collectionBtn;
    public GameObject collectionPanel; // 도감 전체를 담는 패널
    public GameObject[] stagePages; // Stage1, Stage2, Stage3 페이지 UI들을 담을 배열

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
        // 게임 시작 시 상점은 닫혀있도록 함
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
        // 'P' 키를 누르면 상점 UI를 켜고 끔
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
            // 상점이 열려있으면 상점을 닫고
            if (shopPanel != null && shopPanel.activeSelf)
            {
                CloseShopPanel();
            }
            // (상점은 닫혀있고) 도감이 열려있으면 도감을 닫는다
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
            // 도감을 열면 기본으로 첫 번째 스테이지를 보여줌
            OnStageTabClicked(0);
            UpdateCollectionDisplay();
        }
    }

    public void UpdateCollectionDisplay()
    {
        // 모든 페이지 오브젝트를 순회
        foreach (var pageObject in stagePages)
        {
            // 현재 켜져있는 페이지만 찾아서
            if (pageObject.activeSelf)
            {
                // 해당 페이지의 CollectionPage 스크립트에게 UI 업데이트를 명령
                pageObject.GetComponent<CollectionPage>()?.UpdatePageDisplay();
                // 하나 찾았으면 루프 종료
                break;
            }
        }
    }

    public void OnStageTabClicked(int stageIndex)
    {
        if (stagePages == null || stagePages.Length <= stageIndex) return;
        collectionBtn.gameObject.SetActive(false);

        // 모든 페이지를 일단 다 끈다
        for (int i = 0; i < stagePages.Length; i++)
        {
            stagePages[i].SetActive(false);
        }

        // 요청된 인덱스의 페이지만 켠다
        stagePages[stageIndex].SetActive(true);

        CollectionPage collectionPage = stagePages[stageIndex].GetComponent<CollectionPage>();
        if (collectionPage != null)
        {
            // 1. 페이지를 초기화하여 슬롯들을 생성한다 (아직 생성 안 됐을 경우에만 실행됨)
            collectionPage.InitializePage();

            // 2. 생성된 슬롯들의 내용을 최신 데이터로 업데이트한다
            collectionPage.UpdatePageDisplay();
        }
    }

    // 도감 닫기 (X 버튼 또는 ESC키로 호출)
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

        // 상점을 열 때마다 버튼 상태를 업데이트
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
            gm.inventoryCapacityUpgradePrice += 500; // 가격 500원 증가
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
        // 화면 어둡게
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut());

        // --- 부활 처리 ---
        Debug.Log("부활 처리 시작");


        PlayerController.Instance.PlayerDead();

        // 2. 체력 및 산소 회복
        igm.currentHealth = igm.maxHealth;
        UpdateHealthUI();
        if (oxygenSystem != null)
        {
            oxygenSystem.currentOxygen = oxygenSystem.maxOxygen;
            oxygenSystem.UpdateOxygenGauge();
        }

        // 3. 컨트롤 복원
        if (playerController != null)
            playerController.SetControllable(true);

        igm.isDead = false;

        // 4. 화면 밝게
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }
    public void UpdateHealthUI()
    {
        InGameManager igm = InGameManager.Instance;
        if (playerHpSlider == null || igm == null) return;

        // 슬라이더의 최대/최소값 세팅
        playerHpSlider.minValue = 0;
        playerHpSlider.maxValue = igm.maxHealth;
        // 현재 체력으로 값 갱신
        playerHpSlider.value = igm.currentHealth;
    }
}