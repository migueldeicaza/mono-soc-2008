// ParallelTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

using ParallelFxTests.RayTracer;

using NUnit;
using NUnit.Core;
using NUnit.Framework;

namespace ParallelFxTests
{
	
	[TestFixture()]
	public class ParallelTests
	{
		int[] pixels;
		RayTracerApp rayTracer;
		
		[SetUp]
		public void Setup()
		{
			Stream stream = Assembly.GetAssembly(typeof(ParallelTests)).GetManifestResourceStream("raytracer-output.xml");
			Console.WriteLine(stream == null);
			XmlSerializer serializer = new XmlSerializer(typeof(int[]));
			pixels = (int[])serializer.Deserialize(stream);
			rayTracer = new RayTracerApp();
		}
		
		[Test]
		public void ParallelForTestCase()
		{
			// Test the the output of the Parallel RayTracer is the same than the synchronous ones 
			CollectionAssert.AreEquivalent(pixels, rayTracer.Pixels, "#1, same pixels");
			CollectionAssert.AreEqual(pixels, rayTracer.Pixels, "#2, pixels in order");
		}
	}
}