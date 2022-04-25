using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsScript : MonoBehaviour
{
    // Start is called before the first frame update

    //basically, the rank gap between each rank is stored here.
    //starts with Novice, rank1
    public int rank2Exp; //exp required to hit "Experienced Rookie"
    public int rank3Exp; //exp required to hit "Advanced Beginner" from rank2,
    public int rank4Exp; //"Proficient Interviewee",
    public int rank5Exp; //"Veteran Of Knowledge",
    public int rank6Exp; //"Speaking Expert",
    public int rank7Exp; //"Master Of Interviews"
    public int rank8Exp; //"The Visionary",

    public int currRank; //1-8
    public int expAccumulated; //clear exp accumulated so far at each new level
    public int nextAdvExp;
    public bool maxLv;

    public int gainExpAmt; // this is used for exp bar testing

    titleSelectorScript titleController;
    Slider expBar;
    Text currRankTerm;

    public Animator lvUpMessage;

    void Start()
    {
        StartCoroutine(UpdatePlayerStats());
    }

    IEnumerator UpdatePlayerStats()
    {
        var playerHud = GameObject.FindGameObjectWithTag("PlayerHUD");
        if (playerHud == null)
        {
            yield return null;
            StartCoroutine(UpdatePlayerStats());
        }
        else
        {
            yield return null;

            titleController = playerHud.transform.Find("hudText").
            transform.Find("TitleSelector").gameObject.GetComponent<titleSelectorScript>();
            expBar = playerHud.transform.Find("hudText").
                transform.Find("EXPBarObj").transform.Find("ExpBarOutline").gameObject.GetComponent<Slider>();
            currRankTerm = playerHud.transform.Find("hudText").
                transform.Find("CurrRank").transform.Find("rankTerm").gameObject.GetComponent<Text>();
            lvUpMessage = currRankTerm.gameObject.transform.parent.transform.parent.transform.Find("RankedUpText").GetComponent<Animator>();

            //change these lines: we want a server that tracks previous player data, not resetting them every game
            currRank = 1;
            expAccumulated = 0;
            nextAdvExp = rank2Exp;
            maxLv = false;
            updateExpBar(expAccumulated, rank2Exp);
            updateRankDisplay(titleController.rankTitles[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            gainExp(gainExpAmt);
        }


    }

    //easist check is to check currRank and its required exp
    void checkRank()
    {
        switch(currRank) //screw efficient outlook, idk
        {
            case 1: //check for rank2
                if(expAccumulated >= rank2Exp) 
                {
                    rankUp(2, rank2Exp);
                    nextAdvExp = rank3Exp;
                    updateExpBarColor(Color.red);
                }
                break;
            case 2: //check for rank 3
                if (expAccumulated >= rank3Exp)
                {
                    rankUp(3, rank3Exp);
                    nextAdvExp = rank4Exp;
                    updateExpBarColor(new Color32(255, 95, 31, 255)); //orange
                }
                break;
            case 3: //check for rank 4
                if (expAccumulated >= rank4Exp)
                {
                    rankUp(4, rank4Exp);
                    nextAdvExp = rank5Exp;
                    updateExpBarColor(Color.yellow);
                }
                break;
            case 4: //check for rank 5
                if (expAccumulated >= rank5Exp)
                {
                    rankUp(5, rank5Exp);
                    nextAdvExp = rank6Exp;
                    updateExpBarColor(Color.green);
                }
                break;
            case 5: //check for rank 6
                if (expAccumulated >= rank6Exp)
                {
                    rankUp(6, rank6Exp);
                    nextAdvExp = rank7Exp;
                    updateExpBarColor(Color.cyan);
                }
                break;
            case 6: //check for rank 7
                if (expAccumulated >= rank7Exp)
                {
                    rankUp(7, rank7Exp);
                    nextAdvExp = rank8Exp;
                    updateExpBarColor(Color.blue);
                }
                break;
            case 7: //check for rank 8, the max rank
                if (expAccumulated >= rank8Exp)
                {
                    rankUp(8, rank8Exp); //prob do something special at 8, ex not resetting exp but capping
                    nextAdvExp = expAccumulated; //max lv! so 1/1 all filled
                    updateExpBarColor(new Color32(139, 0, 255, 255));
                }
                break;
        }
        updateExpBar(expAccumulated, nextAdvExp);
    }

    void rankUp(int nextRank, int nextRankExp) {
        //some particle system animation (diverse based on lv or nah?)
        //idk about this... performance dude :(

        //some reward alert and text message showing new rank unlocked (diverse based on lv or nah?)
        lvUpMessage.SetTrigger("Display"); //plays once then resets, convenient!

        //unlock new rank
        titleController.AddDropDownOptions(nextRank - 1);
        updateRankDisplay(titleController.rankTitles[nextRank - 1]);

        //going to a new level!
        currRank = nextRank;
        expAccumulated = expAccumulated - nextRankExp; //either 0 or exp overflow to be kept //resets expAccumulated

        //update expbar stats something like that (already covered at the end of check rank, no need for rank up)
        if (currRank == 8) //max lv! do sommething special
        {
            updateExpBarColor(Color.black);
            updateRankDisplay(titleController.rankTitles[currRank - 1] + " (MAX)");
            maxLv = true;
        }
    }

    //use this function to add exp to the player, auto checks for rank up after
    public void gainExp(int amount)
    {
        if(!maxLv)
        {
            expAccumulated += amount;
            checkRank();
        }
    }

    public void updateExpBar(int currAmount, int totalExp)
    {
        expBar.value = (float)currAmount / totalExp;
        if (currAmount == totalExp) //since this is executed last, this is only possible at max rank
        {
            updateExpIndicator(120);
        }
        else
        {
            updateExpIndicator((int)((float)currAmount / totalExp * 100));
        }
    }

    public void updateExpBarColor(Color32 color) //no need to resource.load, assign color to gray
    {
        expBar.gameObject.transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = color;
    }

    public void updateRankDisplay(string name)
    {
        currRankTerm.text = name;
    }

    public void updateExpIndicator(int percentage)
    {
        expBar.gameObject.transform.parent.Find("ExpIndicator").GetComponent<Text>().text = percentage + "%"; 
        if(percentage > 100) //speical value
        {
            expBar.gameObject.transform.parent.Find("ExpIndicator").GetComponent<Text>().text = 9000 + "+ power lvl"; 
        }
    }
}
