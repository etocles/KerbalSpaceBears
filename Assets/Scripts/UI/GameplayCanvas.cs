using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public enum ContextAction { NavigateWithShip, NavigateWithRover, SearchForFish, SearchForOil, RecallAllBears, TameBear };
public class GameplayCanvas : MonoBehaviour
{
    public static GameplayCanvas instance;
    public GameObject ContextMenuButtonPrefab;
    public GameObject PopupPrefab;
    public GameObject IconPrefab;
    [HideInInspector] public bool ContextMenuVisible = false;
    public Sprite FishIcon;
    public Sprite OilIcon;
    public Sprite RocketIcon;
    public Sprite BearIcon;
    public Sprite QuestionMarkIcon;

    private Transform PopupsParent;
    private Transform IconsParent;
    private Transform ContextMenuParent;
                       // Obj      // Icon
    private Dictionary<GameObject, GameObject> SpawnedIcons = new Dictionary<GameObject, GameObject>();


    public UnityEvent OnNavigateWithShip;
    public UnityEvent OnSearchForFish;
    public UnityEvent OnSearchForOil;
    public UnityEvent OnRecallAllBears;
    public UnityEvent OnTameBear;

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
    public void CreateIcon(Sprite icon, GameObject objectToFollow)
    {
        GameObject spawnedIcon = Instantiate(IconPrefab);
        spawnedIcon.transform.SetParent(IconsParent);
        spawnedIcon.transform.localScale = Vector3.one;
        spawnedIcon.GetComponent<Image>().sprite = icon;
        spawnedIcon.GetComponent<WorldUIObject>().Initialize(objectToFollow);
        if(SpawnedIcons.ContainsKey(objectToFollow))
        {
            Destroy(SpawnedIcons[objectToFollow]); // Destroy existing icon
            SpawnedIcons[objectToFollow] = spawnedIcon; // Replace with new icon
        }
        else
        {
            SpawnedIcons.Add(objectToFollow, spawnedIcon);
        }
        
        
    }
    public void SpawnPopup(Sprite icon, string text, Vector3 location)
    {
        GameObject spawnedPopup = Instantiate(PopupPrefab);
        spawnedPopup.transform.SetParent(PopupsParent);
        spawnedPopup.transform.localScale = Vector3.one;
        spawnedPopup.transform.Find("IMG").GetComponent<Image>().sprite = icon;
        spawnedPopup.transform.Find("TXT").GetComponent<TMPro.TextMeshProUGUI>().text = text;
    }
    public void DisplayContextMenu(GameObject SelectedObject)
    {
        if (ContextMenuVisible) HideContextMenu();
        Tile tileCtrl = SelectedObject.GetComponent<Tile>();
        if(tileCtrl != null)
        {
            
            if (tileCtrl.parentPlanet == GameManager.instance.ActivePlanet) // Clicking tile on active planet
            {
                if(tileCtrl.BiomeType == Hexsphere.BiomeType.Ice)
                {
                    if (tileCtrl.Occupied)
                    {
                        AddContextButton(ContextAction.SearchForFish);
                        AddContextButton(ContextAction.SearchForOil);
                        ContextMenuVisible = true;
                    }
                    else if(GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile == tileCtrl)
                    {
                        AddContextButton(ContextAction.RecallAllBears);
                        ContextMenuVisible = true;
                    }
                    
                }
            }
            else if (tileCtrl.parentPlanet != GameManager.instance.ActivePlanet) // Clicking tile on another planet
            {
                if(tileCtrl.BiomeType == Hexsphere.BiomeType.Ice)
                {
                    AddContextButton(ContextAction.NavigateWithShip);
                    ContextMenuVisible = true;
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
    public void AddContextButton(ContextAction action)
    {
        GameObject spawnedButton = Instantiate(ContextMenuButtonPrefab);
        spawnedButton.transform.SetParent(ContextMenuParent);
        spawnedButton.transform.localScale = Vector3.one;
        Image img = spawnedButton.GetComponent<ContextMenuButton>().icon;
        switch (action)
        {
            case ContextAction.NavigateWithShip:
                img.sprite = RocketIcon;
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => OnNavigateWithShip?.Invoke());
                break;
            case ContextAction.SearchForFish:
                img.sprite = FishIcon;
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => OnSearchForFish?.Invoke());
                break;
            case ContextAction.SearchForOil:
                img.sprite = OilIcon;
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => OnSearchForOil?.Invoke());
                break;
            case ContextAction.RecallAllBears:
                img.sprite = BearIcon;
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => OnRecallAllBears?.Invoke());
                break;
            case ContextAction.TameBear:
                img.sprite = FishIcon;
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => OnTameBear?.Invoke());
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
        }
        return null;
    }
}
