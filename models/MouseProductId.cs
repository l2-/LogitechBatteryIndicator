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
            switch ((ulong)ProductId)
            {
                case 4642302164515946496:
                    return "G603";
                case 4644337115725824000:
                    return "G305";
                case 4645956180957462528:
                    return "Pro Wireless";
                case 4647644983573086208:
                case 4647645052292562944:
                    return "G502";
                case 4649615390014439424:
                    return "G703";
                case 4649896869286117376:
                    return "G903";
                case 4656370819520266240:
                    return "G303 SHROUD";
                case 4653274581891547136:
                    return "PRO X SUPERLIGHT";
                case 4659467061443952640:
                    return "PRO X SUPERLIGHT 2";
                case 12692340589811728384:
                    return "G604";
                case 12695155442658099350:
                    return "G705";
                case 12699096178231345152:
                    return "G309 LIGHTSPEED";
                case 13870523902347706368:
                    return "G402";
                case 13870805377324417024:
                    return "G302";
                case 13871086852301127680:
                    return "G303";
                case 13871368327277838336:
                    return "G900";
                case 13871649802254548992:
                    return "G403 Wireless";
                case 13871931277231259648:
                    return "G403";
                case 13872212752207970304:
                case 13876153401881919488:
                    return "G102 / G203";
                case 13872494227184680960:
                    return "Pro Mouse";
                case 13872775702161391616:
                    return "G903 SE";
                case 13873057177138102272:
                    return "G703";
                case 13874183077044944896:
                    return "G502 Hero";
                case 13874746026998366208:
                    return "G502 Wireless";
                case 13875027501975076864:
                    return "MX518";
                case 13875308976951787520:
                    return "G403 Wireless";
                case 13875871926905208832:
                    return "G900";
                case 14065304586231480320:
                    return "G502 Spectrum";
                default:
                    return "";
            }
        }
    }
}
