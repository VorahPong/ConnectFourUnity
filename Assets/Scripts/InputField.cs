using UnityEngine;

public class InputField : MonoBehaviour
{
    public int column;

    public GameManager gm;

    // When mouse is click on the inputField objects
    void OnMouseDown()
    {
        gm.SelectColumn(column);
    }

    // When mouse is hovering over the inputField objects.
    void OnMouseOver()
    {
        gm.HoverColumn(column);
    }
}
