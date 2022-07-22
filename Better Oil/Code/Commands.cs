using Eco.EM.Framework.FileManager;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Math;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterOil
{
    public class Commands : IChatCommandHandler
    {
        [ChatCommand("Hides the oil layer data by saving it to file and overwriting the in-game oil to zero. You can restore it with <b>/showoillayer</b>", ChatAuthorizationLevel.Admin)]
        public static void HideOilLayer(User user)
        {
            OilfieldWorldLayerFileManager.HideOilLayer(user);
        }

        [ChatCommand("Show the oil layer by copying the values from the file back into the game", ChatAuthorizationLevel.Admin)]
        public static void ShowOilLayer(User user)
        {
            OilfieldWorldLayerFileManager.ShowOilLayer(user);
        }
    }
}