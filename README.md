# Smoothie Installer
An all in one installer for Smoothie.

## Features
- Download & install the latest version of Smoothie.

- Install FFmpeg if required.

- Repair your current Smoothie installation if required.

    - You may use this option to update Smoothie also.

- Clean uninstallation, no files are left behind.

## Usage
- Download the latest version from [GitHub Releases](https://github.com/Aetopia/SmoothieInstaller/releases/latest).
- Run `SmoothieInstaller.exe`.

> [!TIP]
> Smoothie Installer is intended for those who aren't familiar with package managers.<br>
> If possible, try to transition to [Scoop](https://scoop.sh) for Smoothie.<br>
> Package managers provide a clean way to install, update and uninstall apps.<br><br>
> If you want to transition, do the following:
> - If you have used Smoothie Installer, do the following:
>   - Backup any configuration files, if desired.
>   - Uninstall Smoothie.
>
> - Run the following commands in PowerShell:
>   ```powershell
>   irm "get.scoop.sh" | iex
>   scoop install git ffmpeg
>   scoop bucket add utils "https://github.com/couleur-tweak-tips/utils"
>   scoop install smoothie
>   ```
> - You may update FFmpeg and Smoothie using the following command:
>   ```powershell
>   scoop update ffmpeg smoothie
>   ```

## FAQ
- How can I update Smoothie using Smoothie Installer?<br>

    - You can update Smoothie by repairing your current Smoothie installation by re-running `SmoothieInstaller.exe`.
    - You can update Smoothie by modifying/changing the installation:

        - Control Panel

            - Go to Programs → Programs & Features.

            - Select Smoothie.

            - Select <kbd>Change</kbd>.

            - Repair the installation.
        
        - Windows Settings
            - Go to Apps → Apps & features.
            - Select Smoothie.
            - Select <kbd>Modify</kbd>.
            - Repair the installation.
        
- How can I use Smoothie?
    - Please check out [Smoothie's documentation](https://ctt.cx/video/smoothie/).

- How can I uninstall Smoothie using Smoothie Installer?
    - You may uninstall Smoothie via the Control Panel or Windows Settings.

- How can I remove Smoothie Installer's FFmpeg installation?
    - Do the following:

        - Go the following directory, `%LOCALAPPDATA%\Programs\Smoothie\bin`.

        - Delete the following:
            - `ffmpeg.exe`

            - `ffprobe.exe`

            - `ffplay.exe`

- Where does Smoothie Installer install Smoothie?
    - Smoothie Installer installs Smoothie to `%LOCALAPPDATA%\Programs\Smoothie`.

## Building
1. Download the following:
    - [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    - [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net481-developer-pack-offline-installer)

2. Run the following command to compile:
    ```cmd
    dotnet publish "src/SmoothieInstaller.csproj"
    ```