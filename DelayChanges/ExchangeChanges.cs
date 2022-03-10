using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine.Networking;

namespace ExchangeChanges
{
    [BepInDependency(R2API.R2API.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    public class ExchangeChanges : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "FlyingComputer";
        public const string PluginName = "ExchangeChanges";
        public const string PluginVersion = "1.0.2";

        public static ConfigEntry<float> printerDelay { get; set; }
        public static ConfigEntry<float> scrapperDelay { get; set; }
        public static ConfigEntry<float> chanceDelay { get; set; } 
        //public static ConfigEntry<float> bazaarDelay { get; set; }



        public void Awake()
        {
            printerDelay = base.Config.Bind<float>("Delay Changes", "Printer Delay", 0.3f, "Printer delay in seconds");
            scrapperDelay = base.Config.Bind<float>("Delay Changes", "Scrapper Delay", 0.3f, "Scrapper delay in seconds");
            chanceDelay = base.Config.Bind<float>("Delay Changes", "Shrine of Chance Delay", 0.3f, "Shrine of Chance delay in seconds");
            //bazaarDelay = base.Config.Bind<float>("Delay Changes", "Bazaar upgrade Delay", 0.3f, "Bazaar item upgrade delay in seconds");

            float p = printerDelay.Value;
            float s = scrapperDelay.Value;
            float c = chanceDelay.Value;
            //float b = bazaarDelay.Value;

            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);

                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("initialDelayDuration", p);
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", p);

                typeof(EntityStates.Scrapper.WaitToBeginScrapping).SetFieldValue("duration", s);
                typeof(EntityStates.Scrapper.ScrappingToIdle).SetFieldValue("duration", s);
                typeof(EntityStates.Scrapper.Scrapping).SetFieldValue("duration", s);

                //typeof(BazaarUpgradeInteraction).SetFieldValue("activationCooldownDuration", 0f);
            };

            On.EntityStates.Duplicator.Duplicating.DropDroplet += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active)
                {
                    self.outer.GetComponent<PurchaseInteraction>().Networkavailable = true;
                }
            };

            On.EntityStates.Duplicator.Duplicating.BeginCooking += (orig, self) =>
            {
                if (!NetworkServer.active)
                {
                    orig(self);
                }
            };

            On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig, self, interactor) =>
            {
                orig(self, interactor);
                self.refreshTimer = c;
            };

            /*On.RoR2.BazaarUpgradeInteraction.OnEnable += (orig, self) =>
            {
                self.activationTimer = b;
                orig(self);
                

            };*/
        }
    }
}