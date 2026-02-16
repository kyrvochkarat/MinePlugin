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
            TryExplode(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryExplode(other.gameObject);
        }

        private void TryExplode(GameObject obj)
        {
            if (!_isArmed || IsExploded) return;
            if (!Player.TryGet(obj, out var player)) return;
            if (!player.IsAlive) return;
            if (player == Owner && Time.time - _spawnTime < _ownerSafeTime) return;

            Explode();
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
