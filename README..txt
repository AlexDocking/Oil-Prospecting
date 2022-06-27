Mod for Eco 9.5 that shows and hides the oil layer via admin commands, so that players don't claim the best yields right at the start of the season
It saves the values to file and sets the in-game layer to zeros, thereby hiding the data and nerfing any pump jacks until the admin decides to reinstate the initial values.

Dependencies:
EM Framework (v3.2.1)

Installation:
Drop the "Oil Prospecting" folder into Mods/UserCode

Commands:
/hideoillayer - save the oilfield data as a json file and sets the in-game layer to 0% everywherre. It may take a few minutes for the changes to appear

/showoillayer - read the json file and write the values back into the game. Again it may take a few minutes to update

Note:
Even when the oil level is zero, pump jacks can still pump oil at 15 minutes per barrel. It's just how the game works.
If the data file gets lost or corrupted you can generate a new world of the same size and copy the oilfield map from that, as it is independent of land mass or any other factors

Author: AlexDocking
