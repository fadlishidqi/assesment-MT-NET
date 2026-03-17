namespace AssesmentIndofoodNet
{
    public class MaintenanceService
    {
        public string TentukanStatus(int rh)
        {
            if (rh <= 0) return "Normal";
            if (rh % 2000 == 0) return "[!!] Critical";
            if (rh % 1000 == 0) return "[!] Warning";
            if (rh % 100 == 0) return "[!] Maintenance";
            return null;
        }

        public string GetPesanNotifikasi(string nama, string status, int rh)
        {
            if (status == "[!!] Critical") return $"[Critical] {nama} perlu Maintenance Berat ({rh} jam)";
            if (status == "[!] Warning") return $"[Warning] {nama} perlu Maintenance Medium ({rh} jam)";
            if (status == "[!] Maintenance") return $"[Warning] {nama} perlu Maintenance Ringan ({rh} jam)";
            return "";
        }

        public string DapatkanJenisMaintenance(string status)
        {
            if (status == "[!!] Critical") return "Maintenance Berat";
            if (status == "[!] Warning") return "Maintenance Medium";
            return "Maintenance Ringan";
        }
    }
}