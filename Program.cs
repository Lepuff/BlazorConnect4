using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BlazorConnect4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("./Data");
            }
            //CreateHostBuilder(args).Build().Run();



            Training();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });



        public static void Training()
            {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("started loading AI's");

            
            AIModels.QAgent RedAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/RedV4.bin");
            AIModels.QAgent YellowAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/YellowV4.bin");

            TimeSpan ts = sw.Elapsed;
            Console.WriteLine("Done loading AI's");
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            //AIModels.QAgent RedAi = new AIModels.QAgent(Model.CellColor.Red);
            //AIModels.QAgent YellowAi = new AIModels.QAgent(Model.CellColor.Yellow);

            //AIModels.RandomAI randomAI = new AIModels.RandomAI();
            //RedAi.WorkoutV2( randomAI, 1000);
            //YellowAi.WorkoutV2(randomAI, 1000);


            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine(i);
                if (i % 2 == 0)
                {
                    YellowAi.WorkoutV2(RedAi, 1000);
                }
                else
                {
                    RedAi.WorkoutV2(YellowAi, 1000);
                }
            }
            ts = sw.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.WriteLine("saving AI's");
            RedAi.ToFile("Data/RedV4.bin");
            YellowAi.ToFile("Data/YellowV4.bin");
            




            ts = sw.Elapsed; ts = sw.Elapsed;
            sw.Stop();
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            
            Console.WriteLine("Done Saving");
            Console.WriteLine();
        }
    }
}
