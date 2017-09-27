# CLRSpy 

[![Build status](https://ci.appveyor.com/api/projects/status/tjh1lycjs4s830wd/branch/master?svg=true)](https://ci.appveyor.com/project/alex-davidson/clrspy/branch/master)
* Download: [ClrSpy.exe](https://ci.appveyor.com/api/projects/alex-davidson/clrspy/artifacts/ClrSpy/bin/Release/ClrSpy.exe?branch=master)
* Symbols: [ClrSpy.pdb](https://ci.appveyor.com/api/projects/alex-davidson/clrspy/artifacts/ClrSpy/bin/Release/ClrSpy.pdb?branch=master)

Extremely basic debug/dump tool. Should work against 32-bit or 64-bit processes.

All usages expect either a running process or a dumpfile as their input.
* Processes may be specified by PID or name. If the name is ambiguous, matching processes will be summarised so that the precise PID may be more easily identified.
* Attachment to a process will be noninvasive unless the -x switch is specified. This will usually pause the target process, so beware when using it against production systems.
* Some commands require invasive attachment and therefore require the -x switch when working with live processes.
* Consuming information from dumpfiles never requires the -x switch.

### showstacks

    clrspy showstacks [-v] [-x] [-p <pid>] [-n <name>] [-d <dumpfile>]

Writes stack traces and object types to STDOUT. May inspect either a dumpfile or a running process.
Generally a fast operation. The -x switch permits retrieval of more accurate information.

### showheap

    clrspy showheap -x [-v] [-p <pid>] [-n <name>] [-d <dumpfile>]
    
Writes tabulated heap usage statistics to STDOUT, grouped by object type and in descending order of total allocated size.
May take some time to run, and it requires -x.

### dumpmemory

    clrspy dumpmemory -x [-d <dumpfile>] [-v] [-p <pid>] [-n <name>] [-f]

Create a full memory dumpfile from a process. Includes the process's entire memory space and symbol tables if possible.
If the dumpfile is not specified, 'memorydump-<pid>' will be used instead.
May take some time to run, and it requires -x.

## Licence

This project is licensed under the UNLICENSE (where valid; MIT Licence otherwise). You can do whatever you like with it, or at least that is my intent.

Other licence terms apply to its dependencies and some components:
* log4net: Apache Licence v2
* Costura: MIT Licence
* Fody: MIT Licence
* Microsoft.Diagnostics.Runtime (CLRMD): Microsoft Licence?
* NDesk.Options (Options.cs): MIT Licence(?)
* DumpStacksJob.cs is based on the CLRMD 'ClrStack' sample project: MIT Licence

As a side-effect of one or more of these licences, it may not be permissible to eg. sell the compiled binary.
