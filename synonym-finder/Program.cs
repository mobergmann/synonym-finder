using System;
using System.Text;
using HtmlAgilityPack;

namespace synonym_finder
{
    class Program
    {
        /// <summary>
        /// A set of all symbold in the german alphabet.
        /// </summary>
        // private static char[] germanAlphabet = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','ä','ö','ü'};

        private static readonly string url_get = @"https://www.duden.de/rechtschreibung/";
        private static readonly string url_search = @"https://www.duden.de/suchen/dudenonline/";
        
        static void Main(string[] args)
        {
            ConsoleKeyInfo runAntotherTime;
            do
            {
                // get user input
                Console.Write("Wort: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                string input = Console.ReadLine();
                Console.ResetColor();

                // check if input is valid
                while (!IsValidString(input))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{input}\" ist keine gültige Eingabe. Bitte wiederholen die die Eingabe.");
                    Console.ResetColor();

                    Console.Write("Wort: ");
                    input = Console.ReadLine();
                    Console.WriteLine();
                }

                // feedback from Programm
                Console.WriteLine("Suche...");

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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{input}\" ist nicht im Duden verzeichnet.");
                        Console.ResetColor();

                        goto APP_END;
                    }

                    // inform the user
                    Console.WriteLine($"\"{input}\" wurde nicht gefunden, meinten sie: ");

                    // print the search results with an integer for selection
                    for (int i = 0; i < collection_list.Count; i++)
                    {
                        Console.WriteLine($"{ i }: { ReduceString(collection_list[i].InnerText) }");
                    }
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{collection_list.Count}: Keine Eingabe");
                    Console.ResetColor();

                    string userChoiceNotParsed = String.Empty;
                    int userChoice;

                    Console.Write("Wählen sie einen Eintrag durch seine entsprechende Nummer: ");
                // as long as the user doesn't input a valid number, ask him to do so
                REPEAT:
                    {
                        userChoiceNotParsed = Console.ReadLine();
                        Console.WriteLine();

                        // check if input is a number
                        if (!Int32.TryParse(userChoiceNotParsed, out int number))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Bitte geben sie eine echte Ganzzahlige Zahl an.");
                            Console.ResetColor();
                            
                            goto REPEAT;
                        }

                        userChoice = Int32.Parse(userChoiceNotParsed);

                        // check if Choice is in bounds of available options
                        if (userChoice > collection_list.Count || userChoice < 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Bitte geben sie eine Zahl an, welche auch repräsentiert ist.");
                            Console.ResetColor();

                            goto REPEAT;
                        }
                    }

                    if (userChoice == collection_list.Count)
                    {
                        goto APP_END;
                    }
                    else
                    {
                        // get the html document of the given text
                        doc = GetDoc(url_get + ReduceString(collection_list[userChoice].InnerText));
                    }
                }

                // get all synonyms
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@id='synonyme']/ul/li/a");

                // if word has no synonyms inform the user and stop the programm
                if (collection == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Für \"{input}\" sind keine Synonyme im Duden verzeichnet.");
                    Console.ResetColor();

                    goto APP_END;
                }

                // print all synonyms for the given word
                Console.Write("Folgende Synonyme wurden gefunden: ");
                for (int i = 0; i < collection.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(collection[i].InnerText);
                    Console.ResetColor();
                    if (i < collection.Count - 1)
                    {
                        Console.Write(", ");
                    }
                }
                Console.WriteLine();


            APP_END:
                // ask if the programm sould be closed
                Console.Write("Möchten sie ein weiteres Wort suchen? [Y/n] ");
                runAntotherTime = Console.ReadKey();
                Console.WriteLine("\n");

            }
            while (runAntotherTime.Key.Equals(ConsoleKey.Enter) ||  // if Enter
                !runAntotherTime.Key.Equals(ConsoleKey.N));             // if something else than y
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
                if (isInGermanAlphabet(c))
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
                if (isInAsciiAlphabet(c))
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

        /// <summary>
        /// checks a given char if its a ascii symbol which can be displayd.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool isInAsciiAlphabet(char c)
        {
            if ((c >= 33 && c <= 126) ||    // ascci symbols
                (c >= 128 && c <= 165))     // extended Ascci symbold
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// checks a given char if its in the german alphabet.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool isInGermanAlphabet(char c)
        {
            if ((c >= 65 && c <= 90) ||     // capital letters
                (c >= 97 && c <= 122) ||    // small letters
                c == 228 || c == 196 ||     // ä, Ä
                c == 246 || c == 214 ||     // ö, Ö
                c == 252 || c == 220 ||     // ü, Ü
                c == 223)                   // ß
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}