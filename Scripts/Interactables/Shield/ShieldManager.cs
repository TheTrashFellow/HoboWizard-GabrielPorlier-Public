using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/*
 * Code by Gabriel Porlier
 * */
 
public class ShieldManager : MonoBehaviour
{    

    [Space]
    [Header("Parry parameters")]
    [SerializeField] private float _parryCooldown = default;
    [SerializeField] private float _parryTime = default;

    [Space]
    [Header("AttachTransforms")]
    [SerializeField] private Transform _leftTransform = default;
    [SerializeField] private Transform _rightTransform = default;

    private bool _isShieldHeld = false;
    private bool _isInParryState = false;
    private bool _isParryReady = true;

    private XRBaseInteractor _currentController;

    private Coroutine _parryTimeCoroutine = null;
    private Coroutine _cooldownTimerCoroutine = null;

    public Player _player;

    [SerializeField] private GameObject _parryVFX = default;

    [SerializeField] private AudioClip _blockSFX;
    [SerializeField] private AudioClip _parrySFX;

    private void Start()
    {
        transform.SetParent(null, true);
        //_player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void HoldingShield(SelectEnterEventArgs args)
    {
        if (!args.interactorObject.transform.gameObject.GetComponent<XRRayInteractor>())
        {
            _isShieldHeld = true;
            _currentController = args.interactorObject as XRBaseInteractor;
        }

        if(_currentController.gameObject.tag == "LeftController")        
            GetComponent<XRGrabInteractable>().attachTransform = _leftTransform;
        else
            GetComponent<XRGrabInteractable>().attachTransform = _rightTransform;
        

        
    }

    public void HoverShield(HoverEnterEventArgs args)
    {
        var _nextController = args.interactorObject as XRBaseInteractor;

        if (_nextController.gameObject.tag == "LeftController")
            GetComponent<XRGrabInteractable>().attachTransform = _leftTransform;
        else
            GetComponent<XRGrabInteractable>().attachTransform = _rightTransform;
    }

    public void ReleasingShield()
    {
        _isShieldHeld = false;
        _isInParryState = false;        
        _currentController = null;
        transform.SetParent(null, true);
        //_animator.SetBool("IsInReloadState", false);
        //_animator.SetBool("IsInLeftHand", false);
        //transform.localScale = new Vector3(1f, 1f, 1f);        
    }

    public bool BlockedAttack()
    {
        if (_isInParryState)
        {
            StopCoroutine(_cooldownTimerCoroutine);
            _isInParryState=false;
            _isParryReady = true;

            AudioManager.Instance.PlayAudioOneTime(_parrySFX, 1.5f, transform);

            if (_parryVFX != null)
            {
                Instantiate(_parryVFX, transform.position, Quaternion.identity);
            }
            return true;            
        }
        else
        {
            AudioManager.Instance.PlayAudioOneTime(_blockSFX, 1f, transform);
            return false;
        }
    }

    public void ActivateParryState()
    {

        if (_isParryReady && !_isInParryState && _isShieldHeld)
        {
            _isInParryState = true;
            //Active Visuel et audio de parry
            _parryTimeCoroutine = StartCoroutine(ParryTime());
            _cooldownTimerCoroutine = StartCoroutine(CooldownParry());
        }


    }

    IEnumerator ParryTime()
    {
        yield return new WaitForSeconds(_parryTime);
        _isInParryState = false;
    }

    IEnumerator CooldownParry()
    {
        yield return new WaitForSeconds(_parryCooldown);
        _isParryReady = true;
    }
}
