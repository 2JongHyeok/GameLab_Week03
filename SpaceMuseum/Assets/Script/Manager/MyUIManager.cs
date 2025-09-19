using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 반드시 추가!

public class MyUIManager : MonoBehaviour
{
    // UIManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static MyUIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI tetherCountText; // 에디터에서 연결할 텍스트 UI

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