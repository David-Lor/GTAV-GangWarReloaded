/* GTA V Gang War Mod
 * COPS+ARMY vs. GANG ALLIANCE
 * MODIFIED BY ENFORCERZHUKOV
 * This script will spawn peds from gangs (ballas, vagos, families & lost)
 * on team A (enemy team) and soldiers, cops & NOOSE specops units
 * on team B (player's team). Both teams will fight each other, but team B
 * will respect cops & army from the game, so native spawned cops & soldiers
 * won't fight with the cops&soldiers spawned by this script.
 * 
 * ORIGINAL CREDITS:
 * 
 * After the play defines during the gametime two spawnpoints, 
 * there will be spawnd a limited crowd of peds for each team.
 * Peds will goto the enemy spawnpoint and atack peds from the enemy team
 * The player is in Team B
 * 
 * Use this Mod for getting introdused into scripting mods for GTA V :)
 * 
 * Version 0.2
 * 
 * (C)Tobias Rosini
 * tobias.rosini@hotmail.de
 */
using GTA;
using GTA.Native;
using GTA.Math;

using System;
using System.Collections.Generic;
using System.Windows.Forms;



public class GangWarB : Script
{
    //RelationshipGroup Indizies
    int relgrpA = 0;
    int relgrpB = 0;

    //Frequenzy divisor counter
    int TickCnt = 0;
    
    //Lists  for keeping references to our Peds
    List<Ped> managedPedsTeamA = new List<Ped>();
    List<Ped> managedPedsTeamB = new List<Ped>();
    
    //List for noticing wich Ped has to be deleted
    List<Ped> LöschIndizies = new List<Ped>();

    //Points
    int PointsTeamA = 0;
    int PointsTeamB = 0;

    public GangWarB()
    {
        //Events from Game
        Tick += OnTick;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;

        Interval = 10;
        
        //Defining Relationships
            //originales
                //relgrpA = World.AddRelationshipGroup("A");
                //relgrpB = World.AddRelationshipGroup("B");

                relgrpA = World.AddRelationshipGroup("A");
                relgrpB = World.AddRelationshipGroup("B");
                relgrpB = World.AddRelationshipGroup("cop");
                relgrpB = World.AddRelationshipGroup("army");
                relgrpB = World.AddRelationshipGroup("player");

        //who hates and like who
            //originales
                //World.SetRelationshipBetweenGroups(Relationship.Hate, relgrpA, relgrpB);
                //World.SetRelationshipBetweenGroups(Relationship.Respect, relgrpA, relgrpA);
                //World.SetRelationshipBetweenGroups(Relationship.Respect, relgrpB, relgrpB);
        World.SetRelationshipBetweenGroups(Relationship.Hate, relgrpA, relgrpB);
        World.SetRelationshipBetweenGroups(Relationship.Respect, relgrpA, relgrpA);
        World.SetRelationshipBetweenGroups(Relationship.Respect, relgrpB, relgrpB);
        
        //Put Player in Team B
        Game.Player.Character.RelationshipGroup = relgrpB;
        //managedPedsTeamA.Add(Game.Player.Character);
    }

    //Happens every 10 ms
    void OnTick(object sender, EventArgs e)
    {
        //I build here a Frequenzy divisor for things that do not have to happen so often
        int TickDivisor = 50; 
        TickCnt++;
        if (TickCnt > TickDivisor)
        {
            TickCnt = 0;
            DividedTick(sender, e);//Happens every 500 ms
        }

        //HUD
        UIText txtA = new UIText(
            PointsTeamA.ToString(), 
            new System.Drawing.Point(0, 0), 
            1,
            System.Drawing.Color.Red);
        txtA.Draw();

        UIText txtB = new UIText(
            PointsTeamB.ToString(),
            new System.Drawing.Point(100, 0),
            1,
            System.Drawing.Color.Yellow);
        txtB.Draw();
    }

    //Happens every 500 ms
    void DividedTick(object sender, EventArgs e)
    {
        CheckPeds(managedPedsTeamA);
        CheckPeds(managedPedsTeamB);

        int livingA = 0;
        int livingB = 0;

        foreach (Ped ped in managedPedsTeamA)
            if (ped.IsAlive)
                livingA++;

        foreach (Ped ped in managedPedsTeamB)
            if (ped.IsAlive)
                livingB++;

        if ((livingB + livingB) < 25)
        {
            if (livingA > livingB)
                CreateNewPed("B");
            else
                CreateNewPed("A");
        }
        CleanUpDeath();
    }

    string GetManagedPedInfo(string team)
    {
        int dead = 0;
        int idle = 0;
        int walking = 0;
        List<Ped>l = null;

        if (team == "A")        
            l = managedPedsTeamA;
        
        if (team == "B")
            l = managedPedsTeamB;
        
        if (l != null)
        {
            foreach (Ped ped in l)
            {
                if (ped.IsDead)
                    dead++;

                if (ped.IsWalking)
                    walking++;

                if (ped.IsIdle)
                    idle++;
            }
        }
        
        return "Team" + team + ": " + managedPedsTeamA.Count + " | dead: " + dead.ToString() + " | idle: " + idle.ToString() + " | walking: " + walking.ToString();
    }

    void CleanUpDeath()
    {        
        while (LöschIndizies.Count > 5)
        {
            int killIndex = -1;
            //Durchsuche Liste A
            for (int i = 0; i < managedPedsTeamA.Count;i++)
            {
                if (LöschIndizies[0].Handle == managedPedsTeamA[i].Handle)
                {
                    killIndex = i;
                }
            }
            if (killIndex != -1)
            {
                managedPedsTeamA[killIndex].Delete();
                managedPedsTeamA.RemoveAt(killIndex);
            }
            else
            {
                //Durchsuche Liste B
                for (int i = 0; i < managedPedsTeamB.Count; i++)
                {                    
                    if (LöschIndizies[0].Handle == managedPedsTeamB[i].Handle)
                    {
                        killIndex = i;
                    }                    
                }
                if (killIndex != -1)
                {
                    managedPedsTeamB[killIndex].Delete();
                    managedPedsTeamB.RemoveAt(killIndex);
                }
            }

            //Entfernen aus Löschliste
            if (killIndex != -1)
                LöschIndizies.RemoveAt(0);
        }

        /*
        //FiFo PedsToDelete Buffer
        while (LöschIndizies.Count > 5)
        {            
            if (targetList.Count > 0)
            {
                if (LöschIndizies.Remove(targetList[0]))
                {
                    //UI.Notify("delete from L-List");
                    targetList[0].Delete();
                    targetList.RemoveAt(0);
                }
            }
        }
         */
    }

    void CheckPeds(List<Ped> targetList)
    {
        Ped player = Game.Player.Character;
                        
        foreach (Ped ped in targetList)
        {/*
            bool isNear = false;
            if (World.GetDistance(player.Position, ped.Position) < 100f)
            {
                isNear = true;
                //UI.Notify("nearly " + ped.Handle.ToString());
            }

            //Cleaning Up far away Peds
            if (!isNear)
            {                
                LöschIndizies.Add(ped);                
            }*/
            //else
            {
                //setting Up Tasks: Every one who idle goto the enemy spawnpoint
                if (ped.IsIdle)
                {
                    if (targetList == managedPedsTeamA)
                        ped.Task.GoTo(spawnLocB);
                    if(targetList == managedPedsTeamB)
                        ped.Task.GoTo(spawnLocA);
                }
            }
            
            //Cleaning Up death Peds
            if (ped.IsDead) // Puts death ped onto the delete-list
            {                             
                Ped killer = ped.GetKiller() as Ped;

                if (killer != null)
                {
                    if (!LöschIndizies.Contains(ped))                    
                    {
                        if (IsPedInTeam(killer, managedPedsTeamA))
                        {
                            PointsTeamA++;
                        }

                        if (IsPedInTeam(killer, managedPedsTeamB))
                        {
                            PointsTeamB++;
                        }
                      
                        if(killer.Equals(player))
                            PointsTeamB++;

                        LöschIndizies.Add(ped); 
                        //UI.Notify(killer.Handle.ToString() + " kills " + ped.Handle.ToString());
                    }
                }                
            }
        }

        
        /*
        //Delete noted Peds
        foreach (Ped lped in LöschIndizies)
        {
            targetList.Remove(lped);
            //UI.Notify("Remove " + lped.Handle.ToString());
            lped.Delete();
        }
        */
        //UI.Notify(PointsTeamA.ToString() + " : " + PointsTeamB.ToString());
        
    }

    /// <summary>
    /// Results true if the ped is in the List
    /// </summary>
    /// <param name="ped">The ped to check</param>
    /// <param name="TeamList">The List with all the Teammeber</param>
    /// <returns></returns>
    bool IsPedInTeam(Ped ped, List<Ped> TeamList)
    {
        foreach(Ped teammember in  TeamList)
        {
            if (ped.Handle == teammember.Handle)
                return true;
        }
        return false;
    }

    //Use this for cathing the KeyDown Event   
    void OnKeyDown(object sender, KeyEventArgs e)
    {
    }
        
    GTA.Math.Vector3 spawnLocA = new GTA.Math.Vector3(0, 0, 0);
    GTA.Math.Vector3 spawnLocB = new GTA.Math.Vector3(0, 0, 0);

    //for securing that a spawnpoint has defined before each team creates its Peds
    bool IsSpawnLocADefined = false;
    bool IsSpawnLocBDefined = false;

    /// <summary>
    /// Creates a new Ped 
    /// </summary>
    /// <param name="team">Team A or B</param>
    void CreateNewPed(string team)    
    {   
        //If there are both Spawnpoints defined
        if (IsSpawnLocADefined && IsSpawnLocBDefined)
        {
            //depending on team we build a small list of models for a random selection
            List<string> model_names = new List<string>();
            if (team == "A")
            {
                //Tip: Google one of the strings for find lists of Modelnames
				//TEAM A = ENEMY -GANGS
                model_names.Add("g_f_y_ballas_01");
                model_names.Add("g_m_y_ballaeast_01");
                model_names.Add("g_m_y_ballaorig_01");
                model_names.Add("g_m_y_ballasout_01");
                model_names.Add("g_f_y_vagos_01");
                model_names.Add("g_f_y_lost_01");
                model_names.Add("g_f_y_families_01");
                model_names.Add("g_m_y_famca_01");
                model_names.Add("g_m_y_famdnf_01");
            }
            if (team == "B")
            {
				//TEAM B = FRIENDLY -COPS
                model_names.Add("s_m_m_security_01");
                model_names.Add("s_m_y_blackops_01");
                model_names.Add("s_m_y_marine_01");                
                model_names.Add("s_m_y_swat_01");
                model_names.Add("s_f_y_ranger_01");
                model_names.Add("s_f_y_sheriff_01");
                model_names.Add("s_m_y_cop_01");
                model_names.Add("s_f_y_cop_01");
            }

            //for random selection 
            Random r = new Random();
            
            //This will become the new created ped
            Ped ped = null;            

            //Yes.. the following code is uggly (to much copy paste for team A & B, nicer would be to isolated this in a new method)
            //if there are excepitons in the follwing lines check the spelling of your modelnames
            if (team == "A")
            {
                ped = GTA.World.CreatePed(model_names[r.Next(0, model_names.Count)], spawnLocA);
                
                //Relationship&weapon TEAM A
                if (ped != null)
                {                    
                    managedPedsTeamA.Add(ped);
                    ped.RelationshipGroup = relgrpA;
                    ped.Weapons.Give(GTA.Native.WeaponHash.AssaultRifle, 30, true, true);
                    ped.Weapons.Give(GTA.Native.WeaponHash.Bat, 0, true, true);
                }
            }

            if (team == "B")
            {                
                ped = GTA.World.CreatePed(model_names[r.Next(0, model_names.Count)], spawnLocB);

                //Relationship&weapon TEAM B
                if (ped != null)
                {                    
                    managedPedsTeamB.Add(ped);
                    ped.RelationshipGroup = relgrpB;
                    ped.Weapons.Give(GTA.Native.WeaponHash.CarbineRifle, 30, true, true);
                    ped.Weapons.Give(GTA.Native.WeaponHash.Bat, 0, true, true);
                }
            }
            
            //There should be no tasks but...
            ped.Task.ClearAllImmediately();
                        
            //ped.Task.FightAgainst(Game.Player.Character);

            
            //Weapon (native)
            //ped.Weapons.Give(GTA.Native.WeaponHash.CarbineRifle, 1, true, true);
            
            //blip on the GTA map
            Blip blip = ped.AddBlip();
            if (blip != null)
            {
                if (team == "A")
                    blip.Color = BlipColor.Red;

                if(team == "B")
                    blip.Color = BlipColor.Yellow;
                blip.Scale = 0.5f;
            }
            
            //Output for debugging
            //UI.Notify(ped.Handle.ToString() + " spawned for team " + team );
        }
        else
            //If there are not both Spawnpoints defined
            UI.Notify("Press K = TeamA (Enemies) or L = Team B (Allies) to define the teams spawnlocation at your actual position");
    }

    //Use this for cathing the KeyUp Event
    void OnKeyUp(object sender, KeyEventArgs e)
    {
        //Set SpawnPos for Team A
        if (e.KeyCode == Keys.K)
        {
            //A Reference to the Player
            Ped player = Game.Player.Character;

            //Use this for one meter in front of the player
            //spawnLoc = player.Position + (player.ForwardVector * 1); 

            //Define the playerpos as the spawnpos for the team 
            spawnLocA = player.Position;

            //notice that location is defined
            IsSpawnLocADefined = true;
            
            //A Blip for the spawn pos
            Blip blip = World.CreateBlip(spawnLocA, 1.5f);
            blip.Color = BlipColor.Red;
            blip.Scale = 5f;
            UI.Notify("Point A (Enemies) is ready");
        }

        //Set SpawnPos for Team B
        if (e.KeyCode == Keys.L)
        {
            //for code comments see a few lines up
            Ped player = Game.Player.Character;            
            spawnLocB = player.Position;
                        
            IsSpawnLocBDefined = true;            

            Blip blip = World.CreateBlip(spawnLocB, 1.5f);
            blip.Color = BlipColor.Yellow;
            blip.Scale = 5f;

            //feedback
            UI.Notify("Point B (Allies) is ready");
        }
    }
}           
