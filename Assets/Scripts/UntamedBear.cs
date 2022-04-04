using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntamedBear : Bear {
    
    private MobileUnit Unit;
    public bool PaidFor = false;

    void Start(){
        Unit = GetComponent<MobileUnit>();
        // everytime the Recruit signal is broadcast, listen
        GameplayCanvas.instance.OnTameBear.AddListener(() => {
            // avoid null reference
            if (gameObject.activeSelf)
            {
                // if not the one selected, don't move
                if (Unit.currentTile != GameManager.instance.SelectedTile) return;

                if (GameManager.instance.Rocket.GetComponent<RocketScript>().PayForBear(gameObject))
                {
                    GetComponent<Outline>().enabled = false;
                    Unit.currentTile.currentBear = null;
                    
                    StartCoroutine(ReturnToShip()); 
                }
            }
        });
    }

    public IEnumerator ReturnToShip(){
        bool ableToReturn = true;
        if(!Unit.moving){
            Stack<Tile> path = new Stack<Tile>();
            if(GameManager.instance.ActivePlanet.navManager.findPath(
            Unit.currentTile, GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile, out path)){
                GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile.Occupied = false;
                yield return Unit.moveOnPathCoroutine(path);
                GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile.Occupied = false;
            } else {
                ableToReturn = false;
            }
        }
        if (!ableToReturn) yield break;
        else
        {
            // if voyage is complete, notify ship and deactivate self
            GameManager.instance.Rocket.GetComponent<RocketScript>().RecruitBear(this.gameObject);        
        }
    }
}
