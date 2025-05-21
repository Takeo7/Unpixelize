using UnityEngine;
using System.Collections;

public class Dots_Script : MonoBehaviour
{
    public GameObject dot1;
    public bool dot1_up;
    public GameObject dot2;
    public bool dot2_up;
    public GameObject dot3;
    public bool dot3_up;

    [Space]
    public float max_Big;
    public float min_Small;
    public float escale = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (dot1_up)
        {
            if (dot1.transform.localScale.x <= max_Big)
            {
                dot1.transform.localScale += new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot1_up = false;
            }
            
        }
        else
        {
            if (dot1.transform.localScale.x >= min_Small)
            {
                dot1.transform.localScale -= new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot1_up = true;
            }
        }

        if (dot2_up)
        {
            if (dot2.transform.localScale.x <= max_Big)
            {
                dot2.transform.localScale += new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot2_up = false;
            }

        }
        else
        {
            if (dot2.transform.localScale.x >= min_Small)
            {
                dot2.transform.localScale -= new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot2_up = true;
            }
        }

        if (dot3_up)
        {
            if (dot3.transform.localScale.x <= max_Big)
            {
                dot3.transform.localScale += new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot3_up = false;
            }

        }
        else
        {
            if (dot3.transform.localScale.x >= min_Small)
            {
                dot3.transform.localScale -= new Vector3(Time.deltaTime * escale, Time.deltaTime * escale);
            }
            else
            {
                dot3_up = true;
            }
        }
    }
}
