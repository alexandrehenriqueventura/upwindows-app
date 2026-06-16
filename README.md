# UpWindows App

> Versão nativa em **C# WPF** do [UpWindows](https://github.com/alexandrehenriqueventura/upwindows) — compilada como `.exe` standalone, sem dependência de PowerShell.

![Versão](https://img.shields.io/badge/vers%C3%A3o-1.1.0-8B5CF6?style=flat-square)
![Plataforma](https://img.shields.io/badge/plataforma-Windows%2010%2F11-0078D4?style=flat-square)
![Licença](https://img.shields.io/badge/licen%C3%A7a-GPL%20v3-green?style=flat-square)

---

## Funcionalidades

### Dashboard Principal
- Exibição da **Edição do Windows** e **Status de Ativação**
- **Tempo de Atividade (Uptime)** do sistema em tempo real
- Informações de **Hardware**: Processador (CPU), Placa de Vídeo (GPU) e Memória RAM
- **Armazenamento** da unidade `C:\` com barra de progresso visual
- **Ações Rápidas**: Limpar Temporários, Esvaziar Lixeira, Limpar Cache DNS

### Privacidade e Telemetria
- Desativar Telemetria e Diagnósticos (DiagTrack)
- Desativar CEIP (Customer Experience Improvement Program)
- Desativar Bing Search no Menu Iniciar
- Desativar Microsoft Copilot (IA Integrada)
- Desativar Windows Recall e rastreadores de IA
- Desativar Painel de Widgets (Notícias e Interesses)
- Desativar Anúncios Personalizados e Dicas do Windows

### Remover Bloatware
- Remoção de pacotes AppX e Provisioned nativos do Windows
- Lista expandida com Xbox (4 entradas granulares), Skype, Copilot App e outros
- Seleção/deseleção em massa

### Ajustes de Interface
- Restaurar Menu de Contexto Clássico (estilo Windows 10)
- Alinhar Barra de Tarefas à Esquerda
- Mostrar pastas e arquivos ocultos
- Mostrar extensões de arquivos conhecidos

### Ajustes de Sistema
- Desativar Fast Startup
- Limitar compartilhamento P2P de Updates (Delivery Optimization)

### Segurança e Ativação
- Criar **Ponto de Restauração** (verifica e inicia o serviço VSS automaticamente)
- Ativação via **Microsoft Activation Scripts (MAS)**

---

## Requisitos

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — apenas para build
- Executar como **Administrador** (manifesto UAC embutido)

---

## Build

### 1. Instalar o .NET 8 SDK (via winget)

Caso ainda não tenha o .NET 8 SDK instalado, execute no terminal como Administrador:

```bash
winget install Microsoft.DotNet.SDK.8
```

Após a instalação, feche e reabra o terminal para que o comando `dotnet` seja reconhecido. Verifique com:

```bash
dotnet --version
```

### 2. Compilar o projeto

**a) Baixar o repositório**

Acesse a página do projeto no GitHub, clique em **Code → Download ZIP** e salve o arquivo em um local de sua preferência.

**b) Descompactar o pacote**

Clique com o botão direito no arquivo `.zip` baixado e selecione **Extrair tudo...**. Escolha o destino e conclua a extração.

**c) Abrir o terminal dentro da pasta**

Navegue até a pasta extraída (`upwindows-app-main`), clique na **barra de endereço** do Explorador de Arquivos, digite `cmd` ou `powershell` e pressione **Enter**. O terminal já abrirá apontando para o diretório correto.

> **Dica:** você também pode usar `Shift + Clique Direito` dentro da pasta e selecionar **Abrir janela do PowerShell aqui**.

**d) Executar os comandos de build**

```bash
# Restaurar dependências
dotnet restore

# Compilar em Release
dotnet build -c Release

# Publicar como .exe único e portátil (sem .NET instalado na máquina alvo)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

O executável final estará em `./publish/UpWindowsApp.exe`.

---

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

---

## Diferenças em relação ao script PowerShell

| Aspecto | PowerShell (`.ps1`) | C# WPF (`.exe`) |
|---|---|---|
| Distribuição | Requer PowerShell 5+ | `.exe` standalone |
| UI | WPF via `Add-Type` | WPF nativo |
| Async | Threads manuais STA | `async/await` + `Task.Run` |
| Acesso ao Registro | `Set-ItemProperty` | `Microsoft.Win32.Registry` |
| Serviços | `Stop-Service` | `System.ServiceProcess` |
| AppX | `Remove-AppxPackage` | `Windows.Management.Deployment` |
| Dashboard | Informações básicas | CPU, GPU, RAM, Disco, Uptime |
| Notificações | Log no console | Toast animado na interface |

---

## Licença

Este projeto é distribuído sob a licença **GNU General Public License v3.0 (GPL-3.0)**.

Você tem liberdade para usar, estudar, modificar e distribuir este software, desde que qualquer trabalho derivado seja disponibilizado sob a mesma licença.

Consulte o arquivo [LICENSE](./LICENSE) ou acesse [gnu.org/licenses/gpl-3.0](https://www.gnu.org/licenses/gpl-3.0) para mais detalhes.
