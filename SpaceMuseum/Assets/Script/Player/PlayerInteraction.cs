using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("��ȣ�ۿ� ����")]
    public float interactionHoldTime = 2f;

    private IInteractable currentInteractable;
    private float currentHoldTime = 0f;

    void Update()
    {
        if (currentInteractable != null)
        {
            // F�� ������ ������ ������ ä��
            if (Input.GetKey(KeyCode.F))
            {
                currentHoldTime += Time.deltaTime;
                float progress = Mathf.Clamp01(currentHoldTime / interactionHoldTime);
                currentInteractable.OnInteract(progress);
            }
            // F�� ���� ������ ����
            else if (Input.GetKeyUp(KeyCode.F))
            {
                ResetInteraction();
            }
        }
    }

    public void SetInteractable(IInteractable newInteractable)
    {
        if (currentInteractable != newInteractable)
        {
            currentInteractable = newInteractable;
            ResetInteraction(); // �� ����̸� ������ ����� �ʱ�ȭ
        }
    }

    public void ClearInteractable(IInteractable interactableToClear)
    {
        if (currentInteractable == interactableToClear)
        {
            ResetInteraction();
            currentInteractable = null;
        }
    }

    private void ResetInteraction()
    {
        currentHoldTime = 0f;
        currentInteractable?.OnInteract(0f); // UI�� 0����
    }
}
