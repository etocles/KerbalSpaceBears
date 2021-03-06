using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialUIHandler : MonoBehaviour
{
    [Range(0, 1)]
    public float fillValue = 0;
    public Image circleFillImage;
    public RectTransform handlerEdgeImage;
    public RectTransform fillHandler;

    public TMPro.TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        FillCircleValue(fillValue);
    }

    void FillCircleValue(float value)
    {
        float fillAmount = value;
        circleFillImage.fillAmount = fillAmount;
        float angle = fillAmount * 360;
        fillHandler.localEulerAngles = new Vector3(0, 0, angle);
        handlerEdgeImage.localEulerAngles = new Vector3(0, 0, -angle);
    }
}
