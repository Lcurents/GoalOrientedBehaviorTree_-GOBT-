using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-GetShovelAction")]
    public class GetShovelAction : GoapActionBase<GetShovelAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 1f; // 1 detik untuk ambil sekop
            data.Acquired = false;

            if (ShovelStorage.Instance == null)
            {
                FarmLog.SystemWarn("GetShovelAction: ShovelStorage not found on Storage object");
                return;
            }

            if (!ShovelStorage.Instance.TryReserve(agent.gameObject))
            {
                FarmLog.ActionWarn(agent.gameObject.name, "GetShovel FAILED | No shovel available");
                return;
            }

            data.Acquired = true;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!data.Acquired)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // SUCCESS: Tambah sekop ke inventory
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    if (stats.HasShovel < 2)
                    {
                        stats.HasShovel++;
                        FarmLog.Resource(agent.gameObject.name, $"GetShovel | Shovel={stats.HasShovel}");
                    }
                }

                var carrier = agent.GetComponent<ShovelCarrier>();
                if (carrier != null)
                    carrier.SetHeld(true);
                
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            if (!data.Acquired)
                return;

            var stats = agent.GetComponent<NPCStats>();
            bool hasShovel = stats != null && stats.HasShovel > 0;

            if (!hasShovel && ShovelStorage.Instance != null && ShovelStorage.Instance.IsReservedBy(agent.gameObject))
            {
                ShovelStorage.Instance.Return(agent.gameObject);
                FarmLog.ActionWarn(agent.gameObject.name, "GetShovel INTERRUPTED | Reservation released");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public bool Acquired;
        }
    }
}
