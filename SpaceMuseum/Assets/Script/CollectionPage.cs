using System.Collections.Generic;
using UnityEngine;

public class CollectionPage : MonoBehaviour
{
    [Header("설정")]
    public List<MineralData> mineralsForThisStage; // 이 페이지에 표시될 미네랄 데이터 리스트
    public GameObject collectionSlotPrefab;      // 미네랄 슬롯 UI 프리팹
    public Transform slotParent;                 // 생성된 슬롯들이 자식으로 들어갈 부모 오브젝트

    // 생성된 슬롯들의 참조를 저장해두어 나중에 쉽게 업데이트할 수 있게 함
    private List<CollectionSlot> spawnedSlots = new List<CollectionSlot>();

    // 페이지 초기화 (MyUIManager가 호출)
    public void InitializePage()
    {
        // 이미 슬롯이 생성되었다면 중복 생성을 막기 위해 함수 종료
        if (spawnedSlots.Count > 0) return;

        // 이 스테이지에 해당하는 미네랄 데이터 목록을 순회
        foreach (MineralData mineralData in mineralsForThisStage)
        {
            // 슬롯 프리팹을 slotParent의 자식으로 생성
            GameObject slotObject = Instantiate(collectionSlotPrefab, slotParent);
            CollectionSlot slot = slotObject.GetComponent<CollectionSlot>();

            // 생성된 슬롯에 미네랄 데이터를 넘겨 초기화
            slot.InitializeSlot(mineralData);
            // 관리 목록에 추가
            spawnedSlots.Add(slot);
        }
    }

    // 페이지의 모든 슬롯 UI를 최신 정보로 업데이트 (MyUIManager가 호출)
    public void UpdatePageDisplay()
    {
        // 생성된 모든 슬롯을 순회
        foreach (CollectionSlot slot in spawnedSlots)
        {
            // 슬롯이 담당하는 미네랄의 이름을 가져옴
            string mineralName = slot.GetMineralName();

            // InGameManager의 딕셔너리에서 해당 미네랄의 진행도를 찾아옴
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