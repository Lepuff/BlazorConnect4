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
            


            AIModels.QAgent RedAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/RedV3.bin");
            AIModels.QAgent YellowAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/YellowV3.bin");

            //AIModels.QAgent RedAi = new AIModels.QAgent(Model.CellColor.Red);
            //AIModels.QAgent YellowAi = new AIModels.QAgent(Model.CellColor.Yellow);

            AIModels.RandomAI randomAI = new AIModels.RandomAI();
            //RedAi.WorkoutV2( randomAI, 1000);
            //YellowAi.WorkoutV2(randomAI, 1000);


            for (int i = 0; i < 100; i++)
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

            RedAi.ToFile("Data/RedV3.bin");
            YellowAi.ToFile("Data/YellowV3.bin");



            Console.WriteLine();
        }
    }
}
