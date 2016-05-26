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
  	- [x] [Average Directional Movement Index](https://intranet.fxopen.org/jira/browse/TTALGO-57)
  	- [x] [Bollinger Bands](https://intranet.fxopen.org/jira/browse/TTALGO-76) (extended version of Common/Bands)
  	- [x] [Envelopes](https://intranet.fxopen.org/jira/browse/TTALGO-58)
  	- [x] [Ichimoku Kinko Hyo](https://intranet.fxopen.org/jira/browse/TTALGO-77) (extended version of Common/Ichimoku)
  	- [x] [Moving Average](https://intranet.fxopen.org/jira/browse/TTALGO-78) (extended version of Common/Custom Moving Averages)
  	- [x] [Parabolic SAR](https://intranet.fxopen.org/jira/browse/TTALGO-79) (extended version of Common/Parabolic)
  	- [x] [Standard Deviation](https://intranet.fxopen.org/jira/browse/TTALGO-59)
 - Oscillators
 	- [x] [Average True Range](https://intranet.fxopen.org/jira/browse/TTALGO-60)
 	- [x] [Bears Power](https://intranet.fxopen.org/jira/browse/TTALGO-80) (extended version of Common/Bears)
 	- [x] [Bulls Power](https://intranet.fxopen.org/jira/browse/TTALGO-81) (extended version of Common/Bulls)
 	- [x] [Commodity Channel Index](https://intranet.fxopen.org/jira/browse/TTALGO-61)
 	- [x] [DeMarker](https://intranet.fxopen.org/jira/browse/TTALGO-62)
 	- [x] [Force Index](https://intranet.fxopen.org/jira/browse/TTALGO-63)
 	- [x] [MACD](https://intranet.fxopen.org/jira/browse/TTALGO-82) (extended version of Common/MACD)
 	- [x] [Momentum](https://intranet.fxopen.org/jira/browse/TTALGO-83) (extended version of Common/Momentum)
 	- [x] [Moving Average of Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-64)
 	- [x] [Relative Strength Index](https://intranet.fxopen.org/jira/browse/TTALGO-65)
 	- [x] [Relative Vigor Index](https://intranet.fxopen.org/jira/browse/TTALGO-66)
 	- [x] [Stochastic Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-84) (extended version of Common/Stochastic)
 	- [x] [Williams' Percent Range](https://intranet.fxopen.org/jira/browse/TTALGO-67)
 - Volumes
 	- [x] [Accumulation/Distribution](https://intranet.fxopen.org/jira/browse/TTALGO-85) (extended version of Common/Accumulation)
 	- [x] [Money Flow Index](https://intranet.fxopen.org/jira/browse/TTALGO-68)
 	- [x] [On Balance Volume](https://intranet.fxopen.org/jira/browse/TTALGO-69)
 	- [x] [Volumes](https://intranet.fxopen.org/jira/browse/TTALGO-70)
 - Bill Williams
 	- [x] [Accelerator Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-86) (extended version of Common/Accelerator)
 	- [x] [Alligator](https://intranet.fxopen.org/jira/browse/TTALGO-87) (extended version of Common/Alligator)
 	- [x] [Awesome Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-88) (extended version of Common/Awesome)
 	- [x] [Fractals](https://intranet.fxopen.org/jira/browse/TTALGO-71)
 	- [x] [Gator Oscillator](https://intranet.fxopen.org/jira/browse/TTALGO-72)
 	- [x] [Market Facilitation Index](https://intranet.fxopen.org/jira/browse/TTALGO-73)
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
 - Move = 7 (Close - Open)
 - Range = 8 (High - Low)

## Moving Average methods
 - Simple = 0
 - Exponential = 1 (Smooth Factor = 2/(Period + 1) as in Meta)
 - Smoothed = 2
 - LinearWeighted = 3
 - Custom Exponential = 4 (Smooth Factor can be specified. Found [here](https://intranet.fxopen.org/wiki/display/TIC/Moving+Average))
 - Triangular = 5 (Found [here](https://intranet.fxopen.org/wiki/display/TIC/Moving+Average))

## Digital Indicators List ([task](https://intranet.fxopen.org/jira/browse/TTALGO-105))
 - [x] Fast Adaptive Trend Line
 - [x] Slow Adaptive Trend Line
 - [x] Reference Fast Trend Line
 - [x] Reference Slow Trend Line
 - [x] Fast Trend Line Momentum
 - [x] Slow Trend Line Momentum
 - [x] FTLM-STLM
 - [x] Perfect Commodity Channel Index
 - [x] FATL Signal
 - [x] Range Channel Bound Index

## News Indicator ([task](https://intranet.fxopen.org/jira/browse/TTALGO-1))

News Source: [FxStreet](http://www.fxstreet.com/economic-calendar/)

Cache can be found in {APP_DIR}\NewsCache\ direcroty.

Db filename is formed like "{SourceName}-{CurrencyCode}.db". In this case our source is FxStreet.
