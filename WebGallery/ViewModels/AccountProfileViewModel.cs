using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AccountProfileViewModel
    {
        public SubmittersContactDetail ContactDetail { get; set; }

        public List<SelectListItem> Countries
        {
            get
            {
                // Inspired from http://cs.vbcity.com/blogs/mike-mcintyre/archive/2009/04/21/country-list-for-combobox-or-dropdown-from-net-globalization-namespace.aspx
                var countries = new List<SelectListItem>();
                // Iterate the Framework Cultures...
                foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.FrameworkCultures))
                {
                    RegionInfo ri = null;
                    try
                    {
                        // If a RegionInfo object could not be created we don't want to use the CultureInfo for the country list.
                        ri = new RegionInfo(ci.Name);
                    }
                    catch
                    {
                        continue;
                    }
                    var country = new SelectListItem() { Text = ri.EnglishName, Value = ri.ThreeLetterISORegionName };
                    if (!countries.Select(c => c.Text).Contains(country.Text))
                    {
                        countries.Add(country);
                    }
                }

                return countries.OrderBy(x => x.Text).ToList();
            }
        }

        public List<SelectListItem> States
        {
            get
            {
                var file = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "USstates.xml");
                var states = from state in XDocument.Load(file).Document.Descendants("state")
                             select new SelectListItem
                             {
                                 Value = state.Attribute("code").Value,
                                 Text = state.Attribute("statename").Value
                             };

                return states.ToList();
            }
        }
    }
}