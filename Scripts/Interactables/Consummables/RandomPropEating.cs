using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/*
    Code by Gabriel Porlier
*/

public class RandomPropEating : Consummable
{
    [SerializeField] private List<AudioClip> _munchingMisc;
    [SerializeField] private float _audioVolume;

    private void Start()
    {
        GetComponent<XRGrabInteractable>().selectExited.AddListener(OnSelectExit);
    }

    public override void ConsummableEffect(Player player)
    {
        int index = Random.Range(0, _munchingMisc.Count);
        AudioManager.Instance.PlayAudioOneTime(_munchingMisc[index], _audioVolume, player._projectileTarget);


        GetComponent<XRGrabInteractable>().selectExited.RemoveListener(OnSelectExit);
        Destroy(gameObject);
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {        
        gameObject.GetComponent<Rigidbody>().isKinematic = false;        
    }

}
