# Multi-Agent System Fix - Changelog

## Tanggal: 6 Februari 2026

## Masalah yang Ditemukan:

### 1. **Agent Beralih Crop Terlalu Sering (Crop Switching Bug)**
- **Gejala**: Agent menyelesaikan PlantingGoal, lalu langsung beralih ke crop lain padahal seharusnya lanjut WateringGoal pada crop yang sama
- **Penyebab**: 
  - `SelectFarmingGoalMultiAgent` dipanggil setiap update cycle
  - Setiap kali dipanggil, agent menghitung utility untuk SEMUA crops
  - Jika ada crop lain dengan utility lebih tinggi, agent langsung beralih dan melepaskan crop lama
  - Tidak ada "commitment" ke crop yang sudah dikerjakan
  
- **Log yang Menunjukkan Masalah**:
  ```
  [PlantFast] Farmer C finished planting Crop C (keeping reservation)
  [Farmer C] Farming: WateringGoal → Crop C (U=0.422)
  [CropManager] Released: Farmer C released Crop C
  [Farmer C] Switched crop: Crop C → Crop A, RELEASED old crop
  ```

### 2. **Agent Tidak Menyelesaikan Full Cycle (Plant → Water → Harvest)**
- **Gejala**: Agent "melompat-lompat" antar crops tanpa menyelesaikan tahapan lengkap
- **Penyebab**: Tidak ada logic untuk memaksa agent menyelesaikan semua tahapan pada satu crop
- **Dampak**: Multiple agents bisa bekerja pada crop yang sama (konflik), crops tidak terurus dengan baik

### 3. **HasWateringCan Increment Berlebihan**
- **Gejala**: HasWateringCan increment terus sampai 14+ tanpa berhenti
- **Penyebab**: 
  - Agent terus re-planning karena crop switching
  - Setiap re-plan, GOAP menambahkan GetWateringCanAction ke plan
  - GetWateringCanAction tidak cek apakah sudah punya watering can
- **Log yang Menunjukkan Masalah**:
  ```
  [GetWateringCanAction] Ambil ember! HasWateringCan: 1
  [GetWateringCanAction] Ambil ember! HasWateringCan: 2
  ...
  [GetWateringCanAction] Ambil ember! HasWateringCan: 14
  ```

---

## Solusi yang Diimplementasikan:

### ✅ Fix 1: Crop Commitment System (`SelectFarmingGoalMultiAgent.cs`)

**Perubahan Utama:**
```csharp
// CRITICAL FIX: Check if we have a reserved crop that still needs work
CropBehaviour currentReservedCrop = null;
foreach (var crop in allCrops)
{
    var reservedAgent = CropManager.Instance.GetReservedAgent(crop);
    if (reservedAgent == Owner.gameObject)
    {
        currentReservedCrop = crop;
        break;
    }
}

// If we have a reserved crop, check if it still needs work
if (currentReservedCrop != null)
{
    int stage = currentReservedCrop.GrowthStage;
    bool cropNeedsWork = (stage == 0) ||  // Empty → needs planting
                        (stage == 1) ||   // Planted → needs watering
                        (stage == 2) ||   // Growing → needs watering  
                        (stage == 3);     // Ready → needs harvesting

    // If crop still needs work, CONTINUE with this crop!
    if (cropNeedsWork)
    {
        // Determine appropriate goal based on stage
        // Request goal and RETURN (don't switch to different crop!)
    }
}
```

**Cara Kerja:**
1. Sebelum mencari crop baru, **cek apakah agent sudah punya reserved crop**
2. Jika ada, cek apakah crop masih butuh dikerjakan (stage 0-3)
3. Jika crop masih needs work:
   - Stage 0 (Empty) → Request PlantingGoal
   - Stage 1-2 (Planted/Growing) → Request WateringGoal
   - Stage 3 (Ready) → Request HarvestingGoal
4. Agent **TIDAK AKAN beralih ke crop lain** sampai:
   - Crop sudah fully harvested (released di HarvestCropAction.End())
   - Agent beralih ke goal non-farming (misal Survival)
   - Crop tidak lagi valid

**Dampak:**
- ✅ Agent menyelesaikan full cycle: Plant → Water → Harvest
- ✅ Tidak ada crop switching di tengah proses
- ✅ Multiple agents bekerja pada crops yang berbeda (proper load balancing)

### ✅ Fix 2: Hapus Auto-Release pada Target Change

**Sebelum:**
```csharp
// Release old crop if switching to different crop
if (targetChanged && lastTargetCrop != null && CropManager.Instance != null)
{
    CropManager.Instance.ReleaseCrop(lastTargetCrop, Owner.gameObject);
    // ❌ INI MENYEBABKAN BUG!
}
```

**Sesudah:**
```csharp
// DON'T release old crop here - this was causing agents to abandon crops mid-cycle!
// Crops are only released when:
// 1. Agent explicitly switches to Survival/other non-farming goal (handled in Action.Stop())
// 2. Crop is fully harvested (handled in HarvestCropAction.End())
// 3. Crop determined to no longer need work (handled above)
```

**Dampak:**
- ✅ Crop hanya direlease di tempat yang benar (HarvestCropAction, Action.Stop())
- ✅ Tidak ada accidental release di tengah proses

### ✅ Fix 3: Prevent Excessive Watering Can Accumulation (`GetWateringCanAction.cs`)

**Perubahan:**
```csharp
if (data.Timer <= 0f)
{
    var stats = agent.GetComponent<NPCStats>();
    if (stats != null)
    {
        // Only get watering can if we don't already have one
        if (stats.HasWateringCan < 1)
        {
            stats.HasWateringCan++;
            UnityEngine.Debug.Log($"[GetWateringCanAction] Ambil ember! HasWateringCan: {stats.HasWateringCan}");
        }
        else
        {
            UnityEngine.Debug.Log($"[GetWateringCanAction] Sudah punya ember, skip.");
        }
    }
}
```

**Dampak:**
- ✅ Agent tidak akan ambil watering can berlebihan
- ✅ HasWateringCan tidak akan increment terus-menerus
- ✅ Resource management lebih efisien

---

## Hasil yang Diharapkan Setelah Fix:

### Log yang Benar:
```
[Farmer A] COMMITTED to reserved crop: PlantingGoal → Crop A (U=0.520, Stage=0)
[PlantFast] Farmer A finished planting Crop A (keeping reservation)
[Farmer A] COMMITTED to reserved crop: WateringGoal → Crop A (U=0.422, Stage=1)
[GetWateringCanAction] Ambil ember! HasWateringCan: 1
[Water] Farmer A finished watering Crop A (keeping reservation)
[Farmer A] COMMITTED to reserved crop: HarvestingGoal → Crop A (U=0.680, Stage=3)
[Harvest] Farmer A finished harvesting Crop A, RELEASED
[Farmer A] NEW CROP: PlantingGoal → Crop B (U=0.515, was Crop A)
```

### Behavior yang Diharapkan:
1. ✅ Agent stick dengan satu crop sampai selesai (Plant → Water → Harvest)
2. ✅ Agent hanya beralih crop setelah harvest selesai
3. ✅ Multiple agents bekerja pada crops yang berbeda secara paralel
4. ✅ Tidak ada konflik atau double-work pada crop yang sama
5. ✅ Resource management (watering can, shovel, seed) terkelola dengan baik
6. ✅ Auction system berfungsi dengan benar untuk initial crop assignment

---

## Testing Checklist:

- [ ] Agent menyelesaikan full cycle pada satu crop
- [ ] Tidak ada log "Switched crop" di tengah proses
- [ ] HasWateringCan tidak melebihi 1-2
- [ ] Multiple farmers bekerja pada crops yang berbeda
- [ ] Auction system assign crops dengan benar
- [ ] Tidak ada crop yang "terbengkalai" atau dikerjakan oleh multiple agents

---

## File yang Diubah:
1. `Script/BehaviorTree/Actions/SelectFarmingGoalMultiAgent.cs` - Tambah crop commitment logic
2. `Script/Farming/Actions/GetWateringCanAction.cs` - Prevent excessive accumulation

## File Terkait (Tidak Diubah, Sudah Benar):
- `Script/Farming/Managers/CropManager.cs` - Auction & reservation system
- `Script/Farming/Actions/PlantSeedFastAction.cs` - Keep reservation di End()
- `Script/Farming/Actions/WaterCropAction.cs` - Keep reservation di End(), consume watering can
- `Script/Farming/Actions/HarvestCropAction.cs` - Release crop di End()
