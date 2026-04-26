using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceObjectOnPlane : MonoBehaviour
{
    [SerializeField]
    private GameObject objectPrefab;

    [Tooltip("Optional extra yaw in degrees. Set to 180 if the FBX faces backwards.")]
    [SerializeField]
    private float yRotationOffset = 0f;

    [Tooltip("Distance in meters to keep the object above the detected plane.")]
    [Min(0f)]
    [SerializeField]
    private float hoverHeight = 0.1f;

    private ARRaycastManager _raycastManager;
    private readonly List<ARRaycastHit> _hits = new();
    private GameObject _spawnedObject;
    private bool _warnedMissingPrefab;

    private void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        if (_raycastManager == null)
        {
            Debug.LogError($"{nameof(PlaceObjectOnPlane)} requires an {nameof(ARRaycastManager)} component on the same GameObject.");
        }
    }

    private void Update()
    {
        if (_raycastManager == null)
            return;

        if (objectPrefab == null)
        {
            if (!_warnedMissingPrefab)
            {
                Debug.LogWarning($"{nameof(PlaceObjectOnPlane)} has no prefab assigned. Assign one in the Inspector.");
                _warnedMissingPrefab = true;
            }

            return;
        }

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (IsTouchOverUI(touch))
            return;

        if (!_raycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = _hits[0].pose;
        Vector3 targetPosition = GetPlacementPosition(hitPose);
        Quaternion targetRotation = GetFacingCameraRotation(targetPosition, hitPose.rotation);

        // Keep only one object in scene: create once, then move it.
        if (_spawnedObject == null)
        {
            _spawnedObject = Instantiate(objectPrefab, targetPosition, targetRotation);
            return;
        }

        _spawnedObject.transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private static bool IsTouchOverUI(Touch touch)
    {
        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem == null)
            return false;

        return currentEventSystem.IsPointerOverGameObject(touch.fingerId);
    }

    private Vector3 GetPlacementPosition(Pose hitPose)
    {
        if (hoverHeight <= 0f)
            return hitPose.position;

        // Offset along plane normal so the object stays above the detected surface.
        Vector3 planeNormal = hitPose.rotation * Vector3.up;
        return hitPose.position + (planeNormal.normalized * hoverHeight);
    }

    private Quaternion GetFacingCameraRotation(Vector3 objectPosition, Quaternion fallbackRotation)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return fallbackRotation;

        // Match the camera height to the object to avoid tilt on X/Z.
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.y = objectPosition.y;

        Vector3 lookDirection = cameraPosition - objectPosition;
        if (lookDirection.sqrMagnitude < 0.0001f)
            return fallbackRotation;

        Quaternion lookRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

        // If the model is authored backwards, set yRotationOffset to 180.
        if (Mathf.Abs(yRotationOffset) > 0.001f)
        {
            lookRotation *= Quaternion.Euler(0f, yRotationOffset, 0f);
        }

        return lookRotation;
    }
}
