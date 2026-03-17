using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Actions;
using FarmingGoap.Goals;
using FarmingGoap.Sensors.Target;
using FarmingGoap.Sensors.World;
using FarmingGoap.TargetKeys;
using FarmingGoap.WorldKeys;

namespace FarmingGoap.Capabilities
{
    public class FarmingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("FarmingCapability");

            // ========== GOALS - REDESIGNED ==========
            
            // GOAL: EatGoal - Priority 1 (Highest)
            builder.AddGoal<EatGoal>()
                .AddCondition<HungerLevel>(Comparison.SmallerThanOrEqual, 30)
                .AddCondition<HasFood>(Comparison.GreaterThanOrEqual, 1) // PENTING: Harus ada food
                .SetBaseCost(1);

            // GOAL: SleepGoal - Priority 2
            builder.AddGoal<SleepGoal>()
                .AddCondition<EnergyLevel>(Comparison.GreaterThanOrEqual, 80)
                .SetBaseCost(1);

            // GOAL: HarvestingGoal - Priority 3
            // Kondisi: CropGrowthStage <= 2, artinya crop BUKAN di stage 3 (matang)
            // Setelah Harvest(), crop kembali ke stage 0, sehingga kondisi ini terpenuhi
            builder.AddGoal<HarvestingGoal>()
                .AddCondition<CropGrowthStage>(Comparison.SmallerThanOrEqual, 2)
                .SetBaseCost(2);

            // GOAL: WateringGoal - Priority 4
            builder.AddGoal<WateringGoal>()
                .AddCondition<CropNeedsWater>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(3);

            // GOAL: PlantingGoal - Priority 5
            builder.AddGoal<PlantingGoal>()
                .AddCondition<CropGrowthStage>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(3);

            // ========== ACTIONS - REDESIGNED ==========

            // ACTION: GetSeedAction
            builder.AddAction<GetSeedAction>()
                .AddEffect<HasSeedKey>(EffectType.Increase)
                .SetTarget<SeedStorageTargetKey>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: GetShovelAction (Optional optimization)
            // Cost 1 agar GOAP hanya ambil sekop jika memang dibutuhkan dalam chain
            builder.AddAction<GetShovelAction>()
                .AddCondition<ShovelAvailable>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<HasShovelKey>(EffectType.Increase)
                .SetTarget<ShovelStorageTargetKey>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: PlantSeedFastAction (WITH shovel - preferred)
            builder.AddAction<PlantSeedFastAction>()
                .AddCondition<CropGrowthStage>(Comparison.SmallerThanOrEqual, 0) // Tanah kosong
                .AddCondition<HasSeedKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya bibit
                .AddCondition<HasShovelKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya sekop ⭐
                .AddEffect<CropGrowthStage>(EffectType.Increase)
                .SetTarget<CropTarget>()
                .SetBaseCost(1) // Low cost karena fast (2s)
                .SetInRange(1f);

            // ACTION: PlantSeedSlowAction (WITHOUT shovel - fallback)
            builder.AddAction<PlantSeedSlowAction>()
                .AddCondition<CropGrowthStage>(Comparison.SmallerThanOrEqual, 0) // Tanah kosong
                .AddCondition<HasSeedKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya bibit
                // NO HasShovelKey condition = fallback option
                .AddEffect<CropGrowthStage>(EffectType.Increase)
                .SetTarget<CropTarget>()
                .SetBaseCost(3) // High cost karena slow (5s)
                .SetInRange(1f);

            // ACTION: GetWateringCanAction
            builder.AddAction<GetWateringCanAction>()
                .AddEffect<HasWateringCanKey>(EffectType.Increase)
                .SetTarget<WaterSourceTargetKey>()
                .SetBaseCost(2)
                .SetInRange(1f);

            // ACTION: WaterCropAction
            // CropGrowthStage condition dihapus - sensor CropNeedsWater sudah handle stage check
            // HasWateringCanKey Decrease ditambahkan agar GOAP tahu ember habis setelah dipakai
            builder.AddAction<WaterCropAction>()
                .AddCondition<CropNeedsWater>(Comparison.GreaterThanOrEqual, 1)
                .AddCondition<HasWateringCanKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya ember
                .AddEffect<CropGrowthStage>(EffectType.Increase)
                .AddEffect<CropNeedsWater>(EffectType.Decrease)
                .AddEffect<HasWateringCanKey>(EffectType.Decrease) // Ember habis setelah dipakai
                .SetTarget<CropTarget>()
                .SetBaseCost(2)
                .SetInRange(1f);

            // ACTION: HarvestCropAction
            builder.AddAction<HarvestCropAction>()
                .AddCondition<CropGrowthStage>(Comparison.GreaterThanOrEqual, 3)
                .AddEffect<CropGrowthStage>(EffectType.Decrease)
                .AddEffect<HasFood>(EffectType.Increase)
                .SetTarget<CropTarget>()
                .SetBaseCost(3)
                .SetInRange(1f);

            // ACTION: EatAction
            builder.AddAction<EatAction>()
                .AddCondition<HasFood>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<HungerLevel>(EffectType.Decrease)
                .SetTarget<StorageTarget>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: SleepAction
            builder.AddAction<SleepAction>()
                .AddEffect<EnergyLevel>(EffectType.Increase)
                .SetTarget<BedTarget>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ========== SENSORS - World ==========
            builder.AddWorldSensor<HungerLevelSensor>()
                .SetKey<HungerLevel>();

            builder.AddWorldSensor<EnergyLevelSensor>()
                .SetKey<EnergyLevel>();

            builder.AddWorldSensor<CropGrowthStageSensor>()
                .SetKey<CropGrowthStage>();

            builder.AddWorldSensor<HasFoodSensor>()
                .SetKey<HasFood>();

            builder.AddWorldSensor<CropNeedsWaterSensor>()
                .SetKey<CropNeedsWater>();

            builder.AddWorldSensor<HasSeedSensor>()
                .SetKey<HasSeedKey>();

            builder.AddWorldSensor<HasWateringCanSensor>()
                .SetKey<HasWateringCanKey>();

            builder.AddWorldSensor<HasShovelSensor>()
                .SetKey<HasShovelKey>();

            builder.AddWorldSensor<ShovelAvailableSensor>()
                .SetKey<ShovelAvailable>();

            // ========== SENSORS - Target ==========
            builder.AddTargetSensor<CropTargetSensor>()
                .SetTarget<CropTarget>();

            builder.AddTargetSensor<StorageTargetSensor>()
                .SetTarget<StorageTarget>();

            builder.AddTargetSensor<BedTargetSensor>()
                .SetTarget<BedTarget>();

            builder.AddTargetSensor<SeedStorageTargetSensor>()
                .SetTarget<SeedStorageTargetKey>();

            builder.AddTargetSensor<WaterSourceTargetSensor>()
                .SetTarget<WaterSourceTargetKey>();

            builder.AddTargetSensor<ShovelStorageTargetSensor>()
                .SetTarget<ShovelStorageTargetKey>();

            builder.AddTargetSensor<RandomWanderTargetSensor>()
                .SetTarget<RandomWanderTargetKey>();

            return builder.Build();
        }
    }
}
