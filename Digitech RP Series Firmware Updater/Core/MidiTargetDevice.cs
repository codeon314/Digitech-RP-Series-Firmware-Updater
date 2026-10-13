using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetUpdaterClone.Core;

public class CommException : Exception
{
    public CommException(string message) : base(message) { }
}

public class CommHostException : CommException
{
    public int Code;
    public CommHostException(string message, int code) : base(message) { Code = code; }
}

public class CommTargetException : CommException
{
    public int ErrorCode0;
    public int ErrorCode1;
    public CommTargetException(int code0, int code1)
        : base($"Device error 0x{code0:X2} (detail 0x{code1:X2})")
    {
        ErrorCode0 = code0;
        ErrorCode1 = code1;
    }
}

/// <summary>WinMM MIDI session for one device candidate. Mirrors hmg::MidiTargetDevice.</summary>
public sealed class MidiTargetDevice : IDisposable
{
    public const int IdentityTimeoutMs = 500;
    public const int VersionTimeoutMs = 1000;
    public const int QuickTimeoutMs = 8000;
    public const int ShortTimeoutMs = 1000;

    public ProductInfo Product;
    public int InPortIndex = -1;
    public int OutPortIndex = -1;

    public string DisplayId = "";
    public string CurrentVersion = "";
    public bool InUpdateMode;

    private byte _sessionByte = 0x7F;   // raw0; echoed from identity response d0
    private IntPtr _hMidiIn;
    private IntPtr _hMidiOut;
    private MidiInProc _inProc;
    private GCHandle _inProcHandle;
    private readonly object _rxLock = new();
    private byte[] _rxData;
    private readonly AutoResetEvent _rxEvent = new(false);
    private const int RxBufferSize = 4096;
    private byte[] _rxBuffer;
    private GCHandle _rxBufferHandle;
    private IntPtr _rxHdrPtr;
    private volatile bool _closing;
    private bool _disposed;

    public MidiTargetDevice(ProductInfo product, int inPort, int outPort)
    {
        Product = product;
        InPortIndex = inPort;
        OutPortIndex = outPort;
    }

    // ---- MIDI plumbing -------------------------------------------------------

    public void OpenMidi()
    {
        CloseMidi();
        _closing = false;

        UpdateLog.Write($"OpenMidi: in={InPortIndex} out={OutPortIndex} product={Product.Name} " +
                        $"family=0x{Product.FamilyId:X2} productId=0x{Product.ProductId:X2} " +
                        $"updateProductId=0x{Product.UpdateProductId:X2} brand={Product.BrandId}");

        _inProc = OnMidiInMessage;
        _inProcHandle = GCHandle.Alloc(_inProc);

        try
        {
            var err = WinMidi.midiInOpen(out _hMidiIn, (uint)InPortIndex, _inProc,
                GCHandle.ToIntPtr(_inProcHandle), WinMidi.CALLBACK_FUNCTION);
            if (err != 0)
                throw new CommHostException($"midiInOpen failed: {WinMidi.MidiError(err)}", 0);

            _rxBuffer = new byte[RxBufferSize];
            _rxBufferHandle = GCHandle.Alloc(_rxBuffer, GCHandleType.Pinned);
            _rxHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MIDIHDR>());
            var hdr = new MIDIHDR
            {
                lpData = _rxBufferHandle.AddrOfPinnedObject(),
                dwBufferLength = (uint)_rxBuffer.Length,
            };
            hdr.Init();
            Marshal.StructureToPtr(hdr, _rxHdrPtr, false);
            if (WinMidi.midiInPrepareHeader(_hMidiIn, _rxHdrPtr, (uint)Marshal.SizeOf<MIDIHDR>()) != 0)
                throw new CommHostException("midiInPrepareHeader failed", 0);
            if (WinMidi.midiInAddBuffer(_hMidiIn, _rxHdrPtr, (uint)Marshal.SizeOf<MIDIHDR>()) != 0)
                throw new CommHostException("midiInAddBuffer failed", 0);
            if (WinMidi.midiInStart(_hMidiIn) != 0)
                throw new CommHostException("midiInStart failed", 0);

            err = WinMidi.midiOutOpen(out _hMidiOut, (uint)OutPortIndex, IntPtr.Zero, IntPtr.Zero,
                WinMidi.CALLBACK_NULL);
            if (err != 0)
                throw new CommHostException($"midiOutOpen failed: {WinMidi.MidiError(err)}", 0);
        }
        catch
        {
            CloseMidi();
            throw;
        }
    }

    public void CloseMidi()
    {
        var firstClose = !_closing;
        if (firstClose)
            UpdateLog.Write("CloseMidi begin");
        _closing = true;

        var hIn = _hMidiIn;
        _hMidiIn = IntPtr.Zero;
        if (hIn != IntPtr.Zero)
        {
            try { WinMidi.midiInReset(hIn); } catch (Exception ex) { UpdateLog.Write($"midiInReset error: {ex.Message}"); }
            try { WinMidi.midiInStop(hIn); } catch (Exception ex) { UpdateLog.Write($"midiInStop error: {ex.Message}"); }
            if (_rxHdrPtr != IntPtr.Zero)
            {
                try { WinMidi.midiInUnprepareHeader(hIn, _rxHdrPtr, (uint)Marshal.SizeOf<MIDIHDR>()); }
                catch (Exception ex) { UpdateLog.Write($"midiInUnprepareHeader error: {ex.Message}"); }
                try { Marshal.FreeHGlobal(_rxHdrPtr); } catch { }
                _rxHdrPtr = IntPtr.Zero;
            }
            try { WinMidi.midiInClose(hIn); } catch (Exception ex) { UpdateLog.Write($"midiInClose error: {ex.Message}"); }
        }

        if (_rxBufferHandle.IsAllocated)
        {
            try { _rxBufferHandle.Free(); } catch { }
        }
        _rxBuffer = null;
        if (_inProcHandle.IsAllocated)
        {
            try { _inProcHandle.Free(); } catch { }
        }

        var hOut = _hMidiOut;
        _hMidiOut = IntPtr.Zero;
        if (hOut != IntPtr.Zero)
        {
            try { WinMidi.midiOutClose(hOut); } catch (Exception ex) { UpdateLog.Write($"midiOutClose error: {ex.Message}"); }
        }

        lock (_rxLock)
            _rxData = null;
        if (firstClose)
            UpdateLog.Write("CloseMidi end");
    }

    private void OnMidiInMessage(IntPtr hMidiIn, uint msg, IntPtr instance, IntPtr param1, IntPtr param2)
    {
        // Native callback: must never throw, and must no-op once closing starts.
        try
        {
            if (_closing)
                return;
            if (msg == WinMidi.MIM_LONGDATA && param1 != IntPtr.Zero && _rxBuffer != null)
            {
                var dataPtr = Marshal.ReadIntPtr(param1,
                    (int)Marshal.OffsetOf<MIDIHDR>(nameof(MIDIHDR.lpData)));
                var len = Marshal.ReadInt32(param1,
                    (int)Marshal.OffsetOf<MIDIHDR>(nameof(MIDIHDR.dwBytesRecorded)));
                if (dataPtr != IntPtr.Zero && len > 0)
                {
                    var buf = new byte[len];
                    Marshal.Copy(dataPtr, buf, 0, len);
                    var preview = buf.Length <= 64 ? buf : buf.Take(64).ToArray();
                    UpdateLog.Write($"RX callback: {len} bytes {UpdateLog.Hex(preview)}{(buf.Length > 64 ? "..." : "")}");

                    if (len > 1)
                    {
                        lock (_rxLock)
                            _rxData = buf;
                        _rxEvent.Set();
                    }
                }
                // re-queue the completed header (param1 is the driver's MIDIHDR)
                if (!_closing && hMidiIn != IntPtr.Zero)
                    WinMidi.midiInAddBuffer(hMidiIn, param1, (uint)Marshal.SizeOf<MIDIHDR>());
            }
        }
        catch (Exception ex)
        {
            UpdateLog.Write($"RX callback error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void SendSysex(byte[] data)
    {
        UpdateLog.Write($"TX: {UpdateLog.Hex(data)}");
        var dataPtr = Marshal.AllocHGlobal(data.Length);
        var hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MIDIHDR>());
        var flagsOffset = (int)Marshal.OffsetOf<MIDIHDR>(nameof(MIDIHDR.dwFlags));
        try
        {
            Marshal.Copy(data, 0, dataPtr, data.Length);
            var hdr = new MIDIHDR
            {
                lpData = dataPtr,
                dwBufferLength = (uint)data.Length,
            };
            hdr.Init();
            Marshal.StructureToPtr(hdr, hdrPtr, false);
            if (WinMidi.midiOutPrepareHeader(_hMidiOut, hdrPtr, (uint)Marshal.SizeOf<MIDIHDR>()) != 0)
                throw new CommHostException("midiOutPrepareHeader failed", 0);
            try
            {
                if (WinMidi.midiOutLongMsg(_hMidiOut, hdrPtr, (uint)Marshal.SizeOf<MIDIHDR>()) != 0)
                    throw new CommHostException("midiOutLongMsg failed", 0);
                var sw = Stopwatch.StartNew();
                // Read dwFlags from the LIVE unmanaged header the driver updates.
                while ((Marshal.ReadInt32(hdrPtr, flagsOffset) & WinMidi.MHDR_DONE) == 0)
                {
                    if (sw.ElapsedMilliseconds > 10000)
                        throw new CommHostException("midiOut timeout waiting for MHDR_DONE", 0);
                    Thread.Sleep(1);
                }
            }
            finally
            {
                for (var attempt = 0; attempt < 500; attempt++)
                {
                    var r = WinMidi.midiOutUnprepareHeader(_hMidiOut, hdrPtr, (uint)Marshal.SizeOf<MIDIHDR>());
                    if (r == 0)
                        break;
                    if (r != WinMidi.MIDIERR_STILLPLAYING)
                        break;
                    Thread.Sleep(2);
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(hdrPtr);
            Marshal.FreeHGlobal(dataPtr);
        }
    }

    private byte ProductByte => (byte)(InUpdateMode ? Product.UpdateProductId : Product.ProductId);

    private byte[] BuildMessage(byte cmd, byte[] payload = null)
        => SysexCodec.Build(Product.BrandId, _sessionByte, (byte)Product.FamilyId, ProductByte, cmd, payload);

    /// <summary>Send + await response. Returns decoded payload or null for a plain ACK.
    /// Throws CommTargetException on NAK, CommHostException on timeout/wrong command.</summary>
    private DecodedMessage Transact(byte cmd, byte[] payload, byte expectedCmd, int timeoutMs)
    {
        lock (_rxLock)
            _rxData = null;

        SendSysex(BuildMessage(cmd, payload));

        if (!_rxEvent.WaitOne(timeoutMs))
        {
            UpdateLog.Write($"RX: TIMEOUT after {timeoutMs} ms (expected cmd 0x{expectedCmd:X2})");
            throw new CommHostException("Timeout waiting for device response", 0);
        }

        byte[] raw;
        lock (_rxLock)
            raw = _rxData;
        UpdateLog.Write($"RX: {UpdateLog.Hex(raw)}");

        DecodedMessage msg;
        try
        {
            msg = SysexCodec.Decode(raw);
        }
        catch (InvalidOperationException ex)
        {
            UpdateLog.Write($"RX decode failed: {ex.Message}");
            throw new CommHostException("Bad sysex response", 1);
        }

        if (msg.Cmd == expectedCmd)
        {
            if (msg.Cmd == 0x7E)
                return null; // plain ACK
            return msg;
        }
        if (msg.Cmd == 0x7F)
        {
            var e0 = msg.Remaining >= 1 ? msg.ReadByte() : 0;
            var e1 = msg.Remaining >= 1 ? msg.ReadByte() : 0;
            UpdateLog.Write($"RX: NAK 0x{e0:X2} (detail 0x{e1:X2})");
            throw new CommTargetException(e0, e1);
        }
        throw new CommHostException($"Unexpected response command 0x{msg.Cmd:X2}", 2);
    }

    // ---- Identification -------------------------------------------------------

    public bool Identify()
    {
        UpdateLog.Write($"Identify start: {Product.Name} (session byte 0x{_sessionByte:X2})");
        OpenMidi();
        try
        {
            IdentifyInner();
            UpdateLog.Write($"Identity OK: sessionByte=0x{_sessionByte:X2} " +
                            $"updateMode={InUpdateMode} id=\"{DisplayId}\"");
            if (!InUpdateMode)
            {
                QueryVersion();
                UpdateLog.Write($"Version OK: \"{CurrentVersion}\"");
            }
            return true;
        }
        finally
        {
            CloseMidi();
        }
    }

    private void IdentifyInner()
    {
        try
        {
            DoIdentityQuery();
        }
        catch (CommHostException ex) when (ex.Code == 0)
        {
            // UpdateProductID retry: resend with FamilyID byte replaced by UpdateProductID
            if (Product.UpdateProductId == Product.ProductId)
                throw;
            var header = SysexCodec.HeaderFor(Product.BrandId);
            var body = new List<byte> { 0xF0 };
            body.AddRange(header.Skip(1));
            body.Add(_sessionByte);
            body.Add((byte)Product.UpdateProductId);   // FamilyID slot patched
            body.Add((byte)Product.ProductId);
            body.Add(0x01);
            body.AddRange(SysexCodec.Pack(new byte[] { 0x7F, 0x7F, 0x7F }));
            var buf = body.ToArray();
            body.Add((byte)SysexCodec.Checksum(buf));
            body.Add(0xF7);

            var msg = TransactRaw(body.ToArray(), 0x02, IdentityTimeoutMs);
            ParseIdentityResponse(msg);
        }
    }

    private void DoIdentityQuery()
    {
        var msg = Transact(0x01, new byte[] { 0x7F, 0x7F, 0x7F }, 0x02, IdentityTimeoutMs);
        ParseIdentityResponse(msg);
    }

    private void ParseIdentityResponse(DecodedMessage msg)
    {
        // payload: b0 session byte, b1, b2 discarded, b3 update-mode flag
        _sessionByte = msg.ReadByte();
        msg.ReadByte();
        msg.ReadByte();
        var d3 = msg.ReadByte();
        InUpdateMode = d3 != 0;

        if (msg.Remaining > 1) // decoded payload length > 5
        {
            msg.ReadByte();                        // discarded
            var s1 = ReadString(msg);              // discarded
            var s2 = ReadString(msg);              // displayed ID
            DisplayId = s2;
        }
    }

    private void QueryVersion()
    {
        var msg = Transact(0x08, null, 0x09, VersionTimeoutMs);

        var b0 = msg.ReadByte();
        var b1 = msg.ReadByte();

        // BCD decode of b1 (sub_402FF0)
        var minor = (b1 >> 4) * 10 + (b1 & 0x0F);
        var major = (int)b0;
        if (b0 == 0)
        {
            major = b1 >> 4;
            minor = (b1 & 0x0F) * 10;
        }

        if (major <= 9)
        {
            // "%.*f" with 1 decimal if minor%10==0, else 2 decimals
            var decimals = minor % 10 == 0 ? 1 : 2;
            CurrentVersion = (major + minor / 10.0).ToString("F" + decimals,
                System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            // "%X%d": hex major, raw b1 as decimal
            CurrentVersion = $"{major:X}{b1}";
        }
    }

    private static string ReadString(DecodedMessage msg)
    {
        var bytes = new List<byte>();
        while (msg.Remaining > 0)
        {
            var b = msg.ReadByte();
            if (b == 0) break;
            bytes.Add(b);
        }
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }

    // ---- Update session -------------------------------------------------------

    /// <summary>
    /// Runs the full update session. With <paramref name="dryRun"/> set, performs the
    /// read-only handshake ('q', '{', 'x') and image validation, then stops before any
    /// 'z' write - the device stays on its current firmware (power-cycle to leave
    /// update mode).
    /// Returns false for a dry run, true when data was actually written.
    /// </summary>
    public bool PerformUpdate(byte[] imageData,
        Action<string> statusCallback = null,
        Action<double> progressCallback = null,
        CancellationToken ct = default,
        bool dryRun = false)
    {
        ct.ThrowIfCancellationRequested();

        // Validate and summarize the image BEFORE touching the device.
        var img = DigBinImage.Parse(imageData);
        var imageBytes = img.Records.Sum(r => (long)r.Size);
        UpdateLog.Write($"Image: {img.Records.Count} records, {imageBytes} data bytes" +
                        (dryRun ? " (DRY RUN - no data will be written)" : ""));

        OpenMidi();
        try
        {
            statusCallback?.Invoke("Initializing the device...");

            if (!InUpdateMode)
            {
                // 'q' are-you-there; on any host exception treat the device as already
                // in update mode (mirrors sub_403320).
                try
                {
                    Transact(0x71, null, 0x7E, QuickTimeoutMs);
                }
                catch (CommHostException)
                {
                    InUpdateMode = true;
                }
            }

            ct.ThrowIfCancellationRequested();

            // GNX3000 special case: device re-enumerates USB-MIDI after '{'
            if (Product.BrandId == 2 && Product.FamilyId == 0x5C && Product.ProductId == 4)
            {
                CloseMidi();
                OpenMidi();
            }

            // '{' enter update mode
            Transact(0x7B, new byte[] { 0x00 }, 0x7E, ShortTimeoutMs);

            // 'x' flash map
            var map = Transact(0x78, null, 0x79, ShortTimeoutMs);
            map.ReadByte();                        // consumed and unused
            var maxBlock = map.ReadWordBE();
            var segmentCount = map.ReadByte();
            var segments = new List<(uint Addr, uint Size)>();
            for (var i = 0; i < segmentCount; i++)
            {
                var a = (uint)map.ReadDwordBE();
                var s = (uint)map.ReadDwordBE();
                segments.Add((a, s));
            }
            UpdateLog.Write($"Flash map: maxBlock={maxBlock}, segments={segmentCount}");
            foreach (var (a, s) in segments)
                UpdateLog.Write($"  segment addr=0x{a:X8} size=0x{s:X8} ({s} bytes)");

            long totalBytes = 0;
            foreach (var (addr, size) in segments)
                totalBytes += img.Coverage(addr, size);
            UpdateLog.Write($"Data to write: {totalBytes} bytes across segments" +
                            $" (image bytes: {imageBytes})");

            if (dryRun)
            {
                statusCallback?.Invoke(
                    "DRY RUN OK: handshake and flash map verified, no data written. " +
                    "Power-cycle the device to return to normal operation.");
                UpdateLog.Write("Dry run complete - aborting before any write.");
                return false;
            }

            long written = 0;
            foreach (var (addr, size) in segments)
            {
                ct.ThrowIfCancellationRequested();

                EraseIfMx200(addr);

                uint a = addr;
                var segEnd = (long)addr + size;
                while ((long)a < segEnd && a != uint.MaxValue)
                {
                    ct.ThrowIfCancellationRequested();

                    var blockAddr = a;
                    var block = img.ReadBlock(ref a, (uint)maxBlock);
                    if (block == null)
                        continue; // gap skipped (ReadBlock advanced a); or loop ends

                    // ---- hard safety guards ----
                    if (block.Length > maxBlock || block.Length > 0xFFFF)
                        throw new InvalidOperationException(
                            $"SAFETY STOP: block length {block.Length} exceeds device max {maxBlock} - aborting before write");
                    if (blockAddr < addr || (long)blockAddr >= segEnd)
                        throw new InvalidOperationException(
                            $"SAFETY STOP: block address 0x{blockAddr:X8} outside device-declared " +
                            $"segment [0x{addr:X8}, 0x{segEnd:X8}) - aborting before write");

                    var allFF = block.All(b => b == 0xFF);
                    if (!allFF || size <= addr)
                        SendDataBlock(blockAddr, block);

                    written += block.Length;
                    progressCallback?.Invoke(totalBytes > 0 ? (double)written / totalBytes : 0);
                    statusCallback?.Invoke($"Writing at address: 0x{blockAddr:X}");
                }
            }

            statusCallback?.Invoke("Update is complete");
            return true;
        }
        finally
        {
            CloseMidi();
        }
    }

    private static byte[] BuildBlockPayload(uint addr, byte[] block)
    {
        var payload = new byte[6 + block.Length];
        payload[0] = (byte)(addr >> 24);
        payload[1] = (byte)(addr >> 16);
        payload[2] = (byte)(addr >> 8);
        payload[3] = (byte)addr;
        payload[4] = (byte)(block.Length >> 8);
        payload[5] = (byte)block.Length;
        Array.Copy(block, 0, payload, 6, block.Length);
        return payload;
    }

    private void SendDataBlock(uint addr, byte[] block)
    {
        var payload = BuildBlockPayload(addr, block);

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                Transact(0x7A, payload, 0x7E, QuickTimeoutMs);
                return;
            }
            catch (CommTargetException ex) when (ex.ErrorCode1 == 0x0C && attempt < 2)
            {
                // transient flash error: resend, up to 3 attempts total
            }
        }
    }

    private void EraseIfMx200(uint segmentAddr)
    {
        // Lexicon MX200 only (brand 1, family 0x1A, product 1), when segment addr == 0
        if (segmentAddr != 0) return;
        if (Product.BrandId != 1 || Product.FamilyId != 0x1A || Product.ProductId != 1)
            return;

        foreach (var addr in new uint[] { 0x8000, 0x6000, 0x4000, 0x0000 })
        {
            Transact(0x7B, new byte[] { 0x00 }, 0x7E, ShortTimeoutMs);
            var payload = new byte[]
            {
                (byte)(addr >> 24), (byte)(addr >> 16), (byte)(addr >> 8), (byte)addr,
                0x00, 0x01, 0xFF
            };
            Transact(0x7A, payload, 0x7E, QuickTimeoutMs);
        }
    }

    // Raw variant of Transact used by the UpdateProductID retry.
    private DecodedMessage TransactRaw(byte[] rawMessage, byte expectedCmd, int timeoutMs)
    {
        lock (_rxLock)
            _rxData = null;
        SendSysex(rawMessage);
        if (!_rxEvent.WaitOne(timeoutMs))
            throw new CommHostException("Timeout waiting for device response", 0);

        byte[] raw;
        lock (_rxLock)
            raw = _rxData;

        var msg = SysexCodec.Decode(raw);
        if (msg.Cmd == expectedCmd && msg.Cmd != 0x7E)
            return msg;
        if (msg.Cmd == 0x7F)
        {
            var e0 = msg.Remaining >= 1 ? msg.ReadByte() : 0;
            var e1 = msg.Remaining >= 1 ? msg.ReadByte() : 0;
            throw new CommTargetException(e0, e1);
        }
        throw new CommHostException($"Unexpected response command 0x{msg.Cmd:X2}", 2);
    }

    public string FormatDisplayString()
    {
        var id = DisplayId;
        var hasId = !string.IsNullOrEmpty(id);
        if (InUpdateMode)
        {
            return hasId
                ? $"{Product.Name} \"{id}\" (waiting for update)"
                : $"{Product.Name} (waiting for update)";
        }
        return hasId
            ? $"{Product.Name} \"{id}\" (at version {CurrentVersion})"
            : $"{Product.Name} (at version {CurrentVersion})";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CloseMidi();
    }
}
