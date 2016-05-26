//+------------------------------------------------------------------+
//|                                                         RFTL.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_chart_window
#property indicator_buffers 1
#property indicator_color1 Blue
//---- input parameters
extern int       CountBars=300;
//---- buffers
double ExtMapBuffer1[];
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
  {
//---- indicators
   SetIndexStyle(0,DRAW_LINE);
   SetIndexBuffer(0,ExtMapBuffer1);
//----
   SetIndexEmptyValue(0, 0.0);
   return(0);
  }
//+------------------------------------------------------------------+
//| Custor indicator deinitialization function                       |
//+------------------------------------------------------------------+
int deinit()
  {
//---- TODO: add your code here
   
//----
   return(0);
  }
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int start()
  {
   int    counted_bars=IndicatorCounted();
   int i,AccountedBars,shift;
//---- TODO: add your code here

AccountedBars = Bars-CountBars;
   
   for(i=AccountedBars;i<=Bars-1;i++) 
   {
   
   shift = Bars - 1 - i;
   ExtMapBuffer1[shift]=-0.0025097319*Close[shift+0]
+0.0513007762*Close[shift+1]
+0.1142800493*Close[shift+2]
+0.1699342860*Close[shift+3]
+0.2025269304*Close[shift+4]
+0.2025269304*Close[shift+5]
+0.1699342860*Close[shift+6]
+0.1142800493*Close[shift+7]
+0.0513007762*Close[shift+8]
-0.0025097319*Close[shift+9]
-0.0353166244*Close[shift+10]
-0.0433375629*Close[shift+11]
-0.0311244617*Close[shift+12]
-0.0088618137*Close[shift+13]
+0.0120580088*Close[shift+14]
+0.0233183633*Close[shift+15]
+0.0221931304*Close[shift+16]
+0.0115769653*Close[shift+17]
-0.0022157966*Close[shift+18]
-0.0126536111*Close[shift+19]
-0.0157416029*Close[shift+20]
-0.0113395830*Close[shift+21]
-0.0025905610*Close[shift+22]
+0.0059521459*Close[shift+23]
+0.0105212252*Close[shift+24]
+0.0096970755*Close[shift+25]
+0.0046585685*Close[shift+26]
-0.0017079230*Close[shift+27]
-0.0063513565*Close[shift+28]
-0.0074539350*Close[shift+29]
-0.0050439973*Close[shift+30]
-0.0007459678*Close[shift+31]
+0.0032271474*Close[shift+32]
+0.0051357867*Close[shift+33]
+0.0044454862*Close[shift+34]
+0.0018784961*Close[shift+35]
-0.0011065767*Close[shift+36]
-0.0031162862*Close[shift+37]
-0.0033443253*Close[shift+38]
-0.0022163335*Close[shift+39]
+0.0002573669*Close[shift+40]
+0.0003650790*Close[shift+41]
+0.0060440751*Close[shift+42]
+0.0018747783*Close[shift+43];

}


//----
   return(0);
  }
//+------------------------------------------------------------------+