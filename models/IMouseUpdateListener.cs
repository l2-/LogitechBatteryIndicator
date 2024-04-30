using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogitechBatteryIndicator.models
{
    public interface IMouseUpdateListener
    {
        void OnMouseUpdate(object? sender, MouseUpdateEvent e);
    }
}
