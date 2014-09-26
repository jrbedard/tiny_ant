// Tiny ANT 1.1
// par Jean-René Bédard (jrbedard.com)
// November 2002

// csc /t:library /debug+ /out:Tiny_ANT.dll /r:System.dll,System.Drawing.dll,OrganismBase.dll Tiny_ANT.cs Astar.cs BinaryHeap.cs navigation.cs util.cs DNA.cs


using System; 
using System.Drawing; 
using System.Collections; 
using System.IO; 



//  Définitions de la créature  
[assembly: OrganismClass("Tiny_ANT")]    
[assembly: AuthorInformation("Jean-René Bédard", "01128041@usherb.ca")]  
[CarnivoreAttribute(false)]       // Herbivore 

// Definition
[AnimalSkin("Ant")]
[MarkingColor(KnownColor.Green)]
[MatureSize(25)]                  
  

[MaximumEnergyPoints(12)]   
[EatingSpeedPoints(4)]      
[AttackDamagePoints(16)]     
[DefendDamagePoints(8)]     
[MaximumSpeedPoints(20)]    
[CamouflagePoints(0)]       
[EyesightPoints(40)]       


public class Tiny_ANT : Animal 
{ 

    
   
    const int cruisingSpeed = 5;          // When there's nothing better to do: dawdle
    const int fleeingSpeed = 20;          // Running from scary crap speed
 

	#region Enumerations
        
    #region private variables
    private Dna dna = new Dna();
    

	#endregion
	#endregion


	ArrayList attacking_h = new ArrayList(); 
		
	

	EvilDog.Navigation navigation;
	





// real variables

	private bool isKiller;
	private bool isCommunicator;
	private bool isBait;



	private bool first_time=true;
	
	private bool please=false;
	
	private bool blocked_ant=false;
	OrganismState Herbivore_blocker=null;


	private bool fleeing_carnivore=false;
	ArrayList fleeing_carnivore_path = new ArrayList();
	ArrayList carnivores_threats = new ArrayList();
	int fleeing_carnivore_mouvement=0;
	
	
	
	ArrayList fleeing_herbivore_path = new ArrayList();

	private bool tracking_plant=false;
	ArrayList tracking_plant_path = new ArrayList();
	int tracking_plant_mouvement=0;
	PlantState targetPlant = null; 
	


	private bool killing_prey=false;
	ArrayList tracking_prey_path = new ArrayList();
	AnimalState targetPrey = null;      


	
	AnimalState TheAttacker=null;

	private bool tracking_herbivore=false;
	
	

// end real variables
	

	//codes de communication
	//  0 rien
	//  1 je mange seul ou avec 2,3,4,5
	//	12,13,14,15,16,17,18,19,20,21,22,23 je fuis un carnivore qui se situe:
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//
	//


	// Species counter variables
    protected int mcKilled;               
    protected int mcBorn; 
    protected int mcTeleported;
    //***end global attributes
    
    //***event calls
    // Set up any event handlers that we want when first initialized 
    protected override void Initialize() 
    { 
        Load += new LoadEventHandler(LoadEvent); 
        Idle += new IdleEventHandler(IdleEvent); 
        Attacked += new AttackedEventHandler(AttackedEvent); 
        Born += new BornEventHandler(BornEvent);
        MoveCompleted += new MoveCompletedEventHandler(MoveCompletedEvent);
        Teleported += new TeleportedEventHandler(TeleportedEvent);
        AttackCompleted += new AttackCompletedEventHandler(AttackCompletedEvent);
        //EatCompleted += new EatCompletedEventHandler(EatCompletedEvent);
        ReproduceCompleted += new ReproduceCompletedEventHandler(ReproduceCompletedEvent);
		
	} 
    //***end event calls
  
    //***event definitions
    // Fired if we arrived at our destination or got blocked
    void MoveCompletedEvent(object sender, MoveCompletedEventArgs e)
    {
		
		if(tracking_plant==true)
		{
			ant_track_plant();
		}


		if(e.Reason == ReasonForStop.Blocked)
        {
			Herbivore_blocker = e.BlockingOrganism;
			WriteTrace("Un fucking herbivore me bloque le chemin"); 
			blocked_ant=true;
			killing_prey=false;
		}

	
		if(killing_prey==true)
		{
			ant_track_prey();
		}
		
		
	

		
	
	}

    
	
	void ant_fleeing_carnivore()
	{

		BeginMoving(new MovementVector((Point)fleeing_carnivore_path[0], 20));
		
		Antennas.AntennaValue=0;
		fleeing_carnivore=true;
		tracking_plant_path.Clear();
		tracking_plant_mouvement=0;
		tracking_plant=false;
		fleeing_carnivore_mouvement=0;
		
		getDestination(getSpeed(15));


	}
	



	void ant_blocked()
	{
		int RandomX, RandomY;
		RandomX = OrganismRandom.Next(State.Position.X-20, State.Position.X+20); 
		RandomY = OrganismRandom.Next(State.Position.Y-20, State.Position.Y+20);
		Point point_initial = new Point(RandomX,RandomY);
		point_initial = correction_point(point_initial);
								
		BeginMoving(new MovementVector(point_initial, 6));
		getDestination(6);
		blocked_ant=false;
	}




	void ant_track_plant()
	{
		tracking_plant=true;
		
		if(tracking_plant_mouvement < tracking_plant_path.Count)
		BeginMoving(new MovementVector((Point)tracking_plant_path[tracking_plant_mouvement++], getSpeed(13)));
		
		
		if(tracking_plant_mouvement >= tracking_plant_path.Count)
		{
			tracking_plant_mouvement=0;
			tracking_plant=false;
		}	
	}



	void ant_track_prey()
	{
		
		
		if(State.PercentInjured <= 0.85)
		{
			WriteTrace("Je veux tuer ce herbivore!");
		
			if(WithinAttackingRange(targetPrey))
			{
				StopMoving();
				BeginDefending(targetPrey);
				BeginAttacking(targetPrey);
				Antennas.AntennaValue = 99;
			}
			else
			BeginMoving(new MovementVector((Point)targetPrey.Position, getSpeed(13)));
	
		}
		else
		{
			Point p;
			ArrayList vide = new ArrayList();
			ArrayList foundAnimals = Scan();

			p = navigation.GetEvasionPath (State, targetPrey, vide);
			fleeing_herbivore_path = navigation.FindPath(State, State.Position, p, foundAnimals);
			killing_prey=false;
		
			BeginMoving(new MovementVector((Point)fleeing_herbivore_path[0], 20));
			WriteTrace("je me sauve, mort imminente");
		}	
		
	}
	


	
	
	public void Beestje()
    {
            
            //Add data to dna: ("V_Name",current,min,max)
            dna.AddChromosoom("VariableName",5,1,10);
        
    }

	 //Use a dna variable:
     private void Demo()
     {
          int value = dna["VariableName"];
     }


	
	 //Call this methode if you want to reproduce
     private void Reproduce()
     {
            if(CanReproduce)
            {
                WriteTrace(dna.ToString());
			//	dna.AddChromosoom("positionX",(byte)Position.X,(byte)0,(byte)WorldWidth);
			//	dna.AddChromosoom("positionY",(byte)Position.Y,(byte)0,(byte)WorldHeight);
        

				BeginReproduction(dna.Furtilize());             
            }
     }

	
	private void BornEvent(object sender, BornEventArgs e) 
    {
       int Cast;
	   
		
		try
            {
                if (State.Generation>0)
                {
                    dna.MergeDna(e.Dna);
                }
            }
            catch(Exception exc)
            {
                WriteTrace(exc);
            }

	 
	 Cast=OrganismRandom.Next( 2 );
	 if(Cast==0)
     isKiller=true;
	 if(Cast==1)
     isCommunicator=true;


   	navigation = new EvilDog.Navigation(this);
	
	}


    // First event fired on an organism each turn 
    void LoadEvent(object sender, LoadEventArgs e) 
    { 
       
    } 




    // Fired if we are being attacked
    void AttackedEvent(object sender, AttackedEventArgs e) 
    { 
       	TheAttacker = e.Attacker; 
			
			if(e.Attacker.IsAlive && killing_prey==false) 
			{ 
				IAnimalSpecies iAS = (IAnimalSpecies) TheAttacker.Species;
				if(iAS.IsCarnivore==false)
				{
					StopMoving();
					BeginDefending(TheAttacker);
					BeginAttacking(TheAttacker);
					WriteTrace("Je me fais attaquer,je contre-attaque!!!");
					targetPrey = e.Attacker;
				}
			
			}
		
		killing_prey=true;
    } 

	



    // Species Counter function
    public void AttackCompletedEvent(object sender, AttackCompletedEventArgs e) 
    {
        if ( e.Killed ) 
        { 
            
            killing_prey=false;
			WriteTrace("YA!!!!!!!!!"); // Doesn't work without this line
        }
    
	
		killing_prey=false;
		
		getDestination(5);
	}
    
    // Species Counter function
    public void TeleportedEvent(object sender, TeleportedEventArgs e) 
    {
        mcTeleported += 1;
    }

    // Species Counter function
    public void ReproduceCompletedEvent(object sender, ReproduceCompletedEventArgs e) 
    {
        mcBorn += 1;
    }

    // Fired after all other events are fired during a turn 
    void IdleEvent(object sender, IdleEventArgs e) 
    { 
        try 
        { 
            if (ScanForAttackers())
			{
			killing_prey=false;
			return;
			}			

			if(State.EnergyState == EnergyState.Hungry && isKiller==true)
			{
				if(State.PercentInjured < 0.25)
				MoveToKill();
			}


			if(killing_prey==true && blocked_ant==false)
			{
				ant_track_prey();
				return;
			}
		
			
			// Reproduce as often as possible 
            if(CanReproduce) 
            {
                Reproduce();
			}
            
       
			

			if(fleeing_carnivore==true)
			{
				fleeing_carnivore_mouvement++;
				getDestination(getSpeed(10));
				get_dir_com();
			}
			if(fleeing_carnivore==true && fleeing_carnivore_mouvement>=15)
			{
				fleeing_carnivore=false;
				fleeing_carnivore_mouvement=0;
			}
		

			if(fleeing_carnivore==false)
			please = ScanForTargetPlant();
			else
			{
				please = false;
				reception_communication();
			}
		
			Antennas.AntennaValue = 0;



			// If we can eat and we have a target plant, eat 
            if(CanEat) 
            { 
                WriteTrace("Je peux manger");
                if(!IsEating) 
                { 
					if(please==true)  
                    { 
                        WriteTrace("plante-cible trouvée, en santé?");
                        
						if (fleeing_carnivore==false && targetPlant != null)
						{
                            
                            if(WithinEatingRange(targetPlant)) 
                            { 
                                WriteTrace("à porté d'atteinte, commencer à manger"); 
                                
								if (targetPlant != null)
								if (targetPlant.PercentInjured < 0.76 || State.EnergyState == EnergyState.Deterioration)
								if (!ScanForHungryAnts())
								BeginEating(targetPlant); 
                                
								tracking_plant=false;
								tracking_plant_mouvement=0;
	
								if(IsMoving) 
                                { 
                                    WriteTrace("Arrèter pour manger"); 
                                    StopMoving();
                                } 
                            } 
                            else 
                            { 
                                WriteTrace("Atteindre la plante cible");   
                            } 
                        }
						else   // Plante en mauvaise santé
						{
						   
						}
                    } 
                    else  // pas de plante cible
                    { 
                        int RandomX=0, RandomY=0;
						WriteTrace("pas de plante cible"); 
                        
						  if(!IsMoving) 
                            { 
                                WriteTrace("je vais me promener au hasard");
							
								if(first_time==true)
								{
									RandomX = OrganismRandom.Next(State.Position.X-20, State.Position.X+20); 
									RandomY = OrganismRandom.Next(State.Position.Y-20, State.Position.Y+20);
									Point point_initial = new Point(RandomX,RandomY);
									point_initial = correction_point(point_initial);
									
									BeginMoving(new MovementVector(point_initial, 6));
									first_time=false;
								}
								else
								{
									if(blocked_ant==false)
									BeginMoving(getDestination(getSpeed(10)));
									else
									ant_blocked();
								}
							} 
                            else 
                            { 

								if(blocked_ant==false && reception_communication()==false)
								BeginMoving(getDestination(getSpeed(10)));
								else
								ant_blocked();
								
								WriteTrace("Je bouge en ce moment");
							} 
                    } 
                } 
                else	// Je mange
                { 
                    WriteTrace("Je Mange!"); 
                    
					if(IsMoving && tracking_herbivore==false) 
                    { 
                        WriteTrace("Stop moving while eating."); 
                        StopMoving(); 
                    } 
                } 
            } 
            else 
            { 
				WriteTrace("Full: do nothing."); 
                
				if(IsMoving && tracking_herbivore==false) 
                    StopMoving(); 
            } 
        } 
        catch(Exception exc) 
        { 
            WriteTrace(exc.ToString()); 
        } 
    } 
    




  
    // Generic "are there attackers around I should run from" method
    bool ScanForAttackers()
    {
		double minDistance = double.MaxValue;
        OrganismState closestAttacker = null;
		Point p;
		bool menace=false;

        try
        {
            ArrayList foundAnimals = Scan();
			
            if(foundAnimals.Count > 0)
            {
                foreach(OrganismState organismState in foundAnimals)
                {
                    if(organismState is AnimalState)
                    {
                        IAnimalSpecies iAS = (IAnimalSpecies) organismState.Species;
                        if (iAS.IsCarnivore  && organismState.IsAlive==true)
                        {
							WriteTrace("Ya un carnivore dans les parrages!"); 
							carnivores_threats.Add(organismState);
							
							menace=true;

							if(DistanceTo(organismState) < minDistance)
							{
								closestAttacker = organismState;
								minDistance = DistanceTo(organismState);
							}
						}
                    }
                
					if(menace==true)
					{
					
					p = navigation.GetEvasionPath (State,closestAttacker,carnivores_threats);//(myState, badState, an);
					fleeing_carnivore_path.Clear();
					fleeing_carnivore_path = navigation.FindPath(State, State.Position, p, foundAnimals);
				
				
					tracking_plant=false;
					ant_fleeing_carnivore();
					}
				
				
				}
            }
        }
        catch(Exception exc)
        {
            WriteTrace(exc.ToString());
        }
        return menace;
    }









	
    // Looks for target plants, and starts moving towards the first one it finds 
    bool ScanForTargetPlant() 
    { 
		ArrayList plants = new ArrayList();
		
		if(fleeing_carnivore==true)
		return false;

		if(tracking_plant==true)
		return true;

		try 
        { 
            ArrayList foundCreatures = Scan(); 
            if(foundCreatures.Count > 0) 
            { 
               foreach(OrganismState organismState in foundCreatures) 
               { 
                    if(organismState is PlantState) 
					plants.Add(organismState);
			   } 
            
			   if(plants.Count > 0)  // si il y des plantes environnantes
			   { 
				 FindClosestPlant(plants);
				 OrganismState org = (OrganismState)targetPlant;
				 tracking_plant_path.Clear();
				 tracking_plant_path = navigation.FindPath(State, State.Position, org.Position, foundCreatures);
				 ant_track_plant();
				 return true; 
			   }
				
			}
        }
        catch(Exception exc) 
        { 
            WriteTrace(exc.ToString()); 
        } 
        return false; 
    } 
   




	


	private void MoveToKill()
	{
		double minDistance = double.MaxValue;
		killing_prey=false;
		
		WriteTrace("Je scan pour une proie"); 
		if(fleeing_carnivore==true)
		return;


		try 
        { 
            ArrayList foundCreatures = Scan(); 
            if(foundCreatures.Count > 0) 
            { 
               foreach(OrganismState organismState in foundCreatures) 
               { 
                  if(organismState is AnimalState)
                  {
				    IAnimalSpecies iAS = (IAnimalSpecies) organismState.Species;
					if (iAS.IsCarnivore == false  && organismState.IsAlive==true && IsMySpecies(organismState)==false)
					if (organismState.PercentEnergy <= 0.65)
					{  
						if(DistanceTo(organismState) < minDistance)
						{
						targetPrey=(AnimalState)organismState;
						minDistance = DistanceTo(organismState);
						killing_prey=true;
						}
					}
				  }   
			   } 
            
			   if(killing_prey==true)
			   ant_track_prey();
						
			
			}
        }
        catch(Exception exc) 
        { 
            WriteTrace(exc.ToString()); 
        } 
      
	}



	

	
    bool ScanForHungryAnts()
    {
		try
        {
           ArrayList foundAnimals = Scan();
			
            if(foundAnimals.Count > 0)
            {
               foreach(OrganismState organismState in foundAnimals)
               {
                  if(organismState is AnimalState)
                  {    
					if (organismState.IsAlive==true && IsMySpecies(organismState)==true)
                    if (State.PercentEnergy >= 0.62 && organismState.PercentEnergy <= 0.40)
						{
							return true;
                        }
                    }
                }
            }
        }
        catch(Exception exc)
        {
            WriteTrace(exc.ToString());
        }
		return false;
     }



    // sets speed based upon current energy level
    int getSpeed(int speed)
    {			
		switch(State.EnergyState) 
		{
           case EnergyState.Full:
                speed = Convert.ToInt32(speed * .95); // 70% speed
                break;
           case EnergyState.Normal:
                speed = Convert.ToInt32(speed * .8); //  50% speed
                break;
           case EnergyState.Hungry:
                speed = Convert.ToInt32(speed * .5); //  40% speed
                break;
           default:
                speed = Convert.ToInt32(speed * .25); // 25% speed
                break;
		}
		return speed;
    }



    // Aller à une destination au "hasard"
    MovementVector getDestination(int req_speed)
    {
        int direction = State.ActualDirection; 
		Point position  = State.Position;
		
		int pointX=0;
		int pointY=0;
		int angle_rand=0;
	
		angle_rand=direction;
	   
		
		if(angle_rand > 337 || angle_rand <= 23)
		{
			pointY = State.Position.Y;
			pointX = State.Position.X-40;
		}
		else if(angle_rand > 23 && angle_rand <= 69)
		{
			pointX = State.Position.X-40;
			pointY = State.Position.Y-40;
		}
		else if(angle_rand > 69 && angle_rand <= 115)
		{
			pointX = State.Position.X;
			pointY = State.Position.Y-40;
		}
		else if(angle_rand > 115 && angle_rand <= 161)
		{
			pointX = State.Position.X + 40;
			pointY = State.Position.Y - 40;
		}
		else if(angle_rand > 161 && angle_rand <= 207)
		{
			pointY = State.Position.Y;
			pointX = State.Position.X + 40;
		}
		else if(angle_rand > 207 && angle_rand <= 253)
		{
			pointX = State.Position.X + 40; 
			pointY = State.Position.Y + 40;
		}
		else if(angle_rand > 253 && angle_rand <= 299)
		{
			pointX = State.Position.X;
			pointY = State.Position.Y + 40;
		}
		else if(angle_rand > 299 && angle_rand <= 337)
		{
			pointX = State.Position.X - 40;
			pointY = State.Position.Y + 40;
		}


		if(State.Position.X > (WorldWidth - 100) && angle_rand >= 90 && angle_rand < 180 )
		{
		pointX = State.Position.X - 100;
		pointY = State.Position.Y - 100;
		}
		else if(State.Position.X > (WorldWidth - 100) && angle_rand >= 180 && angle_rand < 270)
		{
		pointX = State.Position.X - 100;
		pointY = State.Position.Y + 100;
		}
		else if(State.Position.X < (100) && angle_rand >= 0 && angle_rand <= 90)
		{
		pointX = State.Position.X + 100;
		pointY = State.Position.Y - 100;
		}
		else if(State.Position.X < (100) && angle_rand >= 270 && angle_rand <= 360)
		{
		pointX = State.Position.X + 100;
		pointY = State.Position.Y + 100;
		}
		else if(State.Position.Y > (WorldHeight - 100) && angle_rand >= 270 && angle_rand <= 360)
		{
		pointY = State.Position.Y - 100;
		pointX = State.Position.X - 100;
		}
		else if(State.Position.Y > (WorldHeight - 100) && angle_rand >= 180 && angle_rand < 270)
		{
		pointY = State.Position.Y - 100;
		pointX = State.Position.X + 100;
		}
		else if(State.Position.Y < (100) && angle_rand >= 0 && angle_rand < 90)
		{
		pointY = State.Position.Y + 100;
		pointX = State.Position.X - 100;
		}
		else if(State.Position.Y < (100) && angle_rand >= 90 && angle_rand <= 180)
		{
		pointY = State.Position.Y + 100;
		pointX = State.Position.X + 100;
		}

		Point destination_p = new Point(pointX,pointY);
		MovementVector destination = new MovementVector(destination_p, req_speed);
		
      
		return destination;
    }






	// Used to find closest plant from an array of plants
    void FindClosestPlant(ArrayList plants) 
    {
        // we are assuming that plants is already set to an ArrayList of plants
        double minDistance = double.MaxValue;
        
		foreach(PlantState plant in plants) 
        {
             if (DistanceTo(plant) < minDistance && (plant.PercentInjured < 75 || (State.EnergyState == EnergyState.Deterioration))) 
             {
                  targetPlant = plant;
                  minDistance = DistanceTo(plant);
             }
        }
	
	}



	
	Point correction_point(Point newPosition)
	{
		if(newPosition.X > WorldWidth-1)
			newPosition.X = WorldWidth-2;
		if(newPosition.X < 1)
			newPosition.X = 1;
		if(newPosition.Y > WorldHeight-1)
			newPosition.Y = WorldHeight-2;
		if(newPosition.Y < 1)
			newPosition.Y = 1;

		return newPosition;
	}




	public void reproduction()
	{

		MemoryStream m = new MemoryStream();
        BinaryWriter b = new BinaryWriter(m);
		b.Write(Position.X);
		b.Write(Position.Y);
		BeginReproduction(m.ToArray());
		b.Close();
	}




	public bool reception_communication()
	{
			
	try
     {
        ArrayList foundAnimals = Scan();
			
        if(foundAnimals.Count > 0)
        {
            foreach(OrganismState organismState in foundAnimals)
            {
			if(organismState is AnimalState)
            {
               IAnimalSpecies iAS = (IAnimalSpecies) organismState.Species;
               if (organismState.IsAlive==true && IsMySpecies(organismState)==true)
			   {
					AnimalState buddy = (AnimalState)organismState;	

				    if((buddy.Antennas.AntennaValue >= 12 && buddy.Antennas.AntennaValue <= 23) && IsEating==false)				
					{
						diriger_par_autre(buddy.Antennas.AntennaValue);
						return true;
					}
					
					if((buddy.Antennas.AntennaValue == 99))
					{
						MoveToKill();
						return true;
					}

			   }
			   
			}
			}
        }
	  }
      catch(Exception exc)
      {
            WriteTrace(exc.ToString());
      }
	
	return false;
	}


	

	public void get_dir_com()
	{
		int angle_rand = State.ActualDirection; 

		
		if(angle_rand > 337 || angle_rand <= 23)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=21;
		}
		else if(angle_rand > 23 && angle_rand <= 69)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=22;
		}
		else if(angle_rand > 69 && angle_rand <= 115)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=12;
		}
		else if(angle_rand > 115 && angle_rand <= 161)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=13;
		}
		else if(angle_rand > 161 && angle_rand <= 207)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=15;
		}
		else if(angle_rand > 207 && angle_rand <= 253)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=16;
		}
		else if(angle_rand > 253 && angle_rand <= 299)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=18;
		}
		else if(angle_rand > 299 && angle_rand <= 337)
		{
			if(fleeing_carnivore==true)
			Antennas.AntennaValue=19;
		}
	
	}




	void diriger_par_autre(int direction)
	{
		int pointX=0;
		int pointY=0;
		
	    if(direction==21)
		{
			pointY = State.Position.Y;
			pointX = State.Position.X-40;
		}
		else if(direction==22)
		{
			pointX = State.Position.X-40;
			pointY = State.Position.Y-40;
		}
		else if(direction==12)
		{
			pointX = State.Position.X;
			pointY = State.Position.Y-40;
		}
		else if(direction==13)
		{
			pointX = State.Position.X + 40;
			pointY = State.Position.Y - 40;
		}
		else if(direction==15)
		{
			pointY = State.Position.Y;
			pointX = State.Position.X + 40;
		}
		else if(direction==16)
		{
			pointX = State.Position.X + 40; 
			pointY = State.Position.Y + 40;
		}
		else if(direction==18)
		{
			pointX = State.Position.X;
			pointY = State.Position.Y + 40;
		}
		else if(direction==19)
		{
			pointX = State.Position.X - 40;
			pointY = State.Position.Y + 40;
		}

		Point destination_p = new Point(pointX,pointY);
		MovementVector destination = new MovementVector(destination_p, getSpeed(12));
		
	}







    //***species counter bits
    // Species Counter function output
    protected void TraceState() 
    {
        if ( State.IsMature ) 
        {
            WriteTrace("+/" + State.IncubationTicks + "/" + State.ReproductionWait);
        }
        else 
        {
            WriteTrace("-/" + State.Radius + "/" + State.GrowthWait + " == " + GetPercentUsedTicks(State).ToString("n"));
        }
        WriteTrace(mcKilled + "/" + mcBorn + "/" + mcTeleported + "/" + State.Generation);
        WriteTrace(State.PercentLifespanRemaining.ToString("n") + "/" + State.PercentEnergy.ToString("n") + "/" + State.PercentInjured.ToString("n"));
    }
        
    // Above uses those functions:
    public static double GetPercentUsedTicks (AnimalState animal) 
    {
        return (double)GetUsedTicks(animal) / animal.TickAge;
    }

    public static int GetUsedTicks (AnimalState animal) 
    {
        int usedTicks = animal.Species.GrowthWait * ( animal.Radius - 11 ) +
           animal.Species.GrowthWait - animal.GrowthWait;
        return usedTicks;
    }
   

    //***serialize
    public override void SerializeAnimal(MemoryStream m) 
    { 
//		byte[] sperm = dna.Furtilize();
 //       m.Write(sperm,0,sperm.Length);

    } 
  
	//***deserialize
    public override void DeserializeAnimal(MemoryStream m) 
    { 
//		byte[] sperm = m.ToArray();
 //       dna.MergeDna(sperm);

    } 
    
} 
