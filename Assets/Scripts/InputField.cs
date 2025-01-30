using UnityEngine;

public class InputField : MonoBehaviour
{
    public int column;

    public GameManager gm;

    void OnMouseDown()
    {
        gm.SelectColumn(column);
    }

    void OnMouseOver()
    {
        gm.HoverColumn(column);
    }
}
