using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    /// <summary>
    /// Plant seed WITH shovel - FAST (2 seconds)
    /// Requires: HasSeed + HasShovel
    /// </summary>
    [GoapId("Farming-PlantSeedFastAction")]
    public class PlantSeedFastAction : GoapActionBase<PlantSeedFastAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 2f; // Fast with shovel
            data.Agent = agent.gameObject; // Store agent reference for cleanup

            bool usedFallback;
            data.Crop = CropTargeting.ResolveCropTarget(
                agent,
                data.Target,
                crop => crop.GrowthStage == 0,
                "Planting",
                out usedFallback);

            if (data.Crop != null)
            {
                if (usedFallback)
                    FarmLog.ActionWarn(agent.gameObject.name, $"PlantFast FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"PlantFast START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "PlantFast ABORTED | No plantable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Plant seed
                data.Crop.Plant();
                
                // Consume seed only (shovel is returned after planting)
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    FarmLog.Resource(agent.gameObject.name, $"PlantFast consumed | Seed={stats.HasSeed}");
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
                FarmLog.Action(data.Agent.name, $"PlantFast COMPLETE | {data.Crop.name} planted (2s) and released");
            }

            if (data.ActionCompleted)
            {
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null && stats.HasShovel > 0 && ShovelStorage.Instance != null && ShovelStorage.Instance.IsReservedBy(agent.gameObject))
                {
                    stats.HasShovel = Mathf.Max(0, stats.HasShovel - 1);
                    ShovelStorage.Instance.Return(agent.gameObject);
                    var carrier = agent.GetComponent<ShovelCarrier>();
                    if (carrier != null)
                        carrier.SetHeld(false);
                    FarmLog.Resource(agent.gameObject.name, $"PlantFast returned shovel | Shovel={stats.HasShovel}");
                }
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, crop should be kept for next farming stage
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantFast INTERRUPTED | {data.Crop.name} released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
            public GameObject Agent; // For crop release
            public bool ActionCompleted; // Track if action finished successfully
        }
    }
}
