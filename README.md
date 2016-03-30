Algo project
============

## Getting started

In order to compile project it is required to add NuGet Package Sources for
[SciChart](http://support.scichart.com/index.php?/Knowledgebase/Article/View/17232/37/getting-nightly-builds-with-nuget):
 1. primary
 	 - https://www.myget.org/F/abtsoftware/api/v2  (Visual Studio 2013 or below)
 	 - https://www.myget.org/F/abtsoftware/api/v3/index.json (Visual Studio 2015 or above)
 2. secondary
 	 - http://abtsoftware-prd.cloudapp.net:81/nuget/ABTSoftware

## MetaTrader Indicators List

 - Trend
  	- [ ] [Average Directional Movement Index](https://intranet.fxopen.org/jira/browse/TTALGO-57)
  	- [ ] [Bollinger Bands](https://intranet.fxopen.org/jira/browse/TTALGO-76) (extended version of Common/Bands)
  	- [ ] [Envelopes](https://intranet.fxopen.org/jira/browse/TTALGO-58)
  	- [ ] [Ichimoku Kinko Hyo](https://intranet.fxopen.org/jira/browse/TTALGO-77) (extended version of Common/Ichimoku)
  	- [ ] [Moving Average](https://intranet.fxopen.org/jira/browse/TTALGO-78) (extended version of Common/Custom Moving Averages)
  	- [ ] [Parabolic SAR](https://intranet.fxopen.org/jira/browse/TTALGO-79) (extended version of Common/Parabolic)
  	- [ ] [Standard Deviation](https://intranet.fxopen.org/jira/browse/TTALGO-59)
 - Oscillators
 	- [ ] [Average True Range](https://intranet.fxopen.org/jira/browse/TTALGO-60)
 	- [ ] [Bears Power](https://intranet.fxopen.org/jira/browse/TTALGO-80) (extended version of Common/Bears)
 	- [ ] [Bulls Power](https://intranet.fxopen.org/jira/browse/TTALGO-81) (extended version of Common/Bulls)
 	- [ ] [Commodity Channel Index](https://intranet.fxopen.org/jira/browse/TTALGO-61)
 	- [ ] [DeMarker](https://intranet.fxopen.org/jira/browse/TTALGO-62)
 	- [ ] [Force Index](https://intranet.fxopen.org/jira/browse/TTALGO-63)
 	- [ ] [MACD](https://intranet.fxopen.org/jira/browse/TTALGO-82) (extended version of Common/MACD)
 	- [ ] [Momentum](https://intranet.fxopen.org/jira/browse/TTALGO-83) (extended version of Common/Momentum)
 	- [ ] [Moving Average of Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-64)
 	- [ ] [Relative Strength Index](https://intranet.fxopen.org/jira/browse/TTALGO-65)
 	- [ ] [Relative Vigor Index](https://intranet.fxopen.org/jira/browse/TTALGO-66)
 	- [ ] [Stochastic Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-84) (extended version of Common/Stochastic)
 	- [ ] [Williams' Percent Range](https://intranet.fxopen.org/jira/browse/TTALGO-67)
 - Volumes
 	- [ ] [Accumulation/Distribution](https://intranet.fxopen.org/jira/browse/TTALGO-85) (extended version of Common/Accumulation)
 	- [ ] [Money Flow Index](https://intranet.fxopen.org/jira/browse/TTALGO-68)
 	- [ ] [On Balance Volume](https://intranet.fxopen.org/jira/browse/TTALGO-69)
 	- [ ] [Volumes](https://intranet.fxopen.org/jira/browse/TTALGO-70)
 - Bill Williams
 	- [ ] [Accelerator Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-86) (extended version of Common/Accelerator)
 	- [ ] [Alligator](https://intranet.fxopen.org/jira/browse/TTALGO-87) (extended version of Common/Alligator)
 	- [ ] [Awesome Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-88) (extended version of Common/Awesome)
 	- [ ] [Fractals](https://intranet.fxopen.org/jira/browse/TTALGO-71)
 	- [ ] [Gator Oscillators](https://intranet.fxopen.org/jira/browse/TTALGO-72)
 	- [ ] [Market Facilitation Index](https://intranet.fxopen.org/jira/browse/TTALGO-73)
 - Common
	 - [x] Accelerator
	 - [x] Accumulation
	 - [x] Alligator
	 - [x] ATR
	 - [x] Awesome
	 - [x] Bands
	 - [x] Bears
	 - [x] Bulls
	 - [x] CCI
	 - [x] Custom Moving Averages
	 - [x] Heiken Ashi
	 - [x] Ichimoku
	 - [ ] [iExposure](https://intranet.fxopen.org/jira/browse/TTALGO-74)
	 - [x] MACD
	 - [x] Momentum
	 - [x] OsMA
	 - [x] Parabolic
	 - [x] RSI
	 - [x] Stochastic
	 - [x] ZigZag

## Applied Price values('Apply To' field)
 - Close = 0
 - Open = 1
 - High = 2
 - Low = 3
 - Median = 4
 - Typical = 5
 - Weighted = 6

## Moving Average methods
 - Simple = 0
 - Exponential = 1 (Smooth Factor = 2/(Period + 1) as in Meta)
 - Smoothed = 2
 - LinearWeighted = 3
 - Custom Exponential = 4 (Smooth Factor can be specified. Found [here](https://intranet.fxopen.org/wiki/display/TIC/Moving+Average))
 - Triangular = 5 (Found [here](https://intranet.fxopen.org/wiki/display/TIC/Moving+Average))

