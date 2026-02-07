# 🔧 FIX - Multi-Agent GOAP Setup Error

## ❌ Masalah Yang Diperbaiki

```
KeyNotFoundException: No agentType with id FarmerAgent found
GoapException: There is no AgentType assigned to the agent 'Farmer A'!
```

## ✅ Solusi Yang Diimplementasikan

### 1. **FarmerBrain.cs** - Inisialisasi Yang Lebih Robust

**Perubahan Utama:**
- ✅ Memindahkan assignment AgentType dari `Awake()` ke `Start()`
- ✅ Menambahkan error handling yang lebih baik
- ✅ Menambahkan auto-fix dengan menambahkan FarmerAgentTypeFactory jika belum ada
- ✅ Menambahkan flag `agentTypeAssigned` untuk mencegah runtime errors

**Kode Baru:**
```csharp
private void AssignAgentType()
{
    // Cek GoapBehaviour ada
    // Pastikan FarmerAgentTypeFactory ada di GoapBehaviour GameObject
    // Assign AgentType dengan error handling
}
```

### 2. **GoapSetupHelper.cs** - Script Helper Baru (OPSIONAL)

Script helper yang bisa ditambahkan ke GoapRunner GameObject untuk memastikan setup benar.

**Fitur:**
- Berjalan sebelum script lain (`DefaultExecutionOrder(-100)`)
- Auto-add FarmerAgentTypeFactory jika belum ada
- Verifikasi ReactiveController ada (warning jika tidak ada, harus ditambah manual)
- Verifikasi AgentType terdaftar dengan benar

---

## 🎮 CARA PERBAIKI DI SCENE

### Solusi Otomatis (Recommended)

Kode sudah diperbaiki untuk **AUTO-FIX** masalah ini. Tapi untuk hasil terbaik:

1. **Buka Scene Anda**
2. **Cari GameObject dengan GoapBehaviour** (biasanya bernama "GoapRunner")
3. **Pastikan ada 3 components**:
   - ✓ `GoapBehaviour`
   - ✓ `ReactiveController`
   - ✓ `FarmerAgentTypeFactory`

### Jika Belum Ada:

**Opsi A: Tambahkan Manual**
1. Pilih GameObject GoapRunner
2. Add Component → `ReactiveController` (WAJIB - dari Inspector)
3. Add Component → `FarmerAgentTypeFactory`

**Opsi B: Biarkan Auto-Fix**
- Kode sekarang akan otomatis menambahkan component yang hilang
- Lihat Console untuk log: `[FarmerBrain] ✓ Successfully assigned...`

### Opsi C: Gunakan GoapSetupHelper (Extra Safety)
1. Pilih GameObject GoapRunner
2. Add Component → `GoapSetupHelper`
3. Play - script akan otomatis setup semua yang diperlukan

---

## 📊 Struktur Scene Yang Benar

```
Hierarchy:
├─ GoapRunner (GameObject)
│  ├─ GoapBehaviour (Component)
│  ├─ ReactiveController (Component)
│  ├─ FarmerAgentTypeFactory (Component)     ← PENTING!
│  └─ GoapSetupHelper (Component)            ← OPTIONAL
│
├─ Farmer A (GameObject)
│  ├─ AgentBehaviour (Component)
│  ├─ GoapActionProvider (Component)
│  │  └─ Agent Type: (akan diisi otomatis)  ← AUTO-ASSIGNED
│  ├─ FarmerBrain (Component)
│  ├─ NPCStats (Component)
│  └─ ... (components lain)
│
├─ Farmer B (GameObject)
│  └─ ... (sama seperti Farmer A)
│
└─ Farmer C (GameObject)
   └─ ... (sama seperti Farmer A)
```

---

## 🔍 Verifikasi Fix Berhasil

Setelah Play, lihat Console:

### ✅ Berhasil:
```
[GoapSetupHelper] ✓ FarmerAgentTypeFactory found
[GoapSetupHelper] ✓ ReactiveController found
[GoapSetupHelper] ✓ FarmerAgent AgentType registered successfully
[FarmerBrain] ✓ Successfully assigned FarmerAgent AgentType to Farmer A
[FarmerBrain] ✓ Successfully assigned FarmerAgent AgentType to Farmer B
[FarmerBrain] ✓ Successfully assigned FarmerAgent AgentType to Farmer C
```

### ❌ Masih Error:
```
[FarmerBrain] ✗ Failed to get FarmerAgent AgentType
[FarmerBrain] SOLUTION: Add 'FarmerAgentTypeFactory' component to 'GoapRunner' GameObject
```

**Solusi:** Tambahkan `FarmerAgentTypeFactory` ke GoapRunner GameObject secara manual.

---

## 🚀 Perubahan Behavior

### BEFORE (Error):
- Agent langsung crash saat Start
- Exception spam di Console
- Agent tidak bisa request goals

### AFTER (Fixed):
- Agent menunggu GOAP sistem selesai inisialisasi
- Auto-fix missing components
- Error handling yang lebih baik
- Agent bisa request goals dengan normal

---

## 📝 Technical Details

### Urutan Eksekusi (Execution Order)

1. **GoapSetupHelper.Awake()** (`-100`)
   - Memastikan FarmerAgentTypeFactory ada (auto-add jika tidak ada)
   - Memastikan ReactiveController ada (warning jika tidak ada - HARUS DITAMBAH MANUAL)

2. **FarmerBrain.Awake()** (default `0`)
   - Setup references (agent, provider, stats)
   - Link AgentBehaviour ↔ GoapActionProvider

3. **GoapBehaviour Initialization** (internal)
   - Register semua AgentTypeFactories (termasuk FarmerAgentTypeFactory)
   - Build GOAP graph

4. **FarmerBrain.Start()**
   - Assign AgentType ke provider
   - Request initial goal (jika code-based enabled)

### Mengapa Dipindah ke Start()?

`GetAgentType()` memerlukan GOAP graph sudah ter-build. Graph building terjadi antara `Awake()` dan `Start()`, jadi assignment harus di `Start()`.

---

## 🐛 Troubleshooting

### Error: "No agentType with id FarmerAgent found"

**Penyebab:** FarmerAgentTypeFactory belum di-add ke GoapRunner
**Solusi:** Add component `FarmerAgentTypeFactory` ke GameObject dengan `GoapBehaviour`

### Error: "There is no AgentType assigned to the agent"

**Penyebab:** Assignment gagal saat Start()
**Solusi:** Cek Console untuk error message detail. Pastikan GoapBehaviour ada di scene.

### Multiple Farmers Spam Error

**Penyebab:** Semua farmer mencoba assign AgentType, tapi factory belum ada
**Solusi:** Tambahkan `GoapSetupHelper` untuk setup sebelum farmer Awake()

---

## ✨ Summary

**Files Changed:**
1. ✅ `FarmerBrain.cs` - Refactored initialization
2. ✅ `GoapSetupHelper.cs` - New helper script (optional)
3. ✅ `WaterCropAction.cs` - Fixed missing ActionCompleted property
4. ✅ `HarvestCropAction.cs` - Fixed missing ActionCompleted property

**Scene Setup Required:**
- Ensure `FarmerAgentTypeFactory` is on GoapRunner GameObject
- Optionally add `GoapSetupHelper` for automatic setup

**Result:**
- ✅ Multi-agent setup works correctly
- ✅ No more KeyNotFoundException
- ✅ All farmers can be assigned FarmerAgent AgentType
- ✅ Goals can be requested normally
