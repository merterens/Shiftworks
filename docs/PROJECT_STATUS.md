# Shiftworks — Proje Durumu

_Son güncelleme: Aşama 1–6 tamamlandı ve test edildi. Aşama 7 (hazır üretim alanı satın alma —
"Üretim Alanı B") **geliştirildi, kullanıcı oynanış testleri bekleniyor.** Aşama 8 (elektrik
kapasitesi, makine güç tüketimi ve üretim verimliliği) **✅ tamamlandı — kullanıcı tarafından
18/18 testle onaylandı.** Aşama 9 (yeni ürünler, montaj makinesi, çok aşamalı üretim) **✅
tamamlandı — kullanıcı tarafından 22/22 testle onaylandı** (yol boyunca bulunan iki geri
bildirim/UX sorunu düzeltildi — bkz. "Aşama 9 Teslimat Hatası Düzeltmesi" ve "Aşama 9 — E
Tuşu Otomatik Bırakma"; `MachineController`/`OrderManager`/`PackingStation`/`ShippingZone` iş
mantığının kendisinde hiçbir gerçek hata bulunmadı). Aşama 10 (kayıt/yükleme sistemi)
**geliştirildi — kullanıcı testleri bekleniyor.** Kullanıcı Escape'e bastığında menünün hiç
açılmayıp sadece imlecin serbest kaldığını bildirdi — kök neden bulundu ve düzeltildi:
`FirstPersonController` ve `PlayerInteractor` aynı Escape tuşuna bağımsız tepki veriyordu,
çalışma sırasına göre menü bazen hiç açılmıyordu (bkz. "Bilinen Teknik Riskler"deki ÇÖZÜLDÜ
notu). Kullanıcı bu düzeltmeyi test etmeden Aşama 10'u 30/30 testle onaylı kabul etti ve
Aşama 11'i ("gerçek host-authoritative 2 oyunculu multiplayer co-op") talep etti. Aşama 11,
kullanıcının isteğiyle **11A-11E** alt aşamalarına bölündü. **11A** (iki oyuncunun aynı
fabrikaya bağlanıp hareket etmesi) geliştirildi ve **kullanıcı tarafından Unity'de test edilip
onaylandı** (iki oyuncu bağlandı, farklı noktalarda doğdu, birbirinin hareketini gördü).
Bunun üzerine **11B** (ekonomi + makine satın alma/yerleştirmenin host-authoritative hale
gelmesi) **geliştirildi — kullanıcı testleri bekleniyor.** Kullanıcı multiplayer testinde
bağlantı/kamera hatası bildirdi; teşhiste **11B'nin kurulum menülerinin (31-32) hiç
çalıştırılmadığı** ortaya çıktı — yani hata 11B'den değil, hâlâ sadece 11A'nın kurulu olduğu
sahneden kaynaklanıyor. Gerçek, doğrulanabilir sorun bulundu ve düzeltildi (oyuncu doğana kadar
sahnede etkin kamera yoktu); bağlantı zincirine teşhis logları eklendi. Ayrıntılar "Aşama 11 —
Bağlantı ve Kamera Hatası Teşhisi/Düzeltmesi" bölümünde. Bağlantı sonra başarıyla kuruldu, ama
**client bir makine satın alırken donuyordu** — kök neden kesin doğrulanamadı ama gerçek bir
dayanıklılık açığı bulundu ve düzeltildi (host RPC'leri try/catch'siz, başarısızlıkta client'a
hiç yanıt gitmiyordu → sonsuz bekleme). Ayrıntılar "Aşama 11B — Client Satın Alma Çökme/Donma
Hatası Teşhisi/Düzeltmesi" bölümünde. Kullanıcının paylaştığı Player.log ile **asıl kök neden
kesinleşti**: `MultiplayerConnectionUI.OnEnable()`, `NetworkManager.Singleton`'a (ki o da
NetworkManager'ın kendi `OnEnable()`'ında atanıyor — Unity farklı nesnelerin `OnEnable()`
sırasını garanti etmez) bazen hiç abone olamıyordu; bağlantı GERÇEKTEN kuruluyordu ama UI
bundan haberdar olmuyor, 8s'lik izleyici başarılı bağlantıyı yanlışlıkla `Shutdown()`
ediyordu. Abonelik `Start()`'a taşınarak düzeltildi (Unity: tüm `OnEnable()`'lar tüm
`Start()`'lardan önce biter — sıralama artık garanti). Ayrıntılar "Aşama 11B — Gerçek Kök
Neden Bulundu" bölümünde. Bağlantı/satın alma onaylandıktan sonra kullanıcı 3 eksik
senkronizasyon bildirdi (fabrika genişletme, elektrik yükseltme, aktif sipariş) —
`NetworkEconomyBridge` aynı dört parçalı desenle bu üçünü de kapsayacak şekilde genişletildi
(ayrıntı "Aşama 11B Genişletmesi" bölümünde, ardından bir NullReferenceException bulundu ve
düzeltildi — "Aşama 11B Genişletmesi — NullReferenceException Düzeltmesi"). **Aşama 11B
artık kullanıcı tarafından tam onaylandı** (bağlantı, ortak para, makine satın alma/
yerleştirme, fabrika genişletme, elektrik yükseltme, ortak sipariş kabul). Bunun üzerine
**Aşama 11C** başladı, **11C-1/11C-2/11C-3** alt aşamalarına bölündü; bu oturumda **yalnızca
11C-1** (ortak eşya alma/taşıma/bırakma/fırlatma) **geliştirildi — kullanıcı testleri
bekleniyor.** Ayrıntılar "Aşama 11C-1 Mimarisi" bölümünde.
Ayrıntılar "Aşama 11A Mimarisi" ve "Aşama 11B Mimarisi" bölümlerinde._

⚠️ **Düzeltme**: Bu dosya daha önce iki oyunculu multiplayer teknik prototipinin "kullanıcı
tarafından başarıyla test edildiğini" söylüyordu. Bu **yanlıştı** — kullanıcı, test ekranında
"Host Başlat"/"Client Bağlan" gibi seçenekleri göremediğini bildirdi, yani bu doğrulama hiç
yapılmamıştı. Durum **"geliştirildi — kullanıcı testi bekleniyor"** olarak düzeltildi. Kök
neden araştırması ve adım adım test talimatı aşağıda, "Multiplayer Test Talimatları" bölümünde.

## Aşama Durumu

| # | Aşama | Durum |
|---|---|---|
| 1 | FPS karakter ve atölye (WorkshopPrototype sahnesi, Player prefab) | ✅ Tamamlandı |
| 2 | Eşya alma, taşıma, bırakma, fırlatma (fizik tabanlı) | ✅ Tamamlandı |
| 3 | İlk üretim makinesi (kesim makinesi, ürün/reçete sistemi) | ✅ Tamamlandı |
| 4 | Konveyör bantları ve otomatik üretim hattı | ✅ Tamamlandı |
| 5 | Sipariş, ekonomi, paketleme ve teslimat sistemi | ✅ Tamamlandı |
| 6 | Makine satın alma ve yerleştirme | ✅ Tamamlandı (8/8 test, yerleştirme zemin-çakışma hatası düzeltildi) |
| — | İki oyunculu multiplayer teknik prototipi (Aşama 6 civarı, erken doğrulama) | 🛠️ **Geliştirildi — kullanıcı testi bekleniyor (düzeltme: önceki "başarılı" kaydı yanlıştı)** |
| 7 | Fabrika genişletme — hazır üretim alanı satın alma ("Üretim Alanı B") | 🛠️ **Geliştirildi — kullanıcı testleri bekleniyor** |
| 8 | Elektrik kapasitesi, makine güç tüketimi ve üretim verimliliği | ✅ **Tamamlandı (18/18 kullanıcı testi)** |
| 9 | Yeni ürünler, makineler ve çok aşamalı üretim (montaj makinesi) | ✅ **Tamamlandı (22/22 kullanıcı testi)** |
| 10 | Kayıt / yükleme sistemi (save/load) | 🛠️ **Geliştirildi — kullanıcı testleri bekleniyor** |
| 11A | Gerçek multiplayer co-op — bağlantı + hareket (host-authoritative hedefine ilk adım) | ✅ **Kullanıcı tarafından test edildi ve onaylandı** |
| 11B | Ekonomi + makine satın alma/yerleştirme + fabrika genişletme + elektrik yükseltme + sipariş kabul — host-authoritative | ✅ **Kullanıcı tarafından test edildi ve onaylandı** |
| 11C-1 | Ortak eşya alma/taşıma/bırakma/fırlatma — host-authoritative | 🛠️ **Geliştirildi — kullanıcı testleri bekleniyor** |

Kullanıcı tarafından onaylanan test sonuçları: Aşama 1 (7/7), Aşama 2 (4/4 + hız/kaybolma hata
düzeltmesi), Aşama 3 (11/11), Aşama 4 (11/11), Aşama 5 (ilk uygulama + 9/9 hata düzeltme testi),
Aşama 6 (8/8, yerleştirme zemin-çakışma hata düzeltmesi dahil), **Aşama 8 (18/18 — elektrik
kapasitesi, güç tahsisi, yükseltme, üretim istatistikleri)**, **Aşama 9 (22/22 — montaj
makinesi, çok aşamalı üretim, ikinci sipariş/mağaza, sipariş-kabul ve E-tuşu UX
düzeltmeleri dahil)**. **Multiplayer teknik prototipi
henüz kullanıcı tarafından test edilemedi** (bkz. düzeltme notu yukarıda ve "Multiplayer Test
Talimatları" bölümü) — bir `NetworkManager` transport-bağlama hatası Aşama 6 civarında bulunup
düzeltilmişti, ama bu kod/sahne seviyesinde bir düzeltmeydi; iki oyuncunun gerçekten bağlanıp
birbirini gördüğü kullanıcı tarafından hiç doğrulanmadı.

⚠️ **Önemli netlik**: Multiplayer prototipi tamamen test edilmiş olsaydı bile, bu oyunun
**bütün sistemlerinin** multiplayer olduğu anlamına **gelmeyecekti**. Prototip sadece oyuncu
hareketi/kamerası (owner-authoritative `NetworkTransform`) için, ayrı, minimal bir sahnede
(`MultiplayerPrototype.unity`) tasarlandı. Eşya taşıma, üretim, konveyör, sipariş, ekonomi ve
makine satın alma/yerleştirme sistemlerinin **hiçbiri** ağ üzerinden çalışmıyor; hepsi
`WorkshopPrototype.unity`'de tamamen yerel/tek-oyunculu durumda. Gerçek host-authoritative
entegrasyonu Aşama 18'e aittir.

## Yeni Oturum İçin Devam Notları

**0000. En son geliştirme (bu oturum — Aşama 10: Kayıt ve Yükleme Sistemi)**: Aşama 9,
kullanıcı tarafından **22/22 testle onaylandı** (✅ tamamlandı). Yeni `Shiftworks.SaveSystem`
namespace'i: `SaveManager` (tek yetkili orkestratör — her sistemin KENDİ `Capture*`/`Restore*`
uç noktalarını çağırır, hiçbir özel alana doğrudan dokunmaz), `SaveGameData` (JSON'a giden düz
veri sınıfları, hiçbir Unity nesne referansı yok), `SaveCatalog` (ID↔asset çözümleyici —
`ItemDefinition.ItemId`'yi, diğerleri için asset dosya adını kullanıyor, hiçbir sınıfa yeni ID
alanı eklenmedi), `SaveFileHandler` (geçici dosya + yedekle güvenli JSON I/O,
`Application.persistentDataPath/shiftworks_save.json`). `MachineController`,
`ConveyorSegment`, `OutputStorage`, `RawMaterialFeeder`, `OrderManager`, `PowerManager`,
`FactoryExpansionManager`, `EconomyManager`, `ProductionStatsTracker`, `PlayerInteractor`,
`FirstPersonController`, `MachinePlacementController` her biri küçük, tek yetkilisini koruyan
`Capture*`/`Restore*` uç noktaları kazandı (ayrıntı "Aşama 10 Mimarisi" bölümünde). Yeni
`SaveLoadMenuUI` (Escape ile açılıyor, `IsInputSuspended` sayesinde diğer panellerle
çakışmıyor). `ShiftworksSaveSetup.cs` (Editor, menü **28**). **Henüz Unity Editor'de açılıp
menü çalıştırılmadı / Play Mode'da test edilmedi.** Desteklenmeyen/basitleştirilen durumlar
için "Aşama 10 — Bilinmeyen/Desteklenmeyen Durumlar" bölümüne bakın (özellikle: konveyör
üzerindeki ürün tam ara pozisyonunu değil, segment başlangıcını koruyor).

**000. Önceki oturum (Aşama 9: Yeni Ürünler, Montaj Makinesi, Çok Aşamalı
Üretim, ✅ 22/22 ile onaylandı)**: Aşama 8, kullanıcı tarafından **18/18 testle onaylandı**. Mimari
inceleme sonucu çok önemli bir bulgu: `OrderManager`, `PackingStation`, `ShippingZone`,
`ShipmentPackage`, `ConveyorSegment` **zaten tamamen veri-odaklı ve çoklu ürün/sipariş türünü
destekliyordu** — sıfır kod değişikliği gerekti. Tek gerçek genelleme:
`MachineController.TryAcceptInput`'in artık `recipe.InputCount`'u (önceden var ama hiç
okunmayan bir alan) gerçekten kullanması — N girdili bir tarifte ilk N-1 parça güç kontrolü
olmadan arabelleğe alınıyor, sadece partiyi TAMAMLAYAN son parça güç için atomik kontrolden
geçiyor (1 girdili tariflerde bu her zaman ilk parçada olduğu için **kesim makinesinin
davranışı piksel piksel aynı** kaldı). Yeni `AssembledComponent` ürünü (2×CutMetalPart →
1×AssembledComponent, 8 sn), yeni `AssemblyMachine` (450 $, 5 kW, aynı `MachineController`
sınıfı — ikinci bir üretim sistemi yazılmadı), ikinci bir `OrderBoard` ("Montaj Parçası
Siparişi", 2×AssembledComponent, 350$) ve ikinci bir `MachineShopTerminal` (mevcut
`MachinePurchaseManager`'ı paylaşıyor) eklendi. `ShiftworksAssemblyProductionSetup.cs`
(Editor, menü 22-25). **Henüz Unity Editor'de açılıp menüler çalıştırılmadı / Play Mode'da
test edilmedi.** Ayrıntı için "Aşama 9 Mimarisi" ve "Aşama 9 Test Talimatları" bölümlerine
bakın.

**00. Önceki oturum (Aşama 8: Elektrik ve Üretim Verimliliği, ✅ 18/18 ile onaylandı)**: Yeni
`Shiftworks.Power` namespace'i: `IPowerConsumer` (arayüz), `PowerManager` (tek yetkili
kapasite/tahsis durumu), `ProductionStatsTracker` (fabrika geneli toplam üretim sayacı),
`PowerPanelTerminal` + yeni `PowerPanelUI` (elektrik panosu). `MachineController` ve
`ConveyorSegment` güç farkındalığı kazandı (ayrıntı için "Önemli Scriptler" bölümüne bakın).
`ShiftworksPowerSetup.cs` (Editor, menü 20-21) sahnedeki 1 makineyi (4 kW) ve 3 konveyörü
(0,5 kW her biri) `PowerManager`'a bağlıyor, elektrik panosunu ekliyor. Başlangıç kapasitesi
**10 kW** (spec değeri) **değiştirilmedi** — mevcut hattın tepe tüketimi (1 makine aynı anda
işlerken + 3 konveyör sürekli) en fazla 4 + 3×0,5 = **5,5 kW**, 10 kW'ın rahatça altında,
doğrulandı.

**0. Daha önceki oturum (multiplayer test menüsü/durum düzeltmesi)**: Kullanıcı,
multiplayer prototipinin önceki oturumda "başarıyla test edildi" olarak kaydedilmesinin
**yanlış** olduğunu bildirdi — test ekranında Host/Client seçeneklerini göremedi. İnceleme
(`MultiplayerPrototype.unity`, `NetworkManager`/`UnityTransport` yapılandırması,
`EditorBuildSettings.asset`) sonucu **kodda/sahnede bir hata bulunamadı**. Kullanıcının
paylaştığı iki ekran görüntüsü kök nedeni kanıtladı: Editor'de Play'e basılırken hâlâ
`WorkshopPrototype.unity` açıktı (pencere başlığı + Hierarchy içeriğiyle doğrulandı), bu yüzden
multiplayer paneli hiç yüklenmedi; Build (`.exe`) tarafı doğru sahneyi açtı ama hiçbir yerde
Host çalışmadığı için "Client Bağlan" sonsuza kadar "bağlanıyor..." durumunda takılı kaldı.
`MultiplayerConnectionUI.cs`'e **(a)** istenen tüm durum metinlerini (Bağlı değil/Host aktif/
Client bağlanıyor/Client bağlı/Bağlantı kesildi/Bağlantı hatası) ayırt eden mantık, **(b)**
client için 8 saniyelik bir UI-seviyesi bağlantı zaman aşımı (host bulunamazsa UTP'nin kendi
~60 saniyelik iç retry süresini beklemeden net hata göstermek için) eklendi. Multiplayer durumu
bu dosyada her yerde "geliştirildi — kullanıcı testi bekleniyor" olarak düzeltildi. **Doğru
sıradaki test**: önce bir pencerede Host Başlat'a basıp, sonra diğerinde Client Bağlan'a
basmak — henüz bu sırayla denenmedi.

**1. Aşama 7 geliştirmesi (henüz kullanıcı tarafından test edilmedi)**: Hazır üretim alanı
satın alma sistemi. Yeni `Shiftworks.Factory` namespace'i (`FactoryExpansionDefinition`,
`FactoryExpansionManager`, `FactoryExpansionGate`, `FactoryExpansionTerminal`) + yeni
`FactoryExpansionPanelUI` (UI) + `ShiftworksFactoryExpansionSetup.cs` (Editor, menü 18-19).
`WorkshopPrototype.unity`'ye mevcut atölyenin batısına bitişik 10×15 m "Üretim Alanı B" eklendi;
paylaşılan `Wall_West` iki parçaya bölünüp (`Wall_West_South`/`Wall_West_North`, Aşama 1'deki
`Wall_South_Left`/`Right` kapı deseninin aynısı) ortasına 2 m'lik kilitli bir bariyer
(`ExpansionBarrier_HallB`) yerleştirildi. `GameHUD`'a genel amaçlı `ShowNotification(string)`
metodu eklendi (önceki sipariş-tamamlandı bildirimi davranışı değişmedi, sadece parametrik
hale getirildi) ve `PlayerInteractor`'a `FactoryExpansionTerminal` için tek bir `if` dalı
eklendi — ikisi de küçük, davranış koruyan genişletmeler. **Henüz Unity Editor'de açılıp
menüler çalıştırılmadı / Play Mode'da test edilmedi.**

**2. Şu anda çalışan sistemler (kod seviyesinde, Aşama 7 dahil henüz Unity'de doğrulanmadı)**:
FPS hareket/bakış, eşya alma/taşıma/bırakma/fırlatma, kesim makinesi (manuel + otomatik),
konveyör hattı, besleme istasyonu, çıkış deposu, sipariş panosu/kabul, paketleme, teslimat,
ekonomi (bakiye), makine mağazası (satın alma + yerleştirme önizlemesi + geçerlilik kontrolü),
**(Aşama 7, yeni)** üretim alanı genişletme terminali/satın alma/kilitli geçiş, ve ayrı sahnede
iki oyunculu temel ağ bağlantısı/hareket senkronizasyonu. Hepsi `WorkshopPrototype.unity`'de
(multiplayer hariç) çalışıyor.

**3. Mevcut teknik riskler**: Aşağıdaki "Bilinen Teknik Riskler" bölümüne bakın — özetle: hiçbir
aşama gerçek Unity batch-mode derlemesiyle doğrulanmadı (Editor sürekli açık tutulduğu için elle
kod incelemesiyle ilerlendi, Aşama 7 de aynı durumda); `OrderManager` tek aktif sipariş sınırlı;
multiplayer prototipi NGO'nun "pre-release" işaretli bir sürümünü kullanıyor; iki oyuncu şu an
aynı noktada spawn oluyor; **(Aşama 7, yeni)** kilitli bariyerdeki 3D etiketin (`TextMesh`)
tam yüzü hangi yöne baktığı Editor'de gözle doğrulanmadı.

**4. Sıradaki görev**: Kullanıcı sırayla: (a) multiplayer test talimatlarını uygulayıp iki
oyunculu bağlantıyı gerçekten test edecek, (b) Aşama 7'yi Unity Editor'de test edecek (menüler
18-19 + Play Mode), (c) Aşama 9'u test edecek (menüler 22-25 + Play Mode, bkz. "Aşama 9 Test
Talimatları" bölümü) — Aşama 8 zaten 18/18 ile onaylandı. Onay sonrası Aşama 10 (makine
yükseltmeleri) planlanacak.

**5. Yeni oturumda okunması gereken dosyalar**: `CLAUDE.md`, `docs/PROJECT_STATUS.md` (bu dosya),
`docs/ROADMAP.md`.

**6. Aşama 7'de değişen/eklenen dosyalar** (bir sonraki oturumda hızlı başvuru için):
- Yeni: `Assets/_Project/Scripts/Factory/FactoryExpansionDefinition.cs`,
  `FactoryExpansionManager.cs`, `FactoryExpansionGate.cs`, `FactoryExpansionTerminal.cs`,
  `Assets/_Project/Scripts/UI/FactoryExpansionPanelUI.cs`,
  `Assets/_Project/Scripts/Editor/ShiftworksFactoryExpansionSetup.cs`.
- Değiştirilen: `Assets/_Project/Scripts/UI/GameHUD.cs` (genel `ShowNotification`),
  `Assets/_Project/Scripts/Player/PlayerInteractor.cs` (`FactoryExpansionTerminal` dalı).
- Unity içinde henüz çalıştırılmadı: `Shiftworks/Setup/18. Create Factory Expansion Data` ve
  `19. Add Factory Expansion Hall To Scene` (veya tek adımda
  `Apply Factory Expansion System (Data + Scene)`) menüleri — bunlar çalıştırılmadan
  `ProductionAreaB.asset`, ikinci salon ve sahnedeki yeni objeler **oluşmaz**.

**7. Önceki oturumda değişen dosya (multiplayer düzeltmesi)**:
`Assets/_Project/Scripts/Multiplayer/MultiplayerConnectionUI.cs` (durum metni/geçici mesaj
mantığı güçlendirildi). `MultiplayerPrototype.unity` sahne dosyasına **hiç dokunulmadı** —
inceleme sonucu sahnede bir hata bulunmadı, bu yüzden sahne/prefab/Editor aracı
(`ShiftworksMultiplayerPrototypeSetup.cs`) değiştirilmedi.

**8. Aşama 8'de değişen/eklenen dosyalar** (bir sonraki oturumda hızlı başvuru için):
- Yeni: `Assets/_Project/Scripts/Power/IPowerConsumer.cs`, `PowerManager.cs`,
  `ProductionStatsTracker.cs`, `PowerPanelTerminal.cs`,
  `Assets/_Project/Scripts/UI/PowerPanelUI.cs`,
  `Assets/_Project/Scripts/Editor/ShiftworksPowerSetup.cs`.
- Değiştirilen: `Assets/_Project/Scripts/Machines/MachineController.cs` (güç isteme/serbest
  bırakma `TryAcceptInput`/`CompleteProcessing` içine atomik olarak eklendi; `powerDemandKw`,
  `HasPower`, `CompletedCount`, `TotalProcessingSeconds`, `TotalPowerWaitSeconds` yeni alanlar),
  `Assets/_Project/Scripts/Conveyors/ConveyorSegment.cs` (`TryAccept`/`Update` güç kontrolü
  eklendi), `Assets/_Project/Scripts/Player/PlayerInteractor.cs` (`PowerPanelTerminal` dalı).
- Unity içinde henüz çalıştırılmadı: `Shiftworks/Setup/20. Add Power System To Scene` ve
  `21. Add Power Panel To Scene` (veya tek adımda
  `Apply Power System (Managers + Panel)`) menüleri — bunlar çalıştırılmadan `PowerManager`,
  `ProductionStatsTracker` ve elektrik panosu sahnede **oluşmaz**, mevcut makine/konveyörlerin
  `powerManager`/`powerDemandKw` alanları da boş kalır (bu durumda kod geriye dönük uyumlu
  davranır — `powerManager == null` kontrolleri sayesinde güç kısıtlaması olmadan, Aşama 8
  öncesi gibi çalışmaya devam eder, asla hata vermez).

**9. Aşama 9'da değişen/eklenen dosyalar** (bir sonraki oturumda hızlı başvuru için):
- Yeni: `Assets/_Project/Scripts/Editor/ShiftworksAssemblyProductionSetup.cs` (menü 22-25) —
  tüm yeni veri/prefab/sahne nesnelerini bu TEK dosya oluşturuyor, yeni bir runtime script
  (namespace) **gerekmedi**.
- Değiştirilen: `Assets/_Project/Scripts/Machines/MachineController.cs` — `recipe.InputCount`
  artık gerçekten okunuyor (`TryAcceptInput` çok-girdili tarifleri arabelleğe alacak şekilde
  genelleştirildi, yeni `_bufferedInputCount` alanı), `GetPromptFor`'daki "Kesim yapılıyor"
  metni genel "Üretim yapılıyor"a çevrildi (montaj makinesi için de doğru olsun diye), çıktı
  hazır metnine "(Çıktı Hazır)" eklendi. **`OrderManager`, `PackingStation`, `ShippingZone`,
  `ShipmentPackage`, `ConveyorSegment`, `OutputStorage`, `RawMaterialFeeder`,
  `MachinePlacementController`, `MachinePurchaseManager`, `MachineShopTerminal`,
  `MachineShopPanelUI`, `OrderBoard`, `OrderBoardPanelUI`, `PowerManager`, `PowerPanelUI`
  dosyalarının HİÇBİRİ değiştirilmedi** — hepsi zaten yeterince veri-odaklıydı.
- Unity içinde henüz çalıştırılmadı: `Shiftworks/Setup/22. Create Assembly Product Data` →
  `23. Create Assembly Machine Definition And Prefab` → `24. Add Assembly Order To Scene` →
  `25. Add Assembly Machine Shop To Scene` (veya tek adımda
  `Apply Assembly Production Chain (Data + Scene)`) — bu sırayla çalıştırılmalı (her biri
  bir öncekinin ürettiği asset'e bağımlı).

## Sahneler
- `Assets/_Project/Scenes/WorkshopPrototype.unity` — **asıl tek oyunculu oyun sahnesi**. 20×15 m
  atölye, giriş kapısı, test kutuları, kesim makinesi + manuel masa, otomatik üretim hattı
  (besleyici → 2 giriş konveyörü → makine → 1 çıkış konveyörü → depo), sipariş panosu,
  paketleme istasyonu, teslimat alanı, HUD ve sipariş paneli UI canvas'ları, `EventSystem`,
  makine mağazası terminali + `MachinePurchaseManager` + mağaza paneli canvas'ı.
- **(Multiplayer prototipi)** `Assets/_Project/Scenes/MultiplayerPrototype.unity` — **ayrı,
  minimal sahne**. 10×10 m basit oda (sadece zemin+4 duvar, üretim/sipariş sistemleri yok),
  `NetworkManager` + `UnityTransport` (varsayılan `127.0.0.1:7777`), Host/Client/Bağlantı Kes
  butonlu geliştirici test paneli. `WorkshopPrototype.unity`'ye hiç dokunulmadı.
- **(Aşama 7)** `WorkshopPrototype.unity` içine eklenen **"Üretim Alanı B"**: mevcut 20×15 m
  atölyenin batısına bitişik, 10×15 m ikinci salon (`Floor_HallB` + `Wall_HallB_North/South/West`,
  x: -20..-10, z: -7.5..7.5 — eski atölyenin x: -10..10 aralığına tam bitişik, zemin seviyesi
  aynı, fiziksel boşluk yok). Paylaşılan `Wall_West` kaldırılıp `Wall_West_South`/`Wall_West_North`
  olarak ikiye bölündü (Aşama 1'in güney kapı deseninin aynısı), ortadaki 2 m'lik boşluğu
  `ExpansionBarrier_HallB` (kilitli, satın alınana kadar `CharacterController`'ı durduran katı
  collider + "Üretim Alanı B — Kilitli" `TextMesh` etiketi) kapatıyor. `FactoryExpansionTerminal`
  eski atölye içinde (`-8.3, 0.75, -2.5`) bariyerin güneyinde duruyor.
- **(Aşama 8)** `WorkshopPrototype.unity` içine eklenen **elektrik panosu**:
  `PowerPanelTerminal` eski atölye içinde (`-8.3, 0.75, 4`) — `FactoryExpansionTerminal` ile
  aynı x-sütununda (batı duvarından 1,7 m içeride), farklı z'de, hiçbir mevcut objeyle
  çakışmıyor. `PowerManager` ve `ProductionStatsTracker` sahne köküne eklenen, görünmez
  (sadece script) yönetici objeler.
- **(Aşama 9)** `WorkshopPrototype.unity` içine eklenen **ikinci sipariş panosu ve ikinci
  makine mağazası terminali**: `AssemblyOrderBoard` (`5, 0.75, -6.5` — orijinal `OrderBoard`'un
  doğusunda) ve `AssemblyMachineShopTerminal` (`-5, 0.75, -6.5` — orijinal
  `MachineShopTerminal`'ın batısında), ikisi de güney duvarı boyunca, mevcut hiçbir objeyle
  çakışmıyor. Montaj makinesinin kendisi sahneye **önceden yerleştirilmiyor** — Aşama 6/7'deki
  kesim makinesi gibi sadece mağazadan satın alınıp oyuncu tarafından istediği yere
  (atölyede veya Üretim Alanı B'de) yerleştirilebiliyor.

## Prefab'lar
- `Assets/_Project/Prefabs/Player/Player.prefab` — `CharacterController`, `FirstPersonController`,
  `PlayerInteractor`, kamera + `HoldPoint`, `InteractionPromptCanvas` (crosshair/bakış metni),
  **(Aşama 6)** `MachinePlacementController`.
- `Assets/_Project/Prefabs/Items/MetalSheet.prefab`, `CutMetalPart.prefab`,
  `ShipmentPackage.prefab` — her biri `Rigidbody` + `Collider` + `CarryableItem`;
  `ShipmentPackage` ayrıca `ShipmentPackage` bileşenini (sipariş kimliği etiketi) taşır.
- **(Aşama 6)** `Assets/_Project/Prefabs/Machines/CuttingMachine.prefab` — Stage 3'teki sahne
  makinesiyle aynı şekilde kurulmuş, bağımsız, satın alınıp yerleştirilebilen kesim makinesi
  prefab'ı; `CutMetalRecipe`'e bağlı, çıkış konveyörüne bağlı değil (yalnızca manuel üretim).
- **(Aşama 9)** `Assets/_Project/Prefabs/Machines/AssemblyMachine.prefab` — `CuttingMachine`
  ile **aynı** `MachineController` sınıfını kullanan, görsel olarak ayırt edilebilir (silindir
  gövde, mor ton) bağımsız montaj makinesi prefab'ı; `AssembleComponentRecipe`'e bağlı
  (2×CutMetalPart → 1×AssembledComponent, 8 sn), 5 kW, çıkış konveyörüne bağlı değil (yalnızca
  manuel üretim, Stage 6'nın kesim makinesiyle aynı tasarım kararı).
- **(Aşama 9)** `Assets/_Project/Prefabs/Items/AssembledComponent.prefab` — `Rigidbody` +
  `Collider` + `CarryableItem`; görsel olarak MetalSheet/CutMetalPart'tan (küp) farklı bir
  silindir, ayırt edilmesi kolay.
- **(Multiplayer prototipi)** `Assets/_Project/Prefabs/Multiplayer/NetworkPlayer.prefab` —
  `CharacterController` + **mevcut, değiştirilmemiş** `FirstPersonController` (varsayılan
  **devre dışı**, sadece sahip istemcide açılır) + `NetworkObject` + `NetworkTransform`
  (`AuthorityMode = Owner`) + `NetworkPlayerSetup`. Tek oyunculu `Player.prefab`'tan tamamen
  ayrı; eşya taşıma/etkileşim bileşenleri bu prototipte kasıtlı olarak yok.

## ScriptableObject Veri Dosyaları
- `Assets/_Project/Data/Items/MetalSheet.asset`, `CutMetalPart.asset` — `ItemDefinition`.
- **(Aşama 9)** `Assets/_Project/Data/Items/AssembledComponent.asset` — `ItemDefinition`
  ("Montaj Parçası", `ProcessedPart`).
- `Assets/_Project/Data/Recipes/CutMetalRecipe.asset` — `RecipeDefinition` (MetalSheet → CutMetalPart, 5 sn).
- **(Aşama 9)** `Assets/_Project/Data/Recipes/AssembleComponentRecipe.asset` —
  `RecipeDefinition` (2× CutMetalPart → 1× AssembledComponent, 8 sn) — `RecipeDefinition`'ın
  önceden var olan ama hiç kullanılmayan `inputCount` alanını ilk kez 1'den farklı bir değerle
  kullanan tarif.
- `Assets/_Project/Data/Orders/FirstMetalPartOrder.asset` — `OrderDefinition` ("İlk Metal Parça
  Siparişi", 3× CutMetalPart, 150 $ ödül, süre sınırı yok).
- **(Aşama 6)** `Assets/_Project/Data/Machines/CuttingMachineDefinition.asset` — `MachineDefinition`
  ("Kesim Makinesi", 300 $, `CutMetalRecipe`, `CuttingMachine.prefab`).
- **(Aşama 9)** `Assets/_Project/Data/Machines/AssemblyMachineDefinition.asset` —
  `MachineDefinition` ("Montaj Makinesi", 450 $, `AssembleComponentRecipe`, `AssemblyMachine.prefab`).
- **(Aşama 9)** `Assets/_Project/Data/Orders/AssemblyOrder.asset` — `OrderDefinition`
  ("Montaj Parçası Siparişi", 2× AssembledComponent, 350 $ ödül) — `OrderManager`'ın
  `availableOrders` listesine, orijinal "İlk Metal Parça Siparişi"nin yanına eklendi.
- **(Aşama 7)** `Assets/_Project/Data/FactoryExpansions/ProductionAreaB.asset` —
  `FactoryExpansionDefinition` ("Üretim Alanı B", 400 $, 10×15 m — görsel geometri bu asset'ten
  üretilmiyor, sahneye elle inşa edildi; asset sadece fiyat/isim/boyut metni için).
- **(Aşama 8)** Yeni bir ScriptableObject veri dosyası **oluşturulmadı**. Güç tüketimi
  (`powerDemandKw`) ve kapasite/yükseltme değerleri (`initialCapacityKw`, `upgradedCapacityKw`,
  `upgradePriceDollars`) `MachineController`/`ConveyorSegment`/`PowerManager` üzerinde
  Inspector'dan ayarlanabilir sade `[SerializeField]` alanları olarak tutuluyor —
  `MachineDefinition`/`RecipeDefinition`'ın aksine, çünkü sahnedeki orijinal makine/konveyörler
  ile Aşama 6'nın satın alınabilir `CuttingMachine.prefab`'ı ortak bir veri asset'i paylaşmıyor
  (ikisi de aynı sınıfın bağımsız örnekleri). Yine de kodda **sabit değer yok**; hepsi
  Inspector'dan görülebilir/değiştirilebilir alanlar — "mümkünse veri tanımlarından alınmalı"
  talimatının ruhuna uygun, ama ayrı bir asset dosyası gerektirmeden.

## Önemli Scriptler ve Sistemler Arası İletişim
- **Etkileşim çekirdeği**: `IInteractable`/`IInteractor` (Core/Interaction). `PlayerInteractor`
  tek bir raycast ile baktığı nesneyi bulur; nesne tipine göre (`MachineController`,
  `OutputStorage`, `OrderBoard`, `PackingStation`, `ShippingZone` veya düz `CarryableItem`)
  özel bir metodu doğrudan çağırır. Raycast, elde taşınan nesneyi (kendi hold point'inde durduğu
  için) hariç tutmak üzere **"Ignore Raycast" katmanını** maskeler.
- **Taşıma**: `CarryableItem` — fizik tabanlı (Rigidbody hız kontrolü), duvarlardan geçmez,
  oyuncu collider'ını itmez. `IsAutomated` bayrağı, konveyör/makine/besleyici tarafından
  sahiplenilen ürünlerin oyuncu tarafından elle alınmasını engeller.
- **Üretim**: `MachineController` tek yetkili durum (`Idle/Processing/OutputReady/Blocked`).
  Hem manuel (oyuncu E) hem otomatik (`IConveyorReceiver.TryAccept` → `TryAcceptInput`) giriş
  **aynı** guard'lı metottan geçer.
- **Konveyör hattı**: `ConveyorSegment` tek ürün kapasiteli, deterministik (fizik kuvveti yok,
  doğrudan pozisyon ataması). `RawMaterialFeeder` → segment zinciri → `MachineController` →
  segment → `OutputStorage`. Dolu segment geri basınç oluşturur, ürün asla silinmez/çoğalmaz.
- **Sipariş/Ekonomi**: `OrderManager` tek yetkili durum (`Available/Active/ReadyToShip/Completed`),
  `EconomyManager` tek yetkili bakiye. `PackingStation` ürün tüketimi + ilerlemeyi tek işlemde
  yapar; tamamlanınca `ShipmentPackage` oluşturur. `ShippingZone`, paketin `AssociatedOrder`
  kimliğini doğrulayıp `OrderManager.TryCompleteOrder`'ı çağırır (ödül bir kez verilir).
- **UI**: `InteractionPromptUI` (bakışa bağlı crosshair metni, Player prefab içinde),
  `GameHUD` ve `OrderBoardPanelUI` (sahne seviyesinde, sürekli görünür/modal UI — `EventSystem`
  + `InputSystemUIInputModule` gerektirir, çünkü proje yalnızca yeni Input System kullanıyor).
- **(Aşama 6) Makine satın alma/yerleştirme**: `MachinePurchaseManager` tek yetkili kaynak
  (bekleyen/yerleştirilmemiş satın alımlar + `EconomyManager.TrySpend` üzerinden para düşme).
  `MachineShopTerminal` + `MachineShopPanelUI` — Satın Al / Yerleştir / Kapat butonlu modal
  panel (OrderBoard'la aynı suspend/resume deseni). `MachinePlacementController` (Player
  prefab'ında) yarı saydam önizleme + R ile döndürme + sol tık ile yerleştirme + Escape ile
  iptal; geçerlilik `Physics.CheckBox` ile gerçek collider boyutlarına göre kontrol edilir.
  Yerleştirilen makine, Stage 3'ün **aynı** `MachineController` sınıfını kullanan bağımsız bir
  örnektir — yeni bir üretim sistemi yazılmadı.
- **(Aşama 7) Fabrika genişletme**: `FactoryExpansionManager` tek yetkili kaynak — `TryPurchase`
  önce "zaten kilidi açık mı" kontrolü yapar, sonra `EconomyManager.TrySpend` üzerinden parayı
  tek atomik adımda düşer, ancak ikisi de başarılıysa kilidi açar ve `ExpansionUnlocked`
  event'ini yayar (çift tıklama veya ikinci satın alma denemesi para harcamadan reddedilir).
  `FactoryExpansionGate` (bariyer objesinin kendisinde) bu event'i dinler ve kendini
  `SetActive(false)` yapar — `MachinePurchaseManager`/`GameHUD` gibi, durumu **sadece okur**,
  asla yazmaz. `FactoryExpansionTerminal` + `FactoryExpansionPanelUI`, `MachineShopTerminal` +
  `MachineShopPanelUI` ile birebir aynı modal aç/kapat ve "bakiyeyi her `Refresh()`'te yeniden
  oku" desenini izliyor. Başarılı satın alma, `GameHUD.ShowNotification(...)` (bu aşamada
  eklenen genel bildirim metodu) üzerinden "Yeni Üretim Alanı Açıldı!" bildirimini tetikliyor.
- **(Aşama 8) Elektrik sistemi**: `PowerManager` tek yetkili kaynak — `TryRequestPower` tek
  atomik metot: zaten tahsisliyse idempotent `true` döner, kapasite yetiyorsa tahsis edip
  `true`, yetmiyorsa `_waiting` kümesine ekleyip `false` döner. **Saklanan bir bekleme kuyruğu
  yeniden oynatılmıyor** — her tüketici zaten kendi doğal döngüsüyle tekrar deniyor
  (`ConveyorSegment` her `Update()`'te, otomatik beslenen bir makine yukarı akıştaki
  konveyörün her kare tekrar denediği hand-off üzerinden, elle beslenen bir makine oyuncunun
  tekrar E'ye basmasıyla) — bu yüzden "bekleyen ekipmanlar tekrar değerlendirilmeli" talebi
  callback/kuyruk altyapısı kurulmadan karşılanıyor. Tahsis sırası saf "ilk isteyen kazanır"
  (rastgele değil, deterministik) — sahnedeki orijinal hat her zaman runtime'da satın
  alınan/yerleştirilen nesnelerden önce `Awake()` çalıştığı için doğal olarak önceliklidir;
  açık bir öncelik alanı istenirse tek değişiklik noktası `PowerManager.HasCapacityFor`/
  `TryRequestPower`'ın karşılaştırma mantığıdır. `MachineController.TryAcceptInput` gücü
  girdi tüketimiyle **aynı atomik çağrıda** ister — reddedilirse hiçbir şey tüketilmez, makine
  `Idle` kalır (ürün kaybı/çoğalması riski yok). `ConveyorSegment.TryAccept` güç yoksa
  `IsOccupied` ile aynı şekilde reddeder — yukarı akış, mevcut geri-basınç mekanizmasıyla
  kendiliğinden birikir, ayrı bir "güç yok" özel durumu gerekmez. `ProductionStatsTracker`
  sadece fabrika geneli toplamı tutar; makine başına sayaçlar (`CompletedCount`,
  `TotalProcessingSeconds`, `TotalPowerWaitSeconds`) her `MachineController`'ın kendi genel
  salt-okunur property'leri. `PowerPanelTerminal` + `PowerPanelUI`, diğer panellerle birebir
  aynı modal aç/kapat desenini izliyor; panel **açıkken her kare** yenileniyor (üretim devam
  ederken kapasite/kullanım canlı değişebildiği için, diğer panellerin aksine sadece buton
  tıklamasında değil).
- **(Aşama 9) Çok aşamalı üretim / montaj makinesi**: `MachineController.TryAcceptInput`
  artık `recipe.InputCount`'u okuyor — `_bufferedInputCount` adlı yeni bir alan, partiyi
  TAMAMLAYACAK son parçaya kadar gelen her parçayı güç kontrolü yapmadan arabelleğe alıyor;
  sadece partiyi tamamlayan çağrı `PowerManager.TryRequestPower`'dan geçiyor — tek atomik
  işlem ilkesi korunuyor (reddedilirse hiçbir şey tüketilmez). 1 girdili bir tarifte
  (`CutMetalRecipe`) bu kontrol her zaman ilk parçada tetiklendiği için **kesim makinesinin
  davranışı Aşama 9 öncesiyle birebir aynı**. `AssemblyMachine`, Stage 3/6'nın kesim
  makinesiyle **aynı** `MachineController` sınıfını kullanıyor — ikinci bir üretim sistemi
  yazılmadı, sadece farklı bir `RecipeDefinition` (`AssembleComponentRecipe`) ve
  `powerDemandKw` (5) ile yapılandırıldı. `OrderManager`'ın zaten çoklu `OrderDefinition`
  listesi + `HasActiveOrder` tek-aktif-sipariş kısıtı desteklediği için ikinci sipariş
  (`AssemblyOrder`) sadece listeye eklendi — kod değişmedi. `PackingStation`/`ShippingZone`
  zaten `activeOrder.RequestedItem`/`package.AssociatedOrder` üzerinden tamamen veri-odaklı
  çalıştığı için **hiçbir ürün türü ayrımı eklemeye gerek kalmadı**. İkinci `OrderBoard` ve
  ikinci `MachineShopTerminal` (mevcut `MachinePurchaseManager`'ı paylaşıyor, zaten
  `Dictionary<MachineDefinition,int>` ile makine türüne göre anahtarlanmış durumda) sahneye
  eklendi — OrderBoard/MachineShopTerminal'ın kendisi hâlâ "tek nesne = tek sipariş/makine"
  deseninde (Aşama 5/6'daki gibi), genelleştirilmedi; ikinci bir fiziksel terminal eklemek bu
  aşamada en az müdahaleli çözümdü.
- **(Multiplayer prototipi) Hareket senkronizasyonu**: `NetworkPlayerSetup.OnNetworkSpawn()`
  sadece `IsOwner` olan istemcide `FirstPersonController`'ı (ve kamera/`AudioListener`'ı)
  etkinleştirir — bileşen prefab'ta **varsayılan devre dışı** gelir, çünkü her istemci HERKESİN
  oyuncu nesnesini kendi tarafında da örnekliyor; devre dışı başlamazsa uzak oyuncuların
  `OnEnable()`'ı da çalışıp yerel imleç kilidini (`Cursor.lockState`, genel/ekran durumu) ele
  geçirirdi. `NetworkTransform` **Owner Authority** modunda — basitlik için, projenin genel
  host-authoritative hedefinden bilinçli bir sapma (bkz. aşağıdaki not).
- **(Multiplayer prototipi) Mimari not**: Hareket/kamera senkronizasyonu bu prototipte **sahip-
  yetkili** (owner-authoritative) — her istemci kendi `FirstPersonController`'ını değiştirmeden
  yerel çalıştırır, `NetworkTransform` sonucu diğerlerine yansıtır. Oturumun kendisi (host kimin
  bağlanabileceğine karar verir, dünyayı barındırır) hâlâ host-merkezli. Gerçek "sunucu her
  hareketi doğrular" host-authoritative mimarisi, Aşama 18'de ekonomi/üretim/envanter
  sistemleriyle birlikte ele alınacak — bu prototip kasıtlı olarak daha basit tutuldu.

## Multiplayer Test Talimatları

Unity'ye yeni başlayan biri için, baştan sona, hiçbir adım atlanmadan:

**1. Hangi sahneyi açacağım?**
Unity Editor'de üstteki **Project** penceresinden `Assets/_Project/Scenes/MultiplayerPrototype.unity`
dosyasına çift tıklayın. `WorkshopPrototype.unity` **değil** — multiplayer test menüsü **sadece**
bu sahnede var. Sahnenin açıldığını, üstteki sahne sekmesinde "MultiplayerPrototype" yazdığını
görerek teyit edin.

**2. "Host Başlat" butonunu nerede göreceğim?**
Doğru sahne açıkken Editor'ün üstündeki ▶ **Play** butonuna basın. Oyun görünümünün sol üst
köşesinde "MULTIPLAYER TEST" başlıklı küçük bir panel belirmeli; üç buton var: **Host Başlat**,
**Client Bağlan**, **Bağlantıyı Kes**, altında da bir durum satırı. Bu paneli görmüyorsanız,
muhtemelen hâlâ yanlış sahne açık — Play'i durdurup adım 1'i tekrar kontrol edin.

**3. Windows Build'i nasıl oluşturacağım?**
Üst menüden **File > Build Profiles** (veya **File > Build Settings**) açın. Sahne listesinde
`MultiplayerPrototype` zaten işaretli/ekli olmalı (proje bunu önceden ayarladı). Platformun
**Windows** olduğundan emin olun, **Build** butonuna basın, bir klasör seçin (örn.
`Builds/MultiplayerTest`). Unity o klasöre bir `.exe` üretecek.

**4. Editor ve Build'i aynı anda nasıl çalıştıracağım?**
Önce Build'den çıkan `.exe` dosyasını çalıştırın (çift tıklayın) — bu bir pencere açar ve
otomatik olarak `MultiplayerPrototype` sahnesiyle başlar (Build Settings'te kayıtlı tek sahne
bu). Sonra Unity Editor'e dönüp `MultiplayerPrototype.unity` sahnesi açıkken Play'e basın.
Artık iki ayrı pencereniz var: biri Editor içinde (Game view), biri bağımsız `.exe` penceresi.
İkisi de aynı anda açık kalabilir, birbirini etkilemez.

**5. Hangi pencerede Host, hangisinde Client başlatacağım?**
**Sıra önemli — önce Host, sonra Client.** Pencerelerden birinde (örn. Build `.exe`) **Host
Başlat**'a basın — durum "Host aktif (0 istemci bağlı)" olmalı. **Ancak bundan sonra** diğer
pencerede (örn. Unity Editor) IP kutusunun `127.0.0.1` olduğunu doğrulayıp **Client Bağlan**'a
basın. Host penceresindeki yazı "Host aktif (1 istemci bağlı)" olmalı, client penceresinde ise
kısa bir "Client bağlanıyor..." anından sonra "Client bağlı" görünmeli. **Hiçbir pencerede
Host çalışmıyorken Client Bağlan'a basarsanız**, artık en fazla 8 saniye içinde net bir
"Bağlantı hatası (Host bulunamadı)" mesajı göreceksiniz (önceden bu süresiz "bağlanıyor..."
olarak takılı kalıyordu) — bu durumda önce bir pencerede Host Başlat'a basıp tekrar deneyin.

**6. İki oyuncunun birbirini gördüğünü nasıl anlayacağım?**
Bağlantı kurulduktan sonra her iki pencerede de birinci şahıs kamerasına geçersiniz (fare
otomatik kilitlenir, imleç kaybolur — bu normal). WASD ile hareket edin: bir penceredeki
oyuncu hareket ettiğinde, **diğer penceredeki ekranda mavi bir kapsül** (diğer oyuncunun
gövdesi) hareket etmeli/dönmelidir. Kendi karakterinizin gövdesini kendi kameranızdan
göremezsiniz (birinci şahıs) — bu beklenen davranış, sadece karşı tarafı görürsünüz.

**7. Bağlantıyı nasıl sonlandıracağım?**
Önce fareyi serbest bırakmak için **Escape** tuşuna basın (imleç geri gelir, "MULTIPLAYER TEST"
paneli zaten her zaman ekranda kalır, Escape sadece imleç kilidini açar). Sonra **Bağlantıyı
Kes** butonuna tıklayın. Durum satırı "Bağlantı kesildi" gösterecek, birkaç saniye sonra
"Bağlı değil"e dönecek; Host/Client butonları tekrar tıklanabilir hale gelir, istediğiniz
zaman yeniden Host veya Client başlatabilirsiniz. Diğer pencereyi de aynı şekilde kesin veya
kapatın.

**WorkshopPrototype.unity ile fark**: `WorkshopPrototype.unity` oyunun **asıl, tek oyunculu**
sahnesi — üretim, sipariş, ekonomi, makine mağazası, Aşama 7'nin yeni üretim alanı, hepsi
sadece orada. O sahnede hiçbir multiplayer butonu **yoktur ve olmayacaktır** — bu kasıtlı bir
ayrım (bkz. `CLAUDE.md`). Multiplayer, ileride (Aşama 18) asıl oyuna entegre edilene kadar
sadece bu ayrı, minimal test sahnesinde yaşayacak.

## Aşama 8 Test Talimatları (✅ kullanıcı tarafından 18/18 ile tamamlandı)

1. Unity Editor'de **`WorkshopPrototype.unity`** açık olduğundan emin olun (Aşama 8 sadece bu
   sahnede).
2. Menüden sırayla **`Shiftworks/Setup/20. Add Power System To Scene`** sonra
   **`21. Add Power Panel To Scene`** çalıştırın (veya tek adımda
   `Apply Power System (Managers + Panel)`). Console'da hata olmadığını kontrol edin.
3. Play Mode'a girin. Mevcut üretim hattının (besleyici → konveyörler → kesim makinesi →
   depo) **tıpkı Aşama 7 öncesi gibi** çalışmaya devam ettiğini doğrulayın — bu en kritik
   regresyon testi.
4. Elektrik panosuna gidin (`-8.3, 0.75, 4` — mağaza terminalinin kuzeyinde, aynı batı
   duvarı boyunca) ve E'ye basın. Panelde Toplam Kapasite (10 kW), Kullanılan, Boş Kapasite,
   Bağlı Ekipman (4: 1 makine + 3 konveyör), Bekleyen, Bakiye ve Yükseltme butonu görünmeli.
5. Makine işlerken (Processing) panele tekrar bakın — Kullanılan kapasitenin 4 kW arttığını,
   makine bitirince tekrar düştüğünü görün (panel her kare canlı güncelleniyor).
6. **Yükseltme testi**: "Kapasiteyi Yükselt" butonuna basın (300 $) — bakiyenin düştüğünü,
   Toplam Kapasitenin 20 kW olduğunu, butonun tekrar tıklanamaz hale geldiğini doğrulayın.
   Panelin dışına çıkıp tekrar girin, durumun kalıcı olduğunu (20 kW) kontrol edin.
7. **Yetersiz güç testi** (isteğe bağlı, zorlamak için): Aşama 6 mağazasından ek bir kesim
   makinesi satın alıp yerleştirin; aynı anda birden fazla makineyi işletmeye çalışıp
   kapasite dolduğunda makinenin bakışta "Yetersiz Güç" yazdığını ve durum ışığının mor
   renge döndüğünü doğrulayın; kapasite boşalınca (diğer makine bitirince) tekrar
   çalışabildiğini kontrol edin.
8. Aşama 7'nin genişletme/geçit sisteminin ve mağaza/sipariş panellerinin bozulmadığını,
   hiçbir panelin birbirine karışmadığını (aynı anda iki panel açılmadığını, Escape'in
   sadece açık paneli kapattığını) doğrulayın.

## Aşama 9 Test Talimatları (✅ kullanıcı tarafından 22/22 ile tamamlandı)

0. **(Yeni — teslimat hatası düzeltmesi)** Menüden **`26. Fix Order Board Accept Feedback`**
   çalıştırın (22-25 zaten çalıştırılmışsa sadece bunu çalıştırmanız yeterli). Bu, her iki
   sipariş panosuna da "başka bir sipariş aktif" uyarısını ekler.
1. `WorkshopPrototype.unity` açıkken menüden sırayla **`22. Create Assembly Product Data`** →
   **`23. Create Assembly Machine Definition And Prefab`** → **`24. Add Assembly Order To
   Scene`** → **`25. Add Assembly Machine Shop To Scene`** → **`26. Fix Order Board Accept
   Feedback`** çalıştırın (veya tek adımda `Apply Assembly Production Chain (Data + Scene)`,
   ardından ayrıca 26'yı). Console'da hata olmadığını kontrol edin — bu sırayla çalıştırmak
   önemli, her adım bir öncekinin ürettiği asset'e bağımlı.
2. Play Mode'a girin. **Regresyon**: mevcut kesim makinesi/konveyör hattı, Aşama 7 genişletmesi
   ve Aşama 8 elektrik panosu hiçbiri bozulmamış olmalı.
3. Orijinal `MachineShopTerminal`'a (`-2.5, 0.75, -6.5`) gidin — hâlâ sadece "Kesim Makinesi"
   satıyor olmalı. Yeni `AssemblyMachineShopTerminal`'a (`-5, 0.75, -6.5`, batıda) gidin —
   "Montaj Makinesi" (450 $) satın alınabilmeli, yerleştirme önizlemesi boş zeminde yeşil,
   duvar/makinelerle çakışan konumlarda kırmızı olmalı (Aşama 6'daki "her yer geçersiz" hatası
   tekrarlanmamalı).
4. Montaj makinesini yerleştirin. Kesim makinesinden 2 adet CutMetalPart üretip (veya
   depodan/elle taşıyarak) montaj makinesine tek tek yerleştirin — ilk parçada makinenin
   bakışta "CutMetalPart Gerekiyor (1/2)" gibi bir ilerleme göstermesi, ikinci parçada
   üretimin başlayıp "Üretim yapılıyor... %X" yazması, 8 saniye sonra "Çıktı Hazır" olması
   beklenir.
5. Üretilen Montaj Parçası'nı (silindir, mor/mavi) elle taşıyıp `PackingStation`'a götürün —
   önce hangi sipariş aktifse onu paketlediğini doğrulayın (iki sipariş panosundan hangisini
   kabul ettiyseniz).
6. **Önce** yeni `AssemblyOrderBoard`'a (`5, 0.75, -6.5`, doğuda) gidip "Montaj Parçası
   Siparişi"ni kabul edin (üretime başlamadan önce). Orijinal sipariş hâlâ aktifse (önceki
   oturumlardan kalmış olabilir) panelde artık kırmızı "Başka bir sipariş zaten aktif — önce
   onu tamamlayıp teslim edin." uyarısı görünmeli ve Kabul Et butonu tıklanamaz olmalı —
   bu durumda önce orijinal `MachineShopTerminal`/`OrderBoard` tarafındaki siparişi
   (3× CutMetalPart) tamamlayıp `ShippingZone`'a teslim edin, sonra tekrar deneyin. Kabul
   başarılı olduğunda 2 Montaj Parçası paketleyip teslim edin, 350 $ aldığınızı doğrulayın.
7. Elektrik panosuna gidip montaj makinesinin "Bağlı Ekipman" sayısını 1 artırdığını, işlerken
   "Kullanılan" kapasiteyi 5 kW artırdığını, yetersiz kapasitede makinenin "Yetersiz Güç"
   gösterdiğini doğrulayın.
8. Üretim Alanı B satın alınmışsa (Aşama 7), montaj makinesini oraya da yerleştirip aynı
   şekilde çalıştığını doğrulayın (aynı `PowerManager`, zemin/collider kuralları değişmedi).

## Aşama 9 Teslimat Hatası Düzeltmesi

Kullanıcı, montaj makinesini çalıştırıp 2 `AssembledComponent` ürettikten sonra bu ürünleri
hiçbir yere teslim edemediğini bildirdi ("ürünler yerde kalıyor").

**Kök neden (kesinleştirildi — sahne dosyası da doğrulandı, kablolama sorunu değil)**:
`OrderBoardPanelUI.Refresh()` içinde "Siparişi Kabul Et" butonu sadece **o panonun kendi
siparişinin** `Available` olup olmadığına bakıyordu — `OrderManager.HasActiveOrder`'ı (aynı
anda sadece BİR siparişin Aktif/Sevkiyata Hazır olabileceği kuralı) hiç kontrol etmiyordu. Bu
Aşama 9'dan önce fark edilemezdi çünkü tek bir sipariş tanımı vardı. Kullanıcının orijinal
"İlk Metal Parça Siparişi"si hâlâ Aktif (veya Sevkiyata Hazır ama teslim edilmemiş) durumdaysa,
yeni "Montaj Parçası Siparişi" panosu "Müsait" gösterip Kabul Et butonunu tıklanabilir
bırakıyordu — ama tıklandığında `OrderManager.TryAcceptOrder` içindeki
`HasActiveOrder` kontrolü **sessizce** reddediyordu, ekranda hiçbir şey değişmiyordu. Oyuncu
bunu fark etmeden ürünleri üretip `PackingStation`'a götürdü, ama `OrderManager.ActiveOrder`
hâlâ eski CutMetalPart siparişi olduğu için ürün türü uyuşmadı ve reddedildi.

**Düzeltme**: `OrderBoardPanelUI.cs`'e `warningText` alanı eklendi; Kabul Et butonu artık
`state == Available && !HasActiveOrder` kontrolünden geçiyor (başka sipariş aktifken
tıklanamaz hale geliyor) ve panelde "Başka bir sipariş zaten aktif — önce onu tamamlayıp
teslim edin." uyarısı görünüyor. `ShiftworksOrdersSetup.cs`'e yeni, idempotent bir düzeltme
menüsü (**`26. Fix Order Board Accept Feedback`**) eklendi — bu menü hem orijinal sipariş
panosuna hem de yeni montaj sipariş panosuna bu uyarı metnini geriye dönük ekliyor.
`PackingStation`/`ShippingZone`/`OrderManager`'ın kendisi **değiştirilmedi** — onlar zaten
doğru çalışıyordu, eksik olan sadece oyuncuya neden "hiçbir şey olmadığını" söyleyen geri
bildirimdi.

**Kullanıcının şu an yapması gereken** (eğer orijinal sipariş hâlâ aktifse): Önce orijinal
"İlk Metal Parça Siparişi"ni tamamlayıp (3× CutMetalPart paketleyip) `ShippingZone`'a teslim
edin — bu, `HasActiveOrder`'ı serbest bırakır. Ancak bundan sonra `AssemblyOrderBoard`'dan
"Montaj Parçası Siparişi"ni kabul edebilirsiniz.

`26. Fix Order Board Accept Feedback` kullanıcı tarafından çalıştırıldı (sahne dosyasında
`WarningText` nesneleri doğrulandı).

## Aşama 9 İkinci Üretim Takılması Düzeltmesi

Kullanıcı, montaj makinesinde ilk `AssembledComponent`'i ürettikten sonra ikinci üretimi
başlatmaya çalışırken ekranın sürekli "E - Montaj Parçası Al (Çıktı Hazır)" gösterdiğini ve
E'nin bir işe yaramadığını bildirdi.

**Kök neden**: Bir hata/veri kaybı değil — `MachineController.GetPromptFor`'un `OutputReady`
metni, oyuncunun elinde bir şey olup olmadığına bakmadan her zaman "E - Al" diyordu.
`PlayerInteractor.HandleMachineInteraction` ise oyuncu bir şey taşıyorsa E'yi her zaman
**besleme** (`TryAcceptInput`) olarak yorumluyor, **alma** (`TryCollectOutput`) olarak değil.
`TryAcceptInput`, makine `OutputReady` (henüz `Idle` değil) olduğu için sessizce reddediyordu
— yani oyuncu elinde bir CutMetalPart (üçüncü tur için önceden almış) taşırken makineye
baktığında, E'ye bastıkça hiçbir şey olmuyordu ve metin hiç değişmiyordu, bu da "takılmış"
gibi görünüyordu. Çıktının gerçekten alınıp alınmadığı, slotun boşalıp boşalmadığı,
çoğalma/kayıp riski — **hiçbiri sorun değildi**, kod seviyesinde doğrulandı.

**Düzeltme**: `GetPromptFor`'un `OutputReady` dalı artık `carriedItem`'a bakıyor — elinde bir
şey varken "Çıktı Hazır — Önce Elinizdekini Bırakın (G) veya Teslim Edin" gösteriyor, boş
elken eskisi gibi "E - X Al (Çıktı Hazır)". **Sadece metin değişti, hiçbir durum/mantık
değişmedi** — "çıktı doluyken yeni üretim başlamaması" kuralı aynen korundu.

## Montaj Makinesi — Yeniden Doğrulama (bu oturum, kod değişikliği yok)

Kullanıcı ikinci üretimin "zorlandığını" bildirdi ve önceki oturumdaki düzeltmenin
(`"Çıktı Hazır — Önce Elinizdekini..."`) gerçek bir hatayı mı yoksa normal davranışı mı
gösterdiğini sordu. Tüm döngü (`TryAcceptInput` → `CompleteProcessing` → `TryCollectOutput`)
satır satır yeniden izlendi: `_bufferedInputCount` her tamamlanan partiden hemen önce 0'a
sıfırlanıyor, `_pendingOutput` toplanınca `null`'a dönüyor, `HasPower`/durum her döngüde doğru
sıfırlanıyor — **gerçek bir hata bulunamadı, hiçbir kod değişmedi**. Gözlemlenen mesaj,
oyuncunun elinde (bir sonraki tur için önceden aldığı) bir ürün varken çıktıyı almaya
çalışmasının **beklenen** sonucu; önceki oturumdaki metin düzeltmesi bunu zaten doğru
açıklıyor. Doğru sıra: eli boşken çıktıyı al → (gerekirse G ile bırak) → 2 CutMetalPart'ı
tek tek yerleştir → üretim otomatik başlar.

## Geliştirme Ortamı Test Parası (F8)

Yeni `Assets/_Project/Scripts/Debugging/DevelopmentCheats.cs` — tamamen `#if UNITY_EDITOR`
içinde (Steam/release build'de sınıf hâlâ var, ama `Update()` boş çalışır, "missing script"
riski yok). F8'e her basışta (`wasPressedThisFrame`, basılı tutmak tekrar tetiklemez)
`EconomyManager.AddFunds(10000)` çağrılır — **gerçek ödül/satın alma sistemiyle aynı tek
giriş noktası**, ayrı bir bakiye yazma yolu yok. Bir panel açıkken (`PlayerInteractor.
IsInputSuspended`) F8 hiçbir şey yapmaz. Console'a `"TEST: +10000$ eklendi"` yazar. HUD'daki
bakiye zaten her kare `EconomyManager.Balance`'ı okuduğu için (`GameHUD`'a hiç dokunulmadı)
anında güncellenir. `ShiftworksDevToolsSetup.cs` (Editor, menü **27**) sahneye
`DevelopmentCheats` nesnesini ekleyip mevcut `EconomyManager`/`Player`'a bağlar.

## Aşama 9 — E Tuşu Otomatik Bırakma (davranış değişikliği)

Kullanıcı, metin düzeltmesine rağmen ("Çıktı Hazır — Önce Elinizdekini...") ikinci üretimde
hâlâ takıldığını bildirdi — ekran görüntüsü, elinde bir CutMetalPart varken montaj makinesine
baktığını doğruladı. Kod incelemesi (yeniden) hiçbir state/veri hatası bulmadı; sorun saf
etkileşim sürtünmesiydi: çıktıyı almak için önce **G** (bırak) sonra **E** (al) — iki ayrı
tuş — gerekiyordu, ve bu adım kullanıcı tarafından art arda üç kez atlanmış görünüyor.

**Değişiklik**: `PlayerInteractor.HandleMachineInteraction` artık, bakılan makine `OutputReady`
durumundaysa ve oyuncunun elinde bir şey varsa, **E'ye basıldığında önce otomatik olarak
elindekini bırakıp aynı basışta çıktıyı alıyor**. Hiçbir ürün kaybolmuyor/çoğalmıyor — bırakılan
nesne olduğu yerde (oyuncunun önünde) kalıyor, istenirse hemen tekrar alınabilir. "Çıktı
doluyken yeni üretim başlamaması" kuralı **değişmedi** — bu sadece E'nin OutputReady'de her
zaman "bu makineyle ilgilen" anlamına gelmesini sağlıyor. `GetPromptFor` metni buna göre
güncellendi: "E - Elinizdekini Bırak ve X Al". Bu, **Stage 3'ün kesim makinesi dahil tüm
`MachineController` örnekleri için geçerli** (tek bir sınıf, iki üretim sistemi yok ilkesi
korundu).

## Aşama 10 Mimarisi (Kayıt/Yükleme Sistemi)

**Tek yetkili orkestratör ilkesi korundu**: `SaveManager` hiçbir sistemin özel alanına
doğrudan dokunmuyor — her sistem kendi `Balance`/`State`/`StoredCount`/vb. üzerinde **kendisi**
tek yetkili kalıyor, sadece artık ayrıca küçük bir `Capture*`/`Restore*` çifti sunuyor
(`EconomyManager.RestoreBalance`, `PowerManager.RestoreUpgradeState`,
`FactoryExpansionManager.RestoreUnlocked`, `OrderManager.RestoreOrderState`,
`MachineController.CaptureProductionState`/`RestoreProductionState`,
`ConveyorSegment.RestoreOccupant`/`ClearOccupant`, `OutputStorage.RestoreStoredCount`,
`RawMaterialFeeder.RestoreStock`, `ProductionStatsTracker.RestoreTotal`,
`PlayerInteractor.RestoreCarriedItem`/`ClearCarriedItemForRestore`,
`FirstPersonController.SetLookRotation`). Bu `Restore*` metotlarının hiçbiri normal oyun
akışından (E tuşu, satın alma, teslimat) erişilemez — yalnızca `SaveManager` çağırır, bu yüzden
mevcut atomik-işlem/tek-yetkili-durum garantileri bozulmadı.

**Sahne-dahili vs. oyuncu tarafından yaratılan ayrımı**: `MachineController.IsPlayerPlaced`
(yeni alan) bunu ayırt ediyor — `false` olan (sahnedeki orijinal `CuttingMachine`) GameObject
adıyla eşleştirilip yerinde durumu geri yükleniyor; `true` olan (mağazadan satın alınmış)
`MachineDefinition` + pozisyon/rotasyondan **yeniden Instantiate** ediliyor, benzersiz bir
`InstanceId` (GUID, `MachinePlacementController.ConfirmPlacement`'ta atanıyor) ile. Yükleme
öncesi `SaveManager.ClearDynamicState()` tüm oyuncu-yerleştirmesi makineleri, tüm
`ShipmentPackage`'ları ve tüm "izlenebilir" (ID'si `SaveCatalog`'da çözülen) serbest ürünleri
yok ediyor — bu, oturum ortasında tekrar yükleme yapılsa bile **asla çoğaltma olmamasını**
garanti ediyor (taze bir sahne açılışında zaten hiçbiri yok, bu adım orada no-op).

**ID çözümleme**: `SaveCatalog`, `ItemDefinition` için zaten var olan `ItemId` alanını,
`MachineDefinition`/`OrderDefinition`/`FactoryExpansionDefinition` için ise asset dosya adını
(`.name`) kullanıyor — hiçbir veri sınıfına yeni bir ID alanı eklenmedi, sıfır şema değişikliği.

**Elektrik kapasitesi tutarlılığı**: Kayıt dosyası `PowerManager.UsedCapacityKw`'yi **hiç
tutmuyor** — sadece `upgradePurchased` (dolayısıyla `TotalCapacityKw`) saklanıyor. Kullanılan
kapasite, her makine/konveyör kendi `Restore*` çağrısı sırasında `TryRequestPower`'ı tekrar
çağırarak **sıfırdan yeniden hesaplanıyor** — eski bir sayı asla körü körüne yüklenmiyor, bu
yüzden çift tahsis veya yanlış kapasite riski yapısal olarak yok.

## Aşama 10'da Oluşturulan/Değiştirilen Dosyalar

**Yeni**: `Assets/_Project/Scripts/SaveSystem/SaveGameData.cs`, `SaveManager.cs`,
`SaveCatalog.cs`, `SaveFileHandler.cs`, `Assets/_Project/Scripts/UI/SaveLoadMenuUI.cs`,
`Assets/_Project/Scripts/Editor/ShiftworksSaveSetup.cs` (menü 28).

**Değiştirilen** (hepsi küçük, davranış koruyan ekler — mevcut hiçbir metot imzası/mantığı
değişmedi): `MachineController.cs`, `ConveyorSegment.cs`, `OutputStorage.cs`,
`RawMaterialFeeder.cs`, `OrderManager.cs`, `PowerManager.cs`, `FactoryExpansionManager.cs`,
`EconomyManager.cs`, `ProductionStatsTracker.cs`, `PlayerInteractor.cs` (+ Escape artık
`SaveLoadMenuUI`'yi açıyor), `FirstPersonController.cs` (+ `SetLookRotation`/`CurrentPitch`),
`MachinePlacementController.cs` (+ yerleştirilen makineyi `MarkAsPlayerPlaced` ile işaretliyor).

Unity içinde henüz çalıştırılmadı: `Shiftworks/Setup/28. Add Save System To Scene` — bunlar
çalıştırılmadan `SaveCatalog`/`SaveManager`/menü sahnede **oluşmaz**; Escape'e basıldığında
`saveLoadMenuUI == null` kontrolü sayesinde hiçbir şey olmaz (hata vermez, sadece menü
açılmaz) — menü 28 koşulmadan önce bu beklenen, güvenli bir durumdur.

## Aşama 10 — Desteklenmeyen / Basitleştirilmiş Durumlar (açıkça belirtiliyor)

- **Konveyör üzerindeki ürün**: Tam ara pozisyonu (0..1 ilerleme) **korunmuyor** — kayıt sadece
  "bu segment dolu mu, hangi ürün türü" bilgisini tutuyor, yüklemede ürün segmentin **başına**
  yerleştiriliyor. Hiçbir ürün kaybolmuyor/çoğalmıyor, sadece görsel olarak o segmentin
  başından yeniden hareket etmeye başlıyor. Kod tabanında zaten hiçbir yerde ara pozisyon kalıcı
  tutulmuyordu (`ConveyorSegment` onu her an iki Transform + canlı bir float'tan hesaplıyor),
  bu yüzden bunu kayıpsız şekilde birebir korumak yeni bir kalıcı durum türü icat etmek
  anlamına gelirdi — kullanıcının isteğiyle (spec madde 9) bilinçli olarak basitleştirildi.
- **Besleyicideki (RawMaterialFeeder) "uçuştaki" tek parça**: `stock` sayısı doğru korunuyor,
  ama stok'tan düşülmeden önce zaten üretilmiş olabilecek tek bir "beklemedeki" parça
  yüklemede yeniden üretiliyor (stok hiç etkilenmedi, bu yüzden çift sayım/kayıp yok) —
  pratikte bu, bir saniyenin çok altında bir görsel farktır.
- **Sahne süsleri (TestBox_1-4, orijinal 5 MetalSheet yığını)**: Bilinçli olarak **hiç
  kaydedilmiyor** — `ItemDefinition`'ı olmayan (`CarryableItem.Definition == null`) nesneler
  tamamen dokunulmadan bırakılıyor. `MetalSheet`/`CutMetalPart`/`AssembledComponent` gibi
  gerçek `ItemDefinition`'a sahip **serbest** (elde değil, konveyörde değil, depoda değil)
  örnekler ise normal şekilde kaydedilip yüklemede yeniden oluşturuluyor (yoksa yüklemede
  "orijinal" MetalSheet yığınıyla kaydedilmiş MetalSheet'ler çakışıp çoğalırdı — bu yüzden
  yükleme öncesi TÜM izlenebilir serbest ürünler önce yok ediliyor, sonra tam olarak kayıtta
  ne varsa yeniden oluşturuluyor).
- **Kayıt sürümü/migration**: Şimdilik tek sürüm (`saveVersion = 1`). Farklı bir sürüm numarası
  okunursa yükleme **reddediliyor** ve kullanıcıya anlaşılır bir hata gösteriliyor (oyun
  çökmez) — kapsamlı bir migration sistemi kasıtlı olarak yazılmadı (spec madde 2'nin izniyle).
- **Bozuk/eksik JSON**: `SaveFileHandler.TryRead` her şeyi try/catch içinde yapıyor; bozuk bir
  dosya veya eksik alanlar oyunun çökmesine değil, anlaşılır bir hata mesajına yol açıyor.
  Yazma da geçici dosya + yedek deseniyle korunuyor (bkz. "Aşama 10 Mimarisi").
- **Oturum ortasında yükleme** (sadece kapatıp-açma değil) kod seviyesinde destekleniyor
  (`ClearDynamicState` bunun için var) ama **hiç test edilmedi** — asıl test odağı "kaydet →
  Unity/Play Mode'u kapat-aç → yükle" senaryosu olmalı.

## Aşama 10 Test Talimatları

1. `WorkshopPrototype.unity` açıkken **`Shiftworks/Setup/28. Add Save System To Scene`**
   çalıştırın. Console'da hata olmadığını kontrol edin.
2. Play Mode'a girin, biraz oynayın: en az bir sipariş kabul edin, montaj/kesim makinesi
   çalıştırın, biraz CutMetalPart/MetalSheet elinizde veya yerde bırakın, isterseniz Üretim
   Alanı B'yi satın alın.
3. **Escape**'e basın — "OYUN MENÜSÜ" paneli açılmalı (Kaydet/Yükle/Yeni Oyun/Devam Et).
   Diğer panellerden (sipariş/mağaza/elektrik) biri açıkken Escape'in **o paneli kapattığını**,
   bu menüyü **açmadığını** doğrulayın (çakışma olmamalı).
4. **"Oyunu Kaydet"**e basın — yeşil bir "Oyun kaydedildi." mesajı görmelisiniz.
   `%USERPROFILE%\AppData\LocalLow\<ŞirketAdı>\Shiftworks\shiftworks_save.json` dosyasının
   oluştuğunu (Application.persistentDataPath) ve okunabilir JSON içerdiğini kontrol edin.
5. Play Mode'u **durdurun** (tam bir "oyundan çık" simülasyonu için), tekrar **Play**'e basın
   (taze bir sahne yüklemesi).
6. Escape → **"Oyunu Yükle"**ye basın — yeşil "Oyun yüklendi." mesajı. Bakiyenizin,
   siparişinizin, makinelerinizin, elinizdeki/yerdeki ürünlerin kaydettiğiniz anla **aynı**
   olduğunu doğrulayın. Oyuncu konumunuzun/bakış yönünüzün de geri geldiğini kontrol edin.
7. Elektrik panosuna gidip "Bağlı Ekipman"/"Kullanılan" kapasitenin **doğru** (çift sayılmamış)
   olduğunu doğrulayın.
8. **Regresyon**: kesim/montaj makineleri, konveyör hattı, mağaza, siparişler, F8 test parası
   (yalnızca Editor'de), fabrika genişletmesi hâlâ normal çalışıyor mu kontrol edin.
9. (İsteğe bağlı) "Yeni Oyun"a basıp kaydın silinmediğini, "Oyunu Yükle"nin hâlâ çalıştığını
   doğrulayın.

## Aşama 11A Mimarisi (Gerçek Multiplayer Co-op — Bağlantı + Hareket)

Kapsam kullanıcı tarafından açıkça **11A'ya** daraltıldı: "iki oyuncunun aynı fabrikaya
bağlanması ve hareket etmesi". Ekonomi/üretim/sipariş/elektrik/kayıt-yükleme'nin
host-authoritative hale getirilmesi (11B-11E) **bu oturumda yapılmadı**, kullanıcının kendi
isteğiyle 11A testi onaylanana kadar bekliyor.

**Mevcut kod neden değişmeden yeniden kullanılabildi**: Aşama 6 civarında yapılan
`MultiplayerPrototype.unity` teknik prototipi tam olarak bu amaç için (oyuncu hareketinin ağ
senkronizasyonu) tasarlanmıştı — `NetworkPlayer.prefab` (CharacterController + var olan
`FirstPersonController`, varsayılan **kapalı** + `NetworkObject` + owner-authoritative
`NetworkTransform`), `NetworkPlayerSetup` (sadece sahip istemcide FPC/kamera/AudioListener'ı
açar) ve `MultiplayerConnectionUI` (Host/Client/Disconnect paneli, 8s bağlantı zaman aşımı).
Kullanıcının kendi isteği de bunu doğruluyor: "Oyuncu hareketinin ağ senkronizasyonu mevcut
prototipten uyarlanabilir." 11A bu üç script'e **hiçbir değişiklik yapmadan** aynen yeniden
kullandı — tek yeni script, iki oyuncunun aynı noktada doğup sıkışmaması için eklenen
spawn-noktası dağıtımı.

**Yeni sahne, tek-oyunculu oyun dokunulmadan**: `WorkshopPrototype.unity` hiç açılıp
üzerine yazılmadı — yalnızca `AssetDatabase.CopyAsset` ile `WorkshopMultiplayer.unity` adlı
bir **kopyası** oluşturuldu (menü 29), sonra o kopyanın üzerinde çalışıldı. Kopya, gerçek
fabrikanın tüm zemin/duvar/makine/konveyör/sipariş panolarını **aynı üretim kodunu kullanarak**
içeriyor (ayrı bir üretim kod tabanı yok — sadece sahne verisi kopyalandı). Kopyadan sadece
tek-oyunculu `Player.prefab` örneği (`PlayerInteractor`/`FirstPersonController` içeren kök
nesne) kaldırıldı, çünkü multiplayer'da oyuncular `NetworkPlayer.prefab`'tan dinamik olarak
doğuyor. `PlayerStart` nesnesi (eski tek-oyunculu başlangıç noktası, (0,0,-6)) **silinmedi** —
spawn noktası 1 olarak yeniden kullanıldı; 2m yanına yeni bir `PlayerSpawnPoint_2` eklendi.

**Spawn noktası dağıtımı — `Shiftworks.Multiplayer.MultiplayerSpawnPoints`**: NGO'nun kendi
`NetworkManager.ConnectionApprovalCallback`'ini kullanıyor (özel bir spawn RPC'si yok).
`NetworkConfig.ConnectionApproval = true` yapıp callback'i kayıt ediyor; her onay isteğinde
`spawnPoints` dizisinden sırayla bir nokta seçip `response.Position`/`Rotation` olarak
ayarlıyor. NGO kaynağı doğrulandı: host'un **kendi yerel bağlantısı da** bu callback'ten
geçiyor (`HostServerInitialize`), yani host spawn noktası 1'i, ilk bağlanan client spawn
noktası 2'yi alıyor — iki oyuncu asla aynı yerde doğmuyor. Round-robin olduğu için 2'den
fazla oyuncuya da (gelecekte) otomatik genişler.

**Bağlantı paneli**: `MultiplayerConnectionUI`, önceki prototipteki ile birebir aynı script,
yeni sahnede yeni bir `WorkshopConnectionUICanvas` üzerinde yeniden inşa edildi (Host
Başlat/Client Bağlan/Bağlantıyı Kes + durum metni, varsayılan IP `127.0.0.1:7777`).

**Build Settings**: `WorkshopMultiplayer.unity`, build listesinde **ilk ve etkin** sahne
olarak ayarlandı (menü 30, `EnsureStartupScene`); `MultiplayerPrototype.unity`'nin build
listesindeki girdisi **silinmedi**, sadece devre dışı bırakıldı — sahne dosyasının kendisi ve
Build Settings'teki kaydı olduğu gibi duruyor, istenirse pencereden tekrar etkinleştirilebilir.

## Aşama 11A'da Oluşturulan Dosyalar

- Yeni: `Assets/_Project/Scripts/Multiplayer/MultiplayerSpawnPoints.cs` — host tarafı
  `ConnectionApprovalCallback` ile spawn noktası dağıtımı.
- Yeni: `Assets/_Project/Scripts/Editor/ShiftworksWorkshopMultiplayerSetup.cs` (menü 29-30 +
  "Apply Workshop Multiplayer") — `WorkshopMultiplayer.unity`'yi `WorkshopPrototype.unity`'den
  kopyalayıp tek-oyunculu `Player`'ı kaldırır (29), ardından NetworkManager/UnityTransport/
  spawn noktaları/bağlantı panelini ekler ve Build Settings'i günceller (30). Her iki menü de
  idempotent (ikinci çalıştırmada "zaten var, dokunulmadı" ile çıkar).
- Yeni (henüz Unity'de oluşturulmadı — menü 29 ilk çalıştırıldığında ortaya çıkacak): sahne
  asset'i `Assets/_Project/Scenes/WorkshopMultiplayer.unity`.
- Değiştirilen: **Hiçbir mevcut script değiştirilmedi.** `FirstPersonController`,
  `PlayerInteractor`, `NetworkPlayerSetup`, `MultiplayerConnectionUI`, `NetworkPlayer.prefab`,
  `WorkshopPrototype.unity`, `MultiplayerPrototype.unity` bu aşamada **hiç dokunulmadı**
  (Build Settings'teki etkin/devre dışı bayrağı dışında).

## Aşama 11A — Desteklenmeyen / Bilinçli Sınırlar (açıkça belirtiliyor)

- Ekonomi, siparişler, makineler, üretim, elektrik, fabrika genişletmesi, kayıt/yükleme —
  **hiçbiri** henüz ağ üzerinden senkronize değil. `WorkshopMultiplayer.unity`'deki bu
  sistemler her istemcide **ayrı ve bağımsız** çalışıyor (host'un ekonomisi client'ınkini
  etkilemiyor). Bu, 11B (ekonomi/makine), 11C (eşya/üretim/konveyör) ve 11D (elektrik/
  sipariş/paketleme/genişletme) kapsamına bilinçli olarak bırakıldı.
- Hiçbir panel (sipariş panosu, makine mağazası, elektrik panosu, kayıt/yükleme menüsü)
  multiplayer sahnesinde **açılamaz** — bunları açan `PlayerInteractor`, kaldırılan
  tek-oyunculu `Player` nesnesinin üzerindeydi; `NetworkPlayer.prefab`'ta henüz yok (11C).
- Oyuncuların birbirinin eşyasını/işini görmesi, taşıması veya etkilemesi yok — henüz sadece
  hareket/kamera senkronize.
- Bağlantı kopması/yeniden bağlanma/geç katılım (late-join) davranışı henüz özel olarak ele
  alınmadı — NGO'nun varsayılan davranışına bırakıldı (11E kapsamı).
- Bu script'ler gerçek bir Unity derlemesiyle **henüz test edilmedi** (Editor kapalıyken
  yazıldı) — menü 29/30 ilk çalıştırıldığında olası derleme hatası riski var.

## Aşama 11A Test Talimatları (tek bilgisayarda: Editor = Host, Windows Build = Client)

1. Unity Editor'de **`Shiftworks/Setup/29. Create WorkshopMultiplayer Scene (Copy)`**
   çalıştırın — Console'da hata olmadığını ve `Assets/_Project/Scenes/
   WorkshopMultiplayer.unity` dosyasının oluştuğunu kontrol edin.
2. **`Shiftworks/Setup/30. Add Networking To WorkshopMultiplayer Scene`** çalıştırın —
   Console'da hata olmadığını kontrol edin. (Tek adımda ikisini birden yapan
   "Apply Workshop Multiplayer (Scene + Networking)" menüsü de var.)
3. **File > Build Settings** açıp `WorkshopMultiplayer` sahnesinin listede **ilk sırada ve
   işaretli** olduğunu doğrulayın (bu menü otomatik ayarlar, ama kontrol edin).
4. **Windows build alın** (File > Build Settings > Build) — bu .exe **Client** rolünü
   oynayacak.
5. Build tamamlandıktan sonra **Unity Editor'de** `WorkshopMultiplayer.unity` sahnesinin açık
   olduğundan emin olup **Play**'e basın. Sol üstteki panelde **"Host Başlat"**a tıklayın —
   durum "Host aktif (0 istemci bağlı)" olmalı, karakter fabrikada hareket edebilmeli.
6. Az önce aldığınız **.exe'yi çalıştırın** (ayrı bir pencere açılacak). Aynı panelde IP
   kutusunda `127.0.0.1` yazılı olduğunu doğrulayıp **"Client Bağlan"**a tıklayın.
7. Her iki pencerede de durum **"Host aktif (1 istemci bağlı)"** / **"Client bağlı"**
   olmalı. İki karakterin **farklı noktalarda** (üst üste binmeden, ~2m arayla) doğduğunu
   doğrulayın.
8. Her iki pencerede de kendi karakterinizi (WASD + mouse) hareket ettirip, **diğer
   pencerede o hareketin göründüğünü** doğrulayın (owner-authoritative senkronizasyon).
9. **Regresyon**: `WorkshopPrototype.unity`'yi açıp normal tek-oyunculu oyunun (hareket,
   etkileşim, üretim, kayıt/yükleme) hâlâ sorunsuz çalıştığını doğrulayın —
   `WorkshopMultiplayer.unity` sadece bir kopya, orijinal sahneye hiç dokunulmadı.
10. (İsteğe bağlı) Client penceresinde **"Bağlantıyı Kes"**e basıp host tarafının
    "Host aktif (0 istemci bağlı)"a döndüğünü doğrulayın.

## Aşama 11B Mimarisi (Ekonomi + Makine Satın Alma/Yerleştirme — Host-Authoritative)

Kapsam, kullanıcının 11B tanımına göre ikiyle sınırlı tutuldu: **ortak bakiye** ve **makine
satın alma/yerleştirme**. Siparişler, elektrik ve fabrika genişletmesinin ekonomik etkileri
(11D'nin kapsamı) hâlâ ağ üzerinden paylaşılmıyor — bkz. "Desteklenmeyen/Bilinçli Sınırlar".

**Neden `EconomyManager`/`MachinePurchaseManager`'ın kendisi değişmedi**: Bu iki sınıfın
`TrySpend`/`TryPurchase`/`TryConsumePending` metotları hâlâ TEK atomik yetkili — host-authoritative
dönüşüm bunların NE YAPTIĞINI değiştirmedi, sadece KİMİN onları doğrudan çağırabileceğini
sınırladı (yalnızca sunucu/host) ve diğer herkesin isteğini ona yönlendirdi.

**`Shiftworks.Multiplayer.NetworkEconomyBridge`** (yeni, `WorkshopMultiplayer.unity`'ye
özel — `WorkshopPrototype.unity`'de hiç yok, dolayısıyla tek oyunculu oyunda hiçbir kod yolu
bundan geçmiyor):
- `NetworkVariable<int>` ile bakiyeyi tek yönlü ayna gibi senkronize eder: sunucu tarafında
  her `Update()`'te host'un kendi `EconomyManager.Balance`'ı ile karşılaştırılır, değiştiyse
  `NetworkVariable` güncellenir. Bu, sadece bu köprüden geçen satın almaları değil, **host'un
  yerel ekonomisini etkileyen her şeyi** (F8 test parası, sipariş ödülleri, elektrik/genişletme
  satın alımları — hepsi hâlâ yerel) otomatik olarak yayınlar; host'un o anki gerçek bakiyesi
  her zaman doğru şekilde herkese gider.
- Her istemci, `NetworkVariable` değiştiğinde kendi YEREL `EconomyManager`'ına
  `RestoreBalance(yeniDeğer)` çağırır (Aşama 10'un save/load'ta kullandığı AYNI giriş noktası)
  — bu sayede `GameHUD`/`MachineShopPanelUI` **hiç değişmeden** doğru senkronize bakiyeyi
  gösterir.
- Bekleyen (satın alınmış ama henüz yerleştirilmemiş) makine sayıları da aynı mantıkla
  `NetworkList<int>` ile senkronize edilir — `SaveCatalog.AllMachines` sırasına göre
  indekslenir (Aşama 10'un kataloğu yeniden kullanıldı, ikinci bir makine listesi
  oluşturulmadı). Bu havuz **oyuncuya özel değil, paylaşılan** — biri satın alırsa, başka bir
  oyuncu gidip yerleştirebilir (paylaşılan şirket cüzdanıyla tutarlı tasarım kararı).
- Satın alma/yerleştirme isteği: **host** için `RequestPurchase`/`RequestPlacement` senkron
  çalışır (tek oyunculu ile birebir aynı davranış). **Uzak istemci** için bir `ServerRpc` host'a
  gider, host AYNI `TryPurchase`/`TryConsumePending` çağrısını kendi yerel nesneleri üzerinde
  yapar, sonra sadece o isteği gönderen istemciye hedefli bir `ClientRpc` ile sonucu
  (başarılı/yetersiz bakiye/bekleyen kalmadı) bildirir.
- Yerleştirilen makine `NetworkObject.Spawn()` ile ağa açılır (yalnızca bu — `NetworkTransform`
  eklenmedi, çünkü makineler yerleştirildikten sonra hareket etmiyor; NGO'nun kendi spawn
  mesajı başlangıç pozisyon/rotasyonunu zaten taşıyor — kurulu paket kaynağından doğrulandı:
  `NetworkObject.cs`'teki `SceneObject` struct'ı `Position`/`Rotation`/`Scale` alanlarını
  spawn mesajının kendisine gömüyor). Bu sayede host OLMAYAN bir oyuncunun satın aldığı makine
  de **herkesin ekranında aynı yerde** belirir.
- `CuttingMachine.prefab`/`AssemblyMachine.prefab`'a eklenen `NetworkObject` bileşeni, bu
  prefab'ların `WorkshopPrototype.unity`'deki MEVCUT sahne-içi örneklerine de otomatik
  yayılır (Unity prefab mirası) — kurulu Netcode paket kaynağından doğrulandı:
  `NetworkObject.Awake()`/`OnDestroy()` hiçbir `NetworkManager` yokken güvenle no-op çalışıyor,
  dolayısıyla tek oyunculu sahnede hiçbir risk yok.

**NetworkPlayer.prefab'a eklenen etkileşim**: `PlayerInteractor` + `MachinePlacementController`
artık NetworkPlayer.prefab'ta da var — ÖNCEDEN tamamen eksikti ("Carry/interact/production
systems are intentionally not part of this prototype" notuyla). Her ikisi de kendi kamerası/
`HoldPoint`'i/`InteractionPromptCanvas`'ı ile tamamen **kendi içinde** (prefab-local), tıpkı
tek oyunculu `Player.prefab`'ın kendi kopyaları gibi — hiçbir sahne referansı gerekmediği için
`NetworkPlayerSetup`'a yeni bir çalışma-zamanı kablolama adımı eklenmedi, sadece
`FirstPersonController` ile AYNI "sadece sahip için etkinleştir" deseni uygulandı (`GetComponent`
ile bulunup `enabled = isLocal` yapılıyor). `saveLoadMenuUI` kasıtlı olarak kablosuz bırakıldı
— Escape'e basınca `PlayerInteractor`'ın zaten var olan güvenli fallback'i ("menü yoksa sadece
imleci serbest bırak") devreye giriyor; Aşama 11E'ye kadar bu böyle kalacak.

## Aşama 11B'de Oluşturulan/Değiştirilen Dosyalar

- Yeni: `Assets/_Project/Scripts/Multiplayer/NetworkEconomyBridge.cs`.
- Yeni: `Assets/_Project/Scripts/Editor/ShiftworksMultiplayerEconomySetup.cs` (menü **31**:
  NetworkPlayer.prefab'a etkileşim ekler; menü **32**: makine prefab'larına `NetworkObject`
  ekler + `NetworkPrefabsList` oluşturur + köprüyü sahneye ekler; + "Apply Multiplayer
  Economy" birleşik menüsü).
- Değiştirilen (küçük, additive): `SaveCatalog.cs` (`AllMachines` erişimcisi),
  `MachinePurchaseManager.cs` (`RestorePendingCount`), `MachineShopPanelUI.cs` (`OnBuyClicked`
  köprü varsa onu kullanır), `MachinePlacementController.cs` (`ConfirmPlacement` köprü varsa
  onu kullanır + yanıt beklerken çift-tıklama koruması), `NetworkPlayerSetup.cs`
  (`PlayerInteractor`/`MachinePlacementController`'ı sahip için etkinleştirir).
- Değiştirilen prefab'lar: `NetworkPlayer.prefab` (etkileşim eklendi),
  `CuttingMachine.prefab`/`AssemblyMachine.prefab` (`NetworkObject` eklendi — tek oyunculu
  sahnedeki örnekleri de otomatik etkiler, yukarıda açıklanan nedenle risksiz).
- **Hiç dokunulmayanlar**: `EconomyManager.cs`, `MachinePurchaseManager.TryPurchase/
  TryConsumePending`, `MachineShopTerminal.cs`, `WorkshopPrototype.unity`, `Player.prefab`,
  `MultiplayerPrototype.unity`.

## Aşama 11B — Desteklenmeyen / Bilinçli Sınırlar (açıkça belirtiliyor)

- Siparişler, elektrik faturaları/yükseltmeleri, fabrika genişletmesi satın alımları hâlâ
  **ağ üzerinden paylaşılmıyor** — her istemcide yerel ve bağımsız. Bu kalemlerden biri
  host'un bakiyesini değiştirirse, bu değişiklik senkronize balance'a yansır (Update polling
  sayesinde), AMA aynı anda bir istemcinin KENDİ yerel-only kazancı (örn. tek başına
  tamamladığı bir sipariş) bir sonraki senkronizasyonda host'un bakiyesiyle **ezilebilir** —
  bu, 11B'nin bilinçli sınırının doğal bir sonucu. **Test ederken bunu karıştırmamak için**:
  11B testinde SADECE makine satın alma/yerleştirmeyi test edin, siparişleri test etmeyin
  (siparişler 11D'de ağ üzerinden paylaşılacak).
- Yerleştirilen makinenin KENDİSİ (pozisyonu) herkese görünür ama **üretim durumu**
  (reçete ilerlemesi, arabellekteki girdi, çıktı hazır mı) **senkronize değil** — her
  istemci bu makineyi kendi yerel/bağımsız mantığıyla işletiyor. Aşama 11C'nin kapsamı.
- Eşya taşıma (G ile alma/bırakma/fırlatma) artık her oyuncunun KENDİ ekranında çalışıyor
  (çünkü `PlayerInteractor` artık NetworkPlayer'da var) ama **diğer oyunculara görünmüyor**
  — taşınan eşyanın kendisi henüz bir `NetworkObject` değil. Aşama 11C'nin kapsamı.
- Bağlantı kopması sırasında bir isteğin yanıtı hiç gelmeyebilir (`_waitingForNetworkResult`/
  `_pendingPurchaseCallback` sonsuza kadar beklemede kalır) — bu durumda ilgili panel/yerleştirme
  modu manuel olarak kapatılmalı (Escape). Otomatik zaman aşımı eklenmedi (11B kapsamında
  gerekli görülmedi, MultiplayerConnectionUI'ın kendi 8s bağlantı zaman aşımı ayrı bir
  mekanizma).
- Bu script'ler gerçek bir Unity derlemesiyle **henüz test edilmedi**.

## Aşama 11B Test Talimatları (tek bilgisayarda: Editor = Host, Windows Build = Client)

1. Önce **11A'nın kurulumu** tamam olmalı (menü 29-30 çalıştırılmış, `WorkshopMultiplayer.unity`
   var). Şimdi sırayla **`Shiftworks/Setup/31. Add Interaction To NetworkPlayer Prefab`** ve
   **`32. Add Network Economy Bridge To WorkshopMultiplayer Scene`** çalıştırın (veya
   "Apply Multiplayer Economy" birleşik menüsü). Console'da hata olmadığını kontrol edin.
2. Yeni bir **Windows build** alın (NetworkPlayer.prefab ve makine prefab'ları değişti).
3. Editor'de `WorkshopMultiplayer.unity` açıkken Play'e basıp **Host Başlat**. Build'i açıp
   **Client Bağlan**.
4. **Host tarafında**: bir makine mağazası terminaline (E) gidin, bir makine satın alın
   ("Satın Al") — bakiyenin düştüğünü görün. **Client tarafında da aynı anda bakiyenin
   düştüğünü** doğrulayın (paylaşılan bakiye).
5. **Client tarafında**: aynı veya başka bir terminalden makine satın alın — host tarafında
   da bakiyenin düştüğünü doğrulayın. "Yerleştir"e basıp bir yere makineyi koyun.
6. **Host tarafında, aynı anda, o makinenin client'ın yerleştirdiği konumda belirdiğini**
   doğrulayın (host'un ekranında da görünmeli — NetworkObject.Spawn senkronizasyonu).
7. Tersini de deneyin: host bir makine satın alıp yerleştirsin, client'ın ekranında da
   belirdiğini doğrulayın.
8. **Yetersiz bakiye testi**: bakiye yokken client'tan satın almaya çalışın — "Yetersiz
   bakiye!" uyarısının client'ta göründüğünü, hiçbir tarafta para düşmediğini doğrulayın.
9. **Regresyon**: `WorkshopPrototype.unity`'yi açıp tek-oyunculu makine satın alma/yerleştirmenin
   hâlâ sorunsuz çalıştığını doğrulayın.
10. (İsteğe bağlı) Eşya taşımanın (G) her oyuncunun kendi ekranında çalıştığını ama diğer
    oyuncuya görünmediğini gözlemleyin — bu, belgelenen bilinçli sınır, hata değil.

## Aşama 11 — Bağlantı ve Kamera Hatası Teşhisi/Düzeltmesi (11B testinden ÖNCE bulundu)

Kullanıcı multiplayer testinde şu belirtileri bildirdi: Client bağlantı ekranında takılabiliyor,
Editor Game penceresinde zaman zaman "Display 1 — No cameras rendering" uyarısı, client
bağlandığında oyuncu kontrolü çalışmıyor, bir süre sonra bağlantı başarısız oluyor gibi.

**Önemli teşhis bulgusu**: İnceleme sırasında `NetworkPlayer.prefab`'ın son değişim tarihinin
Aşama 11A'dan (23 Ekim) kaldığı, sahnede `NetworkEconomyBridge`'in, `CuttingMachine.prefab`'da
`NetworkObject`'in **bulunmadığı** ve `git status`'un hiçbir fark göstermediği görüldü — yani
**Aşama 11B'nin kurulum menüleri (31, 32) hiç çalıştırılmamış**. Kullanıcının bildirdiği hata
11B'nin kodundan değil, **hâlâ sadece Aşama 11A'nın kurulu olduğu sahneden** kaynaklanıyor —
11B'nin kendi script'leri (`NetworkEconomyBridge` vb.) proje içinde var ama sahneye/prefab'a
henüz hiç kablolanmamış, dolayısıyla bu bugdan sorumlu olamazlar.

**Bulunan gerçek, doğrulanabilir sorun**: `WorkshopMultiplayer.unity`'de, bir oyuncu doğana
kadar (host'un kendi oyuncusu dahil) sahnede **etkin hiçbir kamera yok** — her
`NetworkPlayer.prefab` örneğinin kendi kamerası varsayılan olarak kapalı başlıyor ve sadece
`NetworkPlayerSetup.OnNetworkSpawn`'da sahibi için açılıyor. Bu pencere boyunca Unity'nin
"No cameras rendering" uyarısı **gerçek ve beklenen** bir durumdu — kozmetik gürültü değil.
Bağlantının "takılı" görünmesinin bir kısmı da muhtemelen bundandı: ekran siyah olunca
kontrolün çalışıp çalışmadığı anlaşılamıyor. Bağlantının kendisinin neden başarısız olduğuna
dair kesin bir derleyici/mantık hatası kodda bulunamadı (NetworkManager/UnityTransport/
MultiplayerConnectionUI/MultiplayerSpawnPoints incelendi, hepsi doğru görünüyor;
`EditorBuildSettings.asset` da doğru sahneyi gösteriyor) — bu yüzden en küçük güvenli düzeltme
uygulandı ve **tüm bağlantı/spawn zincirine teşhis logları eklendi**, bir sonraki test
denemesinde tam başarısızlık noktasını kesin olarak görebilmek için.

**Yapılan değişiklikler**:
- Yeni: `Shiftworks/Setup/33. Add Lobby Camera Fallback To WorkshopMultiplayer Scene` menüsü
  — sahneye her zaman açık bir `LobbyCamera` (+ `AudioListener`) ekler. `NetworkPlayerSetup`
  artık yerel oyuncu doğduğunda bunu kapatıyor, oyuncu despawn olduğunda (bağlantı kesilince)
  geri açıyor — asla gerçek bir oyuncu kamerasıyla aynı anda ikisi birden açık olmuyor.
- `NetworkPlayerSetup.cs`: `OnNetworkSpawn`/yeni `OnNetworkDespawn`'a istenen teşhis logları
  eklendi ("NetworkPlayer spawn edildi", "Yerel kamera etkinleştirildi", "Oyuncu kontrolü
  etkinleştirildi", "NetworkPlayer despawn edildi").
- `MultiplayerConnectionUI.cs`: Host başlatma/Client bağlantı isteği/Client bağlandı/
  bağlantı kesildi-reddedildi/sunucu durduruldu için loglar eklendi.
  `NetworkManager.DisconnectReason` artık okunup hata mesajına ekleniyor (önceden her
  zaman sabit "Host bulunamadı" yazıyordu — şimdi gerçek sebep biliniyorsa gösteriliyor).
- `MultiplayerSpawnPoints.cs`: Her bağlantı onayı için hangi spawn noktasının atandığını
  loglayan bir satır eklendi.

**Kullanıcıya düşen adımlar (rapor sonunda da tekrarlandı)**: Bu düzeltme sahneye yeni bir
GameObject eklediği için `WorkshopMultiplayer.unity` yeniden kaydedilmeli (menü 33) ve
**yeni bir Windows build alınmalı**. Sorun bu düzeltmeyle çözülmese bile, artık Console'da
tam olarak hangi adımda (host başlatma / client isteği / onay / spawn / kamera) durduğu
görülebilecek — bir sonraki denemenin logları paylaşılırsa kök neden kesinleştirilebilir.
**Aşama 11B hâlâ "tamamlandı" işaretlenmedi** — kullanıcının isteği doğrultusunda, hem 11B'nin
kendi kurulumu (menü 31-32) hem de bu bağlantı/kamera düzeltmesi kullanıcı tarafından test
edilip onaylanmadan sonraki aşamaya geçilmeyecek.

## Aşama 11B — Client Satın Alma Çökme/Donma Hatası Teşhisi/Düzeltmesi

Önceki düzeltmeden sonra kullanıcı bağlantıyı başarıyla kurdu (host/client çalıştı), ama
**client tarafında bir makine satın alırken client donuyor/çalışamaz hale geliyor**du.

**Teşhis**: `EconomyManager`, `MachinePurchaseManager`, `MachineController.Awake` (PowerManager
kaydı), `NetworkEconomyBridge`'in tüm RPC zinciri, satın alma/yerleştirme aşamaları ayrı ayrı
satır satır incelendi. **Kesin, tek bir istisna kodda doğrulanamadı** (Unity çalıştırılıp
gerçek bir stack trace görülmeden bu mümkün değil) — ama incelemede **gerçek, doğrulanabilir
bir dayanıklılık açığı** bulundu: `RequestPurchaseServerRpc`/`RequestPlacementServerRpc`
(host'ta çalışan RPC metotları) hiçbir try/catch içermiyordu. Host tarafında çalıştırma
sırasında (örn. `Instantiate`/`MachineController.Awake`/`NetworkObject.Spawn` zincirinde)
HERHANGİ bir istisna fırlasaydı, RPC metodu `NotifyPurchaseResultClientRpc`/
`NotifyPlacementResultClientRpc`'yi hiç çağırmadan sonlanırdı — bu da isteği gönderen
**client'ı sonsuza kadar yanıt bekler halde bırakırdı** (`_pendingPurchaseCallback`/
`_pendingPlacementCallback` hiç tetiklenmez, `MachinePlacementController._waitingForNetworkResult`
hiç `false` olmaz). Bu tam olarak "satın alma sırasında client donuyor" belirtisiyle örtüşüyor.
Host tarafında bu istisna Console'a yazılırdı ama **client'a hiç bildirilmezdi** — kullanıcının
bunu "çöktü" olarak yorumlaması tutarlı.

**Yapılan düzeltme (NetworkEconomyBridge.cs)**:
- `RequestPurchaseServerRpc`/`RequestPlacementServerRpc` artık **try/catch** içinde — host
  tarafında ne olursa olsun (istisna dahil) client'a **her zaman** bir `ClientRpc` yanıtı
  gider (istisna durumunda `success=false` + Console'a tam stack trace log'lanır).
- **İkinci, bağımsız güvenlik ağı**: client tarafında `_pendingPurchaseCallback`/
  `_pendingPlacementCallback` 6 saniye içinde yanıtlanmazsa (yanıt paketi kaybolsa/host
  çökse bile), istemci kendini otomatik olarak "Host yanıt vermedi" hatasıyla kurtarır —
  asla sonsuza kadar beklemez.
- `ExecutePlacementOnServer`'a `definition.MachinePrefab == null` koruması eklendi (yanlış
  yapılandırılmış bir makine tanımı artık sessizce patlamak yerine açıkça loglanıp bekleyen
  sayısını geri ekliyor).
- Satın alma/yerleştirme zincirinin her adımına (istek gönderildi, host sonucu, client'a
  gelen yanıt, NetworkObject.Spawn) `[Shiftworks][MP]` loğu eklendi.

**Kullanıcıya düşen adımlar**: Bu değişiklik sadece script, sahne/prefab'a dokunmadı — yine
de **yeni bir Windows build gerekiyor** (derlenen kod değişti). Sorun TEKRARLANIRSA, artık
host'un Unity Editor Console'unda **tam istisna ve stack trace** görünecek (önceden sessizce
yutuluyordu) — bu, kök nedeni kesinleştirmek için yeterli olacak. Ayrıca Windows build'in
kendi log dosyası `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Shiftworks6\Player.log`'da
(bu proje `companyName: DefaultCompany`, `productName: Shiftworks6` kullanıyor) — Notepad ile
açılabilir, en alttaki satırlar en son olaylardır.

**Aşama 11B hâlâ "tamamlandı" işaretlenmedi.**

## Aşama 11B — Gerçek Kök Neden Bulundu: Bağlantı Zaman Aşımı Mekanizması Başarılı Bağlantıyı Kesiyordu

Kullanıcı Windows client'ın `Player.log`'unu paylaştı — bu, önceki iki raporun ("client donuyor")
**asıl, tek ve kesin kök nedenini** ortaya çıkardı. Log sırası: `StartClient çağrıldı` →
**her iki `NetworkPlayer` başarıyla spawn oldu** → **yerel kamera etkinleştirildi** →
**oyuncu kontrolü etkinleştirildi** → (her şey çalışırken) **8 saniye sonra "host hiç yanıt
vermedi" zaman aşımı tetiklendi** → bağlantı kapatıldı (her iki oyuncu despawn oldu).

**Kesin kök neden**: `MultiplayerConnectionUI.OnEnable()` içinde `NetworkManager.Singleton`'a
abone oluyordu (`OnClientConnectedCallback` vb.), ama kurulu Netcode paketinin kaynağı
doğrulandı: **`NetworkManager.Singleton` NetworkManager'ın KENDİ `Awake()`'inde değil,
`OnEnable()`'ında atanıyor** (`NetworkManager.cs:1093-1096`). Unity, bir sahnedeki TÜM
nesnelerin `Awake()`'inin TÜM `OnEnable()`'lardan önce çalışacağını garanti eder, ama
**farklı GameObject'lerin `OnEnable()`'ları arasında sıra garantisi vermez**. Bu sahnede
`MultiplayerConnectionUI`'ın Canvas'ı ile `NetworkManager`'ın GameObject'i ayrı nesneler —
Unity'nin hangisinin `OnEnable()`'ını önce çalıştıracağı tanımsız. Eğer Canvas'ınki önce
çalışırsa, `NetworkManager.Singleton` o anda hâlâ `null` olur, erken `return` abonelikleri
**hiç kaydetmez** ve bir daha asla denenmez (`OnEnable` sadece bir kez çalışır). Sonuç:
bağlantı **gerçekten** kuruluyordu (NGO'nun kendi iç spawn mekanizması bu aboneliklere
bağımlı değil) ama `MultiplayerConnectionUI` bundan **hiç haberdar olmuyordu** —
`_clientConnectAttemptPending` hep `true` kalıyordu, 8s'lik İzleyici (watchdog) bunu
"bağlantı hiç kurulmadı" sanıp **çalışan bağlantıyı `Shutdown()` ile kesiyordu**. Önceki
"satın alma sırasında donma" raporu da muhtemelen aynı hatanın bir tezahürüydü — oyuncu
makineyi tam ~8. saniye civarında satın almaya çalışırken bağlantının kendisi kesiliyordu.

**Düzeltme (tek dosya, `MultiplayerConnectionUI.cs`)**: Abonelik `OnEnable()`'dan
**`Start()`'a** taşındı. Unity'nin garantisi şu: sahnedeki TÜM nesnelerin `OnEnable()`'ı,
HERHANGİ bir nesnenin `Start()`'ından önce çalışır — yani `MultiplayerConnectionUI.Start()`
çalıştığında `NetworkManager.OnEnable()` (ve dolayısıyla `Singleton` ataması) **kesinlikle
tamamlanmış** olur, hangi nesnenin önce enable olduğuna bakılmaksızın. `_subscribed` bayrağı
çift-abonelik/çift-iptal riskini engelliyor. `OnDisable()` artık sadece gerçekten abone
olunduysa iptal ediyor.

**Kullanıcıya düşen adımlar**: Sadece script değişti — **yeni bir Windows build gerekiyor**.
Bu düzeltmeyle artık: bağlantı kurulduğunda durum "Client bağlı"ya geçmeli, 8 saniye sonra
**hiçbir şey kesilmemeli**, ve makine satın alma/yerleştirme artık bağlantı koptuğu için değil
gerçek sonucuna göre çalışmalı.

**Aşama 11B hâlâ "tamamlandı" işaretlenmedi.**

## Aşama 11B Genişletmesi — Fabrika Genişletme / Elektrik Yükseltme / Aktif Sipariş Senkronizasyonu

Bağlantı ve makine satın alma/yerleştirme çalıştıktan sonra kullanıcı üç eksik senkronizasyon
bildirdi: fabrika genişletmesi, elektrik kapasite yükseltmesi, aktif sipariş kabul — hepsi
client'ta yapıldığında host'a (veya tersine) yansımıyordu.

**Ön inceleme bulgusu**: Üçünün de kendi `TryPurchase`/`TryPurchaseUpgrade`/`TryAcceptOrder`
metodu zaten `MachinePurchaseManager.TryPurchase` ile **birebir aynı** atomik-tek-yetkili
desende yazılmıştı (hatta `FactoryExpansionManager`'ın kendi yorum satırı "this purchase
boundary is the seam a future host-authoritative RPC would wrap, same pattern as
MachinePurchaseManager.TryPurchase" diyordu) — sadece henüz `NetworkEconomyBridge`'e
bağlanmamışlardı. Üç sistemin UI'ları (`FactoryExpansionPanelUI.OnBuyClicked`,
`PowerPanelUI.OnUpgradeClicked`, `OrderBoardPanelUI.OnAcceptClicked`) hâlâ kendi
manager'larını **doğrudan** çağırıyordu — her client bunu sadece kendi yerel kopyasında
yapıyordu, host hiç haberdar olmuyordu.

**Yapılan değişiklik — `NetworkEconomyBridge.cs`'e üç yeni sistem eklendi** (aynı dört parçalı
desenin tekrarı: bir `NetworkVariable`/`NetworkList` tek yönlü ayna + senkron host yolu +
`ServerRpc` + hedefli `ClientRpc`, makine satın alma ile birebir aynı try/catch + 6s zaman
aşımı güvenlik ağı dahil):
- **Fabrika genişletmesi**: `NetworkList<bool>`, `SaveCatalog.AllExpansions` sırasına göre
  indeksli. İstemci tarafında yeni `true` olan bir girdi `FactoryExpansionManager.
  RestoreUnlocked`'a uygulanıyor — bu metot zaten `ExpansionUnlocked` olayını fırlatıyor,
  yani `FactoryExpansionGate` **hiç değiştirilmeden** kendi mevcut tepki yoluyla açılıyor.
- **Elektrik yükseltmesi**: Tek bir `NetworkVariable<bool>` (`IsUpgradePurchased` — şu an
  tek seviye var). `PowerManager.RestoreUpgradeState` ile uygulanıyor. **Not**: Sadece
  TOPLAM kapasite (`TotalCapacityKw`) senkronize — her makinenin gerçek anlık güç tüketimi/
  tahsisi (`UsedCapacityKw`) hâlâ yerel (üretim durumu senkronizasyonuyla birlikte Aşama 11C'de
  ele alınacak, kullanıcının "üretim durumu senkronize değil" şeklinde zaten kabul ettiği
  sınırla aynı).
- **Aktif sipariş**: `NetworkVariable<int>` — `OrderManager.AvailableOrders` sırasına göre
  indeks (-1 = yok). Host tarafında her `Update()`'te host'un gerçek aktif siparişiyle
  karşılaştırılıp senkronize ediliyor (bu sayede sipariş yerel olarak tamamlandığında da
  otomatik sıfırlanıyor). İstemci tarafında yeni bir indekse geçildiğinde `OrderManager.
  RestoreOrderState(sipariş, Active, 0, false)` uygulanıyor. **Bilinçli olarak YAPILMAYAN**:
  paketleme ilerlemesi/tamamlama/sevkiyat senkronize edilmiyor (kullanıcının kendi talimatı
  gereği) — sadece "hangi sipariş aktif" senkronize.

**Değiştirilen/eklenen dosyalar**: `NetworkEconomyBridge.cs` (genişletildi),
`FactoryExpansionPanelUI.cs`/`PowerPanelUI.cs`/`OrderBoardPanelUI.cs` (satın alma/kabul
butonları köprü varsa onu kullanıyor — tek oyunculu davranış değişmedi),
`ShiftworksMultiplayerEconomySetup.cs`'nin menü 32'si artık `FactoryExpansionManager`/
`PowerManager`/`OrderManager`'ı da bulup köprüye bağlıyor (zaten var olan bir köprüye bile
— yeni alanları "retrofit" ediyor, menüyü tekrar çalıştırmak güvenli).
**Hiç dokunulmayan**: `FactoryExpansionManager.TryPurchase`, `PowerManager.
TryPurchaseUpgrade`, `OrderManager.TryAcceptOrder/TryRegisterPackedItem/TryCompleteOrder`,
`FactoryExpansionGate`, `PackingStation`, `ShippingZone`.

**Kullanıcıya düşen adımlar**: `Shiftworks/Setup/32. Add Network Economy Bridge To
WorkshopMultiplayer Scene`'i **yeniden çalıştırın** (yeni alanları mevcut köprüye bağlamak
için) — ardından yeni bir Windows build alın.

**Aşama 11B hâlâ "tamamlandı" işaretlenmedi. Aşama 11C'ye geçilmedi.**

## Aşama 11B Genişletmesi Test Talimatları

1. Menü 32'yi yeniden çalıştırın, yeni Windows build alın.
2. Host genişletme satın alır → Client'ta Üretim Alanı B kapısının açıldığını doğrulayın.
3. Client genişletme satın alır → Host'ta kapının açıldığını doğrulayın.
4. Aynı genişletmeyi ikinci kez satın almaya çalışın (her iki taraftan) — buton zaten
   devre dışı olmalı / "zaten satın alındı" uyarısı çıkmalı, para düşmemeli.
5. Host elektrik kapasitesini yükseltir → Client panelinde yeni kapasitenin göründüğünü
   doğrulayın.
6. Client elektrik kapasitesini yükseltir → Host panelinde yeni kapasitenin göründüğünü
   doğrulayın.
7. Her işlemde paranın **sadece bir kez** düştüğünü (her iki ekranda da aynı bakiye)
   doğrulayın.
8. Host yeni sipariş kabul eder → Client'ta aynı siparişin "Devam Ediyor" göründüğünü
   doğrulayın.
9. Client yeni sipariş kabul eder → Host'ta aynı siparişin göründüğünü doğrulayın.
10. Bir sipariş aktifken diğer tarafın başka bir sipariş kabul edemediğini doğrulayın.
11. Client'ı bağlantıdan kesip yeniden bağlayın — genişletme/elektrik/aktif sipariş
    durumunun doğru (güncel) geldiğini doğrulayın.
12. **Regresyon**: makine satın alma/yerleştirme hâlâ çalışıyor mu, Console'da kırmızı
    hata var mı, tek-oyunculu `WorkshopPrototype.unity` hâlâ sorunsuz mu kontrol edin.

## Aşama 11B Genişletmesi — NullReferenceException Düzeltmesi (`NetworkEconomyBridge.IndexOfActiveOrder`)

Kullanıcı Console'da `IndexOfActiveOrder()`/`Update()`'te (satır 350/440) her karede tekrarlanan
(999+) bir `NullReferenceException` bildirdi, ve genişletme/elektrik/sipariş'in hâlâ hiç
senkronize olmadığını.

**Kesin kök neden**: Sahne dosyası doğrudan incelendi — `NetworkEconomyBridge`'in serileştirilmiş
verisinde **hâlâ sadece `economyManager`/`purchaseManager`/`catalog` vardı**,
`factoryExpansionManager`/`powerManager`/`orderManager` hiç yazılmamıştı. Bu, önceki rapordaki
"menü 32'yi yeniden çalıştırın" adımının **henüz yapılmadığını** kanıtlıyor — yani kod mantığında
"gerçek" bir hata değil, `orderManager` alanı gerçekten null olduğu için `orderManager.ActiveOrder`
her `Update()`'te patlıyordu (host her karede çalıştığı için 999+ tekrar).

**Uygulanan düzeltme (sadece gizleme değil — gerçek null kontrolü + daha iyi başlatma sırası)**:
- **`ResolveMissingReferences()`** (yeni, `OnNetworkSpawn`'da ilk iş olarak çalışıyor): eksik
  her referans için `MachineController.Awake()`'in `PowerManager` için zaten kullandığı AYNI
  `FindFirstObjectByType<T>()` yedek-arama desenini uyguluyor. Sahnede her manager türünden
  tam olarak bir tane olduğu için bu güvenli ve doğru — **artık menü 32'yi yeniden çalıştırmak
  bile gerekmiyor**, sadece yeni build yeterli (yine de menüyü çalıştırmanız önerilir, sahne
  verisini de güncel tutar).
- **`ValidateReferences()`** (yeni): yedek aramadan SONRA hâlâ eksik olan varsa, bunu **tam
  olarak hangi alan(lar)** eksik diyerek **bir kez** loglayıp köprüyü güvenli şekilde devre dışı
  bırakıyor — `Update()` ve 5 `RequestX` genel metodunun hepsi artık `_referencesValid` bayrağını
  kontrol ediyor, hiçbiri eksik referansla devam etmiyor. Böylece aynı hata bir kere loglanır,
  999 kere değil, ve **asla sessizce yutulmaz** (try/catch ile maskeleme yapılmadı — talimatınız
  gereği).
- 5 `OnValueChanged`/`OnListChanged` geri çağırması da aynı bayrağı kontrol ediyor (host'ta
  referanslar geçerliyken client'ta (teorik olarak) eksikse bile client tarafında NRE olmaz).

**Kullanıcıya düşen adımlar**: **Yeni bir Windows build gerekiyor** (kod değişti). Menü 32'yi
tekrar çalıştırmanıza artık gerek yok (çalışma-zamanı yedek-arama bunu kapsıyor), ama isterseniz
sahne verisini kalıcı olarak güncellemek için yine çalıştırabilirsiniz — zarar vermez (retrofit).

**Aşama 11B artık kullanıcı tarafından tam olarak onaylandı** (bağlantı, ortak para, makine
satın alma/yerleştirme, fabrika genişletme, elektrik yükseltme, ortak sipariş kabul — hepsi
çalışıyor). Bunun üzerine **Aşama 11C** başladı, kullanıcının isteğiyle **11C-1/11C-2/11C-3**
alt aşamalarına bölündü (her biri test onayı almadan sıradakine geçilmiyor). Bu oturumda
**yalnızca 11C-1** uygulandı.

## Aşama 11C-1 Mimarisi (Ortak Eşya Alma/Taşıma/Bırakma/Fırlatma)

**Ön inceleme bulgusu**: `CarryableItem.Interact/Drop/Throw` zaten `MachinePurchaseManager.
TryPurchase` ile aynı tek-atomik-yetkili desende yazılmıştı; `PlayerInteractor`'daki
`RequestInteraction`/`RequestDrop`/`RequestThrow` metotlarının kendi yorum satırı bile "bu,
gelecekteki multiplayer RPC çağrısının ekleneceği sınır" diyordu — tam da bu aşamada
kullanılan seam. `CuttingMachine`/`AssemblyMachine`/`ConveyorSegment`/`RawMaterialFeeder`/
`OutputStorage`/`PowerManager` hâlâ ağ üzerinden hiç paylaşılmıyor (11C-2/11C-3'ün kapsamı);
bu yüzden bu oturumda **sadece** sahnede zaten var olan (`TestBox_1..4`, sahne-gömülü,
host başlarken otomatik ağa açılan) eşyalar + dört üretim eşyası prefabı
(`MetalSheet`/`CutMetalPart`/`AssembledComponent`/`ShipmentPackage`) hazırlandı — ama bu dört
prefab'tan makine/besleyici tarafından ÜRETILEN örnekler, üretim zinciri kendisi henüz ağa
açılmadığı (hâlâ düz `Instantiate`, `NetworkObject.Spawn()` çağrılmıyor) için **henüz** iki
oyuncu arasında görünmüyor — sadece sahnede zaten duran eşyalar (TestBox'lar) bu aşamada
tam olarak test edilebilir. Prefab'lar bilinçli olarak şimdiden hazırlandı ki 11C-2/11C-3
üretim zincirini ağa açarken sadece `.Spawn()` çağırması yeterli olsun.

**`CarryableItem.cs`'e dokunulmadı** (tek-oyunculu fizik/etkileşim mantığı birebir aynı) —
sadece iki küçük, katkısal ekleme: `_networkAvailable` bayrağı (varsayılan `true`, tek
oyunculuda hiç değişmiyor) ve bunu `CanInteract`'e ekleyen tek bir ek koşul + `SetNetworkAvailability`
setter'ı. Bunun nedeni: ağ üzerinde `Interact()` artık yalnızca HOST'ta çalışıyor, yani bir
client'ın kendi yerel `CurrentHolder`'ı hep `null` kalırdı — bu bayrak olmadan, başka biri
zaten tutarken bile "E - Al" uyarısı yanlışlıkla görünmeye devam ederdi.

**Yeni: `NetworkCarryableItem`** — her eşyanın yanında, `IsHeld || IsAutomated`'i tek yönlü
(host → herkes) bir `NetworkVariable<bool>` ile senkronize eder, `CarryableItem.
SetNetworkAvailability`'ye uygular.

**Yeni: `NetworkItemInteractionBridge`** — sahnede tek bir singleton (`NetworkEconomyBridge`
ile aynı rol). Alma (`RequestPickup`) başarısız olabileceği için (biri sizden önce almış
olabilir) host onayı beklenip `_carriedItem` ancak onay gelince set ediliyor — tam da makine
satın alma ile aynı desen. Bırakma/fırlatma (`RequestDrop`/`RequestThrow`) **"ateşle ve
unut"**: `CarryableItem.Drop/Throw` zaten tutuluyorken asla gerçek anlamda başarısız olamaz,
bu yüzden istek host'a gider ama client kendi `_carriedItem`'ını hemen (tek oyunculudaki gibi
anında tepki veren) temizler.

**Uzak client'ın "kimliği" host'ta nasıl çözülüyor**: Host'ta çalışan `IInteractor`
gerektiren `CarryableItem.Interact(IInteractor)` metodu **hiç değiştirilmedi** — host, isteği
gönderen client'ın `NetworkManager.SpawnManager.GetPlayerNetworkObject(senderClientId)` ile
o client'ın kendi `NetworkPlayer`'ını (host'un zaten sahip olduğu replike temsilini) bulup
üzerindeki `PlayerInteractor`'ı tam olarak aynı `Interact(IInteractor)` çağrısına veriyor —
sıfır değişiklikle.

**Fizik senkronizasyonu**: Her eşyaya `NetworkObject` + `NetworkTransform` (**Server**
yetkili modu — host'un fiziği her zaman tek otorite) + `NetworkRigidbody` (host dışındaki
her kopyanın Rigidbody'sini otomatik olarak kinematic yapar, bu da "client'ta bağımsız fizik
simülasyonları çakışmasın" gereksinimini NGO'nun kendi, resmi, test edilmiş bileşeniyle
çözüyor — özel kod yazılmadı) eklendi.

## Aşama 11C-1'de Oluşturulan/Değiştirilen Dosyalar

- Yeni: `Assets/_Project/Scripts/Multiplayer/NetworkCarryableItem.cs`,
  `Assets/_Project/Scripts/Multiplayer/NetworkItemInteractionBridge.cs`.
- Yeni: `Assets/_Project/Scripts/Editor/ShiftworksMultiplayerItemSetup.cs` (menü **34**:
  4 eşya prefabına ağ bileşenleri ekler + mevcut `MultiplayerNetworkPrefabs.asset`'e kaydeder
  (Aşama 11B'nin zaten oluşturduğu AYNI liste, NetworkManager'a yeniden bağlama gerekmiyor);
  menü **35**: sahnedeki 4 `TestBox`'a aynı bileşenleri ekler + `NetworkItemInteractionBridge`'i
  sahneye ekler; + "Apply Multiplayer Item Interaction" birleşik menüsü).
- Değiştirilen (küçük, katkısal): `CarryableItem.cs` (`_networkAvailable` + `SetNetworkAvailability`),
  `PlayerInteractor.cs` (`HandleItemPickup` yeni metot + `RequestDrop`/`RequestThrow` köprü
  varsa onu kullanıyor).
- **Hiç dokunulmayan**: `CarryableItem`'ın kendi fizik/etkileşim mantığı, `MachineController`,
  `ConveyorSegment`, `RawMaterialFeeder`, `OutputStorage`, `PowerManager`, `WorkshopPrototype.unity`,
  `MultiplayerPrototype.unity`.

## Aşama 11C-1 — Desteklenmeyen / Bilinçli Sınırlar (açıkça belirtiliyor)

- Makine/besleyici tarafından üretilen eşyalar (MetalSheet/CutMetalPart/AssembledComponent)
  **henüz iki oyuncu arasında görünmüyor** — üretim zinciri kendisi ağa açılmadı (11C-2/11C-3).
  Bu eşyaların prefab'ları hazır (ağ bileşenleri eklendi) ama hiçbir şey onları
  `NetworkObject.Spawn()` ile açmıyor henüz.
- Kesim/montaj makinesi üretim durumu hâlâ senkron değil (11C-2'nin kapsamı).
- Konveyör/depo/elektrik-duraklatma/sonradan-bağlanan-client senkronu hâlâ yok (11C-3'ün
  kapsamı).
- Alma isteği host onayı bekler (yetersiz gecikme hissedilmeyecek kadar küçük olmalı,
  localhost testinde), bırakma/fırlatma anında yerel olarak uygulanır (ateşle-ve-unut) —
  host hiçbir zaman reddetmeyeceği için bu seçildi.

## Aşama 11C-1 Test Talimatları (yalnızca sahnedeki TestBox'larla — üretilen eşyalar henüz kapsam dışı)

1. Sırayla **`34. Add NetworkObject To Item Prefabs`** ve **`35. Add Item Interaction To
   WorkshopMultiplayer Scene`** çalıştırın (veya "Apply Multiplayer Item Interaction").
   Console'da hata olmadığını kontrol edin.
2. Yeni bir Windows build alın.
3. Host Başlat / Client Bağlan.
4. **Host bir TestBox'ı alır** → Client'ın ekranında kutunun Host'un elinde (kamerasının
   önünde) göründüğünü doğrulayın.
5. **Client bir TestBox'ı alır** → Host'un ekranında kutunun Client'ın elinde göründüğünü
   doğrulayın.
6. Biri bir kutuyu tutarken diğer tarafın O KUTUYU alamadığını ("E - Al" görünmemeli veya
   işe yaramamalı) doğrulayın.
7. Her iki taraf da kendi kutusunu **bırakıp (G)** ve **fırlatıp (sol tık)** diğer tarafta
   doğru konumda/fizikle göründüğünü doğrulayın.
8. **Regresyon**: `WorkshopPrototype.unity`'de tek-oyunculu eşya alma/taşıma/bırakma/fırlatma
   hâlâ sorunsuz mu kontrol edin. Makine satın alma/genişletme/elektrik/sipariş hâlâ çalışıyor
   mu kontrol edin.
9. Console'da kırmızı hata olmadığını doğrulayın.

**Aşama 11C-1 kullanıcı tarafından test edilip onaylanmadan 11C-2'ye geçilmeyecek.**

## Aşama 11C — Depo Stok Senkronizasyonu Hatası Düzeltmesi

11C-1 testi sırasında kullanıcı şunu buldu: `OutputStorage`'dan bir parça alındığında
**sadece alan oyuncunun ekranındaki stok sayısı düşüyordu**, diğer oyuncu hâlâ eski sayıyı
görüyordu.

**Kesin neden**: `OutputStorage.StoredCount` hiç ağ üzerinden paylaşılmıyordu —
`PlayerInteractor.HandleStorageInteraction`, `OutputStorage.TryWithdraw`'ı **doğrudan**
çağırıyordu, bu da her client'ın kendi yerel `StoredCount`'unu azaltıyordu (host'unkine hiç
dokunmadan). Ayrıca `TryWithdraw`'ın ürettiği eşya düz bir `Instantiate()` idi,
`NetworkObject.Spawn()` edilmiyordu — yani alınan eşya bile diğer oyuncuya hiç görünmüyordu.

**Düzeltme (aynı kurulu desen — yeni bir sistem icat edilmedi)**:
- Yeni `NetworkOutputStorage` — `NetworkEconomyBridge`'in bakiye için yaptığının aynısını
  `StoredCount` için yapar: tek yönlü (host → herkes) `NetworkVariable<int>`, mevcut Aşama
  10 `RestoreStoredCount` giriş noktasına uygulanır (UI/prompt kodu hiç değişmedi).
- `NetworkItemInteractionBridge.RequestWithdraw` (yeni) — `OutputStorage.TryWithdraw`'ı
  **sadece host çağırabilir**: host'ta senkron (tek oyunculuyla birebir), uzak client'ta
  `ServerRpc`→host çalıştırır→ürettiği eşyayı `NetworkObject.Spawn()` eder→sonucu (ve eşyaya
  bir `NetworkObjectReference`) hedefli `ClientRpc` ile geri bildirir. `PlayerInteractor.
  _carriedItem` ancak host onayından sonra set edilir (makine satın alma ile birebir aynı
  güvenlik deseni — host onaylamazsa hiçbir şey olmaz, çoğalma/kaybolma riski yok).
- Host zaten tek `TryWithdraw` çağrısı içinde stoğu düşürüp eşyayı üretiyor — bu **tek atomik
  metot** hiç değişmedi, bu yüzden "aynı anda iki istekte stok negatife düşmesin" ve "stok 0
  olunca kimse alamaz" zaten garantiydi (host'ta HER istek sırayla işlenir, race yok).

## Aşama 11C'de Oluşturulan/Değiştirilen Dosyalar (bu düzeltme)

- Yeni: `Assets/_Project/Scripts/Multiplayer/NetworkOutputStorage.cs`.
- Değiştirilen: `NetworkItemInteractionBridge.cs` (`RequestWithdraw` + ilgili RPC'ler eklendi),
  `PlayerInteractor.cs` (`HandleStorageInteraction` köprü varsa onu kullanıyor),
  `ShiftworksMultiplayerItemSetup.cs` (yeni menü **36**).
- **Hiç dokunulmayan**: `OutputStorage.TryAccept/TryWithdraw`'ın kendi mantığı,
  `ConveyorSegment`, `RawMaterialFeeder`, ortak ekonomi/makine/genişletme/elektrik/sipariş
  sistemleri, tek oyunculu `WorkshopPrototype.unity`.

## Kullanıcıya Düşen Adımlar (bu düzeltme)
1. **`Shiftworks/Setup/36. Add Output Storage Sync To WorkshopMultiplayer Scene`** çalıştırın
   (menü 34 ve 35'in zaten çalıştırılmış olması gerekir — depo `CutMetalPart` üretiyor,
   o prefab'ın ağ bileşenleri menü 34'ten geliyor).
2. Yeni bir Windows build alın.
3. Host/Client: depodan sırayla parça alın — ikisinde de aynı stok sayısını, alınan parçanın
   diğer tarafta da göründüğünü doğrulayın. Stok 0'a inince kimsenin alamadığını doğrulayın.

**Aşama 11C hâlâ tamamlanmadı olarak işaretlendi.**

## Aşama 11C — Client Makine Satın Alınca Haritadan Düşme Hatası Düzeltmesi

**Bulgu**: `NetworkEconomyBridge.ExecutePlacementOnServer`, host'un client'tan gelen
yerleştirme `position`'ını **hiç doğrulamadan** doğrudan `Instantiate` + `NetworkObject.Spawn`
ediyordu. `MachinePlacementController`'ın KENDİ ghost-önizleme çakışma kontrolü sadece
**client'ın kendi yerel** fizik dünyasına göre çalışıyor — host'taki gerçek oyuncu
konumu, owner-authoritative `NetworkTransform` nedeniyle client'ın bildiğinden en az bir ağ
tick'i kadar farklı olabilir. Bu yüzden client'a göre "boş" görünen bir nokta, host'un kendi
simülasyonunda bir oyuncunun (genelde isteği gönderenin kendisinin) üzerine denk gelebiliyordu.
Solid (trigger olmayan) bir collider bir `CharacterController`'ın İÇİNE aniden belirince,
PhysX onu anında "depenetrate" ediyor — zemin kalınlığına/geometriye bağlı olarak bu itme
karakteri beklenmedik bir yöne, hatta haritanın altına fırlatabiliyor. Bu tam olarak
bildirilen "client makine satın alınca haritanın altına düşüyor" belirtisiyle örtüşüyor.

**Düzeltme (`NetworkEconomyBridge.cs`)**: `ExecutePlacementOnServer`'a, `MachinePlacementController`'ın
kendi ghost çakışma kontrolüyle **birebir aynı** (aynı 3cm "skin" payı, aynı sadece-"Ignore
Raycast"-hariç mantığı) host-taraflı bir doğrulama eklendi — host, spawn etmeden önce KENDİ
fizik dünyasında o noktada bir şey olup olmadığını kontrol ediyor; varsa yerleştirmeyi
reddediyor ve **bekleyen makine sayısını geri ekliyor** (parayı kaybetmeden). Ayrıca talep
edilen teşhis logu eklendi: her yerleştirme isteğinde host'un o client için bildiği oyuncu
konumu, hedef yerleştirme konumuyla birlikte Console'a yazılıyor.

**Bilinen küçük kozmetik sınır**: Host çakışma yüzünden reddederse, client'a giden hata mesajı
hâlâ genel "Bekleyen makine yok" metnini kullanıyor (gerçek sebep "çakışma" olsa da) — işlevsel
sonuç doğru (yerleştirme iptal, para kaybı yok, ghost açık kalır, tekrar denenebilir), sadece
mesaj metni şimdilik daha az spesifik.

## Değiştirilen Dosya (bu düzeltme)
- `Assets/_Project/Scripts/Multiplayer/NetworkEconomyBridge.cs` — tek dosya.

## Kullanıcıya Düşen Adımlar (bu düzeltme)
Sadece script değişti — **yeni bir Windows build gerekiyor**, Editor menüsü çalıştırmaya
gerek yok.

## Aşama 11C — Montaj Makinesinde Ortak Girdi Hatası Düzeltmesi

**Kesin neden**: `MachineController` (hem `CuttingMachine` hem `AssemblyMachine`) hiçbir
zaman ağ üzerinden paylaşılmadı — her client'ın kendi sahne/satın-alınan kopyası tamamen
**bağımsız** simülasyon çalıştırıyordu. Host bir `CutMetalPart` koyduğunda bu sadece HOST'un
kendi yerel `_bufferedInputCount`'unu 1 yapıyordu; client'ın kendi ayrı kopyası hâlâ 0'daydı.
Client ikinci parçayı koymaya çalıştığında, KENDİ yerel makinesi bunu "birinci" parça olarak
kabul ediyordu (recipe 2 girdi istiyor, hâlâ Idle) — üretim BAŞLAMIYORDU, çünkü gerçekte
hiçbir taraf "2/2"ye ulaşmadı; sadece iki taraf da kendi "1/2"sinde kilitli kaldı. Senaryoda
"ilk parçayı koyan oyuncu ikinciyi de koyarsa çalışıyor" ifadesi tam bunu doğruluyor — aynı
oyuncunun İKİ ayrı E-basışı KENDİ yerel makinesinde gerçekten 1→2 ilerliyor.

**Düzeltme (aynı kurulu dört parçalı desen — makine satın alma/depo ile birebir aynı mimari)**:
- Yeni `NetworkMachineController` — `MachineController`'ın `State`/`BufferedInputCount`/
  işlenme süresini (host → herkes, tek yönlü) senkronize eder; yeni, **dar kapsamlı**
  `MachineController.RestoreDisplayState` giriş noktasına uygular (Aşama 10'un
  `RestoreProductionState`'i KASITLI OLARAK kullanılmadı — o metot çıktıyı da yeniden
  yaratır, bu ise her client'ın kendi kopyasında ayrı bir çıktı oluşturup çoğaltmasına yol
  açardı). Host'un kendi makinesi `OutputReady`'ye geçtiği anda, zaten üretilmiş çıktı
  (`MachineController.PendingOutput`) tam o anda `NetworkObject.Spawn()` edilir — böylece
  çıktının kendisi de (depo/makine satın alma ile birebir aynı mantıkla) her iki oyuncuya
  aynı anda görünür.
- `NetworkItemInteractionBridge`'e `RequestFeedMachine`/`RequestCollectOutput` eklendi —
  `TryAcceptInput`/`TryCollectOutput`'ı artık **sadece host çağırabilir**. Client isteği
  host'a gönderir; **host, isteği gönderen oyuncunun GERÇEKTEN o ürünü taşıdığını kendi
  gördüğü `CurrentHolder` ile doğrular** (sizin açıkça istediğiniz sahtecilik kontrolü) —
  eşleşmezse istek reddedilir, `TryAcceptInput` hiç çağrılmaz.
- İstenen tüm teşhis logları eklendi: hangi oyuncu istek gönderdi, ne taşıyordu, makinenin
  o anki girdi sayısı/durumu, kabul mü reddedildi mi, reddedilme sebebi.
- Elektrik duraklaması **ekstra kod gerektirmedi**: `HasPower` bilinçli olarak senkronize
  edilmiyor (client'ta hep `false` kalıyor) — bu da client'ın kendi `Update()`'inin üretim
  ilerlemesini KENDİ BAŞINA asla artırmamasını sağlıyor (host'un senkronize ettiği değer
  dışında). Host'un gücü kesilirse host'un kendi ilerlemesi zaten donar, senkronize değer de
  otomatik olarak donmuş görünür — ayrıca ele alınmasına gerek kalmadı.
- İki oyuncu aynı anda girdi koymaya çalışırsa: host istekleri HER ZAMAN sırayla işler (RPC
  çağrıları host'un ana thread'inde kuyruğa alınır) — `TryAcceptInput`'ın kendi atomik
  kontrolü (sadece `Idle` durumda kabul eder) zaten "iki kez tüketilmesin" garantisini
  sağlıyor, hiçbir ek kilitleme mekanizması gerekmedi.

## Değiştirilen/Oluşturulan Dosyalar (bu düzeltme)
- Yeni: `Assets/_Project/Scripts/Multiplayer/NetworkMachineController.cs`.
- Değiştirilen (küçük, katkısal): `MachineController.cs` (`BufferedInputCount`/
  `ProcessingElapsedSeconds`/`PendingOutput` salt-okunur erişimciler + `RestoreDisplayState`),
  `NetworkItemInteractionBridge.cs` (`RequestFeedMachine`/`RequestCollectOutput` + ilgili
  RPC'ler ve loglar), `PlayerInteractor.cs` (`HandleMachineInteraction` köprü varsa onu
  kullanıyor), `ShiftworksMultiplayerEconomySetup.cs` (yeni menü **37**).
- **Hiç dokunulmayan**: `MachineController.TryAcceptInput/TryCollectOutput/CompleteProcessing`'in
  kendi mantığı, `PowerManager`, ortak ekonomi/genişletme/elektrik/sipariş sistemleri, eşya
  taşıma sistemi, tek oyunculu sahne.

## Kullanıcıya Düşen Adımlar (bu düzeltme)
1. **`Shiftworks/Setup/37. Add NetworkMachineController To Machine Prefabs`** çalıştırın
   (menü 32'nin zaten çalıştırılmış olması gerekir).
2. Yeni bir Windows build alın.
3. Senaryo A ve B'yi (Host önce/Client önce) test edin — 8 saniye sonra sadece 1
   `AssembledComponent` oluştuğunu, her iki ekranda da girdi sayısı/durumun aynı göründüğünü
   doğrulayın.
4. Regresyon: metal kesim makinesi, ortak ekonomi/genişletme/elektrik/sipariş/depo, eşya
   taşıma, tek oyunculu sahne hâlâ sorunsuz mu kontrol edin.

**Not**: Scene-gömülü ORİJİNAL (Stage 3) `CuttingMachine` bu düzeltmenin kapsamı dışında
(hiçbir zaman `NetworkObject` almadı, satın alınan bir prefab değil) — tek-input reçetesi bu
2-oyunculu girdi yarışını zaten yaşamıyordu, bu yüzden bilinçli olarak dokunulmadı.

## Aşama 11C — CarryableItem Referans Hatası Düzeltmesi (Bağlantı Kesilen Oyuncunun Elindeki Ürün)

**Kesin neden**: `CarryableItem.Interact/Drop` artık (11C-1'den beri) **sadece host'ta**
çalışıyor. Hiçbir yerde, bir oyuncunun bağlantısı kesildiğinde/despawn olduğunda **elinde
tuttuğu eşyanın bırakılması** yoktu. Host'un `FixedUpdate()`'i, artık var olmayan bir
oyuncunun `HoldPoint`'ine erişmeye çalışmaya **sonsuza kadar** devam ediyordu — her fizik
adımında `MissingReferenceException`. Eşya da "tutuluyor" durumunda sonsuza kadar sıkışıp
kalıyordu (`IsHeld` asla `false` olmuyordu), bu yüzden kimse onu göremiyor/kullanamıyordu.

**Neden `IsHeld` bunu kendi başına yakalayamıyordu**: `CurrentHolder`, `IInteractor`
**arayüz** tipinde tutuluyor — `CurrentHolder != null` kontrolü bu yüzden **düz C# referans
eşitliği**, Unity'nin "alttaki GameObject yok edildi mi" özel kontrolü DEĞİL. Yok edilmiş bir
oyuncuya işaret eden `CurrentHolder` referansı, C# açısından hâlâ "null değil" görünüyordu.

**Düzeltme (gerçek yaşam döngüsü onarımı, sadece null kontrolüyle gizleme değil)**:
- `NetworkPlayerSetup.OnNetworkDespawn` (zaten var olan metot, genişletildi) — bir oyuncu
  despawn olduğunda (bağlantı kesme dahil), **host tarafında** sahnedeki TÜM `CarryableItem`'lar
  taranıp `CurrentHolder`'ı o oyuncuya eşit olan varsa `Drop()` çağrılıyor — eşya güvenli
  şekilde dünyaya bırakılıyor (yer çekimi/layer/collision-ignore doğru geri yükleniyor, çünkü
  bunlar `Drop()`'un KENDİ zaten var olan, değiştirilmemiş mantığı).
- `CarryableItem.FixedUpdate`'e **ikincil bir güvenlik ağı** eklendi: `HoldPoint` gerçek bir
  `Transform` referansı olduğu için `CurrentHolder.HoldPoint == null` kontrolü Unity'nin
  GÜVENLİ "yok edildi mi" kontrolünü kullanıyor — bu YANLIŞLIKLA kaçırılan BAŞKA bir yaşam
  döngüsü boşluğu olursa bile eşya kendini otomatik bırakıyor, sonsuz hata döngüsüne
  girmiyor. (Bu, birincil düzeltme DEĞİL — asıl düzeltme yukarıdaki despawn temizliği.)
- `NetworkCarryableItem` zaten her karede `IsHeld`'i (dolayısıyla `Drop()` sonrası `false`
  olan hâlini) host'tan senkronize ediyordu — bu düzeltmeyle tutarlılığı otomatik olarak
  korunuyor, ek kod gerekmedi.

## Değiştirilen Dosyalar (bu düzeltme)
- `Assets/_Project/Scripts/Multiplayer/NetworkPlayerSetup.cs` — `OnNetworkDespawn` genişletildi.
- `Assets/_Project/Scripts/Items/CarryableItem.cs` — `FixedUpdate`'e ikincil güvenlik kontrolü.
- **Hiç dokunulmayan**: `MachineController`/`NetworkMachineController`/`NetworkItemInteractionBridge`'in
  üretim/alma mantığı (bunlar zaten doğru çalışıyordu — AssembledComponent prefab'ı Aşama
  11C-1'de `NetworkObject` almıştı, montaj makinesi çıktıyı tamamlandığı anda ağa açıyordu);
  ortak ekonomi/makine satın alma/depo sistemleri; tek oyunculu sahne.

## Kullanıcıya Düşen Adımlar (bu düzeltme)
Sadece script değişti, hiçbir Editor menüsü çalıştırmaya gerek yok — **yeni bir Windows build
alın** ve montaj makinesinden çıktı alma testini (özellikle bir bağlantı kesme/yeniden bağlanma
döngüsünden sonra) tekrarlayın.

## Aşama 11C — Montaj Girdisi Host–Client Uyuşmazlığı Düzeltmesi (Sahnedeki CuttingMachine)

**Kesin neden (kod + sahne YAML'ı okunarak doğrulandı)**: `WorkshopMultiplayer.unity`'deki
TEK pre-placed makine olan `CuttingMachine` (sahnede doğrudan oluşturulmuş, hiçbir zaman
canlı bir prefab instance'ı olmamış — sahnenin kendi YAML'ında bu GameObject için
`m_PrefabInstance: {fileID: 0}` doğrulandı) Aşama 11B/37'de `CuttingMachine.prefab`/
`AssemblyMachine.prefab` **prefab asset'lerine** eklenen `NetworkObject`/
`NetworkMachineController`'ı hiçbir zaman almadı — bir prefab asset değişikliği yalnızca HÂLÂ
BAĞLI prefab instance'larına yayılır, bu obje hiç öyle biri olmadı. Sonuç: bu makinenin
ürettiği HER `CutMetalPart`, hiçbir zaman `NetworkObject.Spawn()` edilmedi (bunu tetikleyecek
`NetworkMachineController` zaten yoktu).

Bu, `NetworkItemInteractionBridge.RequestPickup` VE `RequestFeedMachine`'in ikisini de
"henüz ağa açılmamış öğe" olarak değerlendirip **sessizce tek-oyunculu yerel çağrıya
("fallback") düşürmesine** yol açtı — hedef makine (satın alınmış, tamamen ağ senkronlu bir
`AssemblyMachine`) kusursuz olsa bile, taşınan `CutMetalPart` ağa açılmamış olduğu için istek
hiçbir zaman host'a ulaşmadı; her iki çağrı da E'ye basan oyuncunun KENDİ makinesinde yerel
olarak çalıştı. Client E'ye basınca kendi yerel `MachineController.BufferedInputCount`'u
1/2'ye çıktı (host hiç bilgilendirilmedi, o yüzden 0/2'de kaldı) — tam olarak bildirilen
"Client 1/2, Host 0/2" belirtisi. `NetworkMachineController`'ın kendi `Update()`'i host'un
gerçek değeri (0) hiç değişmediği için `OnValueChanged` da hiç tetiklenmedi, bu yüzden
Client'ın yanlış yerel değeri bir daha asla düzeltilmedi.

Önceki raporda "sahnedeki CuttingMachine multiplayer kapsamı dışında" denmişti; bu rapor o
sınırın montaj girdisini de nasıl bozduğunu gösteriyor — ağa açılmamış bir öğe, kendisinden
SONRAKİ host-authoritative adımı da (tamamen ağ senkronlu bir makineye besleme dahi) sessizce
devre dışı bırakıyor.

**Düzeltme**: `ShiftworksMultiplayerEconomySetup.cs`'e yeni, idempotent menü 38 eklendi —
`AddOutputStorageSync`'in (OutputStorage için) kullandığı AYNI yöntemle, sahnedeki
`CuttingMachine` GameObject'ine doğrudan (prefab'a değil) `NetworkObject` +
`NetworkMachineController` ekliyor, `machine` alanını sahnedeki `MachineController`'a
bağlıyor. `MachineController`/`NetworkItemInteractionBridge`'in kendisi hiç değişmedi —
sadece sahnedeki bu TEK eksik bağlantı tamamlandı.

## Değiştirilen/Oluşturulan Dosyalar (bu düzeltme)
- `Assets/_Project/Scripts/Editor/ShiftworksMultiplayerEconomySetup.cs` — yeni menü 38
  (`AddNetworkMachineControllerToSceneCuttingMachine`), `Apply Multiplayer Economy`'e eklendi.
- **Hiç dokunulmayan**: `MachineController`, `NetworkMachineController`,
  `NetworkItemInteractionBridge`, `AssemblyMachine`/`CuttingMachine` prefab asset'leri, ortak
  ekonomi/depo/genişletme/elektrik/sipariş sistemleri, tek oyunculu sahne.

## Kullanıcıya Düşen Adımlar (bu düzeltme)
1. Unity Editor'de **`Shiftworks/Setup/38. Add NetworkMachineController To Scene
   CuttingMachine`** menüsünü çalıştırın (tek seferlik, idempotent; `Apply Multiplayer
   Economy` ile de otomatik çalışır).
2. Yeni bir Windows build alın.
3. Testi tekrarlayın: Client önce CuttingMachine'den bir `CutMetalPart` üretip alsın, sonra
   bunu Assembly makinesine beslesin — Host ve Client ekranlarında girdi sayısının artık
   AYNI (ve doğru) göründüğünü doğrulayın. Ayrıca CuttingMachine'in kendi çıktısının artık
   her iki ekranda da görünüp toplanabildiğini kontrol edin.

## Aşama 11C — Montaj Girdisi Hatası HÂLÂ ÇÖZÜLMEDİ: Teşhis Turu (düzeltme değil)

Kullanıcı, önceki düzeltmeye (sahnedeki `CuttingMachine`'e `NetworkMachineController` ekleyen
menü 38) rağmen AYNI "Client 1/2, Host 0/2" belirtisinin devam ettiğini bildirdi. Kesinlikle
"düzeltildi" denmeden, önce gerçek akış ve ağırlıklı ihtimaller statik olarak (kod + prefab +
sahne YAML'ı okunarak) incelendi:

**Statik incelemeyle DOĞRULANAN, sorun OLMADIĞI kesinleşen noktalar:**
1. `AssemblyMachine.prefab` içinde `NetworkObject` + `NetworkMachineController` mevcut,
   `NetworkMachineController.machine` alanı doğru `MachineController`'a bağlı (prefab dosyası
   doğrudan okunarak doğrulandı).
2. `AssemblyMachineDefinition.asset.machinePrefab` gerçekten bu AYNI prefab asset'ine işaret
   ediyor (guid eşleşmesi doğrulandı) — stale/farklı bir prefab kopyası kullanılmıyor.
3. `MultiplayerNetworkPrefabs.asset` (bizim elle doldurduğumuz liste) şu an **boş**
   (`List: []`) — ama bu ZARARSIZ: Unity Netcode'un kendi otomatik `Assets/
   DefaultNetworkPrefabs.asset`'i (`IsDefault: 1`) CuttingMachine/AssemblyMachine ve dört eşya
   prefabının hepsini ZATEN otomatik olarak içeriyor (doğrudan okunarak doğrulandı) — yani
   prefab kayıt eksikliği bu hatanın nedeni DEĞİL.
4. `NetworkEconomyBridge.ExecutePlacementOnServer`'ın satın alınan makineyi `Instantiate` edip
   `NetworkObject.Spawn(true)` ile ağa açtığı kod akışı doğru görünüyor.
5. Gerçek eşya akışı da çıkarıldı: `CuttingMachine`'in `outputReceiverBehaviour` alanı dolu
   (bir konveyöre bağlı) ve sahnedeki `OutputStorage.acceptedItem` = `CutMetalPart` — yani
   oyuncu CutMetalPart'ı CuttingMachine'den DEĞİL, muhtemelen depodan (`RequestWithdraw`)
   alıyor. `RequestWithdraw`/`OutputStorage.TryWithdraw` kodu da host-authoritative görünüyor
   (host'ta tazeInstantiate + Spawn + CurrentHolder ataması).

**Henüz KANITLANAMAYAN nokta (bu yüzden "düzeltildi" denmiyor)**: Kod akışı kağıt üzerinde
doğru göründüğü için, `RequestFeedMachine`'in Client'ta neden hâlâ "YEREL (FALLBACK)" yoluna
düştüğünü (ya da düşüp düşmediğini) kesin olarak belirlemek için ÇALIŞMA ZAMANI kanıtı
gerekiyor — statik kod okuması bunu tek başına kanıtlayamaz.

### Çıkarılan Akış (dosya + metot adlarıyla)
`PlayerInteractor.HandleInteractKey` → `HandleMachineInteraction` → `HandleMachineFeed` →
`NetworkItemInteractionBridge.RequestFeedMachine` → (makine+ürün ağa açılmışsa) →
`RequestFeedServerRpc` [ServerRpc] → `ExecuteFeedOnServer` → `MachineController.TryAcceptInput`
→ (host) `NetworkMachineController.Update()`/`PushCurrentStateToClients` →
`_syncedBufferedInputCount` `NetworkVariable` değişir → (client) `HandleBufferedInputChanged`
→ `MachineController.RestoreDisplayState` → ekrana yansır. Ayrıca host, aynı istek için
`NotifyFeedResultClientRpc` ile ayrı bir onay/red yanıtı da gönderir (RPC yanıtı ile
NetworkVariable senkronu birbirinden bağımsız iki ayrı kanal).

### Eklenen Teşhis Logları (düzeltme değil — sadece gözlem)
- `NetworkItemInteractionBridge.OnNetworkSpawn`: `[BUILD]` etiketiyle build/protokol
  tanımlayıcısı (`DiagnosticBuildTag`), IsServer/IsClient/LocalClientId. **Bu satır Client'ın
  Player.log'unda hiç görünmüyorsa Client eski bir build çalıştırıyor.**
- `RequestFeedMachine`: her çağrıda `[FEED#N]` etiketiyle LocalClientId, IsServer/IsClient,
  makine adı/NetworkObjectId/IsSpawned/pozisyon/State/BufferedInputCount, ürün adı/
  NetworkObjectId/IsSpawned — ve YEREL (FALLBACK) yoluna düşülürse bunu **açık bir uyarıyla**
  işaretliyor.
- `RequestFeedServerRpc`: `SERVER_REQUEST_RECEIVED`, host'un çözümlediği makine/ürün kimliği.
- `ExecuteFeedOnServer`: `SERVER_REQUEST_ACCEPTED`/`SERVER_REQUEST_REJECTED` (gerçek red
  nedeniyle: null referans / taşıyan eşleşmiyor / `TryAcceptInput` false), kabul edilirse
  `SERVER_INPUT_COUNT_UPDATED` (eski → yeni değer).
- `NotifyFeedResultClientRpc`: `CLIENT_STATE_RECEIVED` (RPC yanıtı kanalı).
- `NetworkMachineController.HandleBufferedInputChanged`: `CLIENT_STATE_RECEIVED`
  (NetworkVariable kanalı — RPC'den tamamen ayrı, pasif senkron yolu).

Her log aynı `reqId` (`[FEED#N]`) ile işaretli, böylece Host Console'daki ve Client
Player.log'undaki satırlar aynı istek için eşleştirilebilir.

## Değiştirilen Dosyalar (bu teşhis turu)
- `Assets/_Project/Scripts/Multiplayer/NetworkItemInteractionBridge.cs` — yukarıdaki loglar.
- `Assets/_Project/Scripts/Multiplayer/NetworkMachineController.cs` — `HandleBufferedInputChanged`'e log.
- **Davranış değişikliği YOK** — hiçbir kontrol/koşul/akış değiştirilmedi, sadece gözlem eklendi.

## Kullanıcıdan İstenen (bu teşhis turu)
1. Yeni bir Windows build alın (Editor menüsü çalıştırmaya gerek yok, sadece script değişti).
2. Aynı testi tekrarlayın: Client depodan bir CutMetalPart alıp Assembly makinesine beslesin.
3. **Host Console'un TAMAMINI** ve **Client'ın `Player.log` dosyasını** (özellikle
   `[Shiftworks][MP][FEED#` ve `[Shiftworks][MP][BUILD]` ile başlayan satırları) paylaşın —
   bu loglar hangi dalın (YEREL FALLBACK mı, yoksa RPC mi) çalıştığını ve host'un isteği
   gerçekten reddedip reddetmediğini kesin olarak gösterecek.

Aşama 11C hâlâ tamamlandı işaretlenmedi.

## Aşama 11C — Montaj Girdisi DOĞRULANDI + Paketleme/Sevkiyat Senkronizasyonu Eklendi

Kullanıcı, önceki teşhis turunda eklenen loglarla testi tekrarladı; hem Host (`Editor.log`)
hem Client (`Player.log`) `[Shiftworks][MP][FEED#1]` satırları karşılaştırıldı: Host kendi
parçasını besledi (0→1), Client'ın isteği gerçekten RPC ile host'a ulaştı
(`SERVER_REQUEST_RECEIVED`), host kabul etti (`SERVER_REQUEST_ACCEPTED`, 1→0, `Processing`'e
geçiş — batch tamamlanınca sayacın sıfırlanması `MachineController.TryAcceptInput`'un zaten
var olan, değişmemiş mantığı, bir hata değil). **Montaj girdisi senkronizasyonu kullanıcı
tarafından çalışır olarak doğrulandı.**

Kullanıcı ardından yeni bir eksik bildirdi: sevkiyat kutusu (paketleme ilerlemesi + teslimat)
taraflar arasında senkron değildi — `PlayerInteractor.HandlePackingInteraction`/
`HandleShippingInteraction`, `PackingStation.TryPack`/`ShippingZone.TryDeliver`'ı hiçbir
köprü olmadan DOĞRUDAN çağırıyordu (kod okunarak doğrulandı) — bu bir regresyon değil, 11B'nin
kendi belgesinde zaten "paketleme/sevkiyat/tamamlama bu aşamanın kapsamı dışında, sadece hangi
siparişin aktif olduğu senkron" diye açıkça belirtilmiş, hiç ele alınmamış bir alandı. Kullanıcı
onayıyla şimdi eklendi.

### Yapılan Değişiklik (aynı yerleşik host-authoritative kalıp)
- **`NetworkEconomyBridge`**: `_syncedActiveOrderIndex`'in yanına iki yeni `NetworkVariable`
  eklendi — `_syncedActiveOrderProgress` (int) ve `_syncedActiveOrderState` (OrderState).
  Önceden `ApplyActiveOrderLocally` her zaman sabit `(Active, 0)` uyguluyordu (yani Client'ın
  "(N/M)" göstergesi hiç ilerlemiyordu); şimdi host'un gerçek ilerleme/durumunu her karede
  (`PushActiveOrderProgressAndState`, balance/index ile aynı poll döngüsünde) yayınlıyor.
- **`NetworkItemInteractionBridge`**: `RequestPack`/`RequestDeliver` eklendi — `RequestFeedMachine`
  ile birebir aynı şekil (anti-sahtecilik `CurrentHolder` kontrolü, tüketim sadece host'ta).
  Paketleme batch'i tamamlarsa oluşan `ShipmentPackage`, `PackingStation.LastSpawnedPackage`
  üzerinden okunup `NetworkObject.Spawn()` ediliyor (mevcut `RequestPickup` akışıyla herkes
  alabiliyor — yeni bir alma mantığı gerekmedi). Teslimatın ödül/para kısmı zaten mevcut
  `EconomyManager.Balance` senkronuyla, sipariş tamamlanması da zaten mevcut aktif-sipariş
  index senkronuyla otomatik yayılıyor — ek bir mekanizma gerekmedi.
- **`PackingStation.cs`**: yeni salt-okunur `LastSpawnedPackage` erişimcisi (`MachineController.
  PendingOutput` ile aynı "dışarıdan sadece okunur" kuralı).
- **`PlayerInteractor.cs`**: `HandlePackingInteraction`/`HandleShippingInteraction` artık köprü
  üzerinden yönlendiriliyor (diğer tüm etkileşimlerle aynı bridge-aware kalıp).
- **Yeni Setup menüsü 39** (`ShiftworksMultiplayerItemSetup.cs`): sahnedeki tek `PackingStation`/
  `ShippingZone`'u köprüye bağlıyor (OutputStorage'ın menü 36'sıyla aynı yöntem — bu iki
  bileşen kendisi `NetworkObject` almıyor, zaten tek örnekler).

### Bilinen Küçük Kozmetik Sınır (kapsam dışı bırakıldı, raporlanıyor)
`ShipmentPackage.AssociatedOrder` senkron DEĞİL (sadece host'ta `Initialize()` ile set
ediliyor) — gerçek teslimat host'un kendi kopyasını kullandığı için **doğru çalışıyor**, ama
Client'ın kendi paketi üzerindeki "E - Siparişi Teslim Et" prompt metni, paket henüz ilk kez
senkron bir sipariş değişikliğinden geçmediyse yanlışlıkla "Geçersiz paket" gösterebilir.
Kullanıcı tarafından bildirilmedi, kapsam dışı tutuldu — rapor ediliyor, istenirse ayrı bir
düzeltme olarak ele alınabilir.

## Değiştirilen/Oluşturulan Dosyalar (bu genişletme)
- `Assets/_Project/Scripts/Multiplayer/NetworkEconomyBridge.cs`
- `Assets/_Project/Scripts/Multiplayer/NetworkItemInteractionBridge.cs`
- `Assets/_Project/Scripts/Orders/PackingStation.cs`
- `Assets/_Project/Scripts/Player/PlayerInteractor.cs`
- `Assets/_Project/Scripts/Editor/ShiftworksMultiplayerItemSetup.cs` (menü 39)
- **Hiç dokunulmayan**: `OrderManager`/`ShippingZone`'un kendi atomik metotları
  (`TryRegisterPackedItem`/`TryCompleteOrder`/`TryDeliver`), montaj/depo/ekonomi sistemleri,
  tek oyunculu sahne.

## Kullanıcıya Düşen Adımlar (bu genişletme)
1. Unity Editor'de **`Shiftworks/Setup/39. Add Packing And Shipping Sync To WorkshopMultiplayer
   Scene`** menüsünü çalıştırın (tek seferlik; `Apply Multiplayer Item Interaction` ile de
   otomatik çalışır).
2. Yeni bir Windows build alın.
3. Test: Host VE Client sırayla (veya birbirini tamamlayarak) paketleme istasyonuna parça
   koysun — her iki ekranda da AYNI "(N/M)" ilerlemesini göstermeli. Paket hazır olunca
   herhangi bir oyuncu sevkiyat alanına taşıyıp teslim edebilmeli, ödül ortak bakiyeye
   eklenmeli.

Aşama 11C hâlâ tamamlandı işaretlenmedi — kullanıcı testlerini bekliyorum.

## Bilinen Teknik Riskler
- Aşama 1'den sonra hiçbir aşamada gerçek bir Unity batch-mode derleme testi yapılmadı (Unity
  Editor kullanıcı tarafından sürekli açık tutulduğu için); tüm kod elle satır satır incelenerek
  doğrulandı, derleyici onayı değildir.
- `OrderManager` şu an sadece **tek bir aktif sipariş**i destekliyor (`HasActiveOrder` kontrolü
  ikinci kabulü engelliyor) — Aşama 9 bu kısıtı **kasıtlı olarak korudu** (spec'in izniyle);
  artık gerçekten iki sipariş tanımı listede olduğu için bu sınır aktif şekilde test edilebilir
  (bkz. "Aşama 9 Test Talimatları" madde 6) — çoklu AKTİF sipariş gelecekte eklenirse bu kısıt
  gözden geçirilmeli.
- Otomatik üretim hattı ve sipariş istasyonlarının sahnedeki konumları elle hesaplanıp
  çarpışmayacak şekilde ayarlandı, ama tam görsel yerleşim kullanıcı tarafından Editor'de
  gözle doğrulanmadı (sadece fonksiyonel testler yapıldı).
- UI, Unity'nin eski/`Text`+`Button` (UGUI) bileşenleriyle kuruldu; TextMeshPro yok.
  `EventSystem` + `InputSystemUIInputModule`, Aşama 5 hata düzeltmesinde eklendi ve kullanıcı
  tarafından çalışır olarak doğrulandı.
- Aşama 6'nın yerleştirme sistemindeki "zemin her yerde çakışma sayılıyor" hatası (önizleme
  kutusunun kendi oturduğu yüzeye sıfır boşlukla dokunması) düzeltildi ve kullanıcı tarafından
  doğrulandı (8/8). Kalan çok düşük olasılıklı bir kenar durum: yerleştirme kontrolü, taşınan/
  otomasyondaki ürünlerle paylaştığı "Ignore Raycast" katmanını hariç tutuyor.
- **(Multiplayer)** `com.unity.netcode.gameobjects` (2.13.3) ve `com.unity.transport` (2.7.4)
  paketleri eklendi; NGO 2.13.3 Unity 6000.3 için Unity dokümantasyonunda **"pre-release"**
  işaretli olduğundan `m_EnablePreviewPackages` 0'dan 1'e değiştirildi. Paketler Unity Editor'de
  hatasız çözümlendi (Console'da paket hatası yoktu) — ama "pre-release" statüsü uzun vadede
  Unity'nin sürümü değiştirme/kaldırma riskini hâlâ taşıyor, ileride izlenmeli.
- **(Multiplayer, düzeltme)** İlk kurulumda `NetworkManager.NetworkConfig.NetworkTransport`
  script ile hiç atanmamıştı ("No transport has been selected!" hatası, Host/Client
  başlatılamıyordu). `Shiftworks/Setup/17. Fix Multiplayer NetworkManager Transport` menüsüyle
  düzeltildi; kaynak Editor aracı da (`ShiftworksMultiplayerPrototypeSetup.cs`) gelecekteki
  taze kurulumlar için düzeltildi. **Düzeltme daha önce "kullanıcı tarafından test edilip
  başarılı olundu" olarak kaydedilmişti — bu yanlıştı.** Sahne dosyası incelendiğinde
  `NetworkTransport` referansının doğru atandığı doğrulandı (kod/sahne seviyesinde geçerli),
  ama kullanıcının gerçekten Host/Client başlatıp bağlandığı hiç doğrulanmadı.
- **(Multiplayer, KANITLANDI)** Kullanıcı test ekranında Host/Client butonlarını göremediğini
  bildirdi. İnceleme sonucu: `MultiplayerPrototype.unity` sahne dosyasında
  `ConnectionUICanvas`/`HostButton`/`ClientButton`/`DisconnectButton`/`StatusText`/`EventSystem`
  hepsi `m_IsActive: 1` ve doğru bağlı; `NetworkManager`'ın `NetworkTransport` ve `PlayerPrefab`
  alanları doluydu; `ProjectSettings/EditorBuildSettings.asset` içinde `MultiplayerPrototype`
  etkin tek sahne olarak kayıtlı. **Kodda/sahnede bir hata bulunamadı.** Kullanıcının paylaştığı
  ekran görüntüsü varsayımı doğruladı: Editor penceresinin başlık çubuğunda "WorkshopPrototype"
  yazıyordu ve Hierarchy'de `Floor`/`Wall_North`/`CuttingMachine` gibi tek oyunculu atölye
  nesneleri vardı — yani Play'e basıldığında Editor'de hâlâ **yanlış sahne** açıktı (Build
  Settings sahne listesi Play Mode'u etkilemez, sadece Build'i etkiler). Bu yüzden multiplayer
  paneli hiç yüklenmedi. Build (`.exe`) tarafı doğru sahneyi (`MultiplayerPrototype`) açtı ve
  panel göründü, ama "Client Bağlan"a basıldığında **hiçbir yerde Host çalışmadığı için**
  (Editor yanlış sahnedeydi) bağlantı kurulamadı; UTP'nin kendi iç yeniden deneme süresi
  60 saniyeye kadar çıkabildiğinden bu "Client bağlanıyor..." yazısının sonsuza kadar takılı
  kalması gibi görünüyordu. `MultiplayerConnectionUI.cs`'e client tarafı için **8 saniyelik bir
  izleyici (watchdog) zaman aşımı** eklendi — bu süre içinde bağlanılamazsa client kendini
  kapatıp net bir şekilde "Bağlantı hatası (Host bulunamadı)" gösteriyor
  (`NetworkManager`/`UnityTransport`'ın kendi retry ayarlarına dokunulmadı, bu saf bir UI
  katmanı zaman aşımı). Durum metinleri de istenen tüm durumları ("Bağlı değil", "Host aktif
  (N istemci)", "Client bağlanıyor...", "Client bağlı", "Bağlantı kesildi", "Bağlantı hatası
  (Host bulunamadı)") açıkça ayırt ediyor. Doğru test prosedürü: **önce** bir pencerede Host
  Başlat'a basılmalı, **sonra** diğer pencerede Client Bağlan — adım adım "Multiplayer Test
  Talimatları" bölümünde.
- **(Multiplayer)** İki oyuncu spawn olduğunda şu an aynı noktada (prefabın varsayılan
  konumu) belirir — ayrı spawn noktaları veya çakışmayı önleyen bir mekanizma eklenmedi (kasıtlı
  olarak basit tutuldu); oyuncular birbirinin içinde başlayabilir, hareket ettiklerinde
  `CharacterController` çarpışmasıyla ayrışmaları beklenir. Bu da henüz kullanıcı tarafından
  gözlemlenmedi (bkz. yukarıdaki düzeltme), bilinen bir kozmetik sınırlama olarak kalıyor.
- **(Multiplayer, yeni)** `WorkshopPrototype.unity`, `ProjectSettings/EditorBuildSettings.asset`
  sahne listesinde **yok** (listede yalnızca devre dışı `SampleScene` ve etkin
  `MultiplayerPrototype` var). Bu, şu an istenen bir sorun değil (Editor Play Mode'u etkilemez,
  tek oyunculu oyun hâlâ Editor'de normal test ediliyor) ama tek oyunculu oyunun gerçek bir
  Windows Build'i alınmak istenirse `WorkshopPrototype.unity`'nin de Build Settings'e
  eklenmesi gerekecek — bu talep gelmeden eklenmedi.
- **Git durumu (ÇÖZÜLDÜ)**: Önceki oturumlarda Aşama 2'den beri hiçbir şey commit edilmemişti;
  bu artık geçerli değil — `af97fd6` ("Complete multiplayer prototype and prepare Stage 7")
  ile çalışma ağacı Aşama 7 başlamadan önce tertemizdi. Aşama 7'nin kendi değişiklikleri
  (bu oturumda yazılan scriptler + kullanıcı Unity'de menüleri çalıştırdıktan sonra oluşacak
  sahne/asset değişiklikleri) **henüz commit edilmedi** — kullanıcı onayı olmadan commit
  yapılmaz (talimat).
- **(Aşama 7, yeni)** Hiçbir script gerçek Unity derlemesiyle (batch-mode veya Editor'ün kendi
  derlemesiyle) test edilmedi — Editor kullanıcı tarafından sürekli açık tutulduğu için elle
  kod incelemesiyle ilerlendi, önceki aşamalarla aynı sınırlama. Kullanıcı Editor'e geçip
  script'lerin derlendiğini (Console'da hata olmadığını) ilk doğrulaması gereken adım bu.
- **(Aşama 7, yeni)** `ShiftworksFactoryExpansionSetup.AddExpansionHallToScene`, paylaşılan
  `Wall_West` objesini siler ve yerine iki parçaya böler (`Wall_West_South`/`North` + 2 m
  boşluk + kilitli bariyer) — bu **tek seferlik, idempotent** bir sahne mutasyonudur
  (`FactoryExpansionManager` sahnede zaten varsa menü hiçbir şey yapmadan çıkar). Menü birden
  fazla çalıştırılırsa `Wall_West` ikinci kez bulunamayacağı için hata verip duracaktır, bu
  beklenen/güvenli davranıştır.
- **(Aşama 7, yeni)** Kilitli bariyerin üzerindeki `TextMesh` etiketinin ("Üretim Alanı B —
  Kilitli") yüzünün hangi yöne baktığı (oyuncunun atölyeden yaklaştığı yönden okunabilir mi)
  Editor'de gözle doğrulanmadı — sadece 90° bir varsayılan döndürme uygulandı. Gerekirse
  `LockedLabel` child objesinin rotasyonu Editor'de elle düzeltilebilir, mekanik işleve
  (bariyerin açılması) etkisi yok.
- **(Aşama 7, yeni)** Yeni salonun tek ışık kaynağı, sahnedeki mevcut `Directional Light`
  (sonsuz menzilli, zaten tüm sahneyi kapsıyor) — bilinçli olarak ek bir ışık eklenmedi
  ("gereksiz asset ekleme" talimatına uyarak). Görsel olarak yetersiz görünürse ayrıca bir
  `Point Light` eklenebilir.
- **(Aşama 8, ÇÖZÜLDÜ)** Hiçbir script gerçek Unity derlemesiyle test edilmedi iddiası artık
  geçerli değil — kullanıcı Aşama 8'i 18/18 testle onayladı, yani tüm Aşama 8 script'leri
  hatasız derlendi ve çalıştı.
- **(Aşama 8, yeni)** Başlangıç kapasitesi 10 kW, mevcut hattın (1 makine 4 kW tepe + 3
  konveyör × 0,5 kW sürekli = 5,5 kW tepe) rahatça altında olduğu için **değiştirilmeden**
  bırakıldı — kod incelemesiyle doğrulandı, Unity'de gözle de teyit edilmeli (adım 3,
  "Aşama 8 Test Talimatları").
- **(Aşama 8, yeni)** "Üretim sırasında güç kesilirse duraklasın, geri gelince kaldığı yerden
  devam etsin" davranışı kod seviyesinde doğru uygulandı (`Update()`'teki zaman akışı
  `HasPower`'a bağlı) ama **bu aşamanın özelliklerinde zaten tahsis edilmiş bir gücü geri
  alan hiçbir mekanizma yok** (kapasite azaltma, elektrik kesintisi vb. kapsam dışı
  bırakıldı) — yani bu güvenlik davranışı şu an pratikte hiç tetiklenmeyecek, sadece
  ileride (ör. Aşama 16 arıza sistemi) böyle bir mekanizma eklenirse sıfır ek değişiklikle
  doğru çalışacak şekilde tasarlandı. Bu normal ve beklenen bir durum, hata değil.
- **(Aşama 8, yeni)** "Yetersiz güç nedeniyle bekleyen ekipman" sayacı, elle beslenen bir
  makinenin oyuncu E'ye bastıktan sonra tekrar denemediği durumlarda hafifçe abartılı
  kalabilir (makine "bekliyor" olarak işaretli kalır çünkü oyuncu başka bir şey yapıyordur)
  — kasıtlı, basit bir yaklaşım (ayrı bir "pes etti" durumu takip edilmiyor); otomatik
  (konveyörle beslenen) ekipmanlar için bu sayaç her zaman anlık doğru.
- **(Aşama 8, yeni)** `ShiftworksPowerSetup.AddPowerSystemToScene`, sahnedeki `CuttingMachine`
  ve üç `Conveyor_*` objesini **isimle** bulup bağlıyor — bu objelerin adları Aşama 1/3/4
  kurulum araçlarınca sabit şekilde veriliyor, değiştirilmediği sürece güvenli. Menü, `.prefab`
  asset'teki `MachineController`'ın `powerManager` alanını **kasıtlı olarak boş bırakıyor**
  (prefab asset'ler sahne nesnesine kalıcı referans tutamaz) — bu alan yerine her çalışma
  zamanı örneği kendi `Awake()`'inde `FindFirstObjectByType<PowerManager>()` ile sahnedeki tek
  `PowerManager`'ı buluyor; bu, Aşama 6/7'de hiç kullanılmamış yeni bir desen, ama sahnede tam
  olarak bir `PowerManager` olduğu sürece güvenli ve deterministik.
- **(Aşama 9, yeni)** Hiçbir script gerçek Unity derlemesiyle test edilmedi — Editor kullanıcı
  tarafından sürekli açık tutulduğu için elle kod incelemesiyle ilerlendi, önceki aşamalarla
  aynı sınırlama. Özellikle `MachineController.TryAcceptInput`'teki `recipe.InputCount`
  genellemesi kritik — kesim makinesinin (InputCount=1) davranışının gerçekten değişmediği
  Unity'de Play Mode'da doğrulanmalı (bkz. "Aşama 9 Test Talimatları" madde 2).
- **(Aşama 9, yeni)** `ShiftworksAssemblyProductionSetup.cs`'nin 4 menü adımı **sırayla**
  çalıştırılmalı (22→23→24→25) — her biri bir öncekinin ürettiği asset'e (`AssembledComponent`
  item'ı, `AssembleComponentRecipe`, `AssemblyMachineDefinition`) bağımlı; yanlış sırada
  çalıştırılırsa Console'da açık bir hata mesajı verip o adımı atlar (sessizce bozuk bir
  duruma düşmez), ama yine de doğru sırayla çalıştırılmalı.
- **(Aşama 9, yeni)** İkinci `OrderBoard`/`MachineShopTerminal` eklemek yerine (OrderBoard/
  MachineShopTerminal'ı çoklu-seçim listesine genelleştirmek) bilinçli bir mimari tercihti —
  "mevcut mimariye uygun, en az müdahale" ilkesiyle gerekçelendirildi (bkz. "Önemli Scriptler"
  bölümü). İleride çok daha fazla ürün/makine türü eklenirse (Aşama 9'un ötesinde), fiziksel
  terminal sayısının da artmasındansa gerçek bir çoklu-seçim UI'sına geçmek gerekebilir —
  şimdilik 2 sipariş/2 makine türü için bu ölçeklenme sorunu değil.
- **(Aşama 9, yeni)** Montaj makinesi (ve kesim makinesinin satın alınan kopyası) **çıkış
  konveyörüne bağlı değil** — Aşama 6'nın kesim makinesi kopyasıyla aynı kasıtlı tasarım
  kararı (yalnızca manuel üretim). Spec'in "uygun konveyör bağlantısı mevcutsa" şartı
  gereği zaten opsiyoneldi; serbest konveyör inşa sistemi olmadığı için bu aşamada
  bağlanmadı, gelecekte (Aşama 11) değerlendirilebilir.
- **(Yeni)** `DevelopmentCheats` (F8 test parası) `#if UNITY_EDITOR` ile korunuyor — gerçek
  bir Windows Build (Steam/release) alındığında bu kod **derlenmez**, F8'in fiziksel olarak
  hiçbir etkisi olmaz. Yine de bu bileşen `WorkshopPrototype.unity`'de kalıcı bir sahne
  nesnesi; ileride bir sürüm hazırlığı yapılırken (Aşama 20) sahneden kaldırılıp
  kaldırılmayacağı ayrıca kararlaştırılmalı (muhtemelen gerek yok, zaten build'de inert).
- **(Yeni)** Hiçbir script gerçek Unity derlemesiyle test edilmedi (bu oturumun
  `DevelopmentCheats`/`ShiftworksDevToolsSetup` dahil) — önceki aşamalarla aynı sınırlama,
  kullanıcı Editor'e geçip Console'u kontrol etmeli.
- **(Aşama 10, yeni)** Hiçbir kayıt/yükleme script'i gerçek Unity derlemesiyle test edilmedi —
  bu, özellikle `SaveManager`'ın bu kadar çok sistemle etkileşen boyutu göz önüne alındığında,
  önceki aşamalardan daha kritik bir doğrulama adımı. "Aşama 10 Test Talimatları" bölümündeki
  adımların tam olarak uygulanması önemle önerilir.
- **(Aşama 10, yeni)** Konveyör üzerindeki ürünün tam ara pozisyonu korunmuyor (segment başına
  sıfırlanıyor) — bilinçli, belgelenmiş bir basitleştirme (spec'in izniyle), ürün kaybı/çoğalma
  riski yok, sadece görsel bir sadeleştirme. Ayrıntı için "Aşama 10 — Desteklenmeyen/
  Basitleştirilmiş Durumlar" bölümüne bakın.
- **(Aşama 10, yeni)** Oturum ortasında (kapat-aç yapmadan, Play Mode hiç durmadan) tekrar
  yükleme yapma senaryosu kod seviyesinde destekleniyor (`ClearDynamicState`) ama **hiç
  denenmedi** — asıl doğrulanması gereken, istenen asıl senaryo olan "kaydet → oyundan çık →
  yeniden aç → yükle" akışıdır.
- **(Aşama 10, yeni)** `SaveCatalog`'un ID çözümlemesi `MachineDefinition`/`OrderDefinition`/
  `FactoryExpansionDefinition` için asset'in Unity **adını** (`.name`, yani `.asset` dosya
  adı) kullanıyor — bu asset'ler Editor'de yeniden adlandırılırsa (örn. "AssemblyOrder.asset"
  → başka bir ad), **eski kayıt dosyaları o referansı çözemez hale gelir** (sessizce atlanır,
  çökme olmaz, ama o veri kaybolur). Pratikte bu asset'ler kurulum araçlarınca sabit adlarla
  oluşturuldu ve elle yeniden adlandırılmadığı sürece risk yok.
- **(Aşama 10, ÇÖZÜLDÜ)** İlk sürümde `PlayerInteractor`'daki yeni Escape işleyicisi ile
  `FirstPersonController`'ın kendi bağımsız "Escape imleci serbest bırakır" davranışı **aynı
  tuşa bağımsız bağımsız tepki veriyordu** — bu, raporda yanlışlıkla "çakışma yaratmıyor"
  olarak değerlendirilmişti. Gerçekte, `FirstPersonController.Update()`'in
  `PlayerInteractor.Update()`'ten **önce** çalıştığı karelerde imleç FirstPersonController
  tarafından hemen serbest bırakılıyor, bu yüzden `PlayerInteractor` kendi Escape kontrolüne
  ulaştığında "imleç zaten kilitli değil" koruması yüzünden erken çıkıyor ve **menü hiç
  açılmıyordu** — kullanıcı tam olarak bunu bildirdi ("Escape'e bastığımda sadece mouse
  imleci geliyor"). Düzeltme: `FirstPersonController.HandleCursorToggle`'dan Escape dalı
  tamamen kaldırıldı (artık yalnızca sol-tık ile yeniden kilitlemeyi yönetiyor);
  `PlayerInteractor.HandleEscapeKey` artık Escape'in **tek** sahibi — menü varsa açıyor,
  yoksa (Aşama 10 menüsü henüz çalıştırılmadıysa) eski "sadece imleci serbest bırak"
  davranışına geri dönüyor. Artık iki bağımsız sistem aynı tuşa tepki vermiyor, çalışma
  sırası riski ortadan kalktı.
- **(Aşama 11A, yeni)** `MultiplayerSpawnPoints`'in `NetworkManager.ConnectionApprovalCallback`
  kullanımı projenin kurulu Netcode for GameObjects 2.13.3 paketinin gerçek kaynak koduna
  karşı doğrulandı (`Library/PackageCache/com.unity.netcode.gameobjects@.../Runtime/Core/
  NetworkManager.cs`): `ConnectionApprovalCallback` `event` değil düz bir `Action<,>` property,
  `+=`/`-=` tek-dinleyici senaryosunda güvenli; `ConnectionApprovalResponse.Position`/
  `Rotation`/`CreatePlayerObject`/`Approved`/`Pending` alan adları ve `HostServerInitialize`'ın
  host'un kendi yerel bağlantısı için de bu callback'i çağırdığı (host da bir spawn noktası
  alır, ilk client farklı birini) doğrulandı. Derleme hatası riski düşük, ama **gerçek Unity
  derlemesiyle** (bu script'ler Editor kapalıyken yazıldı) henüz test edilmedi.
- **(Aşama 11A, yeni)** `WorkshopMultiplayer.unity`, `WorkshopPrototype.unity`'nin tam bir
  kopyası olduğu için, Aşama 10'da zaten kurulmuş olan `SaveManager`/`SaveCatalog`/
  `SaveLoadMenuUI` ve tüm üretim/sipariş/güç panelleri de kopyaya dahil oluyor. Bunların
  hiçbiri 11A'da **çalışmaz hale gelmez** ama **tetiklenemez** hale gelir — çünkü onları açan
  tek bileşen (`PlayerInteractor`, E/Escape tuşlarının tek sahibi) sadece tek-oyunculu
  `Player.prefab`'ın üzerindeydi ve bu kopyada kaldırıldı; `NetworkPlayer.prefab`'ta henüz
  `PlayerInteractor` yok (bu, Aşama 11C'nin kapsamı). Sonuç: HUD'daki bakiye/sipariş sayıları
  her istemcide **yerel ve birbirinden bağımsız** görünür (host'un 500$'ı client'ın 500$'ı ile
  aynı değişken değildir) — bu, 11A'nın bilinçli ve beklenen bir sınırı, hata değil.
