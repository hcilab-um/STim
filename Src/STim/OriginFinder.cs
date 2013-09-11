using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using STim.Util;

namespace STim
{
	public class OriginFinder
	{
		private const double ESTIMATE_ACCURACY = 0.0001;
		private const double GRID_SIZE = 0.05;
		enum Location
		{
			Center,
			Left,
			Right,
			Top,
			Down,
			Front,
			Back
		}

		private Point3D testOrigin;

		private double[,] standardAngles;
		private Dictionary<Location, Point3D> currentLocations;
		private Dictionary<Location, double> locationErrors;

		public OriginFinder()
		{
			currentLocations = new Dictionary<Location, Point3D>();
			locationErrors = new Dictionary<Location, double>();

			ResetOriginFinder();
		}

		private void ResetOriginFinder()
		{
			locationErrors.Clear();
			currentLocations.Clear();

			foreach (Location direction in Enum.GetValues(typeof(Location)))
			{
				currentLocations.Add(direction, new Point3D());
				locationErrors.Add(direction, -1);
			}
		}

		public void TestOriginFinder(int sampleSize)
		{
			double error = 0;
			double totalDur = 0;
			Random randomMaker = new Random(DateTime.Now.Millisecond);
			double maxErr = 0;

			for (int j = 0; j < sampleSize; j++)
			{
				Point3D[] standardPoints = new Point3D[]
				{
					new Point3D(0, 0.5, 1.5), //top
					new Point3D(0.5, 0.5, 1.5), //right
					new Point3D(0, 0.5, 2), //front
					new Point3D(-0.5, 0.25, 1.5) //back
				};

				Point3D[] capturePoints = new Point3D[standardPoints.Length];

				Matrix3D matrix = new Matrix3D();

				//matrix.Translate(new Vector3D(0, 10, 0));

				double size = 1;
				matrix.RotateAt(new Quaternion(new Vector3D((randomMaker.Next(1) - 1) * randomMaker.NextDouble() * size,
																										(randomMaker.Next(1) - 1) * randomMaker.NextDouble() * size,
																										randomMaker.NextDouble() * size),
																										(randomMaker.Next(1) - 1) * randomMaker.NextDouble() * 360),
																			 new Point3D(0, 0, 0));

				matrix.Translate(new Vector3D((randomMaker.Next(1) - 1) * randomMaker.NextDouble() * size,
																			(randomMaker.Next(1) - 1) * randomMaker.NextDouble() * size,
																			(randomMaker.Next(1) - 1) * randomMaker.NextDouble() * size));

				for (int i = 0; i < standardPoints.Length; i++)
				{
					capturePoints[i] = matrix.Transform(standardPoints[i]);
				}

				testOrigin = matrix.Transform(new Point3D());
				DateTime start = DateTime.Now.ToUniversalTime();
				Point3D newOrigin = this.BruteForceEstimateOrigin(standardPoints, capturePoints, 5);
				TimeSpan end = DateTime.Now.ToUniversalTime() - start;
				int duration = (int)end.TotalSeconds;
				totalDur += duration;
				Vector3D og = new Vector3D(testOrigin.X, testOrigin.Y, testOrigin.Z);
				Vector3D offset = new Vector3D(testOrigin.X - newOrigin.X, testOrigin.Y - newOrigin.Y, testOrigin.Z - newOrigin.Z);
				error += offset.Length;
				if (maxErr < offset.Length)
				{
					maxErr = offset.Length;
					Console.WriteLine("Max Error: {1}", j, Math.Round(maxErr, 6));
				}
			}
			double avgErr = error / sampleSize;
			double avgDur = totalDur / sampleSize;
			Console.WriteLine("Average Error: {0}\t Max Error: {1}", avgErr, maxErr);
		}

		public Point3D BruteForceEstimateOrigin(Point3D[] standardPoints, Point3D[] capturePoints, double spaceRange)
		{
			int arraySize = standardPoints.Length;

			Vector3D[] standardVectors = new Vector3D[arraySize];

			standardAngles = new double[arraySize, arraySize];

			for (int i = 0; i < arraySize; i++)
			{
				standardVectors[i] = standardPoints[i].ToVector3D();
			}

			CalculateAngles(standardVectors, standardAngles);

			currentLocations[Location.Center] = FindStartingLocation(capturePoints, spaceRange, GRID_SIZE);

			double jumpDistance = GRID_SIZE / 4;

			do
			{
				currentLocations[Location.Left] = new Point3D(currentLocations[Location.Center].X - jumpDistance, currentLocations[Location.Center].Y, currentLocations[Location.Center].Z);
				currentLocations[Location.Right] = new Point3D(currentLocations[Location.Center].X + jumpDistance, currentLocations[Location.Center].Y, currentLocations[Location.Center].Z);

				currentLocations[Location.Top] = new Point3D(currentLocations[Location.Center].X, currentLocations[Location.Center].Y + jumpDistance, currentLocations[Location.Center].Z);
				currentLocations[Location.Down] = new Point3D(currentLocations[Location.Center].X, currentLocations[Location.Center].Y - jumpDistance, currentLocations[Location.Center].Z);

				currentLocations[Location.Front] = new Point3D(currentLocations[Location.Center].X, currentLocations[Location.Center].Y, currentLocations[Location.Center].Z + jumpDistance);
				currentLocations[Location.Back] = new Point3D(currentLocations[Location.Center].X, currentLocations[Location.Center].Y, currentLocations[Location.Center].Z - jumpDistance);

				foreach (Location direction in Enum.GetValues(typeof(Location)))
				{
					locationErrors[direction] = CalculateLocationError(currentLocations[direction], capturePoints);
				}

				Vector3D movement = CalculateMoveDirection(jumpDistance);
				if (movement.Length == 0)
				{
					jumpDistance *= 0.5;
					continue;
				}

				Point3D newCenter = new Point3D(currentLocations[Location.Center].X + movement.X,
															currentLocations[Location.Center].Y + movement.Y,
															currentLocations[Location.Center].Z + movement.Z);

				if (CalculateLocationError(newCenter, capturePoints) >= locationErrors[Location.Center])
				{
					jumpDistance *= 0.5;
					continue;
				}

				currentLocations[Location.Center] = newCenter;
				locationErrors[Location.Center] = CalculateLocationError(currentLocations[Location.Center], capturePoints);


			} while (jumpDistance > ESTIMATE_ACCURACY);

			Point3D estimateOrigin = currentLocations[Location.Center];
			double result = CalculateLocationError(testOrigin, capturePoints);
			double a = locationErrors[Location.Center];

			ResetOriginFinder();

			return estimateOrigin;
		}

		private void CalculateAngles(Vector3D[] vectors, double[,] angles)
		{
			int size = angles.GetLength(0);
			for (int i = 0; i < size; i++)
			{
				for (int j = i + 1; j < size; j++)
				{
					angles[i, j] = Vector3D.AngleBetween(vectors[i], vectors[j]);
				}
			}
		}

		/// <summary>
		/// //Look around the space from origin and find best starting point
		/// </summary>
		private Point3D FindStartingLocation(Point3D[] capturePoints, double spaceRange, double gridSize)
		{
			int totalSamples = (int)(spaceRange / gridSize);
			double currentErrors = double.PositiveInfinity;
			Point3D startPoint = new Point3D();
			for (int i = 0; i < totalSamples; i++)
			{
				for (int j = 0; j < totalSamples; j++)
				{
					for (int k = 0; k < totalSamples; k++)
					{
						Point3D samplePoint = new Point3D(i * gridSize - spaceRange / 2, j * gridSize - spaceRange / 2, k * gridSize - spaceRange / 2);
						double sampleError = CalculateLocationError(samplePoint, capturePoints);
						if (sampleError < currentErrors)
						{
							startPoint = samplePoint;
							currentErrors = sampleError;
						}
					}
				}
			}

			return startPoint;
		}

		private double CalculateLocationError(Point3D origin, Point3D[] capturePoints)
		{
			int size = capturePoints.Length;
			Vector3D[] vectors = new Vector3D[size];
			double[,] angles = new double[size, size];
			double[,] angleErrors = new double[size, size];

			for (int i = 0; i < size; i++)
			{
				vectors[i] = new Vector3D(capturePoints[i].X - origin.X, capturePoints[i].Y - origin.Y, capturePoints[i].Z - origin.Z);
			}

			CalculateAngles(vectors, angles);

			CalculateAngleErrors(standardAngles, angles, angleErrors);

			double directionOffset = 0;

			for (int i = 0; i < size; i++)
			{
				for (int j = i + 1; j < size; j++)
				{
					directionOffset += angleErrors[i, j];
				}
			}

			return directionOffset;
		}

		private void CalculateAngleErrors(double[,] standardAngles, double[,] captureAngles, double[,] angleOffsets)
		{
			int size = standardAngles.GetLength(0);

			for (int i = 0; i < size; i++)
			{
				for (int j = i + 1; j < size; j++)
				{
					if (Double.IsNaN(captureAngles[i, j]))
					{
						angleOffsets[i, j] = double.PositiveInfinity;
					}
					else
						angleOffsets[i, j] = Math.Abs(standardAngles[i, j] - captureAngles[i, j]) / standardAngles[i, j];
				}
			}
		}

		private Vector3D CalculateMoveDirection(double jumpDistance)
		{
			Vector3D movementDirection = new Vector3D(0, 0, 0);

			double leftImprovement = locationErrors[Location.Center] - locationErrors[Location.Left];
			double rightImprovement = locationErrors[Location.Center] - locationErrors[Location.Right];
			double topImprovement = locationErrors[Location.Center] - locationErrors[Location.Top];
			double downImprovement = locationErrors[Location.Center] - locationErrors[Location.Down];
			double frontImprovement = locationErrors[Location.Center] - locationErrors[Location.Front];
			double backForce = locationErrors[Location.Center] - locationErrors[Location.Back];

			//Only consider positive improvement, when all the improvement is negative, do not move it.
			if (leftImprovement > 0 || rightImprovement > 0)
			{
				movementDirection.X = rightImprovement - leftImprovement;
			}
			if (topImprovement > 0 || downImprovement > 0)
			{
				movementDirection.Y = topImprovement - downImprovement;
			}
			if (frontImprovement > 0 || backForce > 0)
			{
				movementDirection.Z = frontImprovement - backForce;
			}

			if (movementDirection.Length == 0)
			{
				return movementDirection;
			}

			// normalize the movement
			movementDirection.Normalize();

			Vector3D movement = movementDirection * jumpDistance;
			return movement;
		}

	}
}