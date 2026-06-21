using System;
using UnityEngine.InputSystem;

public interface IInputService : IDisposable
{
    InputAction FindAction(string path);
    InputAction FindAction(string mapName, string actionName);
    InputActionMap FindActionMap(string name);
    void Enable();
    void Disable();
}
