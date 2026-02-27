using UnityEngine;

public static class CommandBusExtension
{
    public static void GoToPreviousPlayerState(this ICommandBus commandBus)
    {
        commandBus.Undo<PlayerStateChangeCommand>();
    }
}