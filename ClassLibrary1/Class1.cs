using System.IO.Compression;

namespace ClassLibrary1;

public class Class1
{
    static public String Test()
    {
        if ("abc".ToLower() == "12a".ToLower()) // RCS1155
        {
            return "a";
        }


        try
        {
            return "ddd";
        }
        catch (Exception ex)
        {
            
        }

        return "aaa";

        if (false == true)
        {
            return "weird code";
        }
    }

    public static string UntestedMethod()
    {
        throw new NotImplementedException();
    }
}
