using UnityEngine;


public class RobotJoint : MonoBehaviour
{
    public Vector3 Axis;
    public Vector3 StartOffset;

    void Awake ()
    {
        // transform.position - transform.parent.position;
        StartOffset = transform.position - transform.parent.position; //new Vector3(-transform.localPosition.z, -transform.localPosition.x, -transform.localPosition.y);
        // Debug.Log(transform.name + ": " + transform.position + ", " + StartOffset);
    }
}