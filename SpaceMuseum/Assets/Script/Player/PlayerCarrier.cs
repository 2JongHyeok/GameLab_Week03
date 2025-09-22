using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerCarrier : MonoBehaviour
{
    [Header("운반 설정")]
    public Transform carryPoint; // 광물이 쌓일 위치 (플레이어 자식 오브젝트)
    public float mineralSpacing = 1f; // 쌓이는 광물 사이의 간격

    [Header("투척 설정")]
    public float dropForwardDistance = 2f; // 플레이어 앞쪽으로 버릴 거리
    public float dropCheckRadius = 0.5f;  // 버릴 위치에 장애물이 있는지 확인할 반경
    public LayerMask obstacleMask;        // 장애물로 간주할 레이어 마스크
    public float throwForce = 2f;         // 던질 힘 (고정값)

    [Header("UI 설정")]
    public TextMeshProUGUI weightText; // 무게를 표시할 UI 텍스트
    public TextMeshProUGUI warningText; // 경고 메시지를 표시할 UI 텍스트
    public float warningMessageDuration = 2f; // 경고 메시지가 보이는 시간

    private List<Mineral> carriedMinerals = new List<Mineral>();
    private Coroutine warningCoroutine;
    private InGameManager igm;
    private MyUIManager uim;

    void Start()
    {
        igm = InGameManager.Instance;
        uim = MyUIManager.Instance;
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false); // 시작할 때 경고 텍스트 숨기기
        }
    }

    void Update()
    {
        // G키를 누르면 광물 내려놓기
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropLastMineral();
        }
    }

    public void AddMineral(Mineral mineral)
    {
        carriedMinerals.Add(mineral);

        Rigidbody rb = mineral.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        //mineral.GetComponent<Collider>().isTrigger = true;

        mineral.transform.SetParent(carryPoint);
        Vector3 newPosition = carryPoint.up * (carriedMinerals.Count - 1) * mineralSpacing;
        mineral.transform.localPosition = newPosition;
        mineral.transform.localRotation = Quaternion.identity;
        igm.currentWeight += mineral.data.weight;
        UpdateWeightUI();

    }

    // 가장 마지막에 든 광물을 내려놓는 함수
    public void DropLastMineral()
    {
        if (carriedMinerals.Count == 0) return;

        // 버릴 위치 계산 (플레이어 앞 고정 거리)
        Vector3 dropPosition = transform.position + transform.forward * dropForwardDistance;

        // 버릴 위치에 장애물이 있는지 확인
        if (Physics.CheckSphere(dropPosition, dropCheckRadius, obstacleMask))
        {
            ShowWarning("Can't Drop Mineral");
            return;
        }

        // 리스트의 마지막 아이템 (가장 나중에 든 것)
        int lastIndex = carriedMinerals.Count - 1;
        Mineral mineralToDrop = carriedMinerals[lastIndex];
        carriedMinerals.RemoveAt(lastIndex);

        // 무게 업데이트
        igm.currentWeight -= mineralToDrop.data.weight;
        UpdateWeightUI();

        // 부모-자식 관계 해제 및 물리 속성 복원
        mineralToDrop.transform.SetParent(null);
        mineralToDrop.transform.position = dropPosition;

        Rigidbody rb = mineralToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            mineralToDrop.GetComponent<Collider>().isTrigger = false;

            // 항상 같은 힘으로 앞으로 툭 밀기
            Vector3 throwDir = (transform.forward * 0.5f + Vector3.up * 0.1f).normalized;
            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
        }
    }

    private void UpdateWeightUI()
    {
        igm.UpdateWeight();
    }

    // 경고 메시지를 화면에 표시하는 함수
    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            if (warningCoroutine != null)
                StopCoroutine(warningCoroutine);
            warningCoroutine = StartCoroutine(ShowWarningCoroutine(message));
        }
        else
        {
        }
    }

    // 경고 메시지를 일정 시간 동안 보여주고 숨기는 코루틴
    private IEnumerator ShowWarningCoroutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningMessageDuration);
        warningText.gameObject.SetActive(false);
    }
}
