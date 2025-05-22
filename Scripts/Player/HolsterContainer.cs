using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/*
 * Code by Gabriel Porlier

    My attempt at making equipement holsters based on the direction the player's camera is looking. 
    I try to have an wide fonctionnal angle before the holsters turn, so the player can look down at them.
 * */

public class HolsterContainer : MonoBehaviour
{
    [Header("For positionning")]
    [SerializeField] private Transform _camera;

    [SerializeField] private Vector3 _positionOffset;

    [Space]
    [Header("Haptic Settings")]
    [SerializeField] private float _hapticIntensity = 0.5f;
    [SerializeField] private float _hapticDuration = 0.1f;

    [Space]
    [Header("Rotation")]
    [SerializeField] private float _rotationThreshold = 45f; // Degrees before it follows rotation
    [SerializeField] private float _rotationLerpSpeed = 5f;  // How fast it catches up
    [SerializeField] private bool _debugAngle = false;

    [Space]
    [SerializeField] private bool _isHapticFeedbackOn;

    private float _lastStableYRotation;
    
    private void Start()
    {
        _lastStableYRotation = _camera.eulerAngles.y;
    }

    void Update()
    {        

        float currentHeadY = _camera.eulerAngles.y;
        float angleDifference = Mathf.DeltaAngle(_lastStableYRotation, currentHeadY);

        if (Mathf.Abs(angleDifference) > _rotationThreshold)
        {
            // Start following rotation
            _lastStableYRotation = Mathf.LerpAngle(_lastStableYRotation, currentHeadY, Time.deltaTime * _rotationLerpSpeed);
        }

        transform.rotation = Quaternion.Euler(0, _lastStableYRotation, 0);

        if (_debugAngle)
            Debug.Log($"Angle diff: {angleDifference}");

        Vector3 cameraPosition = _camera.position;
        Vector3 actualPosition = transform.position;

        transform.position = new(_positionOffset.x + cameraPosition.x, _positionOffset.y + cameraPosition.y, _positionOffset.z + cameraPosition.z);
    }

    /*
    private void Update()
    {
        Vector3 holsterEuler = transform.eulerAngles;
        Vector3 cameraEuler = _camera.eulerAngles;

        // Only follow the Y axis rotation
        holsterEuler.y = cameraEuler.y;

        transform.rotation = Quaternion.Euler(holsterEuler);

        Vector3 cameraPosition = _camera.position;
        Vector3 actualPosition = transform.position;

        transform.position = new(_positionOffset.x + cameraPosition.x, _positionOffset.y + cameraPosition.y, _positionOffset.z + cameraPosition.z);
        //transform.localPosition = new(_positionOffset.x , _positionOffset.y , _positionOffset.z );
    }*/

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (_isHapticFeedbackOn)
        {
            var interactable = args.interactableObject as XRGrabInteractable;
            var selectingInteractor = interactable?.GetOldestInteractorSelecting();

            if (selectingInteractor is XRBaseInputInteractor controllerInteractor)
            {
                // Detect if it's left or right hand
                controllerInteractor.SendHapticImpulse(_hapticIntensity, _hapticDuration);
            }
        }
        
        
    }

}
