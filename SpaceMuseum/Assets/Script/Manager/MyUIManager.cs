using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �ݵ�� �߰�!

public class MyUIManager : MonoBehaviour
{
    // UIManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static MyUIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI tetherCountText; // �����Ϳ��� ������ �ؽ�Ʈ UI

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
    public void UpdateTetherCount(int count)
    {
        if (tetherCountText != null)
        {
            tetherCountText.text = $"Tether : {count:D2}";
        }
    }
}