namespace HealthCareABApi.DTO
{
    public class UniqueSlotsDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<CaregiverDTO> Caregivers { get; set; }
    }

    public class CaregiverDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
