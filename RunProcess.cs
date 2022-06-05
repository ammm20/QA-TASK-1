using Newtonsoft.Json;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;

namespace AQ_task
{
    public class RunProcess
    {

        //full path of the executable file of the process to lauch
        string processFullPath;

        //number of seconds between execution of the process
        int intervalInSeconds;

        //object of type process
        static Process myProcess;

        //CPU Usage Percentage
        float CPUUsagePercentage = 0;

        //Memory consumption: Working Set
        long workingSet = 0;

        //Memory consumption: Private Bytes
        long privateBits = 0;

        //Number of open handles
        int openHandles = 0;


        //Data structure for JSON file
        public class jsonData
        {
            public DateTime Date { get; set; }
            public float CPUUsagePercentage { get; set; }
            public long workingSet { get; set; }
            public long privateBits { get; set; }
            public int openHandles { get; set; }
        }

        //variable for logs
        private static Logger logger = LogManager.   GetCurrentClassLogger();

        


        public RunProcess(string processFullPath, int intervalInSeconds)
        {
            this.processFullPath = processFullPath;
            this.intervalInSeconds = intervalInSeconds;

        }


        public void LaunchProcess()
        {
            try
            {
        
                //create a new object of type process to launch our .exe file
                myProcess = new Process();

                myProcess.StartInfo = new ProcessStartInfo(processFullPath, "-n")
                {
                    //this will make to start the process without starting the operating system shell
                    UseShellExecute = false
            
                };


                using (myProcess = Process.Start(myProcess.StartInfo))
                {

                    // timer to call launch every intervalInSeconds seconds
                    System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromSeconds(intervalInSeconds).TotalMilliseconds);
                    timer.AutoReset = true;
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(GetInformation);
                    timer.Start();


                    while (!myProcess.HasExited)
                    {

                        //while the process is running the timer will call launch method every intervalInSeconds seconds

                    }

                    Console.WriteLine($"Process exit code: {myProcess.ExitCode}");
                }
            }
            catch (Exception e)
            {
              
                Console.WriteLine($"The following exception was raised: {e.Message}");

                logger.Fatal("The application finished" + $"The following exception was raised: {e.Message}");

            }

        }


        void GetInformation(object sender, ElapsedEventArgs e)
        {

            if (myProcess.Responding)
            {

                // get CPU usage(percent)
                GetCPUUsagePercentage();

                // get Memory consumption: Working Set
                GetMCWorkingSet();

                // get Memory consumption: Private Bytes
                GetMCPrivateBits();

                // get Number of open handles
                GetNumberOpenHandles();

                //write the data in JSON format
                WriteJSONData();
            }
            else
            {

                logger.Warn("Process Status:  Not Responding" );
                
                Console.WriteLine("Process Status:  Not Responding");
            }

        }


        void GetCPUUsagePercentage()
        {

            try
            {

                PerformanceCounter cpuCounter = new PerformanceCounter
                {
                    CategoryName = "Processor Information",
                    CounterName = "% Processor Utility",
                    InstanceName = "_Total"
                };

                // will always start at 0
                dynamic firstValue = cpuCounter.NextValue();
                System.Threading.Thread.Sleep(1000);

                // now matches task manager reading
                CPUUsagePercentage = cpuCounter.NextValue();


            }
            catch (Exception e)
            {

                logger.Error(e, "Exception was raised in GetCPUUsagePercentage");

                Console.WriteLine($"The following exception was raised in GetCPUUsagePercentage: {e.Message}");
            }

        }


        void GetMCWorkingSet()
        {
            try
            {
                workingSet = myProcess.WorkingSet64;
                     
            }
            catch (Exception e)
            {

                logger.Error(e, "Exception was raised in GetMCWorkingSet");

                Console.WriteLine($"The following exception was raised in GetMCWorkingSet: {e.Message}");
            }

        }


        void GetMCPrivateBits()
        {

            try
            {

                privateBits = myProcess.PrivateMemorySize64;

            } 
            catch (Exception e)
            {

                logger.Error(e, "Exception was raised in GetMCPrivateBits");

                Console.WriteLine($"The following exception was raised in GetMCPrivateBits: {e.Message}");
            }

        }


        void GetNumberOpenHandles()
        {

            try
            {

                openHandles = myProcess.HandleCount;

            }
            catch (Exception e)
            {

                logger.Error(e, "Exception was raised in GetNumberOpenHandles");

                Console.WriteLine($"The following exception was raised in GetNumberOpenHandles: {e.Message}");
            }
       
        }


        void WriteJSONData()
        {

            try
            {

                //List with previous json data
                List<jsonData> previousData = new List<jsonData>();


                jsonData nextData = new jsonData();


                nextData.Date = DateTime.Now;
                nextData.CPUUsagePercentage = CPUUsagePercentage;
                nextData.workingSet = workingSet;
                nextData.privateBits = privateBits;
                nextData.openHandles = openHandles;
                

                string jsonFilePath = @"C:\Users\Public\Documents\jsonFile.json";

                // Read existing json data if file already exists
                if (File.Exists(jsonFilePath))
                {

                    var jsonData = System.IO.File.ReadAllText(jsonFilePath);

                    // De-serialize to object or create new list
                    previousData = JsonConvert.DeserializeObject<List<jsonData>>(jsonData)
                                          ?? new List<jsonData>();
                }

                // Add new Data
                previousData.Add(nextData);
               
                // Update json data string
                var finalJsonData = JsonConvert.SerializeObject(previousData,Formatting.Indented);

                System.IO.File.WriteAllText(jsonFilePath, finalJsonData);



            }
            catch (Exception e)
            {

                logger.Error(e, "Exception was raised in WriteJSONData");

                Console.WriteLine($"The following exception was raised in WriteJSONData: {e.Message}");
            }

        }


    }
}
