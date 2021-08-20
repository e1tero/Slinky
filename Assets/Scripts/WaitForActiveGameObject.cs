using UnityEngine;

public class WaitForActiveGameObject : CustomYieldInstruction
{
    private readonly GameObject _gameObject;

    public WaitForActiveGameObject(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    public override bool keepWaiting => !_gameObject.activeSelf;
}
