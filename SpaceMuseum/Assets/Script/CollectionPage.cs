using System.Collections.Generic;
using UnityEngine;

public class CollectionPage : MonoBehaviour
{
    [Header("����")]
    public List<MineralData> mineralsForThisStage; // �� �������� ǥ�õ� �̳׶� ������ ����Ʈ
    public GameObject collectionSlotPrefab;      // �̳׶� ���� UI ������
    public Transform slotParent;                 // ������ ���Ե��� �ڽ����� �� �θ� ������Ʈ

    // ������ ���Ե��� ������ �����صξ� ���߿� ���� ������Ʈ�� �� �ְ� ��
    private List<CollectionSlot> spawnedSlots = new List<CollectionSlot>();

    // ������ �ʱ�ȭ (MyUIManager�� ȣ��)
    public void InitializePage()
    {
        Debug.Log("InitializePage �����, �̳׶� ����: " + mineralsForThisStage.Count);
        // �̹� ������ �����Ǿ��ٸ� �ߺ� ������ ���� ���� �Լ� ����
        if (spawnedSlots.Count > 0) return;

        // �� ���������� �ش��ϴ� �̳׶� ������ ����� ��ȸ
        foreach (MineralData mineralData in mineralsForThisStage)
        {
            Debug.Log("���� ����: " + mineralData.mineralName);
            // ���� �������� slotParent�� �ڽ����� ����
            GameObject slotObject = Instantiate(collectionSlotPrefab, slotParent);
            CollectionSlot slot = slotObject.GetComponent<CollectionSlot>();

            // ������ ���Կ� �̳׶� �����͸� �Ѱ� �ʱ�ȭ
            slot.InitializeSlot(mineralData);
            // ���� ��Ͽ� �߰�
            spawnedSlots.Add(slot);
        }
    }

    // �������� ��� ���� UI�� �ֽ� ������ ������Ʈ (MyUIManager�� ȣ��)
    public void UpdatePageDisplay()
    {
        // ������ ��� ������ ��ȸ
        foreach (CollectionSlot slot in spawnedSlots)
        {
            // ������ ����ϴ� �̳׶��� �̸��� ������
            string mineralName = slot.GetMineralName();

            // InGameManager�� ��ųʸ����� �ش� �̳׶��� ���൵�� ã�ƿ�
            if (InGameManager.Instance.collectionProgress.TryGetValue(mineralName, out int progress))
            {
                slot.UpdateSlot(progress);
            }
            else
            {
                slot.UpdateSlot(0);
            }
        }
    }
}