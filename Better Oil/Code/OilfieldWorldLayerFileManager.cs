using Eco.EM.Framework.FileManager;
using Eco.Gameplay.Players;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using Eco.World;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterOil
{
    public static class OilfieldWorldLayerFileManager
    {
        public static WorldLayer OilfieldWorldLayer { get => WorldLayerManager.Obj.GetLayer("Oilfield"); }
        public static readonly string DataFileFolder = "Mods/UserCode/Oil Prospecting";
        public static readonly string DataFileName = "Oil Layer Values.json";
        private static string DataFileRelativeLocation
        {
            get
            {
                return Path.Combine(DataFileFolder, DataFileName);
            }
        }
        /// <summary>
        /// Writes the oilfield data to file and sets the in-game oil to zero everywhere
        /// </summary>
        /// <param name="user">The user who issued the command, to send error/success messages to</param>
        public static void HideOilLayer(User user)
        {
            if (OilfieldWorldLayer == null)
            {
                user.MsgLocStr("This command is broken as the oil layer name has changed");
                return;
            }
            if (OilValuesFileExists())
            {
                user.MsgLocStr("There is already a saved oilfield file. Using the command would overwrite this file. If you are sure you want to do this remove the file at " + DataFileRelativeLocation + " and try again");
                return;
            }
            WriteOilLayerValuesToFile();
            WipeOilLayer();
            user.MsgLocStr("Hid oil values. Keep the file " + DataFileRelativeLocation + " safe because you will need it to put the values back in the game");
        }
        /// <summary>
        /// Read the oil data from the file and write it back to the game
        /// </summary>
        /// <param name="user">The user who issued the command, to send error/success messages to</param>
        public static void ShowOilLayer(User user)
        {
            if (!OilValuesFileExists())
            {
                user.MsgLocStr("Couldn't find the data file at " + Path.Combine(DataFileFolder, DataFileName) + "\nHave you used <b>/hideoillayer</b> to save the original oil layer data?");
                return;
            }
            OilLayerValues oilLayerValues;
            try
            {
                oilLayerValues = ReadOilLayerValuesFromFile();
            }
            catch
            {
                user.MsgLocStr("Couldn't read " + DataFileRelativeLocation + "\nMost likely it is in the wrong format");
                return;
            }

            int index = 0;
            for (int x = 0; x < oilLayerValues.Width; x++)
            {
                for (int z = 0; z < oilLayerValues.Height; z++)
                {
                    Vector2i worldPos = OilfieldWorldLayer.LayerPosToWorldPos(new Vector2i(x, z));
                    OilfieldWorldLayer.SetAtWorldPos(worldPos, oilLayerValues.Values[index++]);
                }
            }
            OilfieldWorldLayer.DoTick();
            user.MsgLocStr("Restored oil values");
        }
        private static bool OilValuesFileExists()
        {
            return File.Exists(Path.Combine(Directory.GetCurrentDirectory(), DataFileFolder, DataFileName));
        }
        private static float[] GetOilLayerValues()
        {
            List<float> values = new List<float>();

            //Each value in the oil layer corresponds to a 5x5 voxel chunk
            const int voxelsPerEntry = 5;

            for (int x = 0; x < OilfieldWorldLayer.Width; x++)
            {
                for (int z = 0; z < OilfieldWorldLayer.Height; z++)
                {
                    values.Add(OilfieldWorldLayer.GetValue(new LayerPosition(x, z, voxelsPerEntry)));
                }
            }
            return values.ToArray();
        }
        private static OilLayerValues ReadOilLayerValuesFromFile()
        {
            return FileManager<OilLayerValues>.ReadTypeHandledFromFile(DataFileFolder, DataFileName);
        }
        private static void WipeOilLayer()
        {
            for (int x = 0; x < OilfieldWorldLayer.Width; x++)
            {
                for (int z = 0; z < OilfieldWorldLayer.Height; z++)
                {
                    Vector2i worldPos = OilfieldWorldLayer.LayerPosToWorldPos(new Vector2i(x, z));
                    OilfieldWorldLayer.SetAtWorldPos(worldPos, 0f);
                }
            }
            OilfieldWorldLayer.DoTick();
        }
        private static void WriteOilLayerValuesToFile()
        {
            OilLayerValues oilLayerValues = new OilLayerValues();
            oilLayerValues.Values = GetOilLayerValues();
            oilLayerValues.Width = OilfieldWorldLayer.Width;
            oilLayerValues.Height = OilfieldWorldLayer.Height;
            FileManager<OilLayerValues>.WriteTypeHandledToFile(oilLayerValues, DataFileFolder, DataFileName);
        }
    }
}