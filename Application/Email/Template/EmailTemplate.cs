using System.Reflection;

namespace Application.Email.Template;

public class EmailTemplate
{
    private static string ReadResources(string pathName)
    {
        // Might be a good idea to catch the exception here
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(pathName);
        using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
        return reader.ReadToEnd();
    }

    internal static string InviteUser(string message)
    {
        var html = ReadResources("KeyholeDashboard.Application.Email.Template.InviteUserTemplate.txt");
        var modifiedHtml =
            html.Replace("{{message}}", message)
                .Replace("{{unsubscribe}}", "");
        return modifiedHtml;
    }
}