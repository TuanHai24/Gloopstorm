using UnityEngine;

public class SpinningBlade : MonoBehaviour
{
    public GameObject bladePrefab;
    public int bladeCount = 4;
    public float radius = 1.5f;
    public float spinSpeed = 180f; // độ/giây
    public int damage = 2;

    Transform[] blades;

    void Start()
    {
        blades = new Transform[bladeCount];
        for (int i = 0; i < bladeCount; i++)
        {
            float angle = (360f / bladeCount) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            var go = Instantiate(bladePrefab, transform.position + offset, Quaternion.identity, transform);
            blades[i] = go.transform;

            // gắn collider + script gây damage liên tục vào enemy nếu cần
        }
    }

    void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}
