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

# Sentry DSN passed as a parameter
export SENTRY_DSN=$1

# Directory where the envelopes are located
dir="./envelopes"

# Get an envelope to send
envelope_files=($(ls $dir/${envelope_type}*.envelope 2> /dev/null))

envelope_file=${envelope_files[0]}

# Send the envelope
sentry-cli send-envelope --raw $envelope_file

# Delete the envelope
rm $envelope_file