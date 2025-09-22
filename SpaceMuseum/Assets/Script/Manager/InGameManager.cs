using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static InGameManager Instance { get; private set; }



    [Header("�÷��̾� ������")]
    public int bytes = 10000; // �ӽ÷� 1000���� ������ ����
    public float moveSpeed = 6f;
    public bool isDead = false;
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("�״� ������")]
    public int tetherCount = 10;
    public const int maxTetherCount = 99; // �״� �ִ� ���� ���� (���)
    public int tetherBundlePrice = 100; // �״� 10�� ���� ����

    [Header("���׷��̵� ������")]
    public int moveSpeedLevel = 1; // ���� �̵� �ӵ� ����
    public const int maxMoveSpeedLevel = 5; // �ִ� ���� ���
    public int moveSpeedUpgradePrice = 250; // �̼� ���׷��̵� ����

    [Header("���� ������")]
    public int inventoryCapacityLevel = 1;
    public int inventoryCapacityUpgradePrice = 500; // �ʱ� ����
    public int currentWeight = 0;
    public int maxWeight = 10;

    [Header("���� ������")]
    // Dictionary�� ����� �ڿ� �̸��� ������(%)�� �����մϴ�.
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

        // �÷��̾� �̵��ӵ� �г�Ƽ ����
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
        int progressToAdd = 20; // 1���� 20%�� ���൵ ����

        // �̹� ��ϵ� ���
        if (collectionProgress.ContainsKey(mineralName))
        {
            if (collectionProgress[mineralName] >= 100)
            {
                Debug.Log($"{mineralName} ��(��) �̹� 100% �Ϸ�Ǿ� ����� �� �����ϴ�.");
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

        Debug.Log($"{mineralName} ��� �Ϸ�! ���� ���൵: {collectionProgress[mineralName]}%");
        MyUIManager.Instance.UpdateCollectionDisplay();

        return true; // ��� ����
    }
}