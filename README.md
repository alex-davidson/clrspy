# CLRSpy 

Extremely basic tool for capturing stack-traces of .NET applications non-invasively. Should work against 32-bit or 64-bit processes.

	clrspy [-v] [-x] <pid>
    clrspy [-v] [-x] -p <pid>



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