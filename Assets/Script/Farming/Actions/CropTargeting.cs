using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    internal static class CropTargeting
    {
        public static CropBehaviour ResolveCropTarget(
            IMonoAgent agent,
            ITarget target,
            System.Func<CropBehaviour, bool> fallbackPredicate,
            string goalType,
            out bool usedFallback)
        {
            usedFallback = false;

            if (agent == null || target == null)
                return null;

            var colliders = Physics2D.OverlapCircleAll(target.Position, 1f);
            CropBehaviour bestFallback = null;

            foreach (var col in colliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop == null)
                    continue;

                var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                if (reservedAgent == agent.gameObject)
                {
                    return crop;
                }

                if (reservedAgent == null && fallbackPredicate != null && fallbackPredicate(crop))
                {
                    bestFallback = crop;
                }
            }

            if (bestFallback != null && CropManager.Instance != null)
            {
                CropManager.Instance.SubmitBid(bestFallback, agent.gameObject, 1f, goalType);
                if (CropManager.Instance.IsReservedBy(bestFallback, agent.gameObject))
                {
                    usedFallback = true;
                    return bestFallback;
                }
            }

            return null;
        }
    }
}
