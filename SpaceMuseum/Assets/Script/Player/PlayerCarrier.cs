using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerCarrier : MonoBehaviour
{
    [Header("��� ����")]
    public Transform carryPoint; // ������ ���� ��ġ (�÷��̾� �ڽ� ������Ʈ)
    public float mineralSpacing = 1f; // ���̴� ���� ������ ����

    [Header("��ô ����")]
    public float dropForwardDistance = 2f; // �÷��̾� �������� ���� �Ÿ�
    public float dropCheckRadius = 0.5f;  // ���� ��ġ�� ��ֹ��� �ִ��� Ȯ���� �ݰ�
    public LayerMask obstacleMask;        // ��ֹ��� ������ ���̾� ����ũ
    public float throwForce = 2f;         // ���� �� (������)

    [Header("UI ����")]
    public TextMeshProUGUI weightText; // ���Ը� ǥ���� UI �ؽ�Ʈ
    public TextMeshProUGUI warningText; // ��� �޽����� ǥ���� UI �ؽ�Ʈ
    public float warningMessageDuration = 2f; // ��� �޽����� ���̴� �ð�

    private List<Mineral> carriedMinerals = new List<Mineral>();
    private Coroutine warningCoroutine;
    private InGameManager igm;
    private MyUIManager uim;

    private bool hasDroppedOnDeath = false; // ���� �� �̹� ����ߴ��� üũ

    void Start()
    {
        igm = InGameManager.Instance;
        uim = MyUIManager.Instance;
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false); // ������ �� ��� �ؽ�Ʈ �����
        }
    }

    void Update()
    {
        // GŰ�� ������ ���� ��������
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropLastMineral();
        }

        if (igm.isDead && !hasDroppedOnDeath)
        {
            DropAllMineralsOnDeath();
            hasDroppedOnDeath = true;
        }
    }
    private void DropAllMineralsOnDeath()
    {
        Debug.Log("�Ҹ���?");
        if (carriedMinerals.Count == 0) return;

        foreach (var mineral in carriedMinerals)
        {
            // �θ�-�ڽ� ���� ����
            mineral.transform.SetParent(null);
            mineral.transform.position = transform.position + Vector3.up * 0.5f; // �÷��̾� ��ġ ��¦ ��

            Rigidbody rb = mineral.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                mineral.GetComponent<Collider>().isTrigger = false;

                // ���� ���� ������� �� ����߸��� (���� ����)
                Vector3 randomDir = (Random.insideUnitSphere + Vector3.up * 0.5f).normalized;
                rb.AddForce(randomDir * throwForce, ForceMode.Impulse);
            }
        }

        // ���� �ʱ�ȭ
        igm.currentWeight = 0;
        UpdateWeightUI();
        // ����Ʈ ����
        carriedMinerals.Clear();
    }
    public void ResetDeathFlag()
    {
        hasDroppedOnDeath = false;
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

    // ���� �������� �� ������ �������� �Լ�
    public void DropLastMineral()
    {
        if (carriedMinerals.Count == 0) return;

        // ���� ��ġ ��� (�÷��̾� �� ���� �Ÿ�)
        Vector3 dropPosition = transform.position + transform.forward * dropForwardDistance;

        // ���� ��ġ�� ��ֹ��� �ִ��� Ȯ��
        if (Physics.CheckSphere(dropPosition, dropCheckRadius, obstacleMask))
        {
            ShowWarning("Can't Drop Mineral");
            return;
        }

        // ����Ʈ�� ������ ������ (���� ���߿� �� ��)
        int lastIndex = carriedMinerals.Count - 1;
        Mineral mineralToDrop = carriedMinerals[lastIndex];
        carriedMinerals.RemoveAt(lastIndex);

        // ���� ������Ʈ
        igm.currentWeight -= mineralToDrop.data.weight;
        UpdateWeightUI();

        // �θ�-�ڽ� ���� ���� �� ���� �Ӽ� ����
        mineralToDrop.transform.SetParent(null);
        mineralToDrop.transform.position = dropPosition;

        Rigidbody rb = mineralToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            mineralToDrop.GetComponent<Collider>().isTrigger = false;

            // �׻� ���� ������ ������ �� �б�
            Vector3 throwDir = (transform.forward * 0.5f + Vector3.up * 0.1f).normalized;
            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
        }
    }

    private void UpdateWeightUI()
    {
        igm.UpdateWeight();
    }

    // ��� �޽����� ȭ�鿡 ǥ���ϴ� �Լ�
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

    // ��� �޽����� ���� �ð� ���� �����ְ� ����� �ڷ�ƾ
    private IEnumerator ShowWarningCoroutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningMessageDuration);
        warningText.gameObject.SetActive(false);
    }
}
