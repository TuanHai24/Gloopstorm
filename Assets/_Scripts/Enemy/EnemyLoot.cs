using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("EXP drop")]
    public EXPOrb orbPrefab;
    public int minOrbs = 1;
    public int maxOrbs = 2;
    public int expPerOrb = 1;
    public float scatterRadius = 0.15f;
    public float scatterForce = 0.3f;

    public void Drop()
    {
        if (orbPrefab == null) return;

        int count = Mathf.Clamp(Random.Range(minOrbs, maxOrbs + 1), 0, 99);
        for (int i = 0; i < count; i++)
        {
            Vector2 off = Random.insideUnitCircle * scatterRadius;
            var orb = Instantiate(orbPrefab, transform.position + (Vector3)off, Quaternion.identity);

            // set exp
            orb.exp = expPerOrb;

            // nảy nhẹ ra xung quanh
            var rb = orb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = off.sqrMagnitude > 0.0001f ? off.normalized : Random.insideUnitCircle.normalized;
                rb.AddForce(dir * scatterForce, ForceMode2D.Impulse);
            }
        }
    }
}
