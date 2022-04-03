using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public enum ContextAction { NavigateWithShip, NavigateWithRover, SearchForFish, SearchForOil, RecallAllBears, TameBear };
public class GameplayCanvas : MonoBehaviour
{
    public static GameplayCanvas instance;
    [Header("Prefabs")]
    public GameObject ContextMenuButtonPrefab;
    public GameObject PopupPrefab;
    public GameObject IconPrefab;
    public GameObject ProgressPrefab;
    [HideInInspector] public bool ContextMenuVisible = false;
    [Header("Icons")]
    public Sprite FishIcon;
    public Sprite OilIcon;
    public Sprite RocketIcon;
    public Sprite BearIcon;
    public Sprite TameIcon;
    public Sprite QuestionMarkIcon;
    [Header("References")]
    public RadialUIHandler FishSlider;
    public RadialUIHandler BearSlider;
    public RadialUIHandler OilSlider;
    public RadialUIHandler HeatSlider;
    public TMPro.TextMeshProUGUI messageText;

    private Transform PopupsParent;
    private Transform IconsParent;
    private Transform ContextMenuParent;
    // Obj      // Icon
    private Dictionary<GameObject, GameObject> SpawnedIcons = new Dictionary<GameObject, GameObject>();


    [HideInInspector] public UnityEvent OnNavigateWithShip;
    [HideInInspector] public UnityEvent OnFirstLanding;
    [HideInInspector] public UnityEvent OnSearchForFish;
    [HideInInspector] public UnityEvent OnSearchForOil;
    [HideInInspector] public UnityEvent OnRecallAllBears;
    [HideInInspector] public UnityEvent OnTameBear;

    private void Awake()
    {
        if (instance == null) instance = this;

        PopupsParent = transform.Find("Popups");
        IconsParent = transform.Find("Icons");
        ContextMenuParent = transform.Find("ContextMenu");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public GameObject CreateIcon(Sprite icon, GameObject objectToFollow, GameObject customPrefab = null)
    {
        GameObject prefab = IconPrefab;
        if (customPrefab != null) prefab = customPrefab;
        GameObject spawnedIcon = Instantiate(prefab);
        spawnedIcon.transform.SetParent(IconsParent);
        spawnedIcon.transform.localScale = Vector3.one;
        spawnedIcon.GetComponent<Image>().sprite = icon;
        spawnedIcon.GetComponent<WorldUIObject>().Initialize(objectToFollow);
        if (SpawnedIcons.ContainsKey(objectToFollow))
        {
            Destroy(SpawnedIcons[objectToFollow]); // Destroy existing icon
            SpawnedIcons[objectToFollow] = spawnedIcon; // Replace with new icon
        }
        else
        {
            SpawnedIcons.Add(objectToFollow, spawnedIcon);
        }
        return spawnedIcon;

    }
    public void SpawnPopup(Sprite icon, string text, Vector3 location)
    {
        GameObject spawnedPopup = Instantiate(PopupPrefab);
        spawnedPopup.transform.SetParent(PopupsParent);
        spawnedPopup.transform.localScale = Vector3.one;
        spawnedPopup.transform.Find("IMG").GetComponent<Image>().sprite = icon;
        spawnedPopup.transform.Find("TXT").GetComponent<TMPro.TextMeshProUGUI>().text = text;
        spawnedPopup.transform.position = Camera.main.WorldToScreenPoint(location) + new Vector3(0, 15, 0);
        spawnedPopup.GetComponent<TransformCurves>().Run();
    }
    public void DisplayContextMenu(GameObject SelectedObject)
    {
        if (ContextMenuVisible) HideContextMenu();
        Tile tileCtrl = SelectedObject.GetComponent<Tile>();
        if (tileCtrl != null)
        {

            if (tileCtrl.parentPlanet == GameManager.instance.ActivePlanet) // Clicking tile on active planet
            {
                if (tileCtrl.BiomeType == Hexsphere.BiomeType.Ice)
                {
                    if (tileCtrl.Occupied)
                    {
                        ContextMenuVisible = true;
                        if (tileCtrl.activeBear == ActiveBear.Tamed)
                        {
                            AddContextButton(ContextAction.SearchForFish);
                            AddContextButton(ContextAction.SearchForOil);
                        }
                        else if (tileCtrl.activeBear == ActiveBear.Untamed)
                        {
                            AddContextButton(ContextAction.TameBear);
                        }
                    }
                    else if (GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile == tileCtrl)
                    {
                        AddContextButton(ContextAction.RecallAllBears);
                        ContextMenuVisible = true;
                    }

                }
            }
            else if (tileCtrl.parentPlanet != GameManager.instance.ActivePlanet) // Clicking tile on another planet
            {
                if (GameManager.ValidTileForLanding(tileCtrl))
                {
                    AddContextButton(ContextAction.NavigateWithShip);
                    ContextMenuVisible = true;
                }
                else
                {
                    PushMessage("Can't Land Here! Try choosing a harder surface.", 1.0f);
                }
            }
            if (ContextMenuVisible)
            {
                ContextMenuParent.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                ContextMenuParent.gameObject.SetActive(true);
            }
        }
    }
    public void HideContextMenu()
    {
        ContextMenuVisible = false;
        ContextMenuParent.gameObject.SetActive(false);
        for (int i = ContextMenuParent.childCount - 1; i >= 0; i--)
        {
            Destroy(ContextMenuParent.GetChild(i).gameObject);
        }
    }
    private bool firstLanding = true;
    public void AddContextButton(ContextAction action)
    {
        GameObject spawnedButton = Instantiate(ContextMenuButtonPrefab);
        spawnedButton.transform.SetParent(ContextMenuParent);
        spawnedButton.transform.localScale = Vector3.one;
        Image img = spawnedButton.GetComponent<ContextMenuButton>().icon;
        Button button = spawnedButton.GetComponent<Button>();
        button.onClick.AddListener(HideContextMenu);
        switch (action)
        {
            case ContextAction.NavigateWithShip:
                // if traveling, don't display anything (Ask Andrew how do)
                //if (GameManager.instance.Rocket.GetComponent<RocketScript>().Traveling)
                img.sprite = RocketIcon;
                if (firstLanding) button.onClick.AddListener(() => { 
                    firstLanding = false;
                    OnFirstLanding?.Invoke(); 
                });
                else button.onClick.AddListener(() => OnNavigateWithShip?.Invoke());
                break;
            case ContextAction.SearchForFish:
                img.sprite = FishIcon;
                button.onClick.AddListener(() => OnSearchForFish?.Invoke());
                AudioManager.instance.Bear_SFX();
                break;
            case ContextAction.SearchForOil:
                img.sprite = OilIcon;
                button.onClick.AddListener(() => OnSearchForOil?.Invoke());
                AudioManager.instance.Bear_SFX();
                break;
            case ContextAction.RecallAllBears:
                img.sprite = BearIcon;
                button.onClick.AddListener(() => OnRecallAllBears?.Invoke());
                AudioManager.instance.Rocket_SFX_Short();
                break;
            case ContextAction.TameBear:
                img.sprite = TameIcon;
                button.onClick.AddListener(() => OnTameBear?.Invoke());
                AudioManager.instance.Bear_SFX();
                break;
        }
    }
    public Sprite GetIconByBearState(PolarBearController.BearState state)
    {
        switch (state)
        {
            case PolarBearController.BearState.DEFAULT:
                return BearIcon;
            case PolarBearController.BearState.FISH:
                return FishIcon;
            case PolarBearController.BearState.OIL:
                return OilIcon;
            case PolarBearController.BearState.LOST:
                return QuestionMarkIcon;
            case PolarBearController.BearState.SHIP:
                return RocketIcon;
        }
        return null;
    }
    public enum Resource { Fish, Bear, Oil, Heat }
    public void SetResourceSliderValue(Resource resourceType, float sliderNormValue, string textValue)
    {
        switch (resourceType)
        {
            case Resource.Fish:
                FishSlider.fillValue = sliderNormValue;
                if (FishSlider.text.text != textValue) FishSlider.text.GetComponent<TransformCurves>().Run();
                FishSlider.text.text = textValue;
                break;
            case Resource.Bear:
                BearSlider.fillValue = sliderNormValue;
                if (BearSlider.text.text != textValue) BearSlider.text.GetComponent<TransformCurves>().Run();
                BearSlider.text.text = textValue;
                break;
            case Resource.Oil:
                OilSlider.fillValue = sliderNormValue;
                if (OilSlider.text.text != textValue) OilSlider.text.GetComponent<TransformCurves>().Run();
                OilSlider.text.text = textValue;
                break;
            case Resource.Heat:
                HeatSlider.fillValue = sliderNormValue;
                HeatSlider.text.text = textValue;
                break;
        }
    }

    public void PushMessage(string message, float duration)
    {
        StartCoroutine(SendMessage_Coroutine(message, duration));
    }
    public IEnumerator SendMessage_Coroutine(string message, float duration)
    {
        AnimatedPanel animPanelCtrl = messageText.GetComponent<AnimatedPanel>();
        messageText.text = message;
        animPanelCtrl.FadeIn();
        yield return new WaitForSeconds(animPanelCtrl.timeToFade + duration);
        animPanelCtrl.FadeOut();
    }
}
