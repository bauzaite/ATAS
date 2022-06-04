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
    /// <summary>
    /// This class ensures the user is using strcpy() functon safely.
    /// Otherwise, it suggest to either improve strcpy() and gives fixes
    /// or it suggests to use strncpy() and gives a fix.
    /// </summary>
    public class checkStrcpy
    {
        /// <summary>
        /// Figure out if use of strcpy() is safe, otherwise
        /// suggest to the user to use strncpy() instead.
        /// </summary>
        /// <param name="function">function informaton that contains strcpy</param>
        /// <param name="fixLine">the line in te function with 'strcoy' in it</param>
        /// <param name="location">the location of the 'strcpy' mention</param>
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
                            string fix = "// ☠" + function.contents[line].Replace(sizeOfDestinationStr, (source.Length + 1).ToString());
                            Utilities.all.applySingleLineFix(function, fix, function.start + line);
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

        /// <summary>
        /// Check if source of strcpy(dst,src) is defined locally.
        /// </summary>
        /// <param name="function">contents of the function</param>
        /// <param name="source">the name of source in the function</param>
        /// <returns></returns>
        static bool SourceDefined(functionInfo function, ref string source)
        {
            if (source.Contains("\"")) // if source has been defined like this strcpy(dst, "hello")
            {
                source = source.Split("\"")[1]; // format it and return it like so -> source = "hello"
                return true;
            }
            else // this means source is a initialised variable -> strcpy(dst, hello)
            {   
                for (int line = 0; line < function.length; line++) // look if it's been definied locally
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

        /// <summary>
        /// Suggest the user to user strncpy instead and give a semi-automated fix.
        /// </summary>
        /// <param name="function">function with strcpy</param>
        /// <param name="strCpyLine">line containing the strcpy</param>
        /// <param name="location">location of function strcpy is menttioned in</param>
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
            Utilities.all.applySingleLineFix(function, fix, function.start + location);
        }
    }
}
