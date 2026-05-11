public static class LanguageEnumExtensions
{
    public static string ToLanguageCode(this LanguageEnum language)
    {
        return language switch
        {
            LanguageEnum.English => "en",
            LanguageEnum.Spanish => "es",
            _ => "en"
        };
    }
}
