using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using VRage.Game;

namespace FactionRepSync
{
    public class FactionRepSync : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "FactionRepSyncConfig.cfg";

        private FactionRepSyncControl _control;
        public UserControl GetControl() => _control ?? (_control = new FactionRepSyncControl(this));

        private Persistent<FactionRepSyncConfig> _config;
        public FactionRepSyncConfig Config => _config?.Data;

        private TorchSessionManager _sessionManager;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            SetupConfig();

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;
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
                    InitializePolling();
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

                _config = Persistent<FactionRepSyncConfig>.Load(configFile);

            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data != null) return;
            Log.Info("Create Default Config, because none was found!");

            _config = new Persistent<FactionRepSyncConfig>(configFile, new FactionRepSyncConfig());
            _config.Save();
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

        private void InitializePolling()
        {
            _ = Task.Run(
                async () =>
                {
                    while (Config.EnabledProperty)
                    {
                        Log.Warn("Syncing Reputation...");
                        var factionCollection = MySession.Static.Factions;
                        var playerFactionCollection = factionCollection.Factions.Where(faction => faction.Value.IsEveryoneNpc() == false);
                        Log.Info($"Found {playerFactionCollection.Count()} factions.");
                        foreach (var (_, myFaction) in factionCollection.Factions)
                        {
                            if (myFaction.IsEveryoneNpc()) continue;
                            var factionCollectionWithoutCurrentFaction = factionCollection.Factions.Where(f => f.Value.FactionId != myFaction.FactionId).ToList();
                            foreach (var playerId in myFaction.Members.Select(member => member.Value.PlayerId))
                            {
                                foreach (var (_, targetFaction) in factionCollectionWithoutCurrentFaction)
                                {
                                    var rep = factionCollection.GetRelationBetweenFactions(myFaction.FactionId, targetFaction.FactionId);
                                    factionCollection.SetReputationBetweenPlayerAndFaction(playerId, targetFaction.FactionId, rep.Item2);
                                }
                                Log.Info($"Syncing player {playerId} to {myFaction.Name}'s relations");
                            }
                        }

                        
                        await Task.Delay(TimeSpan.FromSeconds(60));
                    }
                });
            Log.Warn("Successfully Started Faction Polling");
        }
    }
}
