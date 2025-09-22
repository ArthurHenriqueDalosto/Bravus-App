using BravusApp.Shared.Enums;

namespace BravusApp.Client.Model
{
    public static class DutyTypeExtensions
    {
        public static string GetSigla(this DutyType t) => t switch
        {
            DutyType.SV24 => "SV/24",
            DutyType.SVD => "SVD",
            DutyType.SVN => "SVN",
            DutyType.TD => "TD",
            _ => ""
        };

        public static string GetHint(this DutyType t) => t switch
        {
            DutyType.SV24 => "Serviço 24h",
            DutyType.SVD => "Turno Dia",
            DutyType.SVN => "Turno Noite",
            DutyType.TD => "Tarde",
            _ => ""
        };

        public static string GetCssClass(this DutyType t) => t switch
        {
            DutyType.SV24 => "SV24",
            DutyType.SVD => "SVD",
            DutyType.SVN => "SVN",
            DutyType.TD => "TD",
            _ => "NONE"
        };
    }
}
