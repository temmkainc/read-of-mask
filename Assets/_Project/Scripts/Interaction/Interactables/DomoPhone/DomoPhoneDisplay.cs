using TMPro;
using UnityEngine;

public class DomoPhoneDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private int _codeLength = 4;

    private string _currentCode = string.Empty;

    public void UpdateDisplay(string code, bool isError = false, bool raw = false)
    {
        _currentCode = code;

        if (isError)
        {
            _displayText.text = "NOPE";
            _displayText.color = Color.white;
            return;
        }

        _displayText.color = Color.white;
        _displayText.text = raw ? code : code;
    }

    public void Clear()
    {
        _currentCode = string.Empty;
        _displayText.color = Color.white;
        _displayText.text = string.Empty;
    }
}