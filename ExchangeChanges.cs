using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using R2API.Utils;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.Components;
using RiskOfOptions.Components.Panel;


namespace ExchangeChanges
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]

    public class ExchangeChanges : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "FlyingComputer";
        public const string PluginName = "ExchangeChanges";
        public const string PluginVersion = "1.1.3";

        public static ConfigEntry<float> printerDelay { get; set; }
        public static ConfigEntry<float> scrapperDelay { get; set; }
        public static ConfigEntry<float> chanceDelay { get; set; }
        public static ConfigEntry<float> mountainDelay { get; set; }
        public static ConfigEntry<float> bazaarDelay { get; set; }
        public static ConfigEntry<float> cleanseDelay { get; set; }
        public static ConfigEntry<float> woodsDelay { get; set; }
        public static ConfigEntry<float> bloodDelay { get; set; }
        public static ConfigEntry<float> droneVendorDelay { get; set; }
        public static ConfigEntry<float> droneScrapperStartupDelay { get; set; }
        public static ConfigEntry<float> droneScrapperDelay { get; set; }
        public static ConfigEntry<float> droneCombinerDelay { get; set; }
        public static ConfigEntry<float> chefDelay { get; set; }
        

        private static float baseDroneVendorDelay { get; set; }
        private static float baseDroneVendorSpawnDelay { get; set; }
        private static float baseDroneVendorIdleDelay { get; set; }
        private static float baseDroneVendorTotalDelay { get; set; }

        private static float baseDroneCombinerInitDelay { get; set; }
        private static float baseDroneCombinerAbsorbDelay { get; set; }
        private static float baseDroneCombinerAnimDelay { get; set; }
        private static float baseDroneCombinerTotalDelay { get; set; }

        private static float baseChefWaitDelay { get; set; }
        private static float baseChefCookingDelay { get; set; }
        private static float baseChefIdleDelay { get; set; }
        private static float baseChefTotalDelay { get; set; }

        UnityEngine.Events.UnityAction ResetToDefault;

        public void Awake()
        {
            ConfigEntry<float> printerDelay = base.Config.Bind<float>("Delay Settings", "Printer Delay", 0.6f, "Printer delay in seconds");
            ConfigEntry<float> scrapperDelay = base.Config.Bind<float>("Delay Settings", "Scrapper Delay", 0.3f, "Scrapper startup delay in seconds");
            ConfigEntry<float> scrapperItemDelay = base.Config.Bind<float>("Delay Settings", "Scrapper Item Delay", 0.3f, "Scrapper delay between item drops in seconds");
            ConfigEntry<float> chanceDelay = base.Config.Bind<float>("Delay Settings", "Shrine of Chance Delay", 0.4f, "Shrine of Chance delay in seconds");
            ConfigEntry<float> mountainDelay = base.Config.Bind<float>("Delay Settings", "Shrine of the Mountain Delay", 0.4f, "Shrine of the Mountain delay in seconds, for those with mods enabling more than one use");
            ConfigEntry<float> bazaarDelay = base.Config.Bind<float>("Delay Settings", "Bazaar trade Delay", 0.5f, "Bazaar item trade delay in seconds");
            ConfigEntry<float> cleanseDelay = base.Config.Bind<float>("Delay Settings", "Cleansing Pool Delay", 0.5f, "Cleansing Pool delay in seconds");
            ConfigEntry<float> woodsDelay = base.Config.Bind<float>("Delay Settings", "Shrine of the Woods Delay", 0.5f, "Shrine of the Woods delay in seconds");
            ConfigEntry<float> bloodDelay = base.Config.Bind<float>("Delay Settings", "Shrine of Blood Delay", 0.5f, "Shrine of Blood delay in seconds");
            ConfigEntry<float> droneVendorDelay = base.Config.Bind<float>("Delay Settings", "Drone Vendor Delay", 1.0f, "Drone Vendor delay in seconds");
            ConfigEntry<float> droneScrapperStartupDelay = base.Config.Bind<float>("Delay Settings", "Drone Scrapper Startup Delay", 0.1f, "Drone Scrapper startup delay in seconds");
            ConfigEntry<float> droneScrapperDelay = base.Config.Bind<float>("Delay Settings", "Drone Scrapper Delay", 0.5f, "Drone Scrapper delay in seconds");
            ConfigEntry<float> droneCombinerDelay = base.Config.Bind<float>("Delay Settings", "Drone Combiner Delay", 0.7f, "Drone Combiner Station delay in seconds");
            ConfigEntry<float> chefDelay = base.Config.Bind<float>("Delay Settings", "Wandering Chef Delay", 0.5f, "Wandering Chef delay in seconds");

            ModSettingsManager.AddOption(new FloatFieldOption(printerDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(scrapperDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(scrapperItemDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(chanceDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(mountainDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(bazaarDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(cleanseDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(woodsDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(bloodDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(droneVendorDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(droneScrapperStartupDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(droneScrapperDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(droneCombinerDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(chefDelay));


            ResetToDefault += () =>
            {
                ModOptionPanelController panel = FindObjectOfType<ModOptionPanelController>();
                printerDelay.Value = 0.6f;
                scrapperDelay.Value = 0.3f;
                scrapperItemDelay.Value = 0.3f;
                chanceDelay.Value = 0.4f;
                mountainDelay.Value = 0.4f;
                bazaarDelay.Value = 0.5f;
                cleanseDelay.Value = 0.5f;
                woodsDelay.Value = 0.5f;
                bloodDelay.Value = 0.5f;
                droneVendorDelay.Value = 1.0f;
                droneScrapperStartupDelay.Value = 0.1f;
                droneScrapperDelay.Value = 0.5f;
                droneCombinerDelay.Value = 0.7f;
                chefDelay.Value = 0.5f;

                panel.RevertChanges();
            };
            ModSettingsManager.AddOption(new GenericButtonOption("Reset delay settings to default:", "Delay Settings", "", "RESET", ResetToDefault));

            ModSettingsManager.SetModDescription("Adjust the speed of repeatable interactions");



            On.RoR2.Stage.Start += (orig, self) =>
            {
                //Only runs at start of a run, probably will break if using something like a save mod to leave the run and rejoin
                if (Run.instance.stageClearCount == 0)
                {
                    baseDroneVendorDelay = EntityStates.DroneVendor.DroneVendorDispatchState.duration;
                    baseDroneVendorSpawnDelay = EntityStates.DroneVendor.DroneVendorDispatchState.spawnTime;
                    baseDroneVendorIdleDelay = EntityStates.DroneVendor.DroneVendorAssumeIdle.duration;
                    baseDroneVendorTotalDelay = baseDroneVendorDelay + baseDroneVendorIdleDelay;

                    baseDroneCombinerInitDelay = EntityStates.DroneCombiner.DroneCombinerCombining.initialDelayDuration;
                    baseDroneCombinerAbsorbDelay = EntityStates.DroneCombiner.DroneCombinerCombining.droneAbsorbDelay;
                    baseDroneCombinerAnimDelay = EntityStates.DroneCombiner.DroneCombinerCombining.animDelayDuration;
                    baseDroneCombinerTotalDelay = baseDroneCombinerInitDelay + baseDroneCombinerAbsorbDelay + baseDroneCombinerAnimDelay;

                    baseChefWaitDelay = EntityStates.MealPrep.WaitToBeginCooking.duration;
                    baseChefCookingDelay = EntityStates.MealPrep.Cooking.duration;
                    baseChefIdleDelay = EntityStates.MealPrep.CookingToIdle.duration;
                    baseChefTotalDelay = baseChefWaitDelay + baseChefCookingDelay + baseChefIdleDelay;
                }

                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("initialDelayDuration", printerDelay.Value / 2.0f);
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", printerDelay.Value / 2.0f);

                typeof(EntityStates.Scrapper.WaitToBeginScrapping).SetFieldValue("duration", scrapperDelay.Value / 2.0f);
                typeof(EntityStates.Scrapper.Scrapping).SetFieldValue("duration", scrapperDelay.Value / 2.0f);
                typeof(EntityStates.Scrapper.ScrappingToIdle).SetFieldValue("duration", scrapperItemDelay.Value * 2.0f);

                EntityStates.DroneVendor.DroneVendorDispatchState.duration = (baseDroneVendorDelay / baseDroneVendorTotalDelay) * droneVendorDelay.Value;
                EntityStates.DroneVendor.DroneVendorDispatchState.spawnTime = (baseDroneVendorSpawnDelay / baseDroneVendorTotalDelay) * droneVendorDelay.Value;
                EntityStates.DroneVendor.DroneVendorAssumeIdle.duration = (baseDroneVendorIdleDelay / baseDroneVendorTotalDelay) * droneVendorDelay.Value;

                EntityStates.DroneScrapper.DroneScrapping.duration = droneScrapperDelay.Value;
                EntityStates.DroneScrapper.WaitToBeginScrappingDrone.duration = droneScrapperStartupDelay.Value;

                EntityStates.DroneCombiner.DroneCombinerCombining.initialDelayDuration = (baseDroneCombinerInitDelay / baseDroneCombinerTotalDelay) * droneCombinerDelay.Value;
                EntityStates.DroneCombiner.DroneCombinerCombining.droneAbsorbDelay = (baseDroneCombinerAbsorbDelay / baseDroneCombinerTotalDelay) * droneCombinerDelay.Value;
                EntityStates.DroneCombiner.DroneCombinerCombining.animDelayDuration = (baseDroneCombinerAnimDelay / baseDroneCombinerTotalDelay) * droneCombinerDelay.Value;

                EntityStates.MealPrep.WaitToBeginCooking.duration = (baseChefWaitDelay / baseChefTotalDelay) * chefDelay.Value;
                EntityStates.MealPrep.Cooking.duration = (baseChefCookingDelay / baseChefTotalDelay) * chefDelay.Value;
                EntityStates.MealPrep.CookingToIdle.duration = (baseChefIdleDelay / baseChefTotalDelay) * chefDelay.Value;

                return orig(self);
            };

            On.EntityStates.Duplicator.Duplicating.DropDroplet += (orig, self) =>
            {
                orig(self);

                if (NetworkServer.active && self.hasDroppedDroplet)
                {
                    self.GetComponent<PurchaseInteraction>().Networkavailable = true;
                }
            };

            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, interactor) =>
            {
                orig(self, interactor);
                if(self.displayNameToken == "SHRINE_BOSS_NAME")
                    self.GetComponent<ShrineBossBehavior>().refreshTimer = mountainDelay.Value;

                if(self.displayNameToken == "SHRINE_CHANCE_NAME")
                    self.GetComponent<ShrineChanceBehavior>().refreshTimer = chanceDelay.Value;
            };

            On.RoR2.ShrineHealingBehavior.AddShrineStack += (orig, self, interactor) =>
            {
                orig(self, interactor);
                self.refreshTimer = woodsDelay.Value;
            };

            On.RoR2.ShrineBloodBehavior.AddShrineStack += (orig, self, interactor) =>
            {
                orig(self, interactor);
                self.refreshTimer = bloodDelay.Value;
            };

            //Fixes the printer not dropping items at low timers, don't remember why
            On.RoR2.EntityLogic.DelayedEvent.CallDelayed += (orig, self, timer) =>
            {
                if (!self.ToString().Contains("Duplicator"))
                {
                    orig(self, timer);
                }
            };

            On.RoR2.TimerQueue.CreateTimer += (orig, self, time, action) =>
            {
                if (action.Target.ToString().Contains("LunarCauldron"))
                {
                    return orig(self, bazaarDelay.Value, action);
                }
                if (action.Target.ToString().Contains("ShrineCleanse"))
                {
                    return orig(self, cleanseDelay.Value, action);
                }
                return orig(self, time, action);
            };

            On.EntityStates.DroneVendor.DroneVendorDispatchState.OnEnter += (orig, self) =>
            {
                self.GetModelAnimator().speed = baseDroneVendorTotalDelay / droneVendorDelay.Value;
                orig(self);
            };

            On.EntityStates.DroneVendor.DroneVendorAssumeIdle.OnEnter += (orig, self) =>
            {
                self.GetModelAnimator().speed = baseDroneVendorTotalDelay / droneVendorDelay.Value;
                orig(self);
            };
        }
    }
}