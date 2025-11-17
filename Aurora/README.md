# Aurora — Run instructions (Linux)

Quick steps to build and run the Aurora web app locally on Linux.

Requirements
- .NET 8 SDK installed (tested with `dotnet --version` → `8.x`).
- Vivaldi browser installed (optional, used by the provided VS Code task).

Run locally
1. Open a terminal and change to the web project folder:

```bash
cd /home/yarin/Aurora/Aurora/Aurora
```

2. Restore, build and run:

```bash
dotnet restore
dotnet build
dotnet run
```

3. The app listens on the URL printed by `dotnet run` (by default this project uses `http://localhost:5158`).

Open in browser
- From terminal (one-off):

```bash
vivaldi --new-window http://localhost:5158
```

- From VS Code: press F5. The workspace includes a VS Code task `Open Vivaldi` that opens `http://localhost:5158` before launching.
  - Task file: `.vscode/tasks.json`
  - Launch config: `.vscode/launch.json` (uses `preLaunchTask` to open Vivaldi).

Custom port
- To change the port, edit `Properties/launchSettings.json` `applicationUrl` and update `.vscode/tasks.json` accordingly, or run with an environment variable:

```bash
ASPNETCORE_URLS=http://localhost:5000 dotnet run
```

Notes
- The solution references `MySql.Data` and the `DBmanagment` project. If you need database functionality, ensure your MySQL server and connection strings are configured in code or `appsettings.json`.
- The project targets `net8.0`.

Stopping the app
- Press `Ctrl+C` in the terminal where `dotnet run` is running.

If you want, I can:
- Disable the built-in browser launch in `Properties/launchSettings.json` and rely only on the VS Code task, or
- Make the VS Code task port configurable.
