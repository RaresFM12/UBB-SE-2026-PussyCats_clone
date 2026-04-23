using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Repositories
{
    public interface ISubstancesRepository
    {
        void AddSubstance(string name, float lethalDose, string description);
        void RemoveSubstanceByName(string name);
        Substance GetSubstanceByName(string name);
        List<Substance> GetAllSubstances();
        void UpdateSubstance(Substance substance);
        bool SubstanceExists(string name);
        public Dictionary<string, int> GetTop20Substances();
    }
}
