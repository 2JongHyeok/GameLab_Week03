public interface IInteractable
{
    // FŰ�� �� ���� �� ȣ��� �Լ� (����� progress ����)
    void OnHoldInteract(float progress);

    // FŰ�� �� ���� ������ �� ȣ��� �Լ�
    void OnInstantInteract();
}
