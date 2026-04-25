using UnityEngine;

public class PlayerFirstPersonRPSController : MonoBehaviour
{
    private Animator _animator;

    private readonly int DoRockHash = Animator.StringToHash("DoRock");
    private readonly int DoPaperHash = Animator.StringToHash("DoPaper");
    private readonly int DoScissorsHash = Animator.StringToHash("DoScissors");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayAnimation(RockPaperScissorsChoice choice)
    {
        ResetRPSAnimations();
        int hash = choice switch
        {
            RockPaperScissorsChoice.Rock => DoRockHash,
            RockPaperScissorsChoice.Paper => DoPaperHash,
            RockPaperScissorsChoice.Scissors => DoScissorsHash,
            _ => DoRockHash
        };
        _animator.SetTrigger(hash);
    }

    private void ResetRPSAnimations()
    {
        _animator.ResetTrigger(DoRockHash);
        _animator.ResetTrigger(DoPaperHash);
        _animator.ResetTrigger(DoScissorsHash);
    }

}
