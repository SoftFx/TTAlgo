//+------------------------------------------------------------------+
//|                                                         SATL.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_chart_window
#property indicator_buffers 1
#property indicator_color1 Cornsilk
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
   ExtMapBuffer1[shift]=0.0982862174*Close[shift+0]
+0.0975682269*Close[shift+1]
+0.0961401078*Close[shift+2]
+0.0940230544*Close[shift+3]
+0.0912437090*Close[shift+4]
+0.0878391006*Close[shift+5]
+0.0838544303*Close[shift+6]
+0.0793406350*Close[shift+7]
+0.0743569346*Close[shift+8]
+0.0689666682*Close[shift+9]
+0.0632381578*Close[shift+10]
+0.0572428925*Close[shift+11]
+0.0510534242*Close[shift+12]
+0.0447468229*Close[shift+13]
+0.0383959950*Close[shift+14]
+0.0320735368*Close[shift+15]
+0.0258537721*Close[shift+16]
+0.0198005183*Close[shift+17]
+0.0139807863*Close[shift+18]
+0.0084512448*Close[shift+19]
+0.0032639979*Close[shift+20]
-0.0015350359*Close[shift+21]
-0.0059060082*Close[shift+22]
-0.0098190256*Close[shift+23]
-0.0132507215*Close[shift+24]
-0.0161875265*Close[shift+25]
-0.0186164872*Close[shift+26]
-0.0205446727*Close[shift+27]
-0.0219739146*Close[shift+28]
-0.0229204861*Close[shift+29]
-0.0234080863*Close[shift+30]
-0.0234566315*Close[shift+31]
-0.0231017777*Close[shift+32]
-0.0223796900*Close[shift+33]
-0.0213300463*Close[shift+34]
-0.0199924534*Close[shift+35]
-0.0184126992*Close[shift+36]
-0.0166377699*Close[shift+37]
-0.0147139428*Close[shift+38]
-0.0126796776*Close[shift+39]
-0.0105938331*Close[shift+40]
-0.0084736770*Close[shift+41]
-0.0063841850*Close[shift+42]
-0.0043466731*Close[shift+43]
-0.0023956944*Close[shift+44]
-0.0005535180*Close[shift+45]
+0.0011421469*Close[shift+46]
+0.0026845693*Close[shift+47]
+0.0040471369*Close[shift+48]
+0.0052380201*Close[shift+49]
+0.0062194591*Close[shift+50]
+0.0070340085*Close[shift+51]
+0.0076266453*Close[shift+52]
+0.0080376628*Close[shift+53]
+0.0083037666*Close[shift+54]
+0.0083694798*Close[shift+55]
+0.0082901022*Close[shift+56]
+0.0080741359*Close[shift+57]
+0.0077543820*Close[shift+58]
+0.0073260526*Close[shift+59]
+0.0068163569*Close[shift+60]
+0.0062325477*Close[shift+61]
+0.0056078229*Close[shift+62]
+0.0049516078*Close[shift+63]
+0.0161380976*Close[shift+64];


}


//----
   return(0);
  }
//+------------------------------------------------------------------+