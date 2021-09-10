using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WiredBrainCoffeeSurveys.Reports
{
    internal static class Program
    {
        static void Main()
        {
            bool quitApp = false;

            do
            {
                Console.WriteLine("Please specify a report to run (rewards, comments, tasks, quit): ");
                var selectedReport = Console.ReadLine();

                Console.WriteLine();

                if (selectedReport == "quit")
                {
                    quitApp = true;
                }
                else
                {
                    Console.WriteLine("Please specify which quarter of data: (q1, q2)");
                    var selectedData = Console.ReadLine();

                    var surveyResults = JsonConvert.DeserializeObject<SurveyResults>
                        (File.ReadAllText($"Data/{selectedData}.json"));

                    switch (selectedReport)
                    {
                        case "rewards":
                            GenerateWinnerEmails(surveyResults);
                            break;
                        case "comments":
                            GenerateCommentsReport(surveyResults);
                            break;
                        case "tasks":
                            GenerateTasksReport(surveyResults);
                            break;
                        default:
                            Console.WriteLine("Sorry, that's not a valid option.");
                            break;
                    }
                }              
                                
                Console.WriteLine();

            } while (!quitApp);                          
        }

        private static void GenerateWinnerEmails(SurveyResults results)
        {
            var selectedEmails = new List<string>();
            int counter = 0;

            Console.WriteLine(Environment.NewLine + "Selected Winners Output:");
            while (selectedEmails.Count < 2 && counter < results.Responses.Count)
            {
                var currentItem = results.Responses[counter];

                if (currentItem.FavoriteProduct == results.FavoriteProduct)
                {
                    selectedEmails.Add(currentItem.EmailAddress);
                    Console.WriteLine(currentItem.EmailAddress);
                }

                counter++;
            }

            File.WriteAllLines("WinnersReport.csv", selectedEmails);
        }

        private static void GenerateCommentsReport(SurveyResults results)
        {
            var comments = new List<string>();

            Console.WriteLine(Environment.NewLine + "Comments Output:");
            
            foreach (var currentResponse in results.Responses.Where(currentResponse => currentResponse.WouldRecommend < 7.0))
            {
                Console.WriteLine(currentResponse.Comments);
                comments.Add(currentResponse.Comments);
            }

            foreach (var response in results.Responses)
            {
                if (response.AreaToImprove == results.AreaToImprove)
                {
                    Console.WriteLine(response.Comments);
                    comments.Add(response.Comments);
                }
            }

            File.WriteAllLines("CommentsReport.csv", comments);
        }

        private static void GenerateTasksReport(SurveyResults results)
        {
            var tasks = new List<string>();

            double responseRate = results.NumberResponded / results.NumberSurveyed;
            double overallScore = (results.ServiceScore + results.CoffeeScore + results.FoodScore + results.PriceScore) / 4;

            if (results.CoffeeScore < results.FoodScore)
            {
                tasks.Add("Investigate coffee recipes and ingredients.");
            }

            tasks.Add(overallScore > 8.0
                ? "Work with leadership to reward staff"
                : "Work with employees for improvement ideas.");

            if (responseRate < 0.33)
            {
                tasks.Add("Research options to improve response rate.");
            }
            else if (responseRate > 0.33 && responseRate < 0.66)
            {
                tasks.Add("Reward participants with free coffee coupon.");
            }
            else
            {
                tasks.Add("Rewards participants with discount coffee coupon.");
            }

            switch (results.AreaToImprove)
            {
                case "RewardsProgram":
                    tasks.Add("Revisit the rewards deals.");
                    break;
                case "Cleanliness":
                    tasks.Add("Contact the cleaning vendor.");
                    break;
                case "MobileApp":
                    tasks.Add("Contact the consulting firm about app.");
                    break;
                default:
                    tasks.Add("Investigate individual comments for ideas.");
                    break;
            }

            Console.WriteLine(Environment.NewLine + "Tasks Output:");
            foreach (var task in tasks)
            {
                Console.WriteLine(task);
            }

            File.WriteAllLines("TasksReport.csv", tasks);
        }
    }
}
