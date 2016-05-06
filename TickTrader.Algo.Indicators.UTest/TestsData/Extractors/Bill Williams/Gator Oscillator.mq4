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
int indicator_params[111][8];
input int jaws_period = 13;
input int jaws_shift = 8;
input int teeth_period = 8;
input int teeth_shift = 5;
input int lips_period = 5;
input int lips_shift = 3;
input string prefix = "Gator";

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
   bids_file = open_csv_file(StringFormat("bids_%s_%s_%d_%d_%d_%d_%d_%d.bin", symbol, timeframe,
             jaws_period, jaws_shift, teeth_period, teeth_shift, lips_period, lips_shift));
   indicator_files_total = 4*7;
   for (int i = 0; i < 4; i++)
      for (int j = 0; j < 7; j++)
      {
         int ind = 7*i+j;
         indicator_params[ind][0] = jaws_period;
         indicator_params[ind][1] = jaws_shift;
         indicator_params[ind][2] = teeth_period;
         indicator_params[ind][3] = teeth_shift;
         indicator_params[ind][4] = lips_period;
         indicator_params[ind][5] = lips_shift;
         indicator_params[ind][6] = i;
         indicator_params[ind][7] = j;
         indicator_files[ind] = open_csv_file(StringFormat("%s_%d_%d_%d_%d_%d_%d_%d_%d.bin", indicator_base, 
              jaws_period, jaws_shift, teeth_period, teeth_shift, lips_period, lips_shift, i, j));
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
   bool flags[2];
   flags[0] = true;
   flags[1] = true;
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
         for (int k = 1; k < 3; k++)
         {
            double val = iGator(NULL, 0, indicator_params[j][0], indicator_params[j][1],
               indicator_params[j][2], indicator_params[j][3], indicator_params[j][4], indicator_params[j][5],
               indicator_params[j][6], indicator_params[j][7], k, i);
            if(i<rates_total-1)
            {
               double prev_val = iGator(NULL, 0, indicator_params[j][0], indicator_params[j][1],
               indicator_params[j][2], indicator_params[j][3], indicator_params[j][4], indicator_params[j][5],
               indicator_params[j][6], indicator_params[j][7], k, i+1);
               if (k == 1)
               {
                  if (val > prev_val) flags[k-1] = true;
                  if (val < prev_val) flags[k-1] = false;
               }
               if (k == 2)
               {
                  if (val < prev_val) flags[k-1] = true;
                  if (val > prev_val) flags[k-1] = false;
               }
            }
            if (flags[k-1])
            {
               FileWriteDouble(indicator_files[j], val);
               FileWriteDouble(indicator_files[j], 0.0);
            }
            if (!flags[k-1])
            {
               FileWriteDouble(indicator_files[j], 0.0);
               FileWriteDouble(indicator_files[j], val);
            }
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
