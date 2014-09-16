using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmateurLabs.ALD {
	public interface IAldSerializable {

		AldNode Serialize();
		void Deserialize(AldNode node);
	}
}
