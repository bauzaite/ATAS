using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start;
using Utilities;

namespace checkParams
{
    public class checkParams
    {
        struct parameter {
            public string name;
            public string type;
        }

        static List<parameter> allParams = new List<parameter>();

        /// <summary>
        /// take the 'head' of function and extract the appriate params and put them into a list
        /// </summary>
        /// <param name="function">Function that would be called by customer</param>
        /// <param name="functionHead">Function name and parameters</param>
        /// <param name="parameterList">Ponter parameters that have been sent by the customer</param>
        static void initialiseParams(functionInfo function, string functionHead, List<parameter> parameterList)
        {
            string allParams = functionHead.Split('(')[1];
            parameter currentParameter = new parameter();
            allParams = allParams.Split(')')[0];
            List<string> paramList = allParams.Split(',').ToList<string>();

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
                if (currentParameter.name.Contains("p") || currentParameter.type.Contains("PTR"))
                {
                    parameterList.Add(currentParameter);
                }
                currentParameter = default(parameter);
            }

            // remove all params from list that have been checked if they are null_ptr
            foreach (string line in function.contents)
            {
                for (int i = 0; i < parameterList.Count; i++)
                {
                    if (line.Contains("if") && line.Contains(parameterList[i].name) && line.Contains("== NULL_PTR"))
                    {
                        parameterList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// take all pointer parameters and create a fix to put into the code
        /// </summary>
        /// <param name="function">Function that would be called by customer</param>
        /// <param name="parameterList">Pointer parameters sent by the customer, that we have not checked if they are NULL_PTR</param>
        static void createParamsFix(functionInfo function, List<parameter> parameterList)
        {
            int firstInstIf = 0;
            string fixFirstInsIf = "error";
            List<string> uncheckedParams = new List<string>();
            string uncheckedParamNames = "error";
            string tab = "  ";
            string uncheckedParamsIfLine = "error";
            string[] result;

            // Find the correct location of the functions to put the new check in
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

            // For the logging line: Save the params that were not checked
            foreach (parameter param in parameterList)
            {
                uncheckedParams.Add(param.name + " == NULL_PTR");   // theres gonna be a bug-ish here. what if teres only one ptr param it wont look right in error msg
                uncheckedParamNames = uncheckedParamNames + param.name;
            }

            // Prep the first like of if loop
            uncheckedParamsIfLine = "if (";
            foreach (string parameter in uncheckedParams)
            {
                uncheckedParamsIfLine = uncheckedParamsIfLine + parameter;
            }
            uncheckedParamsIfLine = uncheckedParamsIfLine + "){";

            // Create array to apply into the file.
            if (Start.Start.changesCommented == 1)
            {
                result = new string[]{ "// ☠" + tab + uncheckedParamsIfLine,
                                               "// ☠" + tab + "{",
                                               "// ☠" + tab + tab + "rv = CKR_ARGUMENTS_BAD",
                                               "// ☠" + tab + tab + "WCK_ERROR(\"" + function.name + " " + uncheckedParamNames + " is NULL\")",
                                               "// ☠" + tab + "}",
                                               "// ☠" + fixFirstInsIf
                };
            }
            else
            {
                result = new string[] { tab + uncheckedParamsIfLine + " // ☠",
                                               tab + "{ // ☠",
                                               tab + tab + "rv = CKR_ARGUMENTS_BAD // ☠",
                                               tab + tab + "WCK_ERROR(\"" + function.name + " " + uncheckedParamNames + " is NULL\") // ☠",
                                               tab + "} // ☠",
                                               fixFirstInsIf + " // ☠"
                };
            }

            // Apply change into the file.
            Utilities.all.applyFixArray(result, @"C:\Users\bauzaiv\testData.txt", firstInstIf);
        }

        /// <summary>
        /// If there are sent pointer parameters, check the function ensures they
        /// are not NULL_PTR before doing anything else.
        /// </summary>
        /// <param name="function">Functioned called by customer</param>
        /// <param name="functionHead">Part of function that contains its name and parameters sent to it</param>
        public static void notNull(functionInfo function, string functionHead)
        {
            initialiseParams(function, functionHead, allParams);

            // If there are some params where their input isnt checked
            if (allParams.Count > 0)
            {
                createParamsFix(function, allParams);
            }
            // TODO:
            // like we do - if it has 'PTR' in the type then create  'parameter.name != null_ptr 
            // make a new - if it has 'HANDLE' in the type then create 'parameter.name != CK_INVALID_HANDLE' check

        }

    }
}
