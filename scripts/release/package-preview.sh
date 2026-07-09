#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-0.1.0-preview.1}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PACKAGE_NAME="OpenCashFlow-${VERSION}"
RELEASE_DIR="${ROOT_DIR}/artifacts/releases"
STAGING_DIR="${RELEASE_DIR}/${PACKAGE_NAME}"
ARCHIVE_PATH="${RELEASE_DIR}/${PACKAGE_NAME}.tar.gz"
RELEASE_NOTES="${ROOT_DIR}/Docs/releases/${VERSION}.md"

if [[ ! -f "${RELEASE_NOTES}" ]]; then
  echo "Missing release notes: ${RELEASE_NOTES}" >&2
  exit 1
fi

rm -rf "${STAGING_DIR}" "${ARCHIVE_PATH}"
mkdir -p "${STAGING_DIR}/Docs"

copy_file() {
  local source="$1"
  local destination="$2"

  if [[ ! -f "${ROOT_DIR}/${source}" ]]; then
    echo "Missing required file: ${source}" >&2
    exit 1
  fi

  mkdir -p "$(dirname "${STAGING_DIR}/${destination}")"
  cp "${ROOT_DIR}/${source}" "${STAGING_DIR}/${destination}"
}

copy_dir() {
  local source="$1"
  local destination="$2"

  if [[ ! -d "${ROOT_DIR}/${source}" ]]; then
    echo "Missing required directory: ${source}" >&2
    exit 1
  fi

  mkdir -p "$(dirname "${STAGING_DIR}/${destination}")"
  cp -R "${ROOT_DIR}/${source}" "${STAGING_DIR}/${destination}"
}

copy_file "docker-compose.yml" "docker-compose.yml"
copy_file "docker-compose.release.yml" "docker-compose.release.yml"
copy_file ".env.example" ".env.example"
copy_file "README.md" "README.md"
copy_file "LICENSE" "LICENSE"
copy_file "CHANGELOG.md" "CHANGELOG.md"
copy_dir "Docs/setup" "Docs/setup"
copy_dir "Docs/ops" "Docs/ops"
copy_file "Docs/releases/${VERSION}.md" "Docs/releases/${VERSION}.md"

(
  cd "${RELEASE_DIR}"
  tar -czf "${ARCHIVE_PATH}" "${PACKAGE_NAME}"
)

echo "${ARCHIVE_PATH}"
