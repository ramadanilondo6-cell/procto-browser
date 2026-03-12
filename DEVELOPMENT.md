# SafeExamCEF — Saran Pengembangan Lebih Lanjut

## 1. Fitur Lockdown (Menyerupai Safe Exam Browser)

### 🔒 Keamanan Keyboard & Sistem
- [ ] **Block `Alt+Tab`, `Alt+F4`** menggunakan `KeyboardHook` yang sudah ada — tambahkan `VK_F4` saat key `Alt` ditekan
- [ ] **Block `Ctrl+Alt+Del`** — tidak bisa di-hook melalui cara biasa, gunakan `SystemParametersInfo(SPI_SETSCREENSAVERRUNNING)` atau registrasi SAS filter driver
- [ ] **Block Task Switcher (Win+Tab)** — tambahkan VK_TAB saat Win ditekan ke dalam `KeyboardHook`
- [ ] **Disable shortcut browser**: `Ctrl+R` (reload), `Ctrl+P` (print), `Ctrl+S` (save), `F5`, `F12` — implementasikan di `IKeyboardHandler.OnPreKeyEvent`

### 🛡️ Pemantauan Proses
- [ ] Implementasikan **ProcessMonitor** dengan `System.Diagnostics.Process.GetProcesses()` di background `Timer` (setiap 2 detik)
- [ ] Baca daftar proses terlarang dari `default.safeexam.json` → field `forbiddenProcesses`
- [ ] Tampilkan `AlertDialog` jika proses terlarang ditemukan
- [ ] Opsional: langsung hentikan ujian (tutup app) setelah N kali pelanggaran

### 🌐 Filter URL / Whitelist
- [ ] Implementasikan `IRequestHandler.OnBeforeBrowse()` untuk memblokir navigasi ke URL di luar whitelist
- [ ] Dukung wildcard domain: `*.example.com`
- [ ] Baca daftar `allowedUrls` dari `default.safeexam.json`
- [ ] Jika URL diblokir: tampilkan halaman peringatan sederhana (bukan error blank)

### 🔑 Config Key Header (Kompatibilitas SEB)
- [ ] Hitung SHA-256 dari isi file konfigurasi saat startup
- [ ] Sisipkan header `X-SafeExamBrowser-ConfigKeyHash: <hash>` ke setiap request HTTP melalui `IRequestHandler.GetResourceRequestHandler()`
- [ ] Server ujian bisa memverifikasi bahwa request berasal dari SafeExamCEF

### 📋 Clipboard
- [ ] Nonaktifkan clipboard paste (`Ctrl+V`) di dalam browser dengan memblokir shortcut di `OnPreKeyEvent`
- [ ] Opsional: disable clipboard OS sepenuhnya saat ujian berlangsung, restore saat keluar

### 🖨️ Print
- [ ] Blokir `Ctrl+P` dan print via CefSharp di `IKeyboardHandler`
- [ ] Nonaktifkan print dari menu CefSharp jika ada

---

## 2. Konfigurasi (`.safeexam.json`)
- [ ] **Load config dari file** — implementasikan `ConfigManager.cs` menggunakan `System.Text.Json`
- [ ] **Model config** (`ExamConfig.cs`) sudah dirancang — buat dan gunakan di `App.xaml.cs`
- [ ] Pass `config.startUrl` ke `ChromiumWebBrowser.Address` saat startup (bukan hardcode)
- [ ] Pass `config.browserTitle` ke `Window.Title`
- [ ] Validasi field wajib pada startup; tampilkan error dan exit jika tidak valid

---

## 3. Pengembangan UI Sederhana

### 🖥️ Toolbar Atas (StatusBar)
Tambahkan panel tipis di atas browser (tinggi ~36px):

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

- Perbarui `ClockText` setiap detik menggunakan `DispatcherTimer`
- Tombol "Keluar Ujian" membuka `QuitDialog` dengan input password

### 🚪 Dialog Keluar (QuitDialog)
Dialog modal sederhana:

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

- `Window.ShowDialog()` → blokir interaksi ke jendela utama sebelum dialog ditutup
- Bandingkan input dengan `config.quitPassword`
- Jika salah → tampilkan pesan "Password salah", log percobaan

### ⚠️ Dialog Peringatan (AlertDialog)
Untuk notifikasi proses terlarang:

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
- [ ] Gunakan **Serilog** (sudah terinstall) → log ke file `logs/session_<tanggal>.log`
- [ ] Event yang perlu dilog:
  - Sesi dimulai / selesai
  - URL yang dikunjungi
  - Shortcut keyboard yang diblokir
  - Proses terlarang yang terdeteksi
  - Percobaan keluar yang gagal

---

## 5. Distribusi & Deployment
- [ ] **Publish sebagai self-contained** (tidak butuh .NET terinstall di PC peserta):
  ```bat
  dotnet publish SafeExamCEF\SafeExamCEF.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
  ```
- [ ] Distribusikan file `.safeexam.json` bersama `.exe` ke folder yang sama
- [ ] Opsional: Enkripsi file config menggunakan AES-256 agar peserta tidak bisa membacanya

---

## Prioritas Pengembangan (Rekomendasi Urutan)

| Prioritas | Fitur | Dampak |
|---|---|---|
| 🔴 Sangat Penting | Load config JSON + startUrl | Fungsionalitas dasar |
| 🔴 Sangat Penting | QuitDialog dengan password | Keamanan ujian |
| 🔴 Sangat Penting | Block Alt+Tab, Win key | Lockdown pokok |
| 🟠 Penting | Filter URL (whitelist) | Keamanan konten |
| 🟠 Penting | ProcessMonitor | Deteksi kecurangan |
| 🟡 Sedang | StatusBar UI + jam | Pengalaman pengguna |
| 🟡 Sedang | Serilog audit log | Investigasi pelanggaran |
| 🟢 Opsional | Config Key Header (SHA-256) | Kompatibilitas SEB |
| 🟢 Opsional | Publish self-contained | Kemudahan distribusi |
| 🟢 Opsional | Enkripsi file config | Keamanan distribusi |
