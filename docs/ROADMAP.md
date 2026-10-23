# Shiftworks — 20 Aşamalık Yol Haritası

Bu belge genel yöndür; her aşama başlamadan önce ayrıntılı kapsam ayrıca konuşulur. Her aşama
tamamlandığında `docs/PROJECT_STATUS.md` güncellenmelidir.

| # | Aşama | Durum |
|---|---|---|
| 1 | FPS karakter ve atölye prototipi | ✅ Tamamlandı |
| 2 | Eşya alma, taşıma ve fizik | ✅ Tamamlandı |
| 3 | İlk üretim makinesi (kesim) | ✅ Tamamlandı |
| 4 | Konveyör bantları ve otomatik üretim hattı | ✅ Tamamlandı |
| 5 | Sipariş, ekonomi, paketleme ve teslimat | ✅ Tamamlandı |
| 6 | Makine satın alma ve yerleştirme | ⏭️ Sıradaki |
| 7 | Fabrika inşa ve genişletme (alan satın alma, duvar/zemin düzenleme) | Planlandı |
| 8 | Yeni ürünler ve çok aşamalı üretim zincirleri | Planlandı |
| 9 | Makine yükseltmeleri (hız/kalite/kapasite seviyeleri) | Planlandı |
| 10 | Elektrik sistemi ve üretim verimliliği | Planlandı |
| 11 | Konveyör inşa modu (oyuncunun serbestçe hat kurması) | Planlandı |
| 12 | Forklift ve lojistik (araçla taşıma, toplu nakliye) | Planlandı |
| 13 | Gece-gündüz döngüsü ve vardiya sistemi | Planlandı |
| 14 | Uyuma ve sabaha geçme (zaman atlama, enerji/yorgunluk) | Planlandı |
| 15 | Kapı kilitleri, hırsızlık ve güvenlik sistemleri | Planlandı |
| 16 | Makine eskimesi, arızalar ve tamir mekanikleri | Planlandı |
| 17 | Kayıt / yükleme sistemi (save/load) | Planlandı |
| 18 | Online co-op ağ altyapısı (host-authoritative, Netcode/RPC) | Planlandı |
| 19 | Steam entegrasyonu ve davet sistemi (Steamworks, lobi) | Planlandı |
| 20 | Görsel cila, animasyon, ses tasarımı, optimizasyon ve Steam yayına hazırlık | Planlandı |

## ⚠️ Multiplayer Üzerine Önemli Not
Tam multiplayer uygulaması (Aşama 18) yol haritasında ileride olsa da, **Aşama 6 civarında
küçük, basit bir iki oyunculu teknik ağ prototipi** planlanıyor (örn. iki istemcinin aynı
oturuma bağlanıp temel hareket/senkronizasyonu görmesi gibi dar kapsamlı bir deneme). Amaç:
host-authoritative mimarinin temel varsayımlarını (tek yetkili durum, atomik işlemler — bkz.
`CLAUDE.md`) çok geç değil, erken aşamada bir kez doğrulamak. Bu prototip, Aşama 6'nın asıl
kapsamının (makine satın alma/yerleştirme) yerini almaz; ayrı, küçük bir teknik doğrulama
adımı olarak ele alınacaktır.

## Notlar
- Aşama 6 kapsamı bu belgenin yazıldığı anda henüz detaylandırılmadı; "makine satın alma ve
  yerleştirme" başlığı kullanıcı tarafından verilen üst düzey hedeftir.
- Sıralama kesin değildir; bağımlılıklara göre (örn. elektrik sisteminin makine yükseltmelerinden
  önce/sonra gelmesi) aşamalar arasında küçük kaymalar olabilir — bu, geliştirme ilerledikçe
  netleşecektir.
- Bu roadmap'te listelenen hiçbir sistem için henüz kod yazılmadı (Aşama 6 dahil).
