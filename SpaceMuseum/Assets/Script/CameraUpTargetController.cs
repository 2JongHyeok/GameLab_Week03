using UnityEngine;

public class CameraUpTargetController : MonoBehaviour
{
    public Transform player;
    public Transform planet;

    void Update()
    {
        if (player == null || planet == null) return;

        // 1. �÷��̾� ��ġ�� ����ٴѴ�.
        transform.position = player.position;

        // 2. �� ������Ʈ�� '����(up)' ������ �׻� �༺ �߽ɿ��� �÷��̾ ���ϵ��� �����Ѵ�.
        Vector3 gravityUp = (player.position - planet.position).normalized;
        transform.up = gravityUp;
    }
}