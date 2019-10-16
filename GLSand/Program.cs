﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLSand
{
	class Program
	{
		static void Main(string[] args)
		{
			using (Game game = new Game(512, 512, "GLSand"))
			{
				game.Run(60.0, 60.0);
			}
		}
	}
}
