# 🗺️ Procto — Rancangan Pengembangan (Roadmap)

> **Dokumen ini** berisi rencana pengembangan jangka pendek, menengah, dan panjang untuk aplikasi **Procto - Secure Exam Environment**.  
> Dipersiapkan oleh: **Edward Torangga** | Terakhir diperbarui: Maret 2026

---

## 📊 Status Saat Ini (v2.0)

```
Infrastruktur Dasar         ████████████████████  100% ✅
Browser Aman (CefSharp)     ████████████████████  100% ✅
Lockdown Keyboard           ████████████████████  100% ✅
Kontrol Kecerahan           ████████████████████  100% ✅
Kontrol Volume              ████████████████████  100% ✅
Konfigurasi JSON            ████████████████████  100% ✅
Logging Sesi                ████████████████████  100% ✅
Splash Screen               ████████████████████  100% ✅
Sistem Distribusi           ████████████░░░░░░░░   60% 🔄
Manajemen Multi-ujian       ░░░░░░░░░░░░░░░░░░░░    0% 📋
Panel Admin Web             ░░░░░░░░░░░░░░░░░░░░    0% 📋
```

---

## 🎯 Versi & Milestone

```
v1.0 ──── v2.0 ──── v2.1 ──── v2.5 ──── v3.0 ──── v3.5
 ✅         ✅        📋        📋        📋        📋
Rilis     Branding  Stabil   Fitur+   Enterprise  Cloud
```

---

## 🚀 v2.1 — Stabilisasi & Kenyamanan

**Target: April 2026** · Prioritas: 🔴 Tinggi

### Bug Fix & Stabilitas

- [ ] **Perbaikan startup CefSharp** — Handle error saat libcef.dll tidak terload dengan pesan yang lebih jelas
- [ ] **Watchdog Process** — Restart otomatis jika browser crash tanpa keluar dari lockdown mode
- [ ] **Graceful shutdown** — Pastikan semua hook keyboard dan proses monitor bersih saat keluar
- [ ] **Memory leak** — Audit penggunaan memori saat sesi ujian panjang (> 3 jam)

### UI/UX

- [ ] **Toast notification** — Notifikasi kecil di pojok layar (proses diblokir, URL diblokir, dll.)
- [ ] **Indikator status network** — Tampilkan ikon online/offline di status bar
- [ ] **Progress loading halaman** — Progress bar tipis di atas area browser saat halaman dimuat
- [ ] **Animasi transisi kecerahan** — Perhalus animasi overlay gelap agar tidak terasa kaku

### Keamanan

- [ ] **Screenshot prevention** — Blokir PrintScreen, Snipping Tool, dan OBS virtual camera
- [ ] **Clipboard monitor** — Log setiap percobaan paste selama ujian
- [ ] **Anti debug** — Deteksi dan blokir jika ada debugger yang mencoba attach

---

## ⚡ v2.5 — Penambahan Fitur Penting

**Target: Juli 2026** · Prioritas: 🟡 Menengah

### Manajemen Konfigurasi

- [ ] **Config Editor GUI** — Aplikasi kecil terpisah untuk membuat dan mengedit `default.safeexam.json` tanpa teks manual
- [ ] **Config Validator** — Cek konfigurasi sebelum ujian dimulai dan tampilkan peringatan jika ada kesalahan
- [ ] **Profile Ujian** — Simpan beberapa profil konfigurasi (UTS, UAS, Try Out, dll.)
- [ ] **Config Encryption** — Enkripsi file konfigurasi agar tidak bisa diedit peserta

### Monitoring Peserta

- [ ] **Activity Log Viewer** — Tampilan log real-time aktivitas peserta (URL dikunjungi, proses diblokir, dll.)
- [ ] **Screenshot periodik** — Ambil screenshot otomatis setiap N menit dan simpan lokal
- [ ] **Waktu aktif browser** — Log kapan peserta meminimalkan/memaksimalkan window (jika bypass terjadi)

### Navigasi & Browser

- [ ] **Tab terbatas** — Izinkan beberapa tab tapi batasi hanya ke domain tertentu
- [ ] **Tombol Back/Forward** — Tambahkan navigasi back/forward ke toolbar
- [ ] **Zoom control** — Slider zoom halaman browser (50%–150%)
- [ ] **Find in page** — Fitur Ctrl+F untuk pencarian dalam halaman (bisa dikonfigurasi on/off)

### Build & Distribusi

- [ ] **Auto-updater** — Komponen kecil yang cek versi terbaru dari server dan update otomatis
- [ ] **Installer NSIS/Inno Setup** — Buat installer `.exe` resmi yang install ke Program Files
- [ ] **Silent deploy** — Skrip PowerShell untuk deploy massal ke 30+ komputer via jaringan

---

## 🏢 v3.0 — Fitur Enterprise

**Target: Oktober 2026** · Prioritas: 🟢 Rendah (Jangka Menengah)

### Panel Admin (Aplikasi Terpisah)

- [ ] **Dashboard Admin** — Aplikasi WPF atau Web untuk memantau semua sesi ujian real-time
- [ ] **Manajemen Sesi** — Mulai / hentikan ujian dari admin untuk semua komputer sekaligus
- [ ] **Broadcast pesan** — Kirim pesan ke semua peserta (contoh: "Sisa 10 menit")
- [ ] **Remote unlock** — Buka lockdown dari jarak jauh jika peserta membutuhkan bantuan

### Jaringan & Sinkronisasi

- [ ] **Komunikasi LAN** — Procto client berkomunikasi dengan Procto Admin via UDP/TCP lokal
- [ ] **Sinkronisasi waktu** — Jam ujian sinkron dari server admin, bukan jam komputer peserta
- [ ] **Log terpusat** — Semua log dikirim ke server admin selama ujian berlangsung

### Integrasi Platform LMS

- [ ] **Google Forms integration** — Buka Google Forms dalam mode aman
- [ ] **Moodle integration** — Plugin untuk launch Procto langsung dari Moodle
- [ ] **Custom API** — REST API endpoint untuk integrasi dengan sistem ujian sekolah apapun

### Keamanan Lanjutan

- [ ] **Face detection (opsional)** — Deteksi apakah wajah peserta masih di depan layar (menggunakan webcam)
- [ ] **Multiple monitor detection** — Blokir atau blackout monitor sekunder
- [ ] **USB device monitoring** — Log atau blokir perangkat USB yang dicolok saat ujian

---

## ☁️ v3.5 — Edisi Cloud

**Target: 2027** · Prioritas: 🔵 Visi Jangka Panjang

### Backend Cloud

- [ ] **Server cloud** — Backend REST API (ASP.NET Core / Node.js) untuk manajemen ujian multi-sekolah
- [ ] **Database peserta** — Kelola data peserta, kelas, dan jadwal ujian dari cloud
- [ ] **Enkripsi soal** — Soal ujian dienkripsi di server, hanya terbuka saat sesi dimulai
- [ ] **Audit trail** — Rekam jejak lengkap setiap aksi peserta, tersimpan di cloud

### Multi-Platform (Jangka Panjang)

- [ ] **Procto untuk macOS** — Port aplikasi ke macOS menggunakan MAUI atau Avalonia UI
- [ ] **Procto Mobile** — Versi Android/iOS untuk ujian via tablet
- [ ] **Procto Linux** — Dukungan Ubuntu/Debian untuk sekolah berbasis Linux

---

## 🐞 Backlog Bug yang Diketahui

| ID      | Tingkat   | Deskripsi                                                                   | Status                         |
| ------- | --------- | --------------------------------------------------------------------------- | ------------------------------ |
| BUG-001 | 🔴 Tinggi | App stuck di "Initializing CefSharp..." jika DLL rusak                      | ✅ Fixed v2.0                  |
| BUG-002 | 🔴 Tinggi | NullReferenceException saat slider kecerahan dipanggil sebelum Timer dibuat | ✅ Fixed v2.0                  |
| BUG-003 | 🟡 Sedang | Build gagal jika Procto.exe sedang berjalan                                 | ✅ Fixed v2.0 (auto-kill)      |
| BUG-004 | 🟡 Sedang | Konfigurasi tidak ditemukan jika exe dijalankan dari direktori berbeda      | ✅ Fixed v2.0                  |
| BUG-005 | 🟢 Rendah | Warning NETSDK1138 (.NET 6.0 end-of-life)                                   | 📋 Pending (upgrade ke .NET 8) |
| BUG-006 | 🟢 Rendah | Warning NU1903 (CefSharp vulnerability)                                     | 📋 Pending (upgrade CefSharp)  |

---

## 💡 Ide & Saran dari Pengguna

> _Bagian ini akan diisi berdasarkan feedback guru dan administrator_

- [ ] **Mode Kiosk penuh** — Sembunyikan taskbar Windows sepenuhnya selama ujian
- [ ] **Tema gelap/terang** — Toggle tema tampilan aplikasi
- [ ] **Dukungan multi-bahasa** — UI dalam Bahasa Indonesia dan Inggris
- [ ] **Shortcut reset browser** — Tombol untuk kembali ke halaman awal tanpa menutup aplikasi
- [ ] **Batas waktu otomatis** — Aplikasi otomatis keluar setelah durasi ujian habis

---

## 🔧 Hutang Teknis (Technical Debt)

| Item                     | Prioritas | Keterangan                                                      |
| ------------------------ | --------- | --------------------------------------------------------------- |
| Upgrade ke .NET 8        | 🔴 Tinggi | .NET 6.0 sudah EOL sejak November 2024                          |
| Upgrade CefSharp ke 126+ | 🟡 Sedang | Perbaiki kerentanan keamanan NU1903                             |
| Unit Testing             | 🟡 Sedang | Belum ada test coverage untuk ConfigManager dan LockdownEngine  |
| Dependency Injection     | 🟢 Rendah | Refactor untuk mempermudah unit testing                         |
| Pisahkan UI dan Logic    | 🟢 Rendah | MainWindow.xaml.cs terlalu banyak tanggung jawab (MVVM pattern) |
| High DPI Config          | 🟢 Rendah | Pindahkan dari app.manifest ke ApplicationHighDpiMode di csproj |

---

## 📅 Timeline Ringkas

```
2026 Q1 (Jan-Mar)    2026 Q2 (Apr-Jun)    2026 Q3 (Jul-Sep)    2026 Q4 (Oct-Dec)
│                    │                    │                    │
├─ v2.0 ✅           ├─ v2.1 📋           ├─ v2.5 📋           ├─ v3.0 📋
│  • Rebranding      │  • Bug fix         │  • Config GUI      │  • Panel Admin
│  • Splash Screen   │  • Toast notif     │  • Activity log    │  • LAN sync
│  • SVG icons       │  • Screenshot blk  │  • Tab terbatas    │  • Face detect
│  • Brightness fix  │  • Watchdog proc   │  • Auto-updater    │  • Multi-monitor
```

---

## 🤝 Cara Berkontribusi

1. **Fork** repository ini
2. Buat branch fitur: `git checkout -b fitur/nama-fitur`
3. Commit perubahan: `git commit -m "Tambah: nama fitur"`
4. Push ke branch: `git push origin fitur/nama-fitur`
5. Buat **Pull Request** dengan deskripsi lengkap

### Standar Kode

- Gunakan **C# 10+** dengan nullable reference types
- Semua log menggunakan **Serilog** (bukan `Console.WriteLine`)
- Setiap fitur baru wajib punya **null check** dan **try-catch**
- Penamaan variabel dalam **Bahasa Inggris**, komentar boleh Bahasa Indonesia

---

<div align="center">

📬 Kontak & Diskusi: Buka **Issue** di GitHub  
👨‍💻 Maintainer: **[Edward Torangga](https://github.com/edwardtorangga911)**

</div>
