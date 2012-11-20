using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace STimWPF
{
	public class KeyConverter
	{

		public static String KeyToString(Key selectedKey, bool spaceAsUnderscore)
		{
			switch (selectedKey)
			{
				case Key.Space:
					if (spaceAsUnderscore == true)
						return "_";
					return " ";
				case Key.OemComma:
					return ",";
				case Key.OemPeriod:
					return ".";
				case Key.Separator:
					return "_";
				case Key.OemMinus:
					return "-";
				case Key.OemQuestion:
					return "?";
				case Key.OemPlus:
					return "+";
				case Key.D0:
					return "0";
				case Key.D1:
					return "1";
				case Key.D2:
					return "2";
				case Key.D3:
					return "3";
				case Key.D4:
					return "4";
				case Key.D5:
					return "5";
				case Key.D6:
					return "6";
				case Key.D7:
					return "7";
				case Key.D8:
					return "8";
				case Key.D9:
					return "9";
				case Key.None:
					return String.Empty;
				default:
					return selectedKey.ToString();
			}
		}

		public static Key StringToKey(char keyC)
		{
			switch (keyC)
			{
				case ',':
					return Key.OemComma;
				case '.':
					return Key.OemPeriod;
				case '-':
					return Key.OemMinus;
				case '_':
					return Key.Separator;
				case ' ':
					return Key.Space;
				case '0':
					return Key.D0;
				case '1':
					return Key.D1;
				case '2':
					return Key.D2;
				case '3':
					return Key.D3;
				case '4':
					return Key.D4;
				case '5':
					return Key.D5;
				case '6':
					return Key.D6;
				case '7':
					return Key.D7;
				case '8':
					return Key.D8;
				case '9':
					return Key.D9;
				default:
					return StringToKey(String.Format("{0}", keyC).ToUpper());
			}
		}

		private static Key StringToKey(String keyS)
		{
			if (keyS.Length == 0)
				return Key.NoName;

			Key nextKey = (Key)Enum.Parse(typeof(Key), keyS);

			return nextKey;
		}

	}
}