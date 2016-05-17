//+------------------------------------------------------------------+
//|                                                         RBCI.mq4 |
//|                       Copyright © 2005, Finware Technologies Ltd |
//|                                            http://www.finware.ru |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2005, Finware Technologies Ltd"
#property link      "http://www.finware.ru"

#property indicator_separate_window
#property indicator_buffers 5
#property indicator_color1 Teal
#property indicator_color2 DarkOrange
#property indicator_color3 DarkOrange
#property indicator_color4 DarkOrange
#property indicator_color5 DarkOrange
//---- input parameters
extern int       CountBars=300;
extern int       STD=18;
//---- buffers
double ExtMapBuffer1[];
double ExtMapBuffer2[];
double ExtMapBuffer3[];
double ExtMapBuffer4[];
double ExtMapBuffer5[];
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
  {
//---- indicators
   SetIndexStyle(0,DRAW_LINE);
   SetIndexBuffer(0,ExtMapBuffer1);
   SetIndexStyle(1,DRAW_LINE,STYLE_DOT,1);
   SetIndexBuffer(1,ExtMapBuffer2);
   SetIndexStyle(2,DRAW_LINE,STYLE_DOT,1);
   SetIndexBuffer(2,ExtMapBuffer3);
   SetIndexStyle(3,DRAW_LINE,STYLE_DOT,1);
   SetIndexBuffer(3,ExtMapBuffer4);
   SetIndexStyle(4,DRAW_LINE,STYLE_DOT,1);
   SetIndexBuffer(4,ExtMapBuffer5);
//----
   SetIndexEmptyValue(0, 0.0);
   SetIndexEmptyValue(1, 0.0);
   SetIndexEmptyValue(2, 0.0);
   SetIndexEmptyValue(3, 0.0);
   SetIndexEmptyValue(4, 0.0);
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
//---- TODO: add your code here
   
      int i,AccountedBars,shift;
      
      double ArRBCI[];
      double AV, Sum;
//---- TODO: add your code here

AccountedBars = Bars-CountBars;

   Sum=0;
   
   for(i=AccountedBars;i<=Bars-1;i++) 
   {
   
   shift = Bars - 1 - i;
   ExtMapBuffer1[shift]=(
-35.5241819400*Close[shift+0]
-29.3339896500*Close[shift+1]
-18.4277449600*Close[shift+2]
-5.3418475670*Close[shift+3]
+7.0231636950*Close[shift+4]
+16.1762815600*Close[shift+5]
+20.6566210400*Close[shift+6]
+20.3266115800*Close[shift+7]
+16.2702390600*Close[shift+8]
+10.3524012700*Close[shift+9]
+4.5964239920*Close[shift+10]
+0.5817527531*Close[shift+11]
-0.9559211961*Close[shift+12]
-0.2191111431*Close[shift+13]
+1.8617342810*Close[shift+14]
+4.0433304300*Close[shift+15]
+5.2342243280*Close[shift+16]
+4.8510862920*Close[shift+17]
+2.9604408870*Close[shift+18]
+0.1815496232*Close[shift+19]
-2.5919387010*Close[shift+20]
-4.5358834460*Close[shift+21]
-5.1808556950*Close[shift+22]
-4.5422535300*Close[shift+23]
-3.0671459820*Close[shift+24]
-1.4310126580*Close[shift+25]
-0.2740437883*Close[shift+26]
+0.0260722294*Close[shift+27]
-0.5359717954*Close[shift+28]
-1.6274916400*Close[shift+29]
-2.7322958560*Close[shift+30]
-3.3589596820*Close[shift+31]
-3.2216514550*Close[shift+32]
-2.3326257940*Close[shift+33]
-0.9760510577*Close[shift+34]
+0.4132650195*Close[shift+35]
+1.4202166770*Close[shift+36]
+1.7969987350*Close[shift+37]
+1.5412722800*Close[shift+38]
+0.8771442423*Close[shift+39]
+0.1561848839*Close[shift+40]
-0.2797065802*Close[shift+41]
-0.2245901578*Close[shift+42]
+0.3278853523*Close[shift+43]
+1.1887841480*Close[shift+44]
+2.0577410750*Close[shift+45]
+2.6270409820*Close[shift+46]
+2.6973742340*Close[shift+47]
+2.2289941280*Close[shift+48]
+1.3536792430*Close[shift+49]
+0.3089253193*Close[shift+50]
-0.6386689841*Close[shift+51]
-1.2766707670*Close[shift+52]
-1.5136918450*Close[shift+53]
-1.3775160780*Close[shift+54]
-1.6156173970*Close[shift+55]);

Sum = Sum + ExtMapBuffer1[shift];

};

AV = Sum / CountBars;

for(i=AccountedBars;i<=Bars-1;i++) 
   {
   shift = Bars - 1 - i;
   ExtMapBuffer1[shift]=AV-ExtMapBuffer1[shift];

}
double Value2,Value4,Value6,Value5,Value8,Value9,Pow;
int Value3,Value7;

for(i=AccountedBars;i<=Bars-1;i++) 
   {
   
   shift = Bars - 1 - i;
   
   
Value2 = STD;
Value4 = 0;
for(Value3=0;Value3<=Value2-1;Value3++) 
{
Value4 = Value4 -ExtMapBuffer1[shift+Value3];
};


Value5 = Value4 / Value2;
Value6 = 0;
Value4 = 0;

for(Value7=0;Value7<=Value2-1;Value7++) 
{
Pow = -ExtMapBuffer1[shift+Value7]-Value5;
Value6 = Value6 + MathPow(Pow,2);
};

Value8 = Value6/(Value2-1);
Value9 = MathSqrt(Value8);

   
   ExtMapBuffer2[shift]=+Value9;
   ExtMapBuffer3[shift]=-Value9;
   ExtMapBuffer4[shift]=+2*Value9;
   ExtMapBuffer5[shift]=-2*Value9;
   }
   
//----
   return(0);
  }
//+------------------------------------------------------------------+