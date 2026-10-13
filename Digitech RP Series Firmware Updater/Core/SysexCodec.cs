namespace NetUpdaterClone.Core;

/// <summary>
/// Wire format for the Super7 protocol used by NetUpdater (hmg::Super7TxSysexMsg /
/// Super7RxSysexMsg):
///
///   F0 <header> <raw0> <raw1> <raw2> <CMD> [7-bit packed payload] <CHK> <F7>
///
/// header: Lexicon (brand 1)  -> F0 06
///         DigiTech (brand 2) -> F0 00 00 10
/// raw bytes: session byte, FamilyID, ProductID/UpdateProductID, command
/// CHK = (XOR of all bytes after F0 and before CHK) &amp; 0x7F, chosen so that the
///       XOR of the entire message equals 0x07.
/// </summary>
public static class SysexCodec
{
    public static byte[] HeaderFor(int brand)
    {
        return brand == 2 ? new byte[] { 0xF0, 0x00, 0x00, 0x10 } : new byte[] { 0xF0, 0x06 };
    }

    /// <summary>Builds a complete outbound sysex message.</summary>
    public static byte[] Build(int brand, byte sessionByte, byte familyId, byte productId, byte cmd, byte[] payload)
    {
        var header = HeaderFor(brand);
        var body = new List<byte> { 0xF0 };
        body.AddRange(header.Skip(1)); // skip duplicate F0
        body.Add(sessionByte);
        body.Add(familyId);
        body.Add(productId);
        body.Add(cmd);
        if (payload != null && payload.Length > 0)
            body.AddRange(Pack(payload));

        var buf = body.ToArray();
        body.Add((byte)Checksum(buf));
        body.Add(0xF7);
        return body.ToArray();
    }

    /// <summary>XOR of bytes [1..len-1], masked to 7 bits (sub_40A920).</summary>
    public static int Checksum(byte[] data)
    {
        var x = 0;
        for (var i = 1; i < data.Length; i++)
            x ^= data[i];
        return x & 0x7F;
    }

    /// <summary>7-bit packing: every 7 data bytes get one leading header byte
    /// carrying their bit-7s (bit 6 <-> first data byte ... bit 0 <-> seventh).</summary>
    public static byte[] Pack(byte[] data)
    {
        var outBuf = new List<byte>(data.Length + data.Length / 7 + 1);
        var headerPos = 0;
        for (var i = 0; i < data.Length; i++)
        {
            var bit = i % 7;
            if (bit == 0)
            {
                headerPos = outBuf.Count;
                outBuf.Add(0);
            }
            if ((data[i] & 0x80) != 0)
                outBuf[headerPos] |= (byte)(0x40 >> bit);
            outBuf.Add((byte)(data[i] & 0x7F));
        }
        return outBuf.ToArray();
    }

    /// <summary>Unpacks a 7-bit packed payload from an in-memory buffer.</summary>
    public static byte[] Unpack(byte[] data, int offset, int count)
    {
        using var ms = new MemoryStream();
        var header = 0;
        var bit = 0;
        for (var i = offset; i < offset + count; i++)
        {
            var b = data[i];
            if (bit == 0)
            {
                header = b;
                bit = 1;
            }
            else
            {
                var v = (byte)(((header << bit) & 0x80) | (b & 0x7F));
                ms.WriteByte(v);
                bit = (bit + 1) % 8;
            }
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Decodes an inbound message (expected to start with F0 and end with F7).
    /// Returns (cmd, sessionByte, familyId, productId, decodedPayload).
    /// Throws on a bad checksum (whole-message XOR must equal 0x07) or a too-short message.
    /// </summary>
    public static DecodedMessage Decode(byte[] data)
    {
        if (data == null || data.Length < 4)
            throw new InvalidOperationException("MIDI sysex message too short");

        // whole-message XOR check (sub_40A500: xor cl, 7 / jz)
        var acc = 0;
        foreach (var b in data)
            acc ^= b;
        if (acc != 0x07)
            throw new InvalidOperationException("Sysex checksum mismatch");

        // header phase: consume headerLen+4 raw bytes
        var headerLen = (data.Length > 3 && data[1] == 0) ? 4 : 2;
        if (data.Length < headerLen + 4 + 2)
            throw new InvalidOperationException("MIDI sysex message too short");

        var session = data[headerLen + 0];
        var family = data[headerLen + 1];
        var product = data[headerLen + 2];
        var cmd = data[headerLen + 3];

        // packed payload: bytes between the raw region and CHK/F7
        var packedStart = headerLen + 4;
        var packedEnd = data.Length - 2; // exclude CHK and F7
        byte[] payload;
        if (packedEnd > packedStart)
            payload = Unpack(data, packedStart, packedEnd - packedStart);
        else
            payload = Array.Empty<byte>();

        return new DecodedMessage(cmd, session, family, product, payload);
    }
}

public sealed class DecodedMessage
{
    public byte Cmd;
    public byte SessionByte;
    public byte FamilyId;
    public byte ProductId;
    public byte[] Payload;

    public DecodedMessage(byte cmd, byte session, byte family, byte product, byte[] payload)
    {
        Cmd = cmd;
        SessionByte = session;
        FamilyId = family;
        ProductId = product;
        Payload = payload;
    }

    public int Pos;

    public byte ReadByte()
    {
        if (Pos >= Payload.Length)
            throw new InvalidOperationException("MIDI sysex message too short");
        return Payload[Pos++];
    }

    public int ReadWordBE()
    {
        var hi = ReadByte();
        var lo = ReadByte();
        return (hi << 8) | lo;
    }

    public int ReadDwordBE()
    {
        var b0 = ReadByte();
        var b1 = ReadByte();
        var b2 = ReadByte();
        var b3 = ReadByte();
        return (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
    }

    public int Remaining => Payload.Length - Pos;
}
