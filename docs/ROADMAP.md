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
| 7 | Fabrika genişletme — hazır üretim alanı satın alma ("Üretim Alanı B") | 🛠️ Geliştirildi — kullanıcı testleri bekleniyor |
| 8 | Elektrik kapasitesi, makine güç tüketimi ve üretim verimliliği | ✅ Tamamlandı (18/18 kullanıcı testi) |
| 9 | Yeni ürünler, makineler ve çok aşamalı üretim (montaj makinesi, AssembledComponent) | 🛠️ Geliştirildi — kullanıcı testleri bekleniyor |
| 10 | Kayıt / yükleme sistemi (save/load) | 🛠️ Geliştirildi — kullanıcı testleri bekleniyor |
| 11 | Gerçek host-authoritative 2 oyunculu multiplayer co-op (Netcode/RPC entegrasyonu) | 🛠️ **11A ve 11B onaylandı, 11C-1 geliştirildi — kullanıcı testleri bekleniyor** (11C-2, 11C-3, 11D-11E planlandı) |
| 12 | Konveyör inşa modu (oyuncunun serbestçe hat kurması) | Planlandı |
| 13 | Forklift ve lojistik (araçla taşıma, toplu nakliye) | Planlandı |
| 14 | Gece-gündüz döngüsü ve vardiya sistemi | Planlandı |
| 15 | Uyuma ve sabaha geçme (zaman atlama, enerji/yorgunluk) | Planlandı |
| 16 | Kapı kilitleri, hırsızlık ve güvenlik sistemleri | Planlandı |
| 17 | Makine eskimesi, arızalar ve tamir mekanikleri | Planlandı |
| 18 | Makine yükseltmeleri (hız/kalite/kapasite seviyeleri) | Planlandı |
| 19 | Steam entegrasyonu ve davet sistemi (Steamworks, lobi) | Planlandı |
| 20 | Görsel cila, animasyon, ses tasarımı, optimizasyon ve Steam yayına hazırlık | Planlandı |

## 🛠️ Multiplayer Üzerine Önemli Not (düzeltildi)
Aşama 6 civarında planlanan **küçük, iki oyunculu teknik ağ prototipi geliştirildi** (Netcode
for GameObjects + Unity Transport; Host Başlat/Client Bağlan/Bağlantıyı Kes test paneli). Ayrı
bir `MultiplayerPrototype.unity` sahnesinde, `WorkshopPrototype.unity`'ye dokunulmadan uygulandı.

⚠️ **Önceki not yanlıştı**: Burada daha önce "kullanıcı tarafından başarıyla test edildi"
yazıyordu — kullanıcı test ekranında Host/Client butonlarını göremediğini bildirdi, yani bu
doğrulama hiç yapılmamıştı. Durum **"geliştirildi — kullanıcı testi bekleniyor"** olarak
düzeltildi. Kök neden araştırması ve adım adım test talimatı `docs/PROJECT_STATUS.md`'de.

**Bu doğrulamanın kapsamı dar tutulmalıdır**: yalnızca oyuncu hareketi/kamerası senkronize
edildi (owner-authoritative `NetworkTransform`). Eşya taşıma, üretim, konveyör, sipariş,
ekonomi ve makine yerleştirme sistemlerinin **hiçbiri** henüz ağ üzerinden yönetilmiyor — bu
sistemlerin tamamı hâlâ yerel/tek-oyunculu mantıkla çalışıyor. Gerçek "sunucu her işlemi
doğrular" host-authoritative mimarisi (tam multiplayer) artık **Aşama 11** kapsamına taşındı
(eski Aşama 18 ile aynı iş — bkz. "Notlar"daki Aşama 11 kaydırma açıklaması) ve **Aşama 11A**
ile başladı: gerçek `WorkshopPrototype.unity` fabrikasının güvenli bir kopyası
(`WorkshopMultiplayer.unity`) üzerinde, bu prototipteki aynı `NetworkPlayer.prefab`/
`NetworkPlayerSetup`/`MultiplayerConnectionUI` kodu değiştirilmeden yeniden kullanılarak iki
oyuncunun bağlanıp hareket etmesi sağlandı. Ekonomi/üretim/envanterin host-authoritative hale
gelmesi 11B-11D'de, kayıt/yükleme ve bağlantı kopması 11E'de ele alınacak. Ayrıntılar
`docs/PROJECT_STATUS.md`'de.

## Notlar
- Sıralama kesin değildir; bağımlılıklara göre (örn. elektrik sisteminin makine yükseltmelerinden
  önce/sonra gelmesi) aşamalar arasında küçük kaymalar olabilir — bu, geliştirme ilerledikçe
  netleşecektir.
- Aşama 7 kapsamı netleşti ve uygulandı: serbest/hibrit inşaat sistemi **bilinçli olarak bu
  aşamada yapılmadı** — kullanıcı "A: Hazır üretim alanı satın alma" seçeneğini onayladı.
  Modüler mimari (`FactoryExpansionDefinition`/`Manager`/`Gate`/`Terminal`) ileride
  Üretim Alanı C/D gibi yeni salonların yalnızca yeni bir veri asset'i + sahne kurulumu
  olarak eklenmesine izin verecek şekilde tasarlandı; kod tekrar yazılmadan genişletilebilir.
  Ayrıntılar için `docs/PROJECT_STATUS.md`.
- Aşama 8, bu belgenin orijinal sırasında 10. sıradaydı ("Elektrik sistemi ve üretim
  verimliliği"); kullanıcı bunu doğrudan "Aşama 8" olarak talep etti, bu yüzden eski 8 ve 9
  ("Yeni ürünler..." / "Makine yükseltmeleri...") bir basamak kaydırıldı — tam olarak bu
  belgenin kendi öngördüğü türden küçük bir sıralama kayması. Jeneratör yakıtı, elektrik
  kabloları/direkleri, elektrik kesintisi olayları, bataryalar, güneş panelleri ve
  multiplayer elektrik senkronizasyonu **bilinçli olarak kapsam dışı** bırakıldı (kullanıcının
  isteğiyle); gelecekteki bir aşamada değerlendirilebilir. Aşama 8, kullanıcı tarafından
  18/18 testle onaylandı. Ayrıntılar için `docs/PROJECT_STATUS.md`.
- Aşama 9 ("Yeni ürünler, makineler ve çok aşamalı üretim") kullanıcı tarafından doğrudan bu
  sırayla talep edildi; eski 9 ("Makine yükseltmeleri") bir basamak daha kaydırılıp 10 oldu.
  Yeni ham madde satın alma ekonomisi, serbest konveyör inşaatı, çoklu aktif sipariş,
  kayıt/yükleme, makine arızaları/bakım, elektrik faturası, yeni fabrika genişleme alanları,
  Steam entegrasyonu ve multiplayer üretim senkronizasyonu **bilinçli olarak kapsam dışı**
  bırakıldı. Ayrıntılar için `docs/PROJECT_STATUS.md`.
- Aşama 10 ("Kayıt / yükleme sistemi") kullanıcı tarafından doğrudan bu sırayla talep edildi —
  bu belgenin orijinal sırasında 17. sıradaydı. "Makine yükseltmeleri" (eski 10) onun yerine
  17'ye kaydı; 11-16 arası değişmedi. Steam Cloud, çoklu kayıt yuvası, otomatik kayıt,
  multiplayer save/load, şifreleme ve kapsamlı sürüm migration'ı **bilinçli olarak kapsam
  dışı** bırakıldı. Ayrıntılar için `docs/PROJECT_STATUS.md`.
- Aşama 11 ("Gerçek host-authoritative 2 oyunculu multiplayer co-op") kullanıcı tarafından
  doğrudan bu sırayla talep edildi — bu belgenin orijinal sırasında 18. sıradaydı ("Online
  co-op ağ altyapısı"). Eski 11-17 ("Konveyör inşa modu" → "Makine yükseltmeleri") bir basamak
  kaydırılıp 12-18 oldu; 19-20 değişmedi. Kullanıcı bu aşamayı açıkça 5 alt aşamaya böldü
  (11A: bağlantı + hareket, 11B: ekonomi/makine, 11C: eşya/üretim/konveyör, 11D: elektrik/
  sipariş/paketleme/genişletme, 11E: host kayıt-yükleme/bağlantı kopması) ve her alt aşamanın
  kullanıcı testiyle onaylanmadan bir sonrakine geçilmemesini istedi. Bu oturumda **yalnızca
  11A** uygulandı. Ayrıntılar için `docs/PROJECT_STATUS.md`.
