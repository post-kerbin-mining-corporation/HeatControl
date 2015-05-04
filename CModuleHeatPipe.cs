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
        public float MaxTransferTempScale = 0.9f;
        //private CModuleLinkedMesh

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
                    double targetHeatContent = targetPart.thermalMass*targetPart.temperature;
                    double parentHeatContent = parentPart.thermalMass * targetPart.temperature;

                    // compute differences between part heat contents, scale by kw to transfer
                    double targetPartFlux = Mathf.Clamp((float)(parentHeatContent - targetHeatContent),-HeatTransferAbility,HeatTransferAbility);
                    double parentPartFlux = Mathf.Clamp((float)(targetHeatContent - parentHeatContent) ,-HeatTransferAbility,HeatTransferAbility);

                    // Add fluxes
                    targetPart.AddThermalFlux(targetPartFlux);
                    parentPart.AddThermalFlux(parentPartFlux);

                    // Max heat the parts can accept
                    //double targetHeatMax = targetPart.thermalMass * targetPart.maxTemp * MaxTransferTempScale;
                    //double parentHeatMax = parentPart.thermalMass * targetPart.maxTemp * MaxTransferTempScale;
                }
                                   
            }
            
           

           
            
        }
    }
}
