using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineInputAxisController inputController;

    void Update()
    { 
        // 마우스 오른쪽 버튼을 누르고 있을 때
        if (Input.GetMouseButton(1))
        {
            // 입력 컨트롤러 컴포넌트 자체를 활성화합니다.
            inputController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else // 마우스 오른쪽 버튼을 떼고 있을 때
        {
            // 입력 컨트롤러 컴포넌트 자체를 비활성화해서 입력을 막습니다.
            inputController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}