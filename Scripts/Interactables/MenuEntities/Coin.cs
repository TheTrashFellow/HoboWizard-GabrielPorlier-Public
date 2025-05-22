using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit;

/*
    Code by Gabriel Porlier

    I should have had a single scripts or inheritance for most of these as these are incredibly repetitive.

    I could have added more fonctions to them individually if nescessary.

    Altough, I ended up manipulating them in the player script, oops ! 
*/

public class Coin : MonoBehaviour
{    
    void Start()
    {
        transform.SetParent(null, true);
    }
}
