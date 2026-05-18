#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${UNITY_PATH_6000_3_10:-}" ]]; then
  echo "ERROR: UNITY_PATH_6000_3_10 environment variable is not set."
  echo "Example: export UNITY_PATH_6000_3_10=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity"
  exit 1
fi

if [[ ! -f "${UNITY_PATH_6000_3_10}" ]]; then
  echo "ERROR: UNITY_PATH_6000_3_10 does not point to a valid file: \"${UNITY_PATH_6000_3_10}\""
  exit 1
fi

if ! command -v 7z >/dev/null 2>&1; then
  echo "ERROR: 7z was not found in PATH."
  exit 1
fi

if ! command -v butler >/dev/null 2>&1; then
  echo "ERROR: butler was not found in PATH."
  exit 1
fi

ITCH_CHANNEL="grisha-gu/dice-miner:html5"
PROJECT_NAME="DiceMiner"

ITCH_VERSION="${2:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${SCRIPT_DIR}"
BUILD_DIR="${PROJECT_PATH}/Build/WebGL/${PROJECT_NAME}"
ARCHIVE_PATH="${PROJECT_PATH}/Build/WebGL/${PROJECT_NAME}.zip"
LOG_PATH="${PROJECT_PATH}/Build/WebGL/${PROJECT_NAME}_build.log"

echo
echo "[1/4] Building WebGL via Unity..."
"${UNITY_PATH_6000_3_10}" -batchmode -quit -nographics \
  -projectPath "${PROJECT_PATH}" \
  -executeMethod GrishaGuWorkshop.BuildTool.BuildFromCommandLine \
  -buildOutput "${BUILD_DIR}" \
  -logFile "${LOG_PATH}" || {
    echo "ERROR: Unity build failed. Check log: \"${LOG_PATH}\""
    exit 1
  }

echo
echo "[2/4] Compressing build with 7z..."
if [[ -f "${ARCHIVE_PATH}" ]]; then
  rm -f "${ARCHIVE_PATH}"
fi
7z a -tzip "${ARCHIVE_PATH}" "${BUILD_DIR}"/* || {
  echo "ERROR: 7z compression failed."
  exit 1
}

echo
echo "[3/4] Uploading archive to itch.io with butler..."
if [[ -z "${ITCH_VERSION}" ]]; then
  butler push "${ARCHIVE_PATH}" "${ITCH_CHANNEL}" || {
    echo "ERROR: Butler upload failed."
    exit 1
  }
else
  butler push "${ARCHIVE_PATH}" "${ITCH_CHANNEL}" --userversion "${ITCH_VERSION}" || {
    echo "ERROR: Butler upload failed."
    exit 1
  }
fi

echo
echo "[4/4] Cleaning Build/WebGL folder..."
if [[ -d "${PROJECT_PATH}/Build/WebGL" ]]; then
  rm -rf "${PROJECT_PATH}/Build/WebGL"/*
fi

echo
echo "Done."
echo "Uploaded: \"${ARCHIVE_PATH}\" -> \"${ITCH_CHANNEL}\""
exit 0
