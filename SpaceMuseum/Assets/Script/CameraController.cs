using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineInputAxisController inputController;
    public CinemachineCamera vcam;

    [Header("등속 줌 설정")]
    [SerializeField] float zoomStepPerNotch = 1.0f; // 휠 한 칸당 목표값 변화량(거리/스케일)
    [SerializeField] float zoomSpeed = 6.0f;        // 등속 이동 속도(초당 단위)
    [SerializeField] float minDistance = 1.5f;      // Sphere 모드용
    [SerializeField] float maxDistance = 12f;       // Sphere 모드용

    float _target; // Sphere: Radius, ThreeRing: RadialAxis.Value

    void Start()
    {
        if (vcam && vcam.TryGetComponent<CinemachineOrbitalFollow>(out var of))
        {
            _target = of.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere
                ? of.Radius
                : of.RadialAxis.Value;
        }
    }

    void LateUpdate()
    {
        // 우클릭 회전 ON/OFF (네 기존 로직 유지)
        if (Input.GetMouseButton(1))
        {
            if (inputController) inputController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (inputController) inputController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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
}
