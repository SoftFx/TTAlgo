//+------------------------------------------------------------------+
//|                                                    FTLM-STLM.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_separate_window
#property indicator_buffers 2
#property indicator_color1 DarkKhaki
#property indicator_color2 DarkSalmon
//---- input parameters
extern int       CountBars=2500;
//---- buffers
double ExtMapBuffer1[];
double ExtMapBuffer2[];
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
  {
//---- indicators
   SetIndexStyle(0,DRAW_LINE);
   SetIndexBuffer(0,ExtMapBuffer1);
   SetIndexStyle(1,DRAW_LINE);
   SetIndexBuffer(1,ExtMapBuffer2);
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
    int i,AccountedBars,shift,value1,value2,value3,value4;
//---- TODO: add your code here

AccountedBars = Bars-CountBars;

   for(i=AccountedBars;i<=Bars-1;i++) 
   {
   
   shift = Bars - 1 - i;
   
  ExtMapBuffer1[shift]= 0.4360409450*Close[shift+0]
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
+0.0040364019*Close[shift+38]
-(-0.0025097319*Close[shift+0]
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
+0.0018747783*Close[shift+43]);

ExtMapBuffer2[shift]= 0.0982862174*Close[shift+0]
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
+0.0161380976*Close[shift+64]
-(-0.0074151919*Close[shift+0]
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
+0.0073925495*Close[shift+90]);


   
   }
   
//----
   return(0);
  }
//+------------------------------------------------------------------+