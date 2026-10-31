using UnityEngine;
using UnityEngine.UI;

public class PulsingAlert : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Color colorLow = new Color(0.4f, 0f, 0f);  // dark red
    [SerializeField] private Color colorHigh = Color.red;
    [SerializeField] private float pulseSpeed = 4f;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }
    }

    private void Update()
    {
        if (targetGraphic == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        targetGraphic.color = Color.Lerp(colorLow, colorHigh, t);
    }
}