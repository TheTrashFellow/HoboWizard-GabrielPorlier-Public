using UnityEngine;

/*
    Code by Gabriel Porlier

    Simply plays the deathSoundEffect when the VFX prefab is instantiated. 
*/

public class EnnemyDeathSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _deathSoundEffect;

    private void Start()
    {
        AudioManager.Instance.PlayAudioOneTime(_deathSoundEffect, 1, transform);
    }
}
