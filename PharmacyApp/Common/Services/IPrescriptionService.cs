using System;
using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Services
{
    public interface IPrescriptionService
    {
        Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);
        Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    }
}