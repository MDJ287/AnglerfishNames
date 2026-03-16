using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace AnglerfishNames
{
    public class AnglerfishNames : ModBehaviour
    {
        public static AnglerfishNames Instance;

        private readonly Vector3[] fishLocations = [
            new(715.7f, -86.4f, 4220.8f),
            new(-397.8f, -153.8f, -279.4f),
            new(3587.3f, 267.0f, 2057.2f),
            new(406.4f, -3027.0f, 1979.6f),
            new(25.0f, 2432.1f, 2024.3f),
            new(761.3f, 99.2f, 257.0f),
            new(85.6f, -193.6f, 449.9f),
            new(666.5f, -36.2f, 48.8f),
            new(826.6f, 195.3f, 73.1f),
            new(4406.0f, -411.0f, 1203.4f)
        ];
        private readonly string[] fishNames = [
            "Ernesto Sr",
            "Tootholomew",
            "Pebbles",
            "Nemo",
            "Fishy McFishface",
            "Gillbert",
            "Blindrew",
            "Lightney",
            "Finnegan",
            "Bubbles"
        ];
        private readonly string quantumFishName = "Ernesta";
        GameObject fishEggs;
        CanvasMarker[] markers = new CanvasMarker[10];
        CanvasMarker quantumFishMarker = null;
        FogWarpDetector playerFogWarpDetector;
        bool inSolarSystem = false;

        // settings

        bool isModEnabled = true;
        bool displayThroughSeeds = false;

        //

        public void Awake()
        {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(AnglerfishNames)} is loaded!", MessageType.Success);

            new Harmony("MDJ287.AnglerfishNames").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            markers = new CanvasMarker[10];
            quantumFishMarker = null;
            inSolarSystem = newScene == OWScene.SolarSystem;
        }

        public override void Configure(IModConfig config)
        {
            isModEnabled = config.GetSettingsValue<bool>("enabled");
            displayThroughSeeds = config.GetSettingsValue<bool>("displayThroughSeeds");

            if (markers[0] != null)
            {
                foreach (CanvasMarker marker in markers)
                {
                    marker.SetVisibility(
                        isModEnabled &&
                        (
                            displayThroughSeeds ||
                            playerFogWarpDetector.GetOuterFogWarpVolume() == marker._outerFogWarpVolume
                        )
                    );
                }
            }
            if (quantumFishMarker != null)
            {
                quantumFishMarker.SetVisibility(isModEnabled);
            }
        }

        public void Update()
        {
            if (inSolarSystem)
            {
                if (Locator.GetPlayerDetector() != null)
                {
                    playerFogWarpDetector = Locator.GetPlayerDetector().GetComponent<FogWarpDetector>();
                }
                if (markers[0] == null && Locator.GetMarkerManager() != null)
                {
                    fishEggs = GameObject.Find("FishEggs");
                    AnglerfishController[] fishes = Resources.FindObjectsOfTypeAll<AnglerfishController>();
                    int i = 0;
                    foreach (AnglerfishController fish in fishes)
                    {
                        CanvasMarker marker = Locator.GetMarkerManager().InstantiateNewMarker();
                        Locator.GetMarkerManager().RegisterMarker(marker, fish.transform, fishNames[FishIndex(fish)]);
                        marker.SetOuterFogWarpVolume(fish._brambleBody.GetComponentInChildren<OuterFogWarpVolume>());
                        marker.SetVisibility(isModEnabled && displayThroughSeeds);
                        markers[i++] = marker;
                    }
                }
                if (markers[0] != null && isModEnabled && !displayThroughSeeds)
                {
                    foreach (CanvasMarker marker in markers)
                    {
                        marker.SetVisibility(playerFogWarpDetector.GetOuterFogWarpVolume() == marker._outerFogWarpVolume);
                    }
                }
            }
            if (PlayerState.IsInsideTheEye())
            {
                VisibilityObject quantumAnglerfish = FindObjectOfType<QuantumAnglerfish>()?.GetComponentInChildren<VisibilityObject>();
                if (quantumFishMarker == null && quantumAnglerfish != null)
                {
                    quantumFishMarker = Locator.GetMarkerManager().InstantiateNewMarker();
                    Locator.GetMarkerManager().RegisterMarker(quantumFishMarker, quantumAnglerfish.transform, quantumFishName);
                    quantumFishMarker.SetVisibility(isModEnabled);
                }
                if (quantumFishMarker != null && quantumAnglerfish == null)
                {
                    Locator.GetMarkerManager().UnregisterMarker(quantumFishMarker);
                    quantumFishMarker = null;
                }
            }
        }

        private int FishIndex(AnglerfishController fish)
        {
            int i = 0;
            foreach (Vector3 location in fishLocations)
            {
                if (Vector3.Distance(location, fish.transform.position - fishEggs.transform.position) < 1)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }

}
