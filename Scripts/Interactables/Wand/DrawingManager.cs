using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Code adaptated for latest version and right/left hand controller distinction by Gabriel Porlier 
 */

public class DrawingManager : MonoBehaviour
{
    
    public InputActionReference _triggerRight;
    public InputActionReference _triggerLeft;
    public GameObject linePrefab;
    public Transform drawPoint;         // Point à la main
    public Transform lineParent;        // DrawnLines (enfant de DrawingPlane)
    public Transform drawingPlane;      // DrawingPlane invisible

    private LineRenderer currentLine;
    private List<Vector3> points = new();
    private bool isDrawing = false;

    public bool _isInLeftHand = false;

    [SerializeField] private AudioClip _castingActive;
    private GameObject _castingActiveObject;
    [SerializeField] private float _volume = 3f;

    private void Start()
    {
        _castingActiveObject = AudioManager.Instance.SetLoopAudioObject(_castingActive, 0f);
        _castingActiveObject.transform.position = transform.position;
        _castingActiveObject.transform.SetParent(transform);
        _castingActiveObject.GetComponent<AudioSource>().volume = 0f;
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (_isInLeftHand)
        {
            float pressure = _triggerLeft.action.ReadValue<float>();

            if (pressure > 0.1f)
            {
                if (!isDrawing)
                    StartNewLine();

                Vector3 localPoint = drawingPlane.InverseTransformPoint(drawPoint.position);
                localPoint.z = 0f; // Écrase la profondeur

                if (points.Count == 0 || Vector3.Distance(points[^1], localPoint) > 0.005f)
                {

                    points.Add(localPoint);
                    currentLine.positionCount = points.Count;
                    currentLine.SetPositions(points.ToArray());
                }
            }
            else if (isDrawing)
            {
                _castingActiveObject.GetComponent<AudioSource>().volume = 0f;
                isDrawing = false;
                currentLine = null;
                points.Clear();
            }
        }
        else
        {
            float pressure = _triggerRight.action.ReadValue<float>();

            if (pressure > 0.1f)
            {
                if (!isDrawing)
                    StartNewLine();

                Vector3 localPoint = drawingPlane.InverseTransformPoint(drawPoint.position);
                localPoint.z = 0f; // Écrase la profondeur

                if (points.Count == 0 || Vector3.Distance(points[^1], localPoint) > 0.005f)
                {
                    points.Add(localPoint);
                    currentLine.positionCount = points.Count;
                    currentLine.SetPositions(points.ToArray());
                }
            }
            else if (isDrawing)
            {
                _castingActiveObject.GetComponent<AudioSource>().volume = 0f;
                isDrawing = false;
                currentLine = null;
                points.Clear();
            }
        }
        
    }

    void StartNewLine()
    {

        _castingActiveObject.GetComponent<AudioSource>().volume = _volume;

        GameObject newLine = Instantiate(linePrefab, lineParent);
        currentLine = newLine.GetComponent<LineRenderer>();

        if (currentLine == null)
        {
            Debug.LogError("LineRenderer not found on the line prefab!");
            return;
        }

        currentLine.useWorldSpace = false;
        newLine.layer = LayerMask.NameToLayer("Drawing");

        foreach (Transform child in newLine.transform)
            child.gameObject.layer = LayerMask.NameToLayer("Drawing");

        currentLine.sortingLayerName = "Default";
        currentLine.sortingOrder = 10;
        currentLine.startColor = Color.white;
        currentLine.endColor = Color.white;
        currentLine.widthMultiplier = 0.02f;

        points.Clear();
        isDrawing = true;
    }

    public void SetHand(string tag)
    {
        if (tag == "RightController")
            _isInLeftHand = false;
        else
            _isInLeftHand = true;
    }
}
