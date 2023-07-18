namespace Mixwel
{
    public static class ValidationMessages
    {
        public static string NullOrEmpty(string parameter) =>
            $"Pararmeter {parameter} should not be empty or null";
    }
}
