using System.Collections.Generic;
using UnityEngine;

public class MoveElfin : MonoBehaviour
{
    private GameObject _elfinRobot;
    private Camera _camera;
    private KinematicsSolver _kinematicsSolver;
    private Dictionary<int, Vector3> _jointEditorAxis;

    private void Start()
    {
        _elfinRobot = GameObject.Find("elfin3");
        _camera = Camera.main; //GetComponent<Camera>();
        if (_camera == null)
            Debug.Log("MAIN CAMERA NOT FOUND");

        if (_elfinRobot == null)
            Debug.Log("ELFIN3 NOT FOUND");

        _kinematicsSolver = _elfinRobot.GetComponentInChildren<KinematicsSolver>();

        _jointEditorAxis = new Dictionary<int, Vector3>(_kinematicsSolver.Joints.Length);
        _jointEditorAxis.Add(0, new Vector3(0, 1, 0));
        _jointEditorAxis.Add(1, new Vector3(1, 0, 0));
        _jointEditorAxis.Add(2, new Vector3(1, 0, 0));
        _jointEditorAxis.Add(3, new Vector3(0, 1, 0));
        _jointEditorAxis.Add(4, new Vector3(1, 0, 0));
        _jointEditorAxis.Add(5, new Vector3(0, 1, 0));
        _jointEditorAxis.Add(6, new Vector3(0, 0, 0));
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         RaycastHit hit;
    //         Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             var trans = hit.transform.GetComponent<JoyAxisJointTransformWriter>();
    //             if (trans)
    //             {
    //                 Debug.Log(hit.transform.name +" can move " + hit.transform.name);
    //                 trans.Write(2.0f);
    //             }
    //             else
    //             {
    //                 Debug.Log(hit.transform.name +" can not move " + hit.transform.name);
    //             }
    //         }
    //         
    //     }
    // }

    private void Update()
    {
        // CameraUpdate();

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                float[] jointAngles = {0f, 0f, 0f, 0f, 0f, 0f, 0f};

                // Debug.Log("Before");
                // printArray(jointAngles);

                _kinematicsSolver.InverseKinematics(new Vector3(0f, 0.5f, 0.5f), jointAngles);

                Debug.Log("Angles");
                printArray(jointAngles);

                for (int i = 0; i < jointAngles.Length; i++)
                {
                    var robotJoint = _kinematicsSolver.Joints[i];
                    var jointPosition = robotJoint.transform.position;
                    robotJoint.transform.RotateAround(jointPosition, _jointEditorAxis[i], jointAngles[i]);
                }

                // int i = 0;
                // Debug.Log("Before - Joint rotation: " + _kinematicsSolver.Joints[i].transform.rotation +
                //           ", Joint axis: " + _kinematicsSolver.Joints[i].Axis);

                // _kinematicsSolver.Joints[i].transform.RotateAround(_kinematicsSolver.Joints[i].transform.position, new Vector3(0, 1,0 ), 45f);
                // _kinematicsSolver.Joints[2].transform.RotateAround(_kinematicsSolver.Joints[2].transform.position, new Vector3(1, 0,0 ), 45f);
                // _kinematicsSolver.Joints[1].transform.RotateAround(_kinematicsSolver.Joints[1].transform.position, new Vector3(1, 0,0 ), 45f);
                // _kinematicsSolver.Joints[5].transform.RotateAround(_kinematicsSolver.Joints[5].transform.position,
                //     new Vector3(0, 1, 0), 45f);
                // _kinematicsSolver.Joints[3].transform.RotateAround(_kinematicsSolver.Joints[3].transform.position,
                //     new Vector3(0, 1, 0), 45f);


                // for (int i = 0; i < _kinematicsSolver.Joints.Length; i++)
                // {
                // var jointWriter = _kinematicsSolver.Joints[i].transform
                //     .GetComponentInParent<JoyAxisJointTransformWriter>();
                //
                // // _kinematicsSolver.Joints[i].transform.rotation = Quaternion.Euler(jointAngles[i] * _kinematicsSolver.Joints[i].Axis);
                // var jointAnglePerAxis = Vector3.Scale(
                //     _kinematicsSolver.Joints[i].transform.rotation.eulerAngles, _kinematicsSolver.Joints[i].Axis);
                //
                // Debug.Log("Angle " + i + " = " + jointAnglePerAxis.ToString("F4"));
                //
                // var targetAngle = jointAngles[i] * _kinematicsSolver.Joints[i].Axis;
                //
                // var step = 0;
                // var maxSteps = 100;
                //
                // while (step < maxSteps && Vector3.Distance(jointAnglePerAxis, targetAngle) > 0.1)
                // {
                //     jointWriter.Write(10f);
                //     ++step;
                // }
                //
                // jointAnglePerAxis = Vector3.Scale(
                //     _kinematicsSolver.Joints[i].transform.rotation.eulerAngles, _kinematicsSolver.Joints[i].Axis);
                //
                // Debug.Log("Angle " + i + " = " + jointAnglePerAxis.ToString("F4"));


                // _kinematicsSolver.Joints[i].transform.rotation = Quaternion.Euler(jointAngles[i] * _kinematicsSolver.Joints[i].Axis);
                // }

                // Debug.Log("FINISH !");


                // TODO: uncomment
                // var trans = hit.transform.GetComponent<JointStateWriter>();
                // if (trans)
                // {
                //     Debug.Log(hit.transform.name +" can move " + hit.transform.name);
                //     // trans.transform.Translate(new Vector3(0.5f, 0.5f, 0.5f));
                //     
                //     trans.Write(20.0f);
                // }
                // else
                // {
                //     Debug.Log(hit.transform.name +" can not move " + hit.transform.name);
                // }
            }
        }
    }

    private void printArray(float[] arr)
    {
        var ja = "";
        for (int i = 0; i < arr.Length; i++)
        {
            ja += arr[i];
            if (i < arr.Length - 1)
            {
                ja += ", ";
            }
        }

        Debug.Log(ja);
    }
}