using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace synonym_finder
{

    /// <summary>
    /// A set of all symbold in the german alphabet.
    /// </summary>
    struct SearchResult
    {
        public string word;
        public string link;
    }

    class Program
    {
        /// <summary>
        /// A set of all symbold in the german alphabet.
        /// </summary>
        private static char[] germanAlphabet = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','ä','ö','ü'};

        /// <summary>
        /// The base URL, which opens a page of a appended word.
        /// </summary>
        private static readonly string url_base = @"https://www.duden.de";        

        /// <summary>
        /// The base URL, which opens a page of a appended word.
        /// </summary>
        private static readonly string url_get = @"https://www.duden.de/rechtschreibung/";
        
        /// <summary>
        /// The base URL, which searches for a appended word.
        /// </summary>
        private static readonly string url_search = @"https://www.duden.de/suchen/dudenonline/";
        
        static void Main(string[] args)
        {
            do
            {
                #region User Input

                Console.Write("Geben sie ein Word ein (");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("esc");
                Console.ResetColor();
                Console.Write(" zum beenden): ");

                string input = string.Empty;

                // get the first pressed key
                Console.ForegroundColor = ConsoleColor.Cyan;
                ConsoleKeyInfo key = Console.ReadKey();
                Console.ResetColor();

                // when esc key has been pressed, then end the programm
                if (key.Key == ConsoleKey.Escape)
                {
                    return;
                }
                // if key wasn't a german key, then repeat process
                else if (!isInGermanAlphabet(key.KeyChar))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{input}\" ist keine gültige Eingabe. Bitte wiederholen die die Eingabe.");
                    Console.ResetColor();
                    continue;
                }
                // a valid key has been pressed, so add the pressed key to the input string
                else
                {
                    input += key.KeyChar;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                input += Console.ReadLine();
                Console.ResetColor();

                if (!IsValidInput(input))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{input}\" ist keine gültige Eingabe. Bitte wiederholen die die Eingabe."); 
                    Console.ResetColor();
                    continue;
                }

                #endregion

                // feedback from Programm
                Console.WriteLine("Suche...");

                // get html document
                HtmlDocument doc = GetDoc(url_get + input);

                // if no word found, similar words might be found.
                if (Is404(doc))
                {
                    // make a duden search
                    doc = GetDoc(url_search + input);

                    #region handle search results

                    // holds all search results
                    List<SearchResult> searchResults = new List<SearchResult>();

                    // get all search results
                    // HtmlNodeCollection collection_list = doc.DocumentNode.SelectNodes("//section[@class='vignette']/h2/a/strong");

                    HtmlNodeCollection tmp_collection_1 = doc.DocumentNode.SelectNodes("//section[@class='vignette']/h2/a");

                    // if no results were found ask for new word
                    if (tmp_collection_1 == null)
                    {
                        // inform user
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{input}\" ist nicht im Duden verzeichnet.");
                        Console.ResetColor();

                        // print an empty Line
                        Console.WriteLine();
                        continue;
                    }
                    
                    // extract search results
                    foreach (HtmlNode node in tmp_collection_1)
                    {
                        SearchResult result = new SearchResult
                        {
                            word = node.ChildNodes[1].InnerText, // get second child of the a tag (which is the <strong>), which holds the name of the word
                            link = url_base + node.GetAttributeValue("href", string.Empty)
                        };

                        searchResults.Add(result);
                    }

                    #endregion

                    #region print search results

                    // inform the user
                    Console.WriteLine($"\"{input}\" wurde nicht gefunden, meinten sie: ");

                    // print the search results with an integer for selection
                    for (int i = 0; i < searchResults.Count; i++)
                    {
                        Console.WriteLine($"{ i }: { ReduceString(searchResults[i].word) }");
                    }

                    // choose no word
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{tmp_collection_1.Count}: Keine Eingabe");
                    Console.ResetColor();

                    #endregion

                    #region get user choice

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
                        if (userChoice > tmp_collection_1.Count || userChoice < 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Bitte geben sie eine Zahl an, welche auch repräsentiert ist.");
                            Console.ResetColor();

                            goto REPEAT;
                        }
                    }

                    if (userChoice == tmp_collection_1.Count)
                    {
                        // print an empty Line
                        Console.WriteLine();
                        continue;
                    }
                    else
                    {
                        // get the html document of the given text
                        doc = GetDoc(url_get + ReduceString(tmp_collection_1[userChoice].InnerText));
                    }

                    #endregion
                }

                // get all synonyms
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@id='synonyme']/ul/li/a");

                // if word has no synonyms inform the user and stop the programm
                if (collection == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Für \"{input}\" sind keine Synonyme im Duden verzeichnet.");
                    Console.ResetColor();

                    // print an empty Line
                    Console.WriteLine();
                    continue;
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

                // print an empty Line
                Console.WriteLine();
                Console.WriteLine();
                continue;
            }
            while (true);             // if something else than y
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
        /// checks if each characgter of the given string, is in a defined bound.
        /// </summary>
        /// <param name="input">the string which should be checked</param>
        /// <returns></returns>
        private static bool IsValidInput(string input)
        {
            // check string length
            if (input.Length <= 0)
            {
                return false;
            }

            // check each character of the given Word
            foreach (char c in input)
            {
                if (isInGermanAlphabet(c))
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
                c == 'ä' || c == 'Ä' ||     // ä, Ä
                c == 'ö' || c == 'Ö' ||     // ö, Ö
                c == 'ü' || c == 'Ü' ||     // ü, Ü
                c == 'ß')                   // ß
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