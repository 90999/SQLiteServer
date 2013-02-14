using System;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Linq;

namespace Tools {

    class Sync {
        public static Object signal;
        public static bool block;
    }

    public class System
	{

        // Get Directory of current EXE-File
		public static string GetProgramDir()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
		

		// Parse Commandline Parameters
		public static StringDictionary ParseCommandlineArguments(string[] Args)
		{
			StringDictionary Parameters = new StringDictionary();
			Regex RemoveQuote = new Regex(@"^[""]?(.*?)[""]?$", RegexOptions.Compiled);

			// Combine Arguments and Separate again
			string Combined = String.Join(" ", Args);
			Args = Combined.Split(new string[] { "--" }, StringSplitOptions.None);
			if ((Args.Length>0) && (Args[0].Trim () == "")) {
				Args = Args.Where(w => w != Args[0]).ToArray(); 
			}

			// Parse Separated entrys again
			string[] ArgParts;
			foreach (string Arg in Args)
			{
				ArgParts = Arg.Split('=');

				// Parameter without "=" like --verbose
				if (ArgParts.Length == 1) {
					if (! Parameters.ContainsKey(ArgParts[0]))  {
						Parameters.Add(ArgParts[0], "true");
					}
				// Parameter with "=" like --user=test
				} else if (ArgParts.Length == 2) {
					if (! Parameters.ContainsKey(ArgParts[0]))  {
						ArgParts[1] = RemoveQuote.Replace(ArgParts[1].Trim(), "$1").Trim();
						Parameters.Add(ArgParts[0], ArgParts[1]);
					}
				}
			}

			return Parameters;
		}


	}

}

