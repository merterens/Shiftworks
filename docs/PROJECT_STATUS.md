# Shiftworks — Proje Durumu

_Son güncelleme: Aşama 1–6 tamamlandı ve test edildi. Aşama 7 (hazır üretim alanı satın alma —
"Üretim Alanı B") **geliştirildi, kullanıcı oynanış testleri bekleniyor.**_

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

Kullanıcı tarafından onaylanan test sonuçları: Aşama 1 (7/7), Aşama 2 (4/4 + hız/kaybolma hata
düzeltmesi), Aşama 3 (11/11), Aşama 4 (11/11), Aşama 5 (ilk uygulama + 9/9 hata düzeltme testi),
Aşama 6 (8/8, yerleştirme zemin-çakışma hata düzeltmesi dahil). **Multiplayer teknik prototipi
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

**0. En son geliştirme (bu oturum — multiplayer test menüsü/durum düzeltmesi)**: Kullanıcı,
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

**4. Sıradaki görev**: Kullanıcı önce multiplayer test talimatlarını (bkz. aşağıdaki
"Multiplayer Test Talimatları" bölümü) uygulayıp iki oyunculu bağlantıyı gerçekten test edecek,
sonra Aşama 7'yi Unity Editor'de test edecek (menüler + Play Mode). Onay sonrası Aşama 8 (yeni
ürünler ve çok aşamalı üretim zincirleri) planlanacak.

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

**7. Bu oturumda değişen dosya (multiplayer düzeltmesi)**:
`Assets/_Project/Scripts/Multiplayer/MultiplayerConnectionUI.cs` (durum metni/geçici mesaj
mantığı güçlendirildi). `MultiplayerPrototype.unity` sahne dosyasına **hiç dokunulmadı** —
inceleme sonucu sahnede bir hata bulunmadı, bu yüzden sahne/prefab/Editor aracı
(`ShiftworksMultiplayerPrototypeSetup.cs`) değiştirilmedi.

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
- **(Multiplayer prototipi)** `Assets/_Project/Prefabs/Multiplayer/NetworkPlayer.prefab` —
  `CharacterController` + **mevcut, değiştirilmemiş** `FirstPersonController` (varsayılan
  **devre dışı**, sadece sahip istemcide açılır) + `NetworkObject` + `NetworkTransform`
  (`AuthorityMode = Owner`) + `NetworkPlayerSetup`. Tek oyunculu `Player.prefab`'tan tamamen
  ayrı; eşya taşıma/etkileşim bileşenleri bu prototipte kasıtlı olarak yok.

## ScriptableObject Veri Dosyaları
- `Assets/_Project/Data/Items/MetalSheet.asset`, `CutMetalPart.asset` — `ItemDefinition`.
- `Assets/_Project/Data/Recipes/CutMetalRecipe.asset` — `RecipeDefinition` (MetalSheet → CutMetalPart, 5 sn).
- `Assets/_Project/Data/Orders/FirstMetalPartOrder.asset` — `OrderDefinition` ("İlk Metal Parça
  Siparişi", 3× CutMetalPart, 150 $ ödül, süre sınırı yok).
- **(Aşama 6)** `Assets/_Project/Data/Machines/CuttingMachineDefinition.asset` — `MachineDefinition`
  ("Kesim Makinesi", 300 $, `CutMetalRecipe`, `CuttingMachine.prefab`).
- **(Aşama 7)** `Assets/_Project/Data/FactoryExpansions/ProductionAreaB.asset` —
  `FactoryExpansionDefinition` ("Üretim Alanı B", 400 $, 10×15 m — görsel geometri bu asset'ten
  üretilmiyor, sahneye elle inşa edildi; asset sadece fiyat/isim/boyut metni için).

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

## Bilinen Teknik Riskler
- Aşama 1'den sonra hiçbir aşamada gerçek bir Unity batch-mode derleme testi yapılmadı (Unity
  Editor kullanıcı tarafından sürekli açık tutulduğu için); tüm kod elle satır satır incelenerek
  doğrulandı, derleyici onayı değildir.
- `OrderManager` şu an sadece **tek bir aktif sipariş**i destekliyor (`HasActiveOrder` kontrolü
  ikinci kabulü engelliyor) — çoklu sipariş eklenince bu kısıt gözden geçirilmeli.
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
