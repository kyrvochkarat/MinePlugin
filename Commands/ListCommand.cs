using System;
using System.Linq;
using System.Text;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace LandminePlugin.Commands
{
    public class ListCommand : ICommand
    {
        public string Command => "list";
        public string[] Aliases => new[] { "l", "show" };
        public string Description => "Показывает список всех установленных мин на сервере";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("landmine.list"))
            {
                response = "У вас нет прав для использования этой команды!";
                return false;
            }

            var mineItem = LandminePlugin.Instance?.MineItem;
            if (mineItem == null)
            {
                response = "Плагин мин не загружен!";
                return false;
            }

            var activeMines = mineItem.ActiveMines.Where(m => m != null && !m.IsExploded).ToList();

            if (activeMines.Count == 0)
            {
                response = "На сервере нет установленных мин.";
                return true;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Всего установлено мин: {activeMines.Count}");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            foreach (var mine in activeMines)
            {
                string ownerName = mine.Owner != null ? mine.Owner.Nickname : "Неизвестно";
                string position = $"({mine.Position.x:F1}, {mine.Position.y:F1}, {mine.Position.z:F1})";
                sb.AppendLine($"ID: {mine.Id} | Владелец: {ownerName} | Позиция: {position}");
            }

            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("Используйте 'mine remove <ID>' для удаления мины");

            response = sb.ToString();
            return true;
        }
    }
}
