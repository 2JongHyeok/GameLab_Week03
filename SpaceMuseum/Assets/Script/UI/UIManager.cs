using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 반드시 추가!

public class UIManager : MonoBehaviour
{
    // UIManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static UIManager Instance { get; private set; }

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
            // F2 포맷: 10 미만의 숫자를 09, 08처럼 두 자리로 표시해줍니다.
            tetherCountText.text = $"Tether : {count:D2}";
        }
    }
}