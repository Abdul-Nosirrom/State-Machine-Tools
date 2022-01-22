using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Basic Menu Manager example derived from [TaroDev] Game Manager tutorial
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuButtonsHere;
    [SerializeField] private TextMeshProUGUI _stateText;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }

    private void GameManagerOnGameStateChanged(GameState state)
    {
        throw new System.NotImplementedException();
    }
}