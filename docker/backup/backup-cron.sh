#!/bin/sh
set -e

# ==============================================================================
# StitchMind Automated Database Backup & 30-Day Retention Pruning (UAT-03)
# ==============================================================================

BACKUP_DIR="/backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/GarmentsERP_DB_${TIMESTAMP}.bak"
DB_SERVER="${DB_HOST:-mssql}"
DB_USER="${DB_USER:-sa}"
DB_PASS="${DB_PASSWORD:-StitchMind@2026!Secret}"
DB_NAME="${DB_NAME:-GarmentsERP_DB}"

echo "=============================================================================="
echo "STITCHMIND AUTOMATED BACKUP ENGINE STARTED: $(date)"
echo "Target Host    : ${DB_SERVER}"
echo "Target Database: ${DB_NAME}"
echo "Destination    : ${BACKUP_FILE}"
echo "=============================================================================="

mkdir -p "${BACKUP_DIR}"

# 1. Execute SQL Server Native Backup with Compression and Checksum
echo "[1/3] Triggering native SQL Server compressed backup..."
/opt/mssql-tools18/bin/sqlcmd -S "${DB_SERVER}" -U "${DB_USER}" -P "${DB_PASS}" -C -Q "
BACKUP DATABASE [${DB_NAME}]
TO DISK = N'${BACKUP_FILE}'
WITH FORMAT, INIT,
NAME = N'StitchMind_ERP-Full Database Backup',
SKIP, NOREWIND, NOUNLOAD, COMPRESSION, CHECKSUM, STATS = 10;
"

# 2. Verify Backup File Integrity
if [ -f "${BACKUP_FILE}" ]; then
    FILE_SIZE=$(du -h "${BACKUP_FILE}" | cut -f1)
    echo "[2/3] Backup completed successfully! Backup archive size: ${FILE_SIZE}"
else
    echo "ERROR: Backup file ${BACKUP_FILE} was not created!"
    exit 1
fi

# 3. Prune Backups Older Than 30 Days (Rolling Retention Policy)
echo "[3/3] Pruning backups older than 30 days..."
find "${BACKUP_DIR}" -type f -name "*.bak" -mtime +30 -exec echo "Removing stale backup: {}" \; -exec rm -f {} \;

echo "Automated backup job finished successfully at $(date)"
echo "=============================================================================="
