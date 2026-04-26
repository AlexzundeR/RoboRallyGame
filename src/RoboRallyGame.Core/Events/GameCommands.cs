namespace RoboRallyGame.Core.Messages;

public sealed record SelectCardCommand(int CardIndex) : ICommand;

public sealed record ConfirmPlayerCommand() : ICommand;

public sealed record ExecuteRegisterCommand() : ICommand;
