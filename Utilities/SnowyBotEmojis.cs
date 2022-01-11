using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot
{
  public static partial class SnowyBotUtils
  {
    public static readonly string SnowyButton = "<:SnowyButton:929698740757995540>";
    public static readonly string SnowyPlay = "<:SnowyPlay:929698740669935636>";
    public static readonly string SnowyPause = "<:SnowyPause:929698740443435038>";
    public static readonly string SnowyStop = "<:SnowyStop:929698740602810418>";
    public static readonly string SnowyRewind = "<:SnowyRewind:929698740409872396>";
    public static readonly string SnowyFastForward = "<:SnowyFastForward:929698740632170506>";
    public static readonly string SnowySkipBackward = "<:SnowySkipBackward:929698740363722784>";
    public static readonly string SnowySkipForward = "<:SnowySkipForward:929698740724441128>";
    public static readonly string SnowyLineLeftEnd = "<:SnowyLineLeftEnd:929698740363722783>";
    public static readonly string SnowyLineRightEnd = "<:SnowyLineRightEnd:929698740388921404>";
    public static readonly string SnowySmallButton = "<:SnowySmallButton:929698740665741333>";
    public static readonly string SnowyDash = "<:SnowyDash:929698740636364890>";
    public static readonly string SnowyLeftLine = "<:SnowyLeftLine:929698740267286558>";
    public static readonly string SnowyLine = "<:SnowyLine:929698740292419584>";
    public static readonly string SnowyRightLine = "<:SnowyRightLine:929698740409872395>";
    public static readonly string SnowyButtonConnected = "<:SnowyButtonConnected:929698740867051530>";
    public static readonly string SnowyLoopEnabled = "<:SnowyLoopEnabled:929712034126376990>";
    public static readonly string SnowyLoopLimited = "<:SnowyLoopLimited:929712034315120690>";
    public static readonly string SnowyLoopDisabled = "<:SnowyLoopDisabled:929722025092718602>";
    public static readonly string SnowyShuffle = "<:SnowyShuffle:929698740309213235>";
    public static readonly string SnowyUniversalThin = "<:SnowyUniversalThin:929712034289975326>";
    public static readonly string SnowyUniversalStrong = "<:SnowyUniversalStrong:929712034310914129>";
    public static readonly string SnowyError = "<:SnowyError:929714749896282122>";
    public static readonly string SnowySuccess = "<:SnowySuccess:929714750030508082>";
    public static readonly string SnowyBlank = "<:SnowyBlank:929727760891527239>";
    public static readonly string SnowyOneLight = "<:SnowyOneLight:929833994768482344>";
    public static readonly string SnowyTwoLight = "<:SnowyTwoLight:929833994655252490>";
    public static readonly string SnowyThreeLight = "<:SnowyThreeLight:929833994751733760>";
    public static readonly string SnowyFourLight = "<:SnowyFourLight:929833994697195591>";
    public static readonly string SnowyFiveLight = "<:SnowyFiveLight:929833994676236348>";
    public static readonly string SnowySixLight = "<:SnowySixLight:929833994437144607>";
    public static readonly string SnowySevenLight = "<:SnowySevenLight:929833994768515104>";
    public static readonly string SnowyEightLight = "<:SnowyEightLight:929833994474881075>";
    public static readonly string SnowyNineLight = "<:SnowyNineLight:929833994776891402>";
    public static readonly string SnowyTenLight = "<:SnowyTenLight:929833994764288100>";
    public static readonly string SnowyOneDark = "<:SnowyOneDark:929841142445518861>";
    public static readonly string SnowyTwoDark = "<:SnowyTwoDark:929841142680404058>";
    public static readonly string SnowyThreeDark = "<:SnowyThreeDark:929841142613282846>";
    public static readonly string SnowyFourDark = "<:SnowyFourDark:929841142638444594>";
    public static readonly string SnowyFiveDark = "<:SnowyFiveDark:929841142621687839>";
    public static readonly string SnowySixDark = "<:SnowySixDark:929841142638444554>";
    public static readonly string SnowySevenDark = "<:SnowySevenDark:929841142479089695>";
    public static readonly string SnowyEightDark = "<:SnowyEightDark:929841142600716418>";
    public static readonly string SnowyNineDark = "<:SnowyNineDark:929841142596534302>";
    public static readonly string SnowyTenDark = "<:SnowyTenDark:929841142734917693>";

    public static string NumToLightEmoji(int num)
    {
      return num switch
      {
        1 => SnowyOneLight,
        2 => SnowyTwoLight,
        3 => SnowyThreeLight,
        4 => SnowyFourLight,
        5 => SnowyFiveLight,
        6 => SnowySixLight,
        7 => SnowySevenLight,
        8 => SnowyEightLight,
        9 => SnowyNineLight,
        10 => SnowyTenLight,
        _ => null,
      };
    }
    public static string NumToDarkEmoji(int num)
    {
      return num switch
      {
        1 => SnowyOneDark,
        2 => SnowyTwoDark,
        3 => SnowyThreeDark,
        4 => SnowyFourDark,
        5 => SnowyFiveDark,
        6 => SnowySixDark,
        7 => SnowySevenDark,
        8 => SnowyEightDark,
        9 => SnowyNineDark,
        10 => SnowyTenDark,
        _ => null,
      };
    }
  }
}
