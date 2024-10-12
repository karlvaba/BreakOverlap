using System.Text;
using breakoverlap;
using CommandLine;

namespace breakoverlap;


public class UserDialogue(Timeline timeline)
{
    private readonly Timeline Timeline = timeline;

    public void Run(string[] args) {
        Write("Application started.");
        TryReadFromFile(args);

        while (true) {
            Console.Write("Your command ( q(uit) / h(elp) / <HH:mm><HH:mm>): ");
            CallAction(Read()); //Reads a single line of user input, tries to map it to an action
        }
    }

    /*
        Different input options. Currently possible
        q/quit - exits the app
        h/help - displays help text
        otherwise, assumes the user tried to input a driver's break time
    */
    private void CallAction(string input) {
        if (input.Equals("q") || input.Equals("quit")) {
            Environment.Exit(0);
        } else if (input.Equals("h") || input.Equals("help")) {
            DisplayHelpText();
        } else {
            TryAddAndCalculate(input);
        };
    }

    /*
        Tries to add a single input line as a break time. Calls the calculation of overlaps if successful.
    */
    private void TryAddAndCalculate(string input) {
        try {
                Timeline.AddBreakTime(input);
                (TimeRange, int) result = Timeline.MostCommonTimeRange();
                Write(string.Format("\nMost common break time is {0} with {1} people on break.\n", result.Item1, result.Item2));
        } catch (ArgumentException){
                Write("Could not match input to any valid option. For help, write 'help'");
        }
    }

    /*
        Parses the input arguments and calls reading from file if the filename option was provided.
    */
    private void TryReadFromFile(string[] args) {
        Parser.Default.ParseArguments<ArgumentOptions>(args)
            .WithParsed<ArgumentOptions>(opts => {
                if (opts.Filename != null) {
                    Write("Attempting to read from provided file path...");
                    try {
                        List<int> errs = Timeline.ReadFromFile(opts.Filename); //Lines from the file that could not be parsed as times.
                        if (errs.Count > 0) {
                            Write("Not all files lines could be parsed as valid break times." +
                                "Rest of the lines were read and parsed. Uparsed lines:\n" 
                                + string.Join(";", errs));                           
                        }
                        (TimeRange, int) result = Timeline.MostCommonTimeRange(); //Also calculates the most overlapped range if successful.
                        Write(string.Format("Most common break time is {0} with {1} people on break.\n", result.Item1, result.Item2));
                    } catch (Exception e) {
                        Write(e.Message);
                }
            }});
    }

    private static void DisplayHelpText() {
        StringBuilder helpText = new();
        helpText.AppendLine("Hopefully this message helps. Your input options are:\n");
        helpText.AppendLine("q/quit\t\tExits the application\n");
        helpText.AppendLine("h/help\t\tDisplays this help message\n");
        helpText.Append("<HH:mm><HH:mm>\tDenotes start and time of a single driver's break. First = start time, second = end time. ");
        helpText.AppendLine("Both of the times must in 24H format (HH:mm). Start must be <= end.");
        helpText.AppendLine("\t\t\tE.g '10:0012:00' or '06:1523:30'");
        helpText.AppendLine();
        Write(helpText.ToString());
    }

    private static void Write(string message) {
        Console.WriteLine(message);
    }

    private static string Read() {
        return Console.ReadLine() + "";
    }
}

/*
    Helper class to read command line options on startup.
*/
public class ArgumentOptions {
    [Option('f', "filename", Required = false, HelpText = "Input file for break times.")]
    public string? Filename { get; set; } 
}

