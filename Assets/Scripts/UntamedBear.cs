using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntamedBear : Bear {
    
    private MobileUnit Unit;

    void Start(){
        Unit = GetComponent<MobileUnit>();
        GameplayCanvas.instance.OnTameBear.AddListener(() => { if (gameObject.activeSelf) StartCoroutine(ReturnToShip()); });
    }

    public IEnumerator ReturnToShip(){
        bool ableToReturn = true;
        if(!Unit.moving){
            Stack<Tile> path = new Stack<Tile>();
            if(Hexsphere.planetInstances[0].navManager.findPath(
            Unit.currentTile, GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile, out path)){
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ableToReturn = false;
            }
        }
        if (ableToReturn) yield break;
        else
        {
            // if voyage is complete, notify ship and deactivate self
            GameManager.instance.Rocket.GetComponent<RocketScript>().RecruitBear(this.gameObject);
        }
    }
}
