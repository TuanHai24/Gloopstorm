using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public float moveSpeed = 2f; // Tốc độ di chuyển
    private Transform player;

    void Start()
    {
        // Tìm player trong scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player trong scene");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Hướng di chuyển về phía player
        Vector3 dir = (player.position - transform.position).normalized;

        // Di chuyển enemy
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
