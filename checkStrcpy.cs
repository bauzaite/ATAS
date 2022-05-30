using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start;
using Utilities;
using System.IO;

namespace fixStrings
{
    public class checkStrcpy
    {
        public static void strCpyUsed(functionInfo function, string fixLine, int location)
        {
            string desinationName = fixLine.Split('(', ',')[1].Trim();
            string source = fixLine.Split(',', ')')[1].Trim();
            for (int line = 0; line < function.length; line++)
            {
                // is detination size defined
                if (function.contents[line].Contains(desinationName) && function.contents[line].Contains("["))
                {
                    // are source contents defined
                    if (true == SourceDefined(function, ref source))
                    {
                        string sizeOfDestinationStr = function.contents[line].Split("[")[1];
                        sizeOfDestinationStr = sizeOfDestinationStr.Split("]")[0];
                        int sizeOfDestination = Int32.Parse(sizeOfDestinationStr);

                        if (source.Length + 1 >= sizeOfDestination)
                        {
                            string fix = function.contents[line].Replace(sizeOfDestinationStr, (source.Length + 1).ToString());
                            Utilities.all.applyFix(fix, Start.Start.fileLocation, function.start + line);
                        }
                        break;
                    }
                    else
                    {
                        StrcpyToStrncpy(function, fixLine, location);
                        break;
                    }
                }
                else if (line == (function.length - 1))
                {
                    // the size of destination isnt defined within the fucntion
                    // so you prob dont have control over it
                    // so it istn safe to use strcpy prob
                    StrcpyToStrncpy(function, fixLine, location);
                    break;
                }
            }
        }

        static bool SourceDefined(functionInfo function, ref string source)
        {
            if (source.Contains("\""))
            {
                source = source.Split("\"")[1];
                //source = source.Split("\"")[0];
                // now it looks like thiis -> source = "hello"
                return true;
            }
            else
            {   // this means source contains name of source
                for (int line = 0; line < function.length; line++)
                {
                    if (function.contents[line].Contains(source) && function.contents[line].Contains("="))
                    {
                        source = function.contents[line].Split("=")[1];
                        source = source.Split(";")[0].Trim();
                        source = source.Split("\"")[1];
                        return true;
                    }
                    else if (line == (function.length - 1))
                    {
                        break;
                    }
                }
            }
            return false;
        }

        static void StrcpyToStrncpy(functionInfo function, string strCpyLine, int location)
        {
            string desinationName = strCpyLine.Split('(', ',')[1].Trim();
            string sourceName = strCpyLine.Split(',', ')')[1].Trim();
            string fix = "// ☠ Something went wrong";

            if (Start.Start.changesCommented == 1) {
                fix = strCpyLine + " // ☠ strncpy(" + desinationName +
                       ", " + sourceName + ", /* sizeof(" + desinationName + ")*/);";
            }
            else {
                fix = strCpyLine.Split('s')[0] + "strncpy(" + desinationName +
                    ", " + sourceName + ", /* strlen(" + desinationName + ")*/); // ☠";
            }
            Utilities.all.applyFix(fix, Start.Start.fileLocation, function.start + location);
            Utilities.all.updateStructure(function, fix, location);
        }
    }
}
