using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static Spell_Library;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using static UnityEngine.Rendering.GPUSort;

/*
 * Code and integration for Gandalf AI by Gabriel Porlier
 * */
public class WandManager : MonoBehaviour
{
    [Space]
    [SerializeField] private bool _isInDevMode;

    [Space]
    [Header("For Casting")]
    [SerializeField] public Transform _wandTip = default;       
    [SerializeField] private Spell_Library _spellLibrary = default;
    

    public List<EquippedSpell> _currentSpells;
    private SpellData _preparedSpell = null;
    
    [Space]
    [Header("For holding and casting state")]
    [SerializeField] private InputActionReference _leftHandInput = default;
    [SerializeField] private InputActionReference _rightHandInput = default;

    private bool _isWandHeld = false;
    public XRBaseInteractor _currentController;

    private GameObject _curvedRay;
    private GameObject _straightRay;

    [Space]
    [Header("Estetics")]
    [SerializeField] public ParticleSystem _castingParticle = default;
    [SerializeField] private ParticleSystem _spellReadyParticle = default;
    [SerializeField] private Material _spellParticle = default;

    [Space]
    [Header("CastingSFX")]
    [SerializeField] private AudioClip _castingSucces;
    [SerializeField] private AudioClip _castingFail;


    private void Start()
    {
        transform.SetParent(null, true);

    }

    void Update()
    {
        if(_isWandHeld)
        {
            if(_leftHandInput.action.WasPressedThisFrame() && _currentController.gameObject.tag == "LeftController")
            {                
                DrawingInterfaceManager.Instance.ToggleDrawingPlane(this);
                
            }
            if(_rightHandInput.action.WasPressedThisFrame() && _currentController.gameObject.tag == "RightController")
            {            
                DrawingInterfaceManager.Instance.ToggleDrawingPlane(this);
                
            }
        }    
    }
    
   
    public void HoldingWand(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.gameObject.GetComponent<XRSocketInteractor>()) return;
        
        _isWandHeld = true;
        _currentController = args.interactorObject as XRBaseInteractor;

        if (_currentController.gameObject.tag == "RightController")
        {
            _curvedRay = _currentController.gameObject.GetComponentInParent<Player>()._rightCurvedRay;
            _straightRay = _currentController.gameObject.GetComponentInParent<Player>()._rightStraightRay;
        }
        else
        {
            _curvedRay = _currentController.gameObject.GetComponentInParent<Player>()._leftCurvedRay;
            _straightRay = _currentController.gameObject.GetComponentInParent<Player>()._leftStraightRay;
        }

        if (_preparedSpell != null)
        {
            if (_preparedSpell.isCurvedRay)
            {
                _curvedRay.SetActive(true);
                _curvedRay.GetComponent<XRRayInteractor>().rayOriginTransform = _wandTip;
                _curvedRay.GetComponent<XRInteractorLineVisual>().reticle = _preparedSpell.reticle;
                _curvedRay.GetComponent<XRInteractorLineVisual>().blockedReticle = _preparedSpell.blockedReticle;
            }
            else
            {
                _straightRay.SetActive(true);
                _straightRay.GetComponent<XRRayInteractor>().rayOriginTransform = _wandTip;

                Gradient thisSpellGradient = new();

                var colors = new GradientColorKey[2];
                colors[0] = new GradientColorKey(_preparedSpell.spellColor, 0.0f); colors[1] = new GradientColorKey(_preparedSpell.spellColor, 0.0f);

                var alphas = new GradientAlphaKey[2];
                alphas[0] = new GradientAlphaKey(1.0f, 0.0f); alphas[1] = new GradientAlphaKey(0.0f, 1.0f);

                thisSpellGradient.SetKeys(colors, alphas);

                _straightRay.GetComponent<XRInteractorLineVisual>().validColorGradient = thisSpellGradient; _straightRay.GetComponent<XRInteractorLineVisual>().invalidColorGradient = thisSpellGradient;
            }
        }

        _currentSpells = _currentController.gameObject.GetComponentInParent<Player>()._equippedSpells;

        /*if (_isInDevMode)
        {
            SetPreparedSpell(_spellLibrary.GetSpellById(_currentSpells[0].idSpell));
        }  */
    }

    public void ReleasingWand(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform.gameObject.GetComponent<XRSocketInteractor>()) return;

        _isWandHeld = false;
        DrawingInterfaceManager.Instance.StopCasting(this);

        //VOIR COMMENT FAIRE STOP DRAWING AVEC LE NOUVEAU SYST�ME

        //StopDrawing();
        _curvedRay.SetActive(false);
        _straightRay.SetActive(false);
        _currentController = null;

        transform.SetParent(null, true);
    }
   
    /*
    private void StopCasting()
    {
        _isInCastingState = false;
        //_referencePlane.SetActive(false);
        _pointerBall.SetActive(false);

        _castingParticle.Stop();

        //ResetDrawing();
        Debug.Log("Stopping casting state");
    }*/
            
    public void SetPreparedSpell(int spellId)
    {
        SpellData spell = _spellLibrary.GetSpellById(spellId);

        bool canCast = false;

        foreach(EquippedSpell checkedSpell in _currentSpells)
        {
            if (checkedSpell.idSpell == spellId)
                canCast = true;
        }

        if (!canCast) 
        {
            AudioManager.Instance.PlayAudioOneTime(_castingFail, 4f, transform);
            return;
        }

        AudioManager.Instance.PlayAudioOneTime(_castingSucces, 2f, transform);

        if (_preparedSpell == null)
        {
            _preparedSpell = spell;
            _spellParticle.color = spell.spellColor;
            _spellReadyParticle.Play();
            if (spell.isCurvedRay)
            {
                _curvedRay.SetActive(true);
                _curvedRay.GetComponent<XRRayInteractor>().rayOriginTransform = _wandTip;
                _curvedRay.GetComponent<XRInteractorLineVisual>().reticle = spell.reticle;
                _curvedRay.GetComponent<XRInteractorLineVisual>().blockedReticle = spell.blockedReticle;
            }
            else
            {
                _straightRay.SetActive(true);
                _straightRay.GetComponent<XRRayInteractor>().rayOriginTransform = _wandTip;

                Gradient thisSpellGradient = new();

                var colors = new GradientColorKey[2];
                colors[0] = new GradientColorKey(spell.spellColor, 0.0f); colors[1] = new GradientColorKey(spell.spellColor, 0.0f);

                var alphas = new GradientAlphaKey[2];
                alphas[0] = new GradientAlphaKey(1.0f, 0.0f); alphas[1] = new GradientAlphaKey(0.0f, 1.0f);

                thisSpellGradient.SetKeys(colors, alphas);

                _straightRay.GetComponent<XRInteractorLineVisual>().validColorGradient = thisSpellGradient; _straightRay.GetComponent<XRInteractorLineVisual>().invalidColorGradient = thisSpellGradient;
            }
        }       
        
    }

    public void ReleaseSpell()
    {
        if(_preparedSpell != null)
        {

            int spellLevel = 0;

            foreach(EquippedSpell spell in _currentSpells)
            {
                if (spell.idSpell == _preparedSpell.idSpell)
                {
                    spellLevel = spell.spellLevel;
                    Debug.Log("SpellLevel : " + spellLevel);
                }
            }                        

            GameObject spellPrefab = Instantiate(_preparedSpell.spellPrefab);

            BaseSpell script = spellPrefab.GetComponentInChildren<BaseSpell>();

            if (_preparedSpell.isCurvedRay)
            {
                //
                LineRenderer line = _curvedRay.GetComponent<LineRenderer>();
                int pointCount = line.positionCount;
                Vector3[] points = new Vector3[pointCount];
                line.GetPositions(points);

                Vector3 rayEndPoint = points[pointCount - 1];

                Vector3 flatForward = new Vector3(_wandTip.forward.x, 0, _wandTip.forward.z).normalized;
                Quaternion rotation = Quaternion.LookRotation(flatForward, Vector3.up);

                if (Physics.Raycast(rayEndPoint + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 2f))
                {
                    // Use hit.point as your reticle position
                    Vector3 finalPosition = new(hit.point.x, hit.point.y + 0.5f, hit.point.z);
                    script.CastSpell(finalPosition, rotation.normalized, _preparedSpell.idSpell, spellLevel);
                }
            }                

            else
                script.CastSpell(_wandTip.position, _wandTip.rotation, _preparedSpell.idSpell, spellLevel);

            if (!_isInDevMode)
            {
                _preparedSpell = null;
                _spellReadyParticle.Stop();
                _curvedRay.SetActive(false);
                _straightRay.SetActive(false);
            }            
        }
    }    

    public void PlayCastingParticleSystem()
    {
        _castingParticle.Play();
    }

    public void StopCastingParticleSystem()
    {
        _castingParticle.Stop();
    }
    
}
