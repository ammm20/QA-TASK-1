using System;
using System.IO;

namespace AQ_task
{
    class Program
    {

        static void Main(string[] args)
        {

            //full path of the executable file of the process to lauch
            string processFullPath;
            //number of seconds between execution of the process
            int intervalInSeconds;

          
            try
            {

                processFullPath = args[0];

                intervalInSeconds = Convert.ToInt32(args[1]);

                //Create a new RunProcess object to execute the selected process
                RunProcess run = new RunProcess(processFullPath, intervalInSeconds);


                string jsonFilePath = @"C:\Users\Public\Documents\jsonFile.json";

                //if json file with the previous data exists delete it
                if (File.Exists(jsonFilePath))
                {

                    System.IO.File.Delete(jsonFilePath);

                }

                run.LaunchProcess();

            }
            catch (Exception e)
            {
                Console.WriteLine($"The following exception was raised: {e.Message}");
            }

        }

    }
}
