using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{

    // A simplistic heat radiator
    // Can represent a deployable radiator
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

        // Emissive Constant Extended
        [KSPField(isPersistant = false)]
        public float RadiationExtended;

        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float Radiation;

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

        // Allow or disallow sun tracking (cosmetic only for now)
        [KSPField(guiName = "Status", guiActiveEditor = true, guiActive= true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool RadiatorActive = true;

        // Start deployed or closed
       // [KSPField(guiName = "Start", guiActiveEditor = false, isPersistant = true)]
        //[UI_Toggle(disabledText = "Closed", enabledText = "Open")]
        //public bool StartDeployed = false;

        // Allow or disallow sun tracking (cosmetic only for now)
        [KSPField(guiName = "Rotation", guiActiveEditor = true, isPersistant = true)]
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
              
                info += String.Format("Heat Removed: {0:F1} kW", Radiation);
            }
            else
            {
               
                info += String.Format("Heat Removed (closed): {0:F1} kW", Radiation) + "\n" +
                    String.Format("Heat Removed (deployed): {0:F1} kW", RadiationExtended);

                //info += String.Format("Heat Radiated (Retracted): {0:F1} kW", HeatRadiated) + "\n" +
                //    String.Format("Heat Radiated (Deployed): {0:F1} kW", HeatRadiatedExtended);
            }

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

            if (!TrackSun)
                base.trackingSpeed = 0f;

            
        }

        public override void OnUpdate()
        {
            base.chargeRate = 0f;
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
                    if (fld.guiName == "Tracking" && base.animationName == "")
                    {
                        fld.guiActive = false;
                    }
                }
            } else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                foreach (BaseField fld in base.Fields)
                {
                    if (fld.guiName == "Tracking" && base.animationName == "")
                    {
                        fld.guiActiveEditor = false;
                        fld.guiActive = false;
                    }
                }
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
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                    part.heatConductivity = 0.001f;
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
            if (base.animationName != "")
            {
                
                if (base.panelState != ModuleDeployableSolarPanel.panelStates.EXTENDED && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
                {
                    // Utils.Log("Closed! " + HeatRadiated.ToString());
                    heatRemoved += Radiation;
                    
                }
                else if (base.panelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                {
                    // Utils.Log("Broken!! " + 0.ToString());
                    heatRemoved += 0;
                   
                }
                else
                {
                    heatRemoved += RadiationExtended;
                    // Utils.Log("Open! " + HeatRadiatedExtended.ToString());
                }

                
            }
            // always radiate
            else
            {
                heatRemoved += Radiation;
                
            }
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
