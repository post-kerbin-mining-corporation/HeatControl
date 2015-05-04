using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{

    // A simplistic heat radiator
    // Can represent a simple radiator or a deplyed radiator
    // Hijacks ModuleDeplyableSolarPanel to do sun-parallel rotation

    public class ModuleGenericRadiator : ModuleDeployableSolarPanel
    {
        // KSPFields
        // --------

        // GAMEPLAY

        // Emissive Constant Extended
        [KSPField(isPersistant = false)]
        public float EmissiveExtended;

        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float Emissive;

        // Part area extended
        [KSPField(isPersistant = false)]
        public float AreaExtended;

        // Part area closed
        [KSPField(isPersistant = false)]
        public float Area;

        // Resource use when extended
        [KSPField(isPersistant = false)]
        public float ResourceUseExtended = 0f;

        // Resource use when closed
        [KSPField(isPersistant = false)]
        public float ResourceUse = 0f;

        // Name of the resource to use
        [KSPField(isPersistant = false)]
        public string ResourceName = "";

        // ANIMATION

        // Start deployed or closed
       // [KSPField(guiName = "Start", guiActiveEditor = false, isPersistant = true)]
        //[UI_Toggle(disabledText = "Closed", enabledText = "Open")]
        //public bool StartDeployed = false;

        // Allow or disallow sun tracking (cosmetic only for now)
        [KSPField(guiName = "Tracking", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool TrackSun = true;

        // Animation that plays with increasing heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation = "";
        // Transform for animation mixing
        [KSPField(isPersistant = false)]
        public string HeatTransformName = "";

        private AnimationState[] heatStates;
        private Transform heatTransform;
        

        // Actions and UI
        // --------------

        // Heat Rejection UI note
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Heat Rejection")]
        public string HeatRejectionGUI = "0 kW";

        // Toggle radiator
        public void Toggle()
        {
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                base.Retract();
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                base.Extend();
        }

        // UI shown in VAB/SPH
        public override string GetInfo()
        {
            string info = "";

            if (!base.isBreakable)
            {
                float heatRadiated = Area*Emissive * Mathf.Pow((float)part.maxTemp*0.75f, 4) * (float)PhysicsGlobals.StefanBoltzmanConstant * 0.001f;
                info += String.Format("Est. radiation at max temp: {0:F1} kW", heatRadiated);
            }
            else
            {
                float heatRadiated = Area * Emissive * Mathf.Pow((float)part.maxTemp * 0.75f, 4) * (float)PhysicsGlobals.StefanBoltzmanConstant * 0.001f;
                float heatRadiatedOpen = AreaExtended * EmissiveExtended * Mathf.Pow((float)part.maxTemp * 0.75f, 4) * (float)PhysicsGlobals.StefanBoltzmanConstant * 0.001f;
                info += String.Format("Est. radiation at max temp (closed): {0:F1} kW", heatRadiated) + "\n" +
                    String.Format("Est. radiation at max temp (deployed): {0:F1} kW", heatRadiatedOpen);

                //info += String.Format("Heat Radiated (Retracted): {0:F1} kW", HeatRadiated) + "\n" +
                //    String.Format("Heat Radiated (Deployed): {0:F1} kW", HeatRadiatedExtended);
            }

            return info;

        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            // get the animation state for panel deployment
            //deployStates = Utils.SetUpAnimation(base.animationName, part);

            // Set up heat animation
            if (HeatTransformName != "" && HeatAnimation != "")
            {
                heatStates = Utils.SetUpAnimation(HeatAnimation, part);
                heatTransform = part.FindModelTransform(HeatTransformName);

                foreach (AnimationState heatState in heatStates)
                {
                    heatState.AddMixingTransform(heatTransform);
                    heatState.blendMode = AnimationBlendMode.Blend;
                    heatState.layer = 15;
                    heatState.weight = 1.0f;
                    heatState.enabled = true;
                }
            }

            if (!TrackSun)
                base.trackingSpeed = 0f;

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                part.force_activate();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            // Hide all the solar panel fields
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                foreach (BaseField fld in base.Fields)
                {
                    if (fld.guiName == "Sun Exposure")
                        fld.guiActive = false;
                    if (fld.guiName == "Energy Flow")
                        fld.guiActive = false;
                    if (fld.guiName == "Status")
                        fld.guiActive = false;
                }
            }
        }

      

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
               
                    // If an animation name is present, assume deployable
                    if (base.animationName != "")
                    {
                        if (base.panelState != ModuleDeployableSolarPanel.panelStates.EXTENDED && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
                        {
                            // Utils.Log("Closed! " + HeatRadiated.ToString());
                            part.emissiveConstant = Emissive;
                        }
                        else if (base.panelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                        {
                            // Utils.Log("Broken!! " + 0.ToString());
                            part.emissiveConstant = Emissive;
                        }
                        else
                        {

                            // Utils.Log("Open! " + HeatRadiatedExtended.ToString());
                            part.emissiveConstant = EmissiveExtended;
                        }
                    }
                    // always radiate
                    else
                    {
                        part.emissiveConstant = Emissive;
                    }




                    // Update the UI widget
                    HeatRejectionGUI = String.Format("{0:F3} kW", -part.thermalRadiationFlux);

                    if (HeatAnimation != "")
                    {
                        float animFraction = (float) (part.temperature / part.maxTemp);
                        foreach (AnimationState state in heatStates)
                        {
                            state.normalizedTime = Mathf.MoveTowards(state.normalizedTime, Mathf.Clamp01(animFraction), 0.1f * TimeWarp.fixedDeltaTime);
                        }
                    }

                }
            

        }

      

    }

}
