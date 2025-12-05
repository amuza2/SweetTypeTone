#!/bin/bash
# SweetTypeTone Installation Script
# This script installs SweetTypeTone with proper permissions

set -e

INSTALL_DIR="/opt/sweettypetone"
BIN_DIR="/usr/local/bin"
POLICY_DIR="/usr/share/polkit-1/actions"
DESKTOP_DIR="/usr/share/applications"

echo "=== SweetTypeTone Installation ==="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "This script must be run as root (use sudo)"
    exit 1
fi

# Get the actual user (not root when using sudo)
ACTUAL_USER="${SUDO_USER:-$USER}"
ACTUAL_HOME=$(eval echo ~$ACTUAL_USER)

# Find dotnet - prefer user's installation
DOTNET_CMD="dotnet"
if [ -f "$ACTUAL_HOME/.dotnet/dotnet" ]; then
    DOTNET_CMD="$ACTUAL_HOME/.dotnet/dotnet"
    echo "Using .NET from: $DOTNET_CMD"
elif command -v dotnet &> /dev/null; then
    DOTNET_CMD="dotnet"
    echo "Using system .NET"
else
    echo "Error: .NET SDK not found"
    exit 1
fi

# Show .NET version
$DOTNET_CMD --version

# Build the application
echo "Building SweetTypeTone..."
cd "$(dirname "$0")"

# Build as the actual user to avoid permission issues and use correct .NET
sudo -u "$ACTUAL_USER" $DOTNET_CMD publish src/SweetTypeTone/SweetTypeTone.csproj -c Release -r linux-x64 --self-contained false -o build/

# Create installation directory
echo "Creating installation directory..."
mkdir -p "$INSTALL_DIR"
cp -r build/* "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/SweetTypeTone"

# Install helper script
echo "Installing permission setup helper..."
cp install/sweettypetone-setup "$BIN_DIR/"
chmod 755 "$BIN_DIR/sweettypetone-setup"

# Install polkit policy
echo "Installing polkit policy..."
mkdir -p "$POLICY_DIR"
cp install/sweettypetone.policy "$POLICY_DIR/"

# Create launcher script
echo "Creating launcher script..."
cat > "$BIN_DIR/sweettypetone" << 'EOF'
#!/bin/bash
# SweetTypeTone Launcher

# Check if user has input group access
if ! groups | grep -q '\binput\b'; then
    echo "Setting up permissions (this is a one-time setup)..."
    
    # Check if pkexec is available
    if command -v pkexec &> /dev/null; then
        pkexec /usr/local/bin/sweettypetone-setup "$USER"
        echo ""
        echo "============================================"
        echo "Permissions configured successfully!"
        echo ""
        echo "IMPORTANT: You must log out and log back in"
        echo "for the changes to take effect."
        echo ""
        echo "After logging back in, run: sweettypetone"
        echo "============================================"
        exit 0
    else
        echo "============================================"
        echo "Manual setup required:"
        echo ""
        echo "Run the following commands:"
        echo "  sudo usermod -a -G input \$USER"
        echo "  sudo tee /etc/udev/rules.d/99-sweettypetone.rules > /dev/null << 'RULES'"
        echo "KERNEL==\"event*\", SUBSYSTEM==\"input\", MODE=\"0660\", GROUP=\"input\""
        echo "RULES"
        echo "  sudo udevadm control --reload-rules"
        echo "  sudo udevadm trigger"
        echo ""
        echo "Then log out and log back in."
        echo "============================================"
        exit 1
    fi
fi

# Run the application
exec /opt/sweettypetone/SweetTypeTone "$@"
EOF

chmod 755 "$BIN_DIR/sweettypetone"

# Create desktop entry
echo "Creating desktop entry..."
cat > "$DESKTOP_DIR/sweettypetone.desktop" << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=SweetTypeTone
Comment=Play keyboard sounds as you type
Exec=/usr/local/bin/sweettypetone
Icon=SweetTypeTone
Terminal=false
Categories=AudioVideo;Audio;
StartupNotify=true
EOF

echo ""
echo "=== Installation Complete ==="
echo ""
echo "To run SweetTypeTone:"
echo "  1. Type 'sweettypetone' in terminal, or"
echo "  2. Find 'SweetTypeTone' in your application menu"
echo ""
echo "On first run, you'll be prompted to setup permissions."
echo "After granting permission, you must log out and log back in."
echo ""
