// Layered animator for heat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeatControl
{
    public class ModuleHeatAnimator: PartModule
    {

        // Animation that plays with increasing heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation = "";
        // Transform for animation mixing
        [KSPField(isPersistant = false)]
        public string HeatTransformName = "";

        private AnimationState[] heatStates;
        private Transform heatTransform;

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

        public override void OnFixedUpdate()
        {

            base.OnFixedUpdate();
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
               if (HeatAnimation != "")
                {
                    float animFraction = (float)(part.skinTemperature / part.maxTemp);
                    foreach (AnimationState state in heatStates)
                    {
                        state.normalizedTime = Mathf.MoveTowards(state.normalizedTime, Mathf.Clamp01(animFraction), 0.1f * TimeWarp.fixedDeltaTime);
                    }
                }

            }
        }


    }
}
