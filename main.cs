using System;
using System.Collections.Generic;
using System.Globalization;
using System.Transactions;
//first number determines the actual mode, second is whether dev mode is enabled
double[] mode = { 0, 0 };
//holds the user input as a string so it can be parsed
string userInput = "";
//holds the base weight for boars
double[] baseWeight = { 825, 100, 40, 20, 10, 4, 1 };

start();

void start()
{
    Console.WriteLine("What mode do you want to use? Please enter \"blessings\", \"serums\", or \"gifts\". Type \"help\" for an explanation of the modes. ");
    userInput = Console.ReadLine();
    //looks at user input and sends program to correct mode
    switch (userInput)
    {
        case ("blessings"):
            mode[0] = 0;
            blessingsMode();
            break;
        case ("serums"):
            mode[0] = 1;
            serumMode();
            break;
        case ("gifts"):
            mode[0] = 2;
            giftMode();
            break;
        case ("help"):
            Console.WriteLine();
            Console.WriteLine("Blessings Mode will simulate how likely each rarity of boar is with a chosen amount of blessings and miracles.");
            Console.WriteLine();
            Console.WriteLine("Serums Mode will simulate how likely it is that a boar will clone with a chosen amount of cloning serums.");
            Console.WriteLine();
            Console.WriteLine("Gifts mode will tell you how long it will take to get a certain number of items from gifts.");
            start();
            break;

        default:
            Console.WriteLine("Your input is not recognized. Please enter a recogized input.");
            start();
            break;
    }
}

void blessingsMode()
{
    double blessings = 0;
    double charms = 0;
    //stores the combined weight of all boars
    double combinedWeight = 0;
    //stores a number between 0 and 1, determines chance for extra boars
    double[] chance = { 0, 0, 0 };
    //holds the modified weights
    double[] modWeights = { 0, 0, 0, 0, 0, 0, 0, 0 };
    //holds the chance to get a rarity as a percent, [0] is common, [6] is divine, [7] is truth, [8] is common from first extra boar [14] is divine from first extra boar, etc.
    double[] percentChance = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    //stores the chance to get at least one, two, three, or four of a boar, [0-6] is 1, [7-13] is 2, etc.
    double[] atLeastOne = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    List<double> finalChances;
    //asks user for profile information
    Console.WriteLine();
    Console.WriteLine("How many blessings do you have?");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out blessings))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        blessingsMode();

    }
    Console.WriteLine("How many miracle charms do you have? (If you have already used your miracles and they are included in the blessings count that was given, please enter 0.)");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out charms))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        blessingsMode();
    }

    //calculates blessings after charms
    blessings = blessingCalc(blessings, charms);
    //calculates extra boar chance after being affected by blessings
    chance = chanceCalc(blessings, chance);
    //finds new weights
    modWeights = weightCalc(blessings, modWeights);
    //gets the combined weight of modified weights, used as divisor
    for(int i = 0; i < modWeights.Length; i++)
    {
        combinedWeight += modWeights[i];
    }
    //calculates boar chances
    percentChance = boarCalc(combinedWeight, blessings, modWeights, percentChance, chance);
    //chance to get x amount of any rarity
    finalChances = boarCalcAdvanced(percentChance);
    //prints info to user
    printBlessingInfo(finalChances, percentChance, blessings);
}

//calculates blessings after charms
double blessingCalc(double blessings, double miracles)
{
    blessings += 1;
    double blessingsIncrease = 0;
    for (int i = 0; i < miracles; i++)
    {
        //increases blessings by 10%
        blessingsIncrease = blessings * 0.1;
        //sets increase to 1000 if over 1000
        if (blessingsIncrease > 1000)
        {
            blessings += (miracles - i) * 1000;
            break;
        }
        else
        //rounds
        {
            blessings = Math.Ceiling(blessings + blessingsIncrease);
        }
    }

    return blessings;
}

//calculates extra boar chance after being affected by blessings
double[] chanceCalc(double blessings, double[] chance)
{
    //calculates first boar chance
    chance[0] = blessings / 1000;
    if (chance[0] > 1)
    {
        chance[0] = 1;
    }
    //calculates second boar chance
    chance[1] = blessings / 10000;
    if (chance[1] > 1)
    {
        chance[1] = 1;
    }
    //calculates third boar chance
    chance[2] = blessings / 100000;
    if (chance[2] > 1)
    {
        chance[2] = 1;
    }
    return chance;
}


//finds the new weight values after blessings
double[] weightCalc(double blessings, double[] modWeights)
{
    //holds the combined value for everything in modweights
    double combinedWeights = 0;
    //loops over once for every rarity, 0 is common and 6 is divine
    for (int j = 0; j < 7; j++)
    {
        //applies blessings to the weights
        modWeights[j] = baseWeight[j] * (Math.Atan(((blessings - 1) * baseWeight[j]) / 25000) * ((825 - baseWeight[j]) / baseWeight[j]) + 1);
        combinedWeights += modWeights[j];
    }
    //handles truth chance
    double truthBlessings = blessings / 10000000;
    modWeights[7] = combinedWeights * truthBlessings;
    return modWeights;
}

//calculates boar chances 
double[] boarCalc(double combinedWeight, double blessings, double[] modWeight, double[] percentChance, double[] chance){
    //gets the chance of a boar as a percent
    for (int j = 0; j < 8; j++) { 
        percentChance[j] = modWeight[j] / combinedWeight * 100;
        Math.Round(percentChance[j], 4);
    }
    //gets the chance of a boar from extra boars as a percent
    for (int j = 8; j < 16; j++)
    {
        percentChance[j] = percentChance[j-8] * chance[0];
        Math.Round(percentChance[j], 4);
    }
    for (int j = 16; j < 24; j++)
    {
        percentChance[j] = percentChance[j - 16] * chance[1];
        Math.Round(percentChance[j], 4);
    }
    for (int j = 24; j < 32; j++)
    {
        percentChance[j] = percentChance[j - 24] * chance[2];
        Math.Round(percentChance[j], 4);
    }
    return percentChance;
}

//chance to get x amount of any rarity
List<double> boarCalcAdvanced(double[] percentChance)
{ 
    //handles with path the program is calculating
    double pathID = 0;
    //stores the rarity the program is calculating
    int rarityID = 0;
    //total chance of getting the desired boars
    double totalChance = 0;
    //stores values before being assigned to finalChances
    double[] rawChances = { 0, 0, 0, 0, 0 };
    //stores final values
    List<double> finalChances = new List<double>();
    while (rarityID <= 7)
    {
        switch (pathID)
        {
            case 0:
                //0000
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[0] += totalChance;
                break;
            case 1:
                //0001
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= ( percentChance[rarityID + 24]);
                rawChances[1] += totalChance;
                break;
            case 2:
                //0010
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[1] += totalChance;
                break;
            case 3:
                //0011
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 4:
                //0100
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[1] += totalChance;
                break;
            case 5:
                //0101
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 6:
                //0110
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 7:
                //0111
                totalChance = 100 - percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[3] += totalChance;
                break;
            case 8:
                //1000
                totalChance = percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[1] += totalChance;
                break;
            case 9:
                //1001
                totalChance = percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 10:
                //1010
                totalChance = percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 11:
                //1011
                totalChance = percentChance[rarityID];
                totalChance *= (100 - percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[3] += totalChance;
                break;
            case 12:
                //1100
                totalChance = percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[2] += totalChance;
                break;
            case 13:
                //1101
                totalChance = percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (100 - percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[3] += totalChance;
                break;
            case 14:
                //1110
                totalChance = percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (100 - percentChance[rarityID + 24]);
                rawChances[3] += totalChance;
                break;
            case 15:
                //1111
                totalChance = percentChance[rarityID];
                totalChance *= (percentChance[rarityID + 8]);
                totalChance *= (percentChance[rarityID + 16]);
                totalChance *= (percentChance[rarityID + 24]);
                rawChances[4] += totalChance;
                break;
        }
        pathID++;
        //resets pathID, increases rarity, and writes rawChances to finalChances
        if(pathID == 16)
        {
            pathID = 0;
            rarityID++;
            for(int i = 0; i < 5; i++)
            {
                finalChances.Add(rawChances[i]);
                rawChances[i] = 0;
            }
        }
    }
    return finalChances;
}

//prints info to user
void printBlessingInfo(List<double> finalChances, double[] percentChance, double blessings)
{
    //counts how many lines have been printed
    double count = 0;
    //needed for a switch statement
    double countMod = 0;
    //needed as an idex
    int countInt = 0;
    string[] rarityNames = { "common", "uncommon", "rare", "epic", "legendary", "mythic", "divine", "truth"};
    //writes how many blessings the user will have
    Console.WriteLine();
    Console.WriteLine("You will have " + blessings + " total blessings after using charms.");
    Console.WriteLine();
    //writes boar chances
    foreach (double finalChance in finalChances)
    {
        countMod = count % 5;
        countInt = Convert.ToInt32(Math.Floor(count / 5));
        Console.WriteLine("Your chance to get " + countMod + " " + rarityNames[countInt] + " boar(s) is " + Math.Round(finalChance / 1000000, 4) + "%");
        switch(countMod)
        {
            case (0):
                Console.WriteLine("Your chance to get at least one " + rarityNames[countInt] + " boar(s) is " + Math.Round(100 - (finalChance / 1000000), 4) + "%");
                break;
            case (4): 
                Console.WriteLine();
                break;
        }
        count++;
    }
    start();
}

//handles serum mode
void serumMode()
{
    //holds the rarity the user is trying to clone
    double rarity = 0;
    //amount of clones being attempted
    double numClones = 0;
    //chance of succeeding to clone a certain rarity
    double successChance = 0;
    //number of wanted successes
    double numSuccessGoal = 0;
    //chance of hitting the goal amount
    double goalChance = 0;
    Console.WriteLine("What rarity are you attempting to clone? Use either the full rarity name or the first letter.");
    userInput = Console.ReadLine();
    switch(userInput)
    {
        case ("w"):
        case ("wicked"):
            rarity = 0;
            successChance = 0.01;
            break;
        case ("c"):
        case ("common"):
            rarity = 1;
            successChance = 1;
            break;
        case ("u"):
        case ("uncommon"):
            rarity = 2;
            successChance = 0.5;
            break;
        case ("r"):
        case ("rare"):
            rarity = 3;
            successChance = 0.2;
            break;
        case ("e"):
        case ("epic"):
            rarity = 4;
            successChance = 0.1;
            break;
        case ("l"):
        case ("legendary"):
            rarity = 5;
            successChance = 0.05;
            break;
        case ("m"):
        case ("mythic"):
            rarity = 6;
            successChance = 0.02;
            break;
        case ("d"):
        case ("divine"):
            rarity = 7;
            successChance = 0.005;
            break;
        case ("i"):
        case ("immaculate"):
            rarity = 8;
            successChance = 0.001;
            break;
        case ("s"):
        case ("special"):
        case ("uc"):
        case ("upper council"):
            Console.WriteLine("Specials and Upper Councils cannot be cloned.");
            serumMode();
            break;
        default:
            Console.WriteLine("Rarity not recognized. Please input a recognized rarity.");
            serumMode();
            break;

    }

    Console.WriteLine("How many serums are you using?");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out numClones))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        serumMode();
    }

    Console.WriteLine("How many successes do you want?");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out numSuccessGoal))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        serumMode();
    }

    //calculates the chance to get the desired number of successes from X number of serums
    goalChance = serumCalc(rarity, successChance, numClones, numSuccessGoal);
    //prints serum mode info
    printSerumInfo(goalChance, numClones, numSuccessGoal);
}


//calculates the chance to get the desired number of successes from X number of serums
double serumCalc(double rarity, double successChance, double trials, double numSuccessGoal)
{
    //holds final chance
    double finalChance = 0;

    //binomial formula
    if(numSuccessGoal != 0) { 
        //calculates chance of clone x times or more when the user doesnt want the chance of 0 serums 
        for(double i = numSuccessGoal; i < trials; i++)
        {
            finalChance += binomialFormula(successChance, i, trials);
        }
    }
    else
    {
        //calculates chance to clone 0 times
        finalChance = binomialFormula(successChance, numSuccessGoal, trials);
    }
    finalChance *= 100;
    finalChance = Math.Round(finalChance, 4);
    return finalChance;
}

void printSerumInfo(double goalChance, double trials, double numSuccessGoal)
{
    if(numSuccessGoal != 0) { 
        Console.WriteLine("You have a " + goalChance + "% chance to clone that boar " + numSuccessGoal + " times or more when using " + trials + " serums.");
    }
    else
    {
        Console.WriteLine("You have a " + goalChance + "% chance to clone that boar " + numSuccessGoal + " times when using " + trials + " serums.");
    }
    start();
}

void giftMode()
{
    //how common the item is
    double rarity = 0;
    //type of item wanted
    string itemType = "";
    //amount of item wanted
    double itemNumber = 0;
    //number of gifts
    double giftNumber = 0;
    double finalChance = 0;
    //holds whether boarmas is active
    bool isCristmas = false;
    //asks for wanted item and quantity
    Console.WriteLine("Is it boarmas? Y or N");
    userInput = Console.ReadLine();
    switch (userInput)
    {
        case ("Y"):
        case ("y"):
            isCristmas = true;
            break;
        case ("N"):
        case ("n"):
            isCristmas = false;
            break;
        default:
            Console.WriteLine("Your input is not recognized. Please enter a recogized input.");
            giftMode();
            break;
    }
    Console.WriteLine("What item do you want? Use a powerup name, \"random boar\", or the name of any random boar. Bucks are not currently availible.");
    userInput = Console.ReadLine();
    switch (userInput)
    {
        case ("bucks"):
            Console.WriteLine("Due to the complexity of this, bucks will not be availible until 1.2, if ever.");
            //itemType = 0;
            //rarity = 0.4;
            break;
        case ("serums"):
            itemType = "serum(s)";
            if(isCristmas == false)
            {
                rarity = 0.09975;
            }
            else
            {
                rarity = 0.0975;
            }
            break;
        case ("miracle charms"):
        case ("charms"):
            itemType = "charm(s)";
            if (isCristmas == false)
            {
                rarity = 0.089775;
            }
            else
            {
                rarity = 0.8775;
            }
            break;
        case ("transmutation charges"):
        case ("charges"):
            itemType = "charge(s)";
            if (isCristmas == false)
            {
                rarity = 0.009975;
            }
            else
            {
                rarity = 0.00975;
            }
            break;
        case ("random boar"):
        case ("boar"):
            itemType = "boar(s)";
            rarity = askForRarity();
            break;
        case ("boarnderwear"):
        case ("underwear"):
            itemType = "boarnderwear(s)";
            if (isCristmas == false)
            {
                rarity = 0.0005;
            }
            else
            {
                rarity = 0.001;
            }
            break;
        case ("santa"):
            itemType = "santa(s)";
            rarity = 0.004;
            break;

        default:
            Console.WriteLine("Your input is not recognized. Please enter a recogized input.");
            giftMode();
            break;
    }
    Console.WriteLine("How many do you want?");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out itemNumber))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        giftMode();
        Environment.Exit(113);
    }
    Console.WriteLine("How many gifts do you have?");
    userInput = Console.ReadLine();
    if (!double.TryParse(userInput, out giftNumber))
    {
        Console.WriteLine();
        Console.WriteLine("Please enter a positive integer.");
        giftMode();
    }
    finalChance = giftCalc( rarity, itemNumber, giftNumber);
    printGiftInfo(itemType, finalChance, giftNumber, itemNumber);
}

//gets the chance to get any rarity boar from a gift
double askForRarity()
{
    double rarity = 0;
    Console.WriteLine("What rarity of boar do you want? Use either the full rarity name, just the first letter, or any.");
    userInput = Console.ReadLine();
    switch (userInput)
    {
        case ("c"):
        case ("common"):
            rarity = 0.35;
            break;
        case ("u"):
        case ("uncommon"):
            rarity = 0.04;
            break;
        case ("r"):
        case ("rare"):
            rarity = 0.016;
            break;
        case ("e"):
        case ("epic"):
            rarity = 0.008;
            break;
        case ("l"):
        case ("legendary"):
            rarity = 0.004;
            break;
        case ("m"):
        case ("mythic"):
            rarity = 0.0016;
            break;
        case ("d"):
        case ("divine"):
            rarity = 0.0004;
            break;
        case ("any"):
            rarity = 0.4;
            break;
        default:
            Console.WriteLine("Your input is not recognized. Please enter a recogized input.");
            askForRarity();
            Environment.Exit(113);
            break;
    }
    return rarity;
}

//calcs the chance to get the desired outcome
double giftCalc(double rarity, double itemNumber, double giftNumber)
{
    double finalChance = 0;
    finalChance = binomialFormula(rarity, itemNumber, giftNumber);
    return finalChance;
}

void printGiftInfo(string itemType, double finalChance, double giftNumber, double itemNumber)
{
    finalChance *= 100;
    finalChance = Math.Round(finalChance, 4);
    Console.WriteLine("Your chance to get " + itemNumber + " " + itemType + " from " + giftNumber + " gifts is " + finalChance + "%");
    start();
}

//calcs chances
double binomialFormula(double successChance, double numSuccessGoal, double trials)
{
    double finalChance = 0;
    double trialsFact = 0;
    double numFails = trials - numSuccessGoal;
    double numFailsFact = 0;
    double goalFact = 0;
    double failChance = 1 - successChance;
    trialsFact = factorial(trials);
    numFailsFact = factorial(numFails);
    goalFact = factorial(numSuccessGoal);
    finalChance = (trialsFact / (numFailsFact * goalFact)) * Math.Pow(successChance, numSuccessGoal) * (Math.Pow(failChance, trials - numSuccessGoal));
    return finalChance;
}

//calcs factorials
double factorial(double num)
{
    double numFact = 1;
    while (num > 0)
    {
        numFact *= num;
        num--;
    }
    return numFact;
}