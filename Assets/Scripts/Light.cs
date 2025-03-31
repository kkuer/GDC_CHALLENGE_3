using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Light : MonoBehaviour
{
    public GameObject bulb;
    private MeshRenderer mr;
    public List<Material> lightMats = new List<Material>();
    //OFF   [0]
    //ON    [1]

    public bool flashing;
    private bool delayCheck;

    void Start()
    {
        mr = bulb.GetComponent<MeshRenderer>();
        flashing = false;
        delayCheck = false;
    }

    void Update()
    {
        if (flashing)
        {
            if (!delayCheck)
            {
                StartCoroutine(flash());
            }
        }
    }

    public IEnumerator flash()
    {
        delayCheck = true;
        mr.material = lightMats[1];
        yield return new WaitForSeconds(0.5f);
        mr.material = lightMats[0];
        yield return new WaitForSeconds(0.5f);
        delayCheck = false;
    }
}
