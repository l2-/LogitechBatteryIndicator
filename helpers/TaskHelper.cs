using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LogitechBatteryIndicator.helpers
{
    public static class TaskHelper
    {
        public static void Do(this Task t)
        {
            Task.Factory.StartNew(async () => { try { await t; } catch (Exception e) { Console.WriteLine(e); } });
        }
    }
}
