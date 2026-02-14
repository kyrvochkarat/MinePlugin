using Exiled.API.Interfaces;
using System.ComponentModel;

namespace LandminePlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float ArmingTime { get; set; } = 5f;
        public float ExplosionDamage { get; set; } = 250f;
        public float TriggerRadius { get; set; } = 1.0f;
        public float ExplosionRadius { get; set; } = 5f;
        public int MinesPerCaptain { get; set; } = 2;
        public float CheckInterval { get; set; } = 0.25f;
    }
}