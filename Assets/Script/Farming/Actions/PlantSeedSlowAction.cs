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
    /// Plant seed WITHOUT shovel - SLOW (5 seconds)
    /// Requires: Only HasSeed (fallback option)
    /// </summary>
    [GoapId("Farming-PlantSeedSlowAction")]
    public class PlantSeedSlowAction : GoapActionBase<PlantSeedSlowAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 5f; // Slow without shovel
            data.Agent = agent.gameObject; // Store agent reference

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
                    FarmLog.ActionWarn(agent.gameObject.name, $"PlantSlow FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"PlantSlow START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "PlantSlow ABORTED | No plantable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Plant seed (slow)
                data.Crop.Plant();
                
                // Consume seed only
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    FarmLog.Resource(agent.gameObject.name, $"PlantSlow consumed | Seed={stats.HasSeed}");
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
                FarmLog.Action(data.Agent.name, $"PlantSlow COMPLETE | {data.Crop.name} planted (5s) and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantSlow INTERRUPTED | {data.Crop.name} released");
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
