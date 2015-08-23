using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodOverlay : MonoBehaviour 
{
    Image image;

    float curAlpha = 0;
    public float targetAlpha;

    float colorChangeVel;

    void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(1, 1, 1, curAlpha);
    }

	void Update () 
    {
        image.color = new Color(1, 1, 1, curAlpha);

        curAlpha = Mathf.SmoothDamp(curAlpha, targetAlpha, ref colorChangeVel, .3f);
	}
}
