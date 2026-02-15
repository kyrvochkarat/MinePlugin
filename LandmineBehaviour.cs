using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Toys;
using Mirror;

namespace LandminePlugin
{
    public class LandmineBehaviour : MonoBehaviour
    {
        public Player Owner { get; set; }
        public Primitive Primitive { get; set; }
        public float ArmingTime { get; set; }
        public float ExplosionRadius { get; set; }
        public float TriggerRadius { get; set; }
        public bool IsExploded { get; private set; } = false;

        private bool _isArmed = false;
        private float _spawnTime;
        private float _ownerSafeTime;

        private void Start()
        {
            _spawnTime = Time.time;
            _ownerSafeTime = ArmingTime + 3f;
        }

        private void Update()
        {
            if (IsExploded) return;

            if (!_isArmed)
            {
                if (Time.time - _spawnTime >= ArmingTime)
                {
                    _isArmed = true;
                    if (Primitive != null && Primitive.Base != null)
                    {
                        Primitive.Color = new Color(0.6f, 0.1f, 0.1f, 1f);
                    }
                }
                return;
            }

            float timeSinceSpawn = Time.time - _spawnTime;

            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                if (player == Owner && timeSinceSpawn < _ownerSafeTime) continue;

                float distance = Vector3.Distance(player.Position, transform.position);
                if (distance <= TriggerRadius)
                {
                    Explode();
                    return;
                }
            }
        }

        private void Explode()
        {
            if (IsExploded) return;
            IsExploded = true;

            Vector3 pos = transform.position + Vector3.up * 0.2f;

            if (Item.Create(ItemType.GrenadeHE) is ExplosiveGrenade grenade)
            {
                grenade.MaxRadius = ExplosionRadius;
                grenade.ScpDamageMultiplier = 1f;
                grenade.FuseTime = 0.1f;
                grenade.SpawnActive(pos, Owner);
            }

            DestroyMine();
        }

        public void Defuse()
        {
            if (IsExploded) return;
            IsExploded = true;
            DestroyMine();
        }

        private void DestroyMine()
        {
            if (Primitive != null && Primitive.Base != null && Primitive.Base.gameObject != null)
            {
                NetworkServer.Destroy(Primitive.Base.gameObject);
            }
            Primitive = null;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Primitive != null && Primitive.Base != null && Primitive.Base.gameObject != null)
            {
                NetworkServer.Destroy(Primitive.Base.gameObject);
            }
            Primitive = null;
        }

        public void ForceDestroy()
        {
            IsExploded = true;
            DestroyMine();
        }
    }
}