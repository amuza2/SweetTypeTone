#!/bin/bash

# Script to update the desktop file and icon for existing installation

set -e

echo "🔧 Updating SweetTypeTone desktop file and icon..."
echo ""

SHARE_DIR="$HOME/.local/share"
DESKTOP_FILE="$SHARE_DIR/applications/sweettypetone.desktop"

# Copy icon to multiple sizes for better compatibility
if [ -f "src/SweetTypeTone/Assets/icons8-key-press-96.png" ]; then
    echo "🎨 Installing application icon in multiple sizes..."
    for size in 48 64 96 128 256; do
        ICON_DIR="$SHARE_DIR/icons/hicolor/${size}x${size}/apps"
        mkdir -p "$ICON_DIR"
        cp src/SweetTypeTone/Assets/icons8-key-press-96.png "$ICON_DIR/sweettypetone.png"
    done
    echo "✅ Icon installed in sizes: 48x48, 64x64, 96x96, 128x128, 256x256"
else
    echo "⚠️  Warning: Icon file not found at src/SweetTypeTone/Assets/icons8-key-press-96.png"
fi

# Create/update desktop file
echo "📝 Creating desktop file..."
cat > "$DESKTOP_FILE" << 'EOF'
[Desktop Entry]
Version=1.1
Type=Application
Name=SweetTypeTone
Comment=Play keyboard sounds as you type
Exec=SweetTypeTone
Icon=sweettypetone
Terminal=false
Keywords=keyboard;sound;typing;mechanical;
Categories=AudioVideo;Audio;
StartupNotify=false
EOF

echo "✅ Desktop file created: $DESKTOP_FILE"

# Update desktop database
if command -v update-desktop-database &> /dev/null; then
    echo "🔄 Updating desktop database..."
    update-desktop-database "$SHARE_DIR/applications" 2>/dev/null || true
fi

# Update icon cache
if command -v gtk-update-icon-cache &> /dev/null; then
    echo "🔄 Updating icon cache..."
    gtk-update-icon-cache -f -t "$SHARE_DIR/icons/hicolor" 2>/dev/null || true
fi

echo ""
echo "✅ Update complete!"
echo ""
echo "📁 Desktop file: $DESKTOP_FILE"
echo "🎨 Icon: $ICON_FILE"
echo ""
echo "The application should now appear in your application menu with the correct icon."
echo "You may need to log out and log back in for changes to take effect."
echo ""
