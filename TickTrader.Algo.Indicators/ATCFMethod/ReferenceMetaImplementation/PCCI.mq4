//+------------------------------------------------------------------+
//|                                                         PCCI.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_separate_window
#property indicator_buffers 1
#property indicator_color1 Red
//---- input parameters
extern int       CountBars=2500;
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
   ExtMapBuffer1[shift]=Close[shift]-(+0.4360409450*Close[shift+0]
   
+0.3658689069*Close[shift+1]
+0.2460452079*Close[shift+2]
+0.1104506886*Close[shift+3]
-0.0054034585*Close[shift+4]
-0.0760367731*Close[shift+5]
-0.0933058722*Close[shift+6]
-0.0670110374*Close[shift+7]
-0.0190795053*Close[shift+8]
+0.0259609206*Close[shift+9]
+0.0502044896*Close[shift+10]
+0.0477818607*Close[shift+11]
+0.0249252327*Close[shift+12]
-0.0047706151*Close[shift+13]
-0.0272432537*Close[shift+14]
-0.0338917071*Close[shift+15]
-0.0244141482*Close[shift+16]
-0.0055774838*Close[shift+17]
+0.0128149838*Close[shift+18]
+0.0226522218*Close[shift+19]
+0.0208778257*Close[shift+20]
+0.0100299086*Close[shift+21]
-0.0036771622*Close[shift+22]
-0.0136744850*Close[shift+23]
-0.0160483392*Close[shift+24]
-0.0108597376*Close[shift+25]
-0.0016060704*Close[shift+26]
+0.0069480557*Close[shift+27]
+0.0110573605*Close[shift+28]
+0.0095711419*Close[shift+29]
+0.0040444064*Close[shift+30]
-0.0023824623*Close[shift+31]
-0.0067093714*Close[shift+32]
-0.0072003400*Close[shift+33]
-0.0047717710*Close[shift+34]
+0.0005541115*Close[shift+35]
+0.0007860160*Close[shift+36]
+0.0130129076*Close[shift+37]
+0.0040364019*Close[shift+38]);

}


//----
   return(0);
  }
//+------------------------------------------------------------------+