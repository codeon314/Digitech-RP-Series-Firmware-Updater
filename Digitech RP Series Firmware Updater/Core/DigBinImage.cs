namespace NetUpdaterClone.Core;

/// <summary>
/// .DigBin firmware image:
///   offset 0: magic 00 5C 00 05 (LE dword 0x05005C00)
///   then repeated records: uint32 BIG-ENDIAN flashAddress, uint32 BIG-ENDIAN size,
///   followed by <size> bytes of data.
/// (Confirmed against the real rp500V20.DigBin: 16 x 64 KB blocks, 1 MB total,
///  records end exactly at EOF.)
/// </summary>
public sealed class DigBinImage
{
    public const uint Magic = 0x05005C00;

    public sealed class Record
    {
        public uint Address;
        public byte[] Data;
        public uint Size => (uint)Data.Length;
    }

    public List<Record> Records = new();

    private static uint ReadBe32(byte[] b, int off)
        => (uint)((b[off] << 24) | (b[off + 1] << 16) | (b[off + 2] << 8) | b[off + 3]);

    public static DigBinImage Parse(byte[] fileData)
    {
        if (fileData == null || fileData.Length < 4)
            throw new InvalidOperationException("Unexpected end of file encoutered");
        if (BitConverter.ToUInt32(fileData, 0) != Magic)
            throw new InvalidOperationException("Invalid firmware image");

        var img = new DigBinImage();
        var pos = 4;
        while (pos < fileData.Length)
        {
            if (pos + 8 > fileData.Length)
                throw new InvalidOperationException("Unexpected end of file encoutered");
            var addr = ReadBe32(fileData, pos);
            var size = ReadBe32(fileData, pos + 4);
            pos += 8;
            if (pos + size > fileData.Length)
                throw new InvalidOperationException("Unexpected end of file encoutered");
            var data = new byte[size];
            Array.Copy(fileData, pos, data, 0, size);
            pos += (int)size;
            img.Records.Add(new Record { Address = addr, Data = data });
        }
        return img;
    }

    /// <summary>
    /// Reads the next block of image data for flash address <paramref name="address"/>.
    /// Advances <paramref name="address"/> past the block (skipping gaps not covered
    /// by any record). Returns null when no more data remains for the segment.
    /// </summary>
    public byte[] ReadBlock(ref uint address, uint maxBlockSize)
    {
        Record rec = null;
        foreach (var r in Records)
        {
            if (address >= r.Address && address < r.Address + r.Size)
            {
                rec = r;
                break;
            }
        }

        if (rec == null)
        {
            // gap: advance to the next record that starts after this address,
            // or end of image (0xFFFFFFFF terminates the caller's loop)
            Record next = null;
            foreach (var r in Records)
            {
                if (r.Address > address && (next == null || r.Address < next.Address))
                    next = r;
            }
            address = next != null ? next.Address : uint.MaxValue;
            return null;
        }

        var offset = address - rec.Address;
        var count = (int)Math.Min(maxBlockSize, rec.Size - offset);
        var block = new byte[count];
        Array.Copy(rec.Data, offset, block, 0, count);
        address += (uint)count;
        return block;
    }

    /// <summary>Total image bytes overlapping [segmentAddr, segmentAddr+segmentSize).</summary>
    public long Coverage(uint segmentAddr, uint segmentSize)
    {
        long total = 0;
        var segEnd = (long)segmentAddr + segmentSize;
        foreach (var rec in Records)
        {
            var recEnd = (long)rec.Address + rec.Size;
            var overlap = Math.Min(recEnd, segEnd) - Math.Max((long)rec.Address, segmentAddr);
            if (overlap > 0)
                total += overlap;
        }
        return total;
    }
}
