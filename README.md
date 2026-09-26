# 🛒 Envisionary Marketing POS (Satış Noktası Uygulaması)

Bu proje, küçük ve orta ölçekli işletmeler (kırtasiye, market, butik, büfe) için özel olarak geliştirilmiş, tamamen çevrimdışı (offline) çalışabilen, yüksek performanslı bir Masaüstü POS (Satış Noktası) otomasyonudur.

## 🚀 Öne Çıkan Özellikler

* **Hızlı Satış & Barkod Entegrasyonu:** Müşteri kuyruklarını eritmek için optimize edilmiş, barkod okuyucuyla anında eşleşen hızlı sepet ve arama algoritması.
* **Terazi/Gramajlı Ürün Desteği:** Şarküteri, manav veya kuruyemiş gibi terazi (27, 28, 29 barkod önekleri) ürünlerinin barkodlarını otomatik ayrıştırarak (parse) gramaj ve fiyat hesaplama yeteneği.
* **Çoklu / Barkodsuz Hizmet Satışı:** Kırtasiye ve fotokopici gibi işletmeler için barkodu olmayan ürünlerin (fotokopi, çıktı, ciltleme) tek tuşla, miktar girilerek hızlıca sepete eklenmesi.
* **Akıllı Para Üstü Hesaplama:** Nakit alışverişlerde kasiyerin işini kolaylaştıran "verilen para" girişi ile saniyesinde para üstü gösterimi.
* **Excel İle Toplu Veri Yönetimi (ClosedXML):** On binlerce ürünlük toptancı fiyat listelerini tek bir Excel dosyası (.xlsx) aracılığıyla içeri aktarma (Import) ve mevcut envanteri dışarı aktarma (Export).
* **Z-Raporu & İstatistik:** Gün sonu hasılatı, ürün bazlı kar-zarar hesaplamaları ve tarih filtreli satış istatistikleri.
* **Termal Fiş Yazdırma:** `DocumentPaginator` altyapısı ile standart 80mm termal fiş yazıcılardan anında müşteri bilgi fişi (Adisyon) alabilme.
* **Rol Tabanlı Güvenlik (RBAC):** Yönetici ve Kasiyer ayrımı. Kasiyerlerin fiyat değiştirme, geçmiş satış silme veya ayarlar ekranına girmesini engelleyen tam koruma.
* **Kurumsal Kod Güvenliği (Obfuscar):** Tersine mühendislik (Decompile) işlemlerine karşı kaynak kodlarının (DLL/EXE) şifrelenerek karartılması (Obfuscation).

## 🛠️ Kullanılan Teknolojiler ve Mimari

Uygulamanın uzun yıllar sorunsuz yaşayabilmesi ve kodun sürdürülebilirliği için modern yazılım standartları kullanılmıştır:

* **Platform:** .NET 8, C#, Windows Presentation Foundation (WPF)
* **Mimari (Design Pattern):** Tam teşekküllü **MVVM (Model-View-ViewModel)** altyapısı. `DialogService` ve Dependency Injection kullanılarak UI ile iş mantığı (Business Logic) %100 birbirinden izole edilmiştir.
* **Veritabanı:** Kurulum veya sunucu maliyeti gerektirmeyen, doğrudan yerelde (Local) çalışan **SQLite** (System.Data.SQLite).
* **UI/UX:** Kullanıcı dostu, modern *Material Design* esintili özel ResourceDictionary (Theme.xaml) stilleri.
* **Kurulum (Deployment):** Son kullanıcı için tek tıkla kurulum sağlayan **Inno Setup** mimarisi.

## ⚙️ Kurulum ve Derleme (Geliştiriciler İçin)

Projeyi kendi bilgisayarınızda derlemek ve geliştirmek için aşağıdaki adımları izleyebilirsiniz:

1. Bu depoyu (repository) bilgisayarınıza klonlayın:
   ```bash
   git clone https://github.com/KULLANICI_ADINIZ/PosApp.git
   ```
2. **Visual Studio 2022** (veya desteklenen bir IDE) ile `PosApp.csproj` dosyasını açın.
3. Proje açıldıktan sonra NuGet paketlerinin yüklenmesini bekleyin (`ClosedXML`, `System.Data.SQLite`, `Obfuscar`, vb.).
4. Projeyi **Release** modunda derlediğinizde, `Obfuscar` otomatik olarak devreye girip kodlarınızı karartacak ve `bin/Release` klasöründe nihai dosyaları üretecektir.

> 💡 **Not:** Derleme aşamasında yerel veritabanı (SQLite) otomatik olarak oluşturulur. Veritabanı dosyası varsayılan olarak programın çalıştığı dizinde yer alacaktır.

## 🖼️ Ekran Görüntüleri

*(Buraya GitHub'a yüklerken uygulamanın Kasa Ekranı, Envanter Yönetimi ve Giriş Paneli gibi şık ekran görüntülerini ekleyebilirsiniz.)*
* `[Kasa Ekranı Ekran Görüntüsü]`
* `[Envanter (Excel Import) Ekran Görüntüsü]`
* `[Z-Raporu Ekran Görüntüsü]`

---
**Geliştirici:** [Yusuf]  
**Lisans:** MIT License (veya dilediğiniz lisans türü)
