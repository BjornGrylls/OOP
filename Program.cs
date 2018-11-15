using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;


namespace DONDE_ESTA_LA_BIBLIOTECA {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to my Library editor. Start typing a command and press enter after each. Type help to show commands");
            string theInput;
            string theCommand;
            Action action = new Action();
            Method method = new Method();
            List<Book> books = new List<Book>();
            bool isRunning = true;

            List<string> bookList = new List<string>();
            bookList = method.BookToStringList(books);

            string path = ".\\test.txt"; // Sets the path for the save file
            if (!File.Exists(path)) { // Create file with standard names
                File.WriteAllLines(path, bookList.ToArray());
            }

            while (isRunning) {

                // Input things
                Console.WriteLine();
                Console.Write(":{0}> ", path.TrimStart('.'));
                theInput = Console.ReadLine(); // Wait for input
                theCommand = theInput.Split(' ')[0].Trim().ToLower(); // Isolates the command
                theInput = theInput.Substring(theCommand.Length).Trim(); // removes the command

                // Convert chosen file to list of persons
                bookList = File.ReadAllLines(path).ToList();
                books = new List<Book>();
                for (int i = 0; i < bookList.Count; i += 1) {
                    string[] splittet = bookList[i].Split('/');
                    try {
                        if (!bool.Parse(splittet[1])) {
                            books.Add(new Book(splittet[0]));
                        } else {
                            books.Add(new Book(splittet[0], bool.Parse(splittet[1]), DateTime.Parse(splittet[2]), int.Parse(splittet[3])));
                        }
                    } catch (Exception) {
                        Console.WriteLine("Error on line " + i + "\n Contact system manager for assistance");
                    }
                }


                // Switch to determine and run command
                try {
                    switch (theCommand) {
                        case "showall":
                            method.PrintList(books);
                            break;
                        case "addbook":
                            action.AddBookToLibrary(books, theInput, path);
                            break;
                        case "lendbook":
                            action.LendBook(books, theInput, path);
                            break;
                        case "checkbook":
                            action.CheckTimeLeft(books, theInput);
                            break;
                        case "prolongbook":
                            action.ProlongBook(books, theInput, path);
                            break;
                        case "returnbook":
                            action.Return(books, theInput, path);
                            break;
                        case "help":
                            method.ShowHelp();
                            break;
                        case "exit":
                            isRunning = false;
                            break;
                        case "cls":
                            Console.Clear();
                            Console.WriteLine("Welcome to my Library editor. Start typing a command and press enter after each. Type help to show commands");
                            break;
                        default:
                            Console.WriteLine("Command not found or syntax wrong. Try removing any spaces in the end of command. Type help to see syntax");
                            break;
                    }
                } catch (FormatException) {
                    Console.WriteLine("You have written in wrong format. Type help to see syntax");
                } catch (Exception ex) {
                    if (ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException) {
                        Console.WriteLine("You are missing a name, age or balance in the command. Type help to see syntax");
                    } else {
                        Console.WriteLine("Unknown error. Type help to see syntax \n\nFor debugging the error is: \n" + ex);
                    }

                }

            } // While running

        } // Main

    } // class Program

    public class Book {
        readonly string title;
        bool isBeingLend;
        DateTime returnTime;
        int timesProlonged;


        public string Title() {
            return title;
        }

        public bool IsBeingLend() {
            return isBeingLend;
        }

        public DateTime ReturnTime() {
            return returnTime;
        }

        public int TimesProlonged() {
            return timesProlonged;
        }

        public void SetTimeToReturn(DateTime dateTime) {
            returnTime = dateTime;
        }

        public void LendBook() {
            isBeingLend = true;
        }

        public void ProlongBook(int seconds) {
            returnTime = returnTime.AddSeconds(seconds);
            timesProlonged += 1;
        }

        public void ReturnBook() {
            timesProlonged = 0;
            isBeingLend = false;
        }


        public Book(string title) {
            this.title = title;
        }

        public Book(string title, bool isBeingLend, DateTime returnTime, int timesProlonged) {
            this.title = title;
            this.isBeingLend = isBeingLend;
            this.returnTime = returnTime;
            this.timesProlonged = timesProlonged;
        }

    }

    public class Action {
        readonly Method method = new Method();
        readonly int standardLendTime = 200;

        public void LendBook(List<Book> books, string title, string path) {
            int bookPosition = method.CheckIfBookExists(books, title);
            if (bookPosition >= 0) {
                if (!books[bookPosition].IsBeingLend()) {
                    books[bookPosition].SetTimeToReturn(DateTime.Now.AddSeconds(standardLendTime));
                    books[bookPosition].LendBook();
                    method.SaveToFile(books, path);
                    Console.WriteLine("You have lent '{0}' for {1} seconds", title, standardLendTime);
                } else {
                    double timeLeft = Math.Round((books[bookPosition].ReturnTime() - DateTime.Now).TotalSeconds);
                    Console.WriteLine("You have already lent '{0}'. You have {1} seconds left and have to return it before {2}",
                        books[bookPosition].Title(), timeLeft, books[bookPosition].ReturnTime());
                }
            } else {
                Console.WriteLine("This book doesn't exist. Type showall to see books or type help to show commands");
            }

        }

        public void AddBookToLibrary(List<Book> books, string title, string path) {
            int bookPosition = method.CheckIfBookExists(books, title);
            if (title != "") {
                if (bookPosition == -1) {
                    books.Add(new Book(title));
                    method.SaveToFile(books, path);
                    Console.WriteLine("Book added to library");
                } else {
                    Console.WriteLine("Book already exists");
                }
            } else {
                Console.WriteLine("You have to write a name");
            }


        }

        public void CheckTimeLeft(List<Book> books, string title) {
            int bookPosition = method.CheckIfBookExists(books, title);

            if (bookPosition >= 0) {
                if (books[bookPosition].IsBeingLend()) {
                    double timeLeft = Math.Round((books[bookPosition].ReturnTime() - DateTime.Now).TotalSeconds);
                    Console.WriteLine("You have {0} seconds left of '{1}' and have to return it before {2}", timeLeft, title, books[bookPosition].ReturnTime());
                } else {
                    Console.WriteLine("You have not lent this book");
                }
            } else {
                Console.WriteLine("This book doesn't exist. Type showall to see books or type help to show commands");
            }
        }

        public void ProlongBook(List<Book> books, string title, string path) {
            int bookPosition = method.CheckIfBookExists(books, title);
            if (bookPosition >= 0) {
                if (books[bookPosition].IsBeingLend()) {
                    if (books[bookPosition].TimesProlonged() < 2) {
                        Console.WriteLine("You can prolong the book 2 times. This is the {0} time", (books[bookPosition].TimesProlonged() == 0 ? "first" : "second"));
                        books[bookPosition].ProlongBook(standardLendTime);
                        method.SaveToFile(books, path);
                        Console.WriteLine("Book return time prolonged by {0} seconds to {1}", standardLendTime / 2, books[bookPosition].ReturnTime());
                    } else {
                        Console.WriteLine("You can only prolong 2 times. You have to return the book to library and lend it again");
                    }
                } else {
                    Console.WriteLine("You have not lent this book");
                }
            } else {
                Console.WriteLine("This book doesn't exist. Type showall to see books or type help to show commands");
            }
        }

        public void Return(List<Book> books, string title, string path) {
            int bookPosition = method.CheckIfBookExists(books, title);
            if (bookPosition >= 0) {
                if (books[bookPosition].IsBeingLend()) {
                    books[bookPosition].ReturnBook();
                    method.SaveToFile(books, path);
                    Console.WriteLine("The book have been returned");
                    if (method.TimeExeeded(books, title)) {
                        Console.WriteLine("You have returned '{0}' {1} seconds late. You owe Bjørn 50 kr.", books[bookPosition].Title(),
                            Math.Abs(Math.Round((books[bookPosition].ReturnTime() - DateTime.Now).TotalSeconds)));
                    }
                } else {
                    Console.WriteLine("You have not lent this book");
                }
            } else {
                Console.WriteLine("This book doesn't exist. Type showall to see books or type help to show commands");
            }
        }
    }

    public class Method {

        public void SaveToFile(List<Book> list, string path) {
            File.WriteAllLines(path, BookToStringList(list).ToArray());
        }

        public List<string> BookToStringList(List<Book> books) {
            List<string> bookList = new List<string>();
            for (int i = 0; i < books.Count; i += 1) {
                bookList.Add(books[i].Title() + "/" + books[i].IsBeingLend().ToString() + "/" + books[i].ReturnTime().ToString() + "/" + books[i].TimesProlonged().ToString());
            }
            return bookList;
        }

        public int CheckIfBookExists(List<Book> books, string title) {
            int toReturn = -1;
            for (int i = 0; i < books.Count; i += 1) {
                if (books[i].Title() == title) {
                    toReturn = i;
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Displays available commands
        /// </summary>
        public void ShowHelp() {
            int padding = 40;
            Console.WriteLine();
            Console.WriteLine("Available commands:".PadRight(padding) + "Description:");
            Console.WriteLine("showall".PadRight(padding) + "Shows all names");
            Console.WriteLine("addbock <title>".PadRight(padding) + "Adds the book to the library");
            Console.WriteLine("lendbook <title>".PadRight(padding) + "Lends the book from the library");
            Console.WriteLine("checkbook <title>".PadRight(padding) + "Check how much time you have left of lenting book");
            Console.WriteLine("prolongbook <title>".PadRight(padding) + "Prolongs the time you can have the book. Can be done twice before returning it");
            Console.WriteLine("returnbook <title>".PadRight(padding) + "Returns the book to the library");
            Console.WriteLine("help".PadRight(padding) + "Shows this");
            Console.WriteLine("cls".PadRight(padding) + "Clears the window");
            Console.WriteLine("exit".PadRight(padding) + "Exits program");
        }

        /// <summary>
        /// Prints the given list
        /// </summary>
        /// <param name="list">The list</param>
        public void PrintList(List<Book> list) {
            int longestTitle = 4;
            for (int i = 0; i < list.Count; i += 1) {
                longestTitle = (list[i].Title().Length > longestTitle ? list[i].Title().Length : longestTitle);
            }
            longestTitle += 20;

            List<Book> bookList = new List<Book>();

            Console.WriteLine();
            Console.WriteLine("Books at library:");
            for (int i = 0; i < list.Count; i += 1) {
                bool isBookAtLibrary = !list[i].IsBeingLend();
                if (isBookAtLibrary) {
                    Console.WriteLine(list[i].Title());
                } else {
                    bookList.Add(list[i]);
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Books lented out:");
            Console.WriteLine();
            Console.WriteLine("Title".PadRight(longestTitle) + "When it should be back");
            for (int i = 0; i < bookList.Count; i += 1) {
                Console.WriteLine(bookList[i].Title().PadRight(longestTitle) + bookList[i].ReturnTime().ToString());
            }
        }

        public bool TimeExeeded(List<Book> books, string title) {
            int bookPosition = CheckIfBookExists(books, title);
            double timeLeft = Math.Round((books[bookPosition].ReturnTime() - DateTime.Now).TotalSeconds);
            return (timeLeft < 0 ? true : false);
        }

    }


}

/*
 Define a class for a libraryobject. Is must be possible to:

 - Lend a book with a chosen title

 - Return a book

 - Check if a book with a chosen title is at the library

 - Change the lendtime for a book with a chosen title

Must be constructor (alt + enter in a class) and encapsulation (class must have private variables)
 */
