#!/bin/bash +x
# ./build.sh v1.1.1 "$HOME/Downloads/What Lurks Below (v1.1.1)"

NAME="What Lurks Below"

# Check if version number is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <version-number>"
    exit 1
fi

VERSION=$1
PROJECT_PATH=$(pwd)
if [ -z "$2" ]; then
    BUILD_PATH="$PROJECT_PATH/Builds"
else
    BUILD_PATH="$2"
fi

# Ensure Unity is installed and accessible
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.42f1/Unity.app/Contents/MacOS/Unity"

if [ ! -f "$UNITY_PATH" ]; then
    echo "Unity executable not found at $UNITY_PATH"
    exit 1
fi

# Create builds directory
mkdir -p "$BUILD_PATH"

# macOS
"$UNITY_PATH" -quit -batchmode -nographics -clean \
    -buildTarget osxuniversal \
    -projectPath "$PROJECT_PATH" \
    -buildOSXUniversalPlayer "$BUILD_PATH/$NAME.app"

rm -rf "$BUILD_PATH/${NAME}_BurstDebugInformation_DoNotShip"
cd "$BUILD_PATH" && zip -r "$NAME (macOS, $VERSION).zip" "$NAME.app"
rm -rf "$BUILD_PATH/$NAME.app"

# Linux
"$UNITY_PATH" -quit -batchmode -nographics -clean \
    -projectPath "$PROJECT_PATH" \
    -buildTarget linux64 \
    -buildLinux64Player "$BUILD_PATH/$NAME/$NAME"

cd "$BUILD_PATH" && zip -r "$NAME (Linux, $VERSION).zip" "$NAME"
rm -rf "$BUILD_PATH/$NAME"

# Windows
"$UNITY_PATH" -quit -batchmode -nographics -clean \
    -buildTarget win64 \
    -projectPath "$PROJECT_PATH" \
    -buildWindows64Player "$BUILD_PATH/$NAME/$NAME.exe"

cd "$BUILD_PATH" && zip -r "$NAME (Windows, $VERSION).zip" "$NAME"
rm -rf "$BUILD_PATH/$NAME"

# WebGL
"$UNITY_PATH" -quit -batchmode -nographics -clean \
    -projectPath "$PROJECT_PATH" \
    -buildPath "$BUILD_PATH/$NAME" \
    -buildTarget webgl \
    -executeMethod WebGLBuilder.BuildGame

cd "$BUILD_PATH/$NAME" && zip -r "$BUILD_PATH/$NAME (WebGL, $VERSION).zip" *
rm -rf "$BUILD_PATH/$NAME"
