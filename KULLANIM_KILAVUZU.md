# Envisionary Marketing POS - Kullanım Kılavuzu

Envisionary Marketing POS, perakende mağazalarınızın (market, büfe, mağaza vb.) tüm kasa, ürün ve stok yönetimini kolayca yapabilmeniz için geliştirilmiş, hızlı ve modern bir masaüstü otomasyon sistemidir. 

Bu kılavuz, programı ilk açılıştan itibaren nasıl kullanacağınızı tüm ayrıntılarıyla ama en sade haliyle anlatmaktadır.

---

## 1. İlk Kurulum ve Giriş

Programı bilgisayarınıza kurup ilk kez çalıştırdığınızda sizi bir **Kurulum Ekranı** karşılar.
- **Firma Adı:** Fişlerin üstünde ve raporlarda görünecek mağaza/şirket adınız.
- **Kullanıcı Adı ve Şifre:** Sistemin yöneticisi (patron) siz olacağınız için kendinize ait bir giriş hesabı oluşturun.
- **Kurtarma Kelimesi:** Şifrenizi unutmanız durumunda sıfırlamak için gerekli güvenlik kelimesidir. Kimseyle paylaşmayın.

Bu işlemi sadece ilk seferde yaparsınız. Sonraki günlerde programı açtığınızda sadece şifrenizi girerek hızlıca sisteme dahil olabilirsiniz.

---

## 2. Kasa ve Hızlı Satış İşlemleri (Ana Ekran)

Uygulamanın kalbi burasıdır. Müşterileriniz kasaya geldiğinde işlemleri saniyeler içinde tamamlayabilirsiniz.

- **Ürün Okutma:** Ekranda yanıp sönen arama çubuğuna elinizdeki barkod okuyucu ile ürünü okutun. Ürün anında sepete düşer. Eğer okuyucunuz yoksa, klavyeden ürünün adını yazarak da aratıp sepete ekleyebilirsiniz.
- **Hızlı (Barkodsuz) Ürünler:** Poşet, ekmek, su gibi barkodu olmayan ürünleri satmak için tablonun hemen üstündeki **"🍎 Barkodsuz / Hızlı Ürünler Seç"** butonuna tıklayın. Açılan listeden ürünün resmine/ismine tıklayarak anında sepete atabilirsiniz.
- **Adet Değiştirme:** Müşteri aynı üründen 3 tane aldıysa, tek tek okutmanıza gerek yok. Sepete düşen ürünün yanındaki **"+"** veya **"-"** tuşlarına basarak sayıyı anında değiştirebilirsiniz. Tutar otomatik güncellenir. Kritik buton alanları dokunmatik ekranlar için **56x56px** aktif alana sahiptir.
- **Ödeme Alma:** Sağ alt köşedeki **Nakit** veya **Kredi Kartı** butonlarına tıklayarak satışı tamamlayın. İşlem biter bitmez otomatik olarak fişiniz yazdırılır (yazıcı ayarlıysa).
- **İade Alma:** Müşteri ürün iade getirdiyse, sağ alttaki **"İade Al"** butonuna tıklayıp iade edilen ürünü seçmeniz yeterlidir. Tutar kasadan düşülür ve stok tekrar depoya eklenir. Finansal risk taşıyan iade işlemleri yönetici şifre onayına tabidir.

---

## 3. Ürünler ve Envanter Yönetimi

Sol menüden "Ürünler" sekmesine tıklayarak mağazanızdaki tüm ürünleri yönetebilirsiniz.

- **Barkodlu Ürün Ekleme:** Ürünün barkodunu okutun, adını, alış-satış fiyatını ve elinizde kaç tane (stok) olduğunu girin. (İsteğe bağlı olarak Son Kullanma Tarihi ve KDV oranını da girebilirsiniz).
- **Hızlı / Barkodsuz Ürün Ekleme:** Sayfanın biraz daha altındaki "⚡ Barkodsuz / Hızlı Ürün Ekle" kartını kullanarak saniyeler içinde Ekmek, Poşet gibi ürünleri yaratabilirsiniz. Sadece "İsim" ve "Fiyat" girmeniz yeterlidir. Bu ürünler direkt kasadaki hızlı menüye düşer.
- **Stok ve Tarih Uyarıları:** 
  - Elinizde 4 adet ve daha az kalan ürünler kırmızı bir **"Kritik"** uyarısı verir.
  - Son kullanma tarihine 15 günden az kalan ürünler **"SKT Yaklaştı"** veya **"Gün Geçti"** şeklinde sizi uyarır.
- **Excel / CSV ile Toplu Ürün Yükleme:** 
  - Envanter ekranının üst kısmında bulunan **"Excel/CSV İçe Aktar"** butonunu kullanarak yüzlerce ürünü saniyeler içinde sisteme yükleyebilirsiniz.
  - Sistemin kabul ettiği Excel dosyası sırasıyla şu sütunlardan oluşmalıdır:
    1. *Barkod (A)*
    2. *Ürün Adı (B)*
    3. *Satış Fiyatı (C)*
    4. *Stok Miktarı (D)*
    5. *Kategori (E)*
    6. *Alış Fiyatı (F)*
    7. *KDV Oranı (G)*
    8. *Son Kullanma Tarihi (H)* (yyyy-MM-dd formatında)
  - Dosyayı seçtiğinizde program önce bir analiz gerçekleştirir; yeni eklenecek ve güncellenecek ürün sayısını gösteren bir özet ekranı açar. Onayladığınızda aktarım tamamlanır.

---

## 4. Raporlar, Kapanış ve Rapor Arşivi

İşletmenizin ne kadar kazandığını, kar-zarar durumunu bu ekrandan şeffafça görebilirsiniz. (Bu ekranı sadece Yönetici hesabı görebilir, kasiyerler göremez.)

- **Günlük Özet (Dashboard):** Bugün ne kadar ciro yaptığınızı, bunun ne kadarının Nakit, ne kadarının Kredi Kartı olduğunu en üstteki kartlardan takip edin. Ayrıca **Net Karınızı** (Alış fiyatları düşüldükten sonra) görebilirsiniz.
- **En Çok Satanlar:** Hangi ürünlerinizin yok satıyor? Tablodan takip edin.
- **Gün İçi Raporu (X-Raporu):** Kapanış sekmesinden gün içindeki satışları sıfırlamadan, anlık kasa durumunu gösteren bilgi amaçlı X-Raporu çıktısı alabilirsiniz.
- **Z-Raporu Al (Gün Sonu Kapanışı):** Akşam dükkanı kapatırken "Z-Raporu Al" butonuna basın. 
  1. Çekmecedeki nakit parayı sayın ve ekrana yazın. 
  2. Program size "Kasada olması gereken parayı" ve "Sizin saydığınız parayı" karşılaştırarak Kasada **Açık** mı yoksa **Fazla** mı olduğunu kuruşu kuruşuna söyler.
  3. Onayladığınızda dükkan o gün için kapanır ve tüm günün hesabı Z-Raporları geçmişine kaydedilir.
- **Z-Raporu Arşivi:** Sol menüdeki **"Z-Raporu Arşivi"** sekmesinden geçmiş 30 günün Z-Raporlarını tarihsel olarak inceleyebilir, kasa farklarını kontrol edebilir ve yazıcıdan tekrar çıktı alabilirsiniz.

---

## 5. Ayarlar ve Gider Yönetimi

Sol menüden Ayarlar kısmına girerek sistemin genel işleyişine müdahale edebilirsiniz.

- **Yazıcı Ayarları:** Bilgisayarınıza bağlı olan Fiş Yazıcısının (Termal Yazıcı) adını buraya yazarak çıktı almaya başlayabilirsiniz. (Desteklenen kağıt boyutları: 58mm ve 80mm).
- **Gider Ekleme:** Gün içinde kasadan para çıkışı olduysa (Örn: Çaycıya 50 TL, Toptancıya 500 TL), bunu Gider Ekle bölümünden yazın. Bu tutarlar akşam Z-Raporu alırken otomatik olarak hesaptan düşülür.
- **Veri Temizliği:** Aylar geçtikçe çok biriken eski satış geçmişlerini (Örn: 2 ay öncesi) tek tıkla silerek programı hızlandırabilirsiniz.
- **Fabrika Ayarlarına Dön:** Bu özellik TEHLİKELİDİR. Tüm ürünleri, satışları ve şifreleri geri alınamaz şekilde yok eder ve programı ilk kurulduğu günkü haline sıfırlar.

---

## 6. Kullanıcılar (Çalışan Eklemek)

Eğer yanınızda çalışan bir elemanınız varsa, ona kendi şifrenizi vermek yerine "Kullanıcılar" ekranından ona özel bir hesap (Kasiyer) açabilirsiniz.

- **Kasiyer Yetkisi:** Kasiyerler sadece kasada satış yapabilir ve ürünleri görebilir. Raporları inceleyemez, geçmiş satışları ve net karı gömez, ayarları değiştiremezler.
- **Yönetici Yetkisi:** Uygulamadaki her şeye tam erişimi vardır. 
*(Eleman işten ayrıldığında tek tuşla hesabını silip kasaya girişini engelleyebilirsiniz).*

---

## 7. Öne Çıkan Gelişmiş Fonksiyonlar

Uygulama, profesyonel kullanım deneyimi ve yüksek güvenlik için aşağıdaki otomatik fonksiyonları içerir:

- **Otomatik Terazi Barkodu Ayrıştırma:**
  - Kasap, şarküteri veya manav terazilerinden alınan 13 haneli barkodları (27, 28 ve 29 ile başlayan) sistem otomatik olarak tanır.
  - Barkodun içerisindeki *5 haneli ürün kodunu* ve *5 haneli gramaj bilgisini* (Örn: 01250 -> 1.250 kg) ayıklayarak sepete miktar olarak yansıtır.
- **Hatalı Çift Okutma Koruması (Debounce):**
  - Barkod okuyucunun el titremesi veya mekanik hatalardan ötürü 300 ms içinde aynı barkodu yanlışlıkla iki kez okutmasını otomatik olarak engeller.
- **Para Üstü Hesaplama ve Yetersiz Ödeme Engeli:**
  - Nakit ödemelerde müşteriden alınan para girildiğinde, sistem para üstünü otomatik olarak hesaplar. Alınan para toplam tutardan az ise tamamlamayı engeller.
- **Terazi (Tartılı) Miktar Giriş Paneli:**
  - Ekmek, poşet veya manav ürünleri için ondalıklı ve dokunmatik numpad barındıran hassas bir miktar giriş penceresi mevcuttur.
- **Stok Limit Uyarısı ve Kontrolü:**
  - Satış esnasında sepete eklenen miktar eldeki mevcut stoğu aşarsa, kasiyere şık bir uyarı penceresi gösterilir. Satışın eksi stokla devam edip etmeyeceği onaylanır.
- **Suiistimal (Misconduct) Engeli:**
  - Sepeti boşaltma, iade alma, Z-Raporu alma gibi finansal riskli işlemlerde elemanın suistimalini önlemek amacıyla **Yönetici Onay Şifresi** ekranı açılır.
- **Veritabanı Koruma & Otomatik Yedekleme:**
  - Sistem veritabanı işlemlerinde kilitlenmeleri önlemek için **WAL (Write-Ahead Logging)** moduyla çalışır.
  - Program her açıldığında son 7 günün yedeğini `AutoBackups` klasörüne otomatik olarak kopyalar ve 7 günden eski yedekleri otomatik temizler.

---

## 8. Geliştirici & Teknik Notlar

Uygulamanın kaynak kod yapısı kolay okunabilirlik, bakım ve modülerlik amacıyla katmanlı mimariye dönüştürülmüştür. Visual Studio veya VS Code üzerinde şu klasör hiyerarşisi takip edilebilir:

- **📁 Services (İş Mantığı Katmanı):** `DatabaseManager.cs`, `CartManager.cs`, `PrinterManager.cs`, `ProductManager.cs`, `ReportManager.cs`, `SettingsManager.cs`
- **📁 Models (Veri Modelleri):** `Models.cs` (Product, SaleItem, User, ZReport vb. veri yapıları)
- **📁 Utils (Yardımcı Araçlar):** `BarcodeListener.cs` (Klavye kancası / 50ms algoritması), `ProductImporter.cs` (Excel veri okuyucu)
- **📁 Views (Arayüz Katmanı):** `MainWindow.xaml` (Ana Ekran), `Theme.xaml` (Tasarım Sistemi) ve diğer tüm ekran tasarımları.

---
**Envisionary Marketing POS** ile mağazanızı dijitalleştirin, açık vermeyin, işinize odaklanın! İyi çalışmalar dileriz.
