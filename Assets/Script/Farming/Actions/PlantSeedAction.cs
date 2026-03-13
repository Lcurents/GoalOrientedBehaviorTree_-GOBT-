using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-PlantSeedAction")]
    public class PlantSeedAction : GoapActionBase<PlantSeedAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            var stats = agent.GetComponent<NPCStats>();
            
            // Cek apakah punya sekop atau tidak
            if (stats != null && stats.HasShovel > 0)
            {
                data.Timer = 2f; // Dengan sekop: cepat (2 detik)
            }
            else
            {
                data.Timer = 5f; // Tanpa sekop: lambat (5 detik)
            }

            data.Agent = agent.gameObject;

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
                    FarmLog.ActionWarn(agent.gameObject.name, $"PlantSeed FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"PlantSeed START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "PlantSeed ABORTED | No plantable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Tanam bibit
                data.Crop.Plant();
                
                // Consume bibit only (shovel is returned after planting)
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            if (data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantSeed COMPLETE | {data.Crop.name} planted and released");
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
                    FarmLog.Resource(agent.gameObject.name, $"PlantSeed returned shovel | Shovel={stats.HasShovel}");
                }
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantSeed INTERRUPTED | {data.Crop.name} released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
            public bool ActionCompleted; // Track if action finished successfully
            public GameObject Agent; // For crop release
        }
    }
}
