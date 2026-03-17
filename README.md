# 🌾 GOAP + BT + Utility Farming Simulator

Proyek ini adalah sebuah **Simulator Pertanian (Farming Simulator)** berbasis kecerdasan buatan (AI) yang menggunakan tiga sistem pengambilan keputusan sekaligus untuk mengatur perilaku para Petani (Agen).

Sistem dibuat sedemikian rupa agar simulasi terlihat senyata mungkin: agen bisa merasa lapar, lelah, dan harus bekerja sama mengurus lahan perkebunan tanpa saling berebut.

---

## 🧠 3 Otak di Balik Para Petani

Petani dalam simulasi ini tidak bergerak menggunakan skrip kaku, melainkan "berpikir" sendiri menggunakan gabungan 3 sistem:

### 1. Behavior Tree (BT) - Si "Manajer Tugas" 📂
Ini adalah peta jalan utama bagi setiap agen. BT berfungsi memilih "**Apa prioritas hidup hari ini?**"
- **Contoh:** "Apakah aku sangat lapar sampai mau mati? Jika ya, cari makan. Jika tidak, coba lihat apakah ada lahan yang belum ditanam."
- BT membantu agen berpindah dari urusan bertahan hidup (Survival) ke urusan bekerja (Farming).

### 2. Utility Theory - Si "Kalkulator Keuntungan" 🧮
Di simulasi ini, agen memiliki kepribadian (Personality). Ada yang rajin, ada yang malas, dsb. **Utility** adalah nilai/skor yang dihitung untuk menentukan seberapa untung melakukan suatu pekerjaan.
- **Cara Kerja:** Skor Pekerjaan = (Keuntungan Pekerjaan) - (Tenaga yang Dikeluarkan) - (Rasa Lapar yang Ditimbulkan).
- **Contoh:** Memanen hasil bumi memberikan keuntungan besar, jadi skornya sangat tinggi. Namun, jika jaraknya terlalu jauh dan agen sedang agak lelah, skornya akan ditekan sehingga agen lebih memilih menyiram tanaman yang dekat dengannya saja.

### 3. GOAP (Goal-Oriented Action Planning) - Si "Perencana Langkah" 🗺️
Setelah BT dan Kalkulator memutuskan tujuan akhirnya apa (Contoh: "Aku mau Makan" atau "Aku mau Menanam"), GOAP mengambil alih.
- GOAP adalah sistem yang menyusun **langkah-langkah konkret mundur** dari Tujuan ke Keadaan Sekarang.
- **Contoh Goal:** "Menanam Bibit"
- **Rencana GOAP:** "Aku butuh sekop dan bibit. Aku ada di dekat bibit, jadi: Berjalan ke Gudang Sekop → Ambil Sekop → Berjalan ke Gudang Bibit → Ambil Bibit → Jalan ke Tanah Kosong → Tanam (Cepat)."

---

## 🎯 Daftar Tujuan Utama (Goals)

Goal adalah ambisi akhir yang ingin diraih oleh sistem AI. Berikut daftar Goal dalam simulator ini:

1. **EatGoal (Tujuan Makan)** - Tercapai jika rasa Lapar di bawah 30% dan ada makanan tersisa. (Prioritas Tertinggi Survival)
2. **SleepGoal (Tujuan Tidur)** - Tercapai jika Energi tubuh kembali naik lebih dari 80%. (Prioritas Kedua Survival)
3. **HarvestingGoal (Tujuan Memanen)** - Tujuan untuk mengubah tanaman yang sudah matang menjadi Makanan.
4. **WateringGoal (Tujuan Menyiram)** - Tujuan untuk menyiram tanaman yang sedang tumbuh agar tidak menghalangi fase berikutnya.
5. **PlantingGoal (Tujuan Menanam)** - Tujuan untuk mengisi tanah kosong dengan Bibit.

---

## 🏃‍♂️ Daftar Pekerjaan (Actions)

Untuk memenuhi daftar Ambisi (Goals) di atas, agen harus mengeksekusi aksi-aksi berikut secara logis:

### Aksi Pertanian (Farming Actions)
- **GetSeedAction:** Mengambil bibit dari gudang/seed storage. (Biaya: 1 Langkah)
- **GetShovelAction:** Mengambil sekop dari fasilitas. (Biaya: 1 Langkah)
- **GetWateringCanAction:** Mengambil alat penyiram tanaman. (Biaya: 2 Langkah)
- **PlantSeedFastAction (Tanam Cepat):** Menanam bibit menggunakan sekop. Cuma butuh 2 detik, tapi mensyaratkan agen punya Sekop & Bibit. (Biaya: 1 Langkah)
- **PlantSeedSlowAction (Tanam Lambat):** Menanam tanpa sekop. Butuh 5 detik dan menguras lebih banyak tenaga. Diaktifkan jika sekop sedang habis dipinjam. (Biaya: 3 Langkah)
- **WaterCropAction:** Menyiram bibit agar bisa terus bertumbuh. Otomatis membuang kepemilikan Ember (harus ambil lagi). (Biaya: 2 Langkah)
- **HarvestCropAction:** Mencabut tanaman yang matang dan memasukkannya ke cadangan makanan bersama (Shared Food). (Biaya: 3 Langkah)

### Aksi Bertahan Hidup (Survival Actions)
- **EatAction:** Makan hasil panen dari gudang untuk mengurangi persentase Rasa Lapar.
- **SleepAction:** Kembali ke kasur dan memulihkan Stamina/Energi per detik.

---

## 🤝 Multi-Agent System (Sistem Lelang / Bidding)

Jika ada 3 petani dan hanya 1 lahan kosong, bagaimana agar tidak berebut? Proyek ini menggunakan arsitektur pintar bergaya lelang. Fitur ini sepenuhnya dikontrol oleh script komandan utama: `CropManager.cs` bersama `SelectFarmingGoalMultiAgent.cs`

1. **Evaluasi Massal:** Saat bangun atau selesai bekerja, semua agen melihat tanaman di sekitar (yang belum di-booking agen lain).
2. **Kalkulasi & Bidding:** Setiap agen menghitung untung-rugi (Utility Scoring). Jika agen A dekat dengan Lahan 1, nilai untungnya lebih besar. Ia "mengajukan penawaran lelang" ke CropManager atas Lahan 1.
3. **Lelang Diputuskan:** CropManager melihat semua tiket dari para agen. Jika Agen A dan B kebetulan berebut Lahan 1, CropManager membandingkan skor Utility mereka (dan jarak). Pemenang mendapatkan lahan. Yang kalah akan diarahkan untuk mencoba melirik tugas lahan yang lain.
4. Ini mencegah kekacauan para Agent numpuk pada titik atau lokasi yang sama.

---

## 🧪 Bagaimana Parameter Utility Bekerja? (Sistem Kepribadian / Personality)

Pada file `NPCStats`, Anda bisa mengisi angka *Weight* tersendiri untuk setiap agen. Hal ini melahirkan kepribadian unik:

- `WeightEnergy = 0.5` dan `WeightHunger = 0.1` : Agen ini super pemalas. Dia benci pekerjaan berat seperti memanen karena sangat menguras kalori / energinya. Dia lebih suka tugas me-nyiram.
- `WeightHunger = 0.5` : Agen rakus. Dia cepat lapar sehingga sering berhenti kerja demi fokus mengambil hasil panen ke meja makan.

**Rumus Utama Jantung Utility:**
`Utility Skor Akhir = (Benefit/Keuntungan Pekerjaan) - (Potongan Tenaga) - (Potongan Rasa Lapar)` + `(Bonus Jarak bila tanaman sangat dekat)`.

---

## 🛠 Panduan Cepat

1. **Jalankan Proyek**: Buka scene utama, dan tekan tombol **Play** di Unity.
2. **Lihat Panel Console**: Di konsol log Unity, Anda akan melihat barisan riwayat transparan (transparency) tentang apa yang dipikirkan para agen (contoh: "*Agen A memenangkan Lelang Memanen Bawang dengan Utility 0.655*").
3. **Rubah Kepribadian Agen**: Klik salah satu Game Object Farmer. Buka jendela *Inspector*. Cari modul `NPCStats` dan ubah nilai `goalBenefitHarvesting` atau parameter lainnya di waktu *runtime* untuk melihat langsung implikasinya terhadap keputusan mereka di lapangan (seketika).
