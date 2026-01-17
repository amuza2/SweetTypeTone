<div align="center">

  
  ![Sweet Type Tone Logo](https://github.com/amuza2/SweetTypeTone/blob/main/src/Assets/icons8-key-press-96.png)
# 🎵 SweetTypeTone


[![Build](https://github.com/amuza2/SweetTypeTone/actions/workflows/build.yml/badge.svg)](https://github.com/amuza2/SweetTypeTone/actions/workflows/build.yml) [![Release](https://github.com/amuza2/SweetTypeTone/actions/workflows/release.yml/badge.svg)](https://github.com/amuza2/SweetTypeTone/actions/workflows/release.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/) [![Platform](https://img.shields.io/badge/Platform-Linux-orange)](https://www.linux.org/) [![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-FF5E5B?logo=ko-fi)](https://ko-fi.com/codingisamazing)


A modern Linux application that brings mechanical keyboard sounds to your typing experience. Built with Avalonia UI and .NET 10.
 

<img width="30%" height="40%" alt="image" src="https://github.com/user-attachments/assets/e9524f5e-9b6f-4851-9e6a-bc0ee80ca1b6" />

</div> 
  

## ✨ Features

  

- 🎹 **Real-time Sound Playback** - Keyboard sounds as you type

- 🎨 **Modern UI** - Beautiful gradient interface with system tray support

- 📦 **Mechvibes Compatible** - Import existing sound packs (OGG/WAV)

- 🔊 **Volume Control** - Adjustable volume with mute toggle

- ⚡ **High Performance** - Parallel loading, OpenAL audio engine

- 🐧 **Linux Native** - Built for Linux with evdev input monitoring

  

## 📥 Installation

  

### Download AppImage (Recommended)

Download the latest AppImage from [Releases](https://github.com/amuza2/SweetTypeTone/releases):

1. **Download** `SweetTypeTone-x.x.x-x86_64.AppImage`
2. **Make it executable**: `chmod +x SweetTypeTone-*.AppImage`
3. **Double-click to run**
4. **First run**: A dialog will ask to configure permissions - click "Yes" and enter your password
5. **Log out and log back in**
6. **Run again** - Enjoy!

**✨ Includes 20+ pre-installed sound packs!** No installation needed. Works on Ubuntu, Fedora, Arch, and all major Linux distributions.

  

### Download Binary Archive

Alternative installation method:

1. **Download** `SweetTypeTone-x.x.x-linux-x64.tar.gz`
2. **Extract**: `tar -xzf SweetTypeTone-*.tar.gz`
3. **Run installer**: `./install.sh` (optional, installs to ~/.local/bin)
4. **Or run directly**: `./SweetTypeTone`

**✨ Includes 20+ pre-installed sound packs!**

  

### Build from Source

  

```bash

git  clone  https://github.com/amuza2/SweetTypeTone.git

cd  SweetTypeTone

# Build AppImage with bundled sound packs
./scripts/build-appimage.sh

# Or build binary archive
./scripts/build-binary.sh

# Or just build for development
dotnet  build  -c  Release
dotnet  run  --project  src/SweetTypeTone.csproj

```

  

**Requirements:** .NET 10 SDK, Linux with evdev support
  

### 🎹 Bundled Sound Packs

Both AppImage and binary releases include **20+ pre-installed sound packs**:

**Add custom packs**: Copy sound packs (OGG/WAV) to `~/.config/SweetTypeTone/CustomSoundPacks/` and click refresh.
  

## 🛠️ Tech Stack

  

-  **[Avalonia UI](https://avaloniaui.net/)** - Cross-platform UI framework

-  **[OpenAL](https://www.openal.org/)** - High-performance audio engine

-  **[NVorbis](https://github.com/NVorbis/NVorbis)** - OGG Vorbis decoder

-  **[NLayer](https://github.com/naudio/NLayer)** - MP3 decoder

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

  
<div align="center">
Made with ❤️ for the Linux community
</div>
