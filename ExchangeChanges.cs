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
        public const string PluginVersion = "1.1.2";

        public static ConfigEntry<float> printerDelay { get; set; }
        public static ConfigEntry<float> scrapperDelay { get; set; }
        public static ConfigEntry<float> chanceDelay { get; set; }
        public static ConfigEntry<float> mountainDelay { get; set; }
        public static ConfigEntry<float> bazaarDelay { get; set; }
        public static ConfigEntry<float> cleanseDelay { get; set; }
        public static ConfigEntry<float> woodsDelay { get; set; }
        public static ConfigEntry<float> bloodDelay { get; set; }

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

            ModSettingsManager.AddOption(new FloatFieldOption(printerDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(scrapperDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(scrapperItemDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(chanceDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(mountainDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(bazaarDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(cleanseDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(woodsDelay));
            ModSettingsManager.AddOption(new FloatFieldOption(bloodDelay));


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

                panel.RevertChanges();
            };
            ModSettingsManager.AddOption(new GenericButtonOption("Reset delay settings to default:", "Delay Settings", "", "RESET", ResetToDefault));

            ModSettingsManager.SetModDescription("Adjust the speed of repeatable interactions");



            On.RoR2.Stage.Start += (orig, self) =>
            {
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("initialDelayDuration", printerDelay.Value / 2.0f);
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", printerDelay.Value / 2.0f);

                typeof(EntityStates.Scrapper.WaitToBeginScrapping).SetFieldValue("duration", scrapperDelay.Value / 2.0f);
                typeof(EntityStates.Scrapper.Scrapping).SetFieldValue("duration", scrapperDelay.Value / 2.0f);
                typeof(EntityStates.Scrapper.ScrappingToIdle).SetFieldValue("duration", scrapperItemDelay.Value * 2.0f);

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
        }
    }
}