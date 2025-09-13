using UnityEngine;

public class PickupMagnet : MonoBehaviour
{
    [Header("Bán kính hút (m)")]
    public float baseRadius = 1.5f;          // bán kính cơ bản
    [HideInInspector] public float bonusRadius = 0f; // cộng thêm từ nâng cấp

    [Header("Tốc độ/ gia tốc hút")]
    public float pullStartSpeed = 6f;        // tốc độ bắt đầu
    public float pullAcceleration = 30f;     // tăng tốc mỗi giây

    [Header("Lọc va chạm exp")]
    public LayerMask orbMask;                // layer của EXP orb (tạo layer riêng cho orb)

    public int maxPerFrame = 32;             // an toàn/giới hạn mỗi frame

    readonly Collider2D[] _hits = new Collider2D[64];

    public float Radius => baseRadius + bonusRadius;
    public void AddRadius(float add) => bonusRadius += add;

    void Update()
    {
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, Radius, _hits, orbMask);
        for (int i = 0; i < n && i < maxPerFrame; i++)
        {
            var orb = _hits[i].GetComponent<EXPOrb>();
            if (orb != null)
                orb.AttractTo(transform, pullStartSpeed, pullAcceleration);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
#endif
}
