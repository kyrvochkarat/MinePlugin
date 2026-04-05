using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace LandminePlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MineParentCommandBase : ParentCommand
    {
        public MineParentCommandBase() => LoadGeneratedCommands();

        public override string Command => "mine";
        public override string[] Aliases => new[] { "landmine", "lm" };
        public override string Description => "Родительская команда для управления минами";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new ListCommand());
            RegisterCommand(new RemoveCommand());
            RegisterCommand(new RemoveAllCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Доступные подкоманды:\n" +
                       "- mine list - показать список всех мин\n" +
                       "- mine remove <ID> - удалить мину по ID\n" +
                       "- mine removeall - удалить все мины";
            return false;
        }
    }
}
