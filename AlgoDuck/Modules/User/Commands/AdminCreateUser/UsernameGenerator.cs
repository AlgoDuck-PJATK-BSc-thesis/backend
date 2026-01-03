using System.Security.Cryptography;

namespace AlgoDuck.Modules.User.Commands.AdminCreateUser;

public static class UsernameGenerator
{
    private static readonly string[] Adjectives =
    [
        "calm","brave","bright","silent","gentle","swift","kind","lucky","mellow","steady",
        "witty","sly","eager","quiet","bold","clever","sunny","fuzzy","chill","noble",
        "smooth","happy","proud","fresh","wild","shy","tender","zesty","cool","simple"
    ];

    private static readonly string[] Animals =
    [
        "yak","otter","fox","wolf","panda","tiger","eagle","koala","badger","lynx",
        "dolphin","falcon","rabbit","bison","gecko","heron","orca","moose","beaver","marmot",
        "sparrow","whale","seal","crane","lion","bear","owl","horse","camel","giraffe"
    ];

    public static string GenerateUserStyle()
    {
        var adj = Pick(Adjectives);
        var animal = Pick(Animals);
        var n = RandomNumberGenerator.GetInt32(0, 10000);
        return $"{adj}_{animal}_{n:D4}";
    }

    public static string GenerateAdminStyle()
    {
        var n = RandomNumberGenerator.GetInt32(0, 100000);
        return $"admin{n:D5}";
    }

    private static string Pick(string[] items)
    {
        var idx = RandomNumberGenerator.GetInt32(0, items.Length);
        return items[idx];
    }
}