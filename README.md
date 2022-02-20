# tnstool
A cross-platform .NET 6 tool for poking Oracle TNS listener services.

## Usage
There are two modes of operation, the default is to read a single target from the command line or read a list of targets from a text file. Both ways the target(s) must be in format <IP/hostname>[:\<port>] where the port is optional. E.g. `192.168.10.50:1531`

```
./tnstool <args>

-h, --host       Required. Hostname/IP or file containing list of targets. Format <address>[:<port>]
-c, --command    Required. Command to run (e.g. ping, version, status, services)

-a, --args       Arguments (for commands which require them)
-s, --service    (Default: LISTENER) TNS service name
-o, --outfile    Write output to file (JSON)
-d, --debug      (Default: false) Enable debug messages
```

The second mode of operation is to use a previously outputted JSON file as the input, this is useful to run subsequent commands against a range of targets. In this mode arguments like version and service name will be loaded from previously enumerated data. Also targets that did not respond (`IsDead`), or refused connection (`IsSecure`) previously will be skipped over.

```
./tnstool json <args>

-f, --file       Required. JSON file
-c, --command    Required. Command to run (e.g. ping, version, status, services)

-a, --args       Arguments (for commands which require them)
-d, --debug      (Default: false) Enable debug messages
```

## Example usage

1. Ping a list of targets to determine which are alive and their ALIASes

`./tnstool -o testing.json -h host_list.txt -c ping`

2. Use previously output file to connect to live targets and enumerate version number if allowed

`./tnstool json -i testing.json -c version`

3. Use json file to continue running further commands against live, insecure targets

`./tnstool json -i testing.json -c status`

`./tnstool json -i testing.json -c services`

testing.json at this stage would contain json formatted data of all the initial targets, whether they are live, secure, their listener aliases, versions as well as response to any further commands. Example entry below (response data clipped for clarity):

```
{
  "Host": "10.14.100.17",
  "Port": 1521,
  "Version": "135294976",
  "Aliases": [
    "pharma"
  ],
  "IsSecure": false,
  "IsDead": false,
  "ResponseData": {
    "version": "TNSLSNR for Linux: Version 8.1.7.0.0 - Development\n\tTNS for[...]",
    "status": "(DESCRIPTION=(TMP=)(VSNNUM=135294976)(ERR=0)(ALIAS=LISTENER)(SECURITY=OFF)(VER[...]",
    "services": "(SERVICE=(SERVICE_NAME=PLSExtProc)(INSTANCE=(INSTANCE_NAME=PLSExtProc)(NUM=1)(INST[...]"
  }
}
```
