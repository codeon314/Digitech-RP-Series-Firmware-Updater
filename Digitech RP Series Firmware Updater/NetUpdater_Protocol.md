# Harman NetUpdater 1.0.1 — Firmware Update System (Reverse-Engineered)

Target binary: `NetUpdater1.0.1.exe` (PE32, built May 20 2009, JUCE C++ framework, IDA dump `IDA_Dump.asm`).
Purpose of this document: a complete, implementation-ready description of how NetUpdater
discovers, identifies and updates DigiTech RP-series / Lexicon products over USB-MIDI,
so the system can be reproduced locally without DigiTech/Harman servers.

---

## 1. Overview

The application is a 10-page wizard:

1. Welcome ("Welcome to NetUpdater")
2. Scan ("Checking for available firmware updates" / "Downloading Update Information...")
3. Select device ("Select a device to be updated")
4. Version filtering (2 radio checkboxes)
5. Select revision ("Updates are Available")
6. Confirm ("Are you sure you want to update the device: ...")
7. Prepare to update (shows `SpecialPreInstructions`)
8. Download progress
9. Update (flash) progress
10. Result ("Update is complete" / "Update failed" + `SpecialPostInstructions`)

Flow: scan → fetch/parse UpdateTargets.xml → enumerate MIDI ports → match devices →
identify each device (sysex) → compare firmware version against XML revisions →
user picks revision → confirm → download image → flash via sysex → result.

---

## 2. UpdateTargets.xml

### 2.1 Sources (tried in order)

1. `<exeDir>\UpdateTargets.xml` (local file, read-only fallback)
2. `http://www.lexiconpro.com/updater/common/config/UpdateTargets.xml?nocache=<urlencoded current time "h:mm:ss am/pm">`

The `?nocache=` value is the local 12-hour time-of-day string, percent-encoded
(e.g. `?nocache=2%3A04%3A53+pm`). It is a cache buster only.

Download uses WinInet:
- User-Agent: `juce`
- Accept: `*/*`
- Extra header: `Cache-Control: No-cache`
- Flags: `INTERNET_FLAG_RELOAD | INTERNET_FLAG_NO_CACHE_WRITE` (0x84000000), + `INTERNET_FLAG_SECURE` for https
- Connect timeout: 30 s
- Response beginning with `<!DOCTYPE HTML` is treated as an error page and rejected.

### 2.2 XML structure and parsing rules

```
<Config>
  <Brand Name="...">                 → BrandID: "Lexicon"→1, "DigiTech"→2, else 0
    <Family Name="..." FamilyID="0x5E">   → FamilyID parsed as hex (digits accumulated, "0x" ignored; default 0x7F)
      <Product Name="..." ProductID="0x1" [UpdateProductID="0x2"] [SpecialPreInstructions="..."]>
        <Ports>
          <Port Name="DigiTech RP255" [Count="1"]/>
        </Ports>
        <Revisions>
          <Revision Version="1.4" ReleaseDate="..." ImageUrl="http://..." [ReleaseNotesUrl] [SpecialPostInstructions]/>
        </Revisions>
      </Product>
    </Family>
  </Brand>
</Config>
```

- `ProductID` default `0x7F` (hex). `UpdateProductID` defaults to `ProductID`.
- `Port.Count` is decimal, default 1 = how many matching MIDI devices may be claimed per port name.
- All attributes are read by exact (case-sensitive) name match.
- Revision entries are kept in document order. No sorting, no extra version fields are read
  (`WinImageUrl`, `MacImageUrl`, `Protocol` are ignored by this binary version).
- Defaults for optional attributes are `"Unspecified"` (Name/Version/ReleaseDate) or an empty string.

---

## 3. Device discovery (scan)

1. Enumerate MIDI input devices and MIDI output devices (`midiInGetNumDevs`/`midiInGetDevCapsW`,
   `midiOutGetNumDevs`/`midiOutGetDevCapsW`).
2. For every `<Product>` in the parsed XML, for every `<Port>` name of that product
   (up to `Count` matches), find a MIDI-in device whose name **contains** the port name
   and a MIDI-out device whose name **contains** the port name. The match is a substring
   search (`wcsstr(deviceName, portName)`, case-sensitive in the original — sub_488F60/
   sub_488CD0/sub_466A30), so names like `"2- DigiTech RP500"` or `"DigiTech RP500 MIDI"`
   still match the XML port name `"DigiTech RP500"`.
3. For each matching in/out pair, construct a device candidate (FamilyID, ProductID,
   UpdateProductID, Brand) and run **identification** (§4). Only devices that answer
   the identity query are listed.

---

## 4. Sysex envelope (all traffic)

```
F0 <header> <raw0> <raw1> <raw2> <CMD> [7-bit-packed payload] <CHK> <F7>
```

- **Header**: 2 bytes (`F0 <id>`) or 4 bytes (`F0 00 <id>>8> <id&FF>`):
  - Lexicon (Brand 1): message ID = `0x0006` → `F0 06`
  - DigiTech (Brand 2): message ID = `0x00010010` → `F0 00 00 10`
- **4 raw bytes** (never packed):
  1. `raw0` = session/status byte — `0x7F` initially; after identification it is the
     byte `d0` echoed by the device and used in all subsequent messages of the session.
  2. `raw1` = **FamilyID** (from XML)
  3. `raw2` = **ProductID** (XML) — or **UpdateProductID** when the device is in update mode
  4. `CMD` = command byte
- **7-bit packing**: payload data is split into groups of 7 bytes; each group is preceded
  by one header byte whose bits 6..0 carry the bit-7s of the 7 data bytes (bit 6 ↔ first
  data byte, bit 0 ↔ seventh). Data bytes are sent with bit 7 cleared.
- **CHK** (checksum): `XOR` of every byte after `F0` and before `CHK`, masked to 7 bits.
  Equivalently: the XOR of the *entire* message (`F0` … `F7`) equals `0x07`.
  (All intermediate bytes are ≤ 0x7F by construction, so this is unambiguous.)
- **F7** ends the message. Receive side: parse raw region (headerLen+4 bytes), then unpack
  7-bit groups; `F7` terminates; a message whose whole-message XOR ≠ 0x07 is rejected
  (SysexException).

All multi-byte fields inside packed payloads are **big-endian**.

---

## 5. Transaction semantics

Send message → wait for response on the MIDI input (per-device event):

- Response command == expected command, ≠ `0x7E`: OK, return decoded payload to caller.
- Response command == expected == `0x7E`: plain ACK, message discarded.
- Response command == `0x7F`: **NAK**; the next two payload bytes are the failed
  command (byte 0, echo of the command being NAKed) and the error code (byte 1) →
  `CommTargetException(code0, code1)`.
  Observed on a real RP500: a rejected `'z'` write returns `7A 0C` — byte 0 `0x7A`
  is the write command, byte 1 `0x0C` is the actual error code.
- Timeout (no response): `CommHostException(0)`.
- Response with wrong command: `CommHostException(2)`.

Timeouts: identity 500 ms; version/'{'/'x' 1000 ms; 'q'/'z' 8000 ms.

---

## 6. Identification (per device candidate)

Open MIDI in (callback-based, 32 × 64-byte buffers) and MIDI out.

### 6.1 Identity query — command `1` → expected `2` (500 ms)

```
F0 [00 00 10 | 06] 7F <FamID> <ProdID> 01 | 00 7F 7F 7F | CHK F7     (DigiTech 14 bytes / Lexicon 12 bytes)
```

(the trailing payload is three 0x7F bytes, 7-bit packed as `00 7F 7F 7F`)

Response payload (decoded, after the 4 raw bytes):
```
b0 → session/status byte (stored and echoed back as raw0 in later messages)
b1, b2 → read and discarded
b3 → update-mode flag (nonzero = device already in update/bootloader mode)
if payload length > 5:
    b4 → read and discarded
    string1 → read and discarded (NUL-terminated)
    string2 → displayed device ID string (e.g. "00 0F 14")
```

**Retry**: if the query times out and `UpdateProductID != ProductID`, resend once with
the **FamilyID byte replaced by UpdateProductID**.

### 6.2 Version query — command `8` → expected `9` (1000 ms) (only when NOT in update mode)

```
F0 [00 00 10 | 06] 7F <FamID> <ProdID> 08 | CHK F7
```

Response payload: `b0`, `b1`.
- minor = `(b1>>4)*10 + (b1 & 0xF)`  (BCD)
- if `b0 == 0`: major = `b1>>4`, minor = `(b1&0xF)*10`
- else major = b0
- Display string: if major ≤ 9: `major + minor/10` formatted `%.*f` with
  1 decimal if minor%10==0 else 2 decimals (e.g. "1.4", "1.03"); else `%X%d` (hex major, dec minor).

The **current firmware version** used for comparisons is the string produced by 6.2.
The ID string from 6.1 is only displayed.

### 6.3 Version comparison (newer-than)

For two version strings: parse with `sscanf("%d.%d")` (first two dot-separated integers);
if exactly one field matched, retry with `sscanf("%X%d")` (hex major + dec minor).
Compare major, then minor, numerically. `revision > current` ⇒ "newer".

"Updates available" = any revision strictly newer than the device's current version
(only the newest revision when the "only latest version newer than mine" checkbox is on).

---

## 7. Firmware image format (.DigBin)

```
offset 0: magic dword 0x05005C00  (bytes on disk: 00 5C 00 05, little-endian)
offset 4: repeated records until EOF:
    uint32 flashAddress (BIG-ENDIAN)
    uint32 size (BIG-ENDIAN)
    <size bytes of data>
```

- Field endianness confirmed against the real `rp500V20.DigBin`: 16 records of
  `{addr, size = 0x10000}`, addresses 0x00000000, 0x00010000 ... 0x000F0000,
  total 1,048,576 bytes of data; the record list ends exactly at EOF.
  (An earlier little-endian reading of the headers is what the file format is NOT.)
- No encryption, no CRC. Image is validated only by the 4-byte magic before flashing.
- `BinFileException("Unexpected end of file encoutered")` if the file ends mid-record.
- The record list maps flash addresses to data; blocks are read per record,
  capped at the device-reported maximum block size (§8).

---

## 8. Update session (performUpdate)

Sequence (each message uses the current session byte as raw0, FamilyID as raw1,
ProductID/UpdateProductID as raw2):

1. Open MIDI.
2. If device is NOT already in update mode: **'q'** `cmd=0x71`, expect `0x7E`, 8000 ms
   (are-you-there). On `CommHostException` the device is marked as in update mode.
3. Special case DigiTech `FamilyID=0x5C, ProductID=4` (GNX3000): close + reopen MIDI
   (device re-enumerates after '{').
4. **'{'** `cmd=0x7B` + payload byte `0x00` (packed), expect `0x7E`, 1000 ms
   (enter update mode).
5. **'x'** `cmd=0x78`, expect `0x79`, 1000 ms — flash map query. Response payload:
   ```
   b0           → read and discarded
   b1 b2        → BE16 maximum block size (bytes per 'z' message)
   b3           → segment count N
   then N × { dword BE addr, dword BE size }   (segment table)
   ```
6. For each segment `{addr, size}` (in order):
   - **Erase** (Lexicon MX200 only: Brand=1, FamilyID=0x1A, ProductID=1, and only when
     `addr == 0`): for each of `0x8000, 0x6000, 0x4000, 0x0000`:
     `'{'` + `0x00` (1000 ms), then `'z'` writing a single `0xFF` byte at that address (8000 ms).
   - For `a = addr … addr+size-1`:
     - Read next block from the .DigBin: data for flash address `a`, up to
       `min(maxBlockSize, remaining bytes in current record)`. Skip (advance past) any
       gaps not covered by a record.
     - If the block is all `0xFF` **and** `size > addr`, do not send it (still counts
       progress). Otherwise send it.
     - **'z'** `cmd=0x7A` + payload: `BE32 a, BE16 blockLen, <7-bit packed data>`,
       expect `0x7E`, 8000 ms.
      - NAK whose second payload byte (the error code) is `0x0C` → resend same block,
        up to 3 attempts total. Any other NAK/error aborts ("Update failed").
        (The NAK payload is `[failed command, error code]`; byte 0 is the command echo,
        so the retry condition is on byte 1, i.e. `CommTargetException` member +0x10,
        per the catch handler at `sub_4033C0`/`loc_40385C`.)
   - Progress: bytes written / total data bytes (total = Σ over segments of the
     .DigBin data overlapping `[addr, addr+size)`).
7. Close MIDI. "Update is complete". (No explicit reset message — the device resets
   itself, except GNX3000 which re-enumerates USB-MIDI after '{'.)

Message bytes (DigiTech):
- `'q'`: `F0 00 00 10 <ss> <fam> <prod> 71 CHK F7`
- `'{'`: `F0 00 00 10 <ss> <fam> <prod> 7B 00 00 CHK F7`   (packed `00` payload)
- `'x'`: `F0 00 00 10 <ss> <fam> <prod> 78 CHK F7`
- `'z'`: `F0 00 00 10 <ss> <fam> <prod> 7A <a31..a0> <len15..len0> <packed data> CHK F7`
- Lexicon identical with header `F0 06`.

---

## 9. UI logic details (mimic-worthy behavior)

- Device list entry formatting:
  - busy/waiting: `Name " <ID>" (waiting for update)` / `Name (waiting for update)`
  - idle: `Name " <ID>" (at version <v>)` / `Name (at version <v>)`
- Confirm text: `Are you sure you want to update the device: <formatted device> to firmware version <Version>?`
  plus a checkbox "Yes, I realize my settings may be lost." gating Next.
- Result page: on success show `SpecialPostInstructions`; on failure hide Quit.
- Version filter checkboxes:
  - "Only show the latest version newer than mine (recommended)" → list = single newest strictly-newer revision.
  - "Show all available versions" → list = all revisions (any version, including older ones, can be selected and flashed).
  - If no revision is newer than the device: "No Updates Were Found / …firmware is already up to date…".
- Download errors: "A server connection could not be established" / "Update Cancelled".
- Bad image header → treated as download failure.

---

## 11. Safety model & end-to-end verification procedure

### Why the protocol is safe by construction

- The host never chooses flash addresses. The device's bootloader answers `'x'` with a
  segment table (address/size pairs) and a maximum block size; the host only writes
  inside what the device declares. The .DigBin image is merely a map of data to those
  addresses.
- Every block write (`'z'`) is individually ACKed (`0x7E`) or NAKed (`0x7F` + 2-byte
  payload `[failed command, error code]`). A malformed block (wrong address/size/checksum)
  is rejected per-block by the device, never silently misapplied. Transient `0x0C` NAKs
  (error code in the second payload byte) are retried 3x, anything else aborts the session.
- All-0xFF blocks are never transmitted (empty flash regions stay untouched).
- `'q'`/`'{'`/`'x'` are non-destructive handshakes; only `'z'` writes. A device left in
  update mode returns to normal firmware after a power-cycle.
- Re-flashing the same version (or an older one) is supported: the bootloader performs
  no version check, and the original NetUpdater allows it via "Show all available versions".

The brick risk is therefore equivalent to the official tool: the only dangerous event is
losing power or USB mid-write. If that happens, do NOT power-cycle repeatedly — reconnect
and re-run an update (the bootloader normally remains reachable).

### End-to-end verification steps (zero-risk first)

1. **Scan (read-only).** Power on the device, plug USB, run the app, Next.
   Detection proves MIDI plumbing + sysex codec + identity query (cmd 1→2) + version
   query (cmd 8→9) with zero writes. Confirm the device list shows
   e.g. `RP500 "xx xx xx" (at version 2.0)`.
2. **Download (read-only).** Select the device, "Show all available versions", pick
   the installed version, Next, confirm. The image downloads and its magic/records are
   validated. Check the update page summary (record count, byte count).
3. **Dry run (no writes).** Tick "SAFE TEST MODE" on the confirm page and proceed.
   The app performs `'q'`, `'{'`, `'x'`, parses the device's flash map and logs it,
   then stops. Power-cycle the device afterwards. This verifies the entire write-path
   framing except the actual `'z'` blocks.
4. **First real write: same version over itself.** Untick safe mode, confirm, flash.
   Watch the "Writing at address: 0x..." log. On success the device resets itself
   ("Please wait for your RP500 to reset, then complete the calibration procedure.").
   A mid-session failure leaves the device in update mode: re-run the updater.
5. Inspect `NetUpdaterClone.log` (next to the exe) — every sysex frame is hex-dumped
   with timestamps for verification against §4–§8.

---

## 12. Reference: addresses in IDA_Dump.asm

| Function (IDA) | Role |
|---|---|
| `sub_408740` | Check-for-updates driver (URL build + thread orchestration) |
| `sub_407030` / `sub_4070F0` / `sub_407AD0` / `sub_406C30` | Targets download thread / runner / fetch / generic downloader |
| `sub_4072C0` | UpdateTargets.xml walker |
| `sub_402820` | MIDI port enumeration + XML product matching |
| `sub_402AE0` / `sub_402D10` | MidiTargetDevice ctor / identify() |
| `sub_402DE0` / `sub_402FF0` | identity query / version query |
| `sub_403320` / `sub_4033C0` / `sub_4039B0` | 'q' ping / performUpdate / transact |
| `sub_403D10` / `sub_403CC0` / `sub_403C00` | MX200 erase / GNX3000 reopen / MX200 drain |
| `sub_402540` / `sub_4021A0` / `sub_402270` | .DigBin parse / block read / coverage count |
| `sub_403170` | version compare (sscanf %d.%d then %X%d) |
| `sub_40A750` / `sub_40A960` | Super7TxSysexMsg / TxSysexMsg ctors (header+raw bytes) |
| `sub_40A840` / `sub_40A8F0` / `sub_40A920` | 7-bit pack append / finish (chk+F7) / checksum |
| `sub_40A500` / `sub_40A4B0` | Super7RxSysexMsg parse / ctor |
| `sub_40A300` / `sub_40A320` / `sub_40A340` | payload readByte / readWord(BE) / readDword(BE) |
| `sub_496B00` | midiOut send (PrepareHeader/LongMsg/UnprepareHeader) |
| `sub_497010` / `sub_496E80` | midiIn open (CALLBACK_FUNCTION) / midiOut open |
| `sub_407EB0` / `sub_407F40` | DownloadImageThread ctor / run (magic check 0x05005C00) |
| `sub_407BF0` / `sub_408000` | UpdateThread run / ctor |
| `sub_406A10` / `sub_406A80` | find newest revision / device display string |
| `sub_45A180` | WinInet createConnection ("juce" UA, no-cache) |
