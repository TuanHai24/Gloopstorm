using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Refs (optional)")]
    public Transform visual;          

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();            
        if (visual != null) anim = visual.GetComponent<Animator>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = Vector2.ClampMagnitude(input, 1f);     

        if (visual)
        {
            if (input.x != 0)
                visual.localScale = new Vector3(Mathf.Sign(input.x) * Mathf.Abs(visual.localScale.x), visual.localScale.y, visual.localScale.z);

            if (anim) anim.SetFloat("Speed", rb ? rb.linearVelocity.sqrMagnitude : input.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }
}
