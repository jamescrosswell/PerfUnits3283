#!/bin/bash
set -eu
set -o pipefail

# Default envelope type - this can be overriden as a command line argument
envelope_type=event

# Check if Sentry DSN is provided
if [ -z "$1" ]; then
    echo "Error: No Sentry DSN provided. Please provide a Sentry DSN as the first argument."
    exit 1
fi

# Extract the Sentry key, API host, and project ID from the DSN
SENTRY_KEY=$(echo $1 | cut -d'/' -f3 | cut -d'@' -f1)
API_HOST=$(echo $1 | cut -d'@' -f2 | cut -d'/' -f1)
PROJECT_ID=$(echo $1 | cut -d'/' -f4)

echo "Sentry Key: $SENTRY_KEY"
echo "API Host: $API_HOST"
echo "Project ID: $PROJECT_ID"

# Directory where the envelopes are located
dir="./envelopes"

# Get an envelope to send
envelope_files=($(ls $dir/${envelope_type}*.envelope 2> /dev/null))
envelope_file=${envelope_files[0]}
echo "Sending envelope: $envelope_file"

curl -i -X POST \
  -H 'Content-Type: application/x-sentry-envelope' \
  -H "X-Sentry-Auth: Sentry sentry_version=7, sentry_key=$SENTRY_KEY, sentry_client=raven-bash/0.1" \
  -d $envelope_file \
  https://$API_HOST/api/$PROJECT_ID/envelope/

# Delete the envelope
rm $envelope_file