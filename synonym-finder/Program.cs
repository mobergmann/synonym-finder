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

                // as long as the user doesn't input a valid number, ask him to do so
                string userChoiceNotParsed = String.Empty;
                while (!Int32.TryParse(userChoiceNotParsed, out int number))
                {
                    Console.Write("Wälen sie einen Eintrag durch seine entsprechende Nummer: ");
                    userChoiceNotParsed = Console.ReadLine();
                    Console.WriteLine();
                }

                // parse the user input
                int userChoice = Int32.Parse(userChoiceNotParsed);

                // get the html document of the given text
                doc = GetDoc(url_get + ReduceString(collection_list[userChoice].InnerText));
            }

            // get all synonyms
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@id='synonyme']/ul/li/a");

            // if word has no synonyms inform the user and stop the programm
            if (collection == null)
            {
                Console.WriteLine("Keine Synonyme gefunden.");
                return;
            }

            // print all synonyms for the given word
            Console.WriteLine("Folgende Synonyme wurden gefunden:");
            foreach (HtmlNode node in collection)
            {
                Console.Write(node.InnerText + ", ");
            }

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