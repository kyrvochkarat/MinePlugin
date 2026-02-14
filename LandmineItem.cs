using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;

namespace LandminePlugin
{
    public class LandmineItem : CustomItem
    {
        public override uint Id { get; set; } = 100;
        public override string Name { get; set; } = "Мина";
        public override string Description { get; set; } = "Противопехотная мина. Выбросьте чтобы установить.";
        public override float Weight { get; set; } = 0.5f;
        public override ItemType Type { get; set; } = ItemType.Coin;
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        private readonly List<LandmineObject> _activeMines = new List<LandmineObject>();

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            CleanupAllMines();
            base.UnsubscribeEvents();
        }

        protected override void OnWaitingForPlayers()
        {
            CleanupAllMines();
            base.OnWaitingForPlayers();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.Player.Role.Type != RoleTypeId.NtfCaptain) return;

            int count = LandminePlugin.Instance.Config.MinesPerCaptain;
            for (int i = 0; i < count; i++)
            {
                Give(ev.Player);
            }

            ev.Player.ShowHint(
                $"<color=yellow>⚠ Вы получили {count} мин(ы)!</color>\n" +
                "<color=white>Выбросьте монетку чтобы установить мину.</color>",
                5f
            );
        }

        protected override void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.Item == null) return;
            if (!Check(ev.Item)) return;

            ev.IsAllowed = false;
            ev.Player.RemoveItem(ev.Item);

            Vector3 position = ev.Player.Position;

            if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 10f))
            {
                position = hit.point;
            }

            var mine = new LandmineObject(ev.Player, position);
            _activeMines.Add(mine);

            ev.Player.ShowHint(
                "<color=yellow>💣 Мина установлена!</color>\n" +
                $"<color=white>Активация через {LandminePlugin.Instance.Config.ArmingTime} сек.</color>",
                3f
            );

            base.OnDroppingItem(ev);
        }

        public void CleanupAllMines()
        {
            for (int i = _activeMines.Count - 1; i >= 0; i--)
            {
                _activeMines[i]?.Destroy();
            }
            _activeMines.Clear();
        }
    }
}
