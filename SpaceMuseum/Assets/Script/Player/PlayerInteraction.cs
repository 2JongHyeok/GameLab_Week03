using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactionHoldTime = 2f;

    private IInteractable currentInteractable;
    private float currentHoldTime = 0f;

    void Update()
    {
        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                currentInteractable.OnInstantInteract();
            }
            else if (Input.GetKey(KeyCode.F))
            {
                currentHoldTime += Time.deltaTime;
                float progress = Mathf.Clamp01(currentHoldTime / interactionHoldTime);
                currentInteractable.OnHoldInteract(progress);
            }
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
            ResetInteraction(); // 새 대상이면 무조건 진행률 초기화
            currentInteractable = newInteractable;
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
        currentInteractable?.OnHoldInteract(0f); // UI도 0으로
    }
}
