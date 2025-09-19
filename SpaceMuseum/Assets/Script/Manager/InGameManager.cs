using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static InGameManager Instance { get; private set; }

    [Header("�÷��̾� ��ȭ")]
    public int bytes = 10000; // �ӽ÷� 1000���� ������ ����

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
    public int nowWeight = 0;
    public int maxWeight = 0;

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
        MyUIManager.Instance.UpdateBytes(bytes);
    }
}