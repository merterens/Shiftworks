# Shiftworks — Proje Durumu

_Son güncelleme: Aşama 5 tamamlandı (sipariş/ekonomi/paketleme/teslimat hata düzeltmeleri dahil, 9/9 test başarılı)._

## Aşama Durumu

| # | Aşama | Durum |
|---|---|---|
| 1 | FPS karakter ve atölye (WorkshopPrototype sahnesi, Player prefab) | ✅ Tamamlandı |
| 2 | Eşya alma, taşıma, bırakma, fırlatma (fizik tabanlı) | ✅ Tamamlandı |
| 3 | İlk üretim makinesi (kesim makinesi, ürün/reçete sistemi) | ✅ Tamamlandı |
| 4 | Konveyör bantları ve otomatik üretim hattı | ✅ Tamamlandı |
| 5 | Sipariş, ekonomi, paketleme ve teslimat sistemi | ✅ Tamamlandı |
| 6 | Makine satın alma ve yerleştirme | ⏭️ Sıradaki aşama (henüz başlanmadı) |

Kullanıcı tarafından onaylanan test sonuçları: Aşama 1 (7/7), Aşama 2 (4/4 + hız/kaybolma hata
düzeltmesi), Aşama 3 (11/11), Aşama 4 (11/11), Aşama 5 (ilk uygulama + 9/9 hata düzeltme testi
— HUD görünürlüğü ve teslimat raycast hatası dahil).

## Sahneler
- `Assets/_Project/Scenes/WorkshopPrototype.unity` — **tek oyun sahnesi**. 20×15 m atölye,
  giriş kapısı, test kutuları, kesim makinesi + manuel masa, otomatik üretim hattı
  (besleyici → 2 giriş konveyörü → makine → 1 çıkış konveyörü → depo), sipariş panosu,
  paketleme istasyonu, teslimat alanı, HUD ve sipariş paneli UI canvas'ları, `EventSystem`.

## Prefab'lar
- `Assets/_Project/Prefabs/Player/Player.prefab` — `CharacterController`, `FirstPersonController`,
  `PlayerInteractor`, kamera + `HoldPoint`, `InteractionPromptCanvas` (crosshair/bakış metni).
- `Assets/_Project/Prefabs/Items/MetalSheet.prefab`, `CutMetalPart.prefab`,
  `ShipmentPackage.prefab` — her biri `Rigidbody` + `Collider` + `CarryableItem`;
  `ShipmentPackage` ayrıca `ShipmentPackage` bileşenini (sipariş kimliği etiketi) taşır.

## ScriptableObject Veri Dosyaları
- `Assets/_Project/Data/Items/MetalSheet.asset`, `CutMetalPart.asset` — `ItemDefinition`.
- `Assets/_Project/Data/Recipes/CutMetalRecipe.asset` — `RecipeDefinition` (MetalSheet → CutMetalPart, 5 sn).
- `Assets/_Project/Data/Orders/FirstMetalPartOrder.asset` — `OrderDefinition` ("İlk Metal Parça
  Siparişi", 3× CutMetalPart, 150 $ ödül, süre sınırı yok).

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

## Bilinen Teknik Riskler
- **Git durumu**: Aşama 1–5'in TÜM kodu/sahnesi/varlıkları **henüz commit edilmedi**. Depoda
  sadece 2 commit var ("Initial Unity 6.3 project setup", "Update .gitignore"). `git status`
  hâlâ `Assets/_Project/` klasörünü tamamen "untracked" gösteriyor. Bu, en büyük risk — bir
  git sıfırlama/temizleme işlemi tüm ilerlemeyi silebilir.
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
