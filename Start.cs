using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fixStrings;
using checkParams;

// TODO: extensive testing, black, white box ones
// - perf tests - the strcpy check takes 2s to process a 1000 line file

namespace Start
{
    public struct functionInfo
    {
        public string name;
        public int start;
        public int end;
        public int length;
        public List<string> contents;
    }

    public class Start
    {
        static List<functionInfo> allFunctions = new List<functionInfo>();
        static string ctagLocation = "";
        static public string fileLocation = "";
        static public int changesCommented = 0;
        static int fixStrcpy = 0;
        static int initAllVar = 0;
        static int checkParamNotNull = 0;
        static int addWinStrcpy = 0;

        static void Main(string[] args)
        {
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
            }
            finally
            {
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
            /*
            if (initAllVar == 1)
            {
                initVarsCheck();
            }
            */
        }

        static void initialise()
        {
            //ctagLocation = @"C:\Users\bauzaiv\ctag.txt";
            //fileLocation = @"C:\Users\bauzaiv\testData.txt";
            string[] ctagOutput = System.IO.File.ReadAllLines(ctagLocation);


            functionInfo functionInfo = new functionInfo();
            functionInfo.contents = new List<string>();

            foreach (string line in ctagOutput)
            {
                // save function name, start , end
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
                    Console.WriteLine("Error: Ctag filtering is probbably incorrect, try ctags --fields=+ne -o - --sort=yes");
                    Console.WriteLine("{0}", e);
                    System.Environment.Exit(0);
                }

                // go inside the specified c file and save that functions contents.
                int lineLocation = functionInfo.start;
                for (int i = 0; i < functionInfo.length; i++)
                {
                    functionInfo.contents.Add(System.IO.File.ReadLines(fileLocation).Skip(lineLocation).Take(1).First());
                    lineLocation++;
                }
                allFunctions.Add(functionInfo);
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
            foreach (functionInfo function in allFunctions)
            {
                for (int line = 0; line < function.length; line++)
                {
                    // If user used strcpy in the function and program hasn't provided a fix,   <- BUG: what if it checked and there was no problem?
                    // check the strcpy use is safe and provide any solutions.
                    if (function.contents[line].Contains("strcpy") && !function.contents[line].Contains("☠"))
                    {
                        fixStrings.checkStrcpy.strCpyUsed(function, function.contents[line], line);
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
            foreach (functionInfo function in allFunctions)
            {
                string functionHead = function.contents[0] + function.contents[1] + function.contents[2]; // some functions 'heads' spread up to 3 lines. test this later
                //  Perform parameter checking if a function is WSOPC type and has params sent in   -> BUG(?): every wsopc function will have params sent in
                if (functionHead.Contains("WSOPC") && (0 == Utilities.all.paramsAreSentToFunction(functionHead)))
                {
                    checkParams.checkParams.notNull(function, functionHead);
                }
            }

        }
    }
}