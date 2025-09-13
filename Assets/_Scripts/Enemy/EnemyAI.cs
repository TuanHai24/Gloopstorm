using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Refs (kéo sẵn)")]
    public Rigidbody2D rb;
    public Transform player;
    public Animator anim;
    public SpriteRenderer sr;

    [Header("VFX & Timing")]
    public AnimationClip dieClip;   // vẫn dùng để set trigger, KHÔNG còn quyết định Destroy

    [Header("Drop")]
    public EnemyLoot loot;

    bool deadHandled;
    EnemyHealth h;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void OnEnable()
    {
        deadHandled = false;
        h = GetComponent<EnemyHealth>();
        if (h != null) h.onDeath.AddListener(HandleDeath);
    }

    void OnDisable()
    {
        if (h != null) h.onDeath.RemoveListener(HandleDeath);
    }

    void HandleDeath()
    {
        if (deadHandled) return;
        deadHandled = true;

        if (loot) loot.Drop();

        if (rb) rb.simulated = false;
        var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
        var follow = GetComponent<EnemyFollow>(); if (follow) follow.enabled = false;

        if (anim) anim.SetTrigger("Die");

        // KHÔNG Destroy ở đây nữa.
        // EnemyHealth.deferDestroy + deferDestroySeconds sẽ quyết định khi nào biến mất.
        // (Nếu thật sự muốn override tại AI, có thể gọi Destroy ở đây theo dieClip.length)
    }
}
