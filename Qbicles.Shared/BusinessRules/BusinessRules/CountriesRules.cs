using System.Collections.Generic;
using System.Linq;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models.Qbicles;

namespace Qbicles.BusinessRules
{
    public class CountriesRules
    {
        public List<Country> GetAllCountries()
        {
            var countries = Country.All.Where(c => c.CountryCode != CountryCode.World).OrderBy(n => n.CommonName).ToList();
            return countries;
        }

        public Country GetCountryByName(string name)
        {
            name = name ?? "";
            return Country.All.FirstOrDefault(e => e.CommonName.ToLower() == name.ToLower());
        }

        public Country GetCountryByCode(CountryCode code)
        {
            return Country.All.FirstOrDefault(e => e.CountryCode == code);
        }
        public List<MicroCountry> GetCountries()
        {
            var countries = Country.All.Where(c => c.CountryCode != CountryCode.World).OrderBy(n => n.CommonName);
            return countries.Select(e => new MicroCountry { CountryCode = e.CountryCode, CommonName = e.CommonName }).ToList();
        }
    }
}
