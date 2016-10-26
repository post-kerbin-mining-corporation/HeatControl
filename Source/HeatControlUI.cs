using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{
  
    [KSPAddon(KSPAddon.Startup.SpaceCentre,true)] 
    public class HeatControlUI:MonoBehaviour
    {

        public ApplicationLauncherButton appButton = null;
        public bool UIShown = false;

        // App launcher stuff
        private void Awake()
        {
           
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnAppLauncherDestroyed);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
        }

        private void OnAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && appButton == null)
            {
             appButton = ApplicationLauncher.Instance.AddModApplication(OnToggleOn, OnToggleOff, OnHover, OnHoverOut, OnEnable, OnDisable, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                 (Texture)GameDatabase.Instance.GetTexture("HeatControl/Icons/toolbarIcon",false));
            }
        }
        private void OnAppLauncherDestroyed()
        {
            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveApplication(appButton);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnAppLauncherDestroyed);
            }
        }

        private void OnGameSceneLoadRequested(GameScenes scene)
        {
            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveApplication(appButton);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnAppLauncherDestroyed);
            }
        }

        private void OnToggleOn()
        {
            Debug.Log("On");
            UIShown = true;
        }
        private void OnToggleOff()
        {
            Debug.Log("Off");
            UIShown = false;
        }
        private void OnHover()
        {
        }
        private void OnHoverOut()
        {
        }
        private void OnEnable()
        {
         
        }
        private void OnDisable()
        {
         
        }


         // GUI VARS
        // ---------- 
        public Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        public Vector2 scrollPosition = Vector2.zero;
        
        int windowID = new System.Random(32345).Next();
        bool initStyles = false;

        // styles
        GUIStyle progressBarBG;

        GUIStyle gui_bg;
        GUIStyle gui_text;
        GUIStyle gui_header;
        GUIStyle gui_window;

        GUIStyle gui_btn_shutdown;
        GUIStyle gui_btn_start;
       
        private void InitStyles()
        {
            gui_window = new GUIStyle(HighLogic.Skin.window);
            gui_header = new GUIStyle(HighLogic.Skin.label);
            gui_header.fontStyle = FontStyle.Bold;
            gui_header.alignment = TextAnchor.UpperLeft;
            gui_header.stretchWidth = true;

            gui_text = new GUIStyle(HighLogic.Skin.label);
            gui_text.fontSize = 11;
            gui_text.alignment = TextAnchor.MiddleLeft;
            gui_bg = new GUIStyle(HighLogic.Skin.textArea);
            gui_bg.active = gui_bg.hover = gui_bg.normal;

            gui_btn_shutdown = new GUIStyle(HighLogic.Skin.button);
            gui_btn_shutdown.wordWrap = true;
            gui_btn_shutdown.normal.textColor = XKCDColors.RedOrange;
            gui_btn_shutdown.alignment = TextAnchor.MiddleCenter;

            gui_btn_start = new GUIStyle(gui_btn_shutdown);
            gui_btn_start.normal.textColor = XKCDColors.Green;

            progressBarBG = new GUIStyle(HighLogic.Skin.textField);
            progressBarBG.active = progressBarBG.hover = progressBarBG.normal;

            windowPos = new Rect(200f, 200f, 550f, 315f);

            initStyles = true;
        }

        // Actual GUI
        public void OnGUI()
        {
         
            if (!initStyles)
                InitStyles();

            Debug.Log(UIShown);

            if (UIShown && HighLogic.LoadedSceneIsEditor)
            {
                
                GUI.skin = HighLogic.Skin;
                gui_window.padding.top = 5;

                windowPos = GUI.Window(windowID, windowPos, DrawWindow, new GUIContent(), gui_window);
                
            }
        }

        private void DrawWindow(int windowID)
        {
            GUI.skin = HighLogic.Skin;
            Debug.Log(windowPos);
            Debug.Log("Drawing WIndow");
            GUILayout.BeginVertical();
                GUILayout.Label("Heat", gui_header, GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f));

                GUILayout.Label(String.Format("Maximum Engine Heat: {0:F2}/s", engineKw),
                    gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
                GUILayout.Label(String.Format("Maximum Harvester Heat: {0:F2}/s", harvestKw),
                        gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
                GUILayout.Label(String.Format("Maximum Converter Heat: {0:F2}/s", converterKw),
                        gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
                GUILayout.Label(String.Format("Maximum Reactor Heat: {0:F2}/s", reactorKw),
                        gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));

                GUILayout.Label(String.Format("Combined Radiator rating: {0:F1}", radiatorRating),
                           gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));

            GUILayout.EndVertical();
        }

        private List<Part> shipParts = new List<Part>();

        private void OnFixedUpdate()
        {
            if (UIShown && HighLogic.LoadedSceneIsEditor)
            {
                if (EditorLogic.fetch.ship.parts.Count != shipParts.Count)
                {
                    shipParts = EditorLogic.fetch.ship.parts;
                    Recalculate();
                }
            }
        }

        float engineKw = 0f;
        float harvestKw = 0f;
        float converterKw = 0f;
        float reactorKw = 0f;

        float radiatorRating = 0f;

        private void Recalculate()
        {
            engineKw = 0f;
            harvestKw = 0f;
            converterKw = 0f;
            reactorKw = 0f;

            foreach (Part p in shipParts)
            {
                if (p.GetComponent<ModuleEngines>())
                    engineKw += GetEngineHeat(p.GetComponent<ModuleEngines>());
                if (p.GetComponent<ModuleEnginesFX>())
                    engineKw += GetEngineHeat(p.GetComponent<ModuleEnginesFX>());
                if (p.GetComponent<ModuleResourceHarvester>())
                    harvestKw += GetHarvesterHeat(p.GetComponent<ModuleResourceHarvester>());
                if (p.GetComponent<ModuleResourceConverter>())
                    converterKw += GetConverterHeat(p.GetComponent<ModuleResourceConverter>());
                if (p.GetComponent<ModuleActiveRadiatorInfo>())
                    radiatorRating += GetRadiatorRating(p.GetComponent<ModuleActiveRadiatorInfo>());

            }
        
        }

        

        // Various part heat productions
        public float GetEngineHeat(ModuleEngines engine)
        {
            return (float)(engine.part.thermalMass + engine.part.skinThermalMass) * engine.heatProduction;
        }
        public float GetEngineHeat(ModuleEnginesFX engine)
        {
            return (float)(engine.part.thermalMass + engine.part.skinThermalMass) * engine.heatProduction;
        }
        public float GetConverterHeat(ModuleResourceConverter converter)
        {
            return converter.TemperatureModifier;
        }
        public float GetHarvesterHeat(ModuleResourceHarvester harverster)
        {
            return harverster.TemperatureModifier;
        }

        public float GetRadiatorRating(ModuleActiveRadiatorInfo radiator)
        {
            return radiator.RadiatorRating;
            
        }


    }
}
