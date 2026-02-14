using System;
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
            if (IsExploded || _isArmed) return;

            if (Time.time - _spawnTime >= ArmingTime)
            {
                _isArmed = true;
                if (Primitive != null && Primitive.Base != null)
                {
                    Primitive.Color = new Color(0.6f, 0.1f, 0.1f, 1f);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isArmed || IsExploded) return;

            if (!Player.TryGet(collision.gameObject, out var player)) return;
            if (player == null || !player.IsAlive) return;
            if (player == Owner && Time.time - _spawnTime < _ownerSafeTime) return;

            Explode();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isArmed || IsExploded) return;

            if (!Player.TryGet(other.gameObject, out var player)) return;
            if (player == null || !player.IsAlive) return;
            if (player == Owner && Time.time - _spawnTime < _ownerSafeTime) return;

            Explode();
        }

        private void Explode()
        {
            if (IsExploded) return;
            IsExploded = true;

            Vector3 pos = transform.position + Vector3.up * 0.2f;

            try
            {
                ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                grenade.MaxRadius = ExplosionRadius;
                grenade.ScpDamageMultiplier = 1f;
                grenade.FuseTime = 0.1f;
                grenade.SpawnActive(pos, Owner);
            }
            catch (Exception ex)
            {
                Log.Error($"Explosion error: {ex}");
            }

            DestroyPrimitive();
            Destroy(gameObject);
        }

        private void DestroyPrimitive()
        {
            if (Primitive != null && Primitive.Base != null)
            {
                try { NetworkServer.Destroy(Primitive.Base.gameObject); } catch { }
                Primitive = null;
            }
        }

        private void OnDestroy()
        {
            if (Primitive != null && Primitive.Base != null)
            {
                try { NetworkServer.Destroy(Primitive.Base.gameObject); } catch { }
                Primitive = null;
            }
        }

        public void ForceDestroy()
        {
            IsExploded = true;
            DestroyPrimitive();
            Destroy(gameObject);
        }
    }
}