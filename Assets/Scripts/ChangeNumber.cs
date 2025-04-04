using UnityEngine;
using TMPro;

public class ChangeNumber : MonoBehaviour
{
    public TMP_Text label;

    public Material on;
    public Material off;

    private PuzzleFunctionBack manager;
    [HideInInspector] public int buttonNumber;
    [HideInInspector] public bool clicked;

    private void Start()
    {
        clicked = false;
    }
    public void Initialize(PuzzleFunctionBack manager, int number)
    {
        this.manager = manager;
        this.buttonNumber = number;
        clicked = false;
    }

    void OnMouseDown()
    {
        if (!clicked && manager != null)
        {
            manager.HandleButtonPress(this);
        }
    }

    public void updateNumber(int num)
    {
        string text = num.ToString();
        label.text = text;
    }

    private void Update()
    {
        if (clicked)
        {
            gameObject.GetComponent<MeshRenderer>().material = on;
            gameObject.transform.localPosition = new Vector3(0, 0, 0.15f);
        }
        else if (!clicked)
        {
            gameObject.GetComponent<MeshRenderer>().material = off;
            gameObject.transform.localPosition = Vector3.zero;
        }
    }
}
