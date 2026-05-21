# Mobile Input Manager Setup Guide

## Gambaran Umum
Script `MobileInputManager` memungkinkan FirstPersonController Anda bekerja dengan input joystick pada platform mobile (Android/iOS). Ini menggunakan **DynamicJoystick** dari JoystickPack yang otomatis muncul saat touch dan hilang saat tidak ada touch.

## Komponen yang Diperlukan

### 1. Joystick Pack dari Assets
- Movement Joystick (kiri bawah) - untuk gerakan karakter
- Look Joystick (kanan bawah) - untuk menggerakkan camera

### 2. UI Buttons
- Jump Button (atas kanan)
- Sprint Button (tengah kanan)
- Crouch Button (bawah kanan)

## ⚠️ Penting: Coexistence dengan InputManagerOld

**MobileInputManager sekarang compatible dengan InputManagerOld!**

Bagaimana cara kerjanya:
- **Di Platform Mobile (Android/iOS)**: MobileInputManager aktif, InputManagerOld di-disable otomatis
- **Di Platform PC (Editor)**: MobileInputManager di-disable otomatis, InputManagerOld aktif (untuk testing dengan keyboard/mouse)

**Anda tidak perlu menghapus InputManagerOld** - script akan handle switching otomatis!

Setting `Auto Disable On PC` di MobileInputManager mengontrol behavior ini. Jika ingin disable-kan setting ini di runtime, gunakan:
```csharp
mobileInputManager.autoDisableOnPC = false;
```

---

## Step-by-Step Setup

### Step 1: Persiapan Canvas
1. Di Scene, pastikan Anda punya **Canvas** untuk UI
2. Set Canvas → Render Mode ke **Screen Space - Overlay** atau **Screen Space - Camera**

### Step 2: Buat Movement Joystick (DynamicJoystick)
1. Buka folder `/Assets/JoystickPack/Prefabs`
2. Drag prefab **DynamicJoystick** ke dalam Canvas
3. **Nama ulang** menjadi `MovementJoystick`
4. Posisikan di **kiri bawah layar**:
   - Position: X = -200, Y = 200 (atau sesuaikan dengan ukuran canvas)
   - Anchor Preset: Bottom-Left
5. Pastikan joystick ini **Interactable** dan bukan di-raycast oleh Layer Mask yang salah

### Step 3: Buat Look Joystick (DynamicJoystick)
1. Drag prefab **DynamicJoystick** ke dalam Canvas lagi
2. **Nama ulang** menjadi `LookJoystick`
3. Posisikan di **kanan bawah layar**:
   - Position: X = 200, Y = 200 (atau sesuaikan)
   - Anchor Preset: Bottom-Right
4. Pastikan ini juga **Interactable**

### Step 4: Buat UI Buttons
1. Create 3 UI Buttons (Button - TextMeshPro):
   - `JumpButton` 
   - `SprintButton`
   - `CrouchButton`
2. Posisikan di sisi kanan atas/tengah/bawah
3. Tambahkan **script JumpButton** ke JumpButton GameObject
4. Tambahkan **script SprintButton** ke SprintButton GameObject
5. Tambahkan **script CrouchButton** ke CrouchButton GameObject

### Step 5: Setup FirstPersonController GameObject
1. Buka GameObject yang memiliki **FirstPersonController** script
2. **JANGAN HAPUS InputManagerOld** - biarkan tetap ada! MobileInputManager akan auto-disable InputManagerOld pada platform mobile
3. Tambahkan **MobileInputManager** script sebagai component baru
4. Di Inspector, assign references:
   - **Movement Joystick** → Drag MovementJoystick GameObject
   - **Look Joystick** → Drag LookJoystick GameObject
   - **Jump Button** → Drag JumpButton GameObject
   - **Sprint Button** → Drag SprintButton GameObject
   - **Crouch Button** → Drag CrouchButton GameObject
5. Sesuaikan **Look Sensitivity Multiplier** (default 1f, increase untuk lebih sensitive)
6. Cek **Auto Disable On PC** (default true) - ini memungkinkan khidupan testing di PC dengan InputManagerOld
7. Pastikan **Enable Mobile Input** di-check

### Step 6: Testing
1. Build untuk Android/iOS, atau test di Play Mode dengan mengaktifkan touch simulation
2. Pastikan joystick muncul saat Anda menyentuh area mereka
3. Joystick otomatis hilang saat Anda melepas jari

## Customization

### Mengubah Input Sensitivity
Di **MobileInputManager**, edit:
```csharp
[SerializeField] private float lookSensitivityMultiplier = 1f;
```
Tambah nilai untuk lebih sensitive, kurang untuk less sensitive.

### Auto-Disable Behavior
Jika Anda ingin **selalu gunakan MobileInputManager** (bahkan di PC), uncheck:
```
Auto Disable On PC = false
```
Ini berguna jika Anda ingin test mobile input dengan joystick virtual di PC Editor.

### Mengubah Ukuran/Posisi Joystick
1. Select joystick di Hierarchy
2. Ubah **RectTransform** size dan position sesuai kebutuhan
3. Untuk DynamicJoystick, edit **Handle Range** dan **Move Threshold** di Inspector

### Disable Mobile Input untuk Testing PC
1. Di Scene, uncheck **Enable Mobile Input** di MobileInputManager
2. Gunakan InputManagerOld untuk mouse/keyboard input

## Troubleshooting

### Joystick tidak muncul
- Pastikan Canvas parent dari joystick memiliki **Graphic Raycaster**
- Pastikan Layer Mask pada joystick tidak diblokir oleh UI lain
- Cek bahwa joystick GetComponent `Image` tidak disabled

### Input tidak response
- Pastikan FirstPersonController menggunakan **GetComponent&lt;IInputManager&gt;()** 
- Pastikan MobileInputManager tidak di-disable (check checkbox di Inspector, atau cek script sendiri)
- Cek log untuk error message di MobileInputManager
- Pastikan button scripts (JumpButton, SprintButton, CrouchButton) tidak null
- Jika di PC Editor, ensure **Auto Disable On PC** diaktifkan sehingga InputManagerOld yang active

### Joystick stuck di layar
- Pastikan OnPointerUp() dipanggil dengan benar
- Check bahwa EventSystem aktif di Scene

## Catatan Penting
- **MobileInputManager dan InputManagerOld coexist!** Script akan otomatis switch based on platform
  - Mobile: MobileInputManager aktif
  - PC: InputManagerOld aktif (untuk testing dengan keyboard/mouse)
- Anda TIDAK perlu menghapus InputManagerOld dari GameObject
- Jika ingin force menggunakan MobileInputManager di PC (untuk test joystick virtual), uncheck "Auto Disable On PC"
- Joystick Direction sudah normalized antara -1 dan 1, jadi FirstPersonController akan menerima input yang konsisten
