# Envisionary Marketing POS - Kullanım Kılavuzu

Envisionary Marketing POS, perakende mağazalarınızın (kırtasiye, market, büfe, butik vb.) tüm kasa, ürün ve stok yönetimini kolayca yapabilmeniz için geliştirilmiş, hızlı ve modern bir masaüstü otomasyon sistemidir. 

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
- **Hızlı (Barkodsuz) Ürünler:** Fotokopi, poşet, su gibi barkodu olmayan hizmet ve ürünleri satmak için tablonun sol/üst tarafındaki hızlı ürün butonlarına tıklayın.
- **Çoklu Satış (Fotokopi vb.):** Müşteri 100 sayfa fotokopi çektirdiyse ekrandaki butona 100 kere basmanıza gerek yoktur. Miktar girme ekranını kullanarak önce "100" yazıp ardından ürünü seçerek sepete tek kalemde toplu ekleme yapabilirsiniz.
- **Adet Değiştirme:** Sepete düşen ürünün yanındaki **"+"** veya **"-"** tuşlarına basarak sayıyı anında değiştirebilirsiniz. Tutar otomatik güncellenir.
- **Ödeme Alma:** Sağ alt köşedeki **Nakit** veya **Kredi Kartı** butonlarına tıklayarak satışı tamamlayın. İşlem biter bitmez otomatik olarak fişiniz yazdırılır (yazıcı ayarlıysa).
- **Para Üstü Hesaplama:** Nakit ödemelerde, müşterinin size verdiği parayı sisteme girdiğinizde, program devasa rakamlarla para üstünü ekrana yansıtır.
- **İade Alma:** Müşteri ürün iade getirdiyse, sağ alttaki **"İade Al"** butonuna tıklayıp iade edilen ürünü seçmeniz yeterlidir. Tutar kasadan düşülür ve stok tekrar depoya eklenir. Finansal risk taşıyan iade işlemleri yönetici şifre onayına tabidir.

---

## 3. Ürünler ve Envanter Yönetimi

Sol menüden "Ürünler" sekmesine tıklayarak mağazanızdaki tüm ürünleri yönetebilirsiniz.

- **Barkodlu Ürün Ekleme:** Ürünün barkodunu okutun, adını, alış-satış fiyatını ve elinizde kaç tane (stok) olduğunu girin. (İsteğe bağlı olarak Son Kullanma Tarihi ve KDV oranını da girebilirsiniz).
- **Hızlı / Barkodsuz Ürün Ekleme:** Saniyeler içinde barkodsuz ürün (örn: Çıktı, Ekmek) yaratabilirsiniz. Bu ürünler direkt kasadaki hızlı menüye düşer.
- **Stok ve Tarih Uyarıları:** 
  - Elinizde 4 adet ve daha az kalan ürünler kırmızı bir **"Kritik"** uyarısı verir.
  - Son kullanma tarihine 15 günden az kalan ürünler **"SKT Yaklaştı"** veya **"Gün Geçti"** şeklinde sizi uyarır.
- **Excel / CSV ile Toplu Ürün Yükleme (ClosedXML):** 
  - Envanter ekranının üst kısmında bulunan **"Excel/CSV İçe Aktar"** butonunu kullanarak yüzlerce ürünü (Örn: Toptancı fiyat listesini) saniyeler içinde sisteme yükleyebilirsiniz.
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

İşletmenizin ne kadar kazandığını, kar-zarar durumunu bu ekrandan şeffafça görebilirsiniz. (Bu ekranı sadece Yönetici hesabı görebilir).

- **Günlük Özet (Dashboard):** Bugün ne kadar ciro yaptığınızı, bunun ne kadarının Nakit, ne kadarının Kredi Kartı olduğunu en üstteki kartlardan takip edin. Ayrıca **Net Karınızı** (Alış fiyatları düşüldükten sonra) görebilirsiniz.
- **En Çok Satanlar:** Hangi ürünlerinizin yok satıyor? Tablodan takip edin.
- **Gün İçi Raporu (X-Raporu):** Kapanış sekmesinden gün içindeki satışları sıfırlamadan, anlık kasa durumunu gösteren bilgi amaçlı X-Raporu çıktısı alabilirsiniz.
- **Z-Raporu Al (Gün Sonu Kapanışı):** Akşam dükkanı kapatırken "Z-Raporu Al" butonuna basın. 
  1. Çekmecedeki nakit parayı sayın ve ekrana yazın. 
  2. Program size "Kasada olması gereken parayı" ve "Sizin saydığınız parayı" karşılaştırarak Kasada **Açık** mı yoksa **Fazla** mı olduğunu kuruşu kuruşuna söyler.
  3. Onayladığınızda dükkan o gün için kapanır ve tüm günün hesabı arşivlenir.
- **Z-Raporu Arşivi:** Sol menüdeki **"Z-Raporu Arşivi"** sekmesinden geçmiş 30 günün Z-Raporlarını tarihsel olarak inceleyebilir, kasa farklarını kontrol edebilir ve yazıcıdan tekrar çıktı alabilirsiniz.

---

## 5. Ayarlar ve Gider Yönetimi

Sol menüden Ayarlar kısmına girerek sistemin genel işleyişine müdahale edebilirsiniz.

- **Yazıcı Ayarları:** Bilgisayarınıza bağlı olan Fiş Yazıcısının (Termal Yazıcı) adını buraya yazarak çıktı almaya başlayabilirsiniz. (Standart 80mm ve 58mm kağıt boyutları desteklenir).
- **Gider Ekleme:** Gün içinde kasadan para çıkışı olduysa (Örn: Çaycıya 50 TL, Toptancıya 500 TL), bunu Gider Ekle bölümünden yazın. Bu tutarlar akşam Z-Raporu alırken otomatik olarak hesaptan düşülür.
- **Veri Temizliği:** Aylar geçtikçe çok biriken eski satış geçmişlerini tek tıkla silerek programı hızlandırabilirsiniz.
- **Fabrika Ayarlarına Dön:** Bu özellik TEHLİKELİDİR. Tüm ürünleri, satışları ve şifreleri geri alınamaz şekilde yok eder.

---

## 6. Kullanıcılar (Çalışan Eklemek)

Eğer yanınızda çalışan bir elemanınız varsa, ona kendi şifrenizi vermek yerine "Kullanıcılar" ekranından ona özel bir hesap (Kasiyer) açabilirsiniz.

- **Kasiyer Yetkisi:** Kasiyerler sadece kasada satış yapabilir ve ürünleri görebilir. Raporları inceleyemez, geçmiş satışları ve net karı göremez, ayarları değiştiremez veya iptal/iade işlemleri yapamaz.
- **Yönetici Yetkisi:** Uygulamadaki her şeye tam erişimi vardır. İptal, iade ve Z-Raporu onayı gerektiren her yerde Yönetici şifresi sorulur (Suistimal engeli).

---

## 7. Öne Çıkan Gelişmiş Fonksiyonlar

Uygulama, profesyonel kullanım deneyimi ve yüksek güvenlik için aşağıdaki otomatik fonksiyonları içerir:

- **Otomatik Terazi Barkodu Ayrıştırma:** Kasap, şarküteri veya kuruyemiş terazilerinden alınan barkodları (27, 28 ve 29 ile başlayan) sistem otomatik olarak tanır. Barkodun içerisindeki *5 haneli ürün kodunu* ve *5 haneli gramaj bilgisini* ayıklayarak sepete gramajlı/kuruşlu olarak yansıtır.
- **Hatalı Çift Okutma Koruması (Debounce):** Barkod okuyucunun el titremesi veya mekanik hatalardan ötürü aynı barkodu çok kısa süre (ms) içinde iki kez okutmasını otomatik olarak engeller.
- **Stok Limit Uyarısı ve Kontrolü:** Satış esnasında sepete eklenen miktar eldeki mevcut stoğu aşarsa kasiyere şık bir uyarı penceresi gösterilir (Eksi stoka düşme uyarısı).
- **Veritabanı Koruma & Otomatik Yedekleme:** Sistem, kilitlenmeleri önlemek için **WAL (Write-Ahead Logging)** moduyla çalışır. Program her açıldığında son 7 günün yedeğini `AutoBackups` klasörüne otomatik olarak kopyalar.

---

## 8. Geliştirici & Teknik Notlar

Uygulamanın kaynak kod yapısı kolay okunabilirlik, bakım ve modülerlik amacıyla katmanlı mimariye (MVVM) dönüştürülmüştür. 

- **İş Mantığı (Services):** Veritabanı (SQLite), Sepet, Yazdırma, Ayar ve Rapor işlemleri ayrı yöneticilere (Managers) bölünmüştür.
- **Güvenlik (Obfuscar):** Nihai uygulamanın kodları (EXE/DLL), tersine mühendisliğe (Decompile) karşı kaynak kod gizleme (Obfuscation) aracıyla şifrelenmiştir.
- **Arayüz (Views):** Kullanıcı deneyimi, ekranı ortalayan (CenterOwner) diyalog pencereleri ve WPF Theme.xaml standartlarıyla modernleştirilmiştir.

---
**Envisionary Marketing POS** ile mağazanızı dijitalleştirin, açık vermeyin, işinize odaklanın! İyi çalışmalar dileriz.
