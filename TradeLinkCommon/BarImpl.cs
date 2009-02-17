using System;
using System.Collections.Generic;
using TradeLink.API;

namespace TradeLink.Common
{


    /// <summary>
    /// A single bar of price data, which represents OHLC and volume for an interval of time.
    /// </summary>
    public class BarImpl : TickIndicator, TradeLink.API.Bar
    {
        string _sym = "";
        public string Symbol { get { return _sym; } set { _sym = value; } }
        private decimal h = decimal.MinValue;
        private decimal l = decimal.MaxValue;
        private decimal o = 0;
        private decimal c = 0;
        private int v = 0;
        private int tradesinbar = 0;
        private BarInterval tunits = BarInterval.FiveMin; //5min bar default
        private int bartime = 0;
        private int bardate = 0;
        private bool DAYEND = false;
        public bool DayEnd { get { return DAYEND; } }
        public decimal High { get { return h; } }
        public decimal Low { get { return l; } }
        public decimal Open { get { return o; } }
        public decimal Close { get { return c; } }
        public int Volume { get { return v; } }
        public bool isNew { get { return tradesinbar==1; } }
        public bool isValid { get { return (h >= l) && (o != 0) && (c != 0); } }
        public int TradeCount { get { return tradesinbar; } }

        public BarImpl() : this(BarInterval.FiveMin) { }
        public BarImpl(decimal open, decimal high, decimal low, decimal close, int vol, int date, int time)
        {
            h = high;
            o = open;
            l = low;
            c = close;
            v = vol;
            bardate = date;
            bartime = time;
        }
        public BarImpl(decimal open, decimal high, decimal low, decimal close, int vol, int date)
        {
            h = high;
            o = open;
            l = low;
            c = close;
            v = vol;
            bardate = date;
        }
        public BarImpl(BarImpl b)
        {
            h = b.High;
            l = b.Low;
            o = b.Open;
            c = b.Close;
            DAYEND = b.DAYEND;
            bartime = b.bartime;
            bardate = b.bardate;
        }
        
        
        public BarImpl(BarInterval tu) 
        {
            tunits = tu;
        }
        public int Bartime { get { return bartime; } }
        public int Bardate { get { return bardate; } }
        private int BarTime(int time) 
        {
            DateTime dt = Util.ToDateTime(time, 0);
            int bcount = (int)dt.TimeOfDay.TotalMinutes / (int)tunits;
            int h = (bcount * (int)tunits )/ 60;
            int m = (dt.Minute / (int)tunits)*(int)tunits;
            return (h * 100) + m;
        }

        /// <summary>
        /// Accepts the specified tick.
        /// </summary>
        /// <param name="t">The tick you want to add to the bar.</param>
        /// <returns>true if the tick is accepted, false if it belongs to another bar.</returns>
        public bool newTick(Tick t)
        {
            if (_sym == "") _sym = t.symbol;
            if (_sym != t.symbol) throw new InvalidTick();
            if (bartime == 0) { bartime = BarTime(t.time); bardate = t.date;}
            if (bardate != t.date) DAYEND = true;
            else DAYEND = false;
            if ((BarTime(t.time) != bartime) || (bardate!=t.date)) return false; // not our tick
            if (!t.isTrade) return true; // if it's not a trade, accept tick but don't update anything
            tradesinbar++; // count it
            v += t.size; // add trade size to bar volume
            if (o == 0) o = t.trade;
            if (t.trade > h) h = t.trade;
            if (t.trade < l) l = t.trade;
            c = t.trade;
            return true;
        }
        public override string ToString() { return "OHLC ("+bartime+") " + o + "," + h + "," + l + "," + c; }
        /// <summary>
        /// Create bar object from a CSV file providing OHLC+Volume data.
        /// </summary>
        /// <param name="record">The record in comma-delimited format.</param>
        /// <returns>The equivalent Bar</returns>
        public static BarImpl FromCSV(string record)
        {
            // google used as example
            string[] r = record.Split(',');
            if (r.Length < 6) return null;
            DateTime d = new DateTime();
            try
            {
                d = DateTime.Parse(r[0]);
            }
            catch (System.FormatException) { return null; }
            int date = (d.Year*10000)+(d.Month*100)+d.Day;
            decimal open = Convert.ToDecimal(r[1]);
            decimal high = Convert.ToDecimal(r[2]);
            decimal low = Convert.ToDecimal(r[3]);
            decimal close = Convert.ToDecimal(r[4]);
            int vol = Convert.ToInt32(r[5]);
            return new BarImpl(open,high,low,close,vol,date);
        }

    }


}