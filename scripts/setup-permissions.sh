#!/bin/bash

# SweetTypeTone Permission Setup Script
# This script creates a udev rule to allow access to input devices

echo "Setting up permissions for SweetTypeTone..."

# Create udev rule
sudo tee /etc/udev/rules.d/99-sweettypetone.rules > /dev/null << EOF
# Allow users in the input group to access input devices
KERNEL=="event*", SUBSYSTEM=="input", MODE="0660", GROUP="input"
EOF

echo "✓ udev rule created"

# Add user to input group if not already a member
if ! groups $USER | grep -q input; then
    echo "Adding $USER to input group..."
    sudo usermod -a -G input $USER
    echo "✓ User added to input group"
else
    echo "✓ User already in input group"
fi

# Reload udev rules
echo "Reloading udev rules..."
sudo udevadm control --reload-rules
sudo udevadm trigger

echo ""
echo "================================================"
echo "Setup complete!"
echo ""
echo "IMPORTANT: You must log out and log back in"
echo "for the group membership to take effect."
echo ""
echo "Alternatively, run the app with sudo for testing:"
echo "  sudo dotnet run"
echo "================================================"
