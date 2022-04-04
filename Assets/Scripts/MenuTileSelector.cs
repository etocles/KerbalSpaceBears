using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuTileSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public AnimatedPanel panel;
    public Material iceMat;
    public Material hoverMat;
    public RocketMenu rocket;
    public Outline outline;
    public enum Buttons{
        Game, Exit
    }
    public Buttons item;

    

    public void OnPointerEnter(PointerEventData eventData)
	{
		SetHighlight(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetHighlight(false);
	}

    public void OnPointerDown(PointerEventData eventData)
	{
        switch(item){
            case Buttons.Game:
                if(rocket != null)
                    BeginGame();
                else
                    BeginGameWithoutRocket();
                break;
            case Buttons.Exit:
                Application.Quit();
                break;
        }
	}

    private void BeginGame(){
        StartCoroutine(rocket.Launch());
        Fade();
    }

    private void BeginGameWithoutRocket(){
        Fade();
    }

    public void OnFadeCompleted(){
        SceneManager.LoadScene("GameScene");
    }

    public void Fade(){
        panel.SetFadeEventID("1");
        panel.FadeIn();
    }

    public void SetHighlight(bool hilighted)
    {
        outline.enabled = hilighted;
        if (hilighted)
        {
            this.GetComponent<Renderer>().material = hoverMat;
            
        }
        else
        {
            this.GetComponent<Renderer>().material = iceMat;
        }
    }
}
