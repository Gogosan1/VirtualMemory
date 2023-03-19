using System;
namespace VirtualMem
{
	public static class ConsoleHelper
	{
		public static long? ReadLong(String message, int? defaultValue)
		{
			long value;
			do
			{
				Console.Write(message);
				string? s = Console.ReadLine();
				if (s == null || s == "") return defaultValue;

				if (Int64.TryParse(s ?? "", out value))
				{
					break;
				}
			} while (true);

			return value;
		}

		public static String ReadString(String message, String defaultValue)
		{
            Console.Write(message);
            string? s = Console.ReadLine();
            return (s == null || s == "") ? defaultValue : s;
        }
    }
}

