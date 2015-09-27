using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelpie.Core
{
    public class FileReader
    {
	    private IEnumerable<string> _serverPaths;
	    private readonly string _appName;

	    public FileReader(IEnumerable<string> serverPaths, string appName)
	    {
		    _serverPaths = serverPaths;
		    _appName = appName;
	    }

	    IEnumerable<string> GetFilesForServer()
	    {
		    return null;
	    }
    }
}
