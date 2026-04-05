using System;
using System.Linq;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace LandminePlugin.Commands
{
    public class RemoveAllCommand : ICommand
    {
        public string Command => "removeall";
        public string[] Aliases => new[] { "ra", "clear", "deleteall" };
        public string Description => "Удаляет все установленные мины на сервере";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("landmine.removeall"))
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
                response = "На сервере нет установленных мин для удаления.";
                return true;
            }

            int count = activeMines.Count;
            
            foreach (var mine in activeMines)
            {
                mine.Destroy();
            }

            mineItem.ActiveMines.Clear();

            response = $"Успешно удалено мин: {count}";
            return true;
        }
    }
}
