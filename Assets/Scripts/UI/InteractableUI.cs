using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InteractableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite highlightedGraphic;
    public Sprite pressedGraphic;
    public Vector2 offsetMax = new Vector2(12, 12);
    public Vector2 offsetMin = new Vector2(-12, -12);
    public Image.Type imageType = Image.Type.Sliced;

    private Vector2 pressedOffsetMax = Vector2.zero;
    private Vector2 pressedOffsetMin = Vector2.zero;
    private GameObject spawnedImageObj = null;
    private Image spawnedImage = null;
    private RectTransform spawnedRect = null;
    private bool dragging = false;
    private void Awake()
    {


    }
    public void InitializeOverlay()
    {
        spawnedImageObj = new GameObject();
        spawnedImageObj.name = gameObject.name + " Overlay";
        spawnedRect = spawnedImageObj.AddComponent<RectTransform>();
        spawnedRect.SetParent(transform);
        spawnedRect.SetAsFirstSibling();
        spawnedRect.localScale = new Vector3(1, 1, 1);

        spawnedRect.anchorMin = new Vector2(0, 0);
        spawnedRect.anchorMax = new Vector2(1, 1);
        spawnedRect.pivot = new Vector2(0.5f, 0.5f);
        spawnedRect.offsetMax = offsetMax;
        spawnedRect.offsetMin = offsetMin;
        //rect.sizeDelta = new Vector2(-12, -12);
        //rect.anchoredPosition = new Vector2(-12, -12);

        spawnedImage = spawnedImageObj.AddComponent<Image>();
        spawnedImage.type = imageType;
        spawnedImage.raycastTarget = false;
        spawnedImage.sprite = highlightedGraphic;
        spawnedImageObj.SetActive(false);
    }
    /*
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        spawnedImage.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        spawnedImage.sprite = pressedGraphic;
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {

    }
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        spawnedImage.color = new Color(0.95f, 0.95f, 0.95f, 1.0f);
        spawnedImage.gameObject.SetActive(false);
    }
    */
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (spawnedImageObj != null && dragging == false)
        {
            spawnedImage.color = new Color(0.95f, 0.95f, 0.95f, 1.0f);
            spawnedImage.gameObject.SetActive(true);
        }
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (spawnedImageObj != null && dragging == false)
        {
            spawnedImage.color = new Color(0.95f, 0.95f, 0.95f, 1.0f);
            spawnedImage.gameObject.SetActive(false);
        }

    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (spawnedImageObj != null)
        {
            spawnedImage.sprite = pressedGraphic;
            spawnedImage.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
            spawnedRect.offsetMax = pressedOffsetMax;
            spawnedRect.offsetMin = pressedOffsetMin;
        }
    }
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (spawnedImageObj != null)
        {
            spawnedImage.sprite = highlightedGraphic;
            spawnedImage.color = new Color(0.95f, 0.95f, 0.95f, 1.0f);
            spawnedRect.offsetMax = offsetMax;
            spawnedRect.offsetMin = offsetMin;
        }
    }
    private void OnEnable()
    {
        if (transform.GetChild(0) != null && transform.GetChild(0).name == gameObject.name + " Overlay")
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(0).gameObject);
#else
            Destroy(transform.GetChild(0).gameObject);
#endif
            spawnedImageObj = null;
            spawnedImage = null;
            InitializeOverlay();
        }
        else
        {
            InitializeOverlay();
        }
    }
}