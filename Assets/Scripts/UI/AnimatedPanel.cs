using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class AnimatedPanel : MonoBehaviour
{
    [System.Serializable]
    public class FadeEvent
    {
        public string ID;
        public UnityEvent Event;
    }
    public bool visibleOnAwake = false;
    public float timeToFade = 1.0f;
    public float fullOpacity = 1.0f;
    public enum PanelState { Visible, FadingIn, FadingOut, Hidden };
    public PanelState currentState = PanelState.Hidden;
    public bool AlwaysBlocksRaycasts = false;

    public List<FadeEvent> fadeEvents;

    private string currentFadeEventID;
    [HideInInspector] public CanvasGroup canvasGroup = null;
    private float timer = 0.0f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (visibleOnAwake)
        {
            canvasGroup.interactable = true;
            canvasGroup.alpha = fullOpacity;
            currentState = PanelState.Visible;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0.0f;
            currentState = PanelState.Hidden;
            if (AlwaysBlocksRaycasts == false) canvasGroup.blocksRaycasts = false;
        }
        if (AlwaysBlocksRaycasts == true) canvasGroup.blocksRaycasts = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == PanelState.FadingIn)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0.0f, fullOpacity, timer / timeToFade);
            if(timer > timeToFade)
            {
                currentState = PanelState.Visible;
                timer = 0.0f;
                canvasGroup.blocksRaycasts = true;
                InvokeFadeEvent();
            }
        }
        else if(currentState == PanelState.FadingOut)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(fullOpacity, 0.0f, timer / timeToFade);
            if(timer > timeToFade)
            {
                currentState = PanelState.Hidden;
                timer = 0.0f;
                if(AlwaysBlocksRaycasts == false) canvasGroup.blocksRaycasts = false;
                InvokeFadeEvent();
            }
        }
    }
    public void FadeOut()
    {
        // Only fade out if currently visible.
        if (currentState != PanelState.Visible) return;
        canvasGroup.interactable = false;
        timer = 0.0f;
        currentState = PanelState.FadingOut;
    }

    public void FadeIn()
    {
        // Only fade in if currently hidden.
        if (currentState != PanelState.Hidden) return;
        canvasGroup.interactable = true;
        timer = 0.0f;
        currentState = PanelState.FadingIn;
    }

    public void InvokeFadeEvent()
    {
        if (currentFadeEventID == string.Empty) return;
        foreach(FadeEvent fadeEvent in fadeEvents)
        {
            if(fadeEvent.ID == currentFadeEventID)
            {
                fadeEvent.Event.Invoke();
            }
        }
    }

    public void SetFadeEventID(string ID)
    {
        currentFadeEventID = ID;
    }
}
