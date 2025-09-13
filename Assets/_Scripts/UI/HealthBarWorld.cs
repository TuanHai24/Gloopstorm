using UnityEngine;
using UnityEngine.UI;

public class HealthBarWorld : MonoBehaviour
{
    public Image fill;           // kéo Image đỏ vào
    public Vector3 offset;       // tinh chỉnh vị trí nếu cần (thường để (0, -0.6f, 0))

    public void Set(float normalized)   // normalized: 0..1
    {
        if (fill != null)
            fill.fillAmount = Mathf.Clamp01(normalized);
    }

    void LateUpdate()
    {
        // Giữ offset so với Player (vì HPBar là child, dòng này có thể bỏ)
        //transform.localPosition = offset;
        // Nếu muốn luôn quay mặt lên camera:
        // transform.rotation = Quaternion.identity;
    }
}
