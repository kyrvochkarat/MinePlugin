using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using System;

namespace LandminePlugin
{
    public class LandminePlugin : Plugin<Config>
    {
        public override string Name => "LandminePlugin";
        public override string Author => "vityanvsk";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(8, 0, 0);
        public override PluginPriority Priority => PluginPriority.Medium;

        public static LandminePlugin Instance { get; private set; }
        public LandmineItem MineItem { get; private set; }
        public DefuseItem DefuseItem { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            MineItem = new LandmineItem();
            DefuseItem = new DefuseItem();
            MineItem.Register();
            DefuseItem.Register();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            MineItem.Unregister();
            DefuseItem.Unregister();
            MineItem = null;
            DefuseItem = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}