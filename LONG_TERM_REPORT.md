# 🚀 Laporan Pengembangan Jangka Panjang & Roadmap Procto

Procto dan ProctoLite adalah aplikasi ujian yang berkembang pesat. Berikut adalah visi, pengembangan jangka panjang, serta penyelesaian kendala integrasi dengan platform pihak ketiga seperti Google.

---

## 📅 Visi Jangka Panjang (Pengembangan Lingkungan Lokal ala Safe Exam Browser)

Fokus pengembangan Procto ke depannya adalah memperkuat penguncian (lockdown) di tingkat lokal/sistem operasi Windows, sehingga ekuivalen atau melampaui kemampuan Safe Exam Browser (SEB) tanpa bergantung pada infrastruktur *cloud* yang kompleks.

### 1. Penguatan Keamanan Sistem OS (Deep OS Lockdown)
*   **Modifikasi Registry Sementara (Kiosk Mode)**: Memodifikasi registry Windows saat aplikasi berjalan untuk menyembunyikan opsi "Task Manager", "Switch User", dan "Sign Out" dari layar `Ctrl+Alt+Del`. Nilai registry ini akan dikembalikan (restore) saat ujian selesai.
*   **Pemblokiran Virtual Desktop & Multi-Monitor**: Mencegah peserta ujian membuka virtual desktop baru (`Win+Tab` / `Win+Ctrl+D`) atau menggunakan layar eksternal yang dapat dimanfaatkan untuk menyontek.
*   **Pengamanan Papan Klip (Clipboard) Terisolasi**: Memastikan *copy-paste* dinonaktifkan sepenuhnya di OS atau diisolasi agar peserta tidak bisa menyalin soal keluar dari browser atau menempelkan jawaban dari dokumen lain.

### 2. Validasi Konfigurasi Lokal (Config Key & Exam Key)
*   **Config Key (Hashing Konfigurasi)**: Mengimplementasikan sistem *Config Key* seperti SEB. Procto akan menghitung hash SHA-256 dari file `default.procto.json`. Hash ini disisipkan di setiap HTTP Request Header (contoh: `X-Procto-ConfigKey`) sehingga server ujian (misal: Moodle/CBT lokal) dapat memverifikasi bahwa peserta benar-benar menggunakan pengaturan ujian yang valid dan belum diubah.
*   **Enkripsi File Konfigurasi**: Mendukung file `default.procto.json` yang dienkripsi dengan password, memastikan siswa tidak dapat membaca atau memodifikasi daftar URL yang diizinkan (Whitelist) menggunakan teks editor biasa.

### 3. Kontrol Peramban Terpusat (Strict Browser Constraints)
*   **Pembersihan Data Sesi Mandiri (Incognito-like)**: Menghapus seluruh cache, *Local Storage*, *Session Storage*, dan *Cookies* secara otomatis saat aplikasi ditutup agar peserta berikutnya menggunakan sesi yang benar-benar bersih.
*   **URL & Wi-Fi Filtering (LAN/Lokal)**: URL Filter Handler yang mendalam tidak hanya membatasi link yang diklik, tetapi juga memblokir injeksi *iframe* atau *pop-up* dari domain yang tidak masuk *whitelist*.
*   **User-Agent Spoofing Otomatis**: Secara bawaan mengganti CefSharp User-Agent agar menyerupai browser OS bawaan untuk memastikan kompatibilitas penuh dengan sistem LMS lokal.

### 4. Integrasi Aplikasi Pihak Ketiga (Local Allowed Resources)
*   Alih-alih terkoneksi ke server untuk mengawasi layar, Procto akan mendukung peluncuran aplikasi lokal yang secara spesifik diizinkan (misalnya `calc.exe` atau Notepad) sebagai proses *child* di dalam *overlay* ujian yang aman tanpa mengorbankan lockdown layar.

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