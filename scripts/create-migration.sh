dotnet ef migrations add InitialCreate \
  --project src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj \
  --startup-project src/OpenCashFlow.API/OpenCashFlow.API.csproj \
  --context ApplicationDbContext \
  --output-dir Data/Migrations
#!/usr/bin/env bash
set -euo pipefail

# OpenCashFlow - EF Core migration helper
# Usage:
#   ./create-migration.sh <Name> [--apply]
#   ./create-migration.sh <Name> -apply
#   ./create-migration.sh --remove
#   ./create-migration.sh remove
#
# Notes:
# - <Name> is optional; if omitted and we are creating a migration, you will be prompted.
# - --remove removes the last migration (EF Core limitation: you cannot remove by name).

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

PROJECT_CSproj="src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj"
STARTUP_CSproj="src/OpenCashFlow.API/OpenCashFlow.API.csproj"
DBCONTEXT="ApplicationDbContext"
OUTPUT_DIR="Data/Migrations"

print_usage() {
  cat <<'USAGE'
OpenCashFlow migration helper

Create a migration:
  ./create-migration.sh <MigrationName> [--apply]
  ./create-migration.sh <MigrationName> -apply

Remove the last migration:
  ./create-migration.sh --remove
  ./create-migration.sh remove

Options:
  --apply, -apply    Apply migrations to the database (dotnet ef database update)
  --remove, remove   Remove the last migration (dotnet ef migrations remove)
  --help, -h         Show this help

Examples:
  ./create-migration.sh InitialCreate --apply
  ./create-migration.sh AddInvoices
  ./create-migration.sh --remove
USAGE
}

# -------------------------------
# Parse args
# -------------------------------
MIGRATION_NAME=""
DO_APPLY=false
DO_REMOVE=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --help|-h)
      print_usage
      exit 0
      ;;
    --apply|-apply)
      DO_APPLY=true
      shift
      ;;
    --remove|remove)
      DO_REMOVE=true
      shift
      ;;
    *)
      # First non-flag argument is the migration name.
      if [[ -z "${MIGRATION_NAME}" ]]; then
        MIGRATION_NAME="$1"
        shift
      else
        echo "Unknown argument: $1"
        echo
        print_usage
        exit 1
      fi
      ;;
  esac
done

cd "${REPO_ROOT}"

# -------------------------------
# Commands
# -------------------------------
run_remove() {
  echo "Removing the last migration..."
  dotnet ef migrations remove \
    --project "${PROJECT_CSproj}" \
    --startup-project "${STARTUP_CSproj}" \
    --context "${DBCONTEXT}"
  echo "Done."
}

run_add() {
  if [[ -z "${MIGRATION_NAME}" ]]; then
    read -r -p "Enter migration name: " MIGRATION_NAME
    MIGRATION_NAME="${MIGRATION_NAME//[[:space:]]/}"
  fi

  if [[ -z "${MIGRATION_NAME}" ]]; then
    echo "Migration name cannot be empty."
    exit 1
  fi

  echo "Creating migration: ${MIGRATION_NAME}"
  dotnet ef migrations add "${MIGRATION_NAME}" \
    --project "${PROJECT_CSproj}" \
    --startup-project "${STARTUP_CSproj}" \
    --context "${DBCONTEXT}" \
    --output-dir "${OUTPUT_DIR}"

  echo "Migration created."

  if [[ "${DO_APPLY}" == true ]]; then
    echo "Applying migrations to the database..."
    dotnet ef database update \
      --project "${PROJECT_CSproj}" \
      --startup-project "${STARTUP_CSproj}" \
      --context "${DBCONTEXT}"
    echo "Database updated."
  fi
}

# If remove is requested, do it and exit.
if [[ "${DO_REMOVE}" == true ]]; then
  if [[ "${DO_APPLY}" == true ]]; then
    echo "Warning: --apply is ignored when using --remove."
  fi
  run_remove
  exit 0
fi

# Default action: add migration (optionally apply)
run_add