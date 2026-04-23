namespace PharmacyApp.Models
{
    public class Substance
    {
        public string Name { get; private set; }
        public float LethalDose { get; set; }
        public string Description { get; set; }

        public Substance(string name, float lethalDose, string description)
        {
            Name = name;
            LethalDose = lethalDose;
            Description = description;
        }
    }
}
