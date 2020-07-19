using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RosSharp.RosBridgeClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementController : MonoBehaviour
{
    public GameObject placementCursor;
    public Text screenLogger;

    private ARRaycastManager _arRaycastManager; // cast a ray to detect surfaces etc
    private Camera _camera;

    private KinematicProblem _kinematicProblem;

    [SerializeField] [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField] [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PositionPrefab;

    public static bool mode1 = false;

    public static float sign = 1.0f;
    
    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject positionPrefab
    {
        get { return m_PositionPrefab; }
        set { m_PositionPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public static GameObject spawnedRobot { get; set; }

    private static GameObject _positionMarker;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private bool stop = false;
    private static bool _kinnematicsSolverFinished = true;
    private static bool _robotMovedFinished = true;
    private KinematicsSolver _kinematicsSolver;
    private static float[] _jointAngles;


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
        if (spawnedRobot == null)
        {
            placementCursor.SetActive(true);
            UpdatePlacementPose();
        }
        else
        {
            if (!stop)
            {
                if (mode1)
                {
                    interactWithLoadedRobot();
                }
                else
                {
                    interactWithLoadedRobot2();
                }
                
            }
        }
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        var touch = Input.GetTouch(0);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private IEnumerator waitSeconds(float seconds)
    {
        stop = true;
        yield return new WaitForSeconds(seconds);
        stop = false;
    }


    public static void solveKinematics(object parameter)
    {
        var kinematicProblem = (KinematicProblem) parameter;
        KinematicsSolver.InverseKinematics(kinematicProblem);
        _jointAngles = kinematicProblem.angles;

        _kinnematicsSolverFinished = true;
        _robotMovedFinished = false;
    }

    private void SolveInverseKinematics(Vector3 targetPosition)
    {
        var robotJoints = _kinematicsSolver.Joints;
        var jointOffsets = robotJoints.Select(x => x.StartOffset).ToArray();
        var jointAxis = robotJoints.Select(x => x.Axis).ToArray();

        float[] jointAngles = new float[_kinematicsSolver.Joints.Length];

        _kinematicProblem = new KinematicProblem(robotJoints[0].transform.position,
            jointOffsets, jointAxis, targetPosition, jointAngles);
        _kinnematicsSolverFinished = false;
        Thread kinematicThread = new Thread(solveKinematics);
        kinematicThread.Start(_kinematicProblem);
    }

    private void interactWithLoadedRobot2()
    {
        if (Input.touchCount > 0)
        {
            // If clicking on one of UI element, no raycast or robot interaction possible
            if (IsPointerOverUIObject())
            {
                return;
            }

            var worldPoint =
                _camera.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y));
            screenLogger.text = " 3d x: " + worldPoint.x + " y: " + worldPoint.y + " z:" + worldPoint.z + " \n";

            RaycastHit hit;
            var screenPoint = Input.GetTouch(0).position;
            var ray = _camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y));
            if (Physics.Raycast(ray, out hit))
            {
                var trans = hit.transform.GetComponent<JoyAxisJointTransformWriter>();
            
                if (trans == null)
                {
                    screenLogger.text = hit.transform.name + " Detected but not yet ready \n";
                    return;
                }
            
                screenLogger.text = "Moving: " + hit.transform.name + "\n";
                trans.Write(sign * 4.0f);
            }
        }
    }
    
    private void interactWithLoadedRobot()
    {
        if (Input.touchCount > 0)
        {
            // If clicking on one of UI element, no raycast or robot interaction possible
            if (IsPointerOverUIObject())
            {
                return;
            }

            var screenTouchPosition = new Vector2(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);

            if (!_arRaycastManager.Raycast(screenTouchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
                return;

            var hitPose = s_Hits[0].pose;

            _positionMarker = Instantiate(positionPrefab, hitPose.position, Quaternion.identity);

            StartCoroutine(waitSeconds(1.0f));

            UpdateJointsToPosition(hitPose.position);
        }

        if (!_robotMovedFinished)
        {
            var robotJoints = _kinematicsSolver.Joints;
            int numberOfMovableJoint = robotJoints.Length - 2;
            
            var jointOffsets = robotJoints.Select(x => x.StartOffset).ToArray();
            var jointAxis = new Vector3[]
            {
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 0)
            };

            float[] jointAngles = new float[_kinematicsSolver.Joints.Length];

            var pb = new KinematicProblem(robotJoints[0].transform.position,
                jointOffsets, jointAxis, _kinematicProblem.targetPosition, jointAngles);
            
            Vector3 point = KinematicsSolver.ForwardKinematics(pb);
            
            screenLogger.text = "Target: " + _kinematicProblem.targetPosition.ToString() + "\n";
            screenLogger.text += "Curr: " + point.ToString() + "\n";
            
            var distanceFromTarget = KinematicsSolver.DistanceFromTarget(pb);
            screenLogger.text += "Dist: " + distanceFromTarget + "\n";

            Dictionary<int, Vector3> _jointEditorAxis = new Dictionary<int, Vector3>(numberOfMovableJoint);
            _jointEditorAxis.Add(0, new Vector3(1, 0, 0));
            _jointEditorAxis.Add(1, new Vector3(0, 0, 1));
            _jointEditorAxis.Add(2, new Vector3(0, 0, 1));
            _jointEditorAxis.Add(3, new Vector3(1, 0, 0));
            _jointEditorAxis.Add(4, new Vector3(0, 0, 1));
            
            printArray(_jointAngles);
            
            float[] currAngles = new float[numberOfMovableJoint];

            for (int i = 0; i < numberOfMovableJoint; i++)
            {
                robotJoints[i].transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
                currAngles[i] = angle;
                var angleIncrement = _jointAngles[i] - angle;
                robotJoints[i].transform.Rotate(_jointEditorAxis[i], _jointAngles[i]);
            }
            
            printArray(currAngles);

            float[] updatedAngles = new float[numberOfMovableJoint];
            for (int i = 0; i < numberOfMovableJoint; i++)
            {
                robotJoints[i].transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
                updatedAngles[i] = angle;
            }

            pb = new KinematicProblem(robotJoints[0].transform.position,
                jointOffsets, jointAxis, _kinematicProblem.targetPosition, jointAngles);
            
            point = KinematicsSolver.ForwardKinematics(pb);
            
            printArray(updatedAngles);

            screenLogger.text += "Updated pos: " + point.ToString() + "\n";

            _robotMovedFinished = true;
            Destroy(_positionMarker);
        }
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


        // If clicking on one of UI element, no raycast or robot interaction possible
        if (IsPointerOverUIObject())
        {
            return;
        }

        spawnedRobot = Instantiate(m_PlacedPrefab, hitPose.position, rotation);
        _kinematicsSolver = spawnedRobot.GetComponentInChildren<KinematicsSolver>();

        var joints = _kinematicsSolver.Joints;
        // _jointAngles = new float[joints.Length];

        screenLogger.text = "robot pos:" + spawnedRobot.transform.position.ToString() + "\n";
        
        placementCursor.SetActive(false);

        StartCoroutine(waitSeconds(1.0f));
    }


    private void UpdateJointsToPosition(Vector3 targetPosition)
    {
        if (_kinnematicsSolverFinished && _robotMovedFinished)
        {
            screenLogger.text = "New pos: " + targetPosition.ToString() + "... \n";
            SolveInverseKinematics(targetPosition);
        }
    }
    
    private void printArray(float[] arr)
    {
        var ja = "";
        for (int i = 0; i < arr.Length; i++)
        {
            ja += arr[i].ToString("F2");
            if (i < arr.Length - 3)
            {
                ja += ", ";
            }
        }

        screenLogger.text += ja + "\n";
    }
}