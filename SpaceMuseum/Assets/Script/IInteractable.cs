public interface IInteractable
{
    // F키를 꾹 누를 때 호출될 함수 (진행률 progress 전달)
    void OnHoldInteract(float progress);

    // F키를 한 번만 눌렀을 때 호출될 함수
    void OnInstantInteract();
}
