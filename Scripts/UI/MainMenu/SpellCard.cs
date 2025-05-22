using UnityEngine;

/*
    Code by Gabriel Porlier
*/

public class SpellCard : MonoBehaviour
{
    [SerializeField] private GameObject _selectedPin;
    [SerializeField] public int _spellId;
    

    public void ToggleSelectedPin()
    {
        if (_selectedPin.activeSelf)
        {
            _selectedPin.SetActive(false);
        }
        else
        {
            _selectedPin.SetActive(true);
        }
    }

    public void SetSelectedPin()
    {
        _selectedPin.SetActive(true);
    }

    public void UnsetSelectedPin()
    {
        _selectedPin.SetActive(false);
    }
}
