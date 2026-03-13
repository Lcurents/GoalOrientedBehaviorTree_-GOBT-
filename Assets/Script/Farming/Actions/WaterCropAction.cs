using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-WaterCropAction")]
    public class WaterCropAction : GoapActionBase<WaterCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 3f; // 3 detik untuk siram
            data.Agent = agent.gameObject; // Store agent reference

            bool usedFallback;
            data.Crop = CropTargeting.ResolveCropTarget(
                agent,
                data.Target,
                crop => crop.GrowthStage == 1 || crop.GrowthStage == 2,
                "Watering",
                out usedFallback);

            if (data.Crop != null)
            {
                if (usedFallback)
                    FarmLog.ActionWarn(agent.gameObject.name, $"WaterCrop FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"WaterCrop START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "WaterCrop ABORTED | No waterable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                data.Crop.WaterCrop();
                
                // Consume watering can
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasWateringCan--;
                }

                data.ActionCompleted = true;
                FarmLog.Action(agent.gameObject.name, $"WaterCrop PERFORM | {data.Crop.name} watered -> Stage={data.Crop.GrowthStage}");
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Only release on successful completion (ActionCompleted=true)
            // Interrupted case is handled by Stop()
            if (data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"WaterCrop COMPLETE | {data.Crop.name} watered and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, crop should be kept for next farming stage
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"WaterCrop INTERRUPTED | {data.Crop.name} released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference
            
            public float Timer { get; set; }
            
            public GameObject Agent { get; set; } // For crop release
            
            public bool ActionCompleted { get; set; } // Track if action completed successfully
        }
    }
}
