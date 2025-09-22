using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance { get; private set; }

    [Header("���� ����")]
    [Tooltip("�÷��̾���� �� �Ÿ� �ȿ� �ִ� �Ĺ��� Ȱ��ȭ�Ǿ� ���� ������ �����մϴ�.")]
    public float activationRadius = 70f;

    [Tooltip("Ȱ��ȭ�� �Ĺ��� ã�� �ֱ⸦ �����մϴ�. (����: ��)")]
    public float updateInterval = 0.5f;

    // ����Ʈ ����
    private readonly List<DangerousPlant> allPlants = new List<DangerousPlant>(256);
    private readonly List<DangerousPlant> activePlants = new List<DangerousPlant>(128);

    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        TryResolvePlayer();
        // timeScale�� ������ ���� �ʰ� �Ϸ��� �Ʒ� �ڷ�ƾ ������� ��ü�� �� ����.
        InvokeRepeating(nameof(UpdateActivePlants), 0f, updateInterval);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        CancelInvoke();
    }

    private void Update()
    {
        // Ȱ�� �Ĺ��� ƽ
        for (int i = 0; i < activePlants.Count; i++)
        {
            var plant = activePlants[i];
            if (plant != null)
                plant.OnUpdateTick();
        }
    }

    public void RegisterPlant(DangerousPlant plant)
    {
        if (plant == null) return;
        if (!allPlants.Contains(plant))
            allPlants.Add(plant);
        // �ʿ� �� ��� �����ϰ� �ʹٸ� �Ʒ� �ּ� ����
        // DirtyFlag�� �ξ��ٰ� ���� UpdateActivePlants���� �ݿ��ϴ� �ĵ� ����
        // UpdateActivePlants();
    }

    public void UnregisterPlant(DangerousPlant plant)
    {
        if (plant == null) return;
        allPlants.Remove(plant);
        activePlants.Remove(plant);
    }

    private void UpdateActivePlants()
    {
        // �÷��̾� ������ ���ǵ� ��� ��õ�
        if (playerTransform == null && !TryResolvePlayer())
        {
            activePlants.Clear();
            return;
        }

        float sqrRadius = activationRadius * activationRadius;

        // active ����(�ܼ� ���: ��ü �籸��)
        activePlants.Clear();

        // foreach ��� for(������)�� ���� ��ȸ
        int count = allPlants.Count;
        for (int i = 0; i < count; i++)
        {
            var plant = allPlants[i];
            if (plant == null)
                continue;

            float distanceSqr = (playerTransform.position - plant.transform.position).sqrMagnitude;
            if (distanceSqr <= sqrRadius)
            {
                activePlants.Add(plant);
            }
        }

        // ���� null Ŭ����(����): ����� �л��Ϸ��� �����Ӹ��� �Ϻθ� ����
        // �Ʒ��� ������ ���� ���� ����
        for (int i = allPlants.Count - 1; i >= 0; i--)
        {
            if (allPlants[i] == null) allPlants.RemoveAt(i);
        }
    }

    private bool TryResolvePlayer()
    {
        // 1) PlayerController �̱��� �켱
        var pc = PlayerController.Instance;
        if (pc != null)
        {
            playerTransform = pc.transform;
            return true;
        }

        // 2) �±� ����
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            return true;
        }

        // ã�� ���ϸ� ���� �ֱ⿡�� ��õ�
        return false;
    }

    // timeScale ������ �ɼ� (�ʿ�� ���)
    private IEnumerator UpdateLoopRealtime()
    {
        var wait = new WaitForSecondsRealtime(updateInterval);
        while (true)
        {
            UpdateActivePlants();
            yield return wait;
        }
    }
}
