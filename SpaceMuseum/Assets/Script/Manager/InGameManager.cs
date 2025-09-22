using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static InGameManager Instance { get; private set; }



    [Header("플레이어 데이터")]
    public int bytes = 10000; // 임시로 1000원을 가지고 시작
    public float moveSpeed = 6f;
    public bool isDead = false;
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("테더 데이터")]
    public int tetherCount = 10;
    public const int maxTetherCount = 99; // 테더 최대 소지 개수 (상수)
    public int tetherBundlePrice = 100; // 테더 10개 묶음 가격

    [Header("업그레이드 데이터")]
    public int moveSpeedLevel = 1; // 현재 이동 속도 레벨
    public const int maxMoveSpeedLevel = 5; // 최대 레벨 상수
    public int moveSpeedUpgradePrice = 250; // 이속 업그레이드 가격

    [Header("가방 데이터")]
    public int inventoryCapacityLevel = 1;
    public int inventoryCapacityUpgradePrice = 500; // 초기 가격
    public int currentWeight = 0;
    public int maxWeight = 10;

    [Header("도감 데이터")]
    // Dictionary를 사용해 자원 이름과 수집률(%)을 저장합니다.
    public Dictionary<string, int> collectionProgress = new Dictionary<string, int>();

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
        MyUIManager.Instance.UpdateTetherCount(tetherCount);
        MyUIManager.Instance.UpdateBytes();
        currentHealth = maxHealth;
    }

    public void UpdateWeight()
    {
        MyUIManager.Instance.UpdateWeightUI(currentWeight, maxWeight);

        // 플레이어 이동속도 패널티 적용
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerController>()?.ApplyWeightPenalty(currentWeight, maxWeight);
        }
    }

    public void AddBytes(int amount)
    {
        bytes += amount;
        MyUIManager.Instance.UpdateBytes();
    }



    public bool DonateMineral(MineralData mineralData)
    {
        string mineralName = mineralData.mineralName;
        int progressToAdd = 20; // 1개당 20%씩 진행도 증가

        // 이미 등록된 경우
        if (collectionProgress.ContainsKey(mineralName))
        {
            if (collectionProgress[mineralName] >= 100)
            {
                Debug.Log($"{mineralName} 은(는) 이미 100% 완료되어 기부할 수 없습니다.");
                return false;
            }

            collectionProgress[mineralName] += progressToAdd;
            if (collectionProgress[mineralName] > 100)
                collectionProgress[mineralName] = 100;
        }
        else
        {
            collectionProgress.Add(mineralName, progressToAdd);
        }

        Debug.Log($"{mineralName} 기부 완료! 현재 진행도: {collectionProgress[mineralName]}%");
        MyUIManager.Instance.UpdateCollectionDisplay();

        return true; // 기부 성공
    }
}