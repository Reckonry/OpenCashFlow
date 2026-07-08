#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_NAME="${SMOKE_PROJECT_NAME:-opencashflow-smoke}"
API_PORT="${SMOKE_API_PORT:-15100}"
WEB_PORT="${SMOKE_WEB_PORT:-15200}"
DB_PORT="${SMOKE_DB_PORT:-15432}"
RESTORE_DB="${BACKUP_RESTORE_DB:-opencashflow_restore}"
KEEP_STACK="${BACKUP_RESTORE_KEEP_STACK:-0}"
COMPANY_NAME="${SMOKE_COMPANY_NAME:-OpenCashFlow Backup Restore Smoke Company}"
PAYMENT_DESCRIPTION="Clean install smoke payment"
WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/opencashflow-backup-restore.XXXXXX")"
DUMP_FILE="${WORK_DIR}/opencashflow-smoke.dump"

COMPOSE_FILES=(
  -f "${ROOT_DIR}/scripts/smoke/docker-compose.clean-install.yml"
)

cleanup() {
  if [[ "${KEEP_STACK}" == "1" ]]; then
    echo "Backup/restore smoke stack kept because BACKUP_RESTORE_KEEP_STACK=1."
    echo "Clean up with: docker compose -p ${PROJECT_NAME} ${COMPOSE_FILES[*]} down -v --remove-orphans"
  else
    docker compose -p "${PROJECT_NAME}" "${COMPOSE_FILES[@]}" down -v --remove-orphans >/dev/null 2>&1 || true
  fi
  rm -rf "${WORK_DIR}"
}
trap cleanup EXIT

compose() {
  docker compose -p "${PROJECT_NAME}" "${COMPOSE_FILES[@]}" "$@"
}

assert_equals() {
  local label="$1"
  local actual="$2"
  local expected="$3"

  if [[ "${actual}" != "${expected}" ]]; then
    echo "Unexpected ${label}: expected ${expected}, got ${actual}" >&2
    exit 1
  fi
}

query_scalar() {
  local sql="$1"
  compose exec -T db psql -U postgres -d "${RESTORE_DB}" -tAc "${sql}"
}

sql_literal() {
  local value="${1//\'/\'\'}"
  printf "'%s'" "${value}"
}

cd "${ROOT_DIR}"

echo "Running clean-install smoke and preserving stack for backup/restore drill"
SMOKE_KEEP_STACK=1 \
SMOKE_PROJECT_NAME="${PROJECT_NAME}" \
SMOKE_API_PORT="${API_PORT}" \
SMOKE_WEB_PORT="${WEB_PORT}" \
SMOKE_DB_PORT="${DB_PORT}" \
SMOKE_COMPANY_NAME="${COMPANY_NAME}" \
  "${ROOT_DIR}/scripts/smoke/clean-install-smoke.sh"

echo "Creating PostgreSQL custom-format backup"
compose exec -T db pg_dump -U postgres -d opencashflow --format=custom > "${DUMP_FILE}"

echo "Preparing restore database: ${RESTORE_DB}"
compose exec -T db dropdb -U postgres --if-exists "${RESTORE_DB}" >/dev/null
compose exec -T db createdb -U postgres "${RESTORE_DB}"

echo "Restoring backup into ${RESTORE_DB}"
compose exec -T db pg_restore -U postgres -d "${RESTORE_DB}" --clean --if-exists < "${DUMP_FILE}"

companies="$(query_scalar 'select count(*) from "Companies";')"
payments="$(query_scalar 'select count(*) from "Payments";')"
cash_ledgers="$(query_scalar 'select count(*) from "CashLedgers";')"
company_matches="$(query_scalar "select count(*) from \"Companies\" where \"CompanyName\" = $(sql_literal "${COMPANY_NAME}");")"
payment_matches="$(query_scalar "select count(*) from \"Payments\" where \"Description\" = $(sql_literal "${PAYMENT_DESCRIPTION}");")"
cash_payment_matches="$(query_scalar "select count(*) from \"CashLedgers\" where \"RefType\" = 'Payment' and \"Delta\" = 25.75;")"

assert_equals "Companies count" "${companies}" "1"
assert_equals "Payments count" "${payments}" "1"
assert_equals "CashLedgers count" "${cash_ledgers}" "1"
assert_equals "smoke company record count" "${company_matches}" "1"
assert_equals "smoke payment record count" "${payment_matches}" "1"
assert_equals "smoke cash ledger record count" "${cash_payment_matches}" "1"

echo "Backup/restore smoke drill passed"
echo "Backup command: pg_dump -U postgres -d opencashflow --format=custom"
echo "Restore command: pg_restore -U postgres -d ${RESTORE_DB} --clean --if-exists"
echo "Restore database: ${RESTORE_DB}"
echo "Companies: ${companies}"
echo "Payments: ${payments}"
echo "CashLedgers: ${cash_ledgers}"
echo "SmokeCompanyMatches: ${company_matches}"
echo "SmokePaymentMatches: ${payment_matches}"
echo "SmokeCashLedgerMatches: ${cash_payment_matches}"
