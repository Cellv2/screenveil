# ScreenVeil

ScreenVeil creates independent floating covers for masking screen content while studying. This repository currently contains the Phase 1 desktop prototype.

## Run

Requires the .NET 10 SDK.

```sh
dotnet run --project ScreenVeil.App/ScreenVeil.App.csproj
```

## Run as a Windows app from WSL

When launched as a Linux GUI app through WSLg, Windows controls how that window is stacked relative to native Windows apps, and may not honor the Linux topmost request. To have covers stay above Windows apps, publish a native Windows build in WSL, copy it to a Windows folder, and launch it there. This publishes a self-contained x64 app into the WSL filesystem:

```sh
dotnet publish ScreenVeil.App/ScreenVeil.App.csproj \
  -c Release -r win-x64 --self-contained true \
  -o ./publish/win-x64
cd ./publish/win-x64
explorer.exe .
```

In Explorer, copy the published folder to a location on a Windows drive, then run `ScreenVeil.App.exe` from that copy. The Windows build uses Avalonia's native Windows window management for the overlay's topmost setting.

## Prototype Features

- Create multiple independent, borderless, always-on-top cover windows.
- Drag covers to move them, resize from any edge or corner, and rotate with the top handle.
- Choose black, white, moss, or coral for each cover.
- Show or hide covers individually, show or hide all, and delete individual covers.
- Keep the control window focused when covers are shown where the desktop window manager honors non-activating windows.

Cover state is currently in memory only. Closing the app discards the arrangement.

## Verification

```sh
dotnet test ScreenVeil.sln
```

## Platform Notes

The app uses Avalonia windowing for its shared UI and native window manager operations for movement, resizing, and topmost behavior. Linux X11 is the initial target. Wayland compositors may restrict absolute positioning, always-on-top windows, global shortcuts, and click-through behavior; these capabilities need testing per compositor and are not implemented yet. In WSLg, a Linux app's topmost request may not keep it above native Windows apps; use the native Windows build above for that behavior.

Study mode, locking, persistence, workspaces, custom color entry, opacity, global shortcuts, click-through, and tray integration are planned for later phases.