# PANDUAN VERIFIKASI MULTI-AGENT SYSTEM

## Tanggal: 7 Februari 2026

## PERTANYAAN PENTING YANG HARUS DIJAWAB:

### ✅ 1. Apakah benar-benar ada 3 crops terpisah di scene?
### ✅ 2. Apakah setiap crop punya instance ID unik?
### ✅ 3. Apakah auction menerima bids untuk 3 crops berbeda?
### ✅ 4. Apakah sensor menemukan 3 crops?

---

## LANGKAH 1: SETUP CROPS DI SCENE

### Pastikan Ada 3 GameObject Crops:
1. **Buka Unity Scene** (SampleScene atau scene farming Anda)
2. **Cek Hierarchy** - harus ada 3 GameObjects dengan:
   - ✅ Component: `CropBehaviour`
   - ✅ Nama yang jelas: `Crop A`, `Crop B`, `Crop C` (atau nama lain yang berbeda)
   - ✅ **Posisi berbeda** (minimal jarak 2-3 units antar crops)
   - ✅ **Collider2D** (untuk Physics2D detection)

### Contoh Setup di Inspector:
```
Hierarchy:
├── Crop A
│   ├── Transform: Position (0, 0, 0)
│   ├── CropBehaviour (Script)
│   └── BoxCollider2D atau CircleCollider2D
├── Crop B
│   ├── Transform: Position (5, 0, 0)
│   ├── CropBehaviour (Script)
│   └── BoxCollider2D atau CircleCollider2D
└── Crop C
    ├── Transform: Position (10, 0, 0)
    ├── CropBehaviour (Script)
    └── BoxCollider2D atau CircleCollider2D
```

**⚠️ PENTING:**
- **Jangan overlapping positions!** Jika crops terlalu dekat, Physics2D bisa bingung
- **Nama harus berbeda** agar mudah dibedakan di log
- **Pastikan semua aktif** (not disabled di inspector)

---

## LANGKAH 2: ATTACH CROP DEBUGGER

1. **Buat GameObject baru** di scene (nama: `CropDebugger` atau apa saja)
2. **Add Component** → Search `CropDebugger` → Attach script
3. **Inspector settings:**
   - `Enable Continuous Debug`: ✅ (checkmark)
   - `Debug Interval`: `2` (log every 2 seconds)

**Atau attach ke CropManager GameObject** untuk convenience.

---

## LANGKAH 3: RUN & CHECK LOG

### Saat Game Start, Anda HARUS melihat logs ini:

#### A. **CropManager Diagnostic** (dari CropManager.Awake):
```
========== CROPMANAGER DIAGNOSTIC ==========
[CropManager] Total crops found in scene: 3
[CropManager] Crop #1: Name='Crop A', InstanceID=12345, Position=(0.00, 0.00, 0.00)
[CropManager] Crop #2: Name='Crop B', InstanceID=67890, Position=(5.00, 0.00, 0.00)
[CropManager] Crop #3: Name='Crop C', InstanceID=24680, Position=(10.00, 0.00, 0.00)
[CropManager] ✅ All crops have UNIQUE instances (tracking will work correctly)
============================================
```

**❌ JIKA JUMLAH CROPS BUKAN 3:**
- Cek Hierarchy - apakah ada 3 GameObject dengan CropBehaviour?
- Cek apakah GameObject disabled
- Cek apakah script CropBehaviour attached dengan benar

**❌ JIKA INSTANCE IDs DUPLICATE:**
- BUG SERIOUS! Unity tidak seharusnya membuat duplicate instance IDs
- Try: Delete crops → Create new GameObjects → Re-attach CropBehaviour

#### B. **CropDebugger Logs** (setiap 2 detik):
```
========== CROP DEBUGGER ==========
Total Crops Found: 3
Crop #1: Name='Crop A', InstanceID=12345, Pos=(0.00, 0.00, 0.00), Stage=0, ReservedBy=NONE
Crop #2: Name='Crop B', InstanceID=67890, Pos=(5.00, 0.00, 0.00), Stage=0, ReservedBy=NONE
Crop #3: Name='Crop C', InstanceID=24680, Pos=(10.00, 0.00, 0.00), Stage=0, ReservedBy=NONE
===================================
```

**Setelah auction, ReservedBy berubah:**
```
Crop #1: Name='Crop A', InstanceID=12345, Stage=0, ReservedBy=Farmer A
Crop #2: Name='Crop B', InstanceID=67890, Stage=0, ReservedBy=Farmer B
Crop #3: Name='Crop C', InstanceID=24680, Stage=0, ReservedBy=Farmer C
```

#### C. **Auction Logs** (dari CropManager):
```
[Auction] Processing bids for 3 crops (Unique IDs: 3)
[Bid] Farmer A → Crop A (U=0.520, Planting, CropID=12345)
[Bid] Farmer B → Crop B (U=0.803, Planting, CropID=67890)
[Bid] Farmer C → Crop C (U=0.607, Planting, CropID=24680)
[Auction] Crop A: Farmer A (U=0.520)
[Auction] Crop B: Farmer B (U=0.803)
[Auction] Crop C: Farmer C (U=0.607)
```

**✅ YANG BENAR:**
- 3 crops berbeda (CropID berbeda)
- 3 farmers bid ke 3 crops berbeda
- 3 auctions assign ke 3 farmers berbeda

**❌ JIKA SEMUA BID KE CROP YANG SAMA:**
- Check crop positions - mungkin overlapping
- Check SelectFarmingGoalMultiAgent - utility calculation
- Check sensor - apakah menemukan semua crops?

#### D. **Sensor Logs:**
```
[Sensor] Farmer A → Crop A (reserved)
[Sensor] Farmer B → Crop B (reserved)
[Sensor] Farmer C → Crop C (reserved)
```

**✅ YANG BENAR:**
- Setiap farmer mendapat crop berbeda

**❌ JIKA SENSOR RETURN NULL:**
- Crop belum direserve (auction belum jalan)
- Bug di CropTargetSensor

#### E. **Action Verification Logs:**
```
[PlantFast] Farmer A verified reserved crop: Crop A
[PlantFast] Farmer B verified reserved crop: Crop B
[PlantFast] Farmer C verified reserved crop: Crop C
```

**✅ YANG BENAR:**
- Setiap action bekerja pada crop yang benar

**❌ JIKA ADA WARNING:**
```
[PlantFast] Farmer B found Crop A but it's reserved by Farmer A! Ignoring.
```
- Ini artinya crops terlalu dekat (Physics2D detects wrong crop)
- **Solusi:** Pindahkan crops lebih jauh (minimal jarak 3 units)

---

## LANGKAH 4: VERIFY BEHAVIOR

### Expected Behavior (Yang BENAR):
```
T=0s:   Auction → Farmer A gets Crop A, B gets B, C gets C
T=2s:   All farmers plant their respective crops
T=5s:   All farmers water their respective crops
T=15s:  Crops grow to stage 3
T=20s:  All farmers harvest their respective crops
T=22s:  New auction → Farmers get new crops (possibly different ones)
```

### ❌ Bug Symptoms:
1. **Only 1 crop worked:** Hanya Crop A/B/C yang ditanam/disiram/dipanen
2. **Farmers switching crops mid-cycle:** Farmer A pindah dari Crop A ke Crop B sebelum selesai
3. **Infinite sensor loop:** Logs penuh dengan "[Sensor] Farmer X → Crop Y"
4. **STOPPED/INTERRUPTED logs:** Farmers release crops premature

---

## LANGKAH 5: TROUBLESHOOTING

### Problem: "Total crops found: 1" atau "Total crops found: 0"
**Penyebab:**
- Crops tidak ada di scene
- CropBehaviour script tidak attached
- GameObject disabled

**Solusi:**
1. Cek Hierarchy untuk GameObject dengan CropBehaviour
2. Pastikan script attached dengan benar
3. Pastikan GameObject aktif (checkmark di Inspector)
4. Pastikan scene saved

### Problem: "Farmers all bid on same crop"
**Penyebab:**
- Crops terlalu dekat / overlapping positions
- Utility calculation sama untuk semua crops
- Distance bonus tidak cukup besar

**Solusi:**
1. **Pindahkan crops lebih jauh** (minimal 5 units difference)
2. Check `SelectFarmingGoalMultiAgent.cs` - distance bonus calculation
3. Give farmers different stats (different WeightEnergy, WeightHunger)

### Problem: "Sensor finds crop but action can't verify it"
**Penyebab:**
- Crops tidak punya Collider2D
- Physics2D OverlapCircleAll radius terlalu kecil (1f)
- Crops terlalu dekat sehingga Physics2D detects wrong crop

**Solusi:**
1. Add BoxCollider2D atau CircleCollider2D ke setiap crop
2. Set collider isTrigger = true
3. Increase jarak antar crops

### Problem: "ActionCompleted tracking not working"
**Penyebab:**
- Old version of action scripts (before fix)
- Data class tidak punya `ActionCompleted` field

**Solusi:**
1. Check all 6 farming actions punya field:
   ```csharp
   public bool ActionCompleted;
   ```
2. Check Perform() sets:
   ```csharp
   data.ActionCompleted = true;
   ```
3. Check Stop() checks:
   ```csharp
   if (!data.ActionCompleted && ...)
   ```

---

## LANGKAH 6: MANUAL TEST (Context Menu)

Di Inspector (dengan CropDebugger selected), ada 3 buttons:
1. **Log All Crops** - Manual trigger untuk log semua crops
2. **Verify Crop Instances** - Cek apakah semua crops punya unique IDs
3. **Test Crop Reservation** - Cek status reservasi semua crops

Gunakan ini untuk quick verification tanpa perlu run game!

---

## SUMMARY CHECKLIST

Sebelum report bug, pastikan:
- [ ] Ada 3 GameObject dengan CropBehaviour di scene
- [ ] Semua crops punya nama berbeda (Crop A, B, C)
- [ ] Semua crops punya posisi berbeda (minimal jarak 3 units)
- [ ] Semua crops punya Collider2D
- [ ] CropManager diagnostic shows "Total crops: 3"
- [ ] CropManager diagnostic shows "All crops have UNIQUE instances"
- [ ] CropDebugger shows 3 crops dengan instance IDs berbeda
- [ ] Auction logs show "Processing bids for 3 crops (Unique IDs: 3)"
- [ ] Sensor logs show different crops for different farmers
- [ ] Action verification shows correct crops for each farmer
- [ ] No "STOPPED, released" logs during normal completion

---

## KESIMPULAN

**Penyebab masalah "hanya 1 crop dikerjakan" BISA JADI:**

1. ❌ **Hanya ada 1 crop di scene** (most likely!)
2. ❌ **Crops overlapping** → Physics2D bingung
3. ❌ **Semua crops punya posisi sama** → Agent pilih yang sama
4. ❌ **ActionCompleted tracking belum diimplementasi** → Premature release
5. ❌ **Collider2D tidak ada** → Action tidak bisa find crop

**Dengan debug scripts ini, kita bisa MEMBUKTIKAN:**
- ✅ Berapa crops yang benar-benar ada
- ✅ Apakah tracking instance-based (bukan script-based)
- ✅ Apakah auction assign ke crops berbeda
- ✅ Apakah sensor menemukan crops yang benar
- ✅ Apakah actions bekerja pada crops yang benar

**Next step:** Run game → Post logs → Kita analyze bersama! 🔍
