using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PropScatter2D : MonoBehaviour
{
    [Header("Follow mục tiêu (Camera là dễ nhất)")]
    public Transform follow;

    [Header("Khu vực spawn (đơn vị: world units)")]
    public float radius = 35f;          // bán kính ngoài → rải 360°
    public float innerRadius = 0f;      // lỗ ở giữa (0 = rải tới giữa)
    public float cellSize = 2f;         // lưới ô; ô càng to càng thưa
    [Range(0f, 1f)] public float density = 0.25f;
    public int maxInstances = 350;

    [Header("Masks (PHYSICS)")]
    public LayerMask avoidMask;         // ví dụ Player
    public LayerMask overlapMask;       // Decor (để chặn chồng)
    public string physicsLayerName = "Decor"; // gán layer khi spawn

    [Header("Render")]
    public string sortingLayer = "Decor";
    public int orderInLayer = 0;

    [Header("Collider helper")]
    public bool autoAddCollider = true;
    [Range(0.1f, 1.5f)] public float colliderRadiusScale = 0.5f;

    [System.Serializable]
    public class PropDef
    {
        public GameObject prefab;
        [Range(0.01f, 5f)] public float weight = 1f;
    }
    public PropDef[] props;

    struct CellKey
    {
        public int x, y;
        public CellKey(int x, int y) { this.x = x; this.y = y; }
        public override int GetHashCode() => x * 73856093 ^ y * 19349663;
        public override bool Equals(object obj) => obj is CellKey k && k.x == x && k.y == y;
    }

    Dictionary<CellKey, GameObject> occupied = new();
    List<GameObject> spawned = new();
    Camera cam;

    void Awake() { if (!follow) cam = Camera.main; }
    void LateUpdate()
    {
        if (!follow) follow = cam ? cam.transform : null;
        if (!follow) return;

        CullFar();
        FillRing360();
    }

    void FillRing360()
    {
        if (props == null || props.Length == 0) return;

        Vector2 c = follow.position;
        int quota = Mathf.Min(maxInstances - spawned.Count, 128); // hạn mức mỗi frame
        for (int i = 0; i < quota; i++)
        {
            // random 360° + phân bố đều diện tích (sqrt)
            float ang = Random.value * Mathf.PI * 2f;
            float rad = Mathf.Lerp(innerRadius, radius, Mathf.Sqrt(Random.value));
            Vector2 p = c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rad;

            int gx = Mathf.RoundToInt(p.x / cellSize);
            int gy = Mathf.RoundToInt(p.y / cellSize);
            var key = new CellKey(gx, gy);
            if (occupied.ContainsKey(key)) continue;

            Vector2 world = new Vector2(gx * cellSize, gy * cellSize);

            // tránh player & tránh đè décor bằng physics mask
            float probeR = 0.45f * cellSize;
            if (avoidMask != 0 && Physics2D.OverlapCircle(world, probeR, avoidMask)) continue;
            if (overlapMask != 0 && Physics2D.OverlapCircle(world, probeR, overlapMask)) continue;

            var def = PickWeighted();
            if (def == null || def.prefab == null) continue;

            var go = Instantiate(def.prefab, world, Quaternion.identity, transform);
            SetupSpawned(go);
            occupied[key] = go;
            spawned.Add(go);

            if (spawned.Count >= maxInstances) break;
        }
    }

    PropDef PickWeighted()
    {
        float sum = 0f;
        foreach (var p in props) sum += Mathf.Max(0, p.weight);
        float r = Random.value * sum;
        foreach (var p in props)
        {
            r -= Mathf.Max(0, p.weight);
            if (r <= 0) return p;
        }
        return props.Length > 0 ? props[0] : null;
    }

    void SetupSpawned(GameObject go)
    {
        // ép layer vật lý cho OverlapMask hoạt động
        if (!string.IsNullOrEmpty(physicsLayerName))
            go.layer = LayerMask.NameToLayer(physicsLayerName);

        // ép sorting để chắc chắn vẽ trên BG
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = orderInLayer;
        }

        // tự thêm collider trigger nếu chưa có (để check overlap)
        if (autoAddCollider && go.GetComponent<Collider2D>() == null)
        {
            var c = go.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            if (sr != null)
                c.radius = Mathf.Max(sr.bounds.extents.x, sr.bounds.extents.y) * colliderRadiusScale;
            else
                c.radius = 0.5f * colliderRadiusScale;
        }
    }

    void CullFar()
    {
        Vector2 c = follow.position;
        float maxDist = radius + 3f;
        for (int i = spawned.Count - 1; i >= 0; --i)
        {
            var go = spawned[i];
            if (!go) { spawned.RemoveAt(i); continue; }

            if (Vector2.Distance(go.transform.position, c) > maxDist)
            {
                var key = new CellKey(
                    Mathf.RoundToInt(go.transform.position.x / cellSize),
                    Mathf.RoundToInt(go.transform.position.y / cellSize)
                );
                occupied.Remove(key);
                Destroy(go);
                spawned.RemoveAt(i);
            }
        }
    }
}
