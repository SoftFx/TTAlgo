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
int indicator_params[111][5];
input int Kperiod = 5;
input int slowing = 3;
input int Dperiod = 3;
input string symbol = "AUDJPY";
input string timeframe = "M30";
input string prefix = "Stochastic";

int open_csv_file(string filename)
{
   return FileOpen(filename, FILE_WRITE | FILE_BIN);
}

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
  {
//--- indicator buffers mapping
   SetIndexBuffer(0,StateBuffer);
   //return(INIT_FAILED);
   string indicator_base = prefix + "_" + symbol + "_" + timeframe;
   bids_file = open_csv_file(StringFormat("bids_%s_%s_%d_%d_%d.bin", symbol, timeframe, Kperiod, slowing, Dperiod));
   indicator_files_total = 4*2;
   for (int i = 0; i < 4; i++)
      for (int j = 0; j < 2; j++)
      {
         int ind = 2*i+j;
         indicator_params[ind][0] = Kperiod;
         indicator_params[ind][1] = Dperiod;
         indicator_params[ind][2] = slowing;
         indicator_params[ind][3] = i;
         indicator_params[ind][4] = j;
         indicator_files[ind] = open_csv_file(StringFormat("%s_%d_%d_%d_%d_%d.bin", indicator_base, Kperiod, slowing, Dperiod, i, j));
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
         for (int k = 0; k < 2; k++)
         {
            double val = iStochastic(NULL, 0, indicator_params[j][0], indicator_params[j][1], indicator_params[j][2], 
                  indicator_params[j][3], indicator_params[j][4], k, i);
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
