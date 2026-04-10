using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceObjectOnPlane : MonoBehaviour
{
    [SerializeField]
    private GameObject objectPrefab;

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

        if (!_raycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = _hits[0].pose;

        // Keep only one object in scene: create once, then move it.
        if (_spawnedObject == null)
        {
            _spawnedObject = Instantiate(objectPrefab, hitPose.position, hitPose.rotation);
            return;
        }

        _spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
    }
}
