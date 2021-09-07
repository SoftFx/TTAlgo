//+------------------------------------------------------------------+
//|                                                    WriteBars.mq5 |
//|                        Copyright 2019, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2019, MetaQuotes Software Corp."
#property link      "https://www.mql5.com"
#property version   "1.00"
#property indicator_chart_window
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+

input int BBPeriod=100;          //Period for Bollinger levels
input double BandsDeviation = 2; //Deviation
input int RBCIShift=0;      //Horizontal shift of RBCI in bars 



string tf()
{
   switch(Period())
   {
      case PERIOD_M1: return("M1");
      
      case PERIOD_M5: return("M5");
      
      case PERIOD_M15: return("M15");
      
      case PERIOD_M30: return("M30");
      
      case PERIOD_H1: return("H1");
      
      case PERIOD_H4: return("H4");
      
      case PERIOD_D1: return("D1");
      
      case PERIOD_W1: return("W1");
      
      case PERIOD_MN1: return("MN1");
      
      default:return("Unknown timeframe");
   
   }
}

int RBCI;
double _buffer[], _2p_sigma[], _p_sigma[], _middle[], _m_sigma[], _2m_sigma[];
int file_handle, file_bids;

int OnInit()
  {
      MqlParam params[]; 
      ArrayResize(params,4);
      params[0].type = TYPE_STRING;
      params[0].string_value = "rbci";
      params[1].type = TYPE_INT;
      params[1].integer_value = BBPeriod;
      params[2].type = TYPE_DOUBLE;
      params[2].double_value = BandsDeviation;
      params[3].type = TYPE_INT;
      params[3].integer_value = RBCIShift;

      RBCI = IndicatorCreate(Symbol(), Period(), IND_CUSTOM, 4 , params);
      
      if (RBCI < 0)
      {
         Print(GetLastError());
         return(INIT_FAILED);
      }
      
      string bids_path = StringFormat("bids_%s_%s_%d_%.2f.bin", Symbol(), tf(), BBPeriod, BandsDeviation);
      string file_path = StringFormat("RBCI_%s_%s_%d_%.2f.bin", Symbol(), tf(), BBPeriod, BandsDeviation);
  
      file_handle=FileOpen(file_path, FILE_READ|FILE_WRITE|FILE_BIN);
      file_bids=FileOpen(bids_path, FILE_READ|FILE_WRITE|FILE_BIN);
      
      return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+

string GetPrice(double price)
{
   if (MathAbs(price) < 1e12)
      return DoubleToString(price, Digits());
   else
   if (price < 0)
      return "-inf";
   else
      return "inf";
   
}

int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime &time[],
                const double &open[],
                const double &high[],
                const double &low[],
                const double &close[],
                const long &tick_volume[],
                const long &volume[],
                const int &spread[])
  {
  
   CopyBuffer(RBCI,0,0, rates_total, _buffer);
   CopyBuffer(RBCI,1,0, rates_total, _2p_sigma);
   CopyBuffer(RBCI,2,0, rates_total, _p_sigma);
   CopyBuffer(RBCI,3,0, rates_total, _middle);
   CopyBuffer(RBCI,4,0, rates_total, _m_sigma);
   CopyBuffer(RBCI,5,0, rates_total, _2m_sigma);
   
   int as = ArraySize(_buffer);
   
   for (int i = prev_calculated; i < rates_total; i++)
   {
      
      FileWriteLong(file_bids, time[i]);
      FileWriteDouble(file_bids, open[i]);
      FileWriteDouble(file_bids, high[i]);
      FileWriteDouble(file_bids, low[i]);
      FileWriteDouble(file_bids, close[i]);
      FileWriteLong(file_bids, tick_volume[i]);
      
      FileWriteDouble(file_handle, _buffer[i]);
      FileWriteDouble(file_handle, _2p_sigma[i]);
      FileWriteDouble(file_handle, _p_sigma[i]);
      FileWriteDouble(file_handle, _middle[i]);
      FileWriteDouble(file_handle, _m_sigma[i]);
      FileWriteDouble(file_handle, _2m_sigma[i]);
   }   
  
   return(rates_total);
  }
  
  void OnDeinit(const int reason)
  {
      FileClose(file_handle);
      FileClose(file_bids);
  }
  
  void  OnTesterDeinit(void)
  {
      FileClose(file_handle);
      FileClose(file_bids);
  }
//+------------------------------------------------------------------+
