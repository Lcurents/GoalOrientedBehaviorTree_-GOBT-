# 🌾 Farming GOAP - Panduan Setup Unity Scene

## 📁 Struktur File yang Sudah Dibuat

```
Farming/
├── WorldKeys/          (5 files) ✅
├── TargetKeys/         (4 files) ✅
├── Goals/              (5 files) ✅
├── Actions/            (5 files) ✅
├── Sensors/
│   ├── World/         (5 sensors) ✅
│   └── Target/        (3 sensors) ✅
├── Capabilities/       (1 factory) ✅
├── AgentTypes/         (1 factory) ✅
├── Brain/              (1 brain) ✅
└── Behaviours/         (2 behaviours) ✅
```

---

## 🎮 STEP 1: Setup GOAP System di Scene

### 1.1 Buat GameObject "GoapRunner"
1. **Hierarchy** → Klik kanan → `Create Empty`
2. Rename menjadi `GoapRunner`
3. **Add Component** → `GoapBehaviour` (dari package Crashkonijn)
4. **Add Component** → `Reactive Controller` (WAJIB! Pilih salah satu controller)
   - ReactiveController (recommended untuk pemula)
   - ProactiveController (lebih advanced)
   - ManualController (full manual control)
5. **Add Component** → `Farmer Agent Type Factory` (script kita)

**Urutan component di GoapRunner:**
```
GoapRunner
├─ Transform
├─ Goap Behaviour
├─ Reactive Controller  ← PENTING! Harus ada!
└─ Farmer Agent Type Factory
```

**CATATAN**: 
- Controller WAJIB ada, tanpa ini akan error
- GoapBehaviour akan otomatis detect FarmerAgentTypeFactory
- Jangan drag file .cs, harus add component!

---

## 🧑‍🌾 STEP 2: Setup NPC Farmer

### 2.1 Buat GameObject "Farmer"
1. **Hierarchy** → Klik kanan → `Create Empty`
2. Rename menjadi `Farmer`
3. **Position**: (0, 0, 0) atau sesuai keinginan

### 2.2 Tambahkan Visual (Sprite)
1. Klik `Farmer` → Add Component → `Sprite Renderer`
2. Assign sprite karakter dari folder `Art/Cute_Fantasy/NPCs (Premade)` atau `Player`
3. Set **Order in Layer** = 10 (agar di atas background)

### 2.3 Tambahkan Collider & Rigidbody
1. Add Component → `Capsule Collider 2D` (atau `Box Collider 2D`)
2. Add Component → `Rigidbody 2D`
   - **Body Type**: Dynamic
   - **Gravity Scale**: 0 (karena 2D top-down)
   - **Constraints**: Freeze Rotation Z (centang)

### 2.4 Tambahkan GOAP Components (PENTING!)
**Add Component** satu per satu:
1. `AgentBehaviour` (dari Crashkonijn)
2. `GoapActionProvider` (dari Crashkonijn)
3. `AgentMoveBehaviour` (dari Crashkonijn)

**PENTING - Jangan Isi Field di Inspector!**
- FarmerBrain akan otomatis set semua connection via code di Awake():
  - Link AgentBehaviour ↔ GoapActionProvider
  - Set AgentType ke "FarmerAgent"
- Warning di Inspector bisa diabaikan, akan terisi saat Play

### 2.5 Tambahkan Custom Scripts
**Add Component:**
1. `NPCStats` (script kita)
2. `FarmerBrain` (script kita)

**JANGAN ADD CropBehaviour di Farmer!** - Crop adalah GameObject terpisah di scene

**Konfigurasi NPCStats:**
- Hunger: 0
- Energy: 100
- Food Count: 0
- Hunger Increase Rate: 5
- Energy Decrease Rate: 3

---

## 🌱 STEP 3: Setup Crop (Tanaman)

**PENTING: Crop adalah GameObject TERPISAH di scene, BUKAN child dari Farmer!**

### 3.1 Buat GameObject "Crop" di Scene
1. **Hierarchy** → Klik kanan → `Create Empty`
2. Rename menjadi `Crop`
3. **Position**: (3, 0, 0) → Beberapa unit dari Farmer
4. **JANGAN** jadikan child dari Farmer!

### 3.2 Setup Crop Components
1. **Add Component** → `CropBehaviour` (script kita)
2. **Add Component** → `Sprite Renderer`
3. **Add Component** → `Circle Collider 2D` (untuk detection)
   - **Radius**: 0.5
   - **Is Trigger**: Centang (✓)

### 3.3 Konfigurasi CropBehaviour
Di Inspector, `CropBehaviour` component:
- **Growth Stage**: 0 (empty soil)
- **Crop Sprite**: Drag Sprite Renderer dari component di bawah
- **Growth Sprites** (Size: 4):
  - [0]: Sprite tanah kosong (soil)
  - [1]: Sprite benih tertanam (seed)
  - [2]: Sprite tanaman tumbuh (growing)
  - [3]: Sprite tanaman matang (ready harvest)

**Struktur Scene:**
```
Hierarchy
├─ GoapRunner
├─ Farmer (NPC)
├─ Crop          ← GameObject terpisah!
├─ Bed
└─ Storage
```

---

## 🏠 STEP 4: Setup Objects di Scene

### 4.1 Buat GameObject "Bed" (Tempat Tidur)
1. Create Empty → Rename: `Bed`
2. **Tag**: Buat tag baru `Bed` dan assign
3. **Position**: (-5, 0, 0) atau lokasi lain
4. (Optional) Add Sprite Renderer untuk visual tempat tidur

### 4.2 Buat GameObject "Storage" (Penyimpanan Makanan)
1. Create Empty → Rename: `Storage`
2. **Tag**: Buat tag baru `Storage` dan assign
3. **Position**: (5, 0, 0) atau lokasi lain
4. (Optional) Add Sprite Renderer untuk visual storage/chest

### 4.3 Tag Setup (WAJIB!)
**Unity Menu** → `Edit` → `Project Settings` → `Tags and Layers`
- Add Tag: `Bed`
- Add Tag: `Storage`

Lalu assign tag ke GameObject yang sesuai.

---

## 🎬 STEP 5: Final Configuration

### 5.1 Setup AgentMoveBehaviour (Movement)
Di `Farmer` GameObject → `AgentMoveBehaviour` component:
- **Move Speed**: 3
- **Stop Distance**: 0.5

### 5.2 Test Brain Reference
Di `Farmer` → `FarmerBrain` component:
- Seharusnya otomatis detect semua component (Awake)
- Pastikan titidak ada field yang perlu diisi di Inspector
- AgentType akan di-set otomatis saat Awake()
- Tekan Play, cek Console - seharusnya tidak ada error "AgentType not found"
---

## ▶️ STEP 6: Testing

### 6.1 Play Mode
1. Tekan **Play**
2. Lihat Console untuk debug log:
   ```
   [PlantCropAction] Tanaman ditanam!
   [WaterCropAction] Tanaman disiram! Stage: 2
   [HarvestCropAction] Panen berhasil! Food: 1
   [EatAction] Makan! Hunger: 20, Food: 0
   [SleepAction] NPC mulai tidur...
   ```

### 6.2 Debug Inspector
Saat Play, lihat `Farmer` → `NPCStats`:
- **Hunger** akan naik seiring waktu
- **Energy** akan turun seiring waktu
- **Food Count** bertambah saat harvest

### 6.3 GOAP Graph Viewer (Optional)
1. Window → GOAP → Graph Viewer
2. Pilih `Farmer` di Hierarchy
3. Lihat action graph real-time

---

## 🐛 Troubleshooting

### ❌ Error: "No IGoapController found"
**Solusi:** 
- GoapRunner HARUS punya Controller component!
- Add Component → `Reactive Controller` ke GoapRunner
- Controller WAJIB ada untuk GOAP system berfungsi

### ❌ Error: "No ActionReceiver" atau "No ActionProvider"
**Solusi:**
- FarmerBrain sudah handle ini via code: `agent.ActionProvider = provider;`
- Pastikan FarmerBrain component terpasang di Farmer GameObject
- Jangan set ActionProvider manual di Inspector
- Error ini muncul jika FarmerBrain.Awake() tidak jalan

### ❌ Error: "NullReferenceException" di Brain
**Solusi:** Pastikan semua component terpasang:
- AgentBehaviour
- GoapActionProvider
- AgentMoveBehaviour
- NPCStats
- CropBehaviour

### ❌ NPC tidak bergerak
**Solusi:**
- Cek `AgentMoveBehaviour` sudah terpasang
- Cek `Rigidbody2D` → Body Type = Dynamic
- Cek target (Bed, Storage) sudah ada Tag

### ❌ Crop tidak grow
**Solusi:**
- Cek `CropBehaviour` di Inspector
- Pastikan `growthSprites` array terisi 4 sprite

### ❌ GOAP tidak resolve action
**Solusi:**Component `FarmerAgentTypeFactory` sudah terpasang
- Cek `GoapBehaviour` → Agent Type Config Factories → Element 0 terisi
- Cek Console saat Play: "AgentType 'FarmerAgent' registered"
- Restart Play Mode atau restart Unity→ `FarmerAgentTypeFactory` terdaftar
- Restart Play Mode
- Lihat Console untuk error

---

## 📊 Flow Kerja NPC

```
Start (Hunger:0, Energy:100, Crop:0, Food:0)
  ↓
PlantingGoal → PlantCropAction (Crop → 1)
  ↓
WateringGoal → WaterCropAction (Crop → 2)
  ↓
WateringGoal → WaterCropAction (Crop → 3)
  ↓
HarvestingGoal → HarvestCropAction (Crop → 0, Food → 1)
  ↓
[Hunger naik > 70]
  ↓
EatGoal → EatAction (Hunger → 20, Food → 0)
  ↓
[Energy turun < 30]
  ↓
SleepGoal → SleepAction (Energy → 80+)
  ↓
Loop back to PlantingGoal...
```

---

## 🎨 Customization (Opsional)

### Ubah Stats Rate
Edit `NPCStats` → Inspector:
- **Hunger Increase Rate**: Semakin besar = cepat lapar
- **Energy Decrease Rate**: Semakin besar = cepat lelah

### Ubah Action Cost
Edit `FarmingCapabilityFactory.cs`:
```csharp
builder.AddAction<PlantCropAction>()
    .SetBaseCost(5) // Ubah cost di sini
```

### Tambah Visual Feedback
Di masing-masing Action class, tambahkan di `Start()`:
```csharp
public override void Start(IMonoAgent agent, Data data)
{
    // Ubah warna sprite saat action
    var sprite = agent.GetComponent<SpriteRenderer>();
    sprite.color = Color.yellow;
}
```

---

## ✅ Checklist Setup

- [ ] GoapRunner dengan 3 components: GoapBehaviour + ReactiveController + FarmerAgentTypeFactory
- [ ] Farmer GameObject dengan 3 GOAP + 2 custom scripts (NO CropBehaviour!)
- [ ] Crop GameObject TERPISAH dengan CropBehaviour + Collider2D + 4 sprites
- [ ] Bed GameObject dengan Tag "Bed"
- [ ] Storage GameObject dengan Tag "Storage"
- [ ] Tags dibuat di Project Settings
- [ ] Rigidbody2D settings: Dynamic, Gravity 0, Freeze Rotation Z
- [ ] Farmer dan Crop tidak parent-child, terpisah di scene!
- [ ] Play mode berjalan tanpa error
- [ ] Console menampilkan log action

---

## 🎓 Next Steps

Setelah basic system berjalan:
1. Tambahkan multiple crops
2. Buat NPC bisa pilih crop terdekat
3. Tambahkan inventory system
4. Implementasi day/night cycle
5. Tambahkan lebih banyak NPC dengan shared world state

**Selamat mencoba! 🚀**
