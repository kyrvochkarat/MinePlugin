using System;
using System.Collections;
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
        public float ExplosionDamage { get; set; }
        public float ExplosionRadius { get; set; }
        public float TriggerRadius { get; set; }

        private bool _isArmed = false;
        private bool _isExploded = false;
        private float _spawnTime;
        private float _ownerSafeTime;

        private void Start()
        {
            _spawnTime = Time.time;
            _ownerSafeTime = ArmingTime + 3f;
            StartCoroutine(ArmingCoroutine());
        }

        private IEnumerator ArmingCoroutine()
        {
            yield return new WaitForSeconds(ArmingTime);
            _isArmed = true;

            if (Primitive != null && Primitive.Base != null)
            {
                Primitive.Color = new Color(0.6f, 0.1f, 0.1f, 1f);
            }
        }

        private void Update()
        {
            if (!_isArmed || _isExploded) return;

            float timeSinceSpawn = Time.time - _spawnTime;

            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;

                if (player == Owner && timeSinceSpawn < _ownerSafeTime)
                    continue;

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
            if (_isExploded) return;
            _isExploded = true;

            Vector3 pos = transform.position + Vector3.up * 0.2f;

            ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
            grenade.MaxRadius = ExplosionRadius;
            grenade.ScpDamageMultiplier = 1f;
            grenade.FuseTime = 0.1f;
            grenade.SpawnActive(pos, Owner);

            DestroyPrimitive();
            Destroy(gameObject);
        }

        private void DestroyPrimitive()
        {
            try
            {
                if (Primitive != null && Primitive.Base != null)
                {
                    NetworkServer.Destroy(Primitive.Base.gameObject);
                    Primitive = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Cleanup error: {ex}");
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (Primitive != null && Primitive.Base != null)
            {
                try
                {
                    NetworkServer.Destroy(Primitive.Base.gameObject);
                }
                catch { }
                Primitive = null;
            }
        }

        public void ForceDestroy()
        {
            _isExploded = true;
            DestroyPrimitive();
            Destroy(gameObject);
        }
    }
}