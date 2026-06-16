using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UpWindowsApp;

public partial class MainWindow : Window
{
    // -------------------------------------------------------
    // Modelo de Bloatware
    // -------------------------------------------------------
    private record BloatwareItem(string Id, string DisplayName, string Pattern);

    private static readonly List<BloatwareItem> BloatwareList = new()
    {
        new("Clipchamp",    "Clipchamp (Editor de Video)",          "*clipchamp*"),
        new("Cortana",      "Cortana",                              "*549981C3F5F10*"),
        new("FeedbackHub",  "Hub de Feedback",                      "*feedbackhub*"),
        new("GetHelp",      "Obter Ajuda",                          "*gethelp*"),
        new("Maps",         "Mapas",                                "*windowsmaps*"),
        new("MixedReality", "Portal de Realidade Misturada",        "*mixedreality.portal*"),
        new("News",         "Noticias (Bing)",                      "*bingnews*"),
        new("Weather",      "Clima (Bing)",                         "*bingweather*"),
        new("OneNote",      "OneNote para Windows",                 "*onenote*"),
        new("People",       "Pessoas",                              "*people*"),
        new("Solitaire",    "Microsoft Solitaire Collection",       "*solitairecollection*"),
        new("StickyNotes",  "Sticky Notes",                        "*microsoftstickynotes*"),
        new("OfficeHub",    "Portal do Office (Microsoft 365)",     "*microsoftofficehub*"),
        new("MoviesTV",     "Filmes e TV",                          "*zunevideo*"),
        new("GrooveMusic",  "Media Player Antigo (Zune Music)",     "*zunemusic*"),
        new("Widgets",      "Widgets do Windows (WebExperience)",   "*WebExperience*"),
        new("Xbox",         "Xbox Apps (Todos)",                    "*xbox*"),
        new("Spotify",      "Spotify Stub",                        "*spotify*"),
        new("TikTok",       "TikTok Stub",                         "*tiktok*"),
        new("Instagram",    "Instagram Stub",                      "*instagram*"),
        new("Disney",       "Disney+ Stub",                        "*disney*"),
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
        Log("UpWindows App v1.0.0 carregado.");
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
    private readonly Button[] _sidebarButtons = Array.Empty<Button>();

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
        try
        {
            var os = System.Environment.OSVersion;
            TxtWinEdition.Text = $"Windows {os.Version.Major}.{os.Version.Minor} (Build {os.Version.Build})";

            // Status de ativacao via WMI
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT LicenseStatus, Name FROM SoftwareLicensingProduct " +
                "WHERE ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL");

            foreach (System.Management.ManagementObject obj in searcher.Get())
            {
                var status = Convert.ToInt32(obj["LicenseStatus"]);
                TxtWinActivation.Text = status == 1 ? "Ativado (Licenciado)" : "Nao Ativado";
                TxtWinActivation.Foreground = status == 1
                    ? new SolidColorBrush(Colors.LimeGreen)
                    : new SolidColorBrush(Colors.OrangeRed);
                break;
            }
        }
        catch (Exception ex)
        {
            TxtWinEdition.Text    = "Windows 10/11";
            TxtWinActivation.Text = $"Erro ao verificar: {ex.Message}";
        }
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
                // Habilitar protecao do sistema via WMI
                using var mc = new System.Management.ManagementClass("SystemRestore");
                mc.Scope = new System.Management.ManagementScope(@"\\localhost\root\default");
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
                inParams2["Description"]    = "UpWindows App Optimizations";
                inParams2["RestorePointType"] = 12; // MODIFY_SETTINGS
                inParams2["EventType"]      = 100;  // BEGIN_SYSTEM_CHANGE
                var result = mc.InvokeMethod("CreateRestorePoint", inParams2, null);
                Log("[+] Ponto de Restauracao criado com sucesso!");

                // Restaurar valor original de frequencia
                if (oldVal != null)
                    key?.SetValue("SystemRestorePointCreationFrequency", oldVal, RegistryValueKind.DWord);
                else
                    key?.DeleteValue("SystemRestorePointCreationFrequency", throwOnMissingValue: false);

                Log("[-] Frequencia de criacao de pontos restaurada ao padrao.");
            }
            catch (Exception ex)
            {
                Log($"[!] FALHA ao criar Ponto de Restauracao: {ex.Message}");
                Log("[!] Verifique: Painel de Controle > Sistema > Protecao do Sistema > Unidade C:\\");
            }
            await Task.CompletedTask;
        });
    }

    private void BtnExit_Click(object s, RoutedEventArgs e) => Close();

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
                Content   = app.DisplayName,
                Tag       = app.Pattern,
                IsChecked = true,
                Margin    = new Thickness(5, 4, 0, 4),
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
        // Captura padroes antes de entrar na thread
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
            await Task.CompletedTask;
        });
    }

    // -------------------------------------------------------
    // Tab Privacidade / Telemetria
    // -------------------------------------------------------
    private void BtnApplyTweaks_Click(object s, RoutedEventArgs e)
    {
        bool telemetry  = ChkTelemetry.IsChecked  == true;
        bool bing       = ChkBingSearch.IsChecked  == true;
        bool copilot    = ChkCopilot.IsChecked     == true;
        bool recall     = ChkRecall.IsChecked      == true;
        bool widgets    = ChkWidgets.IsChecked     == true;
        bool ads        = ChkWindowsAds.IsChecked  == true;

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
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0);
                Log("[+] Telemetria desativada.");
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
                SetRegistryDWord(@"HKCU\Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffUserActivityTracker", 1);
                SetRegistryDWord(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "TurnOffUserActivityTracker", 1);
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
                Log("[+] Anuncios desativados.");
            }

            Log("[+] Tweaks de Privacidade concluidos.");
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
            const string clsidPath = @"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";
            if (classicMenu)
            {
                using var key = CreateOrOpenKey(clsidPath);
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
                FileName  = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"irm https://get.activated.win | iex\"",
                UseShellExecute = true,
                Verb = "runas"
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
        int idx = fullPath.IndexOf('\\');
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
            // Altera o tipo de inicializacao via sc.exe (ServiceController nao expoe essa API diretamente)
            Process.Start(new ProcessStartInfo
            {
                FileName  = "sc.exe",
                Arguments = $"config {serviceName} start= disabled",
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
            FileName  = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            CreateNoWindow  = true
        })?.WaitForExit();
    }
}
