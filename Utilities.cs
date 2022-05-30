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

        public static void applyFixaRRAY(string[] newLine, string fileName, int lineNumber)
        {
            // TODO: Use streamwriter?
            // i think line number was wrong
            List<string> wholeFile = new List<string>(File.ReadAllLines(fileName));
            wholeFile.InsertRange(lineNumber, newLine);
            File.WriteAllLines(fileName, wholeFile);
        }

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
