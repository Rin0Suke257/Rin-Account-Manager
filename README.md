# Rin Account Manager - Multi Roblox

App desktop Windows giup dang nhap nhieu acc Roblox (bang cookie) va mo nhieu client cung luc de treo/farm tay.

Khong hack, khong inject vao game, khong bot tu choi. Chi la launcher + quan ly acc.

## Chay

1. Mo `dist\RinAccountManager.exe` (2-click, khong can cai dat).
2. Lan dau mo: neu Roblox dang chay nen, app se hoi FIX de bat Multi. Chon YES.
3. Checkbox `Bat Multi-Roblox` phai ON khi mo tu 2 acc tro len.

Build lai tu source:

```
build.bat
```

Chi can .NET Framework 4.x co san tren Windows (dung `csc.exe`), khong can cai SDK.

## Them acc (3 cach, bam nut `Them acc` de chon)

**1. Dang nhap Roblox (de nhat):** form trinh duyet nhung trong app, dang nhap nhu web (captcha/2FA lam theo Roblox). Moi lan mo la 1 phien moi nen khong bi ket acc cu, app tu lay cookie khi xong. Acc trung (cung UserId) se bi chan, khong add 2 lan.

**2. Quick Login:** app hien ma 6 ky tu + QR. Tren dien thoai/may DA dang nhap acc can them: mo app Roblox (hoac web roblox.com) -> vao muc Quick Log In / Dang nhap nhanh -> quet QR hoac nhap ma -> bam Xac nhan. App tu doi vai giay roi tu them acc (dien ten goi nho truoc neu muon). Khong can pass, khong captcha. Ma het han sau ~10 phut hoac dong cua so (app tu huy ma).

**3. Paste cookie (thu cong):** paste `.ROBLOSECURITY` nhu truoc.
Cookie luu ma hoa DPAPI theo may (`%LocalAppData%\RinAccountManager\accounts.dat`).

CANH BAO: ai xin cookie / xin "rbx-player link" thi KHONG cho. Ho co the log vao acc, tieu Robux, lam ban acc.

## Play

1. Nhap `PlaceId` (hoac paste link game, app tu boc so).
2. `JobId` de trong = vao server random. Muon chung server thi dien cung JobId cho cac acc (JobId la ID cua 1 phong/server cu the). Link VIP thi paste thang vao (xem muc Game).
3. **Theo @username**: dien ten nguoi choi vao o `Theo @` de tat ca acc bay vao dung server nguoi do (can ket ban/duoc phep join + nguoi do dang trong game). Uu tien hon PlaceId/JobId/VIP.
3. Tick chon acc -> `Play acc da tick`, hoac `Play TAT CA acc`.
4. Cho delay 2-5s giua cac acc de tranh rate-limit/crash.

## Game: luu + goi nhanh + VIP

- Bam nut `v` canh o PlaceId de mo bang game: the game co **hinh + ten**, bam `Chon` la dien vao o Play.
- `+ Luu game dang nhap`: luu game theo ID/link dang nhap (tu tai ten + hinh).
- Moi game play xong tu vao muc `Gan day`.
- **VIP server:** paste link VIP (`...?privateServerLinkCode=...`) vao o JobId (hoac PlaceId), app tu lay accessCode va join bang `RequestPrivateGame`. Game VIP luu bang the `[VIP]`, chon la dien ca link.

## Treo acc (trong Cai dat, mac dinh TAT het)

- `Tu vao lai khi kick`: app theo doi tung client, tat/rot la tu launch lai dung server cu (toi da 5 lan, gian cach 15s chong rate-limit). Tat app/Kill all thi thoi.
- `Nhay chong-kick moi [N] phut`: moi vai phut dua 1 cua so game len ~1 giay, bam Space, tra focus. Dang full-screen/go do co the bi gian doan; game gat co the ban — tu chiu.

## Web acc

- Chon 1 acc -> bam `Web acc`: mo cua so web Roblox **dang nhap san acc do** (Back/Forward/Reload + o dia chi, nut `~>` mo link ra Chrome that). Thao tac gi cung tinh len acc that.

## Cai dat (nut banh rang goc tren)

- **Webhook Discord:** nhap URL + `Gui thu`. Chon su kien gui: vao game OK / launch loi / tu vao lai / cookie die.
- Tin vao game: `[Alias] (ghi chu) da vao [ten game] (placeId) luc [gio]` + anh chup client (doi N giay cho load, mac dinh 45). Co toggle hien Alias hay username.
- Thong so: so lan tu vao lai toi da, gian cach giua lan. Luu `settings.json` (dung share file co webhook).

## Cua so game (cum 4 nut)

- `Xep cua so`: dan client thanh luoi deu man hinh.
- `Thu gon` / `Hien het`: minimize / restore toan bo client 1 phat.
- `Dong client do`: kill client treo (Not responding), khoi mo Task Manager.

## Quan ly acc

- Cot `TT`: `ok` = live, `die` = cookie chet (them lai), `?` = chua check. Bam `Check live` se cap nhat + popup tong ket.
- Cot `Note`: double-click vao acc de sua ten goi nho + ghi chu (vd: farm map nao).
- Bam `Play`: app tu loc acc DIE truoc (bao ro ly do), chi play acc LIVE, xong mo popup ket qua tung acc. Loi launch nao cung hien day du, khong con fail tham.

## Gioi han da biet

- Roblox dang chu dong chong farm multi-instance: co the tu dong dong bot cua so sau mot luc, teleport loi 773. Day la phia Roblox, app khong bypass duoc 100%.
- Moi game co luat ve alt-acc. Bi ban alt la do game/Roblox, khong phai do app.
- Cookie die thi phai add lai. Dung acc chinh can thi bat 2FA, han che share may.

## Ky thuat

- Multi: giu `Mutex("ROBLOX_singletonEvent")` + `Mutex("ROBLOX_singletonMutex")` giong MultiBloxy (MIT) de chan Roblox tao singleton Event. Dam bao doc code `src/MultiRoblox.cs`, `src/HandleCloser.cs`.
- Launch: doi cookie -> CSRF token -> `POST auth.roblox.com/v1/authentication-ticket` -> ticket -> mo URL `roblox-player:1+launchmode:play+gameinfo:...` (cach lam cua RAM by ic3w0lf22).
