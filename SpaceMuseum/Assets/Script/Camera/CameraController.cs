using System.Collections;
using System.Runtime.InteropServices;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region 카메라 회전시 마우스 위치 관련 변수

    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);

    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int x; public int y; }

    POINT savedScreenPos;
    #endregion
    [Header("Refs")]
    public CinemachineInputAxisController inputController;
    public CinemachineCamera vcam;

    #region 카메라 휠 확대 축소 관련 변수
    [Header("등속 줌 설정")]
    [SerializeField] float zoomStepPerNotch = 1.0f; // 휠 한 칸당 목표값 변화량(거리/스케일)
    [SerializeField] float zoomSpeed = 6.0f;        // 등속 이동 속도(초당 단위)
    [SerializeField] float minDistance = 1.5f;      // Sphere 모드용
    [SerializeField] float maxDistance = 12f;       // Sphere 모드용
    float _target;
    #endregion

    void Start()
    {
        if (vcam && vcam.TryGetComponent<CinemachineOrbitalFollow>(out var of))
        {
            _target = of.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere
                ? of.Radius
                : of.RadialAxis.Value;
        }
        if (inputController) inputController.enabled = false;
    }

    void LateUpdate()
    {
        RotateCamera();

        if (!vcam || !vcam.TryGetComponent<CinemachineOrbitalFollow>(out var of)) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            if (of.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere)
            {
                // 목표 반지름을 증분 + 경계 고정
                _target = Mathf.Clamp(_target - scroll * zoomStepPerNotch, minDistance, maxDistance);
            }
            else // ThreeRing
            {
                var range = of.RadialAxis.Range; // (min, max) 에디터에서 설정됨
                _target = Mathf.Clamp(_target - scroll * zoomStepPerNotch, range.x, range.y);
            }
        }

        // 등속 이동: 현재값 → 목표값
        if (of.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere)
        {
            float next = Mathf.MoveTowards(of.Radius, _target, zoomSpeed * Time.deltaTime);
            of.Radius = Mathf.Clamp(next, minDistance, maxDistance);
        }
        else // ThreeRing
        {
            var axis = of.RadialAxis;               // 구조체 복사
            float next = Mathf.MoveTowards(axis.Value, _target, zoomSpeed * Time.deltaTime);
            axis.Value = Mathf.Clamp(next, axis.Range.x, axis.Range.y);
            of.RadialAxis = axis;                   // 다시 적﻿용(중요)
        }
    }
    void RotateCamera()    // 카메라 회전 시키는 부분
    {
        // 우클릭 회전 ON/OFF
        if (Input.GetMouseButtonDown(1))
        {
            if (inputController) inputController.enabled = true;
            // 1) 현재 커서의 '데스크톱(스크린) 좌표'를 저장
            GetCursorPos(out savedScreenPos);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (inputController) inputController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            StartCoroutine(RestoreNextFrame());
        }
    }
    IEnumerator RestoreNextFrame()
    {
        yield return null; // 1프레임 대기(필요하면 2프레임도 OK)
        SetCursorPos(savedScreenPos.x, savedScreenPos.y);
        Cursor.visible = true;
    }
}
