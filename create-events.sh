#!/bin/bash
set -eu
set -o pipefail

# Check if Sentry DSN is provided
if [ -z "$1" ]; then
    echo "Error: No Sentry DSN provided. Please provide a Sentry DSN as the first argument."
    exit 1
fi

# Number of events passed as a parameter, default to 100 if not provided
num_events=${2:-100}

# Use dotnet to create some events. 25% will be exceptions and 75% will be transactions
dotnet restore 
dotnet run --project PerfUnits3283/PerfUnits3283.csproj --dsn $1  --events $num_events