using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
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
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            CleanupAllMines();
            base.UnsubscribeEvents();
        }

        protected override void OnWaitingForPlayers()
        {
            CleanupAllMines();
            base.OnWaitingForPlayers();
        }

        protected override void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.Item == null) return;
            if (!Check(ev.Item)) return;

            ev.IsAllowed = true;

            Player owner = ev.Player;
            ushort serial = ev.Item.Serial;

            Timing.RunCoroutine(WaitForLandAndSpawnMine(owner, serial));

            base.OnDroppingItem(ev);
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole != RoleTypeId.NtfCaptain) return;

            Timing.CallDelayed(1.5f, () =>
            {
                if (ev.Player == null || !ev.Player.IsAlive) return;
                if (ev.Player.Role.Type != RoleTypeId.NtfCaptain) return;

                int count = LandminePlugin.Instance.Config.MinesPerCaptain;
                for (int i = 0; i < count; i++)
                {
                    Give(ev.Player);
                }

                ev.Player.ShowHint(
                    $"<color=yellow>⚠ Вы получили {count} мин(ы)!</color>\n" +
                    "<color=white>Выбросьте монетку чтобы установить мину.\n" +
                    "Мина активируется через 5 секунд.</color>",
                    5f
                );
            });
        }

        private IEnumerator<float> WaitForLandAndSpawnMine(Player owner, ushort serial)
        {
            Exiled.API.Features.Pickups.Pickup pickup = null;

            for (int i = 0; i < 40; i++)
            {
                yield return Timing.WaitForSeconds(0.25f);

                foreach (var p in Exiled.API.Features.Pickups.Pickup.List)
                {
                    if (p.Serial == serial)
                    {
                        pickup = p;
                        break;
                    }
                }

                if (pickup != null) break;
            }

            if (pickup == null) yield break;

            Vector3 previousPosition = pickup.Position;

            for (int i = 0; i < 40; i++)
            {
                yield return Timing.WaitForSeconds(0.15f);

                if (pickup == null || pickup.Base == null) yield break;

                Vector3 currentPosition = pickup.Position;
                float delta = Mathf.Abs(currentPosition.y - previousPosition.y);

                if (delta < 0.01f)
                {
                    Vector3 minePosition = currentPosition;
                    pickup.Destroy();

                    var mine = new LandmineObject(owner, minePosition);
                    _activeMines.Add(mine);

                    if (owner != null && owner.IsAlive)
                    {
                        owner.ShowHint(
                            "<color=yellow>💣 Мина установлена!</color>\n" +
                            $"<color=white>Активация через {LandminePlugin.Instance.Config.ArmingTime} сек. Отойдите!</color>",
                            3f
                        );
                    }

                    yield break;
                }

                previousPosition = currentPosition;
            }

            if (pickup != null && pickup.Base != null)
            {
                Vector3 minePosition = pickup.Position;
                pickup.Destroy();

                var mine = new LandmineObject(owner, minePosition);
                _activeMines.Add(mine);
            }
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