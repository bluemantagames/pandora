#!/usr/bin/env bash

set -e
set -x

export BUILD_TARGET=Android
export BUILD_NAME=Pandora

mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/

cat Unity_v2019.x.ulf | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf

echo "Building for $BUILD_TARGET"

cd ..

export BUILD_PATH=./Builds/$BUILD_TARGET/

mkdir -p $BUILD_PATH

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -quit \
  -batchmode \
  -buildTarget $BUILD_TARGET \
  -customBuildTarget $BUILD_TARGET \
  -customBuildName $BUILD_NAME \
  -customBuildPath $BUILD_PATH \
  -password "$UNITY_PASSWORD" \
  -customBuildOptions AcceptExternalModificationsToPlayer \
  -executeMethod BuildCommand.PerformBuild \
  -logFile -

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

ls -la $BUILD_PATH
[ -n "$(ls -A $BUILD_PATH)" ] # fail job if build folder is empty

cd Builds/Android/Pandora/Pandora
gradle assembleRelease

cd /root/project

cp Builds/Android/Pandora/Pandora/build/outputs/apk/release/*.apk Pandora.apk
