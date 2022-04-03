using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
public class TooltipManager : MonoBehaviour
{
    [Header("Ref")]
    public AnimatedPanel rectAnim;
    public RectTransform rect;
    public static TooltipManager instance;
    public enum Direction { Above, Below, Right, Left }; 
    [System.Serializable]
    public class TooltipInfo
    {
        public Tooltip.Type type;
        public Direction direction;
        [SerializeField]
        public UnityEvent function;
        public float width;
        public float height;

    }
    [Header("Tooltip Database")]
    public List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
    public Dictionary<Tooltip.Type, TooltipInfo> tooltipDatabase = new Dictionary<Tooltip.Type, TooltipInfo>();

    [Header("Custom Tooltip References")]
    public Transform customTextParent;
    public TextMeshProUGUI custom_text;
    public UnityEngine.UI.Image custom_icon;

    private string CustomText = "";
    private float toolTipLerpSpeed = 10.0f;
    [HideInInspector] public Tooltip activeTooltip = null;
    private bool tooltipActive = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        ConvertListToDict();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activeTooltip != null)
        {
            rect.transform.position = Vector3.Lerp(rect.transform.position, Input.mousePosition, toolTipLerpSpeed * Time.deltaTime);
            if(activeTooltip.hovering == false && rectAnim.currentState == AnimatedPanel.PanelState.Visible)
            {
                HideTooltip();
                /*
                if (activeTooltip.eventType == Tooltip.EventType.Collider)
                {
                    RaycastHit hit = GameManager.instance.thisPlayer.MouseHitPoint(GameManager.instance.EverythingLayerMask);
                    if (activeTooltip != null && hit.collider != null)
                    {
                        if (hit.collider.gameObject.GetComponent<Tooltip>() == null)
                        {
                            
                        }
                    }
                }
                else if (activeTooltip.eventType == Tooltip.EventType.UI)
                {
                    if (GameplayCanvas.instance.MouseOnUI == false)
                    {
                        HideTooltip();
                    }
                    else
                    {
                        List<RaycastResult> results = new List<RaycastResult>();
                        GameplayCanvas.instance.canvas.GetComponent<GraphicRaycaster>().Raycast(new PointerEventData(GameplayCanvas.instance.eventSystem), results);
                        foreach (RaycastResult hit in results)
                        {
                            if (hit.gameObject == activeTooltip.gameObject) return;
                        }
                        HideTooltip();
                    }
                }
                */
            }
        }
    }

    void ConvertListToDict()
    {
        foreach(TooltipInfo info in tooltipInfos)
        {
            tooltipDatabase.Add(info.type, info);
        }
    }
    public void HideTooltip()
    {
        rectAnim.FadeOut();
        tooltipActive = false;
        

        Invoke("OnTooltipHidden", rectAnim.timeToFade);
    }
    public void OnTooltipHidden()
    {
        activeTooltip = null;
        if (customTextParent.gameObject.activeInHierarchy) customTextParent.gameObject.SetActive(false);
    }

    public void DisplayTooltip(Tooltip tooltip)
    {
        CancelInvoke("OnTooltipHidden");
        if (customTextParent.gameObject.activeInHierarchy) customTextParent.gameObject.SetActive(false);

        activeTooltip = tooltip;
        TooltipInfo info = tooltipDatabase[tooltip.type];
        Direction dir = info.direction;
        if(tooltip.type == Tooltip.Type.Custom_Text)
        {
            CustomText = activeTooltip.CustomString;
        }
        rect.sizeDelta = new Vector2(info.width, info.height);
        float offsetVal = 45f;
        Vector2 offset = Vector2.zero;
        switch(dir)
        {
            case Direction.Above:
                //rect.anchorMax = new Vector2(1, 0);
                //rect.anchorMin = new Vector2(1, 0);
                rect.pivot = new Vector2(0.5f, 0);
                offset = new Vector2(0, offsetVal);
                break;
            case Direction.Below:
                //rect.anchorMax = new Vector2(1, 1);
                //rect.anchorMin = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 1f);
                offset = new Vector2(0, -offsetVal);
                break;
            case Direction.Right:
                //rect.anchorMax = new Vector2(1, 1);
                //rect.anchorMin = new Vector2(1, 1);
                rect.pivot = new Vector2(0, 0.5f);
                offset = new Vector2(offsetVal, 0);
                break;
            case Direction.Left:
                rect.pivot = new Vector2(1, 0.5f);
                offset = new Vector2(-offsetVal, 0);
                break;

        }
        rect.transform.position = Input.mousePosition + new Vector3(offset.x, offset.y, 0);
        info.function.Invoke();
        ActivateTooltip();
    }
    public void ActivateTooltip()
    {
        if(rectAnim.currentState == AnimatedPanel.PanelState.Visible)
        {
            rectAnim.ForceState(AnimatedPanel.PanelState.Visible);
            
        }
        else
        {
            rectAnim.ForceState(AnimatedPanel.PanelState.Hidden);
            tooltipActive = true;
            rectAnim.ForceFadeIn();
        }
        
    }
    
    public void Tooltip_Custom()
    {
        if(activeTooltip.customSprite == null) 
        custom_text.text = CustomText;
        customTextParent.gameObject.SetActive(true);
    }
    public void Tooltip_Planet()
    {

    }
    public void Tooltip_PolarBear()
    {

    }
}
