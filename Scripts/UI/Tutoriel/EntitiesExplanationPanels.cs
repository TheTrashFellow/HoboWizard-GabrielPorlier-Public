using System.Collections.Generic;
using UnityEngine;

/*
    Code by Gabriel Porlier
*/

public class EntitiesExplanationPanels : MonoBehaviour
{
    [SerializeField] private List<GameObject> _selectedPanelIndicators;
    [SerializeField] private List<GameObject> _panels;
    [SerializeField] private GameObject _forwardButton;
    [SerializeField] private GameObject _backButton;

    private int _currentPanelIndex = 0;
    
    public void NextPanel()
    {
        _selectedPanelIndicators[_currentPanelIndex].SetActive(false);
        _panels[_currentPanelIndex].SetActive(false);

        _currentPanelIndex++;

        _selectedPanelIndicators[_currentPanelIndex].SetActive(true);
        _panels[_currentPanelIndex].SetActive(true);

        if(_currentPanelIndex > 0)
        {
            _backButton.SetActive(true);
        }

        if(_currentPanelIndex == 4)
        {
            _forwardButton.SetActive(false);
        }
    }

    public void PreviousPanel()
    {
        _selectedPanelIndicators[_currentPanelIndex].SetActive(false);
        _panels[_currentPanelIndex].SetActive(false);

        _currentPanelIndex--;

        _selectedPanelIndicators[_currentPanelIndex].SetActive(true);
        _panels[_currentPanelIndex].SetActive(true);

        if (_currentPanelIndex == 0)
        {
            _backButton.SetActive(false);
        }

        if (_currentPanelIndex < 4)
        {
            _forwardButton.SetActive(true);
        }
    }

}
