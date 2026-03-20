using System;

public interface IValueInGameMenuItem : IInGameMenuItem
{
    float CurrentValue { get; }
    float MinValue { get; }
    float MaxValue { get; }
    float Step { get; }

    Action<float> OnValueChanged { get; set; } 
}