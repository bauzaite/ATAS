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
        /// Adds a fix into the line specified.
        /// </summary>
        /// <param name="newLine">The new line to add into the users file</param>
        /// <param name="lineNumber">Which line in the file to apply the change to</param>
        public static void applySingleLineFix(functionInfo function, string newLine, int lineNumber)
        {
            // TODO: Use streamwriter?
            string[] wholeFile = File.ReadAllLines(Start.Start.fileLocation);
            wholeFile[lineNumber] = newLine;
            File.WriteAllLines(Start.Start.fileLocation, wholeFile);

            updateSingleLineInStructure(function, newLine, lineNumber - function.start);
        }

        /// <summary>
        /// Adds an array of new lines below the line specified.
        /// </summary>
        /// <param name="newLine">set of new lines to insert into the file</param>
        /// <param name="lineNumber">under which line to insert changes to</param>
        public static void applyMultipleLineFix(functionInfo function, string[] newLines, int lineNumber)
        {
            // TODO: Use streamwriter?
            List<string> wholeFile = new List<string>(File.ReadAllLines(Start.Start.fileLocation));
            wholeFile.InsertRange(lineNumber, newLines);
            File.WriteAllLines(Start.Start.fileLocation, wholeFile); 
            //clean up
            wholeFile = new List<string>();

            updateMultiplesLineInStructure(function, newLines, lineNumber - function.start);
        }

        /// <summary>
        /// Since we have updated how the users file looks, we need to update how
        /// our main allFunctions structure looks aswell.
        /// Update a single line in a single functions contents.
        /// TODO: This is not the most elegant way to update our structure. Think of other ways.
        /// </summary>
        /// <param name="function">The function that has been edited</param>
        /// <param name="newLine">The line in the function that has been edited</param>
        /// <param name="location">The location of the line that has been edited</param>
        public static void updateSingleLineInStructure(functionInfo function, string newLine, int location)
        {
            function.contents[location] = newLine;
        }

        /// <summary>
        /// Since we have updated how the users file looks, we need to update how
        /// our main allFunctions structure looks aswell.
        /// Update multiple lines in a single functions contents. Then update the location
        /// of where the function starts and ends in a file. Finally, update each function that
        /// has been affected by this change.
        /// </summary>
        /// <param name="function">The function that has been edited</param>
        /// <param name="newLines">The line in the function that has been edited</param>
        /// <param name="location">The location of the line that has been edited</param>
        public static void updateMultiplesLineInStructure(functionInfo function, string[] newLines, int location)
        {
            int oldEnd = function.end;
            // update the functions contents
            for (int i = 0; i < newLines.Length; i++)
            {
                function.contents.Insert(location + i, newLines[i]);
            }

            function.end = function.end + newLines.Length;
            // update the locations of where function starts and ends, and all functions below it.
            functionInfo tempInfo = new functionInfo();
            for (int i = 0; i < Start.Start.allFunctions.Count; i++)
            {
                // Ctags does not name the functions inside a file in order so our allFunctions
                // structure pretty much has the functions in a random order.
                if (Start.Start.allFunctions[i].start > oldEnd)
                {
                    tempInfo = Start.Start.allFunctions[i];
                    tempInfo.start = tempInfo.start + newLines.Length;
                    tempInfo.end = tempInfo.end + newLines.Length;

                    Start.Start.allFunctions[i] = tempInfo;
                    tempInfo = default(functionInfo);
                }
            }
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
