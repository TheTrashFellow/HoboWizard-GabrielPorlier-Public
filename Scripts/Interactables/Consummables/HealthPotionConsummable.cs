using UnityEngine;

/*
    Code by Gabriel Porlier
*/

public class HealthPotionConsummable : Consummable
{
    [SerializeField] private int _healingAmount;

    [Space]
    [Header("AudioClip for munching glass")]
    [SerializeField] private AudioClip _munchingGlassSound;
    [SerializeField] private float _audioVolume;    

    public override void ConsummableEffect(Player player)
    {
        player._playerHealth += _healingAmount;

        if(player._playerHealth > 100)
            player._playerHealth = 100;

        AudioManager.Instance.PlayAudioOneTime(_munchingGlassSound, _audioVolume, player._projectileTarget);

        Destroy(gameObject);
    }
}
