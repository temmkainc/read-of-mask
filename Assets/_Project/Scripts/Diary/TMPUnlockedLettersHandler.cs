using TMPro;
using UnityEngine;
using Zenject;
using System.Text;
using System;

[RequireComponent(typeof(TMP_Text))]
public class TMPUnlockedLettersHandler : MonoBehaviour
{
    // Compile once for all instances
    private static readonly System.Text.RegularExpressions.Regex SizeTagRegex =
        new System.Text.RegularExpressions.Regex(
            @"^<size=(\d+(?:\.\d+)?)(%?)>$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.Compiled);

    [Inject] private UnlockedLettersManager _lettersManager;
    [SerializeField, TextArea] private string _originalText;

    private TMP_Text _tmp;
    private TMP_FontAsset _font;
    private TMP_FontAsset _lockedFont;
    
    // Cache built text — only rebuild when letters actually change
    private string _cachedRichText;
    private bool _isDirty = true;

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _font = _tmp.font;
        _lockedFont = _lettersManager.LockedLettersFontAsset;
        _originalText = _tmp.text;
        
        // Pre-warm font atlas for all chars at startup, not on demand
        PrewarmFontAtlas();
        
        _isDirty = true;
    }

    private void OnEnable()
    {
        _lettersManager.OnLetterUnlocked += HandleLetterUnlocked;
        _lettersManager.OnReset += HandleReset;
        
        // Only rebuild if something actually changed while disabled
        if (_isDirty)
            ApplyText();
    }

    private void OnDisable()
    {
        _lettersManager.OnLetterUnlocked -= HandleLetterUnlocked;
        _lettersManager.OnReset -= HandleReset;
    }

    private void HandleReset()
    {
        _isDirty = true;
        if (gameObject.activeInHierarchy)
            ApplyText();
    }

    private void HandleLetterUnlocked(char c)
    {
        _isDirty = true;
        if (gameObject.activeInHierarchy)
            ApplyText();
    }

    public void InitializeText()
    {
        _isDirty = true;
        ApplyText();
    }

    private void ApplyText()
    {
        _cachedRichText = BuildRichText(_originalText);
        _tmp.text = _cachedRichText;
        _isDirty = false;
    }

    private void PrewarmFontAtlas()
    {
        // Add all chars from original text to both atlases NOW
        // so TryAddCharacters never fires at runtime
        if (_font != null)
            _font.TryAddCharacters(_originalText);
        if (_lockedFont != null)
            _lockedFont.TryAddCharacters(_originalText);
    }

    private string BuildRichText(string input)
    {
        var builder = new StringBuilder(input.Length * 2);
        bool insideTag = false;
        var tagBuffer = new StringBuilder(32);
        var sizeStack = new System.Collections.Generic.Stack<float>(4);
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

                builder.Append("<font=\"").Append(_lockedFont.name)
                       .Append("\"><size=").Append(finalSize.ToString("F1"))
                       .Append("%>").Append(ch)
                       .Append("</size></font>");
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
        var match = SizeTagRegex.Match(tag); // static compiled regex
        if (!match.Success) return false;

        size = float.Parse(match.Groups[1].Value,
            System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }

    private float GetGlyphWidth(TMP_FontAsset fontAsset, char c)
    {
        if (fontAsset == null) return 0f;
        if (!fontAsset.characterLookupTable.TryGetValue(c, out TMP_Character character))
            return 0f; // PrewarmFontAtlas handled missing chars at startup
        if (character.glyph == null) return 0f;
        return character.glyph.metrics.horizontalAdvance;
    }
}