using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // InGameManager를 어디서든 쉽게 접근할 수 있도록 Singleton 패턴으로 만듭니다.
    public static InGameManager Instance { get; private set; }

    [Header("게임 데이터")]
    public int tetherCount = 10; // 테더 개수를 여기서 관리합니다.

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