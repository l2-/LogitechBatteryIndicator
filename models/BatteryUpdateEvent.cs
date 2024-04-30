using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogitechBatteryIndicator.models
{
    public sealed record BatteryUpdateEvent(int Percentage, BatteryMode State);
}
