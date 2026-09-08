#!/usr/bin/env bash
# One-time setup for a fresh EC2 instance hosting chh-api, in case this
# instance is ever replaced (new AMI, infra rebuild, etc). Captures the
# manual recovery steps taken on 2026-09-07 after the original instance's
# PostgreSQL install and JWT signing key were found missing/lost.
#
# Run this ON the EC2 box itself (as the ubuntu user, with sudo):
#   chmod +x provision-ec2.sh && ./provision-ec2.sh
#
# It does NOT deploy the app itself -- that's handled by
# .github/workflows/backend-deploy.yml on push to main. This script only
# provisions the OS-level dependencies that deploy workflow assumes already
# exist: PostgreSQL, and a JWT signing key wired into the systemd service.
#
# NOT covered by this script (do manually, credentials aren't stored here):
# the Fast2Sms OTP config also lives in the same systemd override and was
# lost along with the JWT key. Get the real API key from
# https://www.fast2sms.com/ (Dev API section) and add to
# /etc/systemd/system/chh-api.service.d/override.conf under [Service]:
#   Environment="Fast2Sms__ApiKey=<real key from Fast2SMS dashboard>"
#   Environment="Fast2Sms__Channel=sms"
#   Environment="Fast2Sms__WhatsApp__PhoneNumberId=1344445125411727"
#   Environment="Fast2Sms__WhatsApp__OtpMessageId=31541"
#   Environment="Fast2Sms__WhatsApp__DonorRequestMessageId=31543"
# (PhoneNumberId/message IDs are not secret -- they're the approved
# WhatsApp template IDs tied to the "Klockk" sender +1555-399-1190 -- only
# the ApiKey itself needs fetching fresh.)
#
# Channel=sms (added 2026-09-07): OTP delivery uses Fast2SMS's Quick SMS
# route (see Fast2SmsGatewayClient) -- WhatsApp OTP delivery is blocked by
# Meta error 131037 (display-name approval pending). Without this line the
# app silently falls back to appsettings.json's default ("whatsapp"), which
# is exactly how this got missed the first time -- no error, just silent
# non-delivery. Switch back to "whatsapp" once the display-name is approved,
# if WhatsApp delivery is preferred once it works again.
set -euo pipefail

DB_NAME="CHH"
DB_USER="postgres"
DB_PASSWORD="Pass@123"  # matches the default ConnectionStrings in appsettings.json

echo "== Installing PostgreSQL =="
sudo apt-get update
sudo apt-get install -y postgresql
sudo systemctl enable --now postgresql

echo "== Setting postgres user password and creating database =="
sudo -u postgres psql -c "ALTER USER ${DB_USER} PASSWORD '${DB_PASSWORD}';"
sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname = '${DB_NAME}'" | grep -q 1 \
  || sudo -u postgres psql -c "CREATE DATABASE \"${DB_NAME}\";"

echo "== Generating a JWT signing key =="
if [ -f /etc/systemd/system/chh-api.service.d/override.conf ] && \
   grep -q "Jwt__SigningKeyBase64" /etc/systemd/system/chh-api.service.d/override.conf; then
  echo "Signing key already present in override.conf -- leaving it as is."
else
  JWT_KEY=$(openssl rand -base64 48)
  sudo mkdir -p /etc/systemd/system/chh-api.service.d
  sudo tee -a /etc/systemd/system/chh-api.service.d/override.conf > /dev/null <<EOF
[Service]
Environment="Jwt__SigningKeyBase64=${JWT_KEY}"
EOF
  echo "Wrote a new signing key to /etc/systemd/system/chh-api.service.d/override.conf"
  echo "NOTE: any JWTs issued before this point (if the key changed) are now invalid."
fi

sudo systemctl daemon-reload

echo "== Done. If chh-api is already deployed, restart it: =="
echo "  sudo systemctl restart chh-api && sudo systemctl status chh-api"
