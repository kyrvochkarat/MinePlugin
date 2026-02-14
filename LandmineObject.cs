using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Mirror;

namespace LandminePlugin
{
    public class LandmineObject
    {
        public LandmineBehaviour Behaviour { get; private set; }
        public bool IsExploded => Behaviour == null || Behaviour.gameObject == null;

        public LandmineObject(Player owner, Vector3 position)
        {
            var config = LandminePlugin.Instance.Config;

            Primitive primitive = Primitive.Create(
                PrimitiveType.Cylinder,
                position + Vector3.up * 0.05f,
                Vector3.zero,
                new Vector3(0.5f, 0.05f, 0.5f),
                true
            );

            primitive.Color = new Color(0.2f, 0.4f, 0.1f, 1f);
            primitive.Collidable = false;

            GameObject mineObject = new GameObject("Landmine");
            mineObject.transform.position = position;

            Behaviour = mineObject.AddComponent<LandmineBehaviour>();
            Behaviour.Owner = owner;
            Behaviour.Primitive = primitive;
            Behaviour.ArmingTime = config.ArmingTime;
            Behaviour.ExplosionDamage = config.ExplosionDamage;
            Behaviour.ExplosionRadius = config.ExplosionRadius;
            Behaviour.TriggerRadius = config.TriggerRadius;
        }

        public void Destroy()
        {
            if (Behaviour != null)
            {
                Behaviour.ForceDestroy();
            }
        }
    }
}