# 🎮 SETUP GUIDE - Farming GOAP (REDESIGNED)

## ⚠️ PERUBAHAN BESAR DARI VERSI LAMA

**DESAIN BARU**:
- ✅ Action chain yang logis (GetSeed → PlantSeed, GetWateringCan → Water, dll)
- ✅ EatGoal hanya jalan kalau ada food (tidak memaksa harvest)
- ✅ IdleGoal untuk berkeliling kalau tidak ada kerjaan
- ✅ Timing crop growth bisa diatur mudah di Inspector
- ✅ Inventory system (HasSeed, HasWateringCan, HasShovel)

---

## 📋 STEP 1: Setup GoapRunner (GameObject Utama)

1. Buat **Empty GameObject** bernama `GoapRunner`
2. Tambahkan 3 components:
   - `GoapBehaviour`
   - `ReactiveController`
   - `FarmerAgentTypeFactory`

**GoapRunner Inspector**:
```
GoapRunner GameObject
├─ GoapBehaviour
├─ ReactiveController
└─ FarmerAgentTypeFactory
```

---

## 📋 STEP 2: Setup Farmer (NPC Agent)

1. Buat **GameObject** bernama `Farmer`
2. Set position: (0, 0, 0)
3. Tambahkan components:
   - `SpriteRenderer` (kasih sprite farmer)
   - `Rigidbody2D`:
     - Body Type: Dynamic
     - Gravity Scale: 0
   - `CircleCollider2D`
   - `AgentBehaviour`
   - `GoapActionProvider`
   - `AgentMoveBehaviour`:
     - Move Speed: 3
   - `NPCStats`
   - `FarmerBrain`
   - `NPCDebugDisplay` (opsional)

**Farmer Inspector**:
```
Farmer GameObject
├─ SpriteRenderer
├─ Rigidbody2D (Dynamic, Gravity 0)
├─ CircleCollider2D
├─ AgentBehaviour
├─ GoapActionProvider
├─ AgentMoveBehaviour (Speed: 3)
├─ NPCStats
│   ├─ Hunger: 0
│   ├─ Energy: 100
│   ├─ Food Count: 0
│   ├─ Has Seed: 0
│   ├─ Has Watering Can: 0
│   └─ Has Shovel: 0
├─ FarmerBrain
└─ NPCDebugDisplay (optional)
```

---

## 📋 STEP 3: Setup Crop (Tanaman) - SEPARATE GAMEOBJECT!

**PENTING**: Crop adalah GameObject **TERPISAH**, bukan child atau component dari Farmer!

1. Buat **GameObject** bernama `Crop`
2. Set position: **(3, 0, 0)** ← jauh dari Farmer!
3. Tag: (biarkan Default atau buat tag "Crop")
4. Tambahkan components:
   - `SpriteRenderer` (kasih sprite tanaman)
   - `CropBehaviour`:
     - Growth Stage: 0
     - **Growth Time Per Stage: 10** ← UBAH INI UNTUK ATUR KECEPATAN TUMBUH
   - `CircleCollider2D`:
     - Radius: 0.5
     - Is Trigger: ✓ (CENTANG!)

**Crop Inspector**:
```
Crop GameObject (Position: 3, 0, 0)
├─ SpriteRenderer
├─ CropBehaviour
│   ├─ Growth Stage: 0
│   └─ Growth Time Per Stage: 10 ← UBAH DI SINI!
└─ CircleCollider2D (Radius: 0.5, Trigger: true)
```

**Cara Mengatur Kecepatan Tumbuh**:
- 5 detik = cepat
- 10 detik = normal (default)
- 20 detik = lambat
- 60 detik = sangat lambat

---

## 📋 STEP 4: Setup Storage (Tempat Makanan & Alat)

1. Buat **GameObject** bernama `Storage`
2. Set position: (-3, 0, 0)
3. Tag: **Storage** (PENTING!)
4. Tambahkan components:
   - `SpriteRenderer` (sprite kotak/storage)
   - `CircleCollider2D`:
     - Radius: 0.5
     - Is Trigger: ✓

**Storage Inspector**:
```
Storage GameObject (Position: -3, 0, 0)
├─ Tag: Storage ← WAJIB!
├─ SpriteRenderer
└─ CircleCollider2D (Radius: 0.5, Trigger: true)
```

---

## 📋 STEP 5: Setup WaterSource (Sumber Air)

1. Buat **GameObject** bernama `WaterSource`
2. Set position: (0, -3, 0)
3. Tag: **WaterSource** (PENTING!)
4. Tambahkan components:
   - `SpriteRenderer` (sprite sumur/air)
   - `CircleCollider2D`:
     - Radius: 0.5
     - Is Trigger: ✓

**WaterSource Inspector**:
```
WaterSource GameObject (Position: 0, -3, 0)
├─ Tag: WaterSource ← WAJIB!
├─ SpriteRenderer
└─ CircleCollider2D (Radius: 0.5, Trigger: true)
```

---

## 📋 STEP 6: Setup Bed (Tempat Tidur)

1. Buat **GameObject** bernama `Bed`
2. Set position: (-3, -3, 0)
3. Tag: **Bed** (PENTING!)
4. Tambahkan components:
   - `SpriteRenderer` (sprite kasur)
   - `CircleCollider2D`:
     - Radius: 0.5
     - Is Trigger: ✓

**Bed Inspector**:
```
Bed GameObject (Position: -3, -3, 0)
├─ Tag: Bed ← WAJIB!
├─ SpriteRenderer
└─ CircleCollider2D (Radius: 0.5, Trigger: true)
```

---

## 📋 STEP 7: Setup Tags

Buat tags berikut di Unity:
- **Storage**
- **WaterSource**
- **Bed**

**Cara Buat Tag**:
1. Edit → Project Settings → Tags and Layers
2. Tambahkan tag: Storage, WaterSource, Bed

---

## 🎯 CHECKLIST SETUP

- [ ] GoapRunner ada dengan 3 components (GoapBehaviour, ReactiveController, FarmerAgentTypeFactory)
- [ ] Farmer ada dengan 8 components (SpriteRenderer, Rigidbody2D, CircleCollider2D, AgentBehaviour, GoapActionProvider, AgentMoveBehaviour, NPCStats, FarmerBrain)
- [ ] Crop adalah **GameObject terpisah** di posisi (3, 0, 0) dengan CropBehaviour + CircleCollider2D (Trigger)
- [ ] Storage ada dengan tag "Storage" di posisi (-3, 0, 0)
- [ ] WaterSource ada dengan tag "WaterSource" di posisi (0, -3, 0)
- [ ] Bed ada dengan tag "Bed" di posisi (-3, -3, 0)
- [ ] Tags sudah dibuat: Storage, WaterSource, Bed

---

## 🎮 CARA BERMAIN

1. Tekan Play
2. Agent akan:
   - **Planting**: Pergi ke Storage ambil bibit → Tanam di Crop
   - **Watering**: Pergi ke WaterSource ambil ember → Siram Crop (2x)
   - **Harvesting**: Panen Crop → Dapat food
   - **Eating**: Kalau lapar & ada food → Makan
   - **Sleeping**: Kalau energy rendah → Tidur
   - **Idle**: Kalau tidak ada kerjaan → Berkeliling random

---

## 🔧 TROUBLESHOOTING

**Q: Agent tidak bergerak ke Crop?**
- A: Pastikan Crop adalah GameObject terpisah, bukan child atau component dari Farmer!

**Q: Tanaman terlalu cepat tumbuh?**
- A: Ubah `Growth Time Per Stage` di CropBehaviour Inspector (nilai lebih besar = lebih lambat)

**Q: Agent tidak ambil bibit/ember?**
- A: Pastikan tag Storage dan WaterSource sudah benar

**Q: Agent idle terus?**
- A: Cek NPCStats di Inspector, pastikan semua inventory = 0 di awal

**Q: Error "GoapRunner not found"?**
- A: Pastikan FarmerAgentTypeFactory sudah ditambah di GoapRunner GameObject

---

## 📊 MONITORING

Gunakan `NPCDebugDisplay` untuk melihat:
- Current Goal
- Current Action
- Hunger / Energy
- Food Count
- Inventory (Seed, WateringCan, Shovel)

Atau lihat di Console untuk log action.

---

## 🎯 TIPS

- **Atur kecepatan tumbuh**: Ubah `Growth Time Per Stage` di Crop Inspector
- **Atur inventory awal**: Kalau mau test cepat, set `Has Seed = 5` di NPCStats
- **Atur hunger/energy rate**: Ubah di NPCStats → Passive Rates

**Selamat mencoba! 🎉**
