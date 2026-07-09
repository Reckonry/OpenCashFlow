#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_NAME="${SMOKE_PROJECT_NAME:-opencashflow-smoke}"
API_PORT="${SMOKE_API_PORT:-15100}"
WEB_PORT="${SMOKE_WEB_PORT:-15200}"
API_URL="http://localhost:${API_PORT}"
WEB_URL="http://localhost:${WEB_PORT}"
ADMIN_EMAIL="${SMOKE_ADMIN_EMAIL:-smoke-admin@example.test}"
ADMIN_PASSWORD="${SMOKE_ADMIN_PASSWORD:-SmokeP@ssw0rd!2026}"
COMPANY_NAME="${SMOKE_COMPANY_NAME:-OpenCashFlow Smoke Company}"
KEEP_STACK="${SMOKE_KEEP_STACK:-0}"
WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/opencashflow-smoke.XXXXXX")"

COMPOSE_FILES=(
  -f "${ROOT_DIR}/scripts/smoke/docker-compose.clean-install.yml"
)

cleanup() {
  local exit_code=$?
  if [[ "${KEEP_STACK}" == "1" && "${exit_code}" != "0" ]]; then
    echo "Smoke failed. Stack kept for inspection because SMOKE_KEEP_STACK=1."
    echo "Collect logs with: docker compose -p ${PROJECT_NAME} ${COMPOSE_FILES[*]} logs"
  else
    docker compose -p "${PROJECT_NAME}" "${COMPOSE_FILES[@]}" down -v --remove-orphans >/dev/null 2>&1 || true
  fi
  rm -rf "${WORK_DIR}"
}
trap cleanup EXIT

compose() {
  docker compose -p "${PROJECT_NAME}" "${COMPOSE_FILES[@]}" "$@"
}

request() {
  local method="$1"
  local url="$2"
  local body="${3:-}"
  local output="$4"
  local token="${5:-}"
  local expected="${6:-200}"
  local status
  local args=(-fsS -X "${method}" -o "${output}" -w "%{http_code}" -H "Accept: application/json")

  if [[ -n "${token}" ]]; then
    args+=(-H "Authorization: Bearer ${token}")
  fi

  if [[ -n "${body}" ]]; then
    args+=(-H "Content-Type: application/json" --data "${body}")
  fi

  status="$(curl "${args[@]}" "${url}")"
  if [[ "${status}" != "${expected}" ]]; then
    echo "Unexpected HTTP status for ${method} ${url}: expected ${expected}, got ${status}" >&2
    echo "Response body:" >&2
    cat "${output}" >&2 || true
    exit 1
  fi
}

json_get() {
  local file="$1"
  shift
  python3 - "$file" "$@" <<'PY'
import json
import sys

def lookup(value, path):
    current = value
    for part in path:
        if isinstance(current, list):
            current = current[int(part)]
            continue
        if not isinstance(current, dict):
            raise KeyError(part)
        match = next((key for key in current.keys() if key.lower() == part.lower()), None)
        if match is None:
            raise KeyError(part)
        current = current[match]
    return current

with open(sys.argv[1], "r", encoding="utf-8") as handle:
    data = json.load(handle)

result = lookup(data, sys.argv[2:])
if isinstance(result, bool):
    print(str(result).lower())
elif result is None:
    print("")
else:
    print(result)
PY
}

json_first_id() {
  local file="$1"
  local id_key="$2"
  local preferred_name_key="${3:-}"
  local preferred_name="${4:-}"
  python3 - "$file" "$id_key" "$preferred_name_key" "$preferred_name" <<'PY'
import json
import sys

with open(sys.argv[1], "r", encoding="utf-8") as handle:
    data = json.load(handle)

id_key = sys.argv[2].lower()
preferred_name_key = sys.argv[3].lower()
preferred_name = sys.argv[4].lower()

if not isinstance(data, list) or not data:
    raise SystemExit("Expected a non-empty JSON array")

def get_case_insensitive(obj, key):
    return next((value for current_key, value in obj.items() if current_key.lower() == key), None)

selected = None
if preferred_name_key and preferred_name:
    for item in data:
        value = get_case_insensitive(item, preferred_name_key)
        if isinstance(value, str) and value.lower() == preferred_name:
            selected = item
            break

selected = selected or data[0]
identifier = get_case_insensitive(selected, id_key)
if not identifier:
    raise SystemExit(f"Could not find id key {sys.argv[2]}")

print(identifier)
PY
}

jwt_claim() {
  local token="$1"
  local claim="$2"
  python3 - "$token" "$claim" <<'PY'
import base64
import json
import sys

token = sys.argv[1]
claim = sys.argv[2].lower()
payload = token.split(".")[1]
payload += "=" * (-len(payload) % 4)
data = json.loads(base64.urlsafe_b64decode(payload.encode("ascii")))
match = next((key for key in data.keys() if key.lower() == claim), None)
if match is None:
    raise SystemExit(f"Missing JWT claim {sys.argv[2]}")
print(data[match])
PY
}

uuid() {
  python3 - <<'PY'
import uuid
print(uuid.uuid4())
PY
}

wait_for_url() {
  local url="$1"
  local label="$2"
  local attempts="${3:-60}"
  for _ in $(seq 1 "${attempts}"); do
    if curl -fsS "${url}" >/dev/null 2>&1; then
      echo "${label} is ready"
      return 0
    fi
    sleep 2
  done

  echo "Timed out waiting for ${label} at ${url}" >&2
  compose logs --tail=200 >&2 || true
  exit 1
}

cd "${ROOT_DIR}"

echo "Starting clean OpenCashFlow smoke stack (${PROJECT_NAME})"
compose down -v --remove-orphans >/dev/null 2>&1 || true
compose up -d --build

wait_for_url "${API_URL}/health" "API health"
wait_for_url "${WEB_URL}/Login" "WebApp login page"

setup_status="${WORK_DIR}/setup-status.json"
request GET "${API_URL}/v1/Setup/status" "" "${setup_status}" "" 200
requires_setup="$(json_get "${setup_status}" requiresSetup)"
if [[ "${requires_setup}" != "true" ]]; then
  echo "Expected a clean database to require setup. Status response:" >&2
  cat "${setup_status}" >&2
  exit 1
fi

setup_body="$(cat <<JSON
{
  "companyName": "${COMPANY_NAME}",
  "adminEmail": "${ADMIN_EMAIL}",
  "adminFirstName": "Smoke",
  "adminLastName": "Admin",
  "language": "en",
  "currency": "EUR",
  "timezone": "Europe/Rome",
  "country": "IT"
}
JSON
)"
setup_response="${WORK_DIR}/setup-response.json"
request POST "${API_URL}/v1/Setup" "${setup_body}" "${setup_response}" "" 201
temporary_admin_password="$(json_get "${setup_response}" temporaryAdminPassword)"

login_body="$(cat <<JSON
{
  "username": "${ADMIN_EMAIL}",
  "password": "${temporary_admin_password}"
}
JSON
)"
login_response="${WORK_DIR}/login-response.json"
request POST "${API_URL}/v1/Authentication/login" "${login_body}" "${login_response}" "" 200

token="$(json_get "${login_response}" data token)"

change_password_body="$(cat <<JSON
{
  "newPassword": "${ADMIN_PASSWORD}"
}
JSON
)"
change_password_response="${WORK_DIR}/change-password-response.json"
request POST "${API_URL}/v1/Authentication/change-password-required" "${change_password_body}" "${change_password_response}" "${token}" 200

login_body="$(cat <<JSON
{
  "username": "${ADMIN_EMAIL}",
  "password": "${ADMIN_PASSWORD}"
}
JSON
)"
request POST "${API_URL}/v1/Authentication/login" "${login_body}" "${login_response}" "" 200

token="$(json_get "${login_response}" data token)"
tenant_id="$(jwt_claim "${token}" TenantID)"
user_id="$(jwt_claim "${token}" UserID)"

methods_response="${WORK_DIR}/payment-methods.json"
request GET "${API_URL}/v1/Payment/PaymentMethods" "" "${methods_response}" "${token}" 200
payment_method_id="$(json_first_id "${methods_response}" PaymentMethodID PaymentMethodName Cash)"

document_types_response="${WORK_DIR}/document-types.json"
request GET "${API_URL}/v1/Payment/DocumentTypes" "" "${document_types_response}" "${token}" 200
document_type_id="$(json_first_id "${document_types_response}" DocumentTypeID DocumentTypeName Invoice)"

payment_id="$(uuid)"
request_id="$(uuid)"
payment_body="$(cat <<JSON
{
  "paymentID": "${payment_id}",
  "tenantID": "${tenant_id}",
  "requestId": "${request_id}",
  "amount": 25.75,
  "entryType": "Income",
  "paymentMethodID": "${payment_method_id}",
  "documentTypeID": "${document_type_id}",
  "description": "Clean install smoke payment",
  "userID": "${user_id}",
  "dateIns": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
}
JSON
)"
payment_response="${WORK_DIR}/payment-response.json"
request POST "${API_URL}/v1/Payment" "${payment_body}" "${payment_response}" "${token}" 201

payments_response="${WORK_DIR}/payments.json"
request POST "${API_URL}/v1/Payments" '{"includeAllUsers":true,"page":1,"pageSize":50}' "${payments_response}" "${token}" 200
python3 - "${payments_response}" "${payment_id}" <<'PY'
import json
import sys

with open(sys.argv[1], "r", encoding="utf-8") as handle:
    payments = json.load(handle)

target = sys.argv[2].lower()
if not any(str(item.get("paymentID") or item.get("PaymentID")).lower() == target for item in payments):
    raise SystemExit("Created payment was not returned by /v1/Payments")
PY

cash_response="${WORK_DIR}/cash-current.json"
request GET "${API_URL}/v1/admin/cash/current?companyId=${tenant_id}" "" "${cash_response}" "${token}" 200
python3 - "${cash_response}" <<'PY'
import decimal
import json
import sys

with open(sys.argv[1], "r", encoding="utf-8") as handle:
    value = json.load(handle)

if decimal.Decimal(str(value)) <= decimal.Decimal("0"):
    raise SystemExit(f"Expected positive cash balance after smoke payment, got {value}")
PY

ledger_response="${WORK_DIR}/cash-ledger.json"
request GET "${API_URL}/v1/admin/cash/ledger?companyId=${tenant_id}&take=20" "" "${ledger_response}" "${token}" 200
python3 - "${ledger_response}" "${payment_id}" <<'PY'
import json
import sys

with open(sys.argv[1], "r", encoding="utf-8") as handle:
    entries = json.load(handle)

target = sys.argv[2].lower()
if not any(str(item.get("refId") or item.get("RefId")).lower() == target for item in entries):
    raise SystemExit("Created payment was not found in cash ledger")
PY

echo "Clean install smoke passed"
echo "API: ${API_URL}"
echo "WebApp: ${WEB_URL}"
echo "TenantID: ${tenant_id}"
echo "UserID: ${user_id}"
echo "PaymentID: ${payment_id}"
