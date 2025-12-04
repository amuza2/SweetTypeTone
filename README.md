
# 🎵 SweetTypeTone


[![Build](https://github.com/amuza2/SweetTypeTone/actions/workflows/build.yml/badge.svg)](https://github.com/amuza2/SweetTypeTone/actions/workflows/build.yml) [![Release](https://github.com/amuza2/SweetTypeTone/actions/workflows/release.yml/badge.svg)](https://github.com/amuza2/SweetTypeTone/actions/workflows/release.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/) [![Platform](https://img.shields.io/badge/Platform-Linux-orange)](https://www.linux.org/) [![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-FF5E5B?logo=ko-fi)](https://ko-fi.com/codingisamazing)

  

A modern Linux application that brings mechanical keyboard sounds to your typing experience. Built with Avalonia UI and .NET 10.

  

![SweetTypeTone Demo](https://via.placeholder.com/800x400?text=SweetTypeTone+Screenshot)

  

## ✨ Features

  

- 🎹 **Real-time Sound Playback** - Keyboard sounds as you type

- 🎨 **Modern UI** - Beautiful gradient interface with system tray support

- 📦 **Mechvibes Compatible** - Import existing sound packs (OGG/WAV)

- 🔊 **Volume Control** - Adjustable volume with mute toggle

- ⚡ **High Performance** - Parallel loading, OpenAL audio engine

- 🐧 **Linux Native** - Built for Linux with evdev input monitoring

  

## 📥 Installation

  

### Download Release (Recommended)

  

Download the latest release from [Releases](https://github.com/amuza2/SweetTypeTone/releases):

  

```bash

# Download and extract

wget  https://github.com/amuza2/SweetTypeTone/releases/latest/download/SweetTypeTone-Linux-x64.tar.gz

tar  -xzf  SweetTypeTone-Linux-x64.tar.gz

cd  SweetTypeTone

  

# Setup permissions (one-time)

./setup-permissions.sh

  

# Log out and log back in, then run

./SweetTypeTone

```

  

### Build from Source

  

```bash

git  clone  https://github.com/amuza2/SweetTypeTone.git

cd  SweetTypeTone

dotnet  build  -c  Release

dotnet  run  --project  src/SweetTypeTone/SweetTypeTone.csproj

```

  

**Requirements:** .NET 10 SDK, Linux with evdev support
  

### Sound Packs

  

Copy sound packs (OGG/WAV) to `~/.config/SweetTypeTone/CustomSoundPacks/` and click refresh.

  

**Note:** MP3 format not supported. Convert with: `ffmpeg -i input.mp3 output.ogg`

  

## 🛠️ Tech Stack

  

-  **[Avalonia UI](https://avaloniaui.net/)** - Cross-platform UI framework

-  **[OpenAL](https://www.openal.org/)** - High-performance audio engine

-  **[NVorbis](https://github.com/NVorbis/NVorbis)** - OGG Vorbis decoder

-  **Linux evdev** - Native input monitoring

-  **.NET 10** - Modern runtime with trimming support

  

## 🤝 Contributing

  

Contributions welcome! See [CONTRIBUTING.md](.github/CONTRIBUTING.md) for guidelines.

  

1. Fork the repository

2. Create your feature branch (`git checkout -b feature/amazing-feature`)

3. Commit your changes (`git commit -m 'feat: add amazing feature'`)

4. Push to the branch (`git push origin feature/amazing-feature`)

5. Open a Pull Request

  

## 📝 License

  

MIT License - see [LICENSE](LICENSE) for details.

  

## 🙏 Acknowledgments

  

- Inspired by [Mechvibes](https://github.com/hainguyents13/mechvibes)

- Sound packs from the mechanical keyboard community

  

## 💬 Support

  

- 🐛 [Report a Bug](https://github.com/amuza2/SweetTypeTone/issues/new?template=bug_report.yml)

- 💡 [Request a Feature](https://github.com/amuza2/SweetTypeTone/issues/new?template=feature_request.yml)

- ☕ [Support on Ko-fi](https://ko-fi.com/codingisamazing)

  

---

  

Made with ❤️ for the Linux community
