using UnityEngine;
using UnityEngine.UI;

public class WaitForButtonClick : CustomYieldInstruction
{
    private Button _button;
    
    public WaitForButtonClick(Button button)
    {
        _button = button;
        _button.onClick.AddListener(OnClick);
    }
    
    private bool _isClicked;

    public override bool keepWaiting => !_isClicked;

    private void OnClick()
    {
        _isClicked = true;
    }
}
