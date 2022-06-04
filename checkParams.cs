using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start;
using Utilities;

namespace checkParams
{
    /// <summary>
    /// Save relevant information about one functions parameters that are being sent in.
    /// If the parameters are pointers, check the function checks they are not NULL_PTR
    /// before executing any code.
    /// </summary>
    public class checkParams
    {
        /// <summary>
        /// Contains information on a parameter sent to a function
        /// </summary>
        public struct FunctionParameter {
            public string name;
            public string type;
        }

        /// <summary>
        /// This is a global list that contains the information on one functions parameters.
        /// Once the class has finsihed applying changes for a function, this list is cleared.
        /// </summary>
        public static List<FunctionParameter> allParams = new List<FunctionParameter>();

        /// <summary>
        /// Take the 'head' of function and extract the appriate params and put them into a list
        /// </summary>
        /// <param name="function">Function that would be called by customer</param>
        /// <param name="functionHead">Function name and parameters</param>
        /// <param name="parameterList">Ponter parameters that have been sent by the customer</param>
        static void initialiseParams(functionInfo function, string functionHead)
        {
            string unformattedParams = functionHead.Split('(')[1];
            FunctionParameter currentParameter = new FunctionParameter();
            unformattedParams = unformattedParams.Split(')')[0];
            List<string> paramList = unformattedParams.Split(',').ToList<string>();

            // trim each parameter
            for (int x = 0; x < paramList.Count; x++)
            {
                paramList[x] = paramList[x].Trim();
            }

            // remove all params that are not pointers
            for (int j = 0; j < paramList.Count(); j++)
            {
                string[] currentChars = paramList[j].Split(' ');
                currentParameter.type = currentChars[0];
                currentParameter.name = currentChars[1];
                if (currentParameter.type.Contains("PTR"))
                {
                    allParams.Add(currentParameter);
                }
                currentParameter = default(FunctionParameter);
            }

            // remove all params from list that have been checked if they are null_ptr
            foreach (string line in function.contents)
            {
                for (int i = 0; i < allParams.Count; i++)
                {
                    if (line.Contains("if") && line.Contains(allParams[i].name) && line.Contains("== NULL_PTR"))
                    {
                        allParams.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// take all pointer parameters and create a fix to put into the code
        /// </summary>
        /// <param name="function">Function that would be called by customer</param>
        /// <param name="parameterList">Pointer parameters sent by the customer, that we have not checked if they are NULL_PTR</param>
        static void createParamsFix(functionInfo function)
        {
            int firstIfLoc = 0;
            string firstIfLine = "error";
            List<string> uncheckedParams = new List<string>();
            string uncheckedParamNames = "error";
            string[] result = {""};

            // Find the correct location of the functions to put the new check in
            getLocation(function, ref firstIfLoc, ref firstIfLine);

            // Save the params that were not checked
            prepParamStrings(ref uncheckedParams, ref uncheckedParamNames);

            // create an array containing an if loop with all unchecked parameters where the if loop
            // handles all outcomes of the loop.
            createFix(ref result, function, uncheckedParamNames, firstIfLine, uncheckedParams);

            // Apply change into the file.
            Utilities.all.applyMultipleLineFix(function, result, firstIfLoc);

            // cleanup
            allParams = new List<FunctionParameter>();
            Array.Clear(result, 0, result.Length);
        }

        /// <summary>
        /// If there are sent pointer parameters, check the function ensures they
        /// are not NULL_PTR before doing anything else.
        /// </summary>
        /// <param name="function">Functioned called by customer</param>
        /// <param name="functionHead">Part of function that contains its name and parameters sent to it</param>
        public static void notNull(functionInfo function, string functionHead)
        {
            initialiseParams(function, functionHead);

            // If there are some params where their input isnt checked
            if (allParams.Count > 0)
            {
                createParamsFix(function);
            }
            // TODO:
            // currently we have - if it has 'PTR' in the type then create  'parameter.name != null_ptr 
            // make a new - if it has 'HANDLE' in the type then create 'parameter.name != CK_INVALID_HANDLE' check? Outside scope

        }

        /// <summary>
        /// Get the location of the first 'if' in a function
        /// </summary>
        /// <param name="function">function to find instance of 'if'</param>
        /// <param name="firstInstIf">location of first instance of 'if'</param>
        /// <param name="fixFirstInsIf">the string containing firts instance of 'if'</param>
        static void getLocation(functionInfo function, ref int firstInstIf, ref string fixFirstInsIf)
        {
            for (int i = 0; i < function.length; i++)
            {
                if (function.contents[i].Contains("if (")) // add a catch here: they might of not have any if statements
                {
                    firstInstIf = function.start + i;
                    fixFirstInsIf = function.contents[i].Replace("if", "else if");
                    break;
                }
                else if (function.length == i)
                {
                    // they have no ifs, put check at top of the function
                    firstInstIf = function.start + 1; // TODO: put below functionhead 
                    fixFirstInsIf = "else {}"; // TEST THIS!
                }
            }
        }

        /// <summary>
        /// This function creates:
        /// A string containing unchecked params for the 'if' line.
        /// A string containing unchecked params for the logging line.
        /// </summary>
        /// <param name="paramsInIfLoop"></param>
        /// <param name="paramsInLogging"></param>
        static void prepParamStrings(ref List<string> paramsInIfLoop, ref string paramsInLogging)
        {
            if (allParams.Count > 1)
            {
                paramsInLogging = "";
                for (int i = 0; i < allParams.Count; i++)
                {
                    if (i < (allParams.Count - 1))
                    {
                        paramsInIfLoop.Add(allParams[i].name + " == NULL_PTR" + " || ");
                        paramsInLogging = paramsInLogging + allParams[i].name + " or ";
                    }
                    else
                    {
                        paramsInIfLoop.Add(allParams[i].name + " == NULL_PTR");
                        paramsInLogging = paramsInLogging + allParams[i].name;
                    }
                }
            }
            else if (allParams.Count == 1)
            {
                paramsInIfLoop.Add(allParams[0].name + " == NULL_PTR");
                paramsInLogging = allParams[0].name;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="function"></param>
        /// <param name="paramNamesForLog"></param>
        /// <param name="fixFirstInsIf"></param>
        /// <param name="paramsInIfLoop"></param>
        static void createFix(ref string[] result, functionInfo function, string paramNamesForLog,
                              string fixFirstInsIf, List<string> paramsInIfLoop)
        {
            string tab = "  ";
            string uncheckedParamsIfLine = "error";

            // Prep the first line of if loop
            uncheckedParamsIfLine = "if (";
            foreach (string parameter in paramsInIfLoop)
            {
                uncheckedParamsIfLine = uncheckedParamsIfLine + parameter;
            }
            uncheckedParamsIfLine = uncheckedParamsIfLine + "){";

            // Create array to apply into the file.
            if (Start.Start.changesCommented == 1)
            {
                result = new string[]{ "// ☠" + tab + uncheckedParamsIfLine,
                                       "// ☠" + tab + tab + "rv = CKR_ARGUMENTS_BAD;",
                                       "// ☠" + tab + tab + "WCK_ERROR(\"" + function.name + " " + paramNamesForLog + " is NULL\");",
                                       "// ☠" + tab + "}",
                                       "// ☠" + fixFirstInsIf
                };
            }
            else
            {
                result = new string[] { tab + uncheckedParamsIfLine + " // ☠",
                                        tab + tab + "rv = CKR_ARGUMENTS_BAD; // ☠",
                                        tab + tab + "WCK_ERROR(\"" + function.name + " " + paramNamesForLog + " is NULL\"); // ☠",
                                        tab + "} // ☠",
                                        fixFirstInsIf + " // ☠"
                };
            }
        }

    }
}
