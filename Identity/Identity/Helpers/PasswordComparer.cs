namespace Identity.Helpers;

public class PasswordComparer
{
    public static bool ComparePasswordHash(string inputPassword,string? savedHash)
    {
        var inputHash =  HashEncoder.HashPassword(inputPassword);
        return inputHash.Equals(savedHash);
    }
}