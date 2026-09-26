# Masaüstü POS Uygulamalarında Tasarım Metodolojisi ve UI/UX Standartları Kuralları

Bu dosya, bu POS uygulamasında uygulanacak ileri düzey tasarım metodolojilerini ve kullanıcı deneyimi (UI/UX) standartlarını tanımlamaktadır.

## 1. Temel Tasarım Felsefesi ve Kullanıcı Deneyimi (UX) Dinamikleri
- **Kas Hafızası Korunumu**: Operatörlerin (kasiyerler, garsonlar vb.) motor hareketlerinin otomatikleşmesini sağlamak amacıyla buton konumları, form alanları ve menü akışları sabit tutulmalıdır. Plansız yapılan görsel güncellemeler işlem süresini uzatır, bu nedenle kritik butonların yerleri korunmalıdır.
- **Suiistimal (Misconduct) ve Yetkilendirme Kontrolü**: İptal, sepet boşaltma, iade ve Z-raporu gibi finansal risk barındıran işlemler doğrudan Rol Tabanlı Erişim Kontrolü (Role-Based Access Control - RBAC) ile yönetici onayına (Manager Authentication) tabi tutulmalıdır. Denetimsiz hata geri alma işlemlerine izin verilmemelidir.

## 2. Jakob Nielsen'in Sezgisel Kullanılabilirlik İlkeleri
- **Sistem Durumunun Görünürlüğü**: Barkod okuma, ağırlık tartımı, fiş yazıcı bağlantısı ve ödeme terminalleri ile olan entegrasyon durumları arayüzde anlık görsel geri bildirimlerle (aktif/pasif indikatörleri) sunulmalıdır.
- **Gerçek Dünya ile Eşleşme**: Ürün kategorileri ve buton yerleşimleri, mağazanın fiziksel reyon ve düzen yapısıyla mantıksal olarak örtüşmelidir.
- **Tanıma ve Hatırlama**: Kasiyerin ürün kodlarını (PLU/SKU) ezberlemesi yerine, arama-filtreleme alanları ve görsel ürün kartları ile ürünü ekranda tanıyarak seçmesi sağlanmalıdır.

## 3. Renk Kuramı, Kontrast (WCAG) ve Tipografi Normları
- **Kontrast Standartları**: WCAG uyarınca normal metinlerde en az **4.5:1**, büyük metinlerde ve grafiklerde en az **3:1** kontrast oranı sağlanmalıdır.
- **Tipografi**: Okunabilirliği yüksek sade sans-serif yazı tipleri tercih edilmelidir. satır yüksekliği ($\text{line-height}$) en az **1.5** olmalıdır.
  - *Başlıklar*: 20px - 28px Semi-Bold
  - *Sepet & Ürün Detayları*: 14px - 16px Regular
  - *Finansal Tutarlar / Fiyatlar*: 28px - 32px Bold
- **Sistem Renk Tokenları**:
  - *Primary (Birincil / Ödeme)*: Mavi (#1A73E8) veya Canlı Mor / İndigo (#3D8BFD)
  - *Success (Başarılı Ödeme / Aktif)*: Yeşil (#266427 / #4CAF50)
  - *Warning (Az Stok / Beklemede)*: Turuncu (#E65100 / #FF9800)
  - *Error (İptal / Tehlike)*: Kırmızı (#EA4335 / #EF5350)
  - *Background (Zemin)*: Gözü yormayan sıcak beyaz (#FAF7F2) veya koyu bar/gece modları için grafit/kömür (#121212)

## 4. Touch Target (Dokunmatik Hedef) ve Arayüz Ergonomisi
- Dokunmatik butonlar için minimum aktif alan (hit area) **44 x 44px** (önerilen **48 x 48px**) olmalıdır.
- Kritik ödeme, miktar değiştirme (+/-) ve sepet kalemi silme (X) butonları için hit area **56 x 56px** seviyesine çıkarılmalıdır.
- Hatalı dokunmaları önlemek amacıyla yan yana veya alt alta duran butonlar arasına en az **8px - 10px** fiziksel mesafe bırakılmalıdır.
- Arayüzde hem Grid (Hızlı görsel tanıma için) hem de List (Dikey alanda maksimum ürün tarama için) görünümleri desteklenmelidir.
