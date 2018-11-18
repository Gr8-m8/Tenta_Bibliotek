using System;
using System.Collections.Generic;

namespace Tenta_Bibliotek
{
    class Program
    {
        static int Main()
        {
            //Skapar en LibraryManager
            LibraryManager lm = new LibraryManager();

            //Kör huvudfunktionerna för evigt
            while (true)
            {
                //UserLogIn kör för att logga in en användare
                lm.UserLogIn();

                //InLib körs när man valt en avnändare
                lm.InLib();
            }

            return 0;
        }
    }

    //Klassen ska ha koll på alt som berör biblioteket
    class LibraryManager
    {
        //Definerar en BookManager
        BookManager bm = new BookManager();
        //Definerar en UserManager
        UserManager um = new UserManager();

        //Sträng för namnet på biblioteket
        string libraryName = "Offlinebiblioteket";

        public LibraryManager()
        {
            //Sätter titeln på programmet till biblioteksnamnet
            Console.Title = libraryName;
            //Sätter consolens bakgrund- och förgrundsfärg till mina prefererade färger
            ReserColor();
            //Consolen töms och mina färger apliceras
            Console.Clear();
            //Gör att programmet kan hantera: å ä ö
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        //Återställer färger (till mina standarder)
        void ReserColor()
        {
            //Sätter bakgrundsfärgen till grå
            Console.BackgroundColor = ConsoleColor.Gray;
            //Sätter förgrundsfärgen till mörkblå
            Console.ForegroundColor = ConsoleColor.DarkBlue;
        }

        //Logga in användare
        public void UserLogIn()
        {
            //Tömmer konsolen
            Console.Clear();
            //Välkomnar användaren till biblioteket
            Console.WriteLine("Välkommen till " + libraryName + "!");
            Console.WriteLine();
            //Loop som kört t.o.m man valt en användare
            while (um.currentUser == null)
            {
                Console.Write("Användare: ");
                //Tar in input från konsolen: vilken användare
                um.LogIn(Console.ReadLine());
            }
        }

        //När man är i biblioteket
        public void InLib()
        {
            //Loop som körs när man är i biblioteket
            bool inLib = true;
            while (inLib)
            {
                //Tömmer konsolen
                Console.Clear();
                //Skriver vilken användare som är inloggad
                Console.WriteLine("Användare: " + um.currentUser.username);
                Console.WriteLine();
                //Listar böckerna
                bm.ListBooks();
                //Ger alternativ till vad man kan göra
                Console.Write("[S]ök : [B]ok : [L]ogga ut");
                //Om man är admin föreslår den även adminverktyg
                if (um.currentUser.admin)
                {
                    Console.WriteLine(" : [A]dmin Verktyg");
                }
                else
                {
                    Console.WriteLine();
                }

                //Kollar vad användaren vill göra
                switch (Console.ReadKey().KeyChar.ToString().ToUpper())
                {
                    default:
                        Console.WriteLine(" : är ej giltigt input.");
                        break;

                    case "S":
                        //Sök efter bok
                        InLibSearch();
                        break;

                    case "B":
                        //Få information om bok samt låna/lämna
                        bm.GetBook(um);
                        break;

                    case "L":
                        //Logga ut
                        inLib = false;
                        um.LogOut();
                        break;

                    case "A":
                        //Om man är admin
                        if (um.currentUser.admin)
                        {
                            //Admin verktyg
                            inLibAdmin();
                        }
                        break;
                }
            }
        }

        //Söka i biblioteket
        void InLibSearch()
        {
            //Definerar sträng för vad man sökt efter
            string sQuery = "";
            //Loop som körs till och med man sökt färdigt
            bool inLibSearch = true;
            while (inLibSearch)
            {
                //Tömmer konsolen
                Console.Clear();
                //Listar böcker man sökt efter
                bm.ListBooks(sQuery);

                //Skriver ut vad man sökt efter
                Console.WriteLine("Search: " + sQuery);

                //Definerar en knapptrycknig med data
                ConsoleKeyInfo sQueryAdd = Console.ReadKey();
                //Om knapptryckningen var sudda, så tar den bort ett tecken från söksträngen om den är längre än ett tecken långt
                if (sQueryAdd.Key == ConsoleKey.Backspace)
                {
                    if (sQuery.Length > 1)
                    {
                        sQuery = sQuery.Remove(sQuery.Length - 1);
                    }
                    else
                    {
                        sQuery = "";
                    }
                }
                //Om knapptryckningen är enter så avslutas söknigen
                else if (sQueryAdd.Key == ConsoleKey.Enter)
                {
                    inLibSearch = false;
                }
                //Annars läggs knapptryckningen till i strängen
                else
                {
                    sQuery += sQueryAdd.KeyChar.ToString();
                }
            }

            //Ger feedback på att man sökt klart
            Console.WriteLine();
            Console.WriteLine("Confirm Results");
            Console.ReadKey();
        }

        //Adminverktyg
        void inLibAdmin()
        {
            //Loop medans man är i adminverktygs menyn
            bool inLibAdmin = true;
            while (inLibAdmin)
            {
                //Skriver alternativ: De är inte skärskilt användarvänliga när admin antas veta vad dessa alternativ är
                Console.Clear();
                Console.WriteLine("[X]AC : [+]B : [-]B : [A]U : [R]U");
                //Tolkar konsol input
                switch (Console.ReadKey().KeyChar.ToString().ToUpper())
                {
                    default:
                        break;

                    case "X":
                        //Avslutar adminverktygsloopen
                        inLibAdmin = false;
                        break;

                    case "+":
                        //Lägger till bok
                        bm.AddBook();
                        break;

                    case "-":
                        //Tar bort bok
                        bm.RemoveBook();
                        break;

                    case "A":
                        Console.WriteLine("U.N.");
                        //Lägger användare, och om denna användare ska vara admin
                        um.AddUser(Console.ReadLine(), Console.ReadLine());
                        break;

                    case "R":
                        Console.WriteLine("U.N.");
                        //Tar bort användare
                        um.RemoveUser(Console.ReadLine());
                        break;

                }
            }
        }
    }

    //Klassen har koll på alt om böcker
    class BookManager
    {
        //Ger tillgång till .txt filen med böcker
        string dataPath = @"..\..\..\_Books.txt";

        //Dictionaryn är en lista, med nyckel, som i detta fall kommer bli boktiteln
        Dictionary<string, Book> books;

        public BookManager()
        {
            //Importerar böcker från .txt filen
            ImportBooks();
        }

        //Importera böcker från .txt fil
        void ImportBooks()
        {
            books = new Dictionary<string, Book>();
            //Läser av alla rader i .txt bok filen och sätter dem i en lista
            string[] bookImport = System.IO.File.ReadAllLines(dataPath);

            //Loopar igenom listan
            for (int i = 0; i < bookImport.Length; i++)
            {
                //Sätter in böckerna i books dictionaryn och sätter raden som bok id och nyckel till bok
                books.Add(i.ToString(), new Book(bookImport[i].Split("}")[0], bookImport[i].Split("}")[1], bookImport[i].Split("}")[2]));
            }
        }

        //Exportera böcker till .txt fil
        public void ExportBooks()
        {
            //skapar en string lista för antalet böcker
            string[] bookExport = new string[books.Count];
            //foreach loopen ränkar inte antalet gånger den loopat
            int i = 0;
            //Loopar igenom alla böcker för att exportera den till .txt filen
            foreach (KeyValuePair<string, Book> bk in books)
            {
                string authors = "";
                //Loopar igenom bokens alla författare (gäller äver för och efternamn)
                for (int j = 0; j < bk.Value.author.Length; j++)
                {
                    authors += bk.Value.author[j];
                    if (j < bk.Value.author.Length - 1)
                    {
                        //Separerar de olika författarna med {
                        authors += "{";
                    }
                }
                //Element i i bokexportsträngen blir en sträng av boken där de okika datan separeras av }
                bookExport[i] = authors + "}" + bk.Value.title + "}" + bk.Value.inLib;
                i++;
            }

            //.txt filen blir exportsträngen
            System.IO.File.WriteAllLines(dataPath, bookExport);

            //Efter exporten av böckerna till .txt hämtas den nya versionen
            ImportBooks();
        }

        //Lägga till bok
        public void AddBook()
        {
            Console.WriteLine("Author{Author2{Author3{...:");
            //Mata in bokens författare: säparerat av { (gäller även för och efternamn)
            string nbAuthor = Console.ReadLine();
            //Mata in bokens titel
            Console.WriteLine("Title:");
            string nbTitle = Console.ReadLine();
            //Sätter att boken inte ska vara utlånad
            string nbInLib = true.ToString();

            //Lägger till boken i bol dictionaryn
            books.Add(System.IO.File.ReadAllLines(dataPath).Length.ToString(), new Book(nbAuthor, nbTitle, nbInLib));

            //Exporterar ändringar
            ExportBooks();
        }

        //Ta bort bok
        public void RemoveBook()
        {
            Console.WriteLine("Id:");
            //mata in bok id
            string id = Console.ReadLine();
            //Tar bort bok från dictionary
            books.Remove(id);

            //Exporterar ändringar
            ExportBooks();

        }

        //Listar böckerna
        public void ListBooks(string search = null)
        {
            //Kollar om det man sökt efter inte är null
            if (search != null)
            {
                string authorDisplay = "";
                string titleDisplay = "";
                //Loopar igenom alla böcker
                foreach (KeyValuePair<string, Book> bk in books)
                {
                    //Loopar igenom bokens alla författare
                    for (int i = 0; i < bk.Value.author.Length; i++)
                    {
                        //Om författare uppfyller sökkraven
                        if (bk.Value.author[i].ToUpper().StartsWith(search.ToUpper()))
                        {
                            //Strängen för författare fylls med författare
                            authorDisplay += bk.Value.WriteAuthor();
                        }
                    }

                    //Om titel uppfyller sökkraven
                    if (bk.Value.title.ToUpper().StartsWith(search.ToUpper()))
                    {
                        //Strängen för titel fylls med titel
                        titleDisplay += bk.Value.WriteTitle();
                    }
                }

                //Om det är fler än 0 författare
                if (authorDisplay.Length > 0)
                {
                    Console.WriteLine("Författare:");
                    foreach (KeyValuePair<string, Book> bk in books)
                    {
                        //Loopar igenom böcker
                        for (int i = 0; i < bk.Value.author.Length; i++)
                        {
                            if (bk.Value.author[i].ToUpper().StartsWith(search.ToUpper()))
                            {
                                //skriver ut bok
                                Console.WriteLine("F: " + bk.Value.WriteAuthor(true) + "|T: " + bk.Value.WriteTitle() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                            }
                        }
                    }
                    Console.WriteLine();
                }

                //Om antalet titlar > 0
                if (titleDisplay.Length > 0)
                {
                    Console.WriteLine("Titlar:");
                    //Loopar igenom böcker
                    foreach (KeyValuePair<string, Book> bk in books)
                    {
                        if (bk.Value.title.ToUpper().StartsWith(search.ToUpper()))
                        {
                            //Skriver ut bok
                            Console.WriteLine("T: " + bk.Value.WriteTitle(true) + "|F: " + bk.Value.WriteAuthor() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                        }
                    }
                    Console.WriteLine();
                }

                //Om det inte finns varken författare eller titlar som uppfyller sökkraven
                if (authorDisplay.Length + titleDisplay.Length <= 0)
                {
                    //Ger feedback
                    Console.WriteLine("Inga Böcker");
                    Console.WriteLine();
                }
            }
            else
            {
                //Listar titlar för författare och titel
                Console.WriteLine("Författare                Titel");
                foreach (KeyValuePair<string, Book> bk in books)
                {
                    //Listar författare och titel
                    Console.WriteLine("F: " + bk.Value.WriteAuthor() + " |T: " + bk.Value.WriteTitle() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                }
                Console.WriteLine();
            }
        }

        //Visar bok för extra information och låna/lämna
        public void GetBook(UserManager um)
        {
            Console.Clear();
            Console.Write("Bok Id: ");
            //mata bok id
            string id = Console.ReadLine();
            Console.WriteLine();

            //Kollar om bok med bok id finns
            if (books.ContainsKey(id))
            {
                //Listar titel
                Console.WriteLine(books[id].WriteTitle(true));
                Console.WriteLine("av");
                //Listar författare
                Console.WriteLine(books[id].WriteAuthor(true));
                Console.WriteLine();
                //Listar alternativ
                Console.Write("[Enter] Återvänd : ");

                //Kollar om den inloggade användaren har lånat boken med inmatat bok id
                if (um.currentUser.bookId.Contains(id))//Om  användaren lånat boken
                {
                    Console.WriteLine("[L]ämna Bok");

                    if (Console.ReadKey().Key == ConsoleKey.L)
                    {
                        //Tar bort lånade boken från användaren
                        um.currentUser.bookId.Remove(id);
                        //Updaterar användare
                        um.ExportUsers();

                        //Boken går åter att lånas från biblioteket
                        books[id].inLib = true;
                        //Updaterar böcker
                        ExportBooks();
                    }
                }
                else if (!books[id].inLib)//Om boken är lånad av anna användare
                {
                    //Skriver utlånad
                    Console.WriteLine(books[id].WriteStatus());
                    Console.ReadKey();
                }
                else //Om boken inte är lånad
                {
                    Console.WriteLine("[L]åna Bok");
                    if (Console.ReadKey().Key == ConsoleKey.L)
                    {
                        //Lånar bok till användaren
                        um.currentUser.bookId.Add(id);
                        //Updaterar användare
                        um.ExportUsers();

                        //Går ej att låna bok
                        books[id].inLib = false;
                        //Updaterar böcker
                        ExportBooks();
                    }
                }
            }
            else
            {
                //Ger feedback om bok med inmatade bok id inte finns
                Console.WriteLine("Bok med detta id finns ej.");
                Console.ReadKey();
            }
        }
    }

    //Klass om böcker
    class Book
    {
        public string title;
        public string[] author;
        public string genre;
        public bool inLib;

        public Book(string authorSet, string titleSet, string inLibSet)
        {
            //Sätter antalet författare
            author = new string[authorSet.Split("{").Length];
            //Loopar författare
            for (int i = 0; i < author.Length; i++)
            {
                //Sätter författare
                author[i] = authorSet.Split("{")[i];
            }
            //Sätter titel
            title = titleSet;
            //Sätter status om utlånad eller ej
            inLib = bool.Parse(inLibSet);
        }

        //Skriver ut författare
        public string WriteAuthor(bool writeFull = false)
        {
            string outAuthor = "";
            for (int i = 0; i < author.Length; i++)
            {
                //Lägger till förfatare för att skriva ut
                outAuthor += author[i];

                if (i < author.Length - 1)
                {
                    //Om farfattaren inte är den sista seppareras den med ,
                    outAuthor += ", ";
                }
            }

            int wrtLng = 20;
            //Om man inte vill skriva för långt (snyggare formatering)
            if (!writeFull)
            {
                //Om författarna är för långt
                if (outAuthor.Length > wrtLng)
                {
                    //Kapar ut strängen och lägger på ...
                    outAuthor = outAuthor.Remove(wrtLng - 3);
                    outAuthor += "...";
                }
            }

            //Om författarna längd är för kort
            if (outAuthor.Length < wrtLng)
            {
                //Loopar igenom till och med det blir tillräckligt långt
                for (int i = wrtLng - outAuthor.Length; i > 0; i--)
                {
                    //Lägger till " "
                    outAuthor += " ";
                }
            }

            //Retunerar författarna
            return outAuthor + " ";
        }

        //Skriver ut titel
        public string WriteTitle(bool writeFull = false)
        {
            string outTitle = title;

            int wrtLng = 20;
            //Om man inte vill skriva hela titel
            if (!writeFull)
            {
                if (outTitle.Length > wrtLng)
                {
                    //Kapar titeln om den är för lång
                    outTitle = outTitle.Remove(wrtLng - 3);
                    outTitle += "...";
                }
            }

            //Om titenl är för kort
            if (outTitle.Length < wrtLng)
            {
                for (int i = wrtLng - outTitle.Length; i > 0; i--)
                {
                    outTitle += " ";
                }
            }

            //Returnerar titel
            return outTitle + " ";
        }

        //Skriver ut status
        public string WriteStatus()
        {
            //Om boken är utlånad
            if (!inLib)
            {
                //returnerar status
                return "[UTLÅNAD]";
            }

            //returnerar status
            return "";
        }
    }

    //Klass för att hålla koll på användare
    class UserManager
    {
        //Fil för användare
        string dataPath = @"..\..\..\_Users.txt";

        Dictionary<string, User> users;
        public User currentUser = null;

        public UserManager()
        {
            //Importera användare från .txt
            ImportUsers();
        }

        //Importera användare från .txt
        void ImportUsers()
        {
            users = new Dictionary<string, User>();
            //Hämtar alla användare från .txt till sträng
            string[] userImport = System.IO.File.ReadAllLines(dataPath);

            for (int i = 0; i < userImport.Length; i++)
            {
                //Lägger till användare i dicionaryn med användarnamn som nyckel
                users.Add(userImport[i].Split("}")[0], new User(userImport[i].Split("}")[0], userImport[i].Split("}")[1], userImport[i].Split("}")[2]));
            }
        }

        //Exportera användare till.txt
        public void ExportUsers()
        {
            //Skapar en exportsträng för användare
            string[] userExport = new string[users.Count];
            int i = 0;
            foreach (KeyValuePair<string, User> us in users)
            {
                userExport[i] = us.Value.username + "}" + us.Value.admin + "}" + us.Value.bookListString();
                i++;
            }

            //Exporterar användare till .txt
            System.IO.File.WriteAllLines(dataPath, userExport);
            //Uppdaterar användare
            ImportUsers();
        }

        //Logga ut användare
        public void LogOut()
        {
            //Inloggade användaren blir null
            currentUser = null;
        }

        //Logga in användare
        public void LogIn(string username)
        {
            //Kollar om användaren finns
            if (users.ContainsKey(username))
            {
                //Sätter användaren som inloggad användare
                currentUser = users[username];
            } else
            {
                //Ger feedback på att användaren inte finns
                Console.WriteLine("Användaren " + username + " finns inte.");
                Console.WriteLine("Vill du skapa en ny användare? [J]a : [N]ej");

                //ger möjlighet att skapa användaren som inte finns
                if (Console.ReadKey().KeyChar.ToString().ToUpper() == "J"){
                    //Lägger till användare
                    AddUser(username);
                    //Loggar in användare
                    currentUser = users[username];
                }
            }
        }

        //Lägga till användare
        public void AddUser(string username, string admin = "False")
        {
            //Om man inte sägger att admin = true.ToString() blir den falsk
            if (admin != "True")
            {
                admin = "False";
            }

            //Lägger till användare i dicionary
            users.Add(username, new User(username, admin, ""));

            //Uppdaterar användare
            ExportUsers();
        }

        //Ta bort användare
        public void RemoveUser(string username)
        {
            //Tar bort användare från dicionary
            users.Remove(username);

            //Uppdaterar användare
            ExportUsers();
        }
    }

    //Klass för användarnas funktion
    class User
    {
        public User(string usernameSet, string adminSet, string borrowedBooksImport)
        {
            username = usernameSet;
            admin = bool.Parse(adminSet);
            for (int i = 0; i < borrowedBooksImport.Split("{").Length; i++)
            {
                bookId.Add(borrowedBooksImport.Split("{")[i]);
            }
        }

        public string username = "";
        public bool admin = false;
        public List<string> bookId = new List<string>();

        //Listar användarens lånade böcker bok id
        public string bookListString()
        {
            string outBook = "";
            foreach (string s in bookId)
            {
                if (s != "")
                {
                    outBook += s + "{";
                }
            }

            return outBook;
        }
    }
}
