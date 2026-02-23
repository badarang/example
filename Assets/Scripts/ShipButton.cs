using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShipButton : MonoBehaviour
{
    public ShipData shipData;

    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (shipData != null)
        {
            ShipPlacer.Instance.StartPlacement(shipData);
        }
        else
        {
            Debug.LogError("ShipData is not assigned to this button!");
        }
    }
}
