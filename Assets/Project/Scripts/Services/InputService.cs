using System;
using UnityEngine.InputSystem;

public class InputService : IInputService
{
    private readonly InputSystem_Actions _actions;

    public InputService()
    {
        _actions = new InputSystem_Actions();
    }

    public InputAction FindAction(string path)
    {
        return _actions.FindAction(path);
    }

    public InputAction FindAction(string mapName, string actionName)
    {
        return _actions.asset.FindActionMap(mapName).FindAction(actionName);
    }

    public InputActionMap FindActionMap(string name)
    {
        return _actions.asset.FindActionMap(name);
    }

    public void Enable()
    {
        _actions.Enable();
    }

    public void Disable()
    {
        _actions.Disable();
    }

    public void Dispose()
    {
        _actions.Dispose();
    }
}
