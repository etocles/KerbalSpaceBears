using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : MonoBehaviour
{
    public enum BearType {
        Brown, Polar
    }
    [SerializeField] private BearType type;
    public BearType GetBearType() => type;


}
