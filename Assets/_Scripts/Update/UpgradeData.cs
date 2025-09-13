using UnityEngine;

public enum UpgradeKind { Weapon, Passive, PrereqOnly, Special }
public enum EvoPattern
{
    None,
    Spiral,
    Fan,
    Radial
}

[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("ID & UI")]
    public string id;                 // ví dụ: "multishot"
    public string displayName;
    public Sprite icon;

    [Header("Phân loại & cấp")]
    public UpgradeKind kind = UpgradeKind.Weapon;
    public int maxLevel = 5;
    public float[] values;            // giá trị theo cấp (dùng cho {V})

    [Header("Mô tả (bình thường)")]
    [TextArea(2, 4)] public string descTemplate = ""; // ví dụ: "Bắn thêm {V} tia."
    public string valueFormat = "0.#";                // ví dụ "0.#", "0.##"

    [Header("Tiến hoá (tuỳ chọn)")]
    public bool canEvolve;
    public string requiredPrereqId;
    public int requiredLevel = 7;
    public EvoPattern evolveToPattern = EvoPattern.None;

    [Header("Thẻ điều kiện thuần tuý")]
    public bool isPrereqOnly;

    [Header("Special")]
    public bool isReroll;
    public bool isSkip;

    // ===== Chế độ bí ẩn / khám phá dần =====
    [Header("Mystery / Discovery")]
    public bool obscure = true;               // bật ẩn thông tin cho thẻ này
    public int revealAfterPicks = 2;          // chọn X lần thì lộ info
    public bool hideNumbers = true;           // ẩn số {V} ngay cả khi đã lộ
    public bool hideEvoHint = true;           // ẩn hint tiến hoá
    public string unknownName = "???";        // tên khi chưa lộ
    [TextArea(2, 4)] public string unknownDesc = "Khá… thú vị. Hiệu ứng khó đoán.";
    public Sprite unknownIcon;                // icon dấu hỏi (nếu có)
}
