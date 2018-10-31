using System;
using System.Collections.Generic;

namespace Tenta_Bibliotek
{
    class Program
    {
        static int Main()
        {
            LibraryManager lm = new LibraryManager();

            while (true)
            {
                lm.UserLogIn();
                lm.InLib();
            }

            return 0;
        }
    }

    class LibraryManager
    {
        BookManager bm = new BookManager();
        UserManager um = new UserManager();

        string libraryName = "Offlinebiblioteket";

        public LibraryManager()
        {
            InitLibrary();
        }

        void InitLibrary()
        {
            Console.Title = libraryName;
            ReserColor();
            Console.Clear();
            //Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        void ReserColor()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
        }

        public void UserLogIn()
        {
            Console.Clear();
            Console.WriteLine("Välkommen till " + libraryName + "!");
            Console.WriteLine();
            while (um.currentUser == null)
            {
                Console.Write("Användare: ");
                um.LogIn(Console.ReadLine());
            }
        }

        public void InLib()
        {
            bool inLib = true;
            while (inLib)
            {
                Console.Clear();
                Console.WriteLine("Användare: " + um.currentUser.username);
                Console.WriteLine();
                bm.ListBooks();
                Console.Write("[S]ök : [B]ok : [L]ogga ut");
                if (um.currentUser.admin)
                {
                    Console.WriteLine(" : [A]dmin Verktyg");
                }
                else
                {
                    Console.WriteLine();
                }
                switch (Console.ReadKey().KeyChar.ToString().ToUpper())
                {
                    default:
                        Console.WriteLine(" : är ej giltigt input.");
                        break;

                    case "S":
                        InLibSearch();
                        break;

                    case "B":
                        bm.GetBook(um);
                        break;

                    case "L":
                        inLib = false;
                        um.LogOut();
                        break;

                    case "A":
                        if (um.currentUser.admin)
                        {
                            inLibAdmin();
                        }
                        break;
                }
            }
        }

        void InLibSearch()
        {
            string sQuery = "";
            bool inLibSearch = true;
            while (inLibSearch)
            {
                Console.Clear();
                bm.ListBooks(sQuery);

                Console.WriteLine("Search: " + sQuery);

                ConsoleKeyInfo sQueryAdd = Console.ReadKey();
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
                else if (sQueryAdd.Key == ConsoleKey.Enter)
                {
                    inLibSearch = false;
                }
                else
                {
                    sQuery += sQueryAdd.KeyChar.ToString();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Confirm Results");
            Console.ReadKey();
        }

        void inLibAdmin()
        {
            bool inLibAdmin = true;
            while (inLibAdmin)
            {
                Console.Clear();
                Console.WriteLine("[X]AC : [+]B : [-]B : [A]U : [R]U");
                switch (Console.ReadKey().KeyChar.ToString().ToUpper())
                {
                    default:
                        break;

                    case "X":
                        inLibAdmin = false;
                        break;

                    case "+":
                        bm.AddBook();
                        break;

                    case "-":
                        bm.RemoveBook();
                        break;

                    case "A":
                        Console.WriteLine("U.N.");
                        um.AddUser(Console.ReadLine(), Console.ReadLine());
                        break;

                    case "R":
                        Console.WriteLine("U.N.");
                        um.RemoveUser(Console.ReadLine());
                        break;

                }
            }
        }
    }

    class BookManager
    {
        string dataPath = @"..\..\..\_Books.txt";

        Dictionary<string, Book> books;

        public BookManager()
        {
            ImportBooks();
        }

        void ImportBooks()
        {
            books = new Dictionary<string, Book>();
            string[] bookImport = System.IO.File.ReadAllLines(dataPath);

            for (int i = 0; i < bookImport.Length; i++)
            {
                books.Add(i.ToString(), new Book(bookImport[i].Split("}")[0], bookImport[i].Split("}")[1], bookImport[i].Split("}")[2]));
            }
        }

        public void ExportBooks()
        {
            string[] bookExport = new string[books.Count];
            int i = 0;
            foreach (KeyValuePair<string, Book> bk in books)
            {
                string authors = "";
                for (int j = 0; j < bk.Value.author.Length; j++)
                {
                    authors += bk.Value.author[j];
                    if (j < bk.Value.author.Length - 1)
                    {
                        authors += "{";
                    }
                }
                bookExport[i] = authors + "}" + bk.Value.title + "}" + bk.Value.inLib;
                i++;
            }

            System.IO.File.WriteAllLines(dataPath, bookExport);
            ImportBooks();
        }

        public void AddBook()
        {
            Console.WriteLine("Author{Author2{Author3{...:");
            string nbAuthor = Console.ReadLine();
            Console.WriteLine("Title:");
            string nbTitle = Console.ReadLine();
            string nbInLib = true.ToString();

            books.Add(System.IO.File.ReadAllLines(dataPath).Length.ToString(), new Book(nbAuthor, nbTitle, nbInLib));

            ExportBooks();
        }

        public void RemoveBook()
        {
            Console.WriteLine("Id:");
            string id = Console.ReadLine();
            books.Remove(id);

            ExportBooks();

        }

        public void ListBooks(string search = null)
        {
            if (search != null)
            {
                string authorDisplay = "";
                string titleDisplay = "";
                foreach (KeyValuePair<string, Book> bk in books)
                {
                    for (int i = 0; i < bk.Value.author.Length; i++)
                    {
                        if (bk.Value.author[i].ToUpper().StartsWith(search.ToUpper()))
                        {
                            authorDisplay += bk.Value.WriteAuthor();
                        }
                    }

                    if (bk.Value.title.ToUpper().StartsWith(search.ToUpper()))
                    {
                        titleDisplay += bk.Value.WriteTitle();
                    }
                }

                if (authorDisplay.Length > 0)
                {
                    Console.WriteLine("Författare:");
                    foreach (KeyValuePair<string, Book> bk in books)
                    {
                        for (int i = 0; i < bk.Value.author.Length; i++)
                        {
                            if (bk.Value.author[i].ToUpper().StartsWith(search.ToUpper()))
                            {
                                Console.WriteLine("F: " + bk.Value.WriteAuthor(true) + "|T: " + bk.Value.WriteTitle() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                            }
                        }
                    }
                    Console.WriteLine();
                }

                if (titleDisplay.Length > 0)
                {
                    Console.WriteLine("Titlar:");
                    foreach (KeyValuePair<string, Book> bk in books)
                    {
                        if (bk.Value.title.ToUpper().StartsWith(search.ToUpper()))
                        {
                            Console.WriteLine("T: " + bk.Value.WriteTitle(true) + "|F: " + bk.Value.WriteAuthor() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                        }
                    }
                    Console.WriteLine();
                }

                if (authorDisplay.Length + titleDisplay.Length <= 0)
                {
                    Console.WriteLine("Inga Böcker");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Författare                Titel");
                foreach (KeyValuePair<string, Book> bk in books)
                {
                    Console.WriteLine("F: " + bk.Value.WriteAuthor() + " |T: " + bk.Value.WriteTitle() + " |Id: " + bk.Key + " |" + bk.Value.WriteStatus());
                }
                Console.WriteLine();
            }
        }

        public void GetBook(UserManager um)
        {
            Console.Clear();
            Console.Write("Bok Id: ");
            string id = Console.ReadLine();
            Console.WriteLine();
            if (books.ContainsKey(id))
            {
                Console.WriteLine(books[id].WriteTitle(true));
                Console.WriteLine("av");
                Console.WriteLine(books[id].WriteAuthor(true));
                Console.WriteLine();
                Console.Write("[Enter] Återvänd : ");

                if (um.currentUser.bookId.Contains(id))
                {
                    Console.WriteLine("[L]ämna Bok");

                    if (Console.ReadKey().Key == ConsoleKey.L)
                    {
                        um.currentUser.bookId.Remove(id);
                        um.ExportUsers();

                        books[id].inLib = true;
                        ExportBooks();
                    }
                }
                else if (!books[id].inLib)
                {
                    Console.WriteLine(books[id].WriteStatus());
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("[L]åna Bok");
                    if (Console.ReadKey().Key == ConsoleKey.L)
                    {
                        um.currentUser.bookId.Add(id);
                        um.ExportUsers();

                        books[id].inLib = false;
                        ExportBooks();
                    }
                }
            }
            else
            {
                Console.WriteLine("Bok med detta id finns ej.");
                Console.ReadKey();
            }
        }
    }

    class Book
    {
        public string title;
        public string[] author;
        public string genre;
        public bool inLib;

        public Book(string authorSet, string titleSet, string inLibSet)
        {
            author = new string[authorSet.Split("{").Length];
            for (int i = 0; i < author.Length; i++)
            {
                author[i] = authorSet.Split("{")[i];
            }
            title = titleSet;
            inLib = bool.Parse(inLibSet);
        }

        public string WriteAuthor(bool writeFull = false)
        {
            string outAuthor = "";
            for (int i = 0; i < author.Length; i++)
            {
                outAuthor += author[i];

                if (i < author.Length - 1)
                {
                    outAuthor += ", ";
                }
            }

            if (!writeFull)
            {
                if (outAuthor.Length > 20)
                {
                    outAuthor = outAuthor.Remove(17);
                    outAuthor += "...";
                }
            }

            if (outAuthor.Length < 20)
            {
                for (int i = 20 - outAuthor.Length; i > 0; i--)
                {
                    outAuthor += " ";
                }
            }

            return outAuthor + " ";
        }

        public string WriteTitle(bool writeFull = false)
        {
            string outTitle = title;

            if (!writeFull)
            {
                if (outTitle.Length > 20)
                {
                    outTitle = outTitle.Remove(17);
                    outTitle += "...";
                }
            }

            if (outTitle.Length < 20)
            {
                for (int i = 20 - outTitle.Length; i > 0; i--)
                {
                    outTitle += " ";
                }
            }

            return outTitle + " ";
        }

        public string WriteStatus()
        {
            if (!inLib)
            {
                return "[UTLÅNAD]";
            }

            return "";
        }
    }

    class UserManager
    {
        string dataPath = @"..\..\..\_Users.txt";

        Dictionary<string, User> users;
        public User currentUser = null;

        public UserManager()
        {
            ImportUsers();
        }

        void ImportUsers()
        {
            users = new Dictionary<string, User>();
            string[] userImport = System.IO.File.ReadAllLines(dataPath);

            for (int i = 0; i < userImport.Length; i++)
            {
                users.Add(userImport[i].Split("}")[0], new User(userImport[i].Split("}")[0], userImport[i].Split("}")[1], userImport[i].Split("}")[2]));
            }
        }

        public void ExportUsers()
        {
            string[] userExport = new string[users.Count];
            int i = 0;
            foreach (KeyValuePair<string, User> us in users)
            {
                userExport[i] = us.Value.username + "}" + us.Value.admin + "}" + us.Value.bookListString();
                i++;
            }

            System.IO.File.WriteAllLines(dataPath, userExport);
            ImportUsers();
        }

        public void LogOut()
        {
            currentUser = null;
        }

        public void LogIn(string username)
        {
            if (users.ContainsKey(username))
            {
                currentUser = users[username];
            } else
            {
                Console.WriteLine("Användaren " + username + " finns inte.");
                Console.WriteLine("Vill du skapa en ny användare? [J]a : [N]ej");

                if (Console.ReadKey().KeyChar.ToString().ToUpper() == "J"){
                    AddUser(username);
                    currentUser = users[username];
                }
            }
        }

        public void AddUser(string username, string admin = "False")
        {
            if (admin != "True")
            {
                admin = "False";
            }

            users.Add(username, new User(username, admin, ""));

            ExportUsers();
        }

        public void RemoveUser(string username)
        {
            users.Remove(username);

            ExportUsers();
        }
    }

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
