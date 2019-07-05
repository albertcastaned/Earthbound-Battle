using UnityEngine;
[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy/Enemy Attack")]
public class MovesData : ScriptableObject
{

    public string moveName;
    public string moveMessage;
    public int moveDamageRange;
    public float probability;

}