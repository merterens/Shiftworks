# Shiftworks — Proje Durumu

_Son güncelleme: Aşama 1–6 tamamlandı ve test edildi. İki oyunculu multiplayer teknik prototipi
başarıyla test edildi (host+client bağlantısı, hareket/kamera senkronizasyonu doğrulandı).
Aşama 7 (fabrika inşa ve genişletme) henüz başlanmadı — sıradaki hedef._

## Aşama Durumu

| # | Aşama | Durum |
|---|---|---|
| 1 | FPS karakter ve atölye (WorkshopPrototype sahnesi, Player prefab) | ✅ Tamamlandı |
| 2 | Eşya alma, taşıma, bırakma, fırlatma (fizik tabanlı) | ✅ Tamamlandı |
| 3 | İlk üretim makinesi (kesim makinesi, ürün/reçete sistemi) | ✅ Tamamlandı |
| 4 | Konveyör bantları ve otomatik üretim hattı | ✅ Tamamlandı |
| 5 | Sipariş, ekonomi, paketleme ve teslimat sistemi | ✅ Tamamlandı |
| 6 | Makine satın alma ve yerleştirme | ✅ Tamamlandı (8/8 test, yerleştirme zemin-çakışma hatası düzeltildi) |
| — | İki oyunculu multiplayer teknik prototipi (Aşama 6 civarı, erken doğrulama) | ✅ **Kullanıcı tarafından başarıyla test edildi** |
| 7 | Fabrika inşa ve genişletme (alan satın alma, duvar/zemin düzenleme) | ⏭️ **Sıradaki hedef — henüz başlanmadı** |

Kullanıcı tarafından onaylanan test sonuçları: Aşama 1 (7/7), Aşama 2 (4/4 + hız/kaybolma hata
düzeltmesi), Aşama 3 (11/11), Aşama 4 (11/11), Aşama 5 (ilk uygulama + 9/9 hata düzeltme testi),
Aşama 6 (8/8, yerleştirme zemin-çakışma hata düzeltmesi dahil), **Multiplayer teknik prototipi
(host başlatma, client bağlanma, iki oyuncunun aynı odada birbirinin hareket/dönüşünü görmesi
— bir `NetworkManager` transport-bağlama hatası bulunup düzeltildikten sonra başarılı)**.

⚠️ **Önemli netlik**: Multiplayer prototipinin başarılı olması, oyunun **bütün sistemlerinin**
multiplayer olduğu anlamına **gelmez**. Sadece oyuncu hareketi/kamerası (owner-authoritative
`NetworkTransform`) ayrı, minimal bir sahnede (`MultiplayerPrototype.unity`) test edildi. Eşya
taşıma, üretim, konveyör, sipariş, ekonomi ve makine satın alma/yerleştirme sistemlerinin
**hiçbiri** henüz ağ üzerinden çalışmıyor; hepsi `WorkshopPrototype.unity`'de tamamen
yerel/tek-oyunculu durumda. Gerçek host-authoritative entegrasyonu Aşama 18'e aittir.

## Yeni Oturum İçin Devam Notları

**1. En son tamamlanan geliştirme**: İki oyunculu multiplayer teknik prototipi. NGO/UTP paketleri
eklendi, `NetworkPlayer.prefab` + `MultiplayerPrototype.unity` sahnesi + Host/Client/Bağlantı
Kes test paneli oluşturuldu. Yol boyunca iki hata bulunup düzeltildi: (a) Aşama 6'da yerleştirme
önizlemesinin kendi oturduğu zemine sıfır boşlukla dokunması yüzünden her yerin "geçersiz"
görünmesi, (b) multiplayer kurulumunda `NetworkManager.NetworkConfig.NetworkTransport`'un script
ile kurulumda hiç atanmaması (`Shiftworks/Setup/17. Fix Multiplayer NetworkManager Transport`
menüsüyle düzeltildi). İkisi de kullanıcı tarafından doğrulandı.

**2. Şu anda çalışan sistemler**: FPS hareket/bakış, eşya alma/taşıma/bırakma/fırlatma, kesim
makinesi (manuel + otomatik), konveyör hattı, besleme istasyonu, çıkış deposu, sipariş
panosu/kabul, paketleme, teslimat, ekonomi (bakiye), makine mağazası (satın alma + yerleştirme
önizlemesi + geçerlilik kontrolü), ve ayrı sahnede iki oyunculu temel ağ bağlantısı/hareket
senkronizasyonu. Hepsi `WorkshopPrototype.unity`'de (multiplayer hariç) çalışıyor.

**3. Mevcut teknik riskler**: Aşağıdaki "Bilinen Teknik Riskler" bölümüne bakın — özetle: hiçbir
aşama gerçek Unity batch-mode derlemesiyle doğrulanmadı (Editor sürekli açık tutulduğu için elle
kod incelemesiyle ilerlendi); `OrderManager` tek aktif sipariş sınırlı; multiplayer prototipi
NGO'nun "pre-release" işaretli bir sürümünü kullanıyor; iki oyuncu şu an aynı noktada spawn
oluyor.

**4. Sıradaki görev**: **Aşama 7 — Fabrika inşa ve genişletme** (alan satın alma, duvar/zemin
düzenleme). Henüz kapsamı kullanıcıyla detaylandırılmadı, henüz kod yazılmadı.

**5. Yeni oturumda okunması gereken dosyalar**: `CLAUDE.md`, `docs/PROJECT_STATUS.md` (bu dosya),
`docs/ROADMAP.md`. Aşama 7'ye özel olarak muhtemelen ilgili olacak mevcut scriptler: sahne/alan
sınırlarıyla ilgili bir sistem henüz yok, bu yüzden `ShiftworksSceneSetup.cs` (WorkshopPrototype'ın
zemin/duvar inşa deseni) ve `MachinePlacementController.cs`/`ShiftworksMachineShopSetup.cs`
(Aşama 6'nın yerleştirme/geçerlilik deseni — muhtemelen fabrika genişletmede yeniden
kullanılabilir bir desen) incelenmeli.

**6. Aşama 7'ye başlamadan önce kontrol edilmesi gerekenler**:
- `git status` ile hâlâ **hiçbir şeyin commit edilmediğini** doğrulayın (bkz. riskler) — kullanıcıya
  hatırlatılmalı, büyük bir kayıp riski.
- Unity Editor'ün açık olup olmadığını kontrol edin (ikinci bir batch-mode süreç başlatmamak için).
- `WorkshopPrototype.unity`'nin mevcut 20×15 m oda sınırlarını (duvar konumları) inceleyin —
  "fabrika genişletme" bu sınırları değiştirecek/aşacak bir sistem olacağından, mevcut duvarların
  nasıl inşa edildiğini (`ShiftworksSceneSetup.CreateWorkshopScene`) anlamadan değişiklik
  yapılmamalı.
- Aşama 7'nin host-authoritative multiplayer'a nasıl uyarlanacağı konusunda (tek yetkili durum,
  atomik işlemler deseni) kullanıcıyla kapsam netleştirilmeli.

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
  işaretli olduğundan `m_EnablePreviewPackages` 0'dan 1'e değiştirildi. Paketler **başarıyla
  çözümlendi ve derlendi** (kullanıcı test etti, doğrulandı) — ama "pre-release" statüsü uzun
  vadede Unity'nin sürümü değiştirme/kaldırma riskini hâlâ taşıyor, ileride izlenmeli.
- **(Multiplayer, ÇÖZÜLDÜ)** İlk kurulumda `NetworkManager.NetworkConfig.NetworkTransport`
  script ile hiç atanmamıştı ("No transport has been selected!" hatası, Host/Client
  başlatılamıyordu). `Shiftworks/Setup/17. Fix Multiplayer NetworkManager Transport` menüsüyle
  düzeltildi; kaynak Editor aracı da (`ShiftworksMultiplayerPrototypeSetup.cs`) gelecekteki
  taze kurulumlar için düzeltildi. Kullanıcı tarafından düzeltme sonrası test başarılı oldu.
- **(Multiplayer)** İki oyuncu spawn olduğunda şu an aynı noktada (prefabın varsayılan
  konumu) belirir — ayrı spawn noktaları veya çakışmayı önleyen bir mekanizma eklenmedi (kasıtlı
  olarak basit tutuldu); oyuncular birbirinin içinde başlayabilir, hareket ettiklerinde
  `CharacterController` çarpışmasıyla ayrışmaları beklenir. Kullanıcı testinde engelleyici
  olmadı ama bilinen bir kozmetik sınırlama.
- **Git durumu**: Aşama 1'den bu yana yapılan **hiçbir** kod/sahne/prefab/paket değişikliği
  commit edilmedi. Depoda hâlâ sadece 3 commit var (`02a5117`, `e2f44b4`, `c182130` — en
  sonuncusu yalnızca `CLAUDE.md`'nin ilk halini içeriyor). `git status`, Aşama 2'den bu yana
  üretilen tüm scriptleri, sahneleri, prefab'ları ve şimdi de NGO/UTP paket değişikliklerini
  hâlâ "untracked"/"modified" olarak gösteriyor. Bu, projenin en büyük riski — bir git
  sıfırlama/temizleme işlemi tüm ilerlemeyi (Aşama 1 hariç) silebilir.
