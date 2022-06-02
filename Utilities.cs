using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start;

namespace Utilities
{
    public class all
    {
        /// <summary>
        /// Adds a fix into the line specified
        /// </summary>
        /// <param name="newLine"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public static void applyFix(string newLine, string fileName, int lineNumber)
        {
            // TODO: Use streamwriter?
            string[] wholeFile = File.ReadAllLines(fileName);
            wholeFile[lineNumber] = newLine;
            File.WriteAllLines(fileName, wholeFile);
        }
        public static void updateStructure(functionInfo function, string newLine, int location)
        {
            function.contents[location] = newLine; // this may not be needed, test it
        }

        /// <summary>
        /// Adds a array of new lines below the line specified
        /// </summary>
        /// <param name="newLine">set of new lines to insert into the file</param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber">under which line to insert changes to</param>
        public static void applyFixArray(string[] newLine, string fileName, int lineNumber)
        {
            // TODO: Use streamwriter?
            // i think line number was wrong
            List<string> wholeFile = new List<string>(File.ReadAllLines(fileName));
            wholeFile.InsertRange(lineNumber, newLine);
            File.WriteAllLines(fileName, wholeFile);
        }

        /// <summary>
        /// Check if a defined function has any parameters sent to it
        /// </summary>
        /// <param name="functionHead">Part of function that contains it's name and any parameters sent</param>
        /// <returns>0 if there are parameters sent</returns>
        public static int paramsAreSentToFunction(string functionHead)
        {
            // only checking in WSOP_C functions
            string paramsSent = functionHead.Split('(')[1];
            paramsSent = paramsSent.Split(')')[0];
            if (paramsSent != "") { return 0; }
            else { return 1; }
        }
    }
}
