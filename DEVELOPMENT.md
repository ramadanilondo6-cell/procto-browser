# SafeExamCEF — Saran Pengembangan Lebih Lanjut

## 1. Fitur Lockdown (Menyerupai Safe Exam Browser)

### 🔒 Keamanan Keyboard & Sistem
- [x] **Block `Alt+Tab`, `Alt+F4`** menggunakan `KeyboardHook` yang sudah ada — tambahkan `VK_F4` saat key `Alt` ditekan
- [x] **Block `Ctrl+Alt+Del`** — tidak bisa di-hook melalui cara biasa, gunakan `SystemParametersInfo(SPI_SETSCREENSAVERRUNNING)` atau registrasi SAS filter driver
- [x] **Block Task Switcher (Win+Tab)** — tambahkan VK_TAB saat Win ditekan ke dalam `KeyboardHook`
- [x] **Disable shortcut browser**: `Ctrl+R` (reload), `Ctrl+P` (print), `Ctrl+S` (save), `F5`, `F12` — implementasikan di `IKeyboardHandler.OnPreKeyEvent`

### 🛡️ Pemantauan Proses
- [x] Implementasikan **ProcessMonitor** dengan `System.Diagnostics.Process.GetProcesses()` di background `Timer` (setiap 2 detik)
- [x] Baca daftar proses terlarang dari `default.procto.json` → field `forbiddenProcesses`
- [x] Tampilkan `AlertDialog` jika proses terlarang ditemukan
- [x] Opsional: langsung hentikan ujian (tutup app) setelah N kali pelanggaran

### 🌐 Filter URL / Whitelist
- [x] Implementasikan `IRequestHandler.OnBeforeBrowse()` untuk memblokir navigasi ke URL di luar whitelist
- [x] Dukung wildcard domain: `*.example.com`
- [x] Baca daftar `allowedUrls` dari `default.procto.json`
- [x] Jika URL diblokir: tampilkan halaman peringatan sederhana (bukan error blank)

### 🔑 Config Key Header (Kompatibilitas SEB)
- [x] Hitung SHA-256 dari isi file konfigurasi saat startup
- [x] Sisipkan header `X-SafeExamBrowser-ConfigKeyHash: <hash>` ke setiap request HTTP melalui `IRequestHandler.GetResourceRequestHandler()`
- [x] Server ujian bisa memverifikasi bahwa request berasal dari SafeExamCEF

### 📋 Clipboard
- [x] Nonaktifkan clipboard paste (`Ctrl+V`) di dalam browser dengan memblokir shortcut di `OnPreKeyEvent`
- [x] Opsional: disable clipboard OS sepenuhnya saat ujian berlangsung, restore saat keluar

### 🖨️ Print
- [x] Blokir `Ctrl+P` dan print via CefSharp di `IKeyboardHandler`
- [x] Nonaktifkan print dari menu CefSharp jika ada

---

## 2. Konfigurasi (`default.procto.json`)
- [x] **Load config dari file** — implementasikan `ConfigManager.cs` menggunakan `System.Text.Json` (atau `Newtonsoft.Json`)
- [x] **Model config** (`ExamConfig.cs`) sudah dirancang — buat dan gunakan di `App.xaml.cs`
- [x] Pass `config.startUrl` ke `ChromiumWebBrowser.Address` saat startup (bukan hardcode)
- [x] Pass `config.browserTitle` ke `Window.Title`
- [x] Validasi field wajib pada startup; tampilkan error dan exit jika tidak valid

---

## 3. Pengembangan UI Sederhana

### 🖥️ Toolbar Atas (StatusBar)
[x] Tambahkan panel tipis di atas browser (tinggi ~36px):

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="36"/>     <!-- StatusBar -->
        <RowDefinition Height="*"/>      <!-- Browser -->
    </Grid.RowDefinitions>

    <!-- StatusBar -->
    <Border Grid.Row="0" Background="#1e1e2e">
        <DockPanel LastChildFill="False" Margin="12,0">
            <TextBlock DockPanel.Dock="Left"
                       Text="Ujian Nasional 2026"
                       Foreground="White"
                       VerticalAlignment="Center"
                       FontSize="13" FontWeight="SemiBold"/>
            <TextBlock DockPanel.Dock="Right"
                       x:Name="ClockText"
                       Foreground="#aaaacc"
                       VerticalAlignment="Center"
                       FontSize="12"/>
            <Button DockPanel.Dock="Right"
                    Content="Keluar Ujian"
                    Background="#e03e3e" Foreground="White"
                    Margin="0,4" Padding="12,4"
                    Click="BtnQuit_Click"/>
        </DockPanel>
    </Border>

    <!-- Browser -->
    <cef:ChromiumWebBrowser Grid.Row="1" x:Name="Browser"/>
</Grid>
```

- [x] Perbarui `ClockText` setiap detik menggunakan `DispatcherTimer`
- [x] Tombol "Keluar Ujian" membuka `QuitDialog` dengan input password

### 🚪 Dialog Keluar (QuitDialog)
[x] Dialog modal sederhana:

```
┌────────────────────────────┐
│   Keluar dari Ujian?       │
│                            │
│   Masukkan Password:       │
│   [__________________]     │
│                            │
│   [Batal]    [Konfirmasi]  │
└────────────────────────────┘
```

- [x] `Window.ShowDialog()` → blokir interaksi ke jendela utama sebelum dialog ditutup
- [x] Bandingkan input dengan `config.quitPassword`
- [x] Jika salah → tampilkan pesan "Password salah", log percobaan

### ⚠️ Dialog Peringatan (AlertDialog)
[x] Untuk notifikasi proses terlarang:

```
⚠ PERINGATAN
Proses terlarang terdeteksi: Wireshark.exe
Kejadian ini telah dicatat.

                          [OK]
```

### 🎨 Warna / Tema
| Elemen | Warna |
|---|---|
| StatusBar background | `#1e1e2e` (biru gelap) |
| StatusBar text | `#ffffff` / `#aaaacc` |
| Tombol Keluar | `#e03e3e` (merah) |
| Pesan peringatan | `#f5a623` (kuning-oranye) |
| Background halaman blokir | `#f0f0f4` (abu muda) |

---

## 4. Logging Aktivitas
- [x] Gunakan **Serilog** (sudah terinstall) → log ke file `logs/session_<tanggal>.log`
- [x] Event yang perlu dilog:
  - Sesi dimulai / selesai
  - URL yang dikunjungi
  - Shortcut keyboard yang diblokir
  - Proses terlarang yang terdeteksi
  - Percobaan keluar yang gagal

---

## 5. Distribusi & Deployment
- [x] **Publish sebagai self-contained** (tidak butuh .NET terinstall di PC peserta):
  ```bat
  dotnet publish SafeExamCEF\SafeExamCEF.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
  ```
- [x] Distribusikan file `.procto.json` bersama `.exe` ke folder yang sama
- [x] Opsional: Enkripsi file config menggunakan AES-256 agar peserta tidak bisa membacanya

---

## Prioritas Pengembangan (Rekomendasi Urutan)

| Prioritas | Fitur | Status |
|---|---|---|
| 🔴 Sangat Penting | Load config JSON + startUrl | ✅ Selesai |
| 🔴 Sangat Penting | QuitDialog dengan password | ✅ Selesai |
| 🔴 Sangat Penting | Block Alt+Tab, Win key | ✅ Selesai |
| 🟠 Penting | Filter URL (whitelist) | ✅ Selesai |
| 🟠 Penting | ProcessMonitor | ✅ Selesai |
| 🟡 Sedang | StatusBar UI + jam | ✅ Selesai |
| 🟡 Sedang | Serilog audit log | ✅ Selesai |
| 🟢 Opsional | Config Key Header (SHA-256) | ✅ Selesai |
| 🟢 Opsional | Publish self-contained | ✅ Selesai |
| 🟢 Opsional | Enkripsi file config | ✅ Selesai |
