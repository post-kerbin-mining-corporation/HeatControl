using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{
    public class CModuleHeatPipe:PartModule
    {
        // Heat transferred per second
        [KSPField(isPersistant = false)]
        public float HeatTransferAbility = 10f;

        [KSPField(isPersistant = false)]
        public float MaxTransferTempScale = 1.05f;
        //private CModuleLinkedMesh

        // heat transfter cost, per s per kW
        [KSPField(isPersistant = false)]
        public float HeatTransferResourceCost = 0.5f;

        [KSPField(isPersistant = false)]
        public string ResourceName = "ElectricCharge";

        // Current reactor power setting (0-100, tweakable)
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Transfer Rate"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float CurrentTransferPercent = 100f;

        // Allow or disallow heat pump\
        // VAB
        [KSPField(guiName = "Heat Pump", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool PumpHeat = true;
        // Flight
        [KSPEvent(guiActive = true, guiName = "Enable Heat Pump", active = true)]
        public void Enable()
        {
            PumpHeat = true;
        }
        [KSPEvent(guiActive = true, guiName = "Disable Heat Pump", active = false)]
        public void Disable()
        {

            PumpHeat = false;
        }

        // Heat transfer UI note
        [KSPField(isPersistant = false, guiActive = true, guiName = "Transfer Rare")]
        public string HeatPumpGUI = "0 kW";

        // Actions
        [KSPAction("Toggle Heat Pump")]
        public void ToggleHeatTransferAction(KSPActionParam param)
        {
            PumpHeat = !PumpHeat;
        }
        [KSPAction("Activate Heat Pump")]
        public void ActivateHeatTransferAction(KSPActionParam param)
        {
            Enable();
        }
        [KSPAction("Deactivate Heat Pump")]
        public void StopHeatTransferAction(KSPActionParam param)
        {
            Disable();
        }

        private CompoundPart cPart;
        private Part targetPart;
        private Part parentPart;

        public override void OnStart(PartModule.StartState state)
        {
            
            if (state != StartState.Editor)
            {
                cPart = GetComponent<CompoundPart>();
                
            }
        }
        public override string GetInfo()
        {
            return String.Format("Heat Transfer Rate: {0:F1} kW", HeatTransferAbility);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Events["Enable"].active == PumpHeat || Events["Disable"].active != PumpHeat)
            {
                Events["Disable"].active = PumpHeat;
                Events["Enable"].active = !PumpHeat;

            }
        }    
        void FixedUpdate()
        {
            
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && cPart != null)
            {
                targetPart = cPart.target;
                parentPart = cPart.parent;
                if (targetPart != null && parentPart != null)
                {
                    //Utils.Log("target part= " + cPart.target.gameObject.name);
                    //Utils.Log("parent part= " + cPart.parent.gameObject.name);

                    // current part heat contents
                    double targetHeatContent = targetPart.thermalMass * targetPart.temperature;
                    double parentHeatContent = parentPart.thermalMass * targetPart.temperature;

                    // compute differences between part heat contents, scale by kw to transfer
                    //double targetPartFlux = Mathf.Clamp((float)(parentHeatContent - targetHeatContent),-HeatTransferAbility,HeatTransferAbility);
                    //double parentPartFlux = Mathf.Clamp((float)(targetHeatContent - parentHeatContent) ,-HeatTransferAbility,HeatTransferAbility);



                    double targetPartFlux = 0d;
                    double parentPartFlux = 0d;

                    if (PumpHeat)
                    {
                        if (parentPart.temperature > 273d)
                        {
                            if (targetPart.temperature <= parentPart.temperature * MaxTransferTempScale)
                            {
                                targetPartFlux = HeatTransferAbility * CurrentTransferPercent / 100f;
                                parentPartFlux = -HeatTransferAbility * CurrentTransferPercent / 100f;
                            }
                        }

                        // Add fluxes
                        targetPart.AddThermalFlux(targetPartFlux);
                        parentPart.AddThermalFlux(parentPartFlux);
                        HeatPumpGUI = String.Format("{0:F2} kW", targetPartFlux);
                    }
                    else
                    {
                        HeatPumpGUI = "Disabled";
                    }
                    // Max heat the parts can accept
                    //double targetHeatMax = targetPart.thermalMass * targetPart.maxTemp * MaxTransferTempScale;
                    //double parentHeatMax = parentPart.thermalMass * targetPart.maxTemp * MaxTransferTempScale;
                }
                else
                {
                    HeatPumpGUI = "Not Connected";
                }
                                   
            }
            
           

           
            
        }
    }
}
