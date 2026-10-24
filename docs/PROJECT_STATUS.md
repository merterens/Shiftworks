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
gelmesi) **geliştirildi — kullanıcı testleri bekleniyor.** Ayrıntılar "Aşama 11A Mimarisi" ve
"Aşama 11B Mimarisi" bölümlerinde._

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
| 11B | Ekonomi + makine satın alma/yerleştirme — host-authoritative | 🛠️ **Geliştirildi — kullanıcı testleri bekleniyor** |

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
