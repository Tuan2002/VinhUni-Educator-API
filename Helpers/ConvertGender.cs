using VinhUni_Educator_API.Configs;

namespace VinhUni_Educator_API.Helpers
{
    public class ConvertGender
    {
        public static int ConvertToInt(string? gender)
        {
            return gender switch
            {
                GenderMap.FEMALE => 0,
                GenderMap.MALE => 1,
                _ => 2,
            };
        }
    }
}