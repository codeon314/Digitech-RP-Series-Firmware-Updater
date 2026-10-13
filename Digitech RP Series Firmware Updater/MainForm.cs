using System.Diagnostics;
using System.Runtime.InteropServices;
using NetUpdaterClone.Core;

namespace NetUpdaterClone;

public partial class MainForm : Form
{
    // wizard state
    private List<ProductInfo> _products = new();
    private readonly List<MidiTargetDevice> _devices = new();
    private MidiTargetDevice _selectedDevice;
    private List<UpdateConfig> _revisions = new();
    private UpdateConfig _selectedRevision;
    private byte[] _imageData;
    private CancellationTokenSource _cts = new();
    private readonly Stopwatch _updateStopwatch = new();

    private List<Panel> _pages;

    private int _currentPage;
    private bool _busy;
    private bool _busyChrome;
    private bool _resultSuccess;
    private bool _suppressFilterEvents;
    private Rectangle _nextButtonNormalBounds;
    private Rectangle _backButtonNormalBounds;

    public MainForm()
    {
        InitializeComponent();

        FormClosing += (_, e) =>
        {
            if (_busy)
            {
                e.Cancel = true;
                MessageBox.Show(this,
                    "An update is in progress. Use the Cancel button to abort it first.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { _cts.Cancel(); } catch { }
            foreach (var d in _devices)
                d.Dispose();
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _pages = new List<Panel>
        {
            welcomePage, scanPage, devicePage, filterPage, revisionPage,
            confirmPage, preparePage, downloadPage, updatePage, resultPage,
        };

        _nextButtonNormalBounds = nextButton.Bounds;
        _backButtonNormalBounds = backButton.Bounds;

        ShowPage(0);
        UpdateNavButtons();
    }

    private void ShowPage(int index)
    {
        _currentPage = index;
        for (var i = 0; i < _pages.Count; i++)
            _pages[i].Visible = i == index;
        Core.UpdateLog.Write($"UI: ShowPage({index})");
        UpdateNavButtons();
    }

    private void UpdateNavButtons()
    {
        cancelButton.Visible = _busy;
        quitButton.Enabled = !_busy;
        if (_busyChrome != _busy)
        {
            _busyChrome = _busy;
            SetBusyChrome(_busy);
        }

        if (_busy)
        {
            nextButton.Enabled = false;
            backButton.Enabled = false;
            return;
        }

        backButton.Enabled = _currentPage switch
        {
            0 => false,
            9 => false,   // result page
            _ => true,
        };
        nextButton.Enabled = _currentPage switch
        {
            0 => true,
            1 => false,   // scan page runs automatically
            2 => deviceList.SelectedIndex >= 0,
            3 => filterNewest.Checked || filterAll.Checked,
            4 => revisionList.SelectedIndex >= 0,
            5 => confirmCheck.Checked,
            6 => true,
            7 => false,
            8 => false,
            9 => _resultSuccess,   // "Update another device"
            _ => true,
        };
        var wideNext = _currentPage == 9 && _resultSuccess;
        nextButton.Text = wideNext ? "Update another device" : "Next >";

        if (wideNext)
        {
            var needed = TextRenderer.MeasureText(nextButton.Text, nextButton.Font,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine).Width + 28;
            var width = Math.Max(_nextButtonNormalBounds.Width, needed);
            nextButton.SetBounds(_nextButtonNormalBounds.Right - width, _nextButtonNormalBounds.Top,
                width, _nextButtonNormalBounds.Height);

            var gap = _nextButtonNormalBounds.Left - _backButtonNormalBounds.Right;
            backButton.SetBounds(nextButton.Left - gap - _backButtonNormalBounds.Width,
                _backButtonNormalBounds.Top, _backButtonNormalBounds.Width, _backButtonNormalBounds.Height);
        }
        else
        {
            nextButton.Bounds = _nextButtonNormalBounds;
            backButton.Bounds = _backButtonNormalBounds;
        }
    }

    private void SetBusyChrome(bool busy)
    {
        if (!IsHandleCreated)
            return;

        var hWnd = Handle;
        var menu = GetSystemMenu(hWnd, false);
        if (menu != IntPtr.Zero)
        {
            EnableMenuItem(menu, ScClose, MfByCommand | (busy ? MfGrayed : MfEnabled));
            EnableMenuItem(menu, ScMinimize, MfByCommand | (busy ? MfGrayed : MfEnabled));
        }

        var style = GetWindowLong(hWnd, GwlStyle);
        var newStyle = busy ? style & ~WsMinimizeBox : style | WsMinimizeBox;
        if (newStyle != style)
        {
            SetWindowLong(hWnd, GwlStyle, newStyle);
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged);
        }
        DrawMenuBar(hWnd);
    }

    private const int GwlStyle = -16;
    private const int WsMinimizeBox = 0x00020000;
    private const uint ScClose = 0xF060;
    private const uint ScMinimize = 0xF020;
    private const uint MfByCommand = 0x00000000;
    private const uint MfGrayed = 0x00000001;
    private const uint MfEnabled = 0x00000000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpFrameChanged = 0x0020;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool DrawMenuBar(IntPtr hWnd);

    private void OnBack()
    {
        switch (_currentPage)
        {
            case 2: ShowPage(0); break;
            case 3: ShowPage(2); break;
            case 4: ShowPage(3); break;
            case 5: ShowPage(4); break;
            case 6: ShowPage(5); break;
            case 8: ShowPage(6); break;
            default: ShowPage(Math.Max(0, _currentPage - 1)); break;
        }
    }

    private async Task OnNextAsync()
    {
        switch (_currentPage)
        {
            case 0:
                await RunScanAsync();
                break;
            case 2:
                _selectedDevice = _devices[deviceList.SelectedIndex];
                ShowPage(3);
                break;
            case 3:
                BuildRevisionList();
                break;
            case 4:
                _selectedRevision = _revisions[revisionList.SelectedIndex];
                ShowConfirm();
                break;
            case 5:
                if (!string.IsNullOrEmpty(_selectedDevice.Product.SpecialPreInstructions))
                {
                    prepareLabel.Text = _selectedDevice.Product.SpecialPreInstructions;
                    ShowPage(6);
                }
                else
                {
                    await RunDownloadAsync();
                }
                break;
            case 6:
                await RunDownloadAsync();
                break;
            case 9:
                RestartWizard();
                break;
        }
    }

    private void RestartWizard()
    {
        foreach (var d in _devices)
            d.Dispose();
        _devices.Clear();
        deviceList.Items.Clear();
        revisionList.Items.Clear();
        _products.Clear();
        _selectedDevice = null;
        _selectedRevision = null;
        _imageData = null;
        _resultSuccess = false;
        filterNewest.Checked = true;
        filterAll.Checked = false;
        dryRunCheck.Checked = false;
        ShowPage(0);
    }

    private async Task RunScanAsync()
    {
        _busy = true;
        UpdateNavButtons();
        ShowPage(1);
        scanStatus.Text = "Locating targets file...";
        try
        {
            _cts = new CancellationTokenSource();

            // 1. download + parse UpdateTargets.xml
            var xml = await Task.Run(() => TargetsDownloader.DownloadAsync(
                    AppContext.BaseDirectory,
                    s => SafeInvoke(() => scanStatus.Text = s),
                    _cts.Token))
                .ConfigureAwait(true);

            scanStatus.Text = "Downloading Update Information...";
            _products = UpdateTargetsParser.Parse(xml);
            Core.UpdateLog.Write($"XML parsed: {_products.Count} products");

            // 2. scan MIDI ports and identify devices
            scanStatus.Text = "Please wait while we look for connected devices...";
            _devices.Clear();
            var found = await Task.Run(() => DeviceScanner.Scan(_products,
                    s => SafeInvoke(() => scanStatus.Text = s),
                    _cts.Token))
                .ConfigureAwait(true);

            _devices.AddRange(found);
            deviceList.BeginUpdate();
            deviceList.Items.Clear();
            foreach (var d in _devices)
                deviceList.Items.Add(d.FormatDisplayString());
            deviceList.EndUpdate();
            if (deviceList.Items.Count > 0)
                deviceList.SelectedIndex = 0;

            Core.UpdateLog.Write(
                $"UI device page: _devices={_devices.Count}, listboxItems={deviceList.Items.Count}, " +
                $"first=\"{(deviceList.Items.Count > 0 ? deviceList.Items[0] : "<none>")}\"");

            ShowPage(2);
            if (_devices.Count == 0)
            {
                var diag = new System.Text.StringBuilder();
                diag.AppendLine("MIDI input devices:");
                foreach (var n in MidiPorts.InputNames())
                    diag.AppendLine($"  \"{n}\"");
                diag.AppendLine("MIDI output devices:");
                foreach (var n in MidiPorts.OutputNames())
                    diag.AppendLine($"  \"{n}\"");
                diag.AppendLine();
                diag.AppendLine($"Session log: {Core.UpdateLog.LogPath}");

                ShowResult("No Devices Were Found",
                    "No supported devices were detected",
                    "Please verify that a compatible device is:\r\n\t-Connected to your computer\r\n\t-Powered on\r\n\t-Not being used by another application\r\n\r\n" +
                    diag.ToString());
            }
        }
        catch (OperationCanceledException)
        {
            Core.UpdateLog.Write("Scan cancelled");
            ShowResult("Update Cancelled", "The update process was aborted", "");
        }
        catch (InvalidOperationException ex)
        {
            Core.UpdateLog.Write($"Scan error: {ex.GetType().Name}: {ex.Message}");
            ShowResult("Invalid Targets File",
                "The downloaded targets file is corrupt or invalid",
                ex.Message);
        }
        catch (Exception ex)
        {
            Core.UpdateLog.Write($"Scan error: {ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}");
            ShowResult("Unable to Connect",
                "A server connection could not be established",
                "A connection to the remote server could not be established. " +
                "The server may be temporarily unavailable or you may need to " +
                "manually connect to the internet first.\r\n\r\n" + ex.Message);
        }
        finally
        {
            _busy = false;
            UpdateNavButtons();
        }
    }

    private void BuildRevisionList()
    {
        var current = _selectedDevice.CurrentVersion;
        if (filterNewest.Checked)
        {
            var newest = _selectedDevice.Product.Revisions
                .Where(r => VersionComparer.IsNewer(r.Version, current))
                .OrderByDescending(r => r, Comparer<UpdateConfig>.Create(
                    (x, y) => VersionComparer.Compare(x.Version, y.Version)))
                .FirstOrDefault();

            if (newest == null)
            {
                ShowResult("No Updates Were Found",
                    "There are no updates available for your device at this time. " +
                    "Please check back periodically.",
                    "Your device's firmware is already up to date");
                return;
            }
            _revisions = new List<UpdateConfig> { newest };
        }
        else
        {
            _revisions = new List<UpdateConfig>(_selectedDevice.Product.Revisions);
        }

        revisionList.Items.Clear();
        foreach (var r in _revisions)
            revisionList.Items.Add(r.ToString());
        if (revisionList.Items.Count > 0)
            revisionList.SelectedIndex = 0;

        ShowPage(4);
    }

    private void ShowConfirm()
    {
        confirmText.Text =
            $"Are you sure you want to update the device: {_selectedDevice.FormatDisplayString()}" +
            $" to firmware version {_selectedRevision.Version}?";
        confirmCheck.Checked = false;
        ShowPage(5);
    }

    private async Task RunDownloadAsync()
    {
        _busy = true;
        UpdateNavButtons();
        ShowPage(7);
        downloadStatus.Text = "Connecting...";
        try
        {
            _cts = new CancellationTokenSource();
            _imageData = await Task.Run(() => FirmwareDownloader.DownloadAsync(
                    _selectedRevision.ImageUrl,
                    s => SafeInvoke(() => downloadStatus.Text = s),
                    _cts.Token))
                .ConfigureAwait(true);

            await RunUpdateAsync();
        }
        catch (OperationCanceledException)
        {
            ShowResult("Update Cancelled", "The update process was aborted", "");
        }
        catch (Exception ex)
        {
            ShowResult("Unable to Connect",
                "A server connection could not be established",
                ex.Message);
        }
        finally
        {
            _busy = false;
            UpdateNavButtons();
        }
    }

    private async Task RunUpdateAsync()
    {
        ShowPage(8);
        updateStatus.Text = "Initializing the device...";
        updateProgress.Value = 0;
        updatePercentLabel.Text = "0%";
        updateSpeedLabel.Text = "";
        updateEtaLabel.Text = "";
        try
        {
            // summarize the image before anything happens to the device
            DigBinImage summary;
            try
            {
                summary = DigBinImage.Parse(_imageData);
            }
            catch (Exception ex)
            {
                ShowResult("Update Failed",
                    "The downloaded image is corrupt or invalid",
                    ex.Message);
                return;
            }
            var dryRun = dryRunCheck.Checked;
            var totalBytes = summary.Records.Sum(r => (long)r.Size);
            updateStatus.Text =
                $"{(dryRun ? "SAFE TEST MODE - no data will be written. " : "")}" +
                $"Image: {summary.Records.Count} records, " +
                $"{totalBytes:N0} data bytes. " +
                "Initializing the device...";

            _updateStopwatch.Restart();
            _cts = new CancellationTokenSource();
            await Task.Run(() => _selectedDevice.PerformUpdate(
                    _imageData,
                    s => SafeInvoke(() => updateStatus.Text = s),
                    f => SafeInvoke(() => UpdateProgressUi(f, totalBytes)),
                    _cts.Token,
                    dryRun))
                .ConfigureAwait(true);
            _updateStopwatch.Stop();

            if (dryRun)
            {
                ShowResult("Test Complete",
                    "Handshake verified, no data was written to the device.",
                    "The device was put into update mode and responded correctly to the " +
                    "handshake. Power-cycle the device to return it to normal operation.\r\n\r\n" +
                    $"Session log: {Core.UpdateLog.LogPath}");
            }
            else
            {
                ShowResult("Update Succeeded",
                    "The update completed successfully",
                    _selectedRevision.SpecialPostInstructions.Length > 0
                        ? _selectedRevision.SpecialPostInstructions
                        : "Your device is now ready to use",
                    success: true);
            }
        }
        catch (OperationCanceledException)
        {
            ShowResult("Update Cancelled", "The update process was aborted", "");
        }
        catch (CommException ex)
        {
            ShowResult("Update Failed",
                "A failure occurred while updating the device's firmware",
                ex.Message + $"\r\n\r\nSession log: {Core.UpdateLog.LogPath}");
        }
        catch (Exception ex)
        {
            ShowResult("Update Failed",
                "A failure occurred while updating the device's firmware",
                ex.Message + $"\r\n\r\nSession log: {Core.UpdateLog.LogPath}");
        }
    }

    private void ShowResult(string title, string body, string detail, bool success = false)
    {
        _resultSuccess = success;
        resultTitle.Text = title;
        resultBody.Text = body;
        resultDetail.Text = detail;
        ShowPage(9);
    }

    private void SafeInvoke(Action a)
    {
        try
        {
            if (!IsDisposed && IsHandleCreated)
                BeginInvoke(a);
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }

    private void UpdateProgressUi(double fraction, long totalBytes)
    {
        if (fraction < 0) fraction = 0;
        if (fraction > 1) fraction = 1;

        var percent = (int)(fraction * 100);
        updateProgress.Value = percent;
        updatePercentLabel.Text = $"{percent}%";

        var elapsed = _updateStopwatch.Elapsed.TotalSeconds;
        if (elapsed < 0.5 || fraction <= 0)
            return;

        var written = (long)(fraction * totalBytes);
        var speed = written / elapsed;
        updateSpeedLabel.Text = FormatSpeed(speed);
        if (speed > 1)
        {
            var remaining = Math.Max(0, (totalBytes - written) / speed);
            updateEtaLabel.Text = "ETA " + FormatDuration(remaining);
        }
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
            return $"{bytesPerSecond:0} B/s";
        if (bytesPerSecond < 1024 * 1024)
            return $"{bytesPerSecond / 1024:0.0} KB/s";
        return $"{bytesPerSecond / 1048576:0.0} MB/s";
    }

    private static string FormatDuration(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Ceiling(seconds));
        return ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
    }

    // ---- designer-wired event handlers ----

    private void UpdateProgressBorder_Paint(object sender, PaintEventArgs e)
    {
        ControlPaint.DrawBorder(e.Graphics, updateProgressBorder.ClientRectangle,
            Color.Silver, ButtonBorderStyle.Solid);
    }

    private void QuitButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private async void NextButton_Click(object sender, EventArgs e)
    {
        await OnNextAsync();
    }

    private void BackButton_Click(object sender, EventArgs e)
    {
        OnBack();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        _cts.Cancel();
    }

    private void DeviceList_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateNavButtons();
    }

    private void RevisionList_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateNavButtons();
    }

    private void ConfirmCheck_CheckedChanged(object sender, EventArgs e)
    {
        UpdateNavButtons();
    }

    private void FilterNewest_CheckedChanged(object sender, EventArgs e)
    {
        if (_suppressFilterEvents) return;
        _suppressFilterEvents = true;
        if (filterNewest.Checked) filterAll.Checked = false;
        else if (!filterAll.Checked) filterNewest.Checked = true; // keep one selected
        _suppressFilterEvents = false;
        UpdateNavButtons();
    }

    private void FilterAll_CheckedChanged(object sender, EventArgs e)
    {
        if (_suppressFilterEvents) return;
        _suppressFilterEvents = true;
        if (filterAll.Checked) filterNewest.Checked = false;
        else if (!filterNewest.Checked) filterAll.Checked = true; // keep one selected
        _suppressFilterEvents = false;
        UpdateNavButtons();
    }
}
