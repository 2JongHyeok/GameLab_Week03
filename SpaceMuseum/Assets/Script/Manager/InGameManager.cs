using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static InGameManager Instance { get; private set; }

    [Header("���� ������")]
    public int tetherCount = 10; // �״� ������ ���⼭ �����մϴ�.

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
    }
}