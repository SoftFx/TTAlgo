//+------------------------------------------------------------------+
//|                                                         RSTL.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_chart_window
#property indicator_buffers 1
#property indicator_color1 DeepSkyBlue
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
   ExtMapBuffer1[shift]=-0.0074151919*Close[shift+0]
-0.0060698985*Close[shift+1]
-0.0044979052*Close[shift+2]
-0.0027054278*Close[shift+3]
-0.0007031702*Close[shift+4]
+0.0014951741*Close[shift+5]
+0.0038713513*Close[shift+6]
+0.0064043271*Close[shift+7]
+0.0090702334*Close[shift+8]
+0.0118431116*Close[shift+9]
+0.0146922652*Close[shift+10]
+0.0175884606*Close[shift+11]
+0.0204976517*Close[shift+12]
+0.0233865835*Close[shift+13]
+0.0262218588*Close[shift+14]
+0.0289681736*Close[shift+15]
+0.0315922931*Close[shift+16]
+0.0340614696*Close[shift+17]
+0.0363444061*Close[shift+18]
+0.0384120882*Close[shift+19]
+0.0402373884*Close[shift+20]
+0.0417969735*Close[shift+21]
+0.0430701377*Close[shift+22]
+0.0440399188*Close[shift+23]
+0.0446941124*Close[shift+24]
+0.0450230100*Close[shift+25]
+0.0450230100*Close[shift+26]
+0.0446941124*Close[shift+27]
+0.0440399188*Close[shift+28]
+0.0430701377*Close[shift+29]
+0.0417969735*Close[shift+30]
+0.0402373884*Close[shift+31]
+0.0384120882*Close[shift+32]
+0.0363444061*Close[shift+33]
+0.0340614696*Close[shift+34]
+0.0315922931*Close[shift+35]
+0.0289681736*Close[shift+36]
+0.0262218588*Close[shift+37]
+0.0233865835*Close[shift+38]
+0.0204976517*Close[shift+39]
+0.0175884606*Close[shift+40]
+0.0146922652*Close[shift+41]
+0.0118431116*Close[shift+42]
+0.0090702334*Close[shift+43]
+0.0064043271*Close[shift+44]
+0.0038713513*Close[shift+45]
+0.0014951741*Close[shift+46]
-0.0007031702*Close[shift+47]
-0.0027054278*Close[shift+48]
-0.0044979052*Close[shift+49]
-0.0060698985*Close[shift+50]
-0.0074151919*Close[shift+51]
-0.0085278517*Close[shift+52]
-0.0094111161*Close[shift+53]
-0.0100658241*Close[shift+54]
-0.0104994302*Close[shift+55]
-0.0107227904*Close[shift+56]
-0.0107450280*Close[shift+57]
-0.0105824763*Close[shift+58]
-0.0102517019*Close[shift+59]
-0.0097708805*Close[shift+60]
-0.0091581551*Close[shift+61]
-0.0084345004*Close[shift+62]
-0.0076214397*Close[shift+63]
-0.0067401718*Close[shift+64]
-0.0058083144*Close[shift+65]
-0.0048528295*Close[shift+66]
-0.0038816271*Close[shift+67]
-0.0029244713*Close[shift+68]
-0.0019911267*Close[shift+69]
-0.0010974211*Close[shift+70]
-0.0002535559*Close[shift+71]
+0.0005231953*Close[shift+72]
+0.0012297491*Close[shift+73]
+0.0018539149*Close[shift+74]
+0.0023994354*Close[shift+75]
+0.0028490136*Close[shift+76]
+0.0032221429*Close[shift+77]
+0.0034936183*Close[shift+78]
+0.0036818974*Close[shift+79]
+0.0038037944*Close[shift+80]
+0.0038338964*Close[shift+81]
+0.0037975350*Close[shift+82]
+0.0036986051*Close[shift+83]
+0.0035521320*Close[shift+84]
+0.0033559226*Close[shift+85]
+0.0031224409*Close[shift+86]
+0.0028550092*Close[shift+87]
+0.0025688349*Close[shift+88]
+0.0022682355*Close[shift+89]
+0.0073925495*Close[shift+90];


}


//----
   return(0);
  }
//+------------------------------------------------------------------+