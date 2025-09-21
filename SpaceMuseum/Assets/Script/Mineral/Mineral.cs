using UnityEngine;

public class Mineral : MonoBehaviour, IInteractable
{
    [Header("데이터")]
    public MineralData data;

    [Header("UI")]
    public GameObject interactionPromptPrefab; // 텍스트 전용 프리팹 권장

    private GameObject promptInstance;
    private bool isCarried = false;
    private PlayerCarrier playerCarrier; // 캐싱된 플레이어 캐리어 참조

    #region UI
    public void ShowPrompt(bool show)
    {
        if (interactionPromptPrefab == null) return;

        if (show && promptInstance == null)
        {
            promptInstance = Instantiate(interactionPromptPrefab, transform);
        }
        else if (!show && promptInstance != null)
        {
            Destroy(promptInstance);
            promptInstance = null; // 즉시 null 처리
        }
    }
    #endregion

    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!isCarried && other.CompareTag("Player"))
        {
            // 플레이어 상호작용/캐리어 스크립트 캐싱
            var interaction = other.GetComponent<PlayerInteraction>();
            playerCarrier = other.GetComponent<PlayerCarrier>();

            interaction?.SetInteractable(this);
            ShowPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isCarried && other.CompareTag("Player"))
        {
            var interaction = other.GetComponent<PlayerInteraction>();
            interaction?.ClearInteractable(this);

            playerCarrier = null;
            ShowPrompt(false);
        }
    }
    #endregion

    #region IInteractable
    // 광물은 Hold 필요 없음
    public void OnHoldInteract(float progress) { }

    // F 한 번 누르면 픽업
    public void OnInstantInteract()
    {
        if (isCarried) return;
        playerCarrier?.AddMineral(this);
    }
    #endregion

    #region 상태 변경
    // PlayerCarrier가 호출
    public void SetCarriedState(bool carried)
    {
        isCarried = carried;

        // 운반 중이면 물리 정지
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = carried;
        }

        // 충돌 처리: 운반 중에는 "Player"와의 상호작용만 막기 위해 레이어 전환 추천
        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = carried;
        }

        // UI 갱신
        if (carried)
        {
            ShowPrompt(false);
        }
    }
    #endregion
}
