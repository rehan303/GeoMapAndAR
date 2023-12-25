using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
public class ARControler : MonoBehaviour
{
    [SerializeField]
    private GameObject prefeb;

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    // Start is called before the first frame update

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();

    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;

    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                GameObject obj = Instantiate(prefeb, pose.position, pose.rotation);

                if (aRPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                {
                    Vector3 position = obj.transform.position;
                    position.y = 0;
                    Vector3 camPosition = Camera.main.transform.position;
                    camPosition.y = 0;

                    Vector3 dir = camPosition - position;
                    Quaternion targetRotation = Quaternion.LookRotation(-dir);

                    obj.transform.rotation = targetRotation;
                }
            }
        }
    }

    private void Start()
    {
        //StartCoroutine(SeePlayer());
    }

    IEnumerator SeePlayer()
    {
        yield return new WaitUntil(() => gameObject.activeInHierarchy);

        Vector3 cameraPosition = Camera.main.transform.position;
        Quaternion cameraRotation = Camera.main.transform.rotation;
        Vector3 spawnDirection = cameraRotation * Vector3.forward;
        Instantiate(prefeb, cameraPosition + spawnDirection * 2f, cameraRotation);


    }
}
