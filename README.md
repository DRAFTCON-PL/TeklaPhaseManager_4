# TeklaPhaseManager TPM

External phase visibility manager for **Tekla Structures 2022** — control phase visibility, transparency, colors, and active phase from a standalone Windows app without opening the built-in Phase Manager.

Developed by **[DRAFTCON.PL](https://www.draftcon.pl)**

---

## Features

- List all model phases with object count, color, visibility, and transparency
- Set active phase (reflected immediately in Tekla)
- Toggle visibility and transparency per phase or for all phases at once
- Change phase color via color picker
- Select all objects belonging to one or more phases
- Filter phases by name or comment
- Sort by any column
- Save / load visibility presets (JSON)
- Bidirectional sync: changes in Tekla Phase Manager update TPM and vice versa
- Windows-light UI, DPI-aware (PerMonitorV2)

---

## Requirements

| Requirement | Version |
|---|---|
| Tekla Structures | 2022 (API 2022.0) |
| .NET Framework | 4.8 |
| Windows | 10 / 11 |

> The app must be run while Tekla Structures is open with a model loaded.

---

## Download

Pre-built executable: [`release/TeklaPhaseManager_4.exe`](release/TeklaPhaseManager_4.exe)

No installer needed — just run the exe.

---

## Macro: +TeklaRedrawView

Some features (phase color and view filter application) require the `+TeklaRedrawView` macro to be installed in Tekla.

### What it does
- Applies the `+TPM_kolory` representation and `+TPM_widocznosc` view filter to all open views
- Called automatically by TPM after visibility/color changes

### Installation

1. Copy [`macros/+TeklaRedrawView.cs`](macros/+TeklaRedrawView.cs) to your Tekla macros folder:
   ```
   C:\TeklaStructures\2022.0\Macros\modeling\
   ```
   or the user macros path shown in Tekla under **Tools → Macros**.

2. In Tekla Structures go to **Tools → Macros**, find `+TeklaRedrawView` and click **Run** once to verify it works.

TPM will call it automatically via `Operation.RunMacro()` after applying changes.

---

## Screenshots

![Main window](screenshots/main.png)

---

## Building from source

Requires **Visual Studio 2022** with .NET Framework 4.8 workload and Tekla Structures 2022 installed.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
  TeklaPhaseManager_4.csproj /p:Configuration=Release
```

---

## License

Freeware — free to use, not for redistribution without permission.

© DRAFTCON.PL — [www.draftcon.pl](https://www.draftcon.pl)
