﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STimWPF.Graphic3D
{
	public class BigPlanet : SphereGeometry3D
	{
		public BigPlanet()
		{
			Radius = 0.0055;
			Separators = 100;
		}
	}

	public class PlanetRing : DiscGeometry3D
	{
		public PlanetRing()
		{
			Radius = 70;
			Separators = 100;
		}
	}

	public class SmallPlanet : SphereGeometry3D
	{
		public SmallPlanet()
		{
			Radius = 0.101;
			Separators = 100;
		}
	}
}
