using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UpWindowsApp;

public partial class MainWindow : Window
{
    // -------------------------------------------------------
    // Modelo de Bloatware
    // -------------------------------------------------------
    private record BloatwareItem(string Id, string DisplayName, string Pattern);

    private static readonly List<BloatwareItem> BloatwareList = new()
    {
        new("Clipchamp",           "Clipchamp (Editor de Video)",           "*clipchamp*"),
        new("Cortana",             "Cortana",                               "*549981C3F5F10*"),
        new("FeedbackHub",         "Hub de Feedback",                       "*feedbackhub*"),
        new("GetHelp",             "Obter Ajuda",                           "*gethelp*"),
        new("Maps",                "Mapas",                                 "*windowsmaps*"),
        new("MixedReality",        "Portal de Realidade Misturada",         "*mixedreality.portal*"),
        new("News",                "Noticias (Bing)",                       "*bingnews*"),
        new("Weather",             "Clima (Bing)",                          "*bingweather*"),
        new("OneNote",             "OneNote para Windows",                  "*onenote*"),
        new("People",              "Pessoas",                               "*Microsoft.People*"),
        new("Solitaire",           "Microsoft Solitaire Collection",        "*solitairecollection*"),
        new("Skype",               "Skype",                                 "*skype*"),
        new("StickyNotes",         "Sticky Notes (Notas Autoadesivas)",     "*microsoftstickynotes*"),
        new("OfficeHub",           "Portal do Office (Microsoft 365)",      "*microsoftofficehub*"),
        new("MoviesTV",            "Filmes e TV (Zune Video)",              "*zunevideo*"),
        new("GrooveMusic",         "Media Player Antigo (Zune Music)",      "*zunemusic*"),
        new("Widgets",             "Widgets do Windows (WebExperience)",    "*WebExperience*"),
        new("XboxApp",             "Xbox App",                              "*Microsoft.GamingApp*"),
        new("XboxGamingOverlay",   "Xbox Game Bar",                         "*Microsoft.XboxGamingOverlay*"),
        new("XboxIdentity",        "Xbox Identity Provider",                "*Microsoft.XboxIdentityProvider*"),
        new("XboxSpeech",          "Xbox Speech to Text",                   "*Microsoft.XboxSpeechToTextOverlay*"),
        new("Copilot",             "Microsoft Copilot (App)",               "*Microsoft.Copilot*"),
        new("Spotify",             "Spotify Stub",                          "*spotify*"),
        new("TikTok",              "TikTok Stub",                           "*tiktok*"),
        new("Instagram",           "Instagram Stub",                        "*instagram*"),
        new("Disney",              "Disney+ Stub",                          "*disney*"),
    };

    // -------------------------------------------------------
    // Construtor
    // -------------------------------------------------------
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SwitchTab(0);
        Log("UpWindows App v1.1.0 carregado.");
        LoadSystemInfo();
        LoadBloatwareList();
        Log("Pronto para otimizacoes.");
    }

    // -------------------------------------------------------
    // Helpers: Log + Async
    // -------------------------------------------------------
    private void Log(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        Dispatcher.Invoke(() =>
        {
            TxtLog.AppendText($"[{ts}] {message}\r\n");
            TxtLog.ScrollToEnd();
        });
    }

    /// <summary>
    /// Executa <paramref name="action"/> em background (Task.Run) e
    /// reabilita <paramref name="button"/> ao concluir.
    /// </summary>
    private void RunAsync(Button button, Func<Task> action)
    {
        button.IsEnabled = false;
        _ = Task.Run(async () =>
        {
            try   { await action(); }
            catch (Exception ex) { Log($"[!] Erro inesperado: {ex.Message}"); }
            finally { Dispatcher.Invoke(() => button.IsEnabled = true); }
        });
    }

    // -------------------------------------------------------
    // Navegacao sidebar
    // -------------------------------------------------------
    private void SwitchTab(int index)
    {
        MainTabControl.SelectedIndex = index;
        Button[] btns = { BtnHome, BtnTweaks, BtnApps, BtnUI, BtnSystem, BtnActivation };
        foreach (var btn in btns)
        {
            btn.Background = Brushes.Transparent;
            btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));
        }
        btns[index].Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A35"));
        btns[index].Foreground = Brushes.White;
    }

    private void BtnHome_Click(object s, RoutedEventArgs e)       => SwitchTab(0);
    private void BtnTweaks_Click(object s, RoutedEventArgs e)     => SwitchTab(1);
    private void BtnApps_Click(object s, RoutedEventArgs e)       => SwitchTab(2);
    private void BtnUI_Click(object s, RoutedEventArgs e)         => SwitchTab(3);
    private void BtnSystem_Click(object s, RoutedEventArgs e)     => SwitchTab(4);
    private void BtnActivation_Click(object s, RoutedEventArgs e) => SwitchTab(5);

    // -------------------------------------------------------
    // Dashboard: informacoes do sistema
    // -------------------------------------------------------
    private void LoadSystemInfo()
    {
        Task.Run(() =>
        {
            try
            {
                // Edicao do Windows
                var os = System.Environment.OSVersion;
                string edition = $"Windows {os.Version.Major}.{os.Version.Minor} (Build {os.Version.Build})";

                // Status de ativacao via WMI
                string activation = "Nao verificado";
                bool isActivated = false;
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        "SELECT LicenseStatus, Name FROM SoftwareLicensingProduct " +
                        "WHERE ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var status = Convert.ToInt32(obj["LicenseStatus"]);
                        isActivated = status == 1;
                        activation = isActivated ? "Ativado (Licenciado)" : "Nao Ativado";
                        break;
                    }
                }
                catch { activation = "Erro ao verificar"; }

                // CPU
                string cpu = "Desconhecido";
                try
                {
                    using var s = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                    foreach (ManagementObject o in s.Get()) { cpu = o["Name"]?.ToString()?.Trim() ?? "Desconhecido"; break; }
                }
                catch { }

                // GPU
                string gpu = "Desconhecido";
                try
                {
                    using var s = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                    foreach (ManagementObject o in s.Get()) { gpu = o["Name"]?.ToString()?.Trim() ?? "Desconhecido"; break; }
                }
                catch { }

                // RAM
                string ram = "Desconhecido";
                try
                {
                    long totalBytes = 0;
                    using var s = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
                    foreach (ManagementObject o in s.Get())
                        totalBytes += Convert.ToInt64(o["Capacity"]);
                    if (totalBytes > 0)
                        ram = $"{Math.Round((double)totalBytes / (1024 * 1024 * 1024), 1)} GB";
                }
                catch { }

                // Disco C:
                string diskFree = "---";
                string diskTotal = "---";
                double diskUsedPct = 0;
                try
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady && drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase))
                        {
                            double free  = Math.Round((double)drive.AvailableFreeSpace  / (1024 * 1024 * 1024), 1);
                            double total = Math.Round((double)drive.TotalSize           / (1024 * 1024 * 1024), 1);
                            diskFree  = $"{free} GB";
                            diskTotal = $"{total} GB";
                            if (total > 0) diskUsedPct = Math.Round((total - free) / total * 100, 0);
                            break;
                        }
                    }
                }
                catch { }

                // Uptime
                string uptime = "N/A";
                try
                {
                    using var s = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
                    foreach (ManagementObject o in s.Get())
                    {
                        var lastBoot = ManagementDateTimeConverter.ToDateTime(o["LastBootUpTime"].ToString()!);
                        var ts = DateTime.Now - lastBoot;
                        uptime = $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
                        break;
                    }
                }
                catch { }

                Dispatcher.Invoke(() =>
                {
                    TxtWinEdition.Text    = edition;
                    TxtWinActivation.Text = activation;
                    TxtWinActivation.Foreground = isActivated
                        ? new SolidColorBrush(Colors.LimeGreen)
                        : new SolidColorBrush(Colors.OrangeRed);

                    if (TxtUptime     != null) TxtUptime.Text     = uptime;
                    if (TxtCpu        != null) TxtCpu.Text        = cpu;
                    if (TxtGpu        != null) TxtGpu.Text        = gpu;
                    if (TxtRam        != null) TxtRam.Text        = ram;
                    if (TxtStorageFree  != null) TxtStorageFree.Text  = $"Livre: {diskFree}";
                    if (TxtStorageTotal != null) TxtStorageTotal.Text  = $"Total: {diskTotal}";
                    if (ProgStorage   != null) ProgStorage.Value  = diskUsedPct;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    TxtWinEdition.Text    = "Windows 10/11";
                    TxtWinActivation.Text = $"Erro ao verificar: {ex.Message}";
                });
            }
        });
    }

    // -------------------------------------------------------
    // Dashboard: Ponto de Restauracao
    // -------------------------------------------------------
    private void BtnCreateRestore_Click(object s, RoutedEventArgs e)
    {
        RunAsync(BtnCreateRestore, async () =>
        {
            Log("[-] Iniciando criacao do Ponto de Restauracao...");
            try
            {
                // Verificar servico VSS
                try
                {
                    using var vss = new ServiceController("VSS");
                    if (vss.Status != ServiceControllerStatus.Running)
                    {
                        Log("[-] Servico VSS nao ativo. Tentando iniciar...");
                        vss.Start();
                        vss.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    }
                }
                catch { Log("[!] Aviso: nao foi possivel verificar o servico VSS."); }

                // Habilitar protecao do sistema via WMI
                using var mc = new ManagementClass("SystemRestore");
                mc.Scope = new ManagementScope(@"\\localhost\root\default");
                var inParams = mc.GetMethodParameters("Enable");
                inParams["Drive"] = "C:\\";
                mc.InvokeMethod("Enable", inParams, null);
                Log("[+] Protecao do sistema verificada/habilitada em C:\\.");

                // Remover limite de frequencia temporariamente
                const string srPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";
                using var key = Registry.LocalMachine.OpenSubKey(srPath, writable: true);
                var oldVal = key?.GetValue("SystemRestorePointCreationFrequency");
                key?.SetValue("SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);
                Log("[-] Limite de frequencia de criacao de pontos removido temporariamente.");

                // Criar ponto de restauracao via WMI
                Log("[-] Solicitando criacao do ponto ao Windows (pode levar alguns segundos)...");
                var inParams2 = mc.GetMethodParameters("CreateRestorePoint");
                inParams2["Description"]      = "UpWindows App Optimizations";
                inParams2["RestorePointType"] = 12;  // MODIFY_SETTINGS
                inParams2["EventType"]        = 100; // BEGIN_SYSTEM_CHANGE
                mc.InvokeMethod("CreateRestorePoint", inParams2, null);
                Log("[+] Ponto de Restauracao criado com sucesso! Descricao: 'UpWindows App Optimizations'.");

                // Restaurar valor original de frequencia
                if (oldVal != null)
                    key?.SetValue("SystemRestorePointCreationFrequency", oldVal, RegistryValueKind.DWord);
                else
                    key?.DeleteValue("SystemRestorePointCreationFrequency", throwOnMissingValue: false);

                Log("[-] Frequencia de criacao de pontos restaurada ao padrao.");
                Dispatcher.Invoke(() => ShowToast("Concluido", "Ponto de Restauracao criado com sucesso!", "Descricao: UpWindows App Optimizations", true));
            }
            catch (Exception ex)
            {
                Log($"[!] FALHA ao criar Ponto de Restauracao: {ex.Message}");
                Log("[!] Verifique: Painel de Controle > Sistema > Protecao do Sistema > Unidade C:\\");
                Dispatcher.Invoke(() => ShowToast("Falha", "Nao foi possivel criar o ponto de restauracao.", ex.Message, false));
            }
            await Task.CompletedTask;
        });
    }

    private void BtnExit_Click(object s, RoutedEventArgs e) => Close();

    // -------------------------------------------------------
    // Dashboard: Acoes Rapidas
    // -------------------------------------------------------
    private void BtnCleanTemp_Click(object s, RoutedEventArgs e)
    {
        RunAsync(BtnCleanTemp, async () =>
        {
            Log("[-] Limpando arquivos temporarios...");
            int count = 0;
            string[] tempPaths =
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            };
            foreach (var dir in tempPaths)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var file in Directory.GetFiles(dir))
                {
                    try { File.Delete(file); count++; } catch { }
                }
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    try { Directory.Delete(sub, recursive: true); count++; } catch { }
                }
            }
            Log($"[+] Limpeza concluida. {count} item(ns) removido(s).");
            Dispatcher.Invoke(() => ShowToast("Limpeza Concluida", $"{count} item(ns) temporario(s) removido(s).", "", true));
            await Task.CompletedTask;
        });
    }

    private void BtnEmptyTrash_Click(object s, RoutedEventArgs e)
    {
        RunAsync(BtnEmptyTrash, async () =>
        {
            Log("[-] Esvaziando Lixeira...");
            try
            {
                RunPowerShell("Clear-RecycleBin -Force -ErrorAction SilentlyContinue");
                Log("[+] Lixeira esvaziada com sucesso.");
                Dispatcher.Invoke(() => ShowToast("Concluido", "Lixeira esvaziada com sucesso.", "", true));
            }
            catch (Exception ex)
            {
                Log($"[!] Erro ao esvaziar a Lixeira: {ex.Message}");
            }
            await Task.CompletedTask;
        });
    }

    private void BtnFlushDNS_Click(object s, RoutedEventArgs e)
    {
        RunAsync(BtnFlushDNS, async () =>
        {
            Log("[-] Limpando cache DNS...");
            try
            {
                using var proc = Process.Start(new ProcessStartInfo
                {
                    FileName        = "ipconfig.exe",
                    Arguments       = "/flushdns",
                    UseShellExecute = false,
                    CreateNoWindow  = true
                });
                proc?.WaitForExit();
                Log("[+] Cache DNS limpo com sucesso.");
                Dispatcher.Invoke(() => ShowToast("Concluido", "Cache DNS limpo com sucesso.", "", true));
            }
            catch (Exception ex)
            {
                Log($"[!] Erro ao limpar cache DNS: {ex.Message}");
            }
            await Task.CompletedTask;
        });
    }

    // -------------------------------------------------------
    // Toast de Resultado
    // -------------------------------------------------------
    private DispatcherTimer? _toastTimer;

    private void ShowToast(string title, string body, string detail, bool isSuccess)
    {
        ToastIcon.Text  = isSuccess ? "✅" : "⚠️";
        ToastTitle.Text = title;
        ToastTitle.Foreground = isSuccess
            ? Brushes.White
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
        ToastBody.Text   = body;
        ToastDetail.Text = detail;
        ToastPanel.BorderBrush = isSuccess
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));

        var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
        ToastPanel.BeginAnimation(OpacityProperty, fadeIn);

        _toastTimer?.Stop();
        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _toastTimer.Tick += (_, _) =>
        {
            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            ToastPanel.BeginAnimation(OpacityProperty, fadeOut);
            _toastTimer!.Stop();
        };
        _toastTimer.Start();
    }

    // -------------------------------------------------------
    // Tab Bloatware
    // -------------------------------------------------------
    private void LoadBloatwareList()
    {
        LstApps.Items.Clear();
        foreach (var app in BloatwareList)
        {
            var cb = new CheckBox
            {
                Content    = app.DisplayName,
                Tag        = app.Pattern,
                IsChecked  = true,
                Margin     = new Thickness(5, 4, 0, 4),
                Foreground = Brushes.White
            };
            LstApps.Items.Add(cb);
        }
    }

    private void BtnSelectAll_Click(object s, RoutedEventArgs e)
    {
        foreach (CheckBox cb in LstApps.Items) cb.IsChecked = true;
    }

    private void BtnDeselectAll_Click(object s, RoutedEventArgs e)
    {
        foreach (CheckBox cb in LstApps.Items) cb.IsChecked = false;
    }

    private void BtnApplyApps_Click(object s, RoutedEventArgs e)
    {
        var patterns = new List<string>();
        foreach (CheckBox cb in LstApps.Items)
            if (cb.IsChecked == true && cb.Tag is string p)
                patterns.Add(p);

        RunAsync(BtnApplyApps, async () =>
        {
            Log($"[-] Iniciando remocao de {patterns.Count} app(s)...");
            foreach (var pattern in patterns)
            {
                Log($"[-] Removendo: {pattern}");
                RunPowerShell(
                    $"Get-AppxPackage -Name '{pattern}' -AllUsers | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue; " +
                    $"Get-AppxProvisionedPackage -Online | Where-Object {{ $_.PackageName -like '{pattern}' }} | " +
                    $"Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue"
                );
            }
            Log("[+] Remocao de aplicativos concluida.");
            Dispatcher.Invoke(() => ShowToast("Bloatware Removido", $"{patterns.Count} app(s) processado(s).", "", true));
            await Task.CompletedTask;
        });
    }

    // -------------------------------------------------------
    // Tab Privacidade / Telemetria
    // -------------------------------------------------------
    private void BtnApplyTweaks_Click(object s, RoutedEventArgs e)
    {
        bool telemetry = ChkTelemetry.IsChecked  == true;
        bool ceip      = ChkCEIP.IsChecked        == true;
        bool bing      = ChkBingSearch.IsChecked  == true;
        bool copilot   = ChkCopilot.IsChecked     == true;
        bool recall    = ChkRecall.IsChecked      == true;
        bool widgets   = ChkWidgets.IsChecked     == true;
        bool ads       = ChkWindowsAds.IsChecked  == true;

        RunAsync(BtnApplyTweaks, async () =>
        {
            Log("[-] Aplicando tweaks de Privacidade e Telemetria...");

            if (telemetry)
            {
                Log("[-] Desativando Telemetria (DiagTrack)...");
                StopAndDisableService("DiagTrack");
                StopAndDisableService("dmwappushservice");
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MaxTelemetryAllowed", 0);
                Log("[+] Telemetria desativada.");
            }

            if (ceip)
            {
                Log("[-] Desativando CEIP...");
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0);
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\SQMClient",         "CEIPEnable", 0);
                Log("[+] CEIP desativado.");
            }

            if (bing)
            {
                Log("[-] Desativando Bing Search...");
                SetRegistryDWord(@"HKCU\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0);
                Log("[+] Bing Search desativado.");
            }

            if (copilot)
            {
                Log("[-] Desativando Copilot...");
                SetRegistryDWord(@"HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
                Log("[+] Copilot desativado.");
            }

            if (recall)
            {
                Log("[-] Desativando Windows Recall...");
                SetRegistryDWord(@"HKCU\Software\Policies\Microsoft\Windows\WindowsAI",  "TurnOffUserActivityTracker", 1);
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "TurnOffUserActivityTracker", 1);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\AI\Settings", "UserActivityTrackerState", 0);
                Log("[+] Windows Recall desativado.");
            }

            if (widgets)
            {
                Log("[-] Desativando Widgets...");
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0);
                Log("[+] Widgets desativados.");
            }

            if (ads)
            {
                Log("[-] Desativando anuncios do Windows...");
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0);
                SetRegistryDWord(@"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0);
                Log("[+] Anuncios desativados.");
            }

            Log("[+] Tweaks de Privacidade concluidos.");
            Dispatcher.Invoke(() => ShowToast("Privacidade Aplicada", "Tweaks de privacidade e telemetria aplicados.", "", true));
            await Task.CompletedTask;
        });
    }

    // -------------------------------------------------------
    // Tab Interface (UI Tweaks)
    // -------------------------------------------------------
    private void BtnApplyUI_Click(object s, RoutedEventArgs e)
    {
        bool classicMenu = ChkClassicMenu.IsChecked    == true;
        bool taskbarLeft = ChkTaskbarLeft.IsChecked    == true;
        bool showHidden  = ChkShowHidden.IsChecked     == true;
        bool showExt     = ChkShowExtensions.IsChecked == true;

        RunAsync(BtnApplyUI, async () =>
        {
            Log("[-] Aplicando tweaks de interface...");

            const string explorerAdv = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

            // Menu de Contexto Classico
            if (classicMenu)
            {
                using var key = CreateOrOpenKey(@"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32");
                key?.SetValue("", "", RegistryValueKind.String);
                Log("[+] Menu de Contexto Classico ativado.");
            }
            else
            {
                DeleteRegistryKey(@"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
                Log("[+] Menu de Contexto padrao restaurado.");
            }

            // Alinhamento da Taskbar
            SetRegistryDWord(explorerAdv, "TaskbarAl", taskbarLeft ? 0 : 1);
            Log($"[+] Taskbar alinhada: {(taskbarLeft ? "Esquerda" : "Centro")}.");

            // Arquivos ocultos
            SetRegistryDWord(explorerAdv, "Hidden", showHidden ? 1 : 2);
            Log($"[+] Arquivos ocultos: {(showHidden ? "Visiveis" : "Ocultos")}.");

            // Extensoes
            SetRegistryDWord(explorerAdv, "HideFileExt", showExt ? 0 : 1);
            Log($"[+] Extensoes: {(showExt ? "Visiveis" : "Ocultas")}.");

            // Reiniciar Explorer
            Log("[-] Reiniciando explorer.exe...");
            foreach (var p in Process.GetProcessesByName("explorer")) p.Kill();
            await Task.Delay(1200);
            if (Process.GetProcessesByName("explorer").Length == 0)
                Process.Start("explorer.exe");
            Log("[+] Explorer reiniciado.");

            Log("[+] Ajustes de interface concluidos.");
            Dispatcher.Invoke(() => ShowToast("Interface Atualizada", "Ajustes de interface aplicados.", "Explorer reiniciado.", true));
        });
    }

    // -------------------------------------------------------
    // Tab Sistema
    // -------------------------------------------------------
    private void BtnApplySystem_Click(object s, RoutedEventArgs e)
    {
        bool fastStartup = ChkDisableFastStartup.IsChecked == true;
        bool delivery    = ChkOptimizeDelivery.IsChecked   == true;

        RunAsync(BtnApplySystem, async () =>
        {
            Log("[-] Aplicando ajustes de sistema...");

            if (fastStartup)
            {
                SetRegistryDWord(
                    @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power",
                    "HiberbootEnabled", 0);
                Log("[+] Fast Startup desativado.");
            }

            if (delivery)
            {
                SetRegistryDWord(
                    @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization",
                    "DODownloadMode", 1);
                Log("[+] Delivery Optimization configurado (somente LAN).");
            }

            Log("[+] Ajustes de sistema concluidos.");
            Dispatcher.Invoke(() => ShowToast("Sistema Configurado", "Ajustes de sistema aplicados com sucesso.", "", true));
            await Task.CompletedTask;
        });
    }

    // -------------------------------------------------------
    // Tab Ativacao (MAS)
    // -------------------------------------------------------
    private void BtnRunMAS_Click(object s, RoutedEventArgs e)
    {
        RunAsync(BtnRunMAS, async () =>
        {
            Log("[-] Iniciando Microsoft Activation Scripts (MAS)...");
            var psi = new ProcessStartInfo
            {
                FileName        = "powershell.exe",
                Arguments       = "-NoProfile -ExecutionPolicy Bypass -Command \"irm https://get.activated.win | iex\"",
                UseShellExecute = true,
                Verb            = "runas"
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit();
            Log("[+] MAS finalizado. Verificando status de ativacao...");
            LoadSystemInfo();
        });
    }

    // -------------------------------------------------------
    // Helpers de Registro e Servicos
    // -------------------------------------------------------

    /// <summary>Define um valor DWORD no registro, criando a chave se necessario.</summary>
    private static void SetRegistryDWord(string fullPath, string name, int value)
    {
        var (hive, subKey) = SplitRegistryPath(fullPath);
        using var key = hive.CreateSubKey(subKey, writable: true);
        key?.SetValue(name, value, RegistryValueKind.DWord);
    }

    /// <summary>Abre ou cria uma chave de registro para escrita.</summary>
    private static RegistryKey? CreateOrOpenKey(string fullPath)
    {
        var (hive, subKey) = SplitRegistryPath(fullPath);
        return hive.CreateSubKey(subKey, writable: true);
    }

    /// <summary>Remove uma chave de registro recursivamente.</summary>
    private static void DeleteRegistryKey(string fullPath)
    {
        var (hive, subKey) = SplitRegistryPath(fullPath);
        hive.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
    }

    /// <summary>Separa o hive (HKLM, HKCU) do caminho da subchave.</summary>
    private static (RegistryKey hive, string subKey) SplitRegistryPath(string fullPath)
    {
        int idx     = fullPath.IndexOf('\\');
        var hiveStr = fullPath[..idx].ToUpperInvariant();
        var sub     = fullPath[(idx + 1)..];
        RegistryKey hive = hiveStr switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER"  => Registry.CurrentUser,
            "HKCR" or "HKEY_CLASSES_ROOT"  => Registry.ClassesRoot,
            _ => throw new ArgumentException($"Hive desconhecido: {hiveStr}")
        };
        return (hive, sub);
    }

    /// <summary>Para e desabilita um servico Windows.</summary>
    private void StopAndDisableService(string serviceName)
    {
        try
        {
            using var svc = new ServiceController(serviceName);
            if (svc.Status == ServiceControllerStatus.Running)
            {
                svc.Stop();
                svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
            Process.Start(new ProcessStartInfo
            {
                FileName        = "sc.exe",
                Arguments       = $"config {serviceName} start= disabled",
                UseShellExecute = false,
                CreateNoWindow  = true
            })?.WaitForExit();
        }
        catch (Exception ex)
        {
            Log($"[!] Aviso ao parar servico '{serviceName}': {ex.Message}");
        }
    }

    /// <summary>Executa um comando PowerShell em background sem exibir janela.</summary>
    private static void RunPowerShell(string command)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName        = "powershell.exe",
            Arguments       = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            CreateNoWindow  = true
        })?.WaitForExit();
    }
}
