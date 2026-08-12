using System;
using System.Collections.Generic;
using TMPro;

public class UnlockedLettersManager
{
    private readonly ISaveService _saveService;
    private readonly HashSet<char> _unlockedLetters = new HashSet<char>();

    public event Action<char> OnLetterUnlocked;
    public event Action OnReset;

    public TMP_FontAsset LockedLettersFontAsset { get; private set; }

    public UnlockedLettersManager(TMP_FontAsset lockedLettersFontAsset, ISaveService saveService)
    {
        LockedLettersFontAsset = lockedLettersFontAsset;
        _saveService = saveService;

        // Restore letters unlocked in a previous scene/session - without this, every new scene
        // (a fresh Zenject container, fresh instance of this class) would start back at zero.
        foreach (var c in _saveService.GetUnlockedLetters())
            _unlockedLetters.Add(c);
    }

    public void UnlockLetter(char c)
    {
        c = char.ToUpper(c);
        if (!_unlockedLetters.Contains(c))
        {
            _unlockedLetters.Add(c);
            _saveService.SetLetterUnlocked(c);
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