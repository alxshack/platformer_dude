using UnityEngine;

public class Catchable: MonoBehaviour
{
    [SerializeField] private int costValue = 10;
    public int CostValue
    {
        get { return costValue; }
    }
}