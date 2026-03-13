using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-PlantCropAction")]
    public class PlantCropAction : GoapActionBase<PlantCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
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
                    FarmLog.ActionWarn(agent.gameObject.name, $"PlantCrop FALLBACK | Claimed free crop {data.Crop.name}");
                else
                    FarmLog.Action(agent.gameObject.name, $"PlantCrop START | Target={data.Crop.name} (reserved, verified)");
            }
            else
            {
                FarmLog.ActionWarn(agent.gameObject.name, "PlantCrop ABORTED | No plantable crop at target");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            if (data.Crop == null)
                return ActionRunState.Stop;

            // Simulate planting time (2 detik)
            if (data.Timer < 2f)
                return ActionRunState.Continue;

            // Set crop stage to planted (1)
            var crop = data.Crop;
            if (crop != null)
            {
                crop.SetGrowthStage(1);
                FarmLog.Action(agent.gameObject.name, $"PlantCrop PERFORM | {crop.name} planted");
            }

            data.ActionCompleted = true; // Mark as successfully completed
            return ActionRunState.Completed;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            if (data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantCrop COMPLETE | {data.Crop.name} planted and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantCrop INTERRUPTED | {data.Crop.name} released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference, not GetComponent
            
            public float Timer { get; set; }
            
            public bool ActionCompleted { get; set; } // Track if action finished successfully
            public GameObject Agent { get; set; } // For crop release
        }
    }
}
