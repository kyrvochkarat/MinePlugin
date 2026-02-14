using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Toys;

namespace LandminePlugin
{
    public class LandmineObject
    {
        public LandmineBehaviour Behaviour { get; private set; }
        public bool IsExploded => Behaviour == null || Behaviour.gameObject == null || Behaviour.IsExploded;

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
            primitive.Collidable = true;

            GameObject primitiveGo = primitive.Base.gameObject;

            Rigidbody rb = primitiveGo.GetComponent<Rigidbody>();
            if (rb == null)
                rb = primitiveGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            BoxCollider col = primitiveGo.GetComponent<BoxCollider>();
            if (col == null)
                col = primitiveGo.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.size = new Vector3(config.TriggerRadius * 2f, 1f, config.TriggerRadius * 2f);
            col.center = Vector3.zero;

            SphereCollider trigger = primitiveGo.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = config.TriggerRadius;
            trigger.center = Vector3.zero;

            Behaviour = primitiveGo.AddComponent<LandmineBehaviour>();
            Behaviour.Owner = owner;
            Behaviour.Primitive = primitive;
            Behaviour.ArmingTime = config.ArmingTime;
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