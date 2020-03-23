//+------------------------------------------------------------------+
//|                                                         RBCI.mq5 |
//|                                  Copyright 2002, Finware.ru Ltd. |
//|                                           http://www.finware.ru/ |
//+------------------------------------------------------------------+
//---- author of the indicator
#property copyright "Copyright 2002, Finware.ru Ltd."
//---- link to the website of the author
#property link      "http://www.finware.ru/"
//---- indicator version
#property version   "1.00"
//---- drawing the indicator in a separate window
#property indicator_separate_window
//----five buffers are used for calculation and drawing the indicator
#property indicator_buffers 6
//---- five graphical plots are used
#property indicator_plots   6
//+--------------------------------------------+
//|  RBCI indicator drawing parameters         |
//+--------------------------------------------+
//---- Drawing the RBCI indicator as a line
#property indicator_type1   DRAW_LINE
//---- lime color is used as the color of the indicator line
#property indicator_color1  Lime
//---- the indicator line is a continuous curve
#property indicator_style1  STYLE_SOLID
//---- indicator line width is equal to 2
#property indicator_width1  2
//---- displaying the indicator label
#property indicator_label1  "RBCI"
//+--------------------------------------------+
//|  BB levels indicator drawing parameters    |
//+--------------------------------------------+
//---- drawing Bollinger levels as lines
#property indicator_type2   DRAW_LINE
#property indicator_type3   DRAW_LINE
#property indicator_type4   DRAW_LINE
#property indicator_type5   DRAW_LINE
#property indicator_type6   DRAW_LINE
//---- orange color is used as the color of the Bollinger levels
#property indicator_color2  Orange
#property indicator_color3  Orange
#property indicator_color4  Gray
#property indicator_color5  Orange
#property indicator_color6  Orange
//---- Bollinger levels are dot-dash curves
#property indicator_style2 STYLE_DASHDOTDOT
#property indicator_style3 STYLE_DASHDOTDOT
#property indicator_style4 STYLE_DASHDOTDOT
#property indicator_style5 STYLE_DASHDOTDOT
#property indicator_style6 STYLE_DASHDOTDOT
//---- Bollinger levels width is equal to 1
#property indicator_width2  1
#property indicator_width3  1
#property indicator_width4  1
#property indicator_width5  1
#property indicator_width6  1
//---- display labels of Bollinger levels
#property indicator_label2  "+2Sigma"
#property indicator_label3  "+Sigma"
#property indicator_label4  "Middle"
#property indicator_label5  "-Sigma"
#property indicator_label6  "-2Sigma"

//---- indicator input parameters
input int BBPeriod=100;          //Period for Bollinger levels
input double BandsDeviation = 2; //Deviation

//---- declaration and initialization of a variable for storing the number of calculated bars
int RBCIPeriod=56;
int RBCIShift=0;      //Horizontal shift of RBCI in bars 

//---- declaration of a dynamic array that 
//---- will be used as an indicator buffer
double ExtLineBuffer0[];
//---- declaration of dynamic arrays that 
// will be used as indicator buffers
double ExtLineBuffer1[],ExtLineBuffer2[],ExtLineBuffer3[],ExtLineBuffer4[],ExtLineBuffer5[];
//+------------------------------------------------------------------+
// Description of the classes of averaging and indicators            |
//+------------------------------------------------------------------+
#include <SmoothAlgorithms.mqh>
#include <IndicatorsAlgorithms.mqh> 
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+  
void OnInit()
  {
//---- set ExtLineBuffer dynamic array as indicator buffer
   SetIndexBuffer(0,ExtLineBuffer0,INDICATOR_DATA);
//---- shifting the indicator horizontally by RBCIShift
   PlotIndexSetInteger(0,PLOT_SHIFT,RBCIShift);
//---- set the position, from which the indicator drawing starts
   PlotIndexSetInteger(0,PLOT_DRAW_BEGIN,RBCIPeriod-1);
//---- initializations of variable for indicator short name
   string shortname;
   StringConcatenate(shortname,"RBCI(",RBCIShift,")");
//---- create label to display in Data Window
   PlotIndexSetString(0,PLOT_LABEL,shortname);
//---- restriction to draw empty values for the indicator
   PlotIndexSetDouble(0,PLOT_EMPTY_VALUE,0);
//---- creating a name for displaying in a separate sub-window and in a tooltip
   IndicatorSetString(INDICATOR_SHORTNAME,shortname);
//---- determination of accuracy of displaying of the indicator values
   IndicatorSetInteger(INDICATOR_DIGITS,_Digits+1);

//---- Set dynamic arrays as indicator buffers
   SetIndexBuffer(1,ExtLineBuffer1,INDICATOR_DATA);
   SetIndexBuffer(2,ExtLineBuffer2,INDICATOR_DATA);
   SetIndexBuffer(3,ExtLineBuffer3,INDICATOR_DATA);
   SetIndexBuffer(4,ExtLineBuffer4,INDICATOR_DATA);
   SetIndexBuffer(5,ExtLineBuffer5,INDICATOR_DATA);
//---- set the position, from which Bollinger levels drawing starts
   PlotIndexSetInteger(1,PLOT_DRAW_BEGIN,RBCIPeriod+BBPeriod-1);
   PlotIndexSetInteger(2,PLOT_DRAW_BEGIN,RBCIPeriod+BBPeriod-1);
   PlotIndexSetInteger(3,PLOT_DRAW_BEGIN,RBCIPeriod+BBPeriod-1);
   PlotIndexSetInteger(4,PLOT_DRAW_BEGIN,RBCIPeriod+BBPeriod-1);
   PlotIndexSetInteger(5,PLOT_DRAW_BEGIN,RBCIPeriod+BBPeriod-1);
//---- create labels to display in Data Window
   PlotIndexSetString(1,PLOT_LABEL,"+2Sigma");
   PlotIndexSetString(2,PLOT_LABEL,"+Sigma");
   PlotIndexSetString(3,PLOT_LABEL,"Middle");
   PlotIndexSetString(4,PLOT_LABEL,"-Sigma");
   PlotIndexSetString(5,PLOT_LABEL,"-2Sigma");
//---- restriction to draw empty values for the indicator
   PlotIndexSetDouble(1,PLOT_EMPTY_VALUE,0);
   PlotIndexSetDouble(2,PLOT_EMPTY_VALUE,0);
   PlotIndexSetDouble(3,PLOT_EMPTY_VALUE,0);
   PlotIndexSetDouble(4,PLOT_EMPTY_VALUE,0);
   PlotIndexSetDouble(5,PLOT_EMPTY_VALUE,0);
//----
  }
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int OnCalculate(const int rates_total,     // number of bars in history at the current tick
                const int prev_calculated, // number of bars calculated at previous call
                const int begin,           // bars reliable counting beginning index
                const double &price[]      // price array for calculation of the indicator
                )
  {
//---- checking the number of bars to be enough for the calculation
   if(rates_total<RBCIPeriod-1+begin)
      return(0);

//---- declarations of local variables 
   int first,bar;
   double RBCI,BB;

//---- calculation of the 'first' starting number for the bars recalculation loop
   if(prev_calculated>rates_total || prev_calculated<=0) // checking for the first start of calculation of an indicator
     {
      first=RBCIPeriod-1+begin;  // starting index for calculation of all bars
      //---- increase the position of the beginning of data by 'begin' bars as a result of calculation using data of another indicator
      if(begin>0)
         PlotIndexSetInteger(0,PLOT_DRAW_BEGIN,begin+RBCIPeriod-1);
     }
   else first=prev_calculated-1; // starting index for calculation of new bars

//---- main indicator calculation loop
   for(bar=first; bar<rates_total; bar++)
     {
      //---- 
      RBCI=
           -35.524181940 * price[bar - 0]
           -29.333989650 * price[bar - 1]
           -18.427744960 * price[bar - 2]
           -5.3418475670 * price[bar - 3]
           +7.0231636950 * price[bar - 4]
           +16.176281560 * price[bar - 5]
           +20.656621040 * price[bar - 6]
           +20.326611580 * price[bar - 7]
           +16.270239060 * price[bar - 8]
           +10.352401270 * price[bar - 9]
           +4.5964239920 * price[bar - 10]
           +0.5817527531 * price[bar - 11]
           -0.9559211961 * price[bar - 12]
           -0.2191111431 * price[bar - 13]
           +1.8617342810 * price[bar - 14]
           +4.0433304300 * price[bar - 15]
           +5.2342243280 * price[bar - 16]
           +4.8510862920 * price[bar - 17]
           +2.9604408870 * price[bar - 18]
           +0.1815496232 * price[bar - 19]
           -2.5919387010 * price[bar - 20]
           -4.5358834460 * price[bar - 21]
           -5.1808556950 * price[bar - 22]
           -4.5422535300 * price[bar - 23]
           -3.0671459820 * price[bar - 24]
           -1.4310126580 * price[bar - 25]
           -0.2740437883 * price[bar - 26]
           +0.0260722294 * price[bar - 27]
           -0.5359717954 * price[bar - 28]
           -1.6274916400 * price[bar - 29]
           -2.7322958560 * price[bar - 30]
           -3.3589596820 * price[bar - 31]
           -3.2216514550 * price[bar - 32]
           -2.3326257940 * price[bar - 33]
           -0.9760510577 * price[bar - 34]
           +0.4132650195 * price[bar - 35]
           +1.4202166770 * price[bar - 36]
           +1.7969987350 * price[bar - 37]
           +1.5412722800 * price[bar - 38]
           +0.8771442423 * price[bar - 39]
           +0.1561848839 * price[bar - 40]
           -0.2797065802 * price[bar - 41]
           -0.2245901578 * price[bar - 42]
           +0.3278853523 * price[bar - 43]
           +1.1887841480 * price[bar - 44]
           +2.0577410750 * price[bar - 45]
           +2.6270409820 * price[bar - 46]
           +2.6973742340 * price[bar - 47]
           +2.2289941280 * price[bar - 48]
           +1.3536792430 * price[bar - 49]
           +0.3089253193 * price[bar - 50]
           -0.6386689841 * price[bar - 51]
           -1.2766707670 * price[bar - 52]
           -1.5136918450 * price[bar - 53]
           -1.3775160780 * price[bar - 54]
           -1.6156173970 * price[bar - 55];

      //---- Initialization of a cell of the indicator buffer with the received value of RBCI
      ExtLineBuffer0[bar]=-RBCI;
     }
//----
//---- checking the number of bars to be enough for the calculation
   if(rates_total<RBCIPeriod+BBPeriod-1+begin)
      return(rates_total);
//---- calculation of the 'first' starting number for the bars recalculation loop
   if(prev_calculated==0)        // checking for the first start of the indicator calculation
     {
      first=RBCIPeriod-1+begin;  // starting index for calculation of all bars
      //---- increase the position of the beginning of data by 'begin' bars as a result of calculation using data of another indicator
      if(begin>0)
        {
         PlotIndexSetInteger(1,PLOT_DRAW_BEGIN,begin+RBCIPeriod+BBPeriod-1);
         PlotIndexSetInteger(2,PLOT_DRAW_BEGIN,begin+RBCIPeriod+BBPeriod-1);
         PlotIndexSetInteger(3,PLOT_DRAW_BEGIN,begin+RBCIPeriod+BBPeriod-1);
         PlotIndexSetInteger(4,PLOT_DRAW_BEGIN,begin+RBCIPeriod+BBPeriod-1);
         PlotIndexSetInteger(5,PLOT_DRAW_BEGIN,begin+RBCIPeriod+BBPeriod-1);
        }
     }
   else first=prev_calculated-1; // starting index for calculation of new bars

//---- declaration of the Moving_Average and StdDeviation classes variables 
   static CBBands BBands;

//---- main indicator calculation loop
   for(bar=first; bar<rates_total; bar++)
     {
      BBands.BBandsSeries(RBCIPeriod+begin,prev_calculated,rates_total,
                          BBPeriod,BandsDeviation,MODE_SMA,ExtLineBuffer0[bar],bar,false,
                          ExtLineBuffer5[bar],ExtLineBuffer3[bar],ExtLineBuffer1[bar]);

      //---- 
      BB=(ExtLineBuffer1[bar]-ExtLineBuffer3[bar])/2;
      ExtLineBuffer2[bar]=ExtLineBuffer3[bar]+BB;
      ExtLineBuffer4[bar]=ExtLineBuffer3[bar]-BB;
     }
//----    
   return(rates_total);
  }
//+------------------------------------------------------------------+
