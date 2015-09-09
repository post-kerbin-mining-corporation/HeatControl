using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{
    public class ModuleActiveRadiatorInfo:PartModule
    {

        // Emissive Constant Extended
        [KSPField(isPersistant = false)]
        public float NameplateRadiationExtended = 0f;

        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float NameplateRadiation = 0f;

        // Emissive Constant Closed
        [KSPField(isPersistant = false)]
        public float RadiatorRating = 1f;

        [KSPField(isPersistant = false)]
        public bool HasExtendedState = true;

        // UI
        // -----------

        // Heat Emission UI note
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Radiation")]
        public string HeatRejectionGUI = "0 kW";

        // Heat Emission UI note
        [KSPField(isPersistant = false, guiActive = false, guiName = "Radiative Area")]
        public string RadiativeAreaGUI = "";

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiator Temperature")]
        public string RadiatorTemp;

        public override string GetInfo()
        {
            string info = "";

            if (!HasExtendedState)
            {

                info += String.Format("Heat Removed: {0:F1} kW", NameplateRadiation);
            }
            else
            {

                info += String.Format("Approx. Radiation (closed): {0:F1} kW", NameplateRadiation) + "\n" +
                    String.Format("Approx. Radiation (deployed): {0:F1} kW", NameplateRadiationExtended);

                //info += String.Format("Heat Radiated (Retracted): {0:F1} kW", HeatRadiated) + "\n" +
                //    String.Format("Heat Radiated (Deployed): {0:F1} kW", HeatRadiatedExtended);
            }

            return info;

        }
        public void Update()
        {
            if (part)
            {
                RadiativeAreaGUI = part.radiativeArea.ToString();
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                RadiatorTemp = String.Format("{0:F1} K", part.skinTemperature);

                // Update the UI widget
                HeatRejectionGUI = String.Format("{0:F1} kW", -part.thermalRadiationFlux);
            }
        }

    }
}
