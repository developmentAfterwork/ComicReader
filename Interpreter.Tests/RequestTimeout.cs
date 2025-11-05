using Interpreter.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Tests
{
	public class RequestTimeout : IRequestTimeout
	{
		public TimeSpan Timeout => TimeSpan.FromSeconds(30);
	}
}
