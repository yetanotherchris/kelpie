using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelpie.Core.Domain
{
	public class Environment
	{
		public string Name { get; set; }
		public List<Server> Servers { get; set; }
	}
}
