using System;
using System.Collections.Generic;
using TMPro;

public class UnlockedLettersManager
{
    private readonly HashSet<char> _unlockedLetters = new HashSet<char>();

    public event Action<char> OnLetterUnlocked;
    public event Action OnReset;

    public TMP_FontAsset LockedLettersFontAsset { get; private set; }

    public UnlockedLettersManager(TMP_FontAsset lockedLettersFontAsset)
    {
        LockedLettersFontAsset = lockedLettersFontAsset;
    }

    public void UnlockLetter(char c)
    {
        c = char.ToUpper(c);
        if (!_unlockedLetters.Contains(c))
        {
            _unlockedLetters.Add(c);
            OnLetterUnlocked?.Invoke(c);
        }
    }

    public bool IsUnlocked(char c)
    {
        c = char.ToUpper(c);
        return !char.IsLetter(c) || _unlockedLetters.Contains(c);
    }

    public HashSet<char> GetUnlockedLetters()
    {
        return new HashSet<char>(_unlockedLetters);
    }

    public void SetUnlockedLetters(IEnumerable<char> letters)
    {
        _unlockedLetters.Clear();
        foreach (var c in letters)
            _unlockedLetters.Add(char.ToUpper(c));
    }

    public void ClearUnlockedLetters()
    {
        _unlockedLetters.Clear();
        OnReset?.Invoke();
    }
}