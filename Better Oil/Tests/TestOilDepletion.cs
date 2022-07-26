using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterOil.Tests
{
    [TestClass]
    public class TestOilDepletion
    {
        private Curve curve = new Curve(20 * 60, 0.95d);
        /// <summary>
        /// Test whether the minimum time per barrel is correctly calculated
        /// </summary>
        [TestMethod]
        public void TestCurveMinTime()
        {
            Assert.AreEqual(60d, curve.MinTime, 0.001d);
        }
        /// <summary>
        /// Test whether the extraction rate in barrels per minute is correctly calculated when given the amount of oil
        /// </summary>
        [TestMethod]
        public void TestRateFromOil()
        {
            double rate = curve.RateGivenOil(1d);
            Assert.AreEqual(1d, rate, 0.001d);
        }
        /// <summary>
        /// Test whether the curve calculates how much oil would be left after a number of barrels are extracted
        /// </summary>
        [TestMethod]
        public void TestOilAfterExtraction()
        {
            double oilAfterExtraction = curve.OilAfterExtraction(1d, 2, 4);
            Assert.AreEqual(0.978199286191d, oilAfterExtraction, 0.001d);
        }
        /// <summary>
        /// Test that the oil amount is still calculated accurately after a single larger extraction
        /// </summary>
        [TestMethod]
        public void TestOilAfterBigExtraction()
        {
            double oilAfterExtraction = curve.OilAfterExtraction(1d, 100, 25);
            Assert.AreEqual(0.210526315789d, oilAfterExtraction, 0.001d);
        }
        /// <summary>
        /// Test that the oil amount is accurate after a high number of sequential extractions
        /// </summary>
        [TestMethod]
        public void TestOilAfterMultipleExtractions()
        {
            //Half life of 8000 barrels, and check that it is still accurate after 4 half lives
            int barrelsExtracted = 0;
            double oilAfterExtraction = 1d;
            double productionRateHalfLife = 8000;
            do
            {
                int barrelsPerExtraction = 1;
                oilAfterExtraction = curve.OilAfterExtraction(oilAfterExtraction, barrelsPerExtraction, productionRateHalfLife);
                barrelsExtracted += barrelsPerExtraction;
                Assert.AreEqual(curve.OilAfterExtraction(1d, barrelsExtracted, productionRateHalfLife), oilAfterExtraction, 0.001d);
            }
            while (barrelsExtracted < productionRateHalfLife * 4);
            Assert.AreEqual(0.210526315789d, oilAfterExtraction, 0.001d);
        }
        /// <summary>
        /// Test that the number of barrels that can be extracted before the oil level reaches zero is correct
        /// </summary>
        [TestMethod]
        public void TestBarrelsGivenOil()
        {
            Assert.AreEqual(17.2877123795d, curve.BarrelsGivenOil(1d, 4), 0.0001d);
            Assert.AreEqual(14.4122558234d, curve.BarrelsGivenOil(0.8d, 7), 0.0001d);
        }
        /// <summary>
        /// Test that the oil level is correct for the number of barrels remaining
        /// </summary>
        [TestMethod]
        public void TestOilGivenBarrels()
        {
            Assert.AreEqual(0.146787960795d, curve.OilGivenBarrels(0.65d, 3), 0.0001d);
        }
        /// <summary>
        /// Test that the OilfieldMap reads the value at the right position
        /// </summary>
        [TestMethod]
        public void TestOilfieldMapGet()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }, curve);
            Assert.AreEqual(2, oilfieldMap[0, 1]);
            Assert.AreEqual(6, oilfieldMap[1, 2]);
        }
        /// <summary>
        /// Test that the right value is given when the coordinates are outside the range of the map size, as they should loop round again
        /// </summary>
        [TestMethod]
        public void TestOilfieldMapGetWrapped()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }, curve);
            Assert.AreEqual(9, oilfieldMap[-1, 5]);
            Assert.AreEqual(5, oilfieldMap[19, -11]);
        }
        /// <summary>
        /// Test that the gaussian weighting function is working
        /// </summary>
        [TestMethod]
        public void TestTotalGaussian()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve, 2, 1);
            (double _, double totalGaussian) = oilfieldMap.GetWeightedOilAmount(3, 3);
            Assert.AreEqual(2.93772165094d, totalGaussian, 0.001d);
        }
        /// <summary>
        /// Test that the maximum number of barrels that can be extracted before the whole map has zero oil is right
        /// </summary>
        [TestMethod]
        public void TestTotalBarrelsRemaining()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 0.65d },
                { 0.2d, 0d },
            }, curve);
            oilfieldMap.ProductionRateHalfLife = 3;
            Assert.AreEqual(18.0372078866d, oilfieldMap.TotalBarrelsRemaining(), 0.001d);
        }
        /// <summary>
        /// Test that the calculation for the whole number of barrels that can be extracted sequentially in a given time is correct
        /// </summary>
        [TestMethod]
        public void TestBarrelsExtractedGivenTime()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve);
            oilfieldMap.ProductionRateHalfLife = 1;
            oilfieldMap.DepletionRadius = 0;
            int barrelsExtracted = oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 900, out double elapsedSeconds);
            Assert.AreEqual(3, barrelsExtracted);
            Assert.AreEqual(420, elapsedSeconds, 0.0001d);
        }
        /// <summary>
        /// Make sure that the oil map doesn't get changed during its simulation of future extractions
        /// </summary>
        [TestMethod]
        public void TestOriginalMapUnchangedAfterBarrelsExtractedGivenTime()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve);
            oilfieldMap.ProductionRateHalfLife = 1;
            oilfieldMap.DepletionRadius = 0;
            oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 900, out double _);

            double[,] expectedValuesAfterExtraction = new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d }
            };
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    Assert.AreEqual(expectedValuesAfterExtraction[x, y], oilfieldMap.Values[x, y], 0.0001d, "Wrong value at " + x + ", " + y);
                }
            }
        }
        /// <summary>
        /// Test that the total number of barrels in the map is correct after extracting a given number of them
        /// </summary>
        [TestMethod]
        public void TestTotalBarrelsIsCorrectAfterExtraction()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve, 2, 1);
            double initialBarrels = oilfieldMap.TotalBarrelsRemaining();
            Console.WriteLine("Initial barrels: " + initialBarrels);
            oilfieldMap.ExtractBarrelsAt(3, 3, 40);
            Print2DArray(oilfieldMap.Values);
            OilfieldMap newOilfieldMap = new OilfieldMap(oilfieldMap.Values, curve);
            newOilfieldMap.ProductionRateHalfLife = 1;
            Console.WriteLine("Remaining barrels: " + newOilfieldMap.TotalBarrelsRemaining());
            Assert.AreEqual(initialBarrels - 40, newOilfieldMap.TotalBarrelsRemaining(), 0.0001d);
        }
        /// <summary>
        /// Test that barrels are only extracted from the specific location and that the amount of oil at that spot is correct
        /// </summary>
        [TestMethod]
        public void TestOilMapUpdatedOnExtraction0Radius()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve, 0, 2);
            oilfieldMap.ExtractBarrelsAt(3, 3, 2);
            double[,] expectedValuesAfterExtraction = new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 0.947368421053d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d }
            };
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    Assert.AreEqual(expectedValuesAfterExtraction[x, y], oilfieldMap.Values[x, y], 0.0001d, "Wrong value at " + x + ", " + y);
                }
            }
        }
        /// <summary>
        /// Test that barrels are extracted correctly from the 5x5 square centred around the specific location
        /// </summary>
        [TestMethod]
        public void TestOilMapUpdatedOnExtraction2Radius()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve, 2, 1);
            oilfieldMap.ExtractBarrelsAt(3, 3, 40);
            double[,] expectedValuesAfterExtraction = new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 0.93237488804d, 0.904920910456d, 0.93237488804d, 1d, 1d },
                { 1d, 0.93237488804d, 0.789474056844d, 0.659847812397d, 0.789474056844d, 0.93237488804d, 1d },
                { 1d, 0.904920910456d, 0.659847812397d, 0.404929213959d, 0.659847812397d, 0.904920910456d, 1d },
                { 1d, 0.93237488804d, 0.789474056844d, 0.659847812397d, 0.789474056844d, 0.93237488804d, 1d },
                { 1d, 1d, 0.93237488804d, 0.904920910456d, 0.93237488804d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d }
            };
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    Assert.AreEqual(expectedValuesAfterExtraction[x, y], oilfieldMap.Values[x, y], 0.0001d, "Wrong value at " + x + ", " + y);
                }
            }
        }
        [TestMethod]
        public void TestOilMapUpdatedOnExtraction3Radius()
        {
            OilfieldMap oilfieldMap = new OilfieldMap(new double[,] {
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d },
                { 1d, 1d, 1d, 1d, 1d, 1d, 1d } },
                curve, 3, 500);
            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));

            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));

            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));

            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));

            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));

            Console.WriteLine(oilfieldMap.BarrelsExtractedDuringTimePeriod(3, 3, 24 * 60 * 60, out _));
            Print2DArray(oilfieldMap.Values);
        }
        public void TestRealData()
        {
            OilLayerValues oilLayerValues = System.Text.Json.JsonSerializer.Deserialize<OilLayerValues>(System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Oil Layer Values.json")));
            OilfieldMap oilfieldMap = new OilfieldMap(new OilLayerValuesGetter(oilLayerValues), null, curve, 6, 100);
            oilfieldMap.Sigma = 3;
            var image = new Bitmap(25, 25);// oilLayerValues.Width, oilLayerValues.Height);
            int offsetX = 119;
            int offsetY = 106;
            int imageWidth = 25;
            int imageHeight = 25;
            int pumpX = offsetX + 14;
            int pumpY = offsetY + 9;
            double max = oilfieldMap.Curve.RateGivenOil(oilfieldMap[pumpX, pumpY]);

            int barrels = 250;
            int cumulativeBarrels = 0;
            int frames = 25;
            OilmapImageHelper.ImageAfterBarrels(image, oilfieldMap, offsetX, offsetY, imageWidth, imageHeight, pumpX, pumpY, max, 0, ref cumulativeBarrels);
            for (int i = 0; i < frames; i++)
            {
                OilmapImageHelper.ImageAfterBarrels(image, oilfieldMap, offsetX, offsetY, imageWidth, imageHeight, pumpX, pumpY, max, barrels, ref cumulativeBarrels);
            }
        }
        /// <summary>
        /// Test the constructor for ValueChange works
        /// </summary>
        [TestMethod]
        public void TestValueChangeConstructor()
        {
            ValueChange valueChange = new ValueChange(2, 3, 0.4d);
            Assert.AreEqual(2, valueChange.X);
            Assert.AreEqual(3, valueChange.Y);
            Assert.AreEqual(0.4d, valueChange.NewValue);
        }
        /// <summary>
        /// Test OilfieldMap constructor reads initial map data from synchroniser object
        /// </summary>
        [TestMethod]
        public void TestConstructorFromSynchroniser()
        {
            double[,] values = new double[,] {
                { 1d, 1d, 1d },
                { 1d, 1d, 1d,},
                { 1d, 1d, 1d },
                { 1d, 1d, 1d },
            };
            Mock<IOilfieldMapGetter> mockGetter = new Mock<IOilfieldMapGetter>();
            mockGetter.Setup(getter => getter.GetValues()).Returns(values);
            Mock<IOilfieldMapSetter> mockSynchroniser = new Mock<IOilfieldMapSetter>();
            OilfieldMap oilfieldMap = new OilfieldMap(mockGetter.Object, mockSynchroniser.Object, curve, 2, 1d);
            Assert.AreEqual(curve, oilfieldMap.Curve);
            Assert.AreEqual(2, oilfieldMap.DepletionRadius);
            Assert.AreEqual(1d, oilfieldMap.ProductionRateHalfLife);
            Assert.AreEqual(4, oilfieldMap.Width);
            Assert.AreEqual(3, oilfieldMap.Height);
            Assert.AreEqual(values, oilfieldMap.Values);

            mockGetter = new Mock<IOilfieldMapGetter>();
            mockGetter.Setup(getter => getter.GetValues()).Returns((double[,])null);
            oilfieldMap = new OilfieldMap(mockGetter.Object, mockSynchroniser.Object, curve, 1, 2.5d);
            Assert.AreEqual(mockSynchroniser.Object, oilfieldMap.MapSynchroniser);
            Assert.AreEqual(curve, oilfieldMap.Curve);
            Assert.AreEqual(1, oilfieldMap.DepletionRadius);
            Assert.AreEqual(2.5d, oilfieldMap.ProductionRateHalfLife);
            Assert.AreEqual(0, oilfieldMap.Width);
            Assert.AreEqual(0, oilfieldMap.Height);
            Assert.IsNotNull(oilfieldMap.Values);
            Assert.AreEqual(0, oilfieldMap.Values.Length);
        }
        [TestMethod]
        public void TestConstructorFromData()
        {
            double[,] values = new double[,] {
                { 1d, 1d, 1d },
                { 1d, 1d, 1d,},
                { 1d, 1d, 1d },
                { 1d, 1d, 1d },
            };
            OilfieldMap oilfieldMap = new OilfieldMap(values, curve, 3, 1d);
            Assert.AreEqual(null, oilfieldMap.MapSynchroniser);
            Assert.AreEqual(curve, oilfieldMap.Curve);
            Assert.AreEqual(3, oilfieldMap.DepletionRadius);
            Assert.AreEqual(1d, oilfieldMap.ProductionRateHalfLife);
            Assert.AreEqual(4, oilfieldMap.Width);
            Assert.AreEqual(3, oilfieldMap.Height);
            Assert.IsNotNull(oilfieldMap.Values);
            Assert.AreEqual(values, oilfieldMap.Values);
        }
        /// <summary>
        /// Test that the oilfield map notifies the synchroniser of the changes to the map after barrels are extracted
        /// </summary>
        [TestMethod]
        public void TestValuesChangedEventIsFired()
        {
            double[,] values = new double[,] {
                { 1d, 1d, 1d },
                { 1d, 1d, 1d,},
                { 1d, 1d, 1d },
                { 1d, 1d, 1d },
            };
            Mock<IOilfieldMapGetter> mockGetter = new Mock<IOilfieldMapGetter>();
            mockGetter.Setup(getter => getter.GetValues()).Returns(values);

            Mock<IOilfieldMapSetter> mockSynchroniser = new Mock<IOilfieldMapSetter>();
            OilfieldMap oilfieldMap = new OilfieldMap(mockGetter.Object, mockSynchroniser.Object, curve, 2, 1d);

            oilfieldMap.ExtractBarrelsAt(2, 3, 1);
            mockSynchroniser.Verify(synchroniser => synchroniser.UpdateValues(It.IsAny<IEnumerable<ValueChange>>()), Times.Once());
        }
        /// <summary>
        /// Found somewhere on StackExchange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        private static void Print2DArray<T>(T[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
        private static class OilmapImageHelper
        {
            public static System.Func<double, Color> PixelColour = (double oil) => Color.FromArgb((int)(oil * 255), (int)(oil * 255), (int)(oil * 255));

            public static void ImageAfterBarrels(Bitmap image, OilfieldMap oilfieldMap, int left, int bottom, int width, int height, int pumpX, int pumpY, double max, int barrelsToExtract, ref int cumulativeBarrels)
            {
                for (int barrels = 0; barrels < barrelsToExtract; barrels++)
                {
                    oilfieldMap.ExtractBarrelsAt(pumpX, pumpY);
                }
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        double rate = oilfieldMap.Curve.RateGivenOil(oilfieldMap[x + left, y + bottom]);
                        image.SetPixel(x, y, PixelColour(rate / max));
                    }
                }
                cumulativeBarrels += barrelsToExtract;
                image.Save(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Oil Heatmap After " + (cumulativeBarrels) + " Barrels.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}