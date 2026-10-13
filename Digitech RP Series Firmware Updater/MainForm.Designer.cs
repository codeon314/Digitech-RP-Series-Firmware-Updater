namespace NetUpdaterClone;

partial class MainForm
{
    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        welcomePage = new Panel();
        welcomePageTitle = new Label();
        welcomePageBody = new Label();
        scanPage = new Panel();
        scanPageTitle = new Label();
        scanPageBody = new Label();
        scanStatus = new Label();
        devicePage = new Panel();
        devicePageTitle = new Label();
        devicePageBody = new Label();
        deviceList = new ListBox();
        filterPage = new Panel();
        filterPageTitle = new Label();
        filterPageBody = new Label();
        filterNewest = new CheckBox();
        filterAll = new CheckBox();
        revisionPage = new Panel();
        revisionPageTitle = new Label();
        revisionPageBody = new Label();
        revisionList = new ListBox();
        confirmPage = new Panel();
        confirmPageTitle = new Label();
        confirmPageBody = new Label();
        confirmText = new TextBox();
        confirmCheck = new CheckBox();
        dryRunCheck = new CheckBox();
        preparePage = new Panel();
        preparePageTitle = new Label();
        prepareLabel = new Label();
        downloadPage = new Panel();
        downloadPageTitle = new Label();
        downloadPageBody = new Label();
        downloadStatus = new Label();
        downloadProgress = new ProgressBar();
        updatePage = new Panel();
        updatePageTitle = new Label();
        updatePageBody = new Label();
        updateStatus = new Label();
        updateProgressBorder = new Panel();
        updateProgress = new ProgressBar();
        updatePercentLabel = new Label();
        updateSpeedLabel = new Label();
        updateEtaLabel = new Label();
        resultPage = new Panel();
        resultTitle = new Label();
        resultBody = new Label();
        resultDetail = new Label();
        navPanel = new Panel();
        quitButton = new Button();
        backButton = new Button();
        cancelButton = new Button();
        nextButton = new Button();
        welcomePage.SuspendLayout();
        scanPage.SuspendLayout();
        devicePage.SuspendLayout();
        filterPage.SuspendLayout();
        revisionPage.SuspendLayout();
        confirmPage.SuspendLayout();
        preparePage.SuspendLayout();
        downloadPage.SuspendLayout();
        updatePage.SuspendLayout();
        updateProgressBorder.SuspendLayout();
        resultPage.SuspendLayout();
        navPanel.SuspendLayout();
        SuspendLayout();
        // 
        // welcomePage
        // 
        welcomePage.Controls.Add(welcomePageTitle);
        welcomePage.Controls.Add(welcomePageBody);
        welcomePage.Dock = DockStyle.Fill;
        welcomePage.Location = new Point(0, 0);
        welcomePage.Margin = new Padding(4, 5, 4, 5);
        welcomePage.Name = "welcomePage";
        welcomePage.Size = new Size(714, 833);
        welcomePage.TabIndex = 0;
        // 
        // welcomePageTitle
        // 
        welcomePageTitle.AutoSize = true;
        welcomePageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        welcomePageTitle.Location = new Point(23, 27);
        welcomePageTitle.Margin = new Padding(4, 0, 4, 0);
        welcomePageTitle.Name = "welcomePageTitle";
        welcomePageTitle.Size = new Size(464, 36);
        welcomePageTitle.TabIndex = 0;
        welcomePageTitle.Text = "DigiTech RP Series Firmware Updater";
        // 
        // welcomePageBody
        // 
        welcomePageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        welcomePageBody.Location = new Point(26, 80);
        welcomePageBody.Margin = new Padding(4, 0, 4, 0);
        welcomePageBody.Name = "welcomePageBody";
        welcomePageBody.Size = new Size(657, 317);
        welcomePageBody.TabIndex = 1;
        welcomePageBody.Text = resources.GetString("welcomePageBody.Text");
        // 
        // scanPage
        // 
        scanPage.Controls.Add(scanPageTitle);
        scanPage.Controls.Add(scanPageBody);
        scanPage.Controls.Add(scanStatus);
        scanPage.Dock = DockStyle.Fill;
        scanPage.Location = new Point(0, 0);
        scanPage.Margin = new Padding(4, 5, 4, 5);
        scanPage.Name = "scanPage";
        scanPage.Size = new Size(714, 833);
        scanPage.TabIndex = 1;
        // 
        // scanPageTitle
        // 
        scanPageTitle.AutoSize = true;
        scanPageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        scanPageTitle.Location = new Point(23, 27);
        scanPageTitle.Margin = new Padding(4, 0, 4, 0);
        scanPageTitle.Name = "scanPageTitle";
        scanPageTitle.Size = new Size(505, 36);
        scanPageTitle.TabIndex = 0;
        scanPageTitle.Text = "Checking for available firmware updates";
        // 
        // scanPageBody
        // 
        scanPageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        scanPageBody.Location = new Point(26, 80);
        scanPageBody.Margin = new Padding(4, 0, 4, 0);
        scanPageBody.Name = "scanPageBody";
        scanPageBody.Size = new Size(657, 67);
        scanPageBody.TabIndex = 1;
        scanPageBody.Text = "Downloading Update Information...";
        // 
        // scanStatus
        // 
        scanStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        scanStatus.Location = new Point(26, 250);
        scanStatus.Margin = new Padding(4, 0, 4, 0);
        scanStatus.Name = "scanStatus";
        scanStatus.Size = new Size(657, 200);
        scanStatus.TabIndex = 2;
        scanStatus.Text = "Locating targets file...";
        // 
        // devicePage
        // 
        devicePage.Controls.Add(deviceList);
        devicePage.Controls.Add(devicePageTitle);
        devicePage.Controls.Add(devicePageBody);
        devicePage.Dock = DockStyle.Fill;
        devicePage.Location = new Point(0, 0);
        devicePage.Margin = new Padding(4, 5, 4, 5);
        devicePage.Name = "devicePage";
        devicePage.Size = new Size(714, 833);
        devicePage.TabIndex = 2;
        // 
        // devicePageTitle
        // 
        devicePageTitle.AutoSize = true;
        devicePageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        devicePageTitle.Location = new Point(23, 27);
        devicePageTitle.Margin = new Padding(4, 0, 4, 0);
        devicePageTitle.Name = "devicePageTitle";
        devicePageTitle.Size = new Size(371, 36);
        devicePageTitle.TabIndex = 0;
        devicePageTitle.Text = "Select a device to be updated";
        // 
        // devicePageBody
        // 
        devicePageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        devicePageBody.Location = new Point(26, 80);
        devicePageBody.Margin = new Padding(4, 0, 4, 0);
        devicePageBody.Name = "devicePageBody";
        devicePageBody.Size = new Size(657, 67);
        devicePageBody.TabIndex = 1;
        devicePageBody.Text = "Select a device";
        // 
        // deviceList
        // 
        deviceList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        deviceList.IntegralHeight = false;
        deviceList.Location = new Point(26, 160);
        deviceList.Margin = new Padding(4, 5, 4, 5);
        deviceList.Name = "deviceList";
        deviceList.Size = new Size(655, 364);
        deviceList.TabIndex = 2;
        deviceList.SelectedIndexChanged += DeviceList_SelectedIndexChanged;
        // 
        // filterPage
        // 
        filterPage.Controls.Add(filterPageTitle);
        filterPage.Controls.Add(filterPageBody);
        filterPage.Controls.Add(filterNewest);
        filterPage.Controls.Add(filterAll);
        filterPage.Dock = DockStyle.Fill;
        filterPage.Location = new Point(0, 0);
        filterPage.Margin = new Padding(4, 5, 4, 5);
        filterPage.Name = "filterPage";
        filterPage.Size = new Size(714, 833);
        filterPage.TabIndex = 3;
        // 
        // filterPageTitle
        // 
        filterPageTitle.AutoSize = true;
        filterPageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        filterPageTitle.Location = new Point(23, 27);
        filterPageTitle.Margin = new Padding(4, 0, 4, 0);
        filterPageTitle.Name = "filterPageTitle";
        filterPageTitle.Size = new Size(410, 36);
        filterPageTitle.TabIndex = 0;
        filterPageTitle.Text = "Select a version filtering method";
        // 
        // filterPageBody
        // 
        filterPageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        filterPageBody.Location = new Point(26, 80);
        filterPageBody.Margin = new Padding(4, 0, 4, 0);
        filterPageBody.Name = "filterPageBody";
        filterPageBody.Size = new Size(657, 67);
        filterPageBody.TabIndex = 1;
        filterPageBody.Text = "Version Filtering";
        // 
        // filterNewest
        // 
        filterNewest.AutoEllipsis = true;
        filterNewest.Checked = true;
        filterNewest.CheckState = CheckState.Checked;
        filterNewest.Location = new Point(26, 183);
        filterNewest.Margin = new Padding(4, 5, 4, 5);
        filterNewest.Name = "filterNewest";
        filterNewest.Size = new Size(657, 73);
        filterNewest.TabIndex = 2;
        filterNewest.Text = "Only show the latest version newer than mine (recommended)";
        filterNewest.CheckedChanged += FilterNewest_CheckedChanged;
        // 
        // filterAll
        // 
        filterAll.AutoEllipsis = true;
        filterAll.Location = new Point(26, 267);
        filterAll.Margin = new Padding(4, 5, 4, 5);
        filterAll.Name = "filterAll";
        filterAll.Size = new Size(657, 50);
        filterAll.TabIndex = 3;
        filterAll.Text = "Show all available versions";
        filterAll.CheckedChanged += FilterAll_CheckedChanged;
        // 
        // revisionPage
        // 
        revisionPage.Controls.Add(revisionList);
        revisionPage.Controls.Add(revisionPageTitle);
        revisionPage.Controls.Add(revisionPageBody);
        revisionPage.Dock = DockStyle.Fill;
        revisionPage.Location = new Point(0, 0);
        revisionPage.Margin = new Padding(4, 5, 4, 5);
        revisionPage.Name = "revisionPage";
        revisionPage.Size = new Size(714, 833);
        revisionPage.TabIndex = 4;
        // 
        // revisionPageTitle
        // 
        revisionPageTitle.AutoSize = true;
        revisionPageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        revisionPageTitle.Location = new Point(23, 27);
        revisionPageTitle.Margin = new Padding(4, 0, 4, 0);
        revisionPageTitle.Name = "revisionPageTitle";
        revisionPageTitle.Size = new Size(277, 36);
        revisionPageTitle.TabIndex = 0;
        revisionPageTitle.Text = "Updates are Available";
        // 
        // revisionPageBody
        // 
        revisionPageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        revisionPageBody.Location = new Point(26, 80);
        revisionPageBody.Margin = new Padding(4, 0, 4, 0);
        revisionPageBody.Name = "revisionPageBody";
        revisionPageBody.Size = new Size(657, 67);
        revisionPageBody.TabIndex = 1;
        revisionPageBody.Text = "Select the firmware version to install";
        // 
        // revisionList
        // 
        revisionList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        revisionList.IntegralHeight = false;
        revisionList.Location = new Point(26, 160);
        revisionList.Margin = new Padding(4, 5, 4, 5);
        revisionList.Name = "revisionList";
        revisionList.Size = new Size(655, 364);
        revisionList.TabIndex = 2;
        revisionList.SelectedIndexChanged += RevisionList_SelectedIndexChanged;
        // 
        // confirmPage
        // 
        confirmPage.Controls.Add(confirmPageTitle);
        confirmPage.Controls.Add(confirmPageBody);
        confirmPage.Controls.Add(confirmText);
        confirmPage.Controls.Add(confirmCheck);
        confirmPage.Controls.Add(dryRunCheck);
        confirmPage.Dock = DockStyle.Fill;
        confirmPage.Location = new Point(0, 0);
        confirmPage.Margin = new Padding(4, 5, 4, 5);
        confirmPage.Name = "confirmPage";
        confirmPage.Size = new Size(714, 833);
        confirmPage.TabIndex = 5;
        // 
        // confirmPageTitle
        // 
        confirmPageTitle.AutoSize = true;
        confirmPageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        confirmPageTitle.Location = new Point(23, 27);
        confirmPageTitle.Margin = new Padding(4, 0, 4, 0);
        confirmPageTitle.Name = "confirmPageTitle";
        confirmPageTitle.Size = new Size(272, 36);
        confirmPageTitle.TabIndex = 0;
        confirmPageTitle.Text = "Confirm your choices";
        // 
        // confirmPageBody
        // 
        confirmPageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        confirmPageBody.Location = new Point(26, 80);
        confirmPageBody.Margin = new Padding(4, 0, 4, 0);
        confirmPageBody.Name = "confirmPageBody";
        confirmPageBody.Size = new Size(657, 67);
        confirmPageBody.TabIndex = 1;
        confirmPageBody.Text = "Confirm";
        // 
        // confirmText
        // 
        confirmText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        confirmText.Location = new Point(26, 160);
        confirmText.Margin = new Padding(4, 5, 4, 5);
        confirmText.Multiline = true;
        confirmText.Name = "confirmText";
        confirmText.ReadOnly = true;
        confirmText.ScrollBars = ScrollBars.Vertical;
        confirmText.Size = new Size(655, 231);
        confirmText.TabIndex = 2;
        // 
        // confirmCheck
        // 
        confirmCheck.Location = new Point(26, 408);
        confirmCheck.Margin = new Padding(4, 5, 4, 5);
        confirmCheck.Name = "confirmCheck";
        confirmCheck.Size = new Size(657, 50);
        confirmCheck.TabIndex = 3;
        confirmCheck.Text = "Yes, I realize my settings may be lost.";
        confirmCheck.CheckedChanged += ConfirmCheck_CheckedChanged;
        // 
        // dryRunCheck
        // 
        dryRunCheck.ForeColor = Color.Firebrick;
        dryRunCheck.Location = new Point(26, 475);
        dryRunCheck.Margin = new Padding(4, 5, 4, 5);
        dryRunCheck.Name = "dryRunCheck";
        dryRunCheck.Size = new Size(657, 67);
        dryRunCheck.TabIndex = 4;
        dryRunCheck.Text = "SAFE TEST MODE: verify handshake only, write NOTHING to the device";
        // 
        // preparePage
        // 
        preparePage.Controls.Add(prepareLabel);
        preparePage.Controls.Add(preparePageTitle);
        preparePage.Dock = DockStyle.Fill;
        preparePage.Location = new Point(0, 0);
        preparePage.Margin = new Padding(4, 5, 4, 5);
        preparePage.Name = "preparePage";
        preparePage.Size = new Size(714, 833);
        preparePage.TabIndex = 6;
        // 
        // preparePageTitle
        // 
        preparePageTitle.AutoSize = true;
        preparePageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        preparePageTitle.Location = new Point(23, 27);
        preparePageTitle.Margin = new Padding(4, 0, 4, 0);
        preparePageTitle.Name = "preparePageTitle";
        preparePageTitle.Size = new Size(238, 36);
        preparePageTitle.TabIndex = 0;
        preparePageTitle.Text = "Prepare to Update";
        // 
        // prepareLabel
        // 
        prepareLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        prepareLabel.Location = new Point(26, 160);
        prepareLabel.Margin = new Padding(4, 0, 4, 0);
        prepareLabel.Name = "prepareLabel";
        prepareLabel.Size = new Size(657, 367);
        prepareLabel.TabIndex = 1;
        // 
        // downloadPage
        // 
        downloadPage.Controls.Add(downloadPageTitle);
        downloadPage.Controls.Add(downloadPageBody);
        downloadPage.Controls.Add(downloadStatus);
        downloadPage.Controls.Add(downloadProgress);
        downloadPage.Dock = DockStyle.Fill;
        downloadPage.Location = new Point(0, 0);
        downloadPage.Margin = new Padding(4, 5, 4, 5);
        downloadPage.Name = "downloadPage";
        downloadPage.Size = new Size(714, 833);
        downloadPage.TabIndex = 7;
        // 
        // downloadPageTitle
        // 
        downloadPageTitle.AutoSize = true;
        downloadPageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        downloadPageTitle.Location = new Point(23, 27);
        downloadPageTitle.Margin = new Padding(4, 0, 4, 0);
        downloadPageTitle.Name = "downloadPageTitle";
        downloadPageTitle.Size = new Size(296, 36);
        downloadPageTitle.TabIndex = 0;
        downloadPageTitle.Text = "Downloading Update...";
        // 
        // downloadPageBody
        // 
        downloadPageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        downloadPageBody.Location = new Point(26, 80);
        downloadPageBody.Margin = new Padding(4, 0, 4, 0);
        downloadPageBody.Name = "downloadPageBody";
        downloadPageBody.Size = new Size(657, 67);
        downloadPageBody.TabIndex = 1;
        downloadPageBody.Text = "Downloading the firmware image";
        // 
        // downloadStatus
        // 
        downloadStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        downloadStatus.Location = new Point(26, 200);
        downloadStatus.Margin = new Padding(4, 0, 4, 0);
        downloadStatus.Name = "downloadStatus";
        downloadStatus.Size = new Size(657, 100);
        downloadStatus.TabIndex = 2;
        downloadStatus.Text = "Connecting...";
        // 
        // downloadProgress
        // 
        downloadProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        downloadProgress.Location = new Point(26, 317);
        downloadProgress.Margin = new Padding(4, 5, 4, 5);
        downloadProgress.Name = "downloadProgress";
        downloadProgress.Size = new Size(657, 33);
        downloadProgress.Style = ProgressBarStyle.Marquee;
        downloadProgress.TabIndex = 3;
        // 
        // updatePage
        // 
        updatePage.Controls.Add(updatePageTitle);
        updatePage.Controls.Add(updatePageBody);
        updatePage.Controls.Add(updateStatus);
        updatePage.Controls.Add(updateProgressBorder);
        updatePage.Controls.Add(updatePercentLabel);
        updatePage.Controls.Add(updateSpeedLabel);
        updatePage.Controls.Add(updateEtaLabel);
        updatePage.Dock = DockStyle.Fill;
        updatePage.Location = new Point(0, 0);
        updatePage.Margin = new Padding(4, 5, 4, 5);
        updatePage.Name = "updatePage";
        updatePage.Size = new Size(714, 833);
        updatePage.TabIndex = 8;
        // 
        // updatePageTitle
        // 
        updatePageTitle.AutoSize = true;
        updatePageTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        updatePageTitle.Location = new Point(23, 27);
        updatePageTitle.Margin = new Padding(4, 0, 4, 0);
        updatePageTitle.Name = "updatePageTitle";
        updatePageTitle.Size = new Size(358, 36);
        updatePageTitle.TabIndex = 0;
        updatePageTitle.Text = "Updating Device Firmware...";
        // 
        // updatePageBody
        // 
        updatePageBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        updatePageBody.Location = new Point(26, 80);
        updatePageBody.Margin = new Padding(4, 0, 4, 0);
        updatePageBody.Name = "updatePageBody";
        updatePageBody.Size = new Size(657, 67);
        updatePageBody.TabIndex = 1;
        updatePageBody.Text = "Do not disconnect the device while updating";
        // 
        // updateStatus
        // 
        updateStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        updateStatus.Location = new Point(26, 200);
        updateStatus.Margin = new Padding(4, 0, 4, 0);
        updateStatus.Name = "updateStatus";
        updateStatus.Size = new Size(657, 100);
        updateStatus.TabIndex = 2;
        updateStatus.Text = "Initializing the device...";
        // 
        // updateProgressBorder
        // 
        updateProgressBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        updateProgressBorder.Controls.Add(updateProgress);
        updateProgressBorder.Location = new Point(24, 315);
        updateProgressBorder.Margin = new Padding(4, 5, 4, 5);
        updateProgressBorder.Name = "updateProgressBorder";
        updateProgressBorder.Size = new Size(660, 37);
        updateProgressBorder.TabIndex = 3;
        updateProgressBorder.Paint += UpdateProgressBorder_Paint;
        // 
        // updateProgress
        // 
        updateProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        updateProgress.Location = new Point(1, 2);
        updateProgress.Margin = new Padding(4, 5, 4, 5);
        updateProgress.Name = "updateProgress";
        updateProgress.Size = new Size(657, 33);
        updateProgress.Style = ProgressBarStyle.Continuous;
        updateProgress.TabIndex = 0;
        // 
        // updatePercentLabel
        // 
        updatePercentLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        updatePercentLabel.Location = new Point(26, 360);
        updatePercentLabel.Margin = new Padding(4, 0, 4, 0);
        updatePercentLabel.Name = "updatePercentLabel";
        updatePercentLabel.Size = new Size(657, 30);
        updatePercentLabel.TabIndex = 4;
        updatePercentLabel.Text = "0%";
        updatePercentLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // updateSpeedLabel
        // 
        updateSpeedLabel.Location = new Point(26, 400);
        updateSpeedLabel.Margin = new Padding(4, 0, 4, 0);
        updateSpeedLabel.Name = "updateSpeedLabel";
        updateSpeedLabel.Size = new Size(314, 30);
        updateSpeedLabel.TabIndex = 5;
        // 
        // updateEtaLabel
        // 
        updateEtaLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        updateEtaLabel.Location = new Point(369, 400);
        updateEtaLabel.Margin = new Padding(4, 0, 4, 0);
        updateEtaLabel.Name = "updateEtaLabel";
        updateEtaLabel.Size = new Size(314, 30);
        updateEtaLabel.TabIndex = 6;
        updateEtaLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // resultPage
        // 
        resultPage.Controls.Add(resultTitle);
        resultPage.Controls.Add(resultBody);
        resultPage.Controls.Add(resultDetail);
        resultPage.Dock = DockStyle.Fill;
        resultPage.Location = new Point(0, 0);
        resultPage.Margin = new Padding(4, 5, 4, 5);
        resultPage.Name = "resultPage";
        resultPage.Size = new Size(714, 833);
        resultPage.TabIndex = 9;
        // 
        // resultTitle
        // 
        resultTitle.AutoSize = true;
        resultTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        resultTitle.Location = new Point(23, 27);
        resultTitle.Margin = new Padding(4, 0, 4, 0);
        resultTitle.Name = "resultTitle";
        resultTitle.Size = new Size(0, 36);
        resultTitle.TabIndex = 0;
        // 
        // resultBody
        // 
        resultBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        resultBody.Location = new Point(26, 87);
        resultBody.Margin = new Padding(4, 0, 4, 0);
        resultBody.Name = "resultBody";
        resultBody.Size = new Size(657, 150);
        resultBody.TabIndex = 1;
        // 
        // resultDetail
        // 
        resultDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        resultDetail.Location = new Point(26, 267);
        resultDetail.Margin = new Padding(4, 0, 4, 0);
        resultDetail.Name = "resultDetail";
        resultDetail.Size = new Size(657, 350);
        resultDetail.TabIndex = 2;
        // 
        // navPanel
        // 
        navPanel.Controls.Add(quitButton);
        navPanel.Controls.Add(backButton);
        navPanel.Controls.Add(cancelButton);
        navPanel.Controls.Add(nextButton);
        navPanel.Dock = DockStyle.Bottom;
        navPanel.Location = new Point(0, 833);
        navPanel.Margin = new Padding(4, 5, 4, 5);
        navPanel.Name = "navPanel";
        navPanel.Size = new Size(714, 100);
        navPanel.TabIndex = 10;
        // 
        // quitButton
        // 
        quitButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        quitButton.Location = new Point(17, 30);
        quitButton.Margin = new Padding(4, 5, 4, 5);
        quitButton.Name = "quitButton";
        quitButton.Size = new Size(129, 47);
        quitButton.TabIndex = 0;
        quitButton.Text = "Quit";
        quitButton.Click += QuitButton_Click;
        // 
        // backButton
        // 
        backButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        backButton.Location = new Point(423, 30);
        backButton.Margin = new Padding(4, 5, 4, 5);
        backButton.Name = "backButton";
        backButton.Size = new Size(129, 47);
        backButton.TabIndex = 1;
        backButton.Text = "< Back";
        backButton.Click += BackButton_Click;
        // 
        // cancelButton
        // 
        cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        cancelButton.Location = new Point(423, 30);
        cancelButton.Margin = new Padding(4, 5, 4, 5);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(129, 47);
        cancelButton.TabIndex = 2;
        cancelButton.Text = "Cancel";
        cancelButton.Visible = false;
        cancelButton.Click += CancelButton_Click;
        // 
        // nextButton
        // 
        nextButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        nextButton.Location = new Point(560, 30);
        nextButton.Margin = new Padding(4, 5, 4, 5);
        nextButton.Name = "nextButton";
        nextButton.Size = new Size(129, 47);
        nextButton.TabIndex = 3;
        nextButton.Text = "Next >";
        nextButton.Click += NextButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(714, 933);
        Controls.Add(welcomePage);
        Controls.Add(scanPage);
        Controls.Add(devicePage);
        Controls.Add(filterPage);
        Controls.Add(revisionPage);
        Controls.Add(confirmPage);
        Controls.Add(preparePage);
        Controls.Add(downloadPage);
        Controls.Add(updatePage);
        Controls.Add(resultPage);
        Controls.Add(navPanel);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(4, 5, 4, 5);
        MaximizeBox = false;
        MinimumSize = new Size(705, 896);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DigiTech RP Series Firmware Updater";
        welcomePage.ResumeLayout(false);
        welcomePage.PerformLayout();
        scanPage.ResumeLayout(false);
        scanPage.PerformLayout();
        devicePage.ResumeLayout(false);
        devicePage.PerformLayout();
        filterPage.ResumeLayout(false);
        filterPage.PerformLayout();
        revisionPage.ResumeLayout(false);
        revisionPage.PerformLayout();
        confirmPage.ResumeLayout(false);
        confirmPage.PerformLayout();
        preparePage.ResumeLayout(false);
        preparePage.PerformLayout();
        downloadPage.ResumeLayout(false);
        downloadPage.PerformLayout();
        updatePage.ResumeLayout(false);
        updatePage.PerformLayout();
        updateProgressBorder.ResumeLayout(false);
        resultPage.ResumeLayout(false);
        resultPage.PerformLayout();
        navPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel welcomePage;
    private System.Windows.Forms.Label welcomePageTitle;
    private System.Windows.Forms.Label welcomePageBody;
    private System.Windows.Forms.Panel scanPage;
    private System.Windows.Forms.Label scanPageTitle;
    private System.Windows.Forms.Label scanPageBody;
    private System.Windows.Forms.Label scanStatus;
    private System.Windows.Forms.Panel devicePage;
    private System.Windows.Forms.Label devicePageTitle;
    private System.Windows.Forms.Label devicePageBody;
    private System.Windows.Forms.ListBox deviceList;
    private System.Windows.Forms.Panel filterPage;
    private System.Windows.Forms.Label filterPageTitle;
    private System.Windows.Forms.Label filterPageBody;
    private System.Windows.Forms.CheckBox filterNewest;
    private System.Windows.Forms.CheckBox filterAll;
    private System.Windows.Forms.Panel revisionPage;
    private System.Windows.Forms.Label revisionPageTitle;
    private System.Windows.Forms.Label revisionPageBody;
    private System.Windows.Forms.ListBox revisionList;
    private System.Windows.Forms.Panel confirmPage;
    private System.Windows.Forms.Label confirmPageTitle;
    private System.Windows.Forms.Label confirmPageBody;
    private System.Windows.Forms.TextBox confirmText;
    private System.Windows.Forms.CheckBox confirmCheck;
    private System.Windows.Forms.CheckBox dryRunCheck;
    private System.Windows.Forms.Panel preparePage;
    private System.Windows.Forms.Label preparePageTitle;
    private System.Windows.Forms.Label prepareLabel;
    private System.Windows.Forms.Panel downloadPage;
    private System.Windows.Forms.Label downloadPageTitle;
    private System.Windows.Forms.Label downloadPageBody;
    private System.Windows.Forms.Label downloadStatus;
    private System.Windows.Forms.ProgressBar downloadProgress;
    private System.Windows.Forms.Panel updatePage;
    private System.Windows.Forms.Label updatePageTitle;
    private System.Windows.Forms.Label updatePageBody;
    private System.Windows.Forms.Label updateStatus;
    private System.Windows.Forms.ProgressBar updateProgress;
    private System.Windows.Forms.Panel updateProgressBorder;
    private System.Windows.Forms.Label updatePercentLabel;
    private System.Windows.Forms.Label updateSpeedLabel;
    private System.Windows.Forms.Label updateEtaLabel;
    private System.Windows.Forms.Panel resultPage;
    private System.Windows.Forms.Label resultTitle;
    private System.Windows.Forms.Label resultBody;
    private System.Windows.Forms.Label resultDetail;
    private System.Windows.Forms.Panel navPanel;
    private System.Windows.Forms.Button quitButton;
    private System.Windows.Forms.Button backButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button nextButton;
}
