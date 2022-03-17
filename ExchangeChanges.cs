using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

namespace ExchangeChanges
{
    [BepInDependency(R2API.R2API.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]

    public class ExchangeChanges : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "FlyingComputer";
        public const string PluginName = "ExchangeChanges";
        public const string PluginVersion = "1.0.8";

        public static ConfigEntry<float> printerDelay { get; set; }
        public static ConfigEntry<float> scrapperDelay { get; set; }
        public static ConfigEntry<float> chanceDelay { get; set; } 
        public static ConfigEntry<float> bazaarDelay { get; set; }
        public static ConfigEntry<float> cleanseDelay { get; set; }



        public void Awake()
        {
            printerDelay = base.Config.Bind<float>("Delay Changes", "Printer Delay", 0.3f, "Printer delay in seconds");
            scrapperDelay = base.Config.Bind<float>("Delay Changes", "Scrapper Delay", 0.3f, "Scrapper delay in seconds");
            chanceDelay = base.Config.Bind<float>("Delay Changes", "Shrine of Chance Delay", 0.3f, "Shrine of Chance delay in seconds");
            bazaarDelay = base.Config.Bind<float>("Delay Changes", "Bazaar trade Delay", 0.4f, "Bazaar item trade delay in seconds");
            cleanseDelay = base.Config.Bind<float>("Delay Changes", "Cleansing Pool Delay", 0.5f, "Cleansing Pool delay in seconds");

            float p = printerDelay.Value;
            float s = scrapperDelay.Value;
            float ch = chanceDelay.Value;
            float b = bazaarDelay.Value;
            float cl = cleanseDelay.Value;




            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);

                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("initialDelayDuration", p);
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", p);

                typeof(EntityStates.Scrapper.WaitToBeginScrapping).SetFieldValue("duration", s);
                typeof(EntityStates.Scrapper.ScrappingToIdle).SetFieldValue("duration", s);
                typeof(EntityStates.Scrapper.Scrapping).SetFieldValue("duration", s);
            };

            On.EntityStates.Duplicator.Duplicating.DropDroplet += (orig, self) =>
            {
                orig(self);

                if (NetworkServer.active && self.hasDroppedDroplet)
                {
                    self.GetComponent<PurchaseInteraction>().Networkavailable = true;
                }

            };


            On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig, self, interactor) =>
            {
                orig(self, interactor);
                self.refreshTimer = ch;
            };

            On.RoR2.EntityLogic.DelayedEvent.CallDelayed += (orig, self, timer) =>
            {
                if (self.ToString().Contains("Duplicator"))
                {
                    //Nothing
                }
                else
                {
                    orig(self, timer);
                }
            };

            On.RoR2.TimerQueue.CreateTimer += (orig, self, time, action) =>
            {
                if (action.Target.ToString().Contains("LunarCauldron"))
                {
                    return orig(self, b, action);
                }
                if (action.Target.ToString().Contains("ShrineCleanse"))
                {
                    return orig(self, cl, action);
                }
                return orig(self, time, action);
            };
        }
    }
}