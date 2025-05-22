using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/*
    Code adaptated for latest version by Gabriel Porlier
*/

public class DrawingInterfaceManager : MonoBehaviour
{
    public static DrawingInterfaceManager Instance { get; private set; }

    [Header("Références")]
    public GameObject drawingPlane;
    public GameObject drawnLinesContainer;
    public RenderTexture captureTexture;
    public OnnxClassifier classifier;
    [SerializeField] private DrawingManager _drawingManager;

    [Header("Debug")]
    public Texture2D lastCapturedTexture;

    [SerializeField] private AudioClip _castingBackground;
    private GameObject _castingBackgroundObject;
    [SerializeField] private float _volume = 0.7f;

    [SerializeField] private AudioClip _castingStart;

    [SerializeField] private AudioClip _castingSucces;
    [SerializeField] private AudioClip _castingFail;

    private bool isActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _castingBackgroundObject = AudioManager.Instance.SetLoopAudioObject(_castingBackground, 0f);
        _castingBackgroundObject.transform.position = transform.position;
        _castingBackgroundObject.transform.SetParent(transform);
        _castingBackgroundObject.GetComponent<AudioSource>().volume = 0f;

        if (drawingPlane != null)
            drawingPlane.SetActive(false);
    }

    public void ToggleDrawingPlane(WandManager wandManager)
    {
        Transform wandTip = wandManager._wandTip;

        _drawingManager.SetHand(wandManager._currentController.gameObject.tag);
        isActive = !isActive;

        if (drawingPlane != null)
        {
            if (isActive)
            {
                _drawingManager.enabled = true;
                _drawingManager.drawPoint = wandTip;

                if (wandTip != null)
                {
                    drawingPlane.transform.position = wandTip.position;
                    Vector3 forward = Vector3.ProjectOnPlane(wandTip.forward, Vector3.up).normalized;
                    if (forward != Vector3.zero)
                        drawingPlane.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

                    transform.position += transform.forward * 0.2f;
                }

                drawingPlane.SetActive(true);
                wandManager.PlayCastingParticleSystem();
                AudioManager.Instance.PlayAudioOneTime(_castingStart, 4f, transform);
                _castingBackgroundObject.GetComponent<AudioSource>().volume = _volume;
            }
            else
            {
                _drawingManager.enabled = false;
                lastCapturedTexture = CaptureFromRenderTexture(captureTexture);
                Debug.Log("✔️ Texture capturée : " + lastCapturedTexture.width + "x" + lastCapturedTexture.height);

                if (classifier != null && lastCapturedTexture != null)
                {
                    int id = classifier.RecognizeSpellID(lastCapturedTexture);
                    string name = classifier.GetClassName(id);

                    Debug.Log($"🔎 Sort détecté : {name} | ID : {id}");

                    if (id >= 0)
                    {
                        
                        wandManager.StopCastingParticleSystem();
                        Debug.Log($"📦 Envoi du sort ID {id} à la baguette...");
                        wandManager.SetPreparedSpell(id);
                    }
                    else
                    {
                        AudioManager.Instance.PlayAudioOneTime(_castingFail, 4f, transform);
                        Debug.LogWarning("⚠️ Aucun sort reconnu ou ID invalide.");
                    }
                }

                drawingPlane.SetActive(false);
                _castingBackgroundObject.GetComponent<AudioSource>().volume = 0f;

                if (drawnLinesContainer != null)
                {
                    foreach (Transform child in drawnLinesContainer.transform)
                        Destroy(child.gameObject);
                }
            }
        }
    }

    public void StopCasting(WandManager wandManager)
    {
        wandManager.StopCastingParticleSystem();
        _castingBackgroundObject.GetComponent<AudioSource>().volume = 0f;

        if (drawnLinesContainer != null)
        {
            foreach (Transform child in drawnLinesContainer.transform)
                Destroy(child.gameObject);
        }

        isActive = !isActive;
        _drawingManager.enabled = false;
        drawingPlane.SetActive(false);
    }

    public Texture2D CaptureFromRenderTexture(RenderTexture source)
    {
        RenderTexture.active = source;
        Texture2D tex = new Texture2D(256, 256, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
}
