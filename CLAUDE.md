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
- Tek sahne: `Assets/_Project/Scenes/WorkshopPrototype.unity`.

## Mevcut Kod Mimarisi
Tüm oyun kodu `Assets/_Project/Scripts/` altında, namespace'e göre klasörlenmiş:

| Klasör / Namespace | İçerik |
|---|---|
| `Core/Interaction` | `IInteractable`, `IInteractor` — tüm etkileşim sisteminin temel sözleşmesi |
| `Player` | `FirstPersonController` (hareket/bakış/imleç), `PlayerInteractor` (raycast, E/G/sol-tık) |
| `Items` | `CarryableItem`, `ItemDefinition`, `RecipeDefinition` |
| `Machines` | `MachineController`, `MachineState` (kesim makinesi, tek yetkili üretim durumu) |
| `Conveyors` | `ConveyorSegment`, `IConveyorReceiver`, `RawMaterialFeeder`, `OutputStorage` |
| `Orders` | `OrderManager`, `OrderDefinition`, `OrderState`, `PackingStation`, `ShippingZone`, `ShipmentPackage` |
| `Economy` | `EconomyManager` |
| `UI` | `InteractionPromptUI` (bakış göstergesi), `GameHUD`, `OrderBoardPanelUI` |
| `Editor` | `ShiftworksSceneSetup`, `ShiftworksInteractionSetup`, `ShiftworksProductionSetup`, `ShiftworksAutomationSetup`, `ShiftworksOrdersSetup` — idempotent sahne/prefab kurulum araçları (menü: `Shiftworks/Setup/...`) |

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
Uzun vadede **host-authoritative** 1–8 oyunculu co-op. Henüz Netcode/RPC kodu yok — ama her
sistem bu geçişi kolaylaştıracak şekilde tasarlandı (tek yetkili durum, atomik işlemler).
Aşama 6 civarında küçük bir **iki oyunculu teknik ağ prototipi** planlanıyor (bkz.
`docs/ROADMAP.md`), böylece ağ mimarisi çok geç doğrulanmış olmaz.

## Proje Belgeleri — Her Yeni Oturumda Okunmalı
1. **Bu dosya (`CLAUDE.md`)** — genel bağlam.
2. **`docs/PROJECT_STATUS.md`** — hangi aşamanın tamamlandığı, mevcut sahne/prefab/script
   envanteri, bilinen riskler.
3. **`docs/ROADMAP.md`** — 20 aşamalık genel plan, sıradaki aşama.

Yeni bir aşamaya başlamadan önce bu üç dosya + ilgili mevcut script'ler okunmalı; `docs/PROJECT_STATUS.md`
her aşama tamamlandığında güncellenmelidir.
