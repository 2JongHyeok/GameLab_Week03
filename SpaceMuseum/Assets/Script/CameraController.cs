using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineInputAxisController inputController;

    void Update()
    { 
        // ���콺 ������ ��ư�� ������ ���� ��
        if (Input.GetMouseButton(1))
        {
            // �Է� ��Ʈ�ѷ� ������Ʈ ��ü�� Ȱ��ȭ�մϴ�.
            inputController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else // ���콺 ������ ��ư�� ���� ���� ��
        {
            // �Է� ��Ʈ�ѷ� ������Ʈ ��ü�� ��Ȱ��ȭ�ؼ� �Է��� �����ϴ�.
            inputController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}