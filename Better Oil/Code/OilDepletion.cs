using System;
using System.Collections.Generic;

namespace BetterOil
{
    internal class OilfieldMap
    {
        public double ProductionRateHalfLife { get; set; }
        public Curve Curve { get; internal set; }
        public double[,] Values { get => values; }
        public int DepletionRadius { get; set; }
        public IOilfieldMapSetter MapSynchroniser { get; internal set; }
        public double Sigma { get; internal set; } = 1.5d;

        public int Width;
        public int Height;
        private readonly double[,] values;
        public OilfieldMap(double[,] values, Curve curve, int depletionRadius = 1, double productionRateHalfLife = 2000)
        {
            this.values = values ?? new double[0, 0];
            Width = values.GetLength(0);
            Height = values.GetLength(1);
            Curve = curve;
            DepletionRadius = depletionRadius;
            ProductionRateHalfLife = productionRateHalfLife;
        }
        public OilfieldMap(IOilfieldMapGetter initialDataGetter, IOilfieldMapSetter synchroniser, Curve curve, int depletionRadius, double productionRateHalfLife)
        {
            this.MapSynchroniser = synchroniser;
            this.Curve = curve;
            DepletionRadius = depletionRadius;
            ProductionRateHalfLife = productionRateHalfLife;
            values = initialDataGetter.GetValues() ?? new double[0, 0];
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
                    int dx = centreX - x;
                    int dy = centreY - y;
                    if (Distance(dx, dy) <= DepletionRadius + 0.5d)
                    {
                        double distanceWeight = Gaussian(dx, dy);
                        double newOilValue = Curve.OilAfterExtraction(this[x, y], barrels * (Curve.BarrelsGivenOil(this[x, y], ProductionRateHalfLife) * distanceWeight) / weightedOilTotal, ProductionRateHalfLife);
                        valueChanges.Add(new ValueChange(x, y, newOilValue));
                        this[x, y] = newOilValue;
                    }
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
                    int dx = centreX - x;
                    int dy = centreY - y;
                    if (Distance(dx, dy) <= DepletionRadius + 0.5d)
                    {
                        double distanceWeight = Gaussian(dx, dy);
                        amount += Curve.BarrelsGivenOil(this[x, y], ProductionRateHalfLife) * distanceWeight;
                        totalGaussian += distanceWeight;
                    }
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
            OilfieldMap predictionMap = new OilfieldMap(Values.Clone() as double[,], Curve, DepletionRadius, ProductionRateHalfLife);
            predictionMap.Sigma = Sigma;
            elapsedSeconds = 0d;
            int barrelsExtracted = 0;
            double nextBarrelTime = Curve.TimeGivenOil(predictionMap[centreX, centreY]);
            while (elapsedSeconds + nextBarrelTime <= seconds)
            {
                elapsedSeconds += nextBarrelTime;
                predictionMap.ExtractBarrelsAt(centreX, centreY, 1);
                barrelsExtracted += 1;
                nextBarrelTime = Curve.TimeGivenOil(predictionMap[centreX, centreY]);
            }
            return barrelsExtracted;
        }
        private double Distance(int dx, int dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private double Gaussian(double dx, double dy)
        {
            return (1d / (Sigma * Math.Sqrt(2 * Math.PI))) * Math.Exp(-0.5d * (Math.Pow(dx, 2) + Math.Pow(dy, 2)) / Math.Pow(Sigma, 2));
        }
        private void OnValuesChanged(IEnumerable<ValueChange> valueChanges)
        {
            if (MapSynchroniser != null)
            {
                MapSynchroniser.UpdateValues(valueChanges);
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