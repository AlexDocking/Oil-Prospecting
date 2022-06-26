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

namespace OilProspecting
{
    public static class OilMap
    {
        public static WorldLayer OilfieldWorldLayer { get => WorldLayerManager.Obj.GetLayer("Oilfield"); }
        public static readonly string DataFileFolder = "Mods/UserCode/Oil Prospecting";
        public static readonly string DataFileName = "Oil Layer Values.json";
        public static void HideOilLayer(User user)
        {
            if (OilfieldWorldLayer == null)
            {
                user.MsgLocStr("This command is broken as the oil layer name has changed");
                return;
            }
            WriteOilLayerValuesToFile();
            WipeOilLayer();
            user.MsgLocStr("Hid oil values. Keep the file " + Path.Combine(DataFileFolder, DataFileName) + " safe because you will need it to put the values back in the game");
        }

        public static void ShowOilLayer(User user)
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), DataFileFolder, DataFileName)))
            {
                user.MsgLocStr("Couldn't find the data file at " + Path.Combine(DataFileFolder, DataFileName) + ". Have you used <b>/hideoillayer</b> to save the oil layer data?");
                return;
            }
            OilLayerValues oilLayerValues;
            try
            {
                oilLayerValues = ReadOilLayerValuesFromFile();
            }
            catch
            {
                user.MsgLocStr("Couldn't read " + Path.Combine(DataFileFolder, DataFileName) + ". Most likely it is in the wrong format");
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
        private static float[] GetOilLayerValues()
        {
            WorldLayer OilfieldWorldLayer = WorldLayerManager.Obj.GetLayer("Oilfield");
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
            WorldLayer oilLayer = WorldLayerManager.Obj.GetLayer("Oilfield");
            OilLayerValues oilLayerValues = new OilLayerValues();
            oilLayerValues.Values = GetOilLayerValues();
            oilLayerValues.Width = oilLayer.Width;
            oilLayerValues.Height = oilLayer.Height;
            FileManager<OilLayerValues>.WriteTypeHandledToFile(oilLayerValues, DataFileFolder, DataFileName);
        }
    }
}