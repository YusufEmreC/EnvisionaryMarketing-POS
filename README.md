# Envisionary Marketing POS

Bu proje, C# ve WPF kullanılarak geliştirilmiş bir masaüstü Satış Noktası (POS) uygulamasıdır. Özellikle kırtasiye, market ve büfe gibi yerel işletmelerin günlük kasa işlemlerini hızlandırmak ve stok takibini kolaylaştırmak amacıyla geliştirilmiştir. Tamamen çevrimdışı (offline) çalışır.

## Temel Özellikler

- **Hızlı Satış:** Barkod okuyucu desteği ile sepete seri ürün ekleme ve KDV/genel toplam hesaplama.
- **Terazi (Gramaj) Desteği:** 27, 28 ve 29 ön eklerine sahip terazi barkodlarını otomatik olarak ayrıştırıp (parse) gramaj ve fiyat bilgisini çıkarma.
- **Hizmet Satışı:** Kırtasiyelerdeki fotokopi veya spiral ciltleme gibi barkodsuz işlemlerin miktar girilerek hızlıca sepete eklenmesi.
- **Excel Entegrasyonu:** ClosedXML kütüphanesi sayesinde toptancıdan gelen fiyat listelerini (Excel) sisteme toplu olarak içe aktarma (Import) ve mevcut envanteri dışa aktarma (Export).
- **Z-Raporu ve İstatistikler:** Gün sonu hasılatı hesaplama, kar-zarar durumu ve geçmiş satış istatistiklerini filtreleme.
- **Termal Fiş Yazdırma:** WPF DocumentPaginator kullanılarak standart 80mm termal fiş yazıcılardan müşteri bilgi fişi (adisyon) çıkarma.
- **Kullanıcı Yetkilendirme (RBAC):** Yönetici ve Kasiyer ayrımı yapılarak ürün silme ve geçmiş kayıtları temizleme gibi kritik işlemlerin kısıtlanması.
- **Kod Güvenliği:** Obfuscar entegrasyonu ile derlenmiş dosyaların (DLL/EXE) tersine mühendisliğe (Decompile) karşı karartılması (Obfuscation).

## Kullanılan Teknolojiler

- **Platform:** C#, .NET 8, WPF
- **Mimari:** MVVM (Model-View-ViewModel) - (Kullanıcı arayüzü ve iş mantığı tamamen izole edilmiştir)
- **Veritabanı:** SQLite (Local DB)
- **Yardımcı Kütüphaneler:** ClosedXML (Excel işlemleri), Obfuscar (Güvenlik)
- **Dağıtım (Deployment):** Inno Setup

## Kurulum ve Çalıştırma

Projeyi kendi ortamınızda derlemek için aşağıdaki adımları izleyebilirsiniz:

1. Depoyu bilgisayarınıza klonlayın:
   ```bash
   git clone https://github.com/KULLANICI_ADINIZ/PosApp.git
   ```
2. **Visual Studio** üzerinden `PosApp.csproj` dosyasını açın.
3. Projeyi derlediğinizde (Build) gerekli NuGet paketleri otomatik olarak yüklenecektir. Uygulamayı ilk kez çalıştırdığınızda SQLite veritabanı varsayılan dizinde (`bin` klasörü) boş olarak oluşturulur.
4. **Not:** Projeyi `Release` modunda derlerseniz, Obfuscar aracı devreye girerek çıktı dosyalarını otomatik olarak karartılmış (şifrelenmiş) şekilde oluşturacaktır.

## Ekran Görüntüleri

*(Ekran görüntüleri buraya eklenecektir)*
