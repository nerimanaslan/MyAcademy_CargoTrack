# CargoTrack - Kargo Takip Sistemi

Bu proje, bir kargonun şubeye teslim edilmesinden alıcıya ulaşmasına kadar geçen tüm operasyonel süreçleri yönetmek için geliştirdiğim N-Katmanlı mimariye (N-Tier Architecture) sahip bir kargo otomasyonudur. 

Projeyi geliştirirken sadece basit veri ekle/sil (CRUD) işlemleri yapmak yerine, gerçek dünyadaki kargo işleyişini ve iş kurallarını (Business Logic) yansıtmaya odaklandım. 

## 📸 Ekran Görüntüleri
*(Buraya projeye ait görseller eklenecektir)*
- Anasayfa
- Kargo Takip Ekranı
- Admin / Şube Müdürü Paneli

## 🚀 Öne Çıkan Özellikler

- **Katı Kargo Yaşam Döngüsü:** Kargolar rastgele durum değiştiremez. (Örn: Oluşturuldu -> Transfer Merkezinde -> Dağıtıma Çıktı -> Teslim Edildi). Geçişler servis katmanında kontrol altındadır.
- **Dinamik Fiyatlandırma:** Kargo ücretleri girilen En, Boy, Yükseklik (Desi kuralı) ve ağırlığa göre otomatik hesaplanır.
- **Teslimat Güvenliği (PIN):** Kargo dağıtıma çıktığında sistem otomatik 6 haneli bir teslimat kodu üretir. Kurye bu kodu girmeden kargoyu teslim edemez.
- **İade Süreci Algoritması:** Eğer bir kargo için 3 kez "Teslim Edilemedi" (evde yok vb.) kaydı girilirse, sistem kargoyu otomatik olarak "İade Sürecine" alır.
- **Rol Bazlı Erişim (RBAC):** Admin (her şeyi görür), Manager (sadece kendi şubesindeki kargoları yönetir) ve User (kendi kargolarını takip eder) olmak üzere 3 farklı rol bulunur.
- **Audit Log (İzlenebilirlik):** Sistemdeki her durum değişikliği, işlemi yapan kullanıcı, tarih, eski durum ve yeni durum şeklinde kayıt altına alınır.

## 🛠️ Kullanılan Teknolojiler

- **Backend:** ASP.NET Core 8.0 MVC, C#
- **Mimari:** N-Tier Architecture (Clean Architecture prensipleri), Repository Design Pattern
- **Veritabanı:** MS SQL Server, Entity Framework Core (Code First)
- **Güvenlik & Yetki:** ASP.NET Core Identity
- **Eşleme & Doğrulama:** Mapster (Sadece Business katmanında izole edildi), FluentValidation
- **Frontend:** Bootstrap 5, Chart.js (Dinamik Admin raporları için), HTML/CSS, ViewComponent & Partial Views

## ⚙️ Kurulum ve Çalıştırma

Projeyi bilgisayarınızda çalıştırmak için:
1. Repoyu bilgisayarınıza klonlayın.
2. `CargoTrack.WebUI` klasörü altındaki `appsettings.json` dosyasından SQL Server bağlantı dizenizi (Connection String) kendi bilgisayarınıza göre ayarlayın.
3. Package Manager Console üzerinden (WebUI projesi seçiliyken) `Update-Database` komutunu çalıştırın.
4. Projeyi başlattığınızda sistem **"Data Seeding"** mekanizması ile varsayılan Admin, Manager hesaplarını ve test kargolarını veritabanına otomatik olarak ekleyecektir.

**Varsayılan Giriş Bilgileri:**
- **Admin:** Kullanıcı adı: `nerimanaslan` | Şifre: `Neriman123*`
- **Şube Müdürü:** Kullanıcı adı: `manager` | Şifre: `123456`
