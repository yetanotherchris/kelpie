using IisConfiguration.Configuration;

namespace Kelpie.Web.IisConfig
{
    public class Config : EnvironmentalConfig
    {
	    public string SiteName
        {
            get
            {
                return "Kelpie";
            }
        }

        public int PortNumber
        {
            get
            {
                return 410;
            }
        }
    }
}