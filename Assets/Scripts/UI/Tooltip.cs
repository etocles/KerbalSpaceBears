using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Type { Custom_Text, Planet, PolarBear, Rocket };
    public Type type;
    public enum EventType { Collider, UI};
    public EventType eventType;
    public float timeToAppear = 0.25f;
    [TextArea]
    public string CustomString = "Only active if \"Type\" is set to \"Custom\"...";
    public Sprite customSprite = null;
    private float timer = 0;
    [HideInInspector] public bool hovering = false;
    private bool tooltipDisplayed = false;

    public void OnMouseEnter()
    {
        if(eventType == EventType.Collider)
        {
            hovering = true;
        }
    }
    public void OnMouseExit()
    {
        if(eventType == EventType.Collider)
        {
            hovering = false;
            tooltipDisplayed = false;
            timer = 0;
        }
    }
    public void Update()
    {
        if(hovering)
        {
            timer += Time.deltaTime;
            if(!tooltipDisplayed && timer > timeToAppear)
            {
                tooltipDisplayed = true;
                TooltipManager.instance.DisplayTooltip(this);
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventType == EventType.UI)
        {
            hovering = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventType == EventType.UI)
        {
            hovering = false;
            tooltipDisplayed = false;
            timer = 0;
        }
            
    }
    
}
