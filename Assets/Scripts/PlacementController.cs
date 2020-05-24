using System;
using System.Collections.Generic;
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

    private void Awake()
    {
        _arRaycastManager = FindObjectOfType<ARRaycastManager>();
        _camera = FindObjectOfType<Camera>();

        var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        // screenLogger.transform.SetPositionAndRotation(new Vector3(screenCenter.x, 10), Quaternion.identity);

        placementCursor.SetActive(true);
        placementCursor.transform.SetPositionAndRotation(screenCenter, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

        // UpdatePlacementPose();
        if (spawnedRobot == null)
        {
            placementCursor.SetActive(true);
            UpdatePlacementPose();
        }
        else
        {
            placementCursor.SetActive(false);
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
        var cameraBearing = new Vector3(cameraForward.x, 0 ,cameraForward.z).normalized;
        var rotation = Quaternion.LookRotation(cameraBearing);
        placementCursor.transform.SetPositionAndRotation(hitPose.position, rotation); // maybe use identity rotation ?

        if (!(Input.touchCount > 0))
            return;

        spawnedRobot = Instantiate(m_PlacedPrefab, hitPose.position, rotation);
        
        // FIXME: the text in the "monitoring console" is not rendered when no more space.
        screenLogger.text += "Robot placed ! Position: " + hitPose.ToString() + "\n";
    }
}