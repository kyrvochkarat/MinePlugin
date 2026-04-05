using System;
using System.Linq;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace LandminePlugin.Commands
{
    public class RemoveCommand : ICommand
    {
        public string Command => "remove";
        public string[] Aliases => new[] { "r", "delete", "del" };
        public string Description => "Удаляет мину по её ID";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("landmine.remove"))
            {
                response = "У вас нет прав для использования этой команды!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Использование: mine remove <ID>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int mineId))
            {
                response = "Неверный формат ID! Используйте число.";
                return false;
            }

            var mineItem = LandminePlugin.Instance?.MineItem;
            if (mineItem == null)
            {
                response = "Плагин мин не загружен!";
                return false;
            }

            var mine = mineItem.ActiveMines.FirstOrDefault(m => m != null && m.Id == mineId && !m.IsExploded);

            if (mine == null)
            {
                response = $"Мина с ID {mineId} не найдена!";
                return false;
            }

            string ownerName = mine.Owner != null ? mine.Owner.Nickname : "Неизвестно";
            mine.Destroy();
            mineItem.ActiveMines.Remove(mine);

            response = $"Мина ID {mineId} (владелец: {ownerName}) успешно удалена!";
            return true;
        }
    }
}
