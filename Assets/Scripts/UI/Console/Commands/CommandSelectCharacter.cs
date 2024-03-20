using static viva.console.DevConsole;

namespace viva.console
{
    public class CommandSelectCharacter : ConsoleCommand
    {
        public sealed override string Name { get; protected set; }
        public sealed override string Command { get; protected set; }
        public sealed override string Description { get; protected set; }
        public sealed override string Help { get; protected set; }
        public sealed override string Example { get; protected set; }

        public CommandSelectCharacter()
        {
            Name = "Select Character";
            Command = "selected";
            Description = "Selects/Deselects Companion's";
            Help = "Syntax: selected <all/none> \n" +
                   $"<color={RequiredColor}><all/none></color> is required!";
            Example = "selected all, selected none";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] data)
        {
            if (data.Length == 2)
            {
                var commandParameter = data[1];
                if (string.IsNullOrWhiteSpace(commandParameter))
                {
                    AddStaticMessageToConsole(ParametersAmount);
                }
                if (commandParameter.Contains("all"))
                {
                    Companion companion = GameDirector.instance.FindNearbyLoli(GameDirector.player.head.position, 500.0f);
                    if (!GameDirector.player.objectFingerPointer.selectedLolis.Contains(companion))
                    {
                        companion.characterSelectionTarget.OnSelected();
                        GameDirector.player.objectFingerPointer.selectedLolis.Add(companion);
                    }

                    AddStaticMessageToConsole("Selected All Companion's");
                }
                else if (commandParameter.Contains("none"))
                {
                    Companion companion = GameDirector.instance.FindNearbyLoli(GameDirector.player.head.position, 500.0f);
                    companion.characterSelectionTarget.OnUnselected();
                    GameDirector.player.objectFingerPointer.selectedLolis.Remove(companion);
                    AddStaticMessageToConsole("Deselected All Companion's");
                }
                else
                {
                    AddStaticMessageToConsole(ArgumentNotRecognized);
                }
            }
            else
            {
                AddStaticMessageToConsole(ParametersAmount);
            }
        }

        public static CommandSelectCharacter CreateCommand()
        {
            return new CommandSelectCharacter();
        }
    }
}

