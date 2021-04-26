namespace RecoveryCore
{
    public interface IWordlistGenerator
    {
        WordlistGeneratorConfiguration Configuration { get; }
        
        string CurrentWord { get; set; }

        bool OutOfWords { get; set; }
        
        int[] NextWordIndexes { get; }


        string GetNextWord();
    }
}
