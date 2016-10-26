using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{

    // A simplistic heat radiator
    // Can represent a simple radiator or a deplyed radiator

    public class ModuleStaticRadiator : PartModule
    {
        // KSPFields
        // --------

        // GAMEPLAY

   
        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float Emissive;

        // Part area closed
        [KSPField(isPersistant = false)]
        public float Area;

        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float Radiation;


        // Resource use when closed
        [KSPField(isPersistant = false)]
        public float ResourceUse = 0f;

        // Name of the resource to use
        [KSPField(isPersistant = false)]
        public string ResourceName = "";

        // ANIMATION

        // Allow or disallow sun tracking (cosmetic only for now)
        [KSPField(guiName = "Status", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool RadiatorActive = true;

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

        // Heat Emission UI note
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Thermal Radiation")]
        public string HeatRejectionGUI = "0 kW";

        // Heat Input UI note
        [KSPField(isPersistant = false, guiActive = false, guiName = "Current Heat Input")]
        public string HeatInputGUI = "0 kW";

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiator Temperature")]
        public string RadiatorTemp;

        [KSPEvent(guiActive = true, guiName = "Enable Radiator", active = true)]
        public void Enable()
        {
            RadiatorActive = true;
        }
        [KSPEvent(guiActive = true, guiName = "Disable Radiator", active = false)]
        public void Disable()
        {

            RadiatorActive = false;
        }

        // Actions
        [KSPAction("Toggle Radiator")]
        public void ToggleHeatRejectionAction(KSPActionParam param)
        {
            RadiatorActive = !RadiatorActive;
        }
        [KSPAction("Activate Radiator")]
        public void ActivateHeatRejectionAction(KSPActionParam param)
        {
            Enable();
        }
        [KSPAction("Deactivate Radiator")]
        public void StopHeatRejectionAction(KSPActionParam param)
        {
            Disable();
        }

    
        // UI shown in VAB/SPH
        public override string GetInfo()
        {
            string info = "";

                info += String.Format("Heat Removed: {0:F1} kW", Radiation);
            
          

            return info;

        }

        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                part.force_activate();
            }
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
        }

        public override void OnUpdate()
        {
            
            base.OnUpdate();
            // Hide all the solar panel fields
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                
            } else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {

            }
            if (Events["Enable"].active == RadiatorActive || Events["Disable"].active != RadiatorActive)
            {
                Events["Disable"].active = RadiatorActive;
                Events["Enable"].active = !RadiatorActive;

            }
        }

      

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            part.heatConductivity = 0.001f;
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                    RadiatorTemp = String.Format("{0:F1} K", part.temperature);
                    // If an animation name is present, assume deployable
                 
                    if (RadiatorActive)
                        DoRadiatorEffectsInternal();
                  
                    // Update the UI widget
                    HeatRejectionGUI = String.Format("{0:F1} kW", -part.thermalRadiationFlux);

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

       

        private void DoRadiatorEffectsInternal()
        {
  

            float heatRemoved = 0f;
            heatRemoved += Radiation;

            if (part.parent != null)
            {
                if (part.parent.temperature >= 300d)
                {
                    if (part.parent.temperature <= 350d)
                    {
                        double delta = (part.parent.temperature - 300d) / 50d;
                        heatRemoved = heatRemoved * (float)delta;
                    }

                    part.parent.AddThermalFlux(-heatRemoved);
                    part.AddThermalFlux(heatRemoved);
                }
               

            }
            // Update the UI widget
            HeatInputGUI = String.Format("{0:F2} kW", heatRemoved);
        }

      

    }

}
