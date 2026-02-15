using Exiled.API.Interfaces;

namespace LandminePlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float ArmingTime { get; set; } = 5f;
        public float ExplosionDamage { get; set; } = 150f;
        public float TriggerRadius { get; set; } = 1.5f;
        public float ExplosionRadius { get; set; } = 5f;
        public int MinesPerCaptain { get; set; } = 2;
        public float DefuseRadius { get; set; } = 3f;
        public int DefusesPerGuard { get; set; } = 2;
    }
}