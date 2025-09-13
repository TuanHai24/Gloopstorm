using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EXPOrb : MonoBehaviour
{
    public int exp = 1;

    Transform target;
    float speed, accel;
    bool attracted;

    Rigidbody2D rb;
    Collider2D col;

    // EXPOrb.cs
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = 0f;
            rb.linearDamping = 4.5f;         // NEW: hãm mạnh
            rb.angularDamping = 0.5f;
            rb.linearVelocity = Vector2.zero; // NEW: reset tốc
            rb.angularVelocity = 0f;
        }
    }


    // gọi bởi PickupMagnet
    public void AttractTo(Transform t, float startSpeed, float acceleration)
    {
        if (attracted) return;
        attracted = true;
        target = t;
        speed = Mathf.Max(0.1f, startSpeed);
        accel = Mathf.Max(0f, acceleration);
        if (rb) rb.simulated = false; // tự điều khiển bằng transform
    }

    void Update()
    {
        if (!attracted && rb != null)
        {
            // NEW: kéo velocity về 0 để khỏi trôi xa
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
        }

        if (!attracted || target == null) return;

        speed += accel * Time.deltaTime;
        Vector3 delta = target.position - transform.position;
        if (delta.magnitude < 0.1f) { GiveExpAndDie(); return; }
        transform.position += delta.normalized * speed * Time.deltaTime;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GiveExpAndDie();
    }

    void GiveExpAndDie()
    {
        var expComp = FindObjectOfType<PlayerEXP>();
        if (expComp != null) expComp.AddExp(exp);
        Destroy(gameObject);
    }
}
