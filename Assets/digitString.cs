using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class digitString : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public MeshRenderer inputResult;
    public MeshRenderer initialString;

    public KMSelectable[] buttons;
    public KMSelectable buttonClear;
    public KMSelectable buttonSubmit;

    //public KMRuleSeedable RuleSeedable;
    string[] letterRules = new string[37]
    {
        "0: A three-digit multiple of 100.",
"1: A two-digit multiple of 13.",
"2: Three even digits in a row.",
"3: 33, 66, or 99.",
"4: 65, 16, 47, 73, or 90.",
"5: A five-digit sequence where the first through fourth digits are different, and the fifth digit is the same as the first.",
"6: Three consecutive digits that add up to exactly 6.",
"7: Two consecutive digits with a difference of 7.",
"8: A three-digit sequence made up of just 2s, 4s, and 8s.",
"9: Two consecutive digits with a sum of 9.",
"A: A five-digit sequence with no 0's, and where every digit is different.",
"B: A three-digit sequence starting in 1 and ending in 2.",
"C: Three consecutive digits 7 or greater.",
"D: A four-digit sequence where each digit is greater than the one before it.",
"E: Three consecutive digits that add up to exactly 13.",
"F: Four consecutive odd digits.",
"G: Two consecutive digits, the second is at least 3 one higher than the first.",
"H: A 7 or 9, followed by an even digit.",
"I: Three consecutive digits, exactly two are 1 and/or 7.",
"J: A three-digit sequence which uses exactly three digits out of 2, 3, 5, and 9.",
"K: A two-digit multiple of 15.",
"L: Four consecutive digits which add up to exactly 14.",
"M: A four digit number from 5930 to 6075, inclusive.",
"N: Two consecutive digits that are the same.",
"O: A five digit sequence where each digit is either even or 7.",
"P: A 2 or 4, followed by an odd digit.",
"Q: Three consecutive digits which add up to 23 or more.",
"R: Two consecutive digits, both in the bomb's serial number.",
"S: A four-digit sequence where each digit is less than the one before it.",
"T: Four consecutive digits which add up to either more than 28 or less than 8.",
"U: Three consecutive digits, the first and third match.",
"V: Three consecutive digits from 0 to 2.",
"W: Four consecutive even digits.",
"X: A three-digit number from 470 to 485, inclusive.",
"Y: Five consecutive digits without a 3 or 6.",
"Z: Three consecutive digits, at least two of which are 2 and/or 5.",
"Repeat: (Current serial number position) times 12 or 15."
    };

    string[] alphabet = new string[26]
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

    string[] theCharacters = new string[36]
    { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
      "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

    int[] theNumbers = new int[36] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                        25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35 };


    bool[] isRepeat = new bool[6];


    bool pressedAllowed = false;

    // TWITCH PLAYS SUPPORT
    //int tpStages; This one is not needed for this module
    // TWITCH PLAYS SUPPORT

    string currentInput = "";
    string shownString = "";
    string bombSerial = "";
    string theSign = "";
    long currentInputAsNumber;
    long moduleSolution;
    int firstNumber;
    int secondNumber;
    int currentRound;
    int uniques;

    bool foundAnswer = false;
    bool isSolved = false;
    
    void Start()
    {
        _moduleId = _moduleIdCounter++;

        Init();
        pressedAllowed = true;
    }

    void Init()
    {
        initialString.GetComponentInChildren<TextMesh>().text = "";
        delegationZone();
        Module.OnActivate += delegate { inputResult.GetComponentInChildren<TextMesh>().text = ""; };

        shownString = "";
        shownString = UnityEngine.Random.Range(1, 10).ToString();
        while (shownString.Length < 7)
        {
            shownString = shownString + UnityEngine.Random.Range(0, 10).ToString();
        }
        shownString = shownString + UnityEngine.Random.Range(1, 10).ToString();
        inputResult.GetComponentInChildren<TextMesh>().text = "";
        //shownString = "54204717"; //debug
        initialString.GetComponentInChildren<TextMesh>().text = shownString;

        doClear();
        bombSerial = Bomb.GetSerialNumber();
        //bombSerial = "HT2GZ6"; //debug

        Debug.LogFormat("[Digit String #{0}] The bomb's serial number is {1}. The eight-digit number is {2}.", _moduleId, bombSerial, shownString);
        currentInput = "";
        for (int cn = 0; cn < 6; cn++)
        {
            currentRound = cn + 1;
            isRepeat[cn] = false;
            for (int ck = 0; ck < 6; ck++)
            {
                if (cn != ck)
                {
                    if (bombSerial.Substring(cn, 1) == bombSerial.Substring(ck, 1))
                    {
                        isRepeat[cn] = true;
                        ck = 5;
                    }
                }
            }
            if (isRepeat[cn])
            {
                if (cn == 0 || cn == 4)
                {
                    theSign = "x";
                }
                else if (cn == 1)
                {
                    theSign = ">";
                }
                else if (cn == 3)
                {
                    theSign = "<";
                }
                else
                {
                    theSign = "+";
                }
                Debug.LogFormat("[Digit String #{0}] Trying serial number character #{1}, which is '{2}'{3}. The sign will be {4} if there is a match on rule {5}"
                    , _moduleId, cn + 1, bombSerial.Substring(cn, 1), isRepeat[cn] ? ", a repeat" : "", theSign, letterRules[36]);
                characterZone(36);
            }
            else
            {
                for (int ln = 0; ln < 36; ln++)
                {
                    if (bombSerial.Substring(cn, 1) == theCharacters[ln])
                    {
                        if (cn == 0 || cn == 4)
                        {
                            theSign = "x";
                        }
                        else if (cn == 1)
                        {
                            theSign = ">";
                        }
                        else if (cn == 3)
                        {
                            theSign = "<";
                        }
                        else
                        {
                            theSign = "+";
                        }
                        Debug.LogFormat("[Digit String #{0}] Trying serial number character #{1}, which is '{2}'{3}. The sign will be {4} if there is a match on rule {5}"
                            , _moduleId, cn + 1, bombSerial.Substring(cn, 1), isRepeat[cn] ? ", a repeat" : "", theSign, letterRules[ln]);
                        characterZone(ln);
                        ln = 99;
                    }
                }
            }

            if (foundAnswer)
            {

                Debug.LogFormat("[Digit String #{0}] Found a match. First number is {1}, sign is {2}, and second number is {3}.", _moduleId, firstNumber, theSign, secondNumber);

                cn = 5;

            }

        }
        if (!foundAnswer)
        {
            var hasMatch = false;
            uniques = 1;
            for (int snn = 1; snn < 6; snn++)
            {
                for (int rnn = 0; rnn < snn; rnn++)
                {
                    if (bombSerial.Substring(rnn, 1) == bombSerial.Substring(snn, 1))
                    {
                        hasMatch = true;
                        rnn = snn;
                    }
                }

                if (!hasMatch)
                {
                    uniques++;
                }
                hasMatch = false;
            }
            theSign = "+";
            foundAnswer = true;
            firstNumber = Int32.Parse(shownString.Substring(0, uniques));
            secondNumber = Int32.Parse(shownString.Substring(uniques + 1, 7 - uniques));
            Debug.LogFormat("[Digit String #{0}] No match found using serial number characters, replacing digit number {1} + 1, or {2}, with a plus sign. First number is {3}, sign is +, and second number is {4}."
                , _moduleId, uniques, uniques + 1, firstNumber, secondNumber);
        }
        if (theSign == ">")
        {
            if (firstNumber > secondNumber)
            {
                moduleSolution = 1;
            }
            else
            {
                moduleSolution = 0;
            }
            Debug.LogFormat("[Digit String #{0}] {1} {2} {3}. This is {4}, so the answer is {5}.", _moduleId, firstNumber, theSign, secondNumber,
                firstNumber > secondNumber ? "true" : "false", moduleSolution);
        }
        else if (theSign == "<")
        {
            if (firstNumber < secondNumber)
            {
                moduleSolution = 1;
            }
            else
            {
                moduleSolution = 0;
            }
            Debug.LogFormat("[Digit String #{0}] {1} {2} {3}. This is {4}, so the answer is {5}.", _moduleId, firstNumber, theSign, secondNumber,
                firstNumber < secondNumber ? "true" : "false", moduleSolution);
        }
        else if (theSign == "+")
        {
            moduleSolution = firstNumber + secondNumber;
            Debug.LogFormat("[Digit String #{0}] {1} {2} {3} = {4}", _moduleId, firstNumber, theSign, secondNumber, moduleSolution);
        }
        else
        {
            moduleSolution = firstNumber * secondNumber;
            Debug.LogFormat("[Digit String #{0}] {1} {2} {3} = {4}", _moduleId, firstNumber, theSign, secondNumber, moduleSolution);
        }
        pressedAllowed = true;
    }

    void doNumber(int n)
    {
        currentInput = currentInput + "" + n;
        if (currentInput.Length > 8)
        {
            currentInput = currentInput.Substring(1, 8);
        }
        currentInputAsNumber = Int64.Parse(currentInput);
        inputResult.GetComponentInChildren<TextMesh>().text = currentInput;
    }

    void doClear()
    {
        currentInput = "";
        currentInputAsNumber = 0;
        inputResult.GetComponentInChildren<TextMesh>().text = "";
    }

    void doSubmit()
    {
        if (pressedAllowed)
        {
            if (Int64.Parse(currentInput) == moduleSolution)
            {
                Debug.LogFormat("[Digit String #{0}] Submitted input of {1} and the expected {2} match, module disarmed!", _moduleId, Int64.Parse(currentInput), moduleSolution);
               
                inputResult.GetComponentInChildren<TextMesh>().text = "DISARMED";
                if (Bomb.GetSolvableModuleNames().Where(x => "Souvenir".Contains(x)).Count() > 0)
                {
                    initialString.GetComponentInChildren<TextMesh>().text = "????????";
                }
                pressedAllowed = false;
                GetComponent<KMBombModule>().HandlePass();
            }
            else
            {
                Debug.LogFormat("[Digit String #{0}] Submitted input of {1} and the expected {2} do not match, that's a strike!", _moduleId, Int64.Parse(currentInput), moduleSolution);
                GetComponent<KMBombModule>().HandleStrike();
            }
        }

    }

    void OnPress()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
    }

    void OnRelease()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if (pressedAllowed)
        {

            return;
        }

    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit an answer with !{0} (submit/s/answer/a) 12345678.";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {

        var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string theError;
        theError = "";
        yield return null;
        if (pieces.Count() == 0)
        {
            theError = "sendtochaterror Not enough arguments! You need to use 'submit/s/answer/a', then one number.";
            yield return theError;
        }
        else if (pieces.Count() == 1 && (pieces[0] == "submit" || pieces[0] == "answer" || pieces[0] == "s" || pieces[0] == "a"))
        {
            theError = "sendtochaterror Not enough arguments! You need a number to submit, e.g. !{0} submit 12345678.";
            yield return theError;

        }
        else if (pieces[0] != "submit" && pieces[0] != "s" && pieces[0] != "answer" && pieces[0] != "a")
        {
            theError = "sendtochaterror Invalid argument! You need to submit/s/answer/a, for example, !{0} submit 12345678.";
            yield return theError;
        }
        else if (pieces.Count() >= 2 && (pieces[0] == "submit" || pieces[0] == "s" || pieces[0] == "answer" || pieces[0] == "a"))
        {
                for (int k = 0; k < pieces[1].Length; k++)
                {
                    if (pieces[1].Substring(k, 1) != "0" && pieces[1].Substring(k, 1) != "1" && pieces[1].Substring(k, 1) != "2" && pieces[1].Substring(k, 1) != "3" &&
                        pieces[1].Substring(k, 1) != "4" && pieces[1].Substring(k, 1) != "5" && pieces[1].Substring(k, 1) != "6" && pieces[1].Substring(k, 1) != "7" &&
                        pieces[1].Substring(k, 1) != "8" && pieces[1].Substring(k, 1) != "9")
                    {
                        
                        theError = "sendtochaterror Invalid character! " + pieces[1].Substring(k, 1) + " is not a digit.";
                        yield return theError;
                    }
                }
            if (theError == "")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                buttonClear.OnInteract();
                for (int l = 0; l < pieces[1].Length; l++)
                {
                    var curDigit = Int16.Parse(pieces[1].Substring(l, 1));
                    yield return new WaitForSeconds(.1f);
                    yield return null;
                    buttons[curDigit].OnInteract();
                }
                yield return new WaitForSeconds(.1f);
                yield return null;
                buttonSubmit.OnInteract();
            }
        }
     }

    void delegationZone()
    {

        buttons[0].OnInteract += delegate () { OnPress(); doNumber(0); buttons[0].AddInteractionPunch(0.2f); return false; };
        buttons[1].OnInteract += delegate () { OnPress(); doNumber(1); buttons[1].AddInteractionPunch(0.2f); return false; };
        buttons[2].OnInteract += delegate () { OnPress(); doNumber(2); buttons[2].AddInteractionPunch(0.2f); return false; };
        buttons[3].OnInteract += delegate () { OnPress(); doNumber(3); buttons[3].AddInteractionPunch(0.2f); return false; };
        buttons[4].OnInteract += delegate () { OnPress(); doNumber(4); buttons[4].AddInteractionPunch(0.2f); return false; };
        buttons[5].OnInteract += delegate () { OnPress(); doNumber(5); buttons[5].AddInteractionPunch(0.2f); return false; };
        buttons[6].OnInteract += delegate () { OnPress(); doNumber(6); buttons[6].AddInteractionPunch(0.2f); return false; };
        buttons[7].OnInteract += delegate () { OnPress(); doNumber(7); buttons[7].AddInteractionPunch(0.2f); return false; };
        buttons[8].OnInteract += delegate () { OnPress(); doNumber(8); buttons[8].AddInteractionPunch(0.2f); return false; };
        buttons[9].OnInteract += delegate () { OnPress(); doNumber(9); buttons[9].AddInteractionPunch(0.2f); return false; };

        buttonClear.OnInteract += delegate () {
            OnPress(); doClear();
            buttonClear.AddInteractionPunch(0.2f); return false;
        };
        buttonSubmit.OnInteract += delegate () { OnPress(); doSubmit(); buttonClear.AddInteractionPunch(0.4f); return false; };

        buttons[0].OnInteractEnded += delegate () { OnRelease(); };
        buttons[1].OnInteractEnded += delegate () { OnRelease(); };
        buttons[2].OnInteractEnded += delegate () { OnRelease(); };
        buttons[3].OnInteractEnded += delegate () { OnRelease(); };
        buttons[4].OnInteractEnded += delegate () { OnRelease(); };
        buttons[5].OnInteractEnded += delegate () { OnRelease(); };
        buttons[6].OnInteractEnded += delegate () { OnRelease(); };
        buttons[7].OnInteractEnded += delegate () { OnRelease(); };
        buttons[8].OnInteractEnded += delegate () { OnRelease(); };
        buttons[9].OnInteractEnded += delegate () { OnRelease(); };

        buttonClear.OnInteractEnded += delegate () { OnRelease(); };
        buttonSubmit.OnInteractEnded += delegate () { OnRelease(); };
        

    }
    /*
    public static class Extensions
    {
        // Fisher-Yates Shuffle
        public static IList<T> shuffle<T>(this IList<T> list, MonoRandom rnd)
        {
            var i = list.Count;
            while (i > 1)
            {
                var index = rnd.Next(i);
                i--;
                var value = list[index];
                list[index] = list[i];
                list[i] = value;
            }

            return list;
        }
    } */
    /*
    void doShuffle()
    {
        var rnd = RuleSeedable.GetRNG();
        if (rnd.Seed == 1)
        {

        }
        else
        {
            
            var numberCount = theNumbers.Length;
            while (numberCount > 1)
            {
                var xyz = rnd.Next(numberCount);
                numberCount--;
                var value = theNumbers[xyz];
                theNumbers[xyz] = theNumbers[numberCount];
                theNumbers[numberCount] = value;
            }
            var theThingy = "";

            for (var i = 0; i < 42; i++)
            {
                //list[i].innerText = theFunctions[theNumbers[i]];
            }
        }
        
    } */
    void characterZone(int cNum)
    {
        //cNum = 27;
        switch (cNum)
        {
            case 0: //Character 0 = 3 digits, multiple of 100
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 3)) % 100 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
            break;
            case 1: //Character 1 = 2 digits, multiple of 13
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 2)) % 13 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 2: //Character 2 = Three even digits in a row.
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) % 2 == 0 && Int32.Parse(shownString.Substring(dn + 1, 1)) % 2 == 0 && Int32.Parse(shownString.Substring(dn + 2, 1)) % 2 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 3: //Character 3 = 33, 66, or 99.
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 2)) == 33 || Int32.Parse(shownString.Substring(dn, 2)) == 66 || Int32.Parse(shownString.Substring(dn, 2)) == 99)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 4: //Character 4 = 65, 16, 47, 73, or 90
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 2)) == 65 || Int32.Parse(shownString.Substring(dn, 2)) == 16 || Int32.Parse(shownString.Substring(dn, 2)) == 47 ||
                        Int32.Parse(shownString.Substring(dn, 2)) == 73 || Int32.Parse(shownString.Substring(dn, 2)) == 90)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 5: //Character 5 = Five digit number, first four are different, fifth matches first
                for (int dn = 1; dn < 3; dn++)
                {
                    if (shownString.Substring(dn, 1) != shownString.Substring(dn + 1, 1) && shownString.Substring(dn, 1) != shownString.Substring(dn + 2, 1) &&
                        shownString.Substring(dn, 1) != shownString.Substring(dn + 3, 1) && shownString.Substring(dn + 1, 1) != shownString.Substring(dn + 2, 1) &&
                        shownString.Substring(dn + 1, 1) != shownString.Substring(dn + 3, 1) && shownString.Substring(dn + 2, 1) != shownString.Substring(dn + 3, 1) &&
                        shownString.Substring(dn, 1) == shownString.Substring(dn + 4, 1))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 5, 3 - dn));
                        dn = 3;
                    }
                }
                break;
            case 6: //Character 6 = Three digits, add up to exactly 6.
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) + Int32.Parse(shownString.Substring(dn + 2, 1)) == 6)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 7: //Character 7 = Two consecutive digits, difference of 7
                for (int dn = 1; dn < 6; dn++)
                {
                    if ( Math.Abs(Int32.Parse(shownString.Substring(dn, 1)) - Int32.Parse(shownString.Substring(dn + 1, 1))) == 7)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 8: //Character 8 = Three-digit sequence, all 2s, 4s, and 8s
                for (int dn = 1; dn < 5; dn++)
                {
                    if ((shownString.Substring(dn, 1) == "2" || shownString.Substring(dn, 1) == "4" || shownString.Substring(dn, 1) == "8") &&
                        (shownString.Substring(dn + 1, 1) == "2" || shownString.Substring(dn + 1, 1) == "4" || shownString.Substring(dn + 1, 1) == "8") &&
                        (shownString.Substring(dn + 2, 1) == "2" || shownString.Substring(dn + 2, 1) == "4" || shownString.Substring(dn + 2, 1) == "8"))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 9: //Character 9 = Two consecutive digits, sum of 9
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) == 9)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 10: //Character A = Five digits, no 0's, all digits different
                for (int dn = 1; dn < 3; dn++)
                {
                    if (shownString.Substring(dn, 1) != shownString.Substring(dn + 1, 1) && shownString.Substring(dn, 1) != shownString.Substring(dn + 2, 1) &&
                        shownString.Substring(dn, 1) != shownString.Substring(dn + 3, 1) && shownString.Substring(dn, 1) != shownString.Substring(dn + 4, 1) &&
                        shownString.Substring(dn + 1, 1) != shownString.Substring(dn + 2, 1) && shownString.Substring(dn + 1, 1) != shownString.Substring(dn + 3, 1) &&
                        shownString.Substring(dn + 1, 1) != shownString.Substring(dn + 4, 1) && shownString.Substring(dn + 2, 1) != shownString.Substring(dn + 3, 1) &&
                        shownString.Substring(dn + 2, 1) != shownString.Substring(dn + 4, 1) && shownString.Substring(dn + 3, 1) != shownString.Substring(dn + 4, 1) &&
                        shownString.Substring(dn, 1) != "0" && shownString.Substring(dn + 1, 1) != "0" && shownString.Substring(dn + 2, 1) != "0" && 
                        shownString.Substring(dn + 3, 1) != "0" && shownString.Substring(dn + 4, 1) != "0")
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 5, 3 - dn));
                        dn = 3;
                    }
                }
                break;
            case 11: //Character B = Three digits, starts in 1 and ends in 2
                for (int dn = 1; dn < 5; dn++)
                {
                    if (shownString.Substring(dn, 1) == "1" && shownString.Substring(dn + 2, 1) == "2")
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 12: //Character C = Three consecutive digits 7 or greater
                {
                    for (int dn = 1; dn < 5; dn++)
                    {
                        if ((shownString.Substring(dn, 1) == "7" || shownString.Substring(dn, 1) == "8" || shownString.Substring(dn, 1) == "9") &&
                            (shownString.Substring(dn + 1, 1) == "7" || shownString.Substring(dn + 1, 1) == "8" || shownString.Substring(dn + 1, 1) == "9") &&
                            (shownString.Substring(dn + 2, 1) == "7" || shownString.Substring(dn + 2, 1) == "8" || shownString.Substring(dn + 2, 1) == "9"))
                        {
                            foundAnswer = true;
                            firstNumber = Int32.Parse(shownString.Substring(0, dn));
                            secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                            dn = 5;
                        }
                    }
                }
                break;
            case 13: //Character D = Four digits, each greater than the one before it
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) < Int32.Parse(shownString.Substring(dn + 1, 1)) &&
                        Int32.Parse(shownString.Substring(dn + 1, 1)) < Int32.Parse(shownString.Substring(dn + 2, 1)) &&
                        Int32.Parse(shownString.Substring(dn + 2, 1)) < Int32.Parse(shownString.Substring(dn + 3, 1)))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 14: //Character E = Three digits which add up to exactly 13
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) + Int32.Parse(shownString.Substring(dn + 2, 1)) == 13)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 15: //Character F = Four consecutive odd digits
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) % 2 == 1 && Int32.Parse(shownString.Substring(dn + 1, 1)) % 2 == 1 &&
                        Int32.Parse(shownString.Substring(dn + 2, 1)) % 2 == 1 && Int32.Parse(shownString.Substring(dn + 3, 1)) % 2 == 1)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 16: //Character G = Two consecutive digits, second at least three more than the first
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + 3 <= Int32.Parse(shownString.Substring(dn + 1, 1)))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 17: //Character H = A 7 or 9, followed by an even digit
                for (int dn = 1; dn < 6; dn++)
                {
                    if ((shownString.Substring(dn, 1) == "7" || shownString.Substring(dn, 1) == "9") &&
                        Int32.Parse(shownString.Substring(dn + 1, 1)) % 2 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 18: //Character I = Three consecutive digits, exactly two are 1 or 7
                var isOneOrSeven = 0;
                for (int dn = 1; dn < 5; dn++)
                {
                    for (int testDigitI = 0; testDigitI < 3; testDigitI++)
                    {
                        if (shownString.Substring(dn + testDigitI, 1) == "1" || shownString.Substring(dn + testDigitI, 1) == "7")
                        {
                            isOneOrSeven++;
                        }
                    }
                    if (isOneOrSeven == 2)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                    isOneOrSeven = 0;
                }
                break;
            case 19: //Character J = Three digits, exactly three of 2, 3, 5, and 9
                var twoUsed = false;
                var threeUsed = false;
                var fiveUsed = false;
                var nineUsed = false;
                var usedCount = 0;
                for (int dn = 1; dn < 5; dn++)
                {
                    for (int testDigitJ = 0; testDigitJ < 3; testDigitJ++)
                    {
                        if (shownString.Substring(dn + testDigitJ, 1) == "2")
                        {
                            twoUsed = true;
                        }
                        else if (shownString.Substring(dn + testDigitJ, 1) == "3")
                        {
                            threeUsed = true;
                        }
                        else if (shownString.Substring(dn + testDigitJ, 1) == "5")
                        {
                            fiveUsed = true;
                        }
                        else if (shownString.Substring(dn + testDigitJ, 1) == "9")
                        {
                            nineUsed = true;
                        }
                    }
                    if (twoUsed)
                    {
                        usedCount++;
                    }
                    if (threeUsed)
                    {
                        usedCount++;
                    }
                    if (fiveUsed)
                    {
                        usedCount++;
                    }
                    if (nineUsed)
                    {
                        usedCount++;
                    }
                    if (usedCount == 3)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                    twoUsed = false;
                    threeUsed = false;
                    fiveUsed = false;
                    nineUsed = false;
                    usedCount = 0;
                }
                break;
            case 20: //Character K = Two-digit multiple of 15
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 2)) % 15 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 21: //Character L = Four consecutive digits which add up to 14.
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) +
                        Int32.Parse(shownString.Substring(dn + 2, 1)) + Int32.Parse(shownString.Substring(dn + 3, 1)) == 14)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 22: //Character M = Four digit number from 5930 to 6075
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 4)) >= 5930 && Int32.Parse(shownString.Substring(dn, 4)) <= 6075)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 23: //Character N = Two digits that are the same
                for (int dn = 1; dn < 6; dn++)
                {
                    if (shownString.Substring(dn, 1) == shownString.Substring(dn + 1, 1))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 24: //Character O = Five digits, each is even or 7
                var evenOrSeven = 0;
                for (int dn = 1; dn < 3; dn++)
                {
                    for (int testDigitO = 0; testDigitO < 5; testDigitO++)
                    {
                        if (Int32.Parse(shownString.Substring(dn + testDigitO, 1)) % 2 == 0 || shownString.Substring(dn + testDigitO, 1) == "7")
                        {
                            evenOrSeven++;
                        }
                    }
                    if (evenOrSeven == 5)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 5, 3 - dn));
                        dn = 3;
                    }
                    evenOrSeven = 0;
                }
                break;
            case 25: //Character P = Two digits, 2 or 4 then an odd digit
                for (int dn = 1; dn < 6; dn++)
                {
                    if ((shownString.Substring(dn, 1) == "2" || shownString.Substring(dn, 1) == "4") &&
                        Int32.Parse(shownString.Substring(dn + 1, 1)) % 2 == 1)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 26: //Character Q = Three digits, sum is 23 or more
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) + Int32.Parse(shownString.Substring(dn + 2, 1)) >= 23)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 27: //Character R = Two consecutive digits, both in the bomb's serial number
                for (int dn = 1; dn < 6; dn++)
                {
                    if (bombSerial.Where(x => shownString.Substring(dn, 1).Contains(x)).Count() > 0 && bombSerial.Where(x => shownString.Substring(dn + 1, 1).Contains(x)).Count() > 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            case 28: //Character S = Four digits, each digit is less than the one before it
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) > Int32.Parse(shownString.Substring(dn + 1, 1)) &&
                        Int32.Parse(shownString.Substring(dn + 1, 1)) > Int32.Parse(shownString.Substring(dn + 2, 1)) &&
                        Int32.Parse(shownString.Substring(dn + 2, 1)) > Int32.Parse(shownString.Substring(dn + 3, 1)))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 29: //Character T = Four digits, add up to 29+ or 7-
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) +
                        Int32.Parse(shownString.Substring(dn + 2, 1)) + Int32.Parse(shownString.Substring(dn + 3, 1)) > 28 || 
                        Int32.Parse(shownString.Substring(dn, 1)) + Int32.Parse(shownString.Substring(dn + 1, 1)) +
                        Int32.Parse(shownString.Substring(dn + 2, 1)) + Int32.Parse(shownString.Substring(dn + 3, 1)) < 8)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 30: //Character U = Three digits, first and third match
                for (int dn = 1; dn < 5; dn++)
                {
                    if (shownString.Substring(dn, 1) == shownString.Substring(dn + 2, 1))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 31: //Character V = Three consecutive digits from 0 to 2
                for (int dn = 1; dn < 5; dn++)
                {
                    if ((shownString.Substring(dn, 1) == "2" || shownString.Substring(dn, 1) == "1" || shownString.Substring(dn, 1) == "0") &&
                        (shownString.Substring(dn + 1, 1) == "2" || shownString.Substring(dn + 1, 1) == "1" || shownString.Substring(dn + 1, 1) == "0") &&
                        (shownString.Substring(dn + 2, 1) == "2" || shownString.Substring(dn + 2, 1) == "1" || shownString.Substring(dn + 2, 1) == "0"))
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 32: //Character W = Four consecutive even digits
                for (int dn = 1; dn < 4; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 1)) % 2 == 0 && Int32.Parse(shownString.Substring(dn + 1, 1)) % 2 == 0 &&
                        Int32.Parse(shownString.Substring(dn + 2, 1)) % 2 == 0 && Int32.Parse(shownString.Substring(dn + 3, 1)) % 2 == 0)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 4, 4 - dn));
                        dn = 4;
                    }
                }
                break;
            case 33: //Character X = Three digit number from 395 to 411, inclusive
                for (int dn = 1; dn < 5; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 3)) >= 395 && Int32.Parse(shownString.Substring(dn, 3)) <= 411)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                }
                break;
            case 34: //Character Y = Five consecutive digits without a 3 or a 6
                for (int dn = 1; dn < 3; dn++)
                {
                    if (shownString.Substring(dn, 1) != "3" && shownString.Substring(dn, 1) != "6" &&
                        shownString.Substring(dn + 1, 1) != "3" && shownString.Substring(dn + 1, 1) != "6" &&
                        shownString.Substring(dn + 2, 1) != "3" && shownString.Substring(dn + 2, 1) != "6" &&
                        shownString.Substring(dn + 3, 1) != "3" && shownString.Substring(dn + 3, 1) != "6" &&
                        shownString.Substring(dn + 4, 1) != "3" && shownString.Substring(dn + 4, 1) != "6")
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 5, 3 - dn));
                        dn = 3;
                    }
                }
                break;
            case 35: //Character Z = Three digits, two or more 2's and/or 5's
                var squiggles = 0;
                for (int dn = 1; dn < 5; dn++)
                {
                    for (int testDigitJ = 0; testDigitJ < 3; testDigitJ++)
                    {
                        if (shownString.Substring(dn + testDigitJ, 1) == "2" || shownString.Substring(dn + testDigitJ, 1) == "5")
                        {
                            squiggles++;
                        }
                    }
                    if (squiggles >= 2)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 3, 5 - dn));
                        dn = 5;
                    }
                    squiggles = 0;
                }
                break;
            case 36: //Repeat character = (serial position * 12) or (serial position * 15)
                for (int dn = 1; dn < 6; dn++)
                {
                    if (Int32.Parse(shownString.Substring(dn, 2)) == currentRound * 12 || Int32.Parse(shownString.Substring(dn, 2)) == currentRound * 15)
                    {
                        foundAnswer = true;
                        firstNumber = Int32.Parse(shownString.Substring(0, dn));
                        secondNumber = Int32.Parse(shownString.Substring(dn + 2, 6 - dn));
                        dn = 6;
                    }
                }
                break;
            default:
            break;
        }



    }

}
