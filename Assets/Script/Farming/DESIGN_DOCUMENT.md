# 📊 GOAP System Design - Farming NPC (REDESIGNED)

## ⚠️ MASALAH DESAIN LAMA
**SALAH**: WateringGoal → PlantCropAction ❌
- Tidak masuk akal! Watering dan Planting adalah bidang berbeda
- Action tidak boleh melompat antar domain yang berbeda

**BENAR**: Setiap Goal harus punya action chain yang logis ✓

---

## 🎯 Goals Hierarchy (Priority-Based) - REDESIGNED

```
Priority 1 (HIGHEST): EatGoal
    ├─ Condition: Hunger > 70 AND HasFood >= 1
    ├─ Jika HasFood < 1 → Goal TIDAK BISA dilakukan (skip ke goal lain)
    └─ Action: EatAction (Cost: 1)

Priority 2: SleepGoal
    ├─ Condition: Energy < 30
    └─ Action: SleepAction (Cost: 1)

Priority 3: HarvestingGoal
    ├─ Condition: CropGrowthStage >= 3
    ├─ Action Chain:
    │   └─ GetToolAction (jika ada alat panen, cost: 2)
    │   └─ HarvestCropAction (cost: 3 jika tanpa alat, cost: 1 jika dengan alat)
    └─ Effect: CropGrowthStage → 0, HasFood +1

Priority 4: WateringGoal
    ├─ Condition: CropGrowthStage >= 1 AND CropGrowthStage < 3
    ├─ Action Chain:
    │   └─ GetWateringCanAction (ambil ember, cost: 2)
    │   └─ WaterCropAction (cost: 2)
    └─ Effect: CropGrowthStage +1

Priority 5: PlantingGoal
    ├─ Condition: CropGrowthStage == 0
    ├─ Action Chain:
    │   ├─ GetSeedAction (ambil bibit, cost: 1)
    │   ├─ GetShovelAction [OPTIONAL] (ambil sekop, cost: 2)
    │   └─ PlantSeedAction:
    │       ├─ Dengan sekop: cost: 2, waktu: 2 detik
    │       └─ Tanpa sekop: cost: 5, waktu: 5 detik
    └─ Effect: CropGrowthStage +1

Priority 6 (LOWEST): IdleGoal
    ├─ Condition: Jika tidak ada goal lain yang bisa dilakukan
    └─ Action: WanderAction (berkeliling random)
```

---

## � Cara Mengatur Timing Crop Growth

**File**: `CropBehaviour.cs`

```csharp
public class CropBehaviour : MonoBehaviour
{
    [Header("Growth Timing Settings")]
    [Tooltip("Waktu tiap fase dalam detik")]
    public float growthTimePerStage = 10f; // Ubah ini untuk atur kecepatan tumbuh!
    
    // Stage 0 → 1: Butuh 10 detik (default)
    // Stage 1 → 2: Butuh 10 detik (default)
    // Stage 2 → 3: Butuh 10 detik (default)
}
```

**Cara Mengubah**:
1. Pilih GameObject "Crop" di Hierarchy
2. Lihat Inspector → CropBehaviour
3. Ubah nilai `Growth Time Per Stage`:
   - 5 detik = cepat
   - 10 detik = normal
   - 20 detik = lambat
   - 60 detik = sangat lambat

---

## 🚶 Idle Behavior

Jika tidak ada goal yang memenuhi kondisi, agent akan **idle dan berkeliling**:

```csharp
// IdleGoal - priority paling rendah, selalu bisa dijalankan
public class IdleGoal : GoalBase
{
    // Tidak ada kondisi, selalu tersedia sebagai fallback
}

// WanderAction - jalan random
public class WanderAction : GoapActionBase<WanderAction.Data>
{
    // Pilih random position di sekitar
    // Berjalan ke sana
    // Tunggu beberapa detik
    // Ulangi
}
```

---

## 🔄 Action Chain Flow - REDESIGNED


### Scenario 1: Normal Farming Cycle (REDESIGNED)
```
[Start] Crop=0, Hunger=20, Energy=100, Food=0, HasSeed=0, HasShovel=0
    ↓
PlantingGoal Selected (Priority 5)
    ↓
GOAP Planning (backward chaining):
    Goal: PlantingGoal
        ├─ Needs: CropGrowthStage == 1
        ├─ Find action with effect: CropGrowthStage +1
        ├─ Found: PlantSeedAction
        │   ├─ Needs condition: HasSeed >= 1
        │   ├─ Find action with effect: HasSeed +1
        │   └─ Found: GetSeedAction (Cost: 1)
        └─ Total Plan: GetSeedAction → PlantSeedAction
    ↓
Execute: GetSeedAction
    ├─ Agent pergi ke StorageTarget
    ├─ Ambil bibit (waktu: 1 detik)
    └─ Effect: HasSeed = 1
    ↓
Execute: PlantSeedAction (tanpa sekop)
    ├─ Agent pergi ke CropTarget
    ├─ Tanam bibit (waktu: 5 detik, cost: 5)
    └─ Effect: CropGrowthStage = 1, HasSeed = 0
    ↓
[State] Crop=1, Hunger=25, Energy=97, Food=0
    ↓
WateringGoal Selected (Priority 4)
    ↓
GOAP Planning:
    Goal: WateringGoal
        ├─ Find action: WaterCropAction
        │   ├─ Needs: HasWateringCan >= 1
        │   └─ Find: GetWateringCanAction
        └─ Plan: GetWateringCanAction → WaterCropAction
    ↓
Execute: GetWateringCanAction
    ├─ Pergi ke WaterSourceTarget
    └─ Effect: HasWateringCan = 1
    ↓
Execute: WaterCropAction (1st time)
    ├─ Siram tanaman (waktu: 3 detik)
    └─ Effect: CropGrowthStage = 2, HasWateringCan = 0
    ↓
[State] Crop=2, Hunger=30, Energy=94, Food=0
    ↓
WateringGoal Selected (Priority 4) [2nd time]
    ↓
Execute: GetWateringCanAction → WaterCropAction
    └─ Effect: CropGrowthStage = 3
    ↓
[State] Crop=3, Hunger=35, Energy=91, Food=0
    ↓
HarvestingGoal Selected (Priority 3)
    ↓
Execute: HarvestCropAction
    ├─ Panen tanaman (waktu: 4 detik)
    └─ Effect: CropGrowthStage = 0, HasFood = 1
    ↓
[State] Crop=0, Hunger=40, Energy=87, Food=1
    ↓
LOOP back to PlantingGoal...
```

### Scenario 2: Hunger Interrupt - REDESIGNED
```
[State] Crop=2, Hunger=75, Energy=50, Food=0
    ↓
EatGoal Selected (Priority 1 - HIGHEST)
    ├─ Check condition: Hunger > 70 ✓
    └─ Check condition: HasFood >= 1 ✗ (Food=0)
    ↓
EatGoal CANNOT be satisfied!
    └─ Brain: Skip EatGoal, cari goal lain
    ↓
WateringGoal Selected (Priority 4)
    ↓ (agent tetap kerja meski lapar)
Execute: WaterCropAction → Crop=3
    ↓
[State] Crop=3, Hunger=80, Energy=47, Food=0
    ↓
HarvestingGoal Selected (Priority 3)
    ↓ (harvest dulu untuk dapat makanan)
Execute: HarvestCropAction → Food=1
    ↓
[State] Crop=0, Hunger=85, Energy=44, Food=1
    ↓
NOW EatGoal dapat dilakukan!
    ├─ Hunger > 70 ✓
    └─ HasFood >= 1 ✓
    ↓
Execute: EatAction
    ├─ Makan makanan (waktu: 2 detik)
    └─ Effect: Hunger = 35, HasFood = 0
    ↓
[State] Crop=0, Hunger=35, Energy=42, Food=0
    ↓
Resume farming work...
```

**PENTING**: EatGoal **tidak boleh** memaksa agent untuk harvest. Jika tidak ada makanan, goal ini diabaikan sampai ada makanan tersedia.

### Scenario 3: Energy Depletion
```
[State] Crop=1, Hunger=30, Energy=25, Food=1
    ↓
SleepGoal Selected (Priority 2)
    ↓ resolve to
SleepAction
    ├─ No conditions (always available)
    ├─ Effect: Energy +30/sec
    └─ Target: BedTarget
    ↓ (runs for ~3 seconds)
[State] Crop=1, Hunger=35, Energy=90, Food=1
    ↓
Resume farming work...
```

### Scenario 4: Idle Behavior - BARU
```
[State] Crop=3, Hunger=25, Energy=100, Food=3
    ↓
Brain checks all goals:
    ├─ EatGoal: Hunger < 70 ✗ (tidak perlu makan)
    ├─ SleepGoal: Energy >= 30 ✗ (tidak perlu tidur)
    ├─ HarvestingGoal: Crop >= 3 ✓ BUT Food sudah banyak, skip
    ├─ WateringGoal: Crop == 3 ✗ (sudah matang)
    └─ PlantingGoal: Crop != 0 ✗ (tanaman masih ada)
    ↓
Tidak ada goal yang mendesak!
    └─ IdleGoal Selected (Priority 6 - LOWEST)
    ↓
Execute: WanderAction
    ├─ Pilih random position di sekitar
    ├─ Berjalan ke sana (waktu: 3 detik)
    ├─ Tunggu sebentar (waktu: 2 detik)
    └─ Ulangi random walk
    ↓
Agent berkeliling-keliling...
    ↓
(Setelah beberapa saat, hunger atau energy menurun)
    ↓
Goal lain menjadi prioritas lagi
```

---

## 🧠 Brain Decision Tree - REDESIGNED

```
FarmerBrain.SelectGoal() - Called every 60 frames
│
├─ IF Hunger > 70 AND FoodCount > 0
│   └─ REQUEST: EatGoal ✓
│
├─ ELSE IF Energy < 30
│   └─ REQUEST: SleepGoal ✓
│
├─ ELSE IF CropStage >= 3
│   └─ REQUEST: HarvestingGoal ✓
│
├─ ELSE IF CropStage >= 1 AND CropStage < 3
│   └─ REQUEST: WateringGoal ✓
│
├─ ELSE IF CropStage == 0
│   └─ REQUEST: PlantingGoal ✓
│
└─ ELSE (tidak ada goal mendesak)
    └─ REQUEST: IdleGoal ✓ (jalan-jalan)
```

**Catatan Penting**:
- Jika `Hunger > 70` tetapi `FoodCount = 0`, EatGoal **tidak direquest**
- Brain tidak memaksa harvest untuk mendapat makanan
- Agent akan tetap kerja atau idle sampai makanan tersedia

---

## 📊 WorldKeys & Sensors

| WorldKey | Type | Sensor | Scope | Returns |
|----------|------|--------|-------|---------|
| HungerLevel | int | HungerLevelSensor | Local | stats.Hunger (0-100) |
| EnergyLevel | int | EnergyLevelSensor | Local | stats.Energy (0-100) |
| CropGrowthStage | int | CropGrowthStageSensor | Local | crop.GrowthStage (0-3) |
| HasFood | bool | HasFoodSensor | Local | stats.FoodCount > 0 ? 1 : 0 |
| CropNeedsWater | bool | CropNeedsWaterSensor | Local | stage >= 1 && stage < 3 ? 1 : 0 |

---

## 🎯 TargetKeys & Sensors

| TargetKey | Sensor | Scope | Returns |
|-----------|--------|-------|---------|
| CropTarget | CropTargetSensor | Local | TransformTarget(crop) |
| StorageTarget | StorageTargetSensor | Local | TransformTarget(GameObject.FindWithTag("Storage")) |
| BedTarget | BedTargetSensor | Local | TransformTarget(GameObject.FindWithTag("Bed")) |

---

## ⚙️ Action Properties

| Action | Conditions | Effects | Target | Cost | Time |
|--------|-----------|---------|--------|------|------|
| PlantCropAction | CropStage <= 0 | CropStage +1 | CropTarget | 2 | 2s |
| WaterCropAction | CropStage >= 1, NeedsWater == 1 | CropStage +1, NeedsWater -1 | CropTarget | 2 | 1.5s |
| HarvestCropAction | CropStage >= 3 | CropStage → 0, HasFood +1 | CropTarget | 3 | 2s |
| EatAction | HasFood >= 1 | Hunger -50, HasFood -1 | StorageTarget | 1 | 1s |
| SleepAction | (none) | Energy +30/sec | BedTarget | 1 | ~3s |

---

## 📈 Stats Passive Changes

| Stat | Rate | Direction | Trigger |
|------|------|-----------|---------|
| Hunger | +5 | Increase | Every 10 seconds (always) |
| Energy | -3 | Decrease | Every 10 seconds (always) |
| Food | - | Manual | From Harvest/Eat actions |
| CropStage | - | Manual | From Plant/Water/Harvest actions |

---

## 🎮 Component Dependencies

```
Farmer GameObject
├─ AgentBehaviour (Crashkonijn) ← Required for GOAP
├─ GoapActionProvider (Crashkonijn) ← Manages goals/actions
├─ ActionReceiver (Crashkonijn) ← Receives action commands
├─ AgentMoveBehaviour (Crashkonijn) ← Handles movement
├─ Rigidbody2D (Unity) ← Physics
├─ Collider2D (Unity) ← Collision
├─ SpriteRenderer (Unity) ← Visual
├─ NPCStats (Custom) ← Hunger, Energy, Food
├─ CropBehaviour (Custom) ← Crop growth logic
├─ FarmerBrain (Custom) ← Goal selection AI
└─ NPCDebugDisplay (Custom) ← Optional debugging
```

---

## 🔍 Debugging Tips

### Check Current Goal
```csharp
Debug.Log(actionProvider.CurrentGoal?.GetType().Name);
```

### Check Current Action
```csharp
Debug.Log(actionProvider.CurrentAction?.GetType().Name);
```

### Force Goal Change
```csharp
actionProvider.RequestGoal<HarvestingGoal>();
```

### Check WorldState
Open **GOAP Graph Viewer** (Window → GOAP → Graph Viewer)
- Select Farmer in Hierarchy
- See live world state values
- See resolved action path

---

## 📝 Key Design Decisions

1. **Priority-based Goal Selection**: Sederhana, predictable, mudah di-debug
2. **Single Crop per NPC**: Memudahkan tracking, scalable nanti ke multiple
3. **Passive Stats Decay**: Simulasi kebutuhan dasar yang realistis
4. **No Idle Goal**: NPC selalu produktif (bisa tambahkan nanti)
5. **Hard-coded Brain Logic**: Fast & simple untuk prototype (bisa upgrade ke Utility AI)

---

## 🚀 Future Enhancements

1. **Multiple Crops**: NPC manage beberapa lahan
2. **Dynamic Crop Selection**: Pilih crop terdekat yang butuh perhatian
3. **Resource Types**: Wood, stone, bukan hanya food
4. **Shared World State**: Global sensors untuk multiple NPCs
5. **Utility-based Goal Selection**: Skor goals berdasarkan urgensi
6. **Social Interactions**: NPC berinteraksi antar mereka
7. **Building System**: Construct buildings, bukan hanya farming
8. **Day/Night Cycle**: Influence behavior (tidur di malam hari)
