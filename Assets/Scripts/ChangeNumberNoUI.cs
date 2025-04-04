using UnityEngine;

public class ChangeNumberNoUI : MonoBehaviour
{

    public Material on;
    public Material off;

    private PuzzleFunctionFront manager;
    [HideInInspector] public int buttonNumber;
    [HideInInspector] public bool clicked;

    private void Start()
    {
        clicked = false;
    }
    public void Initialize(PuzzleFunctionFront manager, int number)
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
