using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Player _player;

    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _confirmReturnPanel;
    [SerializeField] private GameObject _confirmQuitPanel;
/*
    [Space]
    [Header("Audio")]
    [SerializeField] private AudioClip _goodPress;
    [SerializeField] private AudioClip _badPress;*/

    public void ToggleMainPanel()
    {
        _mainPanel.SetActive(true);
        _confirmReturnPanel.SetActive(false);
        _confirmQuitPanel.SetActive(false);        
    }

    public void ToggleConfirmReturnPanel()
    {
        _mainPanel.SetActive(false);
        _confirmReturnPanel.SetActive(true);
    }

    public void ToggleConfirmQuitPanel()
    {
        _mainPanel.SetActive(false);
        _confirmQuitPanel.SetActive(true);
    }

    public void ReturnToMainScene()
    {        
        _player.CallSceneChange(0);
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stops play mode
#endif
        Application.Quit(); // Quits the built game

    }

}
