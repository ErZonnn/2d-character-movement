/*Created by Pawe³ Mularczyk*/

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    [Header("GAME MENU OBJECTS")]
    [SerializeField] private GameObject _gameMenu;
    [SerializeField] private GameObject _optionsWindow;

    [Space]
    [Header("PAUSED OBJECTS")]
    [SerializeField] private MovementSystem _characterMovementSystem;

    private InputMaster _inputMaster;

    private InputAction _openMenuAction;

    private bool _openMenuInput = false;
    private bool _openMenuButtIsPressed = false;

    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    private void Start()
    {
        _optionsWindow.SetActive(false);
        _gameMenu.SetActive(false);
    }

    private void OnEnable()
    {
        _openMenuAction = _inputMaster.UI.OpenGameMenu;
        _openMenuAction.Enable();
    }

    private void OnDisable()
    {
        _openMenuAction.Disable();
    }

    private void Update()
    {
        InputHandle();
        OpenGameMenu();

        if (!_gameMenu.activeSelf && _optionsWindow.activeSelf)
            _optionsWindow.SetActive(false);
    }

    private void OpenGameMenu()
    {
        if (_openMenuInput && !_openMenuButtIsPressed)
        {
            _gameMenu.SetActive(!_gameMenu.activeSelf);
            _openMenuButtIsPressed = true;
        }

        if (!_openMenuInput)
            _openMenuButtIsPressed = false;

        if (_gameMenu.activeSelf)
            _characterMovementSystem._pausedGame = true;
        else
            _characterMovementSystem._pausedGame = false;
    }

    private void InputHandle()
    {
        _openMenuInput = _openMenuAction.ReadValue<float>() > 0.1f ? true : false;
    }

    public void OpenOptions()
    {
        _optionsWindow.SetActive(!_optionsWindow.activeSelf);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
