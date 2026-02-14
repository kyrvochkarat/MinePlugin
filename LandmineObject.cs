using System;
using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Toys;

namespace LandminePlugin
{
    public class LandmineObject
    {
        public Primitive Primitive { get; private set; }
        public Vector3 Position { get; private set; }
        public Player Owner { get; private set; }
        public DateTime PlacedAt { get; private set; }
        public bool IsArmed => (DateTime.UtcNow - PlacedAt).TotalSeconds
                               >= LandminePlugin.Instance.Config.ArmingTime;
        public bool IsExploded { get; private set; } = false;

        public LandmineObject(Player owner, Vector3 position)
        {
            Owner = owner;
            Position = position;
            PlacedAt = DateTime.UtcNow;
            SpawnPrimitive();
        }

        private void SpawnPrimitive()
        {
            try
            {
                Primitive = Primitive.Create(
                    PrimitiveType.Cylinder,
                    Position + Vector3.up * 0.05f,
                    Vector3.zero,
                    new Vector3(0.5f, 0.05f, 0.5f),
                    true
                );

                Primitive.Color = new Color(0.2f, 0.4f, 0.1f, 1f);
                Primitive.Collidable = false;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to spawn landmine primitive: {ex}");
            }
        }

        public void UpdateVisual()
        {
            if (Primitive == null || IsExploded) return;

            if (IsArmed)
            {
                Primitive.Color = new Color(0.6f, 0.1f, 0.1f, 1f);
            }
        }

        public bool IsPlayerInTriggerRange(Player player)
        {
            if (!IsArmed || IsExploded) return false;
            if (player == null || !player.IsAlive) return false;

            float distance = Vector3.Distance(player.Position, Position);
            return distance <= LandminePlugin.Instance.Config.TriggerRadius;
        }

        public void Explode()
        {
            if (IsExploded) return;
            IsExploded = true;

            var config = LandminePlugin.Instance.Config;

            try
            {
                ExplodeEffect();

                foreach (var player in Player.List)
                {
                    if (player == null || !player.IsAlive) continue;

                    float distance = Vector3.Distance(player.Position, Position);
                    if (distance <= config.ExplosionRadius)
                    {
                        float damageMultiplier = 1f - (distance / config.ExplosionRadius);
                        float damage = config.ExplosionDamage * damageMultiplier;

                        player.Hurt(damage, "Landmine Explosion");
                        player.ShowHint("<color=red>💥 Вы попали во взрыв мины!</color>", 3f);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error during landmine explosion: {ex}");
            }

            Destroy();
        }

        private void ExplodeEffect()
        {
            try
            {
                var grenade = (Exiled.API.Features.Items.ExplosiveGrenade)
                    Exiled.API.Features.Items.Item.Create(ItemType.GrenadeHE);

                grenade.FuseTime = 0.05f;
                grenade.MaxRadius = LandminePlugin.Instance.Config.ExplosionRadius;
                grenade.ScpDamageMultiplier = 1f;

                grenade.SpawnActive(Position + Vector3.up * 0.2f, Owner);
            }
            catch (Exception ex)
            {
                Log.Error($"Error spawning explosion effect: {ex}");
            }
        }

        public void Destroy()
        {
            try
            {
                if (Primitive != null)
                {
                    Primitive.Destroy();
                    Primitive = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error destroying landmine primitive: {ex}");
            }
        }
    }
}