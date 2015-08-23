using UnityEngine;
using System.Collections;

public class Cast_RecieveShadows : MonoBehaviour
{
    Renderer r;

    public bool castShadows;
    public bool receiveShadows;

    // Use this for initialization
    void Start()
    {
        r = GetComponent<Renderer>();

        /*r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        r.receiveShadows = true;*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}
