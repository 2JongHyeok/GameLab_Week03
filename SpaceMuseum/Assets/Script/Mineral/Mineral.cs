using UnityEngine;

public class Mineral : MonoBehaviour, IInteractable
{
    [Header("������")]
    public MineralData data;

    [Header("UI")]
    public GameObject interactionPromptPrefab; // �ؽ�Ʈ ���� ������ ����

    private GameObject promptInstance;
    private bool isCarried = false;
    private PlayerCarrier playerCarrier; // ĳ�̵� �÷��̾� ĳ���� ����

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
            promptInstance = null; // ��� null ó��
        }
    }
    #endregion

    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!isCarried && other.CompareTag("Player"))
        {
            // �÷��̾� ��ȣ�ۿ�/ĳ���� ��ũ��Ʈ ĳ��
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
    // ������ Hold �ʿ� ����
    public void OnHoldInteract(float progress) { }

    // F �� �� ������ �Ⱦ�
    public void OnInstantInteract()
    {
        if (isCarried) return;
        playerCarrier?.AddMineral(this);
    }
    #endregion

    #region ���� ����
    // PlayerCarrier�� ȣ��
    public void SetCarriedState(bool carried)
    {
        isCarried = carried;

        // ��� ���̸� ���� ����
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = carried;
        }

        // �浹 ó��: ��� �߿��� "Player"���� ��ȣ�ۿ븸 ���� ���� ���̾� ��ȯ ��õ
        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = carried;
        }

        // UI ����
        if (carried)
        {
            ShowPrompt(false);
        }
    }
    #endregion
}
