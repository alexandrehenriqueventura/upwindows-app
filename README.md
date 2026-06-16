# UpWindows App

Versão nativa em **C# WPF** do [UpWindows](https://github.com/alexandrehenriqueventura/upwindows), compilada como `.exe` standalone sem dependência de PowerShell.

## Funcionalidades

- Desativar Telemetria e Diagnósticos (DiagTrack, CEIP)
- Desativar Bing Search, Copilot, Windows Recall, Widgets e Anúncios
- Remover Bloatware nativo (AppX/Provisioned packages)
- Ajustes de Interface: Menu Clássico, Alinhamento da Taskbar, Arquivos Ocultos, Extensões
- Ajustes de Sistema: Fast Startup, Otimização de Entrega de Updates
- Criar Ponto de Restauração do Windows
- Ativação via Microsoft Activation Scripts (MAS)

## Requisitos

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (apenas para build)
- Executar como **Administrador** (manifesto UAC embutido)

## Build

```bash
# Restaurar dependências
dotnet restore

# Compilar em Release
dotnet build -c Release

# Publicar como .exe único e portátil (sem .NET instalado na máquina alvo)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

O executável final estará em `./publish/UpWindowsApp.exe`.

## Estrutura do Projeto

```
upwindows-app/
├── UpWindowsApp.sln
└── UpWindowsApp/
    ├── UpWindowsApp.csproj
    ├── app.manifest
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    └── Modules/
        ├── TelemetryModule.cs
        ├── PrivacyModule.cs
        ├── BloatwareModule.cs
        ├── UIModule.cs
        ├── SystemModule.cs
        ├── SystemRestoreModule.cs
        └── ActivationModule.cs
```

## Diferenças em relação ao script PowerShell

| Aspecto | PowerShell (.ps1) | C# WPF (.exe) |
|---|---|---|
| Distribuição | Requer PowerShell 5+ | `.exe` standalone |
| UI | WPF via Add-Type | WPF nativo |
| Async | Threads manuais STA | `async/await` + `Task.Run` |
| Acesso ao Registro | `Set-ItemProperty` | `Microsoft.Win32.Registry` |
| Serviços | `Stop-Service` | `System.ServiceProcess` |
| AppX | `Remove-AppxPackage` | `Windows.Management.Deployment` |

## Licença

MIT
