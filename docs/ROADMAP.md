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
| 6 | Makine satın alma ve yerleştirme | ✅ Tamamlandı |
| 7 | Fabrika inşa ve genişletme (alan satın alma, duvar/zemin düzenleme) | ⏭️ Sıradaki (henüz başlanmadı) |
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

## ✅ Multiplayer Üzerine Önemli Not (güncellendi)
Aşama 6 civarında planlanan **küçük, iki oyunculu teknik ağ prototipi tamamlandı ve kullanıcı
tarafından başarıyla test edildi** (Netcode for GameObjects + Unity Transport; host başlatma,
client bağlanma, iki oyuncunun aynı odada birbirinin hareketini/dönüşünü görmesi doğrulandı).
Ayrı bir `MultiplayerPrototype.unity` sahnesinde, `WorkshopPrototype.unity`'ye dokunulmadan
uygulandı.

**Bu doğrulamanın kapsamı dar tutulmalıdır**: yalnızca oyuncu hareketi/kamerası senkronize
edildi (owner-authoritative `NetworkTransform`). Eşya taşıma, üretim, konveyör, sipariş,
ekonomi ve makine yerleştirme sistemlerinin **hiçbiri** henüz ağ üzerinden yönetilmiyor — bu
sistemlerin tamamı hâlâ yerel/tek-oyunculu mantıkla çalışıyor. Gerçek "sunucu her işlemi
doğrular" host-authoritative mimarisi (tam multiplayer), hâlâ **Aşama 18**'de, oyunun asıl
sistemleriyle (ekonomi/üretim/envanter) birlikte ele alınacak.

## Notlar
- Sıralama kesin değildir; bağımlılıklara göre (örn. elektrik sisteminin makine yükseltmelerinden
  önce/sonra gelmesi) aşamalar arasında küçük kaymalar olabilir — bu, geliştirme ilerledikçe
  netleşecektir.
- Aşama 7 kapsamı bu belgenin yazıldığı anda henüz detaylandırılmadı; "fabrika inşa ve
  genişletme" başlığı kullanıcı tarafından verilen üst düzey hedeftir, henüz kod yazılmadı.
