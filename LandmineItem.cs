using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Items;
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
        private CoroutineHandle _checkCoroutine;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            CleanupAllMines();
            base.UnsubscribeEvents();
        }

        private void OnRoundStarted()
        {
            _checkCoroutine = Timing.RunCoroutine(MineCheckCoroutine());
        }

        private void OnWaitingForPlayers()
        {
            CleanupAllMines();
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

        private void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.Item == null) return;
            if (!Check(ev.Item)) return;

            ev.IsAllowed = true;

            Player owner = ev.Player;
            ushort serial = ev.Item.Serial;

            Timing.RunCoroutine(WaitForLandAndSpawnMine(owner, serial));
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

        private IEnumerator<float> MineCheckCoroutine()
        {
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(LandminePlugin.Instance.Config.CheckInterval);

                for (int i = _activeMines.Count - 1; i >= 0; i--)
                {
                    var mine = _activeMines[i];
                    if (mine == null || mine.IsExploded)
                    {
                        _activeMines.RemoveAt(i);
                        continue;
                    }

                    mine.UpdateVisual();

                    if (!mine.IsArmed) continue;

                    foreach (var player in Player.List)
                    {
                        if (player == null || !player.IsAlive) continue;

                        if (player == mine.Owner)
                        {
                            double timeSincePlaced = (System.DateTime.UtcNow - mine.PlacedAt).TotalSeconds;
                            if (timeSincePlaced < LandminePlugin.Instance.Config.ArmingTime + 3f)
                                continue;
                        }

                        if (mine.IsPlayerInTriggerRange(player))
                        {
                            player.ShowHint("<color=red>💥 ВЫ НАСТУПИЛИ НА МИНУ!</color>", 2f);
                            mine.Explode();
                            _activeMines.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        public void CleanupAllMines()
        {
            if (_checkCoroutine.IsRunning)
                Timing.KillCoroutines(_checkCoroutine);

            foreach (var mine in _activeMines)
                mine?.Destroy();

            _activeMines.Clear();
        }
    }
}