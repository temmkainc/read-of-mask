using UnityEngine;
using Cysharp.Threading.Tasks;

public class LetterCubeSlotReceiverWithVoiceline : LetterCubeSlotReceiver
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private VoicelineData _voicelineData;
    [SerializeField] private char _expectedLetter;
    [SerializeField] private WordPassCompleteCondition _wordPassCompleteCondition;
    private bool _hasPlayedVoiceline;
    public override void SetCurrentLetter(char letter = char.MinValue)
    {
        if (letter == _expectedLetter && !_hasPlayedVoiceline && _wordPassCompleteCondition != null && !_wordPassCompleteCondition.CanPlayVoiceline)
        {
            AudioManager.Instance.PlayVoicelineAsync(_voicelineData.Id).Forget();
            _hasPlayedVoiceline = true;
        }
        base.SetCurrentLetter(letter);
    }
}
