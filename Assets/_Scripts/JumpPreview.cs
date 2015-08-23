using UnityEngine;
using System.Collections;

public class JumpPreview : MonoBehaviour 
{
    public LineRenderer sightLine;
    public Transform Reticule;
    public int segmentCount = 20;
    public float segmentScale = 1;

    public Transform origin;
    public Vector3 force;

    private Collider _hitObject;
    public Collider hitObject { get { return _hitObject; } }

    private bool _drawPath = true;

    public void Awake()
    {

    }

    void FixedUpdate()
    {
        if(_drawPath)
        {
            simulatePath();
        }
    }

    public void SetDrawPath(bool DrawPath)
    {
        _drawPath = DrawPath;
        Reticule.gameObject.SetActive(DrawPath);
    }

    void simulatePath()
    {
        Vector3[] segments = new Vector3[segmentCount];

        segments[0] = origin.position;

        Vector3 segVelocity = force * Time.deltaTime;

        _hitObject = null;

        int num = 1;

        for(int i = 1; i < segmentCount; ++i)
        {
            float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;

            segVelocity = segVelocity + Physics.gravity * segTime;

            RaycastHit hit;
            if(Physics.Raycast(segments[i-1], segVelocity, out hit, segmentScale))
            {
                _hitObject = hit.collider;

                Vector3 hitPosition = segments[i - 1] + segVelocity.normalized * hit.distance;
                segments[i] = hitPosition;

                num = i;
                break;
            }
            else
            {
                segments[i] = segments[i - 1] + segVelocity * segTime;
            }
        }

        Color startColor = Color.white;
        Color endColor = startColor;
        startColor.a = 1;
        endColor.a = 1;
        sightLine.SetColors(startColor, endColor);

        sightLine.SetVertexCount(num);
        for (int i = 0; i < num; i++)
            sightLine.SetPosition(i, segments[i]);

        Reticule.position = segments[num];
    }
}
