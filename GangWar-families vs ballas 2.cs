/* GTA V Gang War Mod
 * COPS vs. GANG ALLIANCE - v2
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
 * ---------------------FALLOS: NO SE CUENTAN BAJAS, Y CRASHEA PORQUE SPAWNEAN INFINITAMENTE
 */
using GTA;
using GTA.Native;
using GTA.Math;

using System;
using System.Collections.Generic;
using System.Windows.Forms;



public class GangWar : Script
{
    bool activar = true; //choose if you want the spawning system turned on by default
    bool deletepeds = false; //choose if cleaning death peds [true=yes, false=no]
    bool crear = true; //do not touch this
    int relgrpA = 0; //do not touch this
    int relgrpB = 0; //do not touch this

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

    public GangWar()
    {
        //Events from Game
        Tick += OnTick;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;

        Interval = 10;
        
        //Defining Relationships
            //relgrpA = World.AddRelationshipGroup("A");
            //relgrpB = World.AddRelationshipGroup("B");
            relgrpA = World.AddRelationshipGroup("A");
            relgrpB = World.AddRelationshipGroup("B");
            relgrpB = World.AddRelationshipGroup("cop");
            relgrpB = World.AddRelationshipGroup("army");
            relgrpB = World.AddRelationshipGroup("player");

        //who hates and like who
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

        if ((livingB + livingB) < 25) {
            crear = true;
        }
        if (activar & crear)
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
                        ped.Task.RunTo(spawnLocB);
                    if(targetList == managedPedsTeamB)
                        ped.Task.RunTo(spawnLocA);
                }
            }
            
            //Cleaning Up death Peds
            if (ped.IsDead & deletepeds) // Puts death ped onto the delete-list
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
        foreach(Ped teammember in TeamList)
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

            //FAMILIES VS VAGOS
            /*if (team == "A")
            {
                //ENEMIES
                model_names.Add("g_f_y_vagos_01");
                model_names.Add("g_m_y_mexgoon_01");
                model_names.Add("g_m_y_mexgoon_02");
                model_names.Add("g_m_y_mexgoon_03");
            }
            if (team == "B")
            {
				//ALLIES
                model_names.Add("g_f_y_families_01");
                model_names.Add("g_m_y_famca_01");
                model_names.Add("g_m_y_famdnf_01");
                model_names.Add("g_m_y_famfor_01");
            }*/

            //FAMILIES VS BALLAS
            if (team == "A")
            {
                //ENEMIES
                model_names.Add("g_f_y_ballas_01");
                model_names.Add("g_m_y_ballaeast_01");
                model_names.Add("g_m_y_ballaorig_01");
                model_names.Add("g_m_y_ballasout_01");
            }
            if (team == "B")
            {
				//ALLIES
                model_names.Add("g_f_y_families_01");
                model_names.Add("g_m_y_famca_01");
                model_names.Add("g_m_y_famdnf_01");
                model_names.Add("g_m_y_famfor_01");
            }
            
            //FAMILIES VS LOST
            /*if (team == "A")
            {
                //ENEMIES
                model_names.Add("g_m_y_lost_03");
                model_names.Add("g_m_y_lost_02");
                model_names.Add("g_m_y_lost_01");
                model_names.Add("g_f_y_lost_01");
            }
            if (team == "B")
            {
				//ALLIES
                model_names.Add("g_f_y_families_01");
                model_names.Add("g_m_y_famca_01");
                model_names.Add("g_m_y_famdnf_01");
                model_names.Add("g_m_y_famfor_01");
            }*/

            //for random selection 
            Random r = new Random();

            //This will become the new created ped
            Ped ped = null;

            //probabilities for weapons... change the last number on each row
            //higher the number, lower the probability of spawning with the 1st weapon
            //for example: 1,4 = each 3 spawned peds, 1 should spawn with 1st weapon and 3 with 2nd weapon.
            //DONT CHANGE THE NUMBER 1: just the 2nd number.
            int weaponTeamA = r.Next(1, 3); //probability for team A (enemy)
            int weaponTeamB = r.Next(1, 3); //probability for team B (friendly)

            if (team == "A")
            {
                ped = GTA.World.CreatePed(model_names[r.Next(0, model_names.Count)], spawnLocA);
                
                //Relationship&weapon TEAM A
                if (ped != null)
                {
                    managedPedsTeamA.Add(ped);
                    ped.RelationshipGroup = relgrpA;
                    if (weaponTeamA == 1) { //1st weapon
                        ped.Weapons.Give(GTA.Native.WeaponHash.AssaultRifle, 1, true, true);
                    }
                    else { //2nd weapon
                        ped.Weapons.Give(GTA.Native.WeaponHash.CombatPistol, 1, true, true);
                    }
                    //ped.Weapons.Give(GTA.Native.WeaponHash.weaponA, 1, true, true);
                }
            }

            //Relationship&weapon TEAM B
            if (team == "B")
            {                
                ped = GTA.World.CreatePed(model_names[r.Next(0, model_names.Count)], spawnLocB);

                //Relationship&weapon TEAM B = COPS
                if (ped != null)
                {                    
                    managedPedsTeamB.Add(ped);
                    ped.RelationshipGroup = relgrpB;
                    if (weaponTeamB == 1) { //1st weapon
                        ped.Weapons.Give(GTA.Native.WeaponHash.AssaultRifle, 1, true, true);
                    }
                    else { //2nd weapon
                        ped.Weapons.Give(GTA.Native.WeaponHash.Pistol, 1, true, true);
                    }
                    //ped.Weapons.Give(GTA.Native.WeaponHash.weaponA, 1, true, true);
                }
            }
            
            //There should be no tasks but...
            ped.Task.ClearAllImmediately();
                        
            //ped.Task.FightAgainst(Game.Player.Character);
            
            //Weapon (native)
            //ped.Weapons.Give(GTA.Native.WeaponHash.CarbineRifle, 1, true, true);

            //Setting Up Health and Armor
            //ped.Health = 50;
            //ped.Armor = 0;
            //Health and Armor for different teams
            if (team == "A") { //teamA=enemies
                ped.Health = 60;
                ped.Armor = 0;
            }
            if (team == "B") { //teamB=friends
                ped.Health = 60;
                ped.Armor = 0;
            }

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
            UI.Notify("Press H = TeamA (Enemies) or J = Team B (Allies) to define the teams spawnlocation at your actual position. K=Start/Stop spawn");
    }

    //Use this for cathing the KeyUp Event
    void OnKeyUp(object sender, KeyEventArgs e)
    {
        //Set SpawnPos for Team A
        if (e.KeyCode == Keys.H)
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
        if (e.KeyCode == Keys.J)
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

        //Activate & deactivate ped spawning on both teams
        /* simple script = 2 keys
        if (e.KeyCode == Keys.K) {
            activar = true;
            UI.Notify("Peds will spawn");
        }
        if (e.KeyCode == Keys.L) {
            activar = false;
            UI.Notify("Peds won't spawn");
        } */

        if (e.KeyCode == Keys.K) { //change the K key to other key you want
            activar = !activar; //mod activated=true or false on each K key pulse (toggling)
            //debugging messages when it's activated or not
            /*if (activar) {
                UI.Notify("Peds will spawn"); //debugging messages when it's activated or not
            }
            else {
                UI.Notify("Peds won't spawn");
            }*/
        }
    }
}           
