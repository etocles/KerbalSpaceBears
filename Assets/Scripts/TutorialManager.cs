using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public enum TutorialEvent { None, OnGameStarted, OnRocketLanded, OnFirstFishObtained, 
                            OnFirstOilObtained, OnFirstBearObtained, OnFirstPlanetHalfMelted, OnEnoughFuelAccumulated, 
                            OnFirstPlanetTraveledTo };
[System.Serializable]
public class TutorialPrompt
{
    public Sprite icon;
    public string text;
    public TutorialEvent activationEvent;
    public float duration = 3.0f;
    public UnityEvent OnPromptDisappear = new UnityEvent();
}
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    public Image image;
    public TMPro.TextMeshProUGUI Text;

    public List<TutorialPrompt> TutorialPrompts = new List<TutorialPrompt>();
    private Dictionary<TutorialEvent, TutorialPrompt> TutorialPrompts_Dict = new Dictionary<TutorialEvent, TutorialPrompt>();
    private AnimatedPanel panel;
    public TutorialPrompt activePrompt;
    private void Awake()
    {
        instance = this;
        panel = GetComponent<AnimatedPanel>();
        InitializeDictionary();
    }
    // Start is called before the first frame update
    void Start()
    {
        InitiateTutorialEvent(TutorialEvent.OnGameStarted);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitializeDictionary()
    {
        foreach(TutorialPrompt prompt in TutorialPrompts)
        {
            if(TutorialPrompts_Dict.ContainsKey(prompt.activationEvent) == false && prompt.activationEvent != TutorialEvent.None)
            {
                TutorialPrompts_Dict.Add(prompt.activationEvent, prompt);
            }
            
        }
    }
    public void InitiateTutorialPromptByIndex(int i)
    {
        InitiateTutorialEvent(TutorialEvent.None, i);
        
    }
    public void InitiateTutorialEvent(TutorialEvent Event, int i = -1)
    {
        if (i == -1 && Event != TutorialEvent.None) activePrompt = TutorialPrompts_Dict[Event];
        else activePrompt = TutorialPrompts[i];

        if (activePrompt.duration != -1) StartCoroutine(DisplayTutorialPrompt_Coroutine(Event, activePrompt.duration));
        else DisplayMessage(activePrompt.icon, activePrompt.text);
    }
    public IEnumerator DisplayTutorialPrompt_Coroutine(TutorialEvent Event, float dur)
    {
        DisplayMessage(activePrompt.icon, activePrompt.text);
        yield return new WaitForSeconds(dur);
        StartCoroutine(HideMessage());
    }
    public void DisplayMessage(Sprite icon, string text)
    {
        image.sprite = icon;
        Text.text = text;
        panel.FadeIn();
    }
    public IEnumerator HideMessage()
    {
        panel.FadeOut();
        yield return new WaitForSeconds(panel.timeToFade);
        activePrompt.OnPromptDisappear.Invoke();
    }
}
