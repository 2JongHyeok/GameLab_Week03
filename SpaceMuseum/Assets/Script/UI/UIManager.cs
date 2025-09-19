using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �ݵ�� �߰�!

public class UIManager : MonoBehaviour
{
    // UIManager�� ��𼭵� ���� ������ �� �ֵ��� Singleton �������� ����ϴ�.
    public static UIManager Instance { get; private set; }

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
            // F2 ����: 10 �̸��� ���ڸ� 09, 08ó�� �� �ڸ��� ǥ�����ݴϴ�.
            tetherCountText.text = $"Tether : {count:D2}";
        }
    }
}