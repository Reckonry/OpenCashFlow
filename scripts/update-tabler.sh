#!/usr/bin/env bash
set -euo pipefail

# -----------------------------
# Config (adatta solo se serve)
# -----------------------------
TABLER_SRC="${TABLER_SRC:-../templates/tabler}"
DEST="${DEST:-../src/OpenCashFlow.App/wwwroot/vendor/tabler}"

echo "🚀 Tabler update starting..."
echo "   - Source: $TABLER_SRC"
echo "   - Dest:   $DEST"

if [ ! -d "$TABLER_SRC" ]; then
  echo "❌ Tabler source folder not found: $TABLER_SRC"
  exit 1
fi

# Tabler's build expects its internal helper scripts (note: this is a dot-folder).
# Newer Tabler versions ship TypeScript helpers (.ts) instead of .mjs.
if [ ! -f "$TABLER_SRC/.build/zip-package.ts" ] && [ ! -f "$TABLER_SRC/.build/zip-package.mjs" ]; then
  echo "❌ Missing $TABLER_SRC/.build/zip-package.(ts|mjs)"
  echo "   The .build helpers are required to finish the build."
  echo "   Fix: ensure Tabler was cloned fully (including dot-folders)."
  exit 1
fi

# -----------------------------
# Ensure pnpm is available
# -----------------------------
# We prefer a real pnpm binary, but we can fall back to running pnpm via npx
# (no global install, no sudo required).
PNPM_CMD="pnpm"

ensure_pnpm() {
  if command -v pnpm >/dev/null 2>&1; then
    PNPM_CMD="pnpm"
    return 0
  fi

  echo "ℹ️ pnpm not found. Trying corepack..."
  if command -v corepack >/dev/null 2>&1; then
    corepack enable >/dev/null 2>&1 || true
    corepack prepare pnpm@latest --activate >/dev/null 2>&1 || true
  fi

  if command -v pnpm >/dev/null 2>&1; then
    PNPM_CMD="pnpm"
    return 0
  fi

  if command -v npx >/dev/null 2>&1; then
    echo "ℹ️ Falling back to pnpm via npx (no global install)."
    PNPM_CMD="npx -y pnpm@latest"
    return 0
  fi

  echo "❌ pnpm not available and npx not found. Install Node.js (LTS) and try again."
  exit 1
}

ensure_pnpm

# -----------------------------
# Build (workspace install)
# -----------------------------
pushd "$TABLER_SRC" >/dev/null

echo "🧹 Cleaning mixed installs (safe cleanup)..."
# If you previously ran npm install, it can confuse workspace builds.
rm -rf node_modules
rm -rf core/node_modules docs/node_modules preview/node_modules 2>/dev/null || true

echo "📦 Installing dependencies with pnpm (workspace)..."
$PNPM_CMD install

echo "🏗️ Building Tabler..."
# This is the command Tabler itself declares (turbo build + zip-package)
$PNPM_CMD run build

popd >/dev/null

# -----------------------------
# Locate build output
# -----------------------------
echo "🔍 Searching for compiled assets (tabler*.min.css)..."
# Look for the compiled css in common output dirs
CSS_MATCHES=$(find "$TABLER_SRC" -type f \( -name "tabler.min.css" -o -name "tabler*.min.css" \) 2>/dev/null | head -n 20 || true)

if [ -z "${CSS_MATCHES:-}" ]; then
  echo "❌ Could not find any compiled Tabler CSS output."
  echo "   Build likely failed or output path changed."
  echo "   Tip: re-run build and check for errors above."
  exit 1
fi

echo "✅ Found compiled CSS candidates:"
echo "$CSS_MATCHES" | sed 's/^/   - /'

# Pick the first match and infer the asset root (the folder that contains css/)
FIRST_CSS_FILE=$(echo "$CSS_MATCHES" | head -n 1)
CSS_DIR=$(dirname "$FIRST_CSS_FILE")

# Asset root is parent of css/ if path ends with /css
ASSET_ROOT="$CSS_DIR"
if [[ "$CSS_DIR" == */css ]]; then
  ASSET_ROOT="$(dirname "$CSS_DIR")"
fi

echo "📁 Using asset root:"
echo "   $ASSET_ROOT"

# Validate expected folders exist
missing=0
for d in css js; do
  if [ ! -d "$ASSET_ROOT/$d" ]; then
    echo "❌ Missing folder: $ASSET_ROOT/$d"
    missing=1
  fi
done

if [ "$missing" -ne 0 ]; then
  echo "❌ Asset root does not look like a compiled Tabler dist (needs at least css/ and js/)."
  echo "   Found CSS at: $FIRST_CSS_FILE"
  exit 1
fi

# -----------------------------
# Copy to wwwroot
# -----------------------------
echo "🧹 Cleaning destination: $DEST"
rm -rf "$DEST"
mkdir -p "$DEST"

echo "📤 Copying assets to destination..."
# Copy only the folders that typically exist
for d in css js icons img fonts; do
  if [ -d "$ASSET_ROOT/$d" ]; then
    cp -R "$ASSET_ROOT/$d" "$DEST/"
    echo "   ✅ copied $d/"
  fi
done

# Also copy LICENSE if present (nice for OSS hygiene)
if [ -f "$TABLER_SRC/LICENSE" ]; then
  cp "$TABLER_SRC/LICENSE" "$DEST/LICENSE"
  echo "   ✅ copied LICENSE"
fi

echo "🎉 Done! Tabler assets updated in:"
echo "   $DEST"