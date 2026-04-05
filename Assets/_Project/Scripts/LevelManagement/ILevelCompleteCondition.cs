using System;

public interface ILevelCompleteCondition
{
    event Action OnConditionMet;
}