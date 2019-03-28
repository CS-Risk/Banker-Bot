using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankerBot
{
	[Serializable]
	class CharacterNotFoundException : Exception
	{
		public CharacterNotFoundException(string name) 
			: base (string.Format("Character note found: {0}", name))
		{

		}
	}
}
