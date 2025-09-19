using UnityEngine;

// 상호작용 가능한 모든 오브젝트가 따라야 할 규칙(함수)을 정의합니다.
public interface IInteractable
{
    void OnInteract(float progress);
}