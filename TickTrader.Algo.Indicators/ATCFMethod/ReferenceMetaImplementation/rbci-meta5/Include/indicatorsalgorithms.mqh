//MQL5 Version  June 21, 2010 Final
//+------------------------------------------------------------------+
//|                                         IndicatorsAlgorithms.mqh |
//|                               Copyright © 2010, Nikolay Kositsin |
//|                              Khabarovsk,   farria@mail.redcom.ru | 
//+------------------------------------------------------------------+
#property copyright "2010,   Nikolay Kositsin"
#property link      "farria@mail.redcom.ru"
#property version   "1.00"

//+------------------------------------------------------------------+
// Declaration of smoothing classes                                  |
//+------------------------------------------------------------------+
#include <SmoothAlgorithms.mqh> 
//+------------------------------------------------------------------+
//|  Linear regression smoothing of price series                     |
//+------------------------------------------------------------------+
class CLRMA
  {
public:
   double LRMASeries(uint begin,           // Bars reliable counting beginning index
                     uint prev_calculated, // Number of bars calculated at previous call
                     uint rates_total,     // Number of bars in history at the current tick
                     int Length,           // Smoothing period
                     double series,        // Value of the price series calculated for the new bar having the 'bar' index
                     uint bar,             // Bar index
                     bool set              // Direction of the arrays indexing.
                     )
     {
      //---- Declaration of local variables
      double sma,lwma,lrma;

      //---- declaration of variables of the Moving_Average class from the SmoothAlgorithms file
      // CMoving_Average SMA, LWMA;

      //---- Getting values of moving averages  
      sma=m_SMA.SMASeries(begin,prev_calculated,rates_total,Length,series,bar,set);
      lwma=m_LWMA.LWMASeries(begin,prev_calculated,rates_total,Length,series,bar,set);

      //---- Calculation of LRMA
      lrma=3.0*lwma-2.0*sma;
      //----+
      return(lrma);
     };

protected:
   //---- declaration of variables of the ÑMoving_Average class
   CMoving_Average   m_SMA,m_LWMA;
  };
//+-----------------------------------------------------------------------+
//|  The algorithm of getting the Bollinger channel calculated from VIDYA |
//+-----------------------------------------------------------------------+
class CVidyaBands
  {
public:
   double VidyaBandsSeries(uint begin,           // Bars reliable counting beginning index
                           uint prev_calculated, // Number of bars calculated at previous call
                           uint rates_total,     // Number of bars in history at the current tick
                           int CMO_period,       // Period of CMO oscillator smoothing
                           double EMA_period,    // EMA smoothing period
                           int BBLength,         // Bollinger channel smoothing period
                           double deviation,     // Deviation
                           double series,        // Value of the price series calculated for the new bar having the 'bar' index
                           uint bar,             // Bar index
                           bool set,             // Direction of the arrays indexing
                           double& DnMovSeries,  // Value of the lower border of the channel for the current bar 
                           double& MovSeries,    // Value of the middle line of the channel for the current bar 
                           double &UpMovSeries   // Value of the upper border of the channel for the current bar 
                           )
     {
      //---- Calculation of the middle line    
      MovSeries=m_VIDYA.VIDYASeries(begin,prev_calculated,rates_total,CMO_period,EMA_period,series,bar,set);

      //---- Calculation of the Bollinger channel
      double StdDev=m_STD.StdDevSeries(begin+CMO_period+1,prev_calculated,rates_total,BBLength,deviation,series,MovSeries,bar,set);
      DnMovSeries = MovSeries - StdDev;
      UpMovSeries = MovSeries + StdDev;
      //----
      return(StdDev);
     }

protected:
   //---- declaration of variables of the CCMO and CStdDeviation classes
   CCMO              m_VIDYA;
   CStdDeviation     m_STD;
  };
//+------------------------------------------------------------------+
//|  Algorithm of getting the Bollinger channel                      |
//+------------------------------------------------------------------+
class CBBands
  {
public:
   double            BBandsSeries(uint begin,               // Bars reliable counting beginning index
                                  uint prev_calculated,     // Number of bars calculated at previous call
                                  uint rates_total,         // Number of bars in history at the current tick
                                  int Length,               // Smoothing period
                                  double deviation,         // Deviation
                                  ENUM_MA_METHOD MA_Method, // Smoothing method
                                  double series,            // Value of the price series calculated for the new bar having the 'bar' index
                                  uint bar,                 // Bar index
                                  bool set,                 // Direction of the arrays indexing
                                  double& DnMovSeries,      // Value of the lower border of the channel for the current bar 
                                  double& MovSeries,        // Value of the middle line of the channel for the current bar 
                                  double &UpMovSeries       // Value of the upper border of the channel for the current bar 
                                  );

   double            BBandsSeries_(uint begin,              // Bars reliable counting beginning index
                                   uint prev_calculated,    // Number of bars calculated at previous call
                                   uint rates_total,        // Number of bars in history at the current tick
                                   int MALength,            // Moving average smoothing period
                                   ENUM_MA_METHOD MA_Method,// Smoothing method
                                   int BBLength,            // Bollinger channel smoothing period
                                   double deviation,        // Deviation
                                   double series,           // Value of the price series calculated for the new bar having the 'bar' index
                                   uint bar,                // Bar index
                                   bool set,                // Direction of the arrays indexing
                                   double& DnMovSeries,     // Value of the lower border of the channel for the current bar 
                                   double& MovSeries,       // Value of the middle line of the channel for the current bar 
                                   double &UpMovSeries      // Value of the upper border of the channel for the current bar 
                                   );
protected:
   //---- declaration of variables of the ÑMoving_Average and CStdDeviation classes
   CStdDeviation     m_STD;
   CMoving_Average   m_MA;
  };
//+------------------------------------------------------------------+
//|  Bollinger channel calculation                                   |
//+------------------------------------------------------------------+
double CBBands::BBandsSeries(uint begin,              // Bars reliable counting beginning index
                             uint prev_calculated,    // Number of bars calculated at previous call
                             uint rates_total,        // Number of bars in history at the current tick
                             int Length,              // Smoothing period
                             double deviation,        // Deviation
                             ENUM_MA_METHOD MA_Method,// Smoothing method
                             double series,           // Value of the price series calculated for the new bar having the 'bar' index
                             uint bar,                // Bar index
                             bool set,                // Direction of the arrays indexing
                             double &DnMovSeries,     // Value of the lower border of the channel for the current bar 
                             double &MovSeries,       // Value of the middle line of the channel for the current bar
                             double &UpMovSeries      // Value of the upper border of the channel for the current bar
                             )
// BBandsMASeries(begin, prev_calculated, rates_total, period, deviation,
// MA_Method, Series, bar, set, DnMovSeries, MovSeries, UpMovSeries) 
//+------------------------------------------------------------------+
  {
//---- Calculation of the middle line
   MovSeries=m_MA.MASeries(begin,prev_calculated,rates_total,Length,MA_Method,series,bar,set);

//---- Calculation of the Bollinger channel
   double StdDev=m_STD.StdDevSeries(begin,prev_calculated,rates_total,Length,deviation,series,MovSeries,bar,set);
   DnMovSeries = MovSeries - StdDev;
   UpMovSeries = MovSeries + StdDev;
//----+
   return(StdDev);
  }
//+------------------------------------------------------------------+
//|  Bollinger channel calculation                                   |
//+------------------------------------------------------------------+
double CBBands::BBandsSeries_(uint begin,               // Bars reliable counting beginning index
                              uint prev_calculated,     // Number of bars calculated at previous call
                              uint rates_total,         // Number of bars in history at the current tick
                              int MALength,             // Moving average smoothing period
                              ENUM_MA_METHOD MA_Method, // Smoothing method
                              int BBLength,             // Bollinger channel smoothing period
                              double deviation,         // Deviation
                              double series,            // Value of the price series calculated for the new bar having the 'bar' index
                              uint bar,                 // Bar index
                              bool set,                 // Direction of the arrays indexing
                              double &DnMovSeries,      // Value of the lower border of the channel for the current bar 
                              double &MovSeries,        // Value of the middle line of the channel for the current bar
                              double &UpMovSeries       // Value of the upper border of the channel for the current bar
                              )
// BBandsMASeries_(begin, prev_calculated, rates_total, MALength, MA_Method,
// deviation, BBLength, Series, bar, set, DnMovSeries, MovSeries, UpMovSeries) 
//+------------------------------------------------------------------+
  {
//---- Calculation of the middle line
   MovSeries=m_MA.MASeries(begin,prev_calculated,rates_total,MALength,MA_Method,series,bar,set);

//---- Calculation of the Bollinger channel
   double StdDev=m_STD.StdDevSeries(begin+MALength+1,prev_calculated,rates_total,BBLength,deviation,series,MovSeries,bar,set);
   DnMovSeries = MovSeries - StdDev;
   UpMovSeries = MovSeries + StdDev;
//----
   return(StdDev);
  }
//+------------------------------------------------------------------+
