namespace NetUpdaterClone.Core;

public sealed class DeviceScanner
{
    /// <summary>
    /// Mirrors sub_402820: enumerate MIDI ports, substring-match names against each
    /// product's Port entries (up to Port.Count matches), and identify candidates.
    /// </summary>
    public static List<MidiTargetDevice> Scan(List<ProductInfo> products,
        Action<string> statusCallback = null,
        CancellationToken ct = default)
    {
        var result = new List<MidiTargetDevice>();
        var inNames = MidiPorts.InputNames();
        var outNames = MidiPorts.OutputNames();

        UpdateLog.Write($"--- Scan: {products.Count} products from XML ---");
        UpdateLog.Write($"MIDI input devices ({inNames.Count}):");
        for (var i = 0; i < inNames.Count; i++)
            UpdateLog.Write($"  in[{i}] = \"{inNames[i]}\"");
        UpdateLog.Write($"MIDI output devices ({outNames.Count}):");
        for (var i = 0; i < outNames.Count; i++)
            UpdateLog.Write($"  out[{i}] = \"{outNames[i]}\"");

        foreach (var product in products)
        {
            ct.ThrowIfCancellationRequested();
            UpdateLog.Write($"Product \"{product.Name}\": family=0x{product.FamilyId:X2} " +
                            $"productId=0x{product.ProductId:X2} " +
                            $"updateProductId=0x{product.UpdateProductId:X2} " +
                            $"brand={product.BrandId} ports={product.Ports.Count}");
            foreach (var port in product.Ports)
            {
                UpdateLog.Write($"  Port \"{port.Name}\" count={port.Count}");
                var matched = 0;
                if (matched >= port.Count)
                    break;

                for (var i = 0; i < inNames.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    if (inNames[i].IndexOf(port.Name, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    UpdateLog.Write($"    in[{i}] \"{inNames[i]}\" matches port name");

                    var outIdx = MidiPorts.IndexOfName(outNames, port.Name);
                    if (outIdx < 0)
                    {
                        UpdateLog.Write($"    ...but no MIDI output device matches \"{port.Name}\"");
                        continue;
                    }
                    UpdateLog.Write($"    out[{outIdx}] \"{outNames[outIdx]}\" matches port name");

                    statusCallback?.Invoke($"Identifying devices... ({port.Name})");

                    var dev = new MidiTargetDevice(product, i, outIdx);
                    try
                    {
                        if (dev.Identify())
                        {
                            result.Add(dev);
                            matched++;
                            UpdateLog.Write($"    IDENTIFIED: id=\"{dev.DisplayId}\" " +
                                            $"version=\"{dev.CurrentVersion}\" " +
                                            $"updateMode={dev.InUpdateMode}");
                        }
                        else
                        {
                            UpdateLog.Write("    identify() returned false");
                            dev.Dispose();
                        }
                    }
                    catch (CommException ex)
                    {
                        UpdateLog.Write($"    identify FAILED: {ex.GetType().Name}: {ex.Message}");
                        dev.Dispose();
                    }
                    catch (InvalidOperationException ex)
                    {
                        UpdateLog.Write($"    identify FAILED: {ex.GetType().Name}: {ex.Message}");
                        dev.Dispose();
                    }

                    if (matched >= port.Count)
                        break;
                }
            }
        }

        UpdateLog.Write($"--- Scan complete: {result.Count} device(s) found ---");
        return result;
    }
}
