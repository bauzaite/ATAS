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

        public static void notNull(functionInfo function, string functionHead)
        {
            // save variables if they have p in the name or ptr in type
            initialiseParams(functionHead, allParams);

            // remove any params in list that have been checked if they are NULL_PTR
            foreach (string line in function.contents)
            {
                for (int i = 0; i < allParams.Count; i++) {
                    if (line.Contains(allParams[i].name) && line.Contains("== NULL_PTR"))
                    {
                        allParams.RemoveAt(i);
                    }
                }
            }

            // if there are some params where their input isnt checked
            if (allParams.Count > 0)
            {
                createParamsFix(function, allParams);
            }

            // ELSE DO NOTHING


            // old notes
            // if it has 'HANDLE' in the type then create 'parameter.name != CK_INVALID_HANDLE'
            // if it has 'PTR' in the type then create  'parameter.name != null_ptr
            // else then create 'parameter.nmae != 0'

            // 1) create a code line that checks all these and put it in the code
            // fix = 
            // if(  )


            // 2) code line that only contains params checks that dont already esxist

        }

        /// <summary>
        /// take all pointer parameters and create a fix to put into the code
        /// </summary>
        /// <param name="function"></param>
        /// <param name="parameterList"></param>
        static void createParamsFix(functionInfo function, List<parameter> parameterList)
        {
            // create fix and add to top of the file

            int firstInstIf = 0;

            for (int i = 0; i < function.length; i++)
            {
                if (function.contents[i].Contains("if (")) // add a catch here: they might of not have any if statements
                {
                    firstInstIf = function.start + i;
                    break;
                }
            }

            List<string> paramFix = new List<string>();
            string paramNames = "";


            foreach (parameter param in parameterList)
            {
                paramFix.Add(param.name + " == NULL_PTR");   // theres gonna be a bug-ish here. what if teres only one ptr param it wont look right in error msg
                paramNames = paramNames + param.name + " ";
            }
            string newFix = "if (" ;

            foreach(string pp in paramFix){
                newFix = newFix + pp;
            }

            newFix = newFix + "){";

            string[] paramFixArray = {newFix, "{", "rv = CKR_ARGUMENTS_BAD", "WCK_ERROR(\""+function.name+ " "+ paramNames + " is NULL\")", "}" };

            Utilities.all.applyFixaRRAY(paramFixArray, @"C:\Users\bauzaiv\testData.txt", firstInstIf - 1);

        }

        /// <summary>
        /// take the 'head' of function and extract the params and put them into a list
        /// </summary>
        /// <param name="functionHead"></param>
        /// <param name="parameterList"></param>
        static void initialiseParams(string functionHead, List<parameter> parameterList)
        {
            string allParams = functionHead.Split('(')[1];
            allParams = allParams.Split(')')[0];

            List<string> paramList = allParams.Split(',').ToList<string>();
            for (int i = 0; i < paramList.Count; i++)
            {
                paramList[i] = paramList[i].Trim();
            }

            string[,] sepPrams = new string [paramList.Count(), 2] ;
            int k = 0;
            parameter currentParameter = new parameter();
            for (int j = 0; j < paramList.Count(); j++)
            {
                string[] currentChars = paramList[j].Split(' ');
                currentParameter.type = currentChars[0];
                currentParameter.name = currentChars[1];
                if (currentParameter.name.Contains("p") || currentParameter.type.Contains("PTR")) {
                    parameterList.Add(currentParameter);
                }
                currentParameter = default(parameter);
            }
        }


    }
}
