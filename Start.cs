
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fixStrings;
using checkParams;
using checkInitVariables;

namespace Start
{
    /// <summary>
    /// Information about each function inside file sent by the user.
    /// </summary>
    public struct functionInfo
    {
        public string name;
        public int start;
        public int end;
        public int length;
        public List<string> contents;
    }

    /// <summary>
    /// This class saves all relevant information about the code in the file
    /// sent by the user. It then execures requested checks on the code that
    /// was set in the flags inside the script prior executing.
    /// </summary>
    public class Start
    {
        public static List<functionInfo> allFunctions = new List<functionInfo>();
        static public string fileLocation = "";
        static public int changesCommented = 0;
        static string ctagLocation = "";
        static int addWinStrcpy = 0;
        static int fixStrcpy = 0;

        /// <summary>
        /// Saves relevant data in file and performs check
        /// according to the flags set.
        /// </summary>
        /// <param name="args">File location and flags sent by user from script and cmd</param>
        static void Main(string[] args)
        {
            int initAllVar = 0;
            int checkParamNotNull = 0;
            fileLocation = args[0];
            try
            {
                fixStrcpy = Int32.Parse(args[1]);
                initAllVar = Int32.Parse(args[2]);
                checkParamNotNull = Int32.Parse(args[3]);
                addWinStrcpy = Int32.Parse(args[4]);
                changesCommented = Int32.Parse(args[5]);
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("Flags are not set, turning everything on.");
                changesCommented = 1;
                fixStrcpy = 1;
                initAllVar = 1;
                checkParamNotNull = 1;
                addWinStrcpy = 1;
            }

            // Initialise Function Information
            initialise();

            // Perform Error Checking
            if(fixStrcpy == 1)
            {
                strCpyUsedCheck();
            }
            if (checkParamNotNull == 1)
            {
                allParamsCheck();
            }
            if (addWinStrcpy == 1)
            {
                strCpyUsedCheck();
            }
            /*
            if (initAllVar == 1)
            {
                initVarsCheck();
            }
            */
        }

        /// <summary>
        /// Reads each function in file sent by the user and
        /// extracts relavant information and saves it into a list
        /// of stucts.
        /// </summary>
        static void initialise()
        {
            string[] ctagOutput = System.IO.File.ReadAllLines(ctagLocation);

            functionInfo functionInfo = new functionInfo();
            functionInfo.contents = new List<string>();

            foreach (string line in ctagOutput)
            {
                // Try to save function information else, skip to next one.
                try
                {
                    functionInfo.name = line.Split('\t')[0].Trim();
                    string tmp = line.Split("line:")[1];
                    functionInfo.start = Int32.Parse(tmp.Split("t")[0].Trim()) - 1;
                    functionInfo.end = Int32.Parse(line.Split("end:")[1]) - 1;
                    functionInfo.length = functionInfo.end - functionInfo.start + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Tool cannot process ctags filter, skipping to the next function.");
                    Console.WriteLine("Function skipped: " + line);
                    continue;
                }

                // go inside the specified c file and save that functions contents.
                int lineLocation = functionInfo.start;
                for (int i = 0; i < functionInfo.length; i++)
                {
                    functionInfo.contents.Add(System.IO.File.ReadLines(fileLocation).Skip(lineLocation).Take(1).First());
                    lineLocation++;
                }

                // add it to the list
                allFunctions.Add(functionInfo);

                // clear function information we just extracted since it has been saved in the list
                functionInfo = default(functionInfo);
                functionInfo.contents = new List<string>();
            }

        }

        /// <summary>
        /// Tool should only provide a fix for strcpy if src and dest is defined locally.
        /// Otherwise, it should suggest to use strncpy but it wont give the third parameter for strncpy.
        /// </summary>
        static void strCpyUsedCheck()
        {
            //foreach (functionInfo function in allFunctions)
            for(int i = 0; i < allFunctions.Count; i++)
            {
                for (int line = 0; line < allFunctions[i].length; line++)
                {
                    // If user used strcpy in the function and program hasn't provided a fix,   <- BUG: what if it checked and there was no problem?
                    // check the strcpy use is safe and provide any solutions.
                    if (allFunctions[i].contents[line].Contains("strcpy") && !allFunctions[i].contents[line].Contains("☠") && 1 == fixStrcpy)
                    {
                        fixStrings.checkStrcpy.strCpyUsed(allFunctions[i], allFunctions[i].contents[line], line);
                    }
                    else if (allFunctions[i].contents[line].Contains("strncpy") && !allFunctions[i].contents[line].Contains("☠") && 1 == addWinStrcpy) // I didnt make a new func for this since I though id save some checcking for the program
                    {
                         checkInitVariables.supportWinStrncpy_s.addWinStrncpy_s(allFunctions[i], allFunctions[i].contents[line], line);
                    }
                }
            }
        }

        /// <summary>
        /// Tool should identify if a function does not check for null ptr parameters before exectuting.
        /// Otherwise, it should add in code to check for null ptr parameters.
        /// </summary>
        static void allParamsCheck()
        {
           // foreach (functionInfo function in allFunctions) // super interesting bug here!!! the foreach was okay with 1 change in allFunctions but then complained about the list changing
            for(int i =0; i < allFunctions.Count; i++)
            {
                string functionHead = allFunctions[i].contents[0] + allFunctions[i].contents[1] + allFunctions[i].contents[2]; // some functions 'heads' spread up to 3 lines. test this later
                //  Perform parameter checking if a function is WSOPC type and has params sent in   -> BUG(?): every wsopc function will have params sent in
                if (functionHead.Contains("WSOPC") && (0 == Utilities.all.paramsAreSentToFunction(functionHead)))
                {
                    // TODO: checkDoesntContainSkull(); // Actually, i dont need this since the program will know if checking has been done.
                    checkParams.checkParams.notNull(allFunctions[i], functionHead);
                }
            }
        }
    }
}
