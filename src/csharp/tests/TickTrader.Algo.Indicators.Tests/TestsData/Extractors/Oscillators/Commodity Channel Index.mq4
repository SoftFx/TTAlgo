#property copyright ""
#property link      ""
#property version   "1.00"
#property strict
#property indicator_separate_window
#property indicator_minimum 0
#property indicator_maximum 2
#property indicator_buffers 1
#property indicator_plots   1
//--- plot State
#property indicator_label1  "State"
#property indicator_type1   DRAW_LINE
#property indicator_color1  clrRed
#property indicator_style1  STYLE_SOLID
#property indicator_width1  1
//--- indicator buffers
double         StateBuffer[];


int bids_file;
int indicator_files_total;
int indicator_files[111];
int indicator_params[111][2];
input int period = 14;
input string prefix = "CCI";

int open_csv_file(string filename)
{
   return FileOpen(filename, FILE_WRITE | FILE_BIN);
}

string get_timeframe_name()
{
   int p = Period();
   if (p < 60)
   {
      return "M"+IntegerToString(p);
   }
   if (p%60 == 0 && p/60 < 24)
   {
      return "H" + IntegerToString(p/60);
   }
   if (p%(60*24) == 0 && p/(60*24) < 7)
   {
      return "D" + IntegerToString(p/(60*24));
   }
   if (p%(60*24*7) == 0 && p/(60*24*7) < 4)
   {
      return "W" + IntegerToString(p/(60*24*7));
   }
   if (p%(60*24*30) == 0 && p/(60*24*30) < 12)
   {
      return "MN" + IntegerToString(p/(60*24*30));
   }
   return "Unknown";
}

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
  {
   string symbol = Symbol();
   string timeframe = get_timeframe_name();
//--- indicator buffers mapping
   SetIndexBuffer(0,StateBuffer);
   //return(INIT_FAILED);
   string indicator_base = prefix + "_" + symbol + "_" + timeframe;
   bids_file = open_csv_file(StringFormat("bids_%s_%s_%d.bin", symbol, timeframe, period));
   indicator_files_total = 1*7;
   for (int i = 0; i < 1; i++)
      for (int j = 0; j < 7; j++)
      {
         int ind = 7*i+j;
         indicator_params[ind][0] = period;
         indicator_params[ind][1] = j;
         indicator_files[ind] = open_csv_file(StringFormat("%s_%d_%d.bin", indicator_base, period, j));
         if (indicator_files[ind] == INVALID_HANDLE)
         {
            PrintFormat("cannot open file #%d, error = %d", ind, GetLastError());
            return(INIT_FAILED);
         }
      }
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
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
   PrintFormat("starting prev = %d, total = %d", prev_calculated, rates_total);
   for (int i = rates_total - 1; i >= prev_calculated; i--)
   {
      FileWriteLong(bids_file, time[i]);
      FileWriteDouble(bids_file, open[i]);
      FileWriteDouble(bids_file, high[i]);
      FileWriteDouble(bids_file, low[i]);
      FileWriteDouble(bids_file, close[i]);
      FileWriteLong(bids_file, tick_volume[i]);
      for (int j = 0; j < indicator_files_total; j++)
      {
         double val = iCCI(NULL, 0, indicator_params[j][0], indicator_params[j][1], i);
         FileWriteDouble(indicator_files[j], val);
      }
      //PrintFormat("proceeded %d", i);
      StateBuffer[i] = 1;
   }
   FileFlush(bids_file);
   for (int j = 0; j < indicator_files_total; j++)
   {
      FileFlush(indicator_files[j]);
   }
   PrintFormat("Done prev = %d, total = %d", prev_calculated, rates_total);
   return(rates_total);
  }
