using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;

namespace LandminePlugin
{
    public class DefuseItem : CustomItem
    {
        public override uint Id { get; set; } = 101;
        public override string Name { get; set; } = "Дефузер";
        public override string Description { get; set; } = "Комплект разминирования. Выбросьте рядом с миной чтобы обезвредить.";
        public override float Weight { get; set; } = 0.5f;
        public override ItemType Type { get; set; } = ItemType.KeycardJanitor;
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            base.UnsubscribeEvents();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.Player.Role.Type != RoleTypeId.ChaosRepressor) return;

            int count = LandminePlugin.Instance.Config.DefusesPerGuard;
            for (int i = 0; i < count; i++)
            {
                Give(ev.Player);
            }

            ev.Player.ShowHint(
                $"<color=cyan>🔧 Вы получили {count} дефузер(ов)!</color>\n" +
                "<color=white>Выбросьте рядом с миной чтобы обезвредить.</color>",
                5f
            );
        }

        protected override void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.Item == null) return;
            if (!Check(ev.Item)) return;

            ev.IsAllowed = false;
            ev.Player.RemoveItem(ev.Item);

            float defuseRadius = LandminePlugin.Instance.Config.DefuseRadius;
            List<LandmineObject> mines = LandminePlugin.Instance.MineItem.ActiveMines;
            bool defused = false;

            for (int i = mines.Count - 1; i >= 0; i--)
            {
                LandmineObject mine = mines[i];
                if (mine == null || mine.IsExploded) continue;
                if (mine.Behaviour == null) continue;

                float distance = Vector3.Distance(ev.Player.Position, mine.Behaviour.transform.position);
                if (distance <= defuseRadius)
                {
                    mine.Behaviour.Defuse();
                    mines.RemoveAt(i);
                    defused = true;
                    break;
                }
            }

            if (defused)
            {
                ev.Player.ShowHint("<color=green>✅ Мина обезврежена!</color>", 3f);
            }
            else
            {
                ev.Player.ShowHint("<color=red>❌ Рядом нет мин для обезвреживания.</color>", 3f);
                Give(ev.Player);
            }

            base.OnDroppingItem(ev);
        }
    }
}