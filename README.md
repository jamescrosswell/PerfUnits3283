# PerfUnits3283
 
This repository contains:
1. `create-events.sh` - a bash script that creates valid envelopes in the `./envelopes` directory
2. `send-event.sh` - a bash script that will send one of the envelopes using the `sentry-cli`. 

## Pre-requisites

.NET 8 or later will need to be installed. You can [download this](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) for *macOS*, *Linux* and *Windows*. 

## Usage

### Creating envelopes to send

`create-events.sh` requires a single parameter, which is the DSN of a Sentry project. 

```zsh
$ ./create-events.sh <DSN>
```

This will run a program that loops, sending 90 transactions to the Sentry DSN provided. It will also create an `./envelopes` directory and create 10 envelopes containing errors, which can then be sent manually/separately (see below). 

**Important Note:** The DSN should be that of a project that has no remaining Performance unit quota, so that relevant rate limit headers are returned for the first requests(s). That ensures envelope items covered by such rate limit headers get removed from any envelopes being created for exceptions. [See here](https://github.com/getsentry/sentry-dotnet/blob/main/src/Sentry/Http/HttpTransportBase.cs#L97-L99) for detail.

#### Controlling the number of events to be sent

If you want to create more than 10 events (or send more than 90 transactions), a second optional parameter can be provided, specifying how many transactions and events should be generated in total. 90% of these will be transactions and 10% of these will be errors.

So for example you could run the following command to generate 9 transactions and 1 error:

```zsh
$ ./create-events.sh https://yourdsngoeshere 10
```

### Sending envelopes

`send-event.sh` accepts a single parameter, which is the DSN of the Sentry project where you want to send the event. 

```zsh
$ ./send-event.sh <DSN>
```

Assuming you give it a valid DSN, it will take the first event envelope it finds in the `./envelopes` folder and send this using the Sentry CLI.

Alternatively, you could mimic that behaviour using cURL or some other mechanism.