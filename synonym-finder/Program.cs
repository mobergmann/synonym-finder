using System;
using System.Text;
using HtmlAgilityPack;

namespace synonym_finder
{
    class Program
    {
        private static readonly string url_get = @"https://www.duden.de/rechtschreibung/";
        private static readonly string url_search = @"https://www.duden.de/suchen/dudenonline/";

        static void Main(string[] args)
        {
            // get user input
            Console.WriteLine("Wort: ");
            string input = Console.ReadLine();

            // check if input is valid
            while (!IsValidString(input))
            {
                Console.WriteLine("Ihre eingabe ist nicht valide. Bitte machen sie eine neue eingabe.");
                Console.WriteLine("Wort: ");
                input = Console.ReadLine();
            }

            // feedback from Programm
            Console.WriteLine("Suche...\n");

            // get html document
            HtmlDocument doc = GetDoc(url_get + input);

            // if no word found, similar words might be found.
            if (Is404(doc))
            {
                // make a duden search
                doc = GetDoc(url_search + input);

                // get all search results
                HtmlNodeCollection collection_list = doc.DocumentNode.SelectNodes("//section[@class='vignette']/h2/a/strong");

                // if no results were found stop the programm
                if (collection_list == null)
                {
                    Console.WriteLine("Ihr Wort exisitiert nicht im Duden.");

                    // ask if the programm sould be closed
                    Console.WriteLine("Drücken sie eine beliebige Taste um das Programm zu beenden.");
                    Console.ReadKey();
                    return;
                }

                // inform the user
                Console.WriteLine("Ihr angegebenes Wort wurde nicht gefunden.");
                Console.WriteLine("Meinten sie:");

                // print the search results with an integer for selection
                for (int i = 0; i < collection_list.Count; i++)
                {
                    Console.WriteLine($"{ i }: { ReduceString(collection_list[i].InnerText) }");
                }

                string userChoiceNotParsed = String.Empty;
                int userChoice;

                // as long as the user doesn't input a valid number, ask him to do so
                REPEAT: {
                    Console.Write("Wälen sie einen Eintrag durch seine entsprechende Nummer: ");
                    userChoiceNotParsed = Console.ReadLine();
                    Console.WriteLine();

                    // check if input is a number
                    if (!Int32.TryParse(userChoiceNotParsed, out int number))
                    {
                        Console.WriteLine("Bitte geben sie eine echte Ganzzahlige Zahl an.");
                        goto REPEAT;
                    }

                    userChoice = Int32.Parse(userChoiceNotParsed);
                
                    // check if Choice is in bounds of available options
                    if (userChoice > collection_list.Count || userChoice < 0)
                    {
                        Console.WriteLine("Bitte geben sie eine Zahl an, welche auch repräsentiert ist.");
                        goto REPEAT;
                    }
                }

                // get the html document of the given text
                doc = GetDoc(url_get + ReduceString(collection_list[userChoice].InnerText));
            }

            // get all synonyms
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@id='synonyme']/ul/li/a");

            // if word has no synonyms inform the user and stop the programm
            if (collection == null)
            {
                Console.WriteLine("Keine Synonyme gefunden.");

                // ask if the programm sould be closed
                Console.WriteLine("Drücken sie eine beliebige Taste um das Programm zu beenden.");
                Console.ReadKey();
                return;
            }

            // print all synonyms for the given word
            Console.WriteLine("Folgende Synonyme wurden gefunden:");
            for (int i = 0; i < collection.Count; i++)
            {
                Console.Write(collection[i].InnerText);
                if (i < collection.Count - 1)
                {
                    Console.Write(", ");
                }
            }
            Console.WriteLine();

            // ask if the programm sould be closed
            Console.WriteLine("Drücken sie eine beliebige Taste um das Programm zu beenden.");
            Console.ReadKey();
        }

        /// <summary>
        /// returns the html document of the given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static HtmlDocument GetDoc(string url)
        {
            // From Web
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            if (doc == null)
            {
                Console.WriteLine("Duden konnte nicht erreicht werden. Bitte überprüfen sie ihre Internetverbindung.");

                // ask if the programm sould be closed
                Console.WriteLine("Drücken sie eine beliebige Taste um das Programm zu beenden.");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            return doc;
        }

        /// <summary>
        /// check if no search results were found by checking the title
        /// </summary>
        /// <param name="doc">the html document</param>
        /// <returns></returns>
        private static bool Is404(HtmlDocument doc)
        {
            var node = doc.DocumentNode.SelectSingleNode("//head/title");
            string title = node.InnerText;
            if (title.Equals("Die Seite wurde nicht gefunden"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all characters, which are not in the german alphabet/ not easely readable 
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>a new string, without characters, whixh are not conole friendly.</returns>
        public static string ReduceString(string input)
        {
            StringBuilder builder = new StringBuilder();

            foreach (char c in input)
            {
                if ((c >= 65 && c <= 90) || // capital letters 
                    (c >= 97 && c <= 122) || // small letters
                    (c >= 128 && c <= 165) || c == 225) // german extra symbols
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// checks each characgter of the given string, if it is in the given bounds.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsValidString(string input)
        {
            foreach (char c in input)
            {
                if ((c >= 65 && c <= 90) || // capital letters 
                    (c >= 97 && c <= 122) || // small letters
                    (c >= 128 && c <= 165) || c == 225) // german extra symbols
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}