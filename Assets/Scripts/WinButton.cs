using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinButton : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private int _sceneNumToLoad;

    private void Start()
    {
        _nextButton.onClick.AddListener(() => SceneManager.LoadScene(_sceneNumToLoad));
    }

}
