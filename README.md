# 🛡️ Procto — Secure Exam Environment

<div align="center">

![Procto Banner](https://img.shields.io/badge/Procto-Secure%20Exam%20Browser-4a90d9?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet)
![CefSharp](https://img.shields.io/badge/CefSharp-120.1.110-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D4?style=for-the-badge&logo=windows)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

**Procto** adalah aplikasi browser aman berbasis Chromium (CefSharp) untuk pelaksanaan ujian digital di lingkungan sekolah dan institusi pendidikan.  
Dirancang untuk mencegah kecurangan dengan mengunci akses perangkat selama ujian berlangsung.

</div>

---

## ✨ Fitur Utama

| Fitur                    | Deskripsi                                                                        |
| ------------------------ | -------------------------------------------------------------------------------- |
| 🔒 **Lockdown Mode**     | Memblokir Alt+Tab, Alt+F4, Win Key, Task Manager, dan shortcut berbahaya lainnya |
| 🌐 **Browser Aman**      | Berbasis Chromium (CefSharp) — mendukung HTML5, JavaScript, CSS3 penuh           |
| 📋 **URL Filtering**     | Membatasi akses hanya ke URL yang diizinkan sesuai konfigurasi                   |
| ☀️ **Kontrol Kecerahan** | Slider pengatur kecerahan layar real-time (15%–100%)                             |
| 🔊 **Kontrol Volume**    | Pengaturan volume sistem terintegrasi langsung di toolbar                        |
| 🖥️ **Splash Screen**     | Layar pembuka animasi dengan verifikasi status sistem                            |
| 🔑 **Password Keluar**   | Pengajar dapat mengunci tombol keluar dengan password                            |
| 📁 **Konfigurasi JSON**  | Semua pengaturan ujian dikonfigurasi via file `default.safeexam.json`            |
| 📝 **Logging**           | Log sesi lengkap tersimpan otomatis di folder `logs/`                            |

---

## 📸 Tampilan Aplikasi

```
┌─────────────────────────────────────────────────────────┐
│ 🛡️ Procto   • Ujian Aktif           🕐 10:30   [Keluar] │  ← Status Bar
├─────────────────────────────────────────────────────────┤
│                                                         │
│                  [ Browser Ujian ]                      │  ← Area Chromium
│                  https://ujian.sekolah.id               │
│                                                         │
├─────────────────────────────────────────────────────────┤
│ [🏠] [🔄]                    ☀️━━━●━━ 80%  🔊━━●━━ 70% │  ← Toolbar
└─────────────────────────────────────────────────────────┘
```

---

## ⚙️ Konfigurasi

Buat atau edit file `config/default.safeexam.json`:

```json
{
  "StartUrl": "https://ujian.sekolah.id",
  "BrowserTitle": "Ujian Semester Genap 2025",
  "QuitPassword": "admin123",
  "AllowClipboard": false,
  "AllowPrint": false,
  "AllowedUrls": ["https://ujian.sekolah.id/*", "https://cdn.sekolah.id/*"],
  "ForbiddenProcesses": ["chrome.exe", "firefox.exe", "notepad.exe"]
}
```

### Penjelasan Parameter

| Parameter            | Tipe   | Default              | Deskripsi                                      |
| -------------------- | ------ | -------------------- | ---------------------------------------------- |
| `StartUrl`           | string | `https://google.com` | URL yang dibuka saat aplikasi dimulai          |
| `BrowserTitle`       | string | `Procto`             | Judul yang ditampilkan di status bar           |
| `QuitPassword`       | string | `admin`              | Password untuk keluar dari mode ujian          |
| `AllowClipboard`     | bool   | `false`              | Izinkan copy-paste                             |
| `AllowPrint`         | bool   | `false`              | Izinkan cetak halaman                          |
| `AllowedUrls`        | array  | `[]`                 | Daftar URL yang diizinkan (kosong = semua URL) |
| `ForbiddenProcesses` | array  | `[]`                 | Proses yang otomatis ditutup saat ujian        |

---

## 🚀 Cara Build

### Prasyarat

- **Windows 10/11** (64-bit atau 32-bit)
- **[.NET 9 SDK](https://dotnet.microsoft.com/download)** — untuk proses compile
- **Visual C++ Redistributable 2015–2022** — sudah tersedia di Windows 10/11 modern

> ⚠️ Komputer **peserta ujian** tidak perlu instalasi .NET apapun karena output adalah **self-contained executable**.

### Build via Script (Rekomendasi)

Double-click file `build-custom.bat` dan ikuti menu interaktifnya:

```
 =============================================
   PROCTO  -  Build Script v2.0
 =============================================

 Pilih Arsitektur:
   1.  x64 - 64-bit  [Rekomendasi]
   2.  x86 - 32-bit

 Pilih Format Output:
   1.  Single EXE  - Satu file Procto.exe  [Rekomendasi]
   2.  Standard    - Banyak file DLL dalam folder
```

File hasil build tersimpan di:

```
publish/
└── win-x64/
    ├── Procto.exe          ← File utama (~430 MB all-in-one)
    └── config/
        └── default.safeexam.json
```

### Build via Command Line

```bat
dotnet publish SafeExamCEF\SafeExamCEF.csproj ^
    -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeAllContentForSelfExtract=true ^
    -o publish\win-x64
```

---

## 📦 Distribusi ke Komputer Siswa

Cukup salin **dua item** ke setiap komputer:

```
📁 Folder Ujian/
├── 📄 Procto.exe             ← Aplikasi (copy ke semua komputer)
└── 📁 config/
    └── 📄 default.safeexam.json  ← Konfigurasi ujian
```

Jalankan `Procto.exe` — tidak perlu instalasi apapun.

---

## 🏗️ Struktur Proyek

```
safeexam/
├── SafeExamCEF/                  # Source code utama
│   ├── App.xaml(.cs)             # Entry point WPF
│   ├── Program.cs                # Custom bootstrap CefSharp
│   ├── MainWindow.xaml(.cs)      # Jendela utama + browser
│   ├── SplashScreen.xaml(.cs)    # Layar pembuka
│   ├── QuitDialog.xaml(.cs)      # Dialog keluar + password
│   ├── AlertDialog.xaml(.cs)     # Dialog peringatan
│   ├── LockdownEngine.cs         # Blokir keyboard & proses
│   ├── KeyboardHook.cs           # Global keyboard hook
│   ├── UrlFilterHandler.cs       # Filter URL browser
│   ├── VolumeController.cs       # Kontrol volume sistem
│   ├── ProcessMonitor.cs         # Monitor & tutup proses terlarang
│   ├── ConfigManager.cs          # Baca & validasi konfigurasi
│   └── ExamConfig.cs             # Model konfigurasi
├── publish/                      # Output build
├── build-custom.bat              # Script build interaktif
├── update-nuget.bat              # Update NuGet packages
└── default.safeexam.json         # Konfigurasi default
```

---

## 🔧 Teknologi

| Komponen                 | Versi     | Keterangan                |
| ------------------------ | --------- | ------------------------- |
| **.NET**                 | 6.0       | Framework aplikasi        |
| **WPF**                  | 6.0       | UI framework              |
| **CefSharp.Wpf.NETCore** | 120.1.110 | Browser Chromium embedded |
| **Serilog**              | 3.1.1     | Logging                   |
| **Newtonsoft.Json**      | 13.0.3    | Parsing konfigurasi JSON  |

---

## 🔒 Mekanisme Keamanan

```
Saat ujian aktif:
  ✅ Keyboard hook global (blokir Win, Alt+Tab, Alt+F4, Ctrl+Esc, dll.)
  ✅ Proses terlarang otomatis ditutup setiap 5 detik
  ✅ URL filtering — akses hanya ke domain yang diizinkan
  ✅ Jendela selalu Topmost — tidak bisa disembunyikan
  ✅ Clipboard dinonaktifkan (opsional via konfigurasi)
  ✅ Klik kanan dan menu konteks dinonaktifkan
  ✅ Tombol keluar memerlukan password pengajar
```

---

## 📋 Changelog

### v2.0 (Maret 2026)

- ✨ Rebranding dari SafeExamCEF ke **Procto**
- ✨ Splash Screen dengan animasi verifikasi sistem
- ✨ Icon SVG Path untuk toolbar (tanpa ketergantungan emoji)
- ✨ Kontrol kecerahan layar dengan overlay gelap yang lebih akurat
- 🐛 Fix: Inisialisasi CefSharp dipindah ke `Program.cs` (mencegah deadlock)
- 🐛 Fix: Null check pada `_brightnessTimer` saat `InitializeComponent`
- 🐛 Fix: Build script menggunakan `goto` label (menghindari bug kurung di `if-else`)

### v1.0 (Awal 2026)

- 🎉 Rilis perdana SafeExamCEF

---

## 👨‍💻 Developer

<div align="center">

**Edward Torangga**  
💼 Developer & Sistem Administrator  
📧 GitHub: [@edwardtorangga911](https://github.com/edwardtorangga911)

</div>

---

## 📄 Lisensi

Proyek ini dilisensikan di bawah **MIT License** — bebas digunakan, dimodifikasi, dan didistribusikan dengan tetap mencantumkan kredit ke pengembang asli.

---

<div align="center">

Dibuat dengan ❤️ untuk dunia pendidikan Indonesia 🇮🇩

</div>
