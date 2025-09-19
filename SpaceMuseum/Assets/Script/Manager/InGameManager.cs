using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static InGameManager Instance { get; private set; }

    [Header("플레이어 재화")]
    public int bytes = 10000; // 임시로 1000원을 가지고 시작

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
    public int nowWeight = 0;
    public int maxWeight = 0;

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
        MyUIManager.Instance.UpdateBytes(bytes);
    }
}