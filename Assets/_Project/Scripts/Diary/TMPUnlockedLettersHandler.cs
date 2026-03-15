using TMPro;
using UnityEngine;
using Zenject;
using System.Text;
using System;

[RequireComponent(typeof(TMP_Text))]
public class TMPUnlockedLettersHandler : MonoBehaviour
{
    [Inject] private UnlockedLettersManager _lettersManager;

    [SerializeField, TextArea] private string _originalText;

    private TMP_Text _tmp;
    private TMP_FontAsset _font;
    private TMP_FontAsset _lockedFont;

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _font = _tmp.font;
        _lockedFont = _lettersManager.LockedLettersFontAsset;
        _originalText = _tmp.text;

        InitializeText();
    }

    private void OnEnable()
    {
        _lettersManager.OnLetterUnlocked += HandleLetterUnlocked;
        _lettersManager.OnReset += HandleReset;
    }

    private void OnDisable()
    {
        _lettersManager.OnLetterUnlocked -= HandleLetterUnlocked;
        _lettersManager.OnReset -= HandleReset;
    }

    private void HandleReset()
    {
        _tmp.text = BuildRichText(_originalText);
    }

    private void HandleLetterUnlocked(char c)
    {
        _tmp.text = BuildRichText(_originalText);
    }

    public void InitializeText()
    {
        _tmp.text = BuildRichText(_originalText);
    }

    private string BuildRichText(string input)
    {
        var builder = new StringBuilder();
        bool insideTag = false;
        var tagBuffer = new StringBuilder();

        var sizeStack = new System.Collections.Generic.Stack<float>();
        float currentSize = 100f; 

        foreach (var ch in input)
        {
            if (ch == '<')
            {
                insideTag = true;
                tagBuffer.Clear();
                tagBuffer.Append(ch);
                continue;
            }
            else if (ch == '>')
            {
                insideTag = false;
                tagBuffer.Append(ch);
                string tag = tagBuffer.ToString();

                if (TryParseSizeTag(tag, out float parsedSize))
                {
                    sizeStack.Push(currentSize);
                    currentSize = currentSize * (parsedSize / 100f);
                }
                else if (tag.Equals("</size>", StringComparison.OrdinalIgnoreCase))
                {
                    if (sizeStack.Count > 0)
                        currentSize = sizeStack.Pop();
                }

                builder.Append(tag);
                continue;
            }

            if (insideTag)
            {
                tagBuffer.Append(ch);
                continue;
            }

            if (!_lettersManager.IsUnlocked(ch) && char.IsLetter(ch))
            {
                float normalWidth = GetGlyphWidth(_font, ch);
                float lockedWidth = GetGlyphWidth(_lockedFont, ch);
                float scaleX = lockedWidth > 0 ? normalWidth / lockedWidth : 1f;

                float finalSize = currentSize * scaleX;

                builder.Append($"<font=\"{_lockedFont.name}\"><size={finalSize:F1}%>{ch}</size></font>");
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    private bool TryParseSizeTag(string tag, out float size)
    {
        size = 100f;
        var match = System.Text.RegularExpressions.Regex.Match(
            tag, @"^<size=(\d+(?:\.\d+)?)(%?)>$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) return false;

        size = float.Parse(match.Groups[1].Value,
            System.Globalization.CultureInfo.InvariantCulture);

        return true;
    }

    private float GetGlyphWidth(TMP_FontAsset fontAsset, char c)
    {
        if (!fontAsset.characterLookupTable.TryGetValue(c, out TMP_Character character))
            return 0;

        if (character.glyph == null)
            return 0;

        return character.glyph.metrics.horizontalAdvance;
    }
}