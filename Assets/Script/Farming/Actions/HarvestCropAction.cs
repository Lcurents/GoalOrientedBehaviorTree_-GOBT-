using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-HarvestCropAction")]
    public class HarvestCropAction : GoapActionBase<HarvestCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 4f; // 4 detik untuk panen
            data.Agent = agent.gameObject; // Store agent reference

            bool usedFallback;
            data.Crop = CropTargeting.ResolveCropTarget(
                agent,
                data.Target,
                crop => crop.GrowthStage == 3,
                "Harvesting",
                out usedFallback);

            if (data.Crop != null)
            {
                if (usedFallback)
                    FarmLog.ActionWarn(agent.gameObject.name, $"HarvestCrop FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"HarvestCrop START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "HarvestCrop ABORTED | No harvestable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Harvest the crop
                data.Crop.Harvest();
                
                // Add food
                var stats = data.Stats;
                if (stats != null)
                {
                    stats.AddFood(1);
                    FarmLog.Resource(agent.gameObject.name, $"+1 Food | SharedTotal={stats.FoodCount}");
                }

                data.ActionCompleted = true; // Mark as successfully completed
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
                FarmLog.Action(data.Agent.name, $"HarvestCrop COMPLETE | {data.Crop.name} harvested and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, End() already released the crop
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"HarvestCrop INTERRUPTED | {data.Crop.name} released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference
            
            [GetComponent]
            public NPCStats Stats { get; set; }
            
            public float Timer { get; set; }
            
            public GameObject Agent { get; set; } // For crop release
            
            public bool ActionCompleted { get; set; } // Track if action completed successfully
        }
    }
}
