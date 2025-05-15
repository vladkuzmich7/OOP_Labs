
namespace DocMaster.Utilities
{
    public static class InputValidator
    {
        public static int GetIntInput(int a, int b)
        {
            int output;
            string s;
            while (true)
            {
                s = Console.ReadLine();
                if (int.TryParse(s, out output) && output >= a && output <= b)
                {
                    return output;
                }
                else
                {
                    Console.Write("Incorrect input. Try again: ");
                }
            }
        }

    }
}
