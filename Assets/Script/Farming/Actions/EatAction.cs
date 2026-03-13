using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-EatAction")]
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        public override void Created()
        {
        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var agentObject = agent.Transform.gameObject;
            var stats = data.Stats ?? agentObject.GetComponent<NPCStats>();
            if (stats == null)
                return false;

            return NPCStats.HasFoodReservation(agentObject) || NPCStats.CanReserveFood(agentObject);
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
            data.AteFood = false;

            if (!NPCStats.HasFoodReservation(agent.gameObject))
            {
                if (!NPCStats.TryReserveFood(agent.gameObject))
                {
                    FarmLog.ActionWarn(agent.gameObject.name, "Eat ABORTED | No food reservation available");
                }
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            // Simulate eating time (1 detik)
            if (data.Timer < 1f)
                return ActionRunState.Continue;

            // Eat food
            var stats = data.Stats;
            if (stats != null && NPCStats.TryConsumeReservedFood(agent.gameObject))
            {
                stats.DecreaseHunger(50);
                data.AteFood = true;
                FarmLog.Action(agent.gameObject.name, $"Eat COMPLETE | Hunger={stats.Hunger:F0}, SharedFood={stats.FoodCount}");
                return ActionRunState.Completed;
            }

            return ActionRunState.Stop;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            if (!data.AteFood)
                NPCStats.ReleaseFoodReservation(agent.gameObject);
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            if (!data.AteFood)
                NPCStats.ReleaseFoodReservation(agent.gameObject);
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            [GetComponent]
            public NPCStats Stats { get; set; }
            
            public float Timer { get; set; }
            public bool AteFood { get; set; }
        }
    }
}
