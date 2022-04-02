using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public enum TutorialEvent { None, OnGameStarted, OnRocketLanded, OnFirstFishObtained, OnFirstOilObtained, OnFirstBearObtained, OnFirstPlanetHalfMelted, OnEnoughFuelAccumulated }
[System.Serializable]
public class TutorialPrompt
{
    public Sprite icon;
    public string text;
    public TutorialEvent activationEvent;
    public bool required = false; // Stays visible until event finished
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
    private TutorialPrompt activePrompt;
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
        InitiateTutorialEvent(TutorialEvent.OnGameStarted, i);
        
    }
    public void InitiateTutorialEvent(TutorialEvent Event, int i = -1)
    {
        if (i == -1) activePrompt = TutorialPrompts_Dict[Event];
        else activePrompt = TutorialPrompts[i];

        if (activePrompt.required == false) StartCoroutine(DisplayTutorialPrompt_Coroutine(Event, 5.0f));
        else DisplayMessage(TutorialPrompts_Dict[Event].icon, TutorialPrompts_Dict[Event].text);
    }
    private IEnumerator DisplayTutorialPrompt_Coroutine(TutorialEvent Event, float dur)
    {
        DisplayMessage(TutorialPrompts_Dict[Event].icon, TutorialPrompts_Dict[Event].text);
        yield return new WaitForSeconds(dur);
        HideMessage();
    }
    public void DisplayMessage(Sprite icon, string text)
    {
        image.sprite = icon;
        Text.text = text;
        panel.FadeIn();
    }
    public void HideMessage()
    {
        activePrompt.OnPromptDisappear.Invoke();
        panel.FadeOut();
        activePrompt = null;
    }
}
