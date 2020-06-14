using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp;
using RosSharp.RosBridgeClient;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementController : MonoBehaviour
{
    public GameObject placementCursor;
    public Text screenLogger;

    private ARRaycastManager _arRaycastManager; // cast a ray to detect surfaces etc
    private Camera _camera;

    [SerializeField] [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public static GameObject spawnedRobot { get; set; }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private bool stop = false;
    private bool _kinnematicsSolverFinished = true;
    private bool _robotMovedFinished = true;
    private KinematicsSolver _kinematicsSolver;
    private float[] _jointAngles;


    private void Awake()
    {
        _arRaycastManager = FindObjectOfType<ARRaycastManager>();
        _camera = FindObjectOfType<Camera>();

        var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        placementCursor.SetActive(true);
        placementCursor.transform.SetPositionAndRotation(screenCenter, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        // screenLogger.text = "Is robot null: " + (spawnedRobot == null) + "\n";
        if (spawnedRobot == null)
        {
            placementCursor.SetActive(true);
            UpdatePlacementPose();
        }
        else
        {
            if (!stop)
            {
                interactWithLoadedRobot();
            }
        }
    }

    private IEnumerator waitSeconds(float seconds)
    {
        stop = true;
        yield return new WaitForSeconds(seconds);
        stop = false;
    }


    private IEnumerator SolveInverseKinematics(Vector3 targetPosition, float[] jointAngles)
    {
        _kinnematicsSolverFinished = false;
        screenLogger.text = "Solving ... \n";
        _kinematicsSolver.InverseKinematics(targetPosition, jointAngles);
        screenLogger.text += "Done s ! ";
        printArray(jointAngles);
        // screenLogger.text += "Moving ... ";
        // MoveRobot(targetPosition, jointAngles);
        // StartCoroutine(MoveRobot(targetPosition, jointAngles));
        // screenLogger.text += "Done m ! \n";
        // StartCoroutine(MoveRobot(targetPosition, jointAngles));
        // _robotMovedFinished = false;
        _kinnematicsSolverFinished = true;
        _robotMovedFinished = false;
        yield return null;
    }

    private IEnumerator MoveRobot2(Vector3 targetPosition, float[] jointAngles)
    {
        var robotJoint = _kinematicsSolver.Joints[4];
        var targetAngle = jointAngles[4] * robotJoint.Axis;
        var currentJointAngle = Vector3.Scale(robotJoint.transform.rotation.eulerAngles, robotJoint.Axis);

        var jointWriter = robotJoint.transform.GetComponentInParent<JoyAxisJointTransformWriter>();
        while (Vector3.Distance(targetAngle, currentJointAngle) > 0.05f)
        {
            var tangle = Vector3.Dot(Vector3.one, targetAngle);
            var cangle = Vector3.Dot(Vector3.one, currentJointAngle);
            var sign = (tangle > cangle) ? -1 : 1;
            jointWriter.Write(sign * 1f);
        }

        yield return null;
    }

    private void MoveRobot(float[] jointAngles)
    {
        var robotJoints = _kinematicsSolver.Joints;
        int jointSuccess = 0;
        for (int i = 0; i < robotJoints.Length; i++)
        {
            var robotJoint = robotJoints[i];
            var targetAngle = jointAngles[i] * robotJoint.Axis;
            var currentJointAngle = Vector3.Scale(robotJoint.transform.rotation.eulerAngles, robotJoint.Axis);
            var angleDistance = Vector3.Distance(targetAngle, currentJointAngle);
            
            if (angleDistance > 0.05f)
            {
                var jointWriter = robotJoint.transform.GetComponentInParent<JoyAxisJointTransformWriter>();
                var tangle = Vector3.Dot(robotJoint.Axis, targetAngle);
                var cangle = Vector3.Dot(robotJoint.Axis, currentJointAngle);
                var sign = (tangle > cangle) ? -1 : 1;
                jointWriter.Write(sign * 1f);
            }
            else
            {
                ++jointSuccess;
            }

            if (jointSuccess == robotJoints.Length)
            {
                _robotMovedFinished = true;
            }

        }
        


        // screenLogger.text = "Moving ...\n ";
        // screenLogger.text += "target angle: " + targetAngle.ToString() + 
        //                      "\ncurrent angle: " + currentJointAngle.ToString() + "\n";
        // screenLogger.text += "dist: " + angleDistance;
    }


    private void interactWithLoadedRobot()
    {
        // if (!(Input.touchCount > 0))
        //     return;

        if (Input.touchCount > 0)
        {            
            UpdateJointsToPosition(new Vector3(0f, 0.5f, 0.5f));
        }

        if (!_robotMovedFinished)
        {
            MoveRobot(_jointAngles);
        }

        // var worldPoint =
        //     _camera.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y));
        // screenLogger.text = " 3d x: " + worldPoint.x + " y: " + worldPoint.y + " z:" + worldPoint.z + " \n";

        // RaycastHit hit;
        // var screenPoint = Input.GetTouch(0).position;
        // var ray = _camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y));
        // if (Physics.Raycast(ray, out hit))
        // {
        //     // screenLogger.text = "Touched something ! " + hit.transform.name + " \n";
        //     var trans = hit.transform.GetComponent<JoyAxisJointTransformWriter>();
        //
        //     if (trans == null)
        //     {
        //         screenLogger.text = hit.transform.name + " Detected but not yet ready \n";
        //         return;
        //     }
        //
        //     screenLogger.text = "Moving: " + hit.transform.name + "\n";
        //     trans.Write(4.0f);
        // }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var screenCenter2D = new Vector2(screenCenter.x, screenCenter.y);

        if (!_arRaycastManager.Raycast(screenCenter2D, s_Hits, TrackableType.PlaneWithinPolygon))
            return;

        // Raycast hits are sorted by distance, so the first one will be the closest hit.
        var hitPose = s_Hits[0].pose;
        var cameraForward = _camera.transform.forward;
        var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
        var rotation = Quaternion.LookRotation(cameraBearing);
        placementCursor.transform.SetPositionAndRotation(hitPose.position, rotation); // maybe use identity rotation ?

        if (!(Input.touchCount > 0))
            return;

        spawnedRobot = Instantiate(m_PlacedPrefab, hitPose.position, rotation);
        _kinematicsSolver = spawnedRobot.GetComponentInChildren<KinematicsSolver>();

        var joints = _kinematicsSolver.Joints;
        // _jointAngles = new float[joints.Length];

        screenLogger.text = "orig j1:" + joints[0].transform.position.ToString("F4") + ", " +
                            joints[0].StartOffset.ToString("F4") + "\n";
        screenLogger.text += "orig j2:" + joints[1].transform.position.ToString("F4") + ", " +
                             joints[1].StartOffset.ToString("F4") + "\n";


        // FIXME: the text in the "monitoring console" is not rendered when no more space.
        // screenLogger.text += "Robot placed ! Position: " + hitPose.ToString() + "\n";
        placementCursor.SetActive(false);


        // TODO: When robot is placed, look for all desired GameObject and components, so that it can be moved etc

        StartCoroutine(waitSeconds(1.0f));
        // Input.touchCount = 0;
    }


    private void UpdateJointsToPosition(Vector3 targetPosition)
    {
        // StartCoroutine(SolveInverseKinematics(targetPosition, jointAngles));

        // _kinematicsSolver.InverseKinematics(targetPosition, jointAngles);
        //
        // screenLogger.text = "angles: ";
        // printArray(jointAngles);


        if (_kinnematicsSolverFinished && _robotMovedFinished)
        {
            _jointAngles = new float[_kinematicsSolver.Joints.Length];
            StartCoroutine(SolveInverseKinematics(targetPosition, _jointAngles));
            // float[] jointAngles = {0f, 0f, 0f, 0f, 0f, 0f, 0f};

            // if (_robotMovedFinished)
            // {
            //     _jointAngles = new float[_kinematicsSolver.Joints.Length];
            //     StartCoroutine(SolveInverseKinematics(targetPosition, _jointAngles));
            // }
            //
            // if (_robotMovedFinished)
            // {
            //     StartCoroutine(SolveInverseKinematics(targetPosition, jointAngles));
            // }
            // else
            // {
            //     MoveRobot(targetPosition, jointAngles);
            // }

            // var robotJoint5 = _kinematicsSolver.Joints[4];
            // robotJoint5.transform.RotateAround(robotJoint5.transform.position, robotJoint5.Axis, 45f);
        }

        // for (int i = 0; i < _kinematicsSolver.Joints.Length; i++)
        // {
        // _kinematicsSolver.Joints[i].transform.RotateAround(
        //     _kinematicsSolver.Joints[i].transform.position, _kinematicsSolver.Joints[i].Axis, jointAngles[i]);

        // var jointWriter = _kinematicsSolver.Joints[i].transform.GetComponentInParent<JoyAxisJointTransformWriter>();
        //
        // var jointAnglePerAxis = Vector3.Scale(
        //     _kinematicsSolver.Joints[i].transform.rotation.eulerAngles, _kinematicsSolver.Joints[i].Axis);
        //
        // var targetAngle = jointAngles[i] * _kinematicsSolver.Joints[i].Axis;
        //
        // var step = 0;
        // var maxSteps = 100;
        //
        // while (step < maxSteps && Vector3.Distance(jointAnglePerAxis, targetAngle) > 0.1)
        // {
        //     jointWriter.Write(5f);
        //     ++step;
        // }

        // jointAnglePerAxis = Vector3.Scale(
        //     _kinematicsSolver.Joints[i].transform.rotation.eulerAngles, _kinematicsSolver.Joints[i].Axis);
        //
        // // Debug.Log("Angle " + i + " = " + jointAnglePerAxis.ToString("F4"));
        // }
    }


    private void printArray(float[] arr)
    {
        var ja = "";
        for (int i = 0; i < arr.Length; i++)
        {
            ja += arr[i].ToString("F4");
            if (i < arr.Length - 1)
            {
                ja += ", ";
            }
        }

        screenLogger.text += ja + "\n";
        // Debug.Log(ja);
    }
}