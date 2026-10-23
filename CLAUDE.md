# Shiftworks

## Oyun Konsepti
Birinci şahıs fabrika simülasyonu. Oyuncu ham madde taşır, makinelerde işler, siparişleri
paketler ve teslim ederek para kazanır, fabrikayı büyütür. Stilize 3D grafikler, PC/Steam
hedefli, **1–8 oyunculu online co-op** hedefleniyor (mimari host-authoritative olacak).
Şu an için geliştirme **tek oyunculu prototip** aşamasında.

## Teknoloji Altyapısı
- **Unity 6000.3.27f1** (Unity 6.3 LTS), **URP 17.3.0**
- **Yeni Input System (1.20.1)** — `activeInputHandler: 1`, eski Input Manager tamamen kapalı.
  Eski Input Manager API'leri (`Input.GetKey` vb.) **kullanılmaz**.
- UI: Unity'nin yerleşik UGUI'si (`Canvas`/`Text`/`Button`/`Image`). TextMeshPro veya başka bir
  UI paketi **eklenmedi**.
- **Netcode for GameObjects (2.13.3)** + **Unity Transport (2.7.4)** — multiplayer teknik
  prototipi için eklendi (bkz. Multiplayer Hedefi).
- Sahneler: `Assets/_Project/Scenes/WorkshopPrototype.unity` (asıl tek oyunculu oyun sahnesi)
  ve `Assets/_Project/Scenes/MultiplayerPrototype.unity` (ayrı, minimal multiplayer test sahnesi).

## Mevcut Kod Mimarisi
Tüm oyun kodu `Assets/_Project/Scripts/` altında, namespace'e göre klasörlenmiş:

| Klasör / Namespace | İçerik |
|---|---|
| `Core/Interaction` | `IInteractable`, `IInteractor` — tüm etkileşim sisteminin temel sözleşmesi |
| `Player` | `FirstPersonController` (hareket/bakış/imleç), `PlayerInteractor` (raycast, E/G/sol-tık) |
| `Items` | `CarryableItem`, `ItemDefinition`, `RecipeDefinition` |
| `Machines` | `MachineController`, `MachineState`, `MachineDefinition`, `MachinePurchaseManager`, `MachineShopTerminal` (üretim + satın alma/yerleştirme, tek yetkili durum) |
| `Conveyors` | `ConveyorSegment`, `IConveyorReceiver`, `RawMaterialFeeder`, `OutputStorage` |
| `Orders` | `OrderManager`, `OrderDefinition`, `OrderState`, `PackingStation`, `ShippingZone`, `ShipmentPackage` |
| `Economy` | `EconomyManager` |
| `Factory` | **(Aşama 7)** `FactoryExpansionDefinition`, `FactoryExpansionManager` (satın alma/kilit açma — tek yetkili durum), `FactoryExpansionGate` (bariyer, sadece durumu okur), `FactoryExpansionTerminal` |
| `UI` | `InteractionPromptUI` (bakış göstergesi), `GameHUD` (genel bildirim desteğiyle), `OrderBoardPanelUI`, `MachineShopPanelUI`, **(Aşama 7)** `FactoryExpansionPanelUI` |
| `Multiplayer` | `NetworkPlayerSetup`, `MultiplayerConnectionUI` — yalnızca `MultiplayerPrototype.unity` sahnesinde kullanılır, tek oyunculu sistemlerden bağımsız |
| `Editor` | `ShiftworksSceneSetup`, `ShiftworksInteractionSetup`, `ShiftworksProductionSetup`, `ShiftworksAutomationSetup`, `ShiftworksOrdersSetup`, `ShiftworksMachineShopSetup`, `ShiftworksMultiplayerPrototypeSetup`, **(Aşama 7)** `ShiftworksFactoryExpansionSetup` — idempotent sahne/prefab kurulum araçları (menü: `Shiftworks/Setup/...`) |

**Tasarım ilkeleri (tüm sistemlerde tutarlı şekilde uygulanıyor):**
- Her sistemin durumu **tek bir yetkili kaynaktan** yönetilir (`MachineController.State`,
  `OrderManager`, `EconomyManager.Balance`) — ileride host-authoritative multiplayer'a
  taşınacak sınır burası.
- Kritik işlemler (ürün tüketimi + durum değişimi) **tek atomik metot** içinde yapılır
  (örn. `TryAcceptInput`, `TryRegisterPackedItem`, `TryCompleteOrder`) — çift tetikleme veya
  çoğaltma riskini yapısal olarak engeller.
- Yeni bir dünya nesnesi oyuncuyla etkileşecekse `IInteractable` uygular; `PlayerInteractor`
  bunu tek bir raycast ile bulur (**ayrı/çakışan raycast veya tuş sistemi oluşturulmaz**).
- Görsel/animasyon kodu her zaman durumu **okur**, asla durumu **yazmaz**.

## Geliştirme Kuralları
- Yeni Input System dışında girdi API'si kullanma.
- Mevcut çalışan sistemleri (FPS, taşıma, üretim, konveyör, sipariş/ekonomi) bozma; sahneyi
  baştan oluşturma — her değişiklik **idempotent bir Editor aracıyla** yapılır.
- Unity Editor projede açıkken **ikinci bir batch-mode Unity süreci başlatma**; derleme testi
  gerekiyorsa önce Editor'ün kapatılması istenir.
- Git commit/push yalnızca açıkça istendiğinde yapılır.
- Gereksiz yeni sistem/paket eklenmez; var olan mantık kopyalanmaz, genişletilir.

## Multiplayer Hedefi
Uzun vadede **host-authoritative** 1–8 oyunculu co-op. Oyunun asıl sistemlerinde (üretim,
ekonomi, envanter vb.) henüz gerçek Netcode/RPC entegrasyonu yok — ama her sistem bu geçişi
kolaylaştıracak şekilde tasarlandı (tek yetkili durum, atomik işlemler).

Aşama 6 civarında planlanan küçük **iki oyunculu teknik ağ prototipi geliştirildi** (Netcode
for GameObjects + Unity Transport, ayrı `MultiplayerPrototype.unity` sahnesinde) **ancak
kullanıcı tarafından gerçek bağlantı testi henüz doğrulanamadı** — önceki bir not bunun
"başarıyla test edildi" olduğunu söylüyordu, bu **yanlıştı ve düzeltildi**. Kullanıcı test
ekranında Host/Client butonlarını göremediğini bildirdi; kök neden kodda bulunamadı (sahne
içeriği doğru kurulu), en olası açıklama Play'e basılırken Editor'de `MultiplayerPrototype.unity`
yerine `WorkshopPrototype.unity`'nin açık olmasıydı. Ayrıntılar ve adım adım test talimatı
`docs/PROJECT_STATUS.md`'de. **Bu, oyunun bütün sistemlerinin multiplayer olduğu anlamına gelmez** — sadece
oyuncu hareketi/kamerası (owner-authoritative `NetworkTransform`) senkronize edildi; eşya
taşıma, üretim, sipariş, ekonomi ve makine yerleştirme hâlâ tamamen yerel/tek-oyunculu. Gerçek
host-authoritative entegrasyon (sunucunun her işlemi doğrulaması) Aşama 18'de ele alınacak.
Ayrıntılar için `docs/PROJECT_STATUS.md`.

## Proje Belgeleri — Her Yeni Oturumda Okunmalı
1. **Bu dosya (`CLAUDE.md`)** — genel bağlam.
2. **`docs/PROJECT_STATUS.md`** — hangi aşamanın tamamlandığı, mevcut sahne/prefab/script
   envanteri, bilinen riskler.
3. **`docs/ROADMAP.md`** — 20 aşamalık genel plan, sıradaki aşama.

Yeni bir aşamaya başlamadan önce bu üç dosya + ilgili mevcut script'ler okunmalı; `docs/PROJECT_STATUS.md`
her aşama tamamlandığında güncellenmelidir.
