using logi.hidppio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogitechBatteryIndicator.models
{
    internal class MouseProductId
    {
        public static string GetDeviceNameOverride(long ProductId)
        {
            return (ulong)ProductId switch
            {
                4642302164515946496 => "G603",
                4644337115725824000 => "G305",
                4645956180957462528 => "Pro Wireless",
                4647644983573086208 or 4647645052292562944 => "G502",
                4649615390014439424 => "G703",
                4649896869286117376 => "G903",
                4656370819520266240 => "G303 SHROUD",
                4659467061443952640 => "PRO X SUPERLIGHT 2",
                12692340589811728384 => "G604",
                12695155442658099350 => "G705",
                12699096178231345152 => "G309 LIGHTSPEED",
                13870523902347706368 => "G402",
                13870805377324417024 => "G302",
                13871086852301127680 => "G303",
                13871368327277838336 => "G900",
                13871649802254548992 => "G403 Wireless",
                13871931277231259648 => "G403",
                13872212752207970304 or 13876153401881919488 => "G102 / G203",
                13872494227184680960 => "Pro Mouse",
                13872775702161391616 => "G903 SE",
                13873057177138102272 => "G703",
                13874183077044944896 => "G502 Hero",
                13874746026998366208 => "G502 Wireless",
                13875027501975076864 => "MX518",
                13875308976951787520 => "G403 Wireless",
                13875871926905208832 => "G900",
                14065304586231480320 => "G502 Spectrum",
                _ => "",
            };
        }
    }
}
