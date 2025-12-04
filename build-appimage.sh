#!/bin/bash

# SweetTypeTone AppImage Build Script
# This script builds a universal AppImage for all Linux distributions

set -e  # Exit on error

VERSION="1.0.0"
ARCH="x86_64"
APP_NAME="SweetTypeTone"
APPDIR="${APP_NAME}.AppDir"

echo "🎵 Building ${APP_NAME} AppImage v${VERSION}..."
echo ""

# Check for required tools
if ! command -v appimagetool &> /dev/null; then
    echo "⚠️  appimagetool not found. Downloading..."
    wget -q https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-${ARCH}.AppImage -O appimagetool
    chmod +x appimagetool
    APPIMAGETOOL="./appimagetool"
else
    APPIMAGETOOL="appimagetool"
fi

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf ./publish/linux-x64
rm -rf ./${APPDIR}

# Publish the application
echo "📦 Publishing application..."
dotnet publish src/SweetTypeTone/SweetTypeTone.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish/linux-x64

echo ""
echo "✅ Build completed successfully!"
echo ""

# Create AppDir structure
echo "📁 Creating AppImage directory structure..."
mkdir -p ${APPDIR}/usr/bin
mkdir -p ${APPDIR}/usr/lib
mkdir -p ${APPDIR}/usr/share/applications
mkdir -p ${APPDIR}/usr/share/icons/hicolor/256x256/apps
mkdir -p ${APPDIR}/usr/share/BundledSoundPacks

# Copy application files
echo "📋 Copying application files..."
cp ./publish/linux-x64/SweetTypeTone ${APPDIR}/usr/bin/

# Copy shared libraries
echo "📚 Copying shared libraries..."
for lib in ./publish/linux-x64/*.so*; do
    if [ -f "$lib" ]; then
        cp "$lib" ${APPDIR}/usr/lib/
        echo "✅ Copied $(basename $lib)"
    fi
done

# Copy bundled sound packs
echo "🎹 Copying bundled sound packs..."
if [ -d "./BundledSoundPacks" ]; then
    cp -r ./BundledSoundPacks/* ${APPDIR}/usr/share/BundledSoundPacks/
    echo "✅ Copied $(ls -1 BundledSoundPacks | wc -l) sound packs"
else
    echo "⚠️  Warning: BundledSoundPacks directory not found"
fi

# Copy icon
echo "🎨 Copying application icon..."
if [ -f "src/SweetTypeTone/Assets/icons8-key-press-96.png" ]; then
    cp src/SweetTypeTone/Assets/icons8-key-press-96.png ${APPDIR}/sweettypetone.png
    cp src/SweetTypeTone/Assets/icons8-key-press-96.png ${APPDIR}/usr/share/icons/hicolor/256x256/apps/sweettypetone.png
    echo "✅ Icon copied"
else
    echo "⚠️  Warning: Icon file not found"
fi

# Create desktop file
echo "📝 Creating desktop file..."
cat > ${APPDIR}/sweettypetone.desktop << 'EOF'
[Desktop Entry]
Name=SweetTypeTone
Comment=Mechanical keyboard sound effects for Linux
Exec=sweettypetone
Icon=sweettypetone
Type=Application
Categories=Audio;Utility;
Terminal=false
StartupWMClass=SweetTypeTone
EOF

# Copy desktop file to applications
cp ${APPDIR}/sweettypetone.desktop ${APPDIR}/usr/share/applications/

# Create AppRun script
echo "🔧 Creating AppRun script..."
cat > ${APPDIR}/AppRun << 'EOF'
#!/bin/bash

# AppRun script for SweetTypeTone

APPDIR="$(dirname "$(readlink -f "$0")")"

# Set library path
export LD_LIBRARY_PATH="${APPDIR}/usr/lib:${LD_LIBRARY_PATH}"

# Set bundled sound packs path
export SWEETTYPETONE_BUNDLED_PACKS="${APPDIR}/usr/share/BundledSoundPacks"

# Run the application
exec "${APPDIR}/usr/bin/SweetTypeTone" "$@"
EOF

# Make AppRun executable
chmod +x ${APPDIR}/AppRun

# Build AppImage
echo ""
echo "🔨 Building AppImage..."
ARCH=${ARCH} ${APPIMAGETOOL} ${APPDIR} ${APP_NAME}-${VERSION}-${ARCH}.AppImage

echo ""
echo "✅ AppImage created successfully!"
echo ""
echo "📦 Output: ./${APP_NAME}-${VERSION}-${ARCH}.AppImage"
echo "📊 Size: $(du -h ${APP_NAME}-${VERSION}-${ARCH}.AppImage | cut -f1)"
echo ""
echo "🚀 To run the AppImage:"
echo "   chmod +x ${APP_NAME}-${VERSION}-${ARCH}.AppImage"
echo "   ./${APP_NAME}-${VERSION}-${ARCH}.AppImage"
echo ""
echo "📤 To distribute:"
echo "   Upload ${APP_NAME}-${VERSION}-${ARCH}.AppImage to GitHub Releases"
echo ""
echo "⚠️  Note: Users need to run setup-permissions.sh once for input access"
echo ""
