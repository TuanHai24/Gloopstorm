using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BGAutoTile : MonoBehaviour
{
    [SerializeField] Transform target;          // kéo Player vào đây
    public Camera cam;                // để trống = Camera.main
    [Range(0.7f, 1.2f)]
    public float follow = 1f;         // <1 = parallax nhẹ
    public float paddingTiles = 2f;   // nới thêm số tile xung quanh

    SpriteRenderer sr;
    Vector2 tile; // kích thước 1 tile (world units)

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!cam) cam = Camera.main;

        sr.drawMode = SpriteDrawMode.Tiled;

        // kích thước 1 tile theo world units
        float ppu = sr.sprite.pixelsPerUnit;
        tile = sr.sprite.rect.size / ppu;

        ResizeToCamera();
    }

    void LateUpdate()
    {
        if (!target) return;

        // snap theo lưới để pattern không “bơi”
        float tx = target.position.x * follow;
        float ty = target.position.y * follow;

        float x = Mathf.Floor(tx / tile.x) * tile.x;
        float y = Mathf.Floor(ty / tile.y) * tile.y;

        transform.position = new Vector3(x, y, transform.position.z);
    }

    void ResizeToCamera()
    {
        // diện tích camera (world units)
        float h = 2f * cam.orthographicSize;
        float w = h * cam.aspect;

        // số tile cần để phủ kín + padding
        int nx = Mathf.CeilToInt(w / tile.x) + (int)paddingTiles * 2;
        int ny = Mathf.CeilToInt(h / tile.y) + (int)paddingTiles * 2;

        // đổi sang world size cho SpriteRenderer.size
        sr.size = new Vector2(nx * tile.x, ny * tile.y);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (sr && sr.sprite != null)
        {
            float ppu = sr.sprite.pixelsPerUnit;
            tile = sr.sprite.rect.size / ppu;
        }
    }
#endif
}
