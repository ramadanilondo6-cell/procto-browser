# 🚀 Laporan Pengembangan Jangka Panjang & Roadmap Procto

Procto dan ProctoLite adalah aplikasi ujian yang berkembang pesat. Berikut adalah visi, pengembangan jangka panjang, serta penyelesaian kendala integrasi dengan platform pihak ketiga seperti Google.

---

## 📅 Visi Jangka Panjang (1 - 2 Tahun ke Depan)

### 1. Sistem Pengawasan Aktif Berbasis AI
Aplikasi tidak lagi sekadar pasif memblokir interaksi (Lockdown), tetapi aktif mendeteksi kecurangan.
*   **Webcam Tracking**: Mendeteksi jika wajah peserta tidak berada di layar atau jika ada lebih dari satu orang di bingkai kamera.
*   **Audio Monitoring**: Mendeteksi suara percakapan menggunakan AI noise analysis dan merekamnya sebagai *flag* pelanggaran.
*   **Screen Recording Bawah Air**: Jika jaringan terputus, sesi ujian dapat direkam secara lokal (dienkripsi) lalu dikirim ke server setelah koneksi stabil.

### 2. Integrasi Cloud Dashboard (Procto Server)
*   **Monitoring Real-Time**: Pengawas dapat melihat layar peserta mana pun dari browser mereka sendiri, mirip dengan fitur LAN *screen monitoring* tetapi dioptimalkan untuk Cloud.
*   **Centralized Configuration**: Konfigurasi `default.procto.json` tidak lagi disebar via flashdisk atau LAN, melainkan di-pull langsung dari URL Server (contoh: `https://ujian.sekolah.id/procto.json`).
*   **Kill Switch Terpusat**: Pengawas dapat mematikan paksa aplikasi Procto milik peserta tertentu yang ketahuan curang dari jarak jauh.

### 3. Dukungan Lintas Platform
*   Meskipun .NET 6.0 + WPF sangat tangguh di Windows, Procto perlu dikembangkan dengan teknologi seperti **MAUI** atau **Avalonia** untuk mendukung:
    *   **macOS**: Menggunakan WKWebView (Mac) yang dikunci.
    *   **ChromeOS / Android**: Untuk ujian yang berbasis Tablet dan Chromebook.

---

## 🛠️ Isu Khusus: Login Akun Google di Embedded Browser (CEF)

**Pertanyaan:** *Apakah API Google bisa diganti ke login Google biasa tanpa mengalami pesan error "Browser Not Secure"?*

**Jawaban:** Bisa, dengan modifikasi khusus.

Secara bawaan, Google **memblokir seluruh percobaan login** (Gmail/Google SSO) yang dilakukan dari *embedded browser* atau *webview* kustom seperti CEF (Chromium Embedded Framework) / CefSharp untuk mencegah potensi pencurian kredensial (Phishing/Man-in-the-Middle).

Jika peserta ujian menggunakan Google Forms atau aplikasi web yang membutuhkan login Google, mereka akan dicegah dengan layar "*This browser or app may not be secure*".

### Solusi untuk Mengizinkan Login Google Biasa:

Terdapat dua pendekatan untuk menyelesaikan masalah ini.

#### A. Spoofing User-Agent (Paling Mudah)
Kita bisa "mengelabui" server Google agar mereka mengira Procto adalah browser Google Chrome standar, bukan CEF.
Tambahkan parameter ini pada konfigurasi CefSharp di `Program.cs`:

```csharp
var settings = new CefSharp.Wpf.CefSettings();
// Ganti User-Agent ke Chrome modern tanpa string "CefSharp"
settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
```
*Catatan: Metode ini paling cepat diimplementasikan, tetapi Google terkadang memutakhirkan metode deteksinya. Jika ini terjadi, kita harus terus meng-update User Agent Chrome yang valid.*

#### B. Menggunakan OAuth 2.0 Loopback / System Browser (Paling Aman, Rekomendasi Google)
*   Alih-alih memaksa pengguna login di *dalam* jendela Procto, aplikasi membuka default browser Windows (Edge/Chrome asli) khusus untuk memuat layar login Google.
*   Setelah pengguna login di browser aslinya, Google mengirim token kembali ke Procto (via `localhost` listener).
*   Procto menyuntikkan token/cookies tersebut ke dalam sesi ujian CefSharp.
*   *Catatan: Pendekatan ini lebih kompleks secara coding dan kurang ideal untuk mode "Lockdown" yang ekstrim, tetapi dijamin tidak akan pernah diblokir oleh Google.*

---

## 📈 Kesimpulan

Pengembangan **ProctoLite** membuktikan arsitektur modular yang adaptif untuk berbagai spesifikasi hardware, namun pengembangan fitur proctoring tingkat lanjut dan integrasi cloud akan menjadi faktor penentu untuk mendominasi lingkungan pendidikan modern.

Terkait dukungan Google Login, implementasi **Spoofing User-Agent** akan menjadi roadmap terdekat untuk memastikan kompatibilitas Google Forms berjalan lancar tanpa error keamanan.