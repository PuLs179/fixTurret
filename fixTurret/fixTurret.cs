using NLog;
using System;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using NLog;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Generic;
using System.Reflection;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Vector2 = VRageMath.Vector2;
using Vector3 = VRageMath.Vector3;
namespace fixTurret
{


    public class fixTurret : TorchPluginBase, IWpfPlugin
    {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "fixTurretConfig.cfg";

        private fixTurretControl _control;
        public UserControl GetControl() => _control ?? (_control = new fixTurretControl(this));

        private Persistent<fixTurretConfig> _config;
        public fixTurretConfig Config => _config?.Data;

        [PatchShim]
        public static class MyLargeTurretBasePatch
        {

            public static readonly Logger Log = LogManager.GetCurrentClassLogger();

            internal static readonly MethodInfo update =
                typeof(MyLargeTurretBase).GetMethod("UpdateAiWeapon", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo updatePatch =
                typeof(MyLargeTurretBasePatch).GetMethod(nameof(UpdateAiWeaponPath), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");

            public static void UpdateAiWeaponPath(MyLargeTurretBase __instance)
            {

                if (__instance is MyLargeTurretBase && __instance.Target != null)
                {
                    __instance.SyncRotationAndOrientation();
                    __instance.RotationAndElevation();

                }


            }

            public static void Patch(PatchContext ctx)
            {
                ctx.GetPattern(update).Prefixes.Add(updatePatch);
                Log.Info("Patching Successful MyLargeTurretBase!");
            }
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            SetupConfig();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            switch (state)
            {

                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    break;
            }
        }

        private void SetupConfig()
        {

            var configFile = Path.Combine(StoragePath, CONFIG_FILE_NAME);

            try
            {

                _config = Persistent<fixTurretConfig>.Load(configFile);

            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data == null)
            {

                Log.Info("Create Default Config, because none was found!");

                _config = new Persistent<fixTurretConfig>(configFile, new fixTurretConfig());
                _config.Save();
            }
        }

        public void Save()
        {
            try
            {
                _config.Save();
                Log.Info("Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration failed to save");
            }
        }
    }
}
