# DigiTech RP Series Firmware Updater

An open-source, reverse-engineered firmware updater for DigiTech RP-series and Lexicon USB-MIDI devices.

## Overview
This project is a modern, clean-room implementation of the original Harman NetUpdater 1.0.1. As legacy hardware ages, the original manufacturer servers and software often become unavailable or incompatible with modern operating systems. This tool ensures that owners of DigiTech and Lexicon equipment can continue to maintain, update, and restore their devices indefinitely.

## Features
- **Modern Architecture**: Built on modern .NET/C#, ensuring compatibility with current Windows environments.
- **Safe Test Mode**: Includes a non-destructive "Dry Run" feature that verifies USB-MIDI communication, handshakes, and flash mapping without writing any data to the device.
- **Standalone Operation**: Capable of using bundled firmware images and target XML files, removing the dependency on external Harman servers.
- **Protocol Documentation**: Includes a comprehensive, reverse-engineered specification of the Harman Super7 SysEx protocol (`NetUpdater_Protocol.md`).

## Supported Devices
Based on the original `UpdateTargets.xml`, this updater supports:
- **DigiTech RP Series**: RP150, RP155, RP250, RP255, RP350, RP355, RP500, RP1000
- **DigiTech Bass/Other**: BP355

## Usage
1. Connect your device via USB.
2. Launch the application.
3. The software will automatically scan your MIDI ports and identify connected hardware.
4. Select the target firmware version (the app defaults to the latest available).
5. Follow the on-screen prompts to flash the device.

**Important**: Do not disconnect power or USB during the flashing process. If a flash fails mid-write, the device will remain in bootloader mode. Simply leave it plugged in, restart the application, and run the update again.

## Building from Source
Open the solution in Visual Studio 2026 or later. The project targets modern .NET 10 and uses standard Windows Forms. No third-party dependencies are required for the core update engine.

## Disclaimer
This project is not affiliated with, endorsed by, or sponsored by Harman International, DigiTech, or Lexicon. Use this software at your own risk. While extensive care has been taken to ensure the flashing process is safe and matches the original OEM behavior, the authors are not responsible for bricked or damaged hardware.

## Screenshot
<img width="359" height="490" alt="image" src="https://github.com/user-attachments/assets/3f6df438-ef88-4d53-8180-d99861ae5964" />