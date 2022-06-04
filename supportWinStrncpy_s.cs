using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.IO;
using Start;

namespace checkInitVariables
{
    internal class supportWinStrncpy_s
    {

        /// <summary>
        /// Provide windows build support by turning:
        /// strncpy(dst, src, strlen(dst)) into strncpy_s(dst, strlen(dst), src, strlen(dst)).
        /// </summary>
        /// <param name="function">function to edit</param>
        /// <param name="strncpyLine">The line containing the strncpy statement</param>
        /// <param name="locationStrncpyLine">Which part of the function the strncpy line exists</param>
        public static void addWinStrncpy_s(functionInfo function, string strncpyLine, int locationStrncpyLine)
        {
            string[] result = { "" }; ;
            string strcpyLineMark = "";
            string desinationName = strncpyLine.Split('(', ',')[1].Trim();
            string source = strncpyLine.Split(',', ',')[1].Trim();
            string destinationSize = strncpyLine.Split(',', ')')[2].Trim();
            string tab = strncpyLine.Split('s')[0];
            string strncpy_sLine = tab + "strncpy_s(" + desinationName + ", " + destinationSize + ", " + source + ", " + destinationSize + ");";

            // Create array to apply into the file.
            if (Start.Start.changesCommented == 1)
            {
                result = new string[]{ "// ☠" + tab + "#ifdef WIN32",
                                       "// ☠" + tab + strncpy_sLine,
                                       "// ☠" + tab + "#else",
                                       "// ☠" + tab + strncpyLine,
                                       "// ☠" + tab + "#endif"
                };
                strcpyLineMark = strncpyLine + "// ☠: Delete"; // didnt add this change in array since I think it would be easier for future changes.
            }
            else
            {
                result = new string[]{ tab + "#ifdef WIN32) // ☠",
                                       tab + strncpy_sLine + "// ☠",
                                       tab + "#else // ☠",
                                       tab + strncpyLine + "// ☠",
                                       tab + "#endif // ☠"
                };
                strcpyLineMark = "// ☠ //" + strncpyLine;
            }

            // update file
            Utilities.all.applySingleLineFix(function, strcpyLineMark, function.start + locationStrncpyLine);
            Utilities.all.applyMultipleLineFix(function, result, function.start + locationStrncpyLine);

            // cleanup
            Array.Clear(result, 0, result.Length);
        }
    }
}
