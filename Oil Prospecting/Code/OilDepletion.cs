﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OilProspecting
{
    public abstract class OilfieldMapSynchroniser
    {
        public abstract void ValuesChanged(IEnumerable<ValueChange> newValues);
        public abstract double[,] GetValues();
    }
    public class ValueChange
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public double NewValue { get; internal set; }
        public ValueChange(int x, int y, double newValue)
        {
            X = x;
            Y = y;
            NewValue = newValue;
        }
    }
    internal class OilfieldMap
    {
        public double ProductionRateHalfLife { get; set; } = 2000;
        public Curve Curve { get; internal set; }
        public double[,] Values { get => values; }
        public int DepletionRadius { get; set; } = 2;
        public OilfieldMapSynchroniser MapSynchroniser { get; internal set; }

        public int Width;
        public int Height;
        private readonly double[,] values;
        public OilfieldMap(int width, int height, double[,] values, Curve curve)
        {
            Width = width;
            Height = height;
            this.values = values;
            Curve = curve;
        }
        public OilfieldMap(OilfieldMapSynchroniser synchroniser, Curve curve)
        {
            this.MapSynchroniser = synchroniser;
            this.Curve = curve;
            values = MapSynchroniser.GetValues();
            Width = values.GetLength(0);
            Height = values.GetLength(1);
        }
        public void ExtractBarrelsAt(int centreX, int centreY, double barrels = 1f)
        {
            (double weightedOilTotal, double weightedDistanceTotal) = GetWeightedOilAmount(centreX, centreY);
            if (weightedOilTotal <= 0)
            {
                return;
            }
            List<ValueChange> valueChanges = new List<ValueChange>();
            for (int x = centreX - DepletionRadius; x <= centreX + DepletionRadius; x++)
            {
                for (int y = centreY - DepletionRadius; y <= centreY + DepletionRadius; y++)
                {
                    double distanceWeight = Gaussian(centreX - x, centreY - y);
                    double newOilValue = Curve.OilAfterExtraction(this[x, y], barrels * (Curve.BarrelsGivenOil(this[x, y], ProductionRateHalfLife) * distanceWeight) / weightedOilTotal, ProductionRateHalfLife);
                    valueChanges.Add(new ValueChange(x, y, newOilValue));
                    this[x, y] = newOilValue;
                }
            }
            OnValuesChanged(valueChanges);
        }
        public (double, double) GetWeightedOilAmount(int centreX, int centreY)
        {
            double totalGaussian = 0f;
            double amount = 0f;
            for (int x = centreX - DepletionRadius; x <= centreX + DepletionRadius; x++)
            {
                for (int y = centreY - DepletionRadius; y <= centreY + DepletionRadius; y++)
                {
                    double distanceWeight = Gaussian(centreX - x, centreY - y);
                    amount += Curve.BarrelsGivenOil(this[x, y], ProductionRateHalfLife) * distanceWeight;
                    totalGaussian += distanceWeight;
                }
            }
            return (amount, totalGaussian);
        }
        public double TotalBarrelsRemaining()
        {
            double total = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    total += Curve.BarrelsGivenOil(this[x, y], ProductionRateHalfLife);
                }
            }
            return total;
        }
        public int BarrelsExtractedDuringTimePeriod(int centreX, int centreY, double seconds, out double elapsedSeconds)
        {
            OilfieldMap predictionMap = new OilfieldMap(Width, Height, Values.Clone() as double[,], Curve);
            predictionMap.ProductionRateHalfLife = ProductionRateHalfLife;
            predictionMap.DepletionRadius = DepletionRadius;
            elapsedSeconds = 0d;
            int barrelsExtracted = 0;
            while (elapsedSeconds + Curve.TimeGivenOil(predictionMap[centreX, centreY]) <= seconds)
            {
                elapsedSeconds += Curve.TimeGivenOil(predictionMap[centreX, centreY]);
                predictionMap.ExtractBarrelsAt(centreX, centreY, 1);
                barrelsExtracted += 1;
            }
            return barrelsExtracted;
        }
        private double Gaussian(double dx, double dy)
        {
            double sigma = 1.5d;
            return (1d / (sigma * Math.Sqrt(2 * Math.PI))) * Math.Exp(-0.5d * (Math.Pow(dx, 2) + Math.Pow(dy, 2)) / Math.Pow(sigma, 2));
        }
        private void OnValuesChanged(IEnumerable<ValueChange> valueChanges)
        {
            if (MapSynchroniser != null)
            {
                MapSynchroniser.ValuesChanged(valueChanges);
            }
        }
        private (int, int) WrapIndex(int x, int y)
        {
            int wrappedX, wrappedY;
            if (x < 0)
            {
                wrappedX = Width - (-x) % Width;
            }
            else if (x >= Width)
            {
                wrappedX = x % Width;
            }
            else
            {
                wrappedX = x;
            }
            if (y < 0)
            {
                wrappedY = Height - (-y) % Height;
            }
            else if (y >= Height)
            {
                wrappedY = y % Height;
            }
            else
            {
                wrappedY = y;
            }
            return (wrappedX, wrappedY);
        }
        public double this[int x, int y]
        {
            get
            {
                int wrappedX, wrappedY;
                (wrappedX, wrappedY) = WrapIndex(x, y);
                return values[wrappedX, wrappedY];
            }
            set
            {
                int wrappedX, wrappedY;
                (wrappedX, wrappedY) = WrapIndex(x, y);
                values[wrappedX, wrappedY] = value;
            }
        }
    }
    internal class Curve
    {
        public double MaxTime { get; } = 20 * 60;
        public double MinTime { get => TimeGivenOil(1d); }
        public double Dampener = 0.95d;

        public Curve(double maxTime, double dampener)
        {
            MaxTime = maxTime;
            Dampener = dampener;
        }

        public double RateGivenOil(double oil)
        {
            return RateGivenTime(TimeGivenOil(oil));
        }
        public double RateGivenTime(double time)
        {
            return 60d / time;
        }
        public double TimeGivenRate(double rate)
        {
            return 60d / rate;
        }
        public double TimeGivenOil(double oil)
        {
            return MaxTime * (1d - Dampener * oil);
        }
        public double OilGivenTime(double time)
        {
            return (1f - time / MaxTime) / Dampener;
        }
        public double OilGivenRate(double rate)
        {
            return OilGivenTime(TimeGivenRate(rate));
        }
        public double OilAfterExtraction(double oil, double barrelsExtracted, double productionRateHalfLife)
        {
            return Math.Max(0, OilGivenTime(TimeGivenOil(oil) / Math.Pow(0.5d, barrelsExtracted / productionRateHalfLife)));
        }
        public double BarrelsGivenOil(double oil, double productionRateHalfLife)
        {
            return productionRateHalfLife * Math.Log2(MaxTime / TimeGivenOil(oil));
        }

        public double OilGivenBarrels(double barrelsRemaining, double productionRateHalfLife)
        {
            return OilGivenTime(MaxTime / Math.Pow(2, barrelsRemaining / productionRateHalfLife));
        }
    }
}