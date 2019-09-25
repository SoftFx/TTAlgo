<?xml version="1.0" encoding="utf-8"?>
<Profile xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="BotTerminal.Profile.v2">
  <Bots />
  <Charts>
    <Chart>
      <ChartType>Candle</ChartType>
      <CrosshairEnabled>false</CrosshairEnabled>
      <Id>Chart0</Id>
      <Indicators>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Fast Adaptive Trend Line 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.ATCFMethod.FastAdaptiveTrendLine.FastAdaptiveTrendLine</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>CountBars</d6p1:Key>
                <d6p1:Value>300</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Fatl</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>FATLs 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.ATCFMethod.FATLSignal.FatlSignal</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>CountBars</d6p1:Key>
                <d6p1:Value>300</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetPrice</d6p1:Key>
                <d6p1:Value>Close</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Up</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>1</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Down</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Fast Trend Line Momentum 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.ATCFMethod.FastTrendLineMomentum.FastTrendLineMomentum</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>CountBars</d6p1:Key>
                <d6p1:Value>300</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Ftlm</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.147027269</d6p1:Blue>
                  <d6p1:Green>0.4735315</d6p1:Green>
                  <d6p1:Red>0.50888133</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Perfect Commodity Channel In 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.ATCFMethod.PerfectCommodityChannelIndex.PerfectCommodityChannelIndex</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>CountBars</d6p1:Key>
                <d6p1:Value>300</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Pcci</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Reference Fast Trend Line 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.ATCFMethod.ReferenceFastTrendLine.ReferenceFastTrendLine</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>CountBars</d6p1:Key>
                <d6p1:Value>300</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Rftl</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>1</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Bears Power 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Oscillators.BearsPower.BearsPower</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Period</d6p1:Key>
                <d6p1:Value>13</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetPrice</d6p1:Key>
                <d6p1:Value>Close</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Bears</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.527115166</d6p1:Blue>
                  <d6p1:Green>0.527115166</d6p1:Green>
                  <d6p1:Red>0.527115166</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Ichimoku Kinko Hyo 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Trend.IchimokuKinkoHyo.IchimokuKinkoHyo</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>TenkanSen</d6p1:Key>
                <d6p1:Value>9</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>KijunSen</d6p1:Key>
                <d6p1:Value>26</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>SenkouSpanB</d6p1:Key>
                <d6p1:Value>52</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Tenkan</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Kijun</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>1</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>SenkouA</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.116970673</d6p1:Blue>
                  <d6p1:Green>0.3712377</d6p1:Green>
                  <d6p1:Red>0.9046612</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>DotsRare</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>SenkouB</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.6866853</d6p1:Blue>
                  <d6p1:Green>0.5209956</d6p1:Green>
                  <d6p1:Red>0.6866853</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>DotsRare</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Chikou</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>1</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Alligator 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.BillWilliams.Alligator.Alligator</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>JawsPeriod</d6p1:Key>
                <d6p1:Value>13</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>JawsShift</d6p1:Key>
                <d6p1:Value>8</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>TeethPeriod</d6p1:Key>
                <d6p1:Value>8</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>TeethShift</d6p1:Key>
                <d6p1:Value>5</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>LipsPeriod</d6p1:Key>
                <d6p1:Value>5</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>LipsShift</d6p1:Key>
                <d6p1:Value>3</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetMethod</d6p1:Key>
                <d6p1:Value>Smoothed</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Jaws</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>1</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Teeth</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Lips</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0.215860531</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Fractals 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.BillWilliams.Fractals.Fractals</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:MarkerSeriesOutput">
                <d6p1:Key>FractalsUp</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.215860531</d6p1:Blue>
                  <d6p1:Green>0.215860531</d6p1:Green>
                  <d6p1:Red>0.215860531</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:MarkerSize>Medium</d6p1:MarkerSize>
              </d6p1:property>
              <d6p1:property i:type="d6p1:MarkerSeriesOutput">
                <d6p1:Key>FractalsDown</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.215860531</d6p1:Blue>
                  <d6p1:Green>0.215860531</d6p1:Green>
                  <d6p1:Red>0.215860531</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:MarkerSize>Medium</d6p1:MarkerSize>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Force Index 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Oscillators.ForceIndex.ForceIndex</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Period</d6p1:Key>
                <d6p1:Value>13</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetMethod</d6p1:Key>
                <d6p1:Value>Simple</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetPrice</d6p1:Key>
                <d6p1:Value>Close</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Force</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.4019778</d6p1:Blue>
                  <d6p1:Green>0.445201218</d6p1:Green>
                  <d6p1:Red>0.0144438446</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>MACD 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Oscillators.MACD.Macd</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>FastEma</d6p1:Key>
                <d6p1:Value>12</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>SlowEma</d6p1:Key>
                <d6p1:Value>26</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>MacdSma</d6p1:Key>
                <d6p1:Value>9</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>MacdSeries</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.527115166</d6p1:Blue>
                  <d6p1:Green>0.527115166</d6p1:Green>
                  <d6p1:Red>0.527115166</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Signal</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>DotsRare</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>DeMarker 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Oscillators.DeMarker.DeMarker</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Period</d6p1:Key>
                <d6p1:Value>14</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>DeMark</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0.4019778</d6p1:Blue>
                  <d6p1:Green>0.445201218</d6p1:Green>
                  <d6p1:Red>0.0144438446</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Envelopes 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Trend.Envelopes.Envelopes</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Period</d6p1:Key>
                <d6p1:Value>7</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Shift</d6p1:Key>
                <d6p1:Value>0</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Double">
                <d6p1:Key>Deviation</d6p1:Key>
                <d6p1:Value>0.1</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Enum">
                <d6p1:Key>TargetMethod</d6p1:Key>
                <d6p1:Value>Simple</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BarCloseReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:SecondaryReduction>
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToDoubleInput">
                <d6p1:Key>Price</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBestBidReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>TopLine</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>1</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>BottomLine</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Parabolic SAR 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Trend.ParabolicSAR.ParabolicSar</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Double">
                <d6p1:Key>Step</d6p1:Key>
                <d6p1:Value>0.02</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:Double">
                <d6p1:Key>Maximum</d6p1:Key>
                <d6p1:Value>0.2</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Sar</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>1</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
        <Indicator>
          <Config xmlns:d6p1="TTAlgo.Config.v2">
            <d6p1:InstanceId>Relative Vigor Index 1</d6p1:InstanceId>
            <d6p1:Key>
              <d6p1:DescriptorId>TickTrader.Algo.Indicators.Oscillators.RelativeVigorIndex.RelativeVigorIndex</d6p1:DescriptorId>
              <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
              <d6p1:PackageName>ticktrader.algo.indicators.dll</d6p1:PackageName>
            </d6p1:Key>
            <d6p1:MainSymbol>
              <d6p1:Name>AUDCHF</d6p1:Name>
              <d6p1:Origin>Online</d6p1:Origin>
            </d6p1:MainSymbol>
            <d6p1:Mapping>
              <d6p1:PrimaryReduction>
                <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
              </d6p1:PrimaryReduction>
              <d6p1:SecondaryReduction i:nil="true" />
            </d6p1:Mapping>
            <d6p1:Permissions>
              <d6p1:isolated>true</d6p1:isolated>
              <d6p1:tradeAllowed>true</d6p1:tradeAllowed>
            </d6p1:Permissions>
            <d6p1:Properties>
              <d6p1:property i:type="d6p1:Int">
                <d6p1:Key>Period</d6p1:Key>
                <d6p1:Value>10</d6p1:Value>
              </d6p1:property>
              <d6p1:property i:type="d6p1:BarToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.BidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:QuoteToBarInput">
                <d6p1:Key>Bars</d6p1:Key>
                <d6p1:Symbol>
                  <d6p1:Name>AUDCHF</d6p1:Name>
                  <d6p1:Origin>Online</d6p1:Origin>
                </d6p1:Symbol>
                <d6p1:Mapping>
                  <d6p1:PrimaryReduction>
                    <d6p1:DescriptorId>TickTrader.Algo.Ext.QuoteBidBarReduction</d6p1:DescriptorId>
                    <d6p1:PackageLocation>Embedded</d6p1:PackageLocation>
                    <d6p1:PackageName>ticktrader.algo.ext.dll</d6p1:PackageName>
                  </d6p1:PrimaryReduction>
                  <d6p1:SecondaryReduction i:nil="true" />
                </d6p1:Mapping>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>RviAverage</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0.215860531</d6p1:Green>
                  <d6p1:Red>0</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
              <d6p1:property i:type="d6p1:ColoredLineOutput">
                <d6p1:Key>Signal</d6p1:Key>
                <d6p1:Color>
                  <d6p1:Alpha>1</d6p1:Alpha>
                  <d6p1:Blue>0</d6p1:Blue>
                  <d6p1:Green>0</d6p1:Green>
                  <d6p1:Red>1</d6p1:Red>
                </d6p1:Color>
                <d6p1:Enabled>true</d6p1:Enabled>
                <d6p1:Thickness>1</d6p1:Thickness>
                <d6p1:LineStyle>Solid</d6p1:LineStyle>
              </d6p1:property>
            </d6p1:Properties>
            <d6p1:TimeFrame>M1</d6p1:TimeFrame>
          </Config>
        </Indicator>
      </Indicators>
      <Period>M1</Period>
      <Symbol>AUDCHF</Symbol>
    </Chart>
  </Charts>
  <Layout>&lt;?xml version="1.0"?&gt;
&lt;LayoutRoot&gt;
  &lt;RootPanel Orientation="Horizontal"&gt;
    &lt;LayoutPanel Orientation="Vertical" DockWidth="290" DocMinWidth="200"&gt;
      &lt;LayoutAnchorablePane&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="Symbols" IsSelected="True" ContentId="Tab_Symbols" CanClose="False" LastActivationTimeStamp="07/10/2019 11:45:09" /&gt;
      &lt;/LayoutAnchorablePane&gt;
      &lt;LayoutAnchorablePane&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="Bot Instances" IsSelected="True" ContentId="Tab_Bots" CanClose="False" LastActivationTimeStamp="07/05/2019 14:42:35" /&gt;
      &lt;/LayoutAnchorablePane&gt;
      &lt;LayoutAnchorablePane&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="Algo Repository" IsSelected="True" ContentId="Tab_Algo" CanClose="False" /&gt;
      &lt;/LayoutAnchorablePane&gt;
    &lt;/LayoutPanel&gt;
    &lt;LayoutPanel Orientation="Vertical"&gt;
      &lt;LayoutDocumentPaneGroup Orientation="Horizontal"&gt;
        &lt;LayoutDocumentPane&gt;
          &lt;LayoutDocument Title="AUDCHF, M1" IsSelected="True" IsLastFocusedDocument="True" ContentId="Chart0" LastActivationTimeStamp="09/05/2019 10:22:18" /&gt;
        &lt;/LayoutDocumentPane&gt;
      &lt;/LayoutDocumentPaneGroup&gt;
      &lt;LayoutAnchorablePane DockHeight="300"&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="Trade" IsSelected="True" ContentId="Tab_Trade" CanClose="False" /&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="History" ContentId="Tab_History" CanClose="False" /&gt;
        &lt;LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="Journal" ContentId="Tab_Journal" CanClose="False" /&gt;
      &lt;/LayoutAnchorablePane&gt;
    &lt;/LayoutPanel&gt;
  &lt;/RootPanel&gt;
  &lt;TopSide /&gt;
  &lt;RightSide /&gt;
  &lt;LeftSide /&gt;
  &lt;BottomSide /&gt;
  &lt;FloatingWindows /&gt;
  &lt;Hidden /&gt;
&lt;/LayoutRoot&gt;</Layout>
  <SelectedChart>AUDCHF</SelectedChart>
  <ViewModelStorages>
    <ViewModelStorage>
      <Name>NetPositions</Name>
      <Properties />
    </ViewModelStorage>
    <ViewModelStorage>
      <Name>Orders</Name>
      <Properties />
    </ViewModelStorage>
    <ViewModelStorage>
      <Name>History</Name>
      <Properties />
    </ViewModelStorage>
    <ViewModelStorage>
      <Name>NetPositionsBacktester</Name>
      <Properties />
    </ViewModelStorage>
    <ViewModelStorage>
      <Name>OrdersBacktester</Name>
      <Properties />
    </ViewModelStorage>
    <ViewModelStorage>
      <Name>HistoryBacktester</Name>
      <Properties />
    </ViewModelStorage>
  </ViewModelStorages>
</Profile>