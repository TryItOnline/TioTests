# TioTests

A tool that is capable of running test suits on [TryItOnline!](https://tryitonline.net). A test suite with "Hello, World!" test for every supported language is also provided. 
Runs everywhere, where .NET Core runs. Tested on Windows 10, Fedora 24, Fedora 25.

## Installation

    1. Install [.NET Core](https://www.microsoft.com/net/core) you need version 1.1
    2. `git clone https://github.com/AndrewSav/TioTests.git`
    3. `cd TioTests`
    4. `dotnet restore`
    5. Edit `config.json` if required
    6. `dotnet build`
    7. `dotnet run`

## Ways to specify parameters

Each option has a default value. These can be overridden in `config.json`. Finally, these can be overridden from the command line.
`config.json` - is a convenient place to keep your personal defaults. This file needs to be in the current folder to get picked up.
Take a note of that, if you are running the program from a different folder.

All options including boolean must have an explicit value specified on the command line (the only exception is the help option `-h`).
Any of the following values specify a turned on boolean option: "on", "true", "enable", "+", "yes".
Any of the following values specify a turned off boolean option: "off", "false", "disable", "-", "no".

For example `-q on` and `-q off` is correct, but simply `-q` is not.

Certain string options can be specified as `null` in the `config.json`. There is no way to specify empty option on the command line.
If you would like to override `-c` or `-x` option specified in the `config.json` with the null value specify `-c off` or `-x off` respectively.


## Running the program

Since the program is written in `dotnet` it by default needs to be run from the "project" folder with `dotnet run`. This can often be inconvenient.
An alternate way to run it executing `dotnet /path/to/TioTests/bin/Debug/netcoreapp1.0/TioTests.dll`. If the latter is used don't forget to execute
`dotnet build` in the project folder each time, the source code changes, for example, after you pulled the latest version from git.

You can create shell script like this:

```bash
#!/bin/bash

dotnet /opt/TioTests/bin/Debug/netcoreapp1.0/TioTests.dll "$@"
```

And, assuming it's called `tiotest` link it to your PATH like this: `ln -st /usr/local/bin/tiotest /path/to/tiotest`.
Then you should be able to call it from anywhere by just typing `tiotest`.

## Command line options

The following sub-headers are the representation of the command line options in `config.json`.

### RunUrl

**Command line switch**: -u, --url

**Default**: `https://backend.tryitonline.net/run/api/no-cache/`

**Description**: Specifies TryItOnline backend url. The `api` part of the url makes sure that returned http response contains `Content-Encoding: gzip` header, this way the response automatically unzipped on the client. TioTests relies on presence of this header for unzipping the response. The `no-cache` part of the url makes sure that the test is actually run even if a previous test result is already cached. This parameter is not used in the local run mode.

### TestPath

**Command line switch**: -n,  --test

**Default**: See below.

**Description**: Specifies test(s) to run. If specified path is a folder, then all the *.json files from the folder are run in alphabetical order. Otherwise, if specified file exists, then run the file. Otherwise, append the `.json` to the specified file and run the result. The default for this parameter is the supplied Hello World test suite, which is located in the HelloWorldTests folder of this repo.

### TrimWhitespacesFromResults

**Command line switch**: -t, --trim

**Default**: true

**Description**: Trim, tabs, spaces, line feeds and carriage returns from the test result before comparing it with the expected output. For example if the expected output is "Hello, World!", and the result is " Hello, World!\t\n", the test will be a success, with the default of this setting. There is no reason to switch this setting off for the Hello World test suites, but it might be useful if you want to run your own tests.

### DisplayDebugInfoOnError

**Command line switch**: -f, --dump-on-error

**Default**: true

**Description**: If enabled, in case of test failed result, TryItOnline debug output is displayed in addition to the test result. I most cases it's advantageous to keep this turned on, since when a test fails, the debug output usually hints on the failure reason. Under certain scenarios, for example when adding many languages to TryItOnline site in succession, it is known why a test would fail - simply because the language is not there yet. In this case this option can be turned off to make the results more compact. Note, that this option overrides the `-q` option below, but only for failed tests.

### DisplayDebugInfoOnSuccess

**Command line switch**: -s, --dump-on-success

**Default**: false

**Description**: If enabled, in case of test successful result, TryItOnline debug output is displayed in addition to the test result. In most cases it would produce too verbose output. Enable this if you want to see TryItOnline debug output for successful tests. Note, that this option overrides the `-q` option below, but only for successful tests.

### UseConsoleCodes

**Command line switch**: -d, --use-console-codes

**Default**: true

**Description**: This option controls the output mode. If enabled, ANSI codes are used to display successful tests in green and failed tests in red. In addition, ANSI codes are used to display test start and test finish information on the same line. When you run the test interactively, you want to keep this option on. If you run your test from a background job, such as described in [WebSetup.md](WebSetup.md), switch this off to prevent ANSI codes from appearing in the logs.

### CheckUrl

**Command line switch**: -c, --check-missing-tests

**Default**: `https://tryitonline.net/languages.json`

**Description**: If this is provided, the url is queried and the list received in response is checked against the currently running set of tests. This option makes sense only for a test suite that is supposed to include every single language on TryItOnline to detect the missing languages. If you are not running such a test suite (e.g. Hello World test suite), switch this option off with `-c off`.

### Retries

**Command line switch**: -r, --retries

**Default**: 3

**Description**: This option is only important if you have unstable internet connection. It allows to re-try a test, if http connection fails during the test. The number is how many times each test will be re-tried. This option is not used in the local run mode.

### LocalRun

**Command line switch**: -l, --local-run

**Default**: false

**Description**: Enables local run mode, which allows running tests against local backend. This means that you want to use this option, if you are running tests from your main TryItOnline server. This option, when enabled, does not use http for executing the tests, instead, it spawns local cgi backend process. This is usually much faster, than http requests. In addition, when running in the local run mode, there is an option to batch up all the tests in a test suite in a single request. This is not possible in the http mode, since such a huge requests will most likely be terminated by the arena server, executing the code, when it times out. In the local run mode, however, it is possible to specify a longer timeout. It is not possible in the http mode for security reasons, since http request can be executed by anyone, and the local request can only be executed by you.

### LocalRoot

**Command line switch**: -o, --local-root

**Default**: `/srv`

**Description**: Specifies where your main server installation is. By default, the arena address is taken from `etc/run` relative to this path and the script to execute is `backend.tryitonline.net/run` also relative to this path. These defaults can be overwritten by `-p` and `-z` respectively. See below. This option is only used in local run mode.

### LocalProcess

**Command line switch**: -p, --local-process

**Default**: `null`

**Description**: Allows to override the process to run in local run mode. E.g. `-p /srv/backened/tryitonline.net/run-dev`. If not specified, default value is used, see `-o` above. This option is only used in local run mode.

### ArenaHost

**Command line switch**: -z, --arena-host

**Default**: `null`

**Description**: Allows to override the process to run in local run mode. E.g. `-z runner@arena2.tryitonline.nz`. If not specified, default value is used, see `-o` above. Note, that arena server should be added in known_hosts. You can use [this script](https://github.com/TryItOnline/TioSetup/blob/master/misc/addarena) for it. Also your main server and the specified arena server should have matching ssh public / private keys. See <https://github.com/TryItOnline/TioSetup> for more details. This option is only used in local run mode.

### BatchMode

**Command line switch**: -b, --batch-mode

**Default**: true

**Description**: Batch up all the tests in the test suite into a single request and execute it at once. Note, that in this mode it is not possible to get progress of test execution, so the program may appear to hang for sometime until the server returns there results. This allows greatly speed up test executions for large test suites. For example a remote http run of Hello World tests can take 6 minutes, but a local run of batched up tests executes in just over a minute. This option is only used in local run mode. Note that while the batch mode is only possible in the local run mode, local run mode itself *can* execute tests one by one. Specify `-b off` to enable this.

### Quiet

**Command line switch**: -q, --quiet

**Default**: false

**Description**: Suppresses results for individual tests, only the tests summary is displayed. This option is only used in batch mode. Since seeing 200+ green lines all at once does not provide a lot of value (outside of batch mode they are displayed one by one providing progress feedback), you can use this option to suppress them. `-f` and `-s` options override this option. If you are using the default values for `-f` and `-s` and turn on this option, you will only see error results (with debug output) displayed.

### DebugDump

**Command line switch**: -x, --debug-dump

**Default**: null

**Description**: Specify a file to dump all responses and requests to. This file is never overwritten, it's just appended to, so if you have this option set to on often this file will grow rapidly. In this case you need to have a plan how to keep it under control.

## Test file format

This is an example of a test file:

```json
{
  "Input":  [
      { "Command" : "V" , "Payload" : { "lang" : ["05ab1e"] } },
      { "Command" : "F" , "Payload" : { ".code.tio": "\"Hello, World!\"" } },
      { "Command" : "F" , "Payload" : { ".input.tio" : "" } },
      { "Command" : "V" , "Payload" : { "args": [] } },
      { "Command" : "R" }
  ],
  "Output": "Hello, World!"
}
```

The layout of this file is intended to match TryItOnline request protocol soon to be published.


## Note on .net Core installation on Fedora 25

As of the moment of writing the official downloads are provided by Microsoft only for Fedora 23 and 24.
Here is how one can get .net Core up and running on Fedora 25.

On your main TryItOnline serever execute:

```bash
dnf install lldb-devel lttng-ust -y
rm -rf usr
rm -rf /opt/microsoft
rm -f /usr/local/bin/dotnet
mkdir -p /opt/microsoft/dotnet
rpm2cpio https://dl.fedoraproject.org/pub/fedora/linux/releases/24/Server/x86_64/os/Packages/l/libicu-56.1-4.fc24.x86_64.rpm | cpio -dim -D /
rpm2cpio https://dl.fedoraproject.org/pub/fedora/linux/releases/25/Everything/x86_64/os/Packages/l/lldb-3.8.0-2.fc25.x86_64.rpm | cpio -dim -D /
curl -sSL https://go.microsoft.com/fwlink/?LinkID=835025 | tar xz -C /opt/microsoft/dotnet
ln -s /opt/microsoft/dotnet/dotnet /usr/local/bin
```

Now assuming, that your TioTests folder is at `/opt/TioTests`, run:

```bash
cd /opt/TioTests
mkdir -p /root/waldo
TMPDIR=/root/waldo HOME=/root dotnet restore
HOME=/root dotnet build
rm -rf /root/waldo
```

That should be enough to get TioTests up and running.


