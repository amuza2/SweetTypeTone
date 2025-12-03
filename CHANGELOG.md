# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Parallel loading for sound packs (3-4x faster loading)
- Proper WAV file format validation and support
- Disabled state for unsupported sound packs in UI
- Sorting: supported packs first, then alphabetically
- Refresh button to reload sound packs without restart
- Window icon matching tray icon
- Comprehensive GitHub workflows (build, release)
- Issue templates (bug report, feature request)
- Pull request template
- Contributing guidelines
- Code of Conduct

### Changed
- Improved WAV loader with proper chunk parsing
- Optimized console output during loading
- Removed unused dependencies (NAudio, duplicate packages)
- Removed unused commands from ViewModel

### Fixed
- WAV file loading issues with NK Cream sound pack
- Sorting lost when clicking refresh button
- Sound packs not reloading on refresh
- Missing window icon

## [1.0.0] - YYYY-MM-DD

### Added
- Initial release
- Cross-platform keyboard sound effects for Linux
- Support for Mechvibes sound packs (OGG, WAV)
- System tray integration
- Modern Avalonia UI
- Volume control and mute functionality
- Sound pack management
- Settings persistence
- OpenAL-based audio engine
- Linux evdev input monitoring

### Supported Platforms
- Linux (Ubuntu, Debian, Fedora, Arch, etc.)

[Unreleased]: https://github.com/amuza2/SweetTypeTone/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/amuza2/SweetTypeTone/releases/tag/v1.0.0
