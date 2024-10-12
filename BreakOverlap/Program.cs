
using breakoverlap;


namespace breakoverlap;

public class Program {
    static void Main(string[] args){
        UserDialogue ud = new(new Timeline(new TimelineConfig()));
        ud.Run(args);
    }
}


