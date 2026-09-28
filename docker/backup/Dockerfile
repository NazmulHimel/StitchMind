FROM mcr.microsoft.com/mssql-tools:latest

USER root

# Install cron and curl
RUN apt-get update && apt-get install -y --no-install-recommends \
    cron \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /backup-app

COPY backup-cron.sh /backup-app/backup-cron.sh
RUN chmod +x /backup-app/backup-cron.sh

# Setup cron entry: Run every night at 02:00 AM
RUN echo "0 2 * * * /backup-app/backup-cron.sh >> /var/log/cron-backup.log 2>&1" > /etc/cron.d/db-backup-cron \
    && chmod 0644 /etc/cron.d/db-backup-cron \
    && crontab /etc/cron.d/db-backup-cron

VOLUME ["/backups"]

# Start cron daemon and tail log
CMD ["sh", "-c", "/backup-app/backup-cron.sh && cron && tail -f /var/log/cron-backup.log"]
