using UnityEngine;

[CreateAssetMenu(fileName = "New ShipData", menuName = "Battleship/Ship Data")]
public class ShipData : ScriptableObject
{
    public Vector2Int size = new Vector2Int(1, 2);
}
