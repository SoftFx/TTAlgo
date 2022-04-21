TickTrader Algo
============

## Overview

TickTrader Algo is made for developing and running algorithmic trading systems.


## Quick start guide

In order to start developing your own bots, you will need to install:
 - Visual Studio with .NET desktop workload or .NET SDK if you are familar with dotnet CLI
 - Install or compile AlgoServer and AlgoTerminal.

After that you need to install Visual studio extension or grab project templates for dotnet CLI from [nuget](https://www.nuget.org/packages/TickTrader.Algo.Templates/).
```
dotnet new --install TickTrader.Algo.Templates::1.0.0
```
With one of them install you can create sample trading bot project from templates and compile it. By default compiled package is exported to %USERPROFILE%\Documents\AlgoTerminal\AlgoRepository. You should be able to run it in AlgoTerminal now. For details on how UI works see [help](docs/TTAlgoHelp.pdf).
